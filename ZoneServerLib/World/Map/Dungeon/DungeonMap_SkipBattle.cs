using CommonUtility;
using EnumerateUtility;
using Google.Protobuf;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class CachedMessageWithId
    {
        public uint Id { get; private set; }
        public IMessage Message { get; private set; }

        public CachedMessageWithId(IMessage message, uint id)
        {
            Id = id;
            Message = message;
        }
    }

    public class CachedMessageInfo
    {
        public ArraySegment<byte> First { get; private set; }
        public ArraySegment<byte> Second { get; private set; }

        public CachedMessageInfo(ArraySegment<byte> first, ArraySegment<byte> second)
        {
            First = first;
            Second = second;
        }
    }

    partial class DungeonMap
    {
        private bool setedKickPlayerTime = false;
        private MSG_ZGC_DUNGEON_REWARD battleResultMessage = null;

        /// <summary>
        /// 加速后的帧索引
        /// </summary>
        private int speedUpFsmIndex = 0;
        private float speedUpTime = 3f;

        protected float speedUpFinishDelayTime = 0f;

        //<fpsindex,<uid, List<message>>>
        DoubleDeapthListMap<int, int, CachedMessageWithId> cachedPlayerMessage = new DoubleDeapthListMap<int, int, CachedMessageWithId>();
        DoubleDeapthListMap<int, int, CachedMessageInfo> cachedPlayerMessageOther = new DoubleDeapthListMap<int, int, CachedMessageInfo>();

        public int SpeedUpFsmIndex => speedUpFsmIndex;
        public bool IsSpeedUpIng { get; private set; }//是否加速中
        public bool IsSpeedUpDungeon { get; protected set; }//是否为加速副本
        public bool HadSpeedUp { get; protected set; }//是否加速过
        public float SpeedUpFinishDelayTime => speedUpFinishDelayTime;

        /// <summary>
        /// 是否真正启动了加速，如果加速倍率为1则不算加速
        /// </summary>
        public bool IsRealSpeedUp => IsSpeedUpDungeon && DungeonLibrary.SkipBattleSpeedUp > 1;
        public bool CanSkipBattle() => Model.CanSkipBattle();

        public bool IsNeedCacheMessage()
        {
            return IsSpeedUpIng && IsRealSpeedUp && State >= DungeonState.Started;
        }

        private  void CheckAndSpeedUp(float dt)
        {
            if (IsSpeedUpIng) return;

            if (IsSpeedUpDungeon && State == DungeonState.Started)
            {
                speedUpTime -= dt;

                if (speedUpTime <= 0)
                {
                    SetSpeedUp(true);
                }
            }
        }

        public void CachePlayerMessage<T>(T msg, PlayerChar player) where T : Google.Protobuf.IMessage
        {
            if (msg is MSG_ZGC_DUNGEON_REWARD)
            {
                //战斗结果单独存
                battleResultMessage = msg as MSG_ZGC_DUNGEON_REWARD;
            }
            else
            {
                cachedPlayerMessage.Add(speedUpFsmIndex, player.Uid, new CachedMessageWithId(msg, Id<T>.Value));
            }
        }

        public void CachePlayerMessage(PlayerChar player, ArraySegment<byte> first, ArraySegment<byte> second)
        {
            cachedPlayerMessageOther.Add(speedUpFsmIndex, player.Uid, new CachedMessageInfo(first, second));
        }

        private List<CachedMessageWithId> GetPlayerMessages(int normalFsmIndex, int uid)
        {
            List<CachedMessageWithId> messages;
            cachedPlayerMessage.TryGetValue(normalFsmIndex, uid, out messages);
            return messages;
        }

        private List<CachedMessageInfo> GetPlayerOtherMessages(int normalFsmIndex, int uid)
        {
            List<CachedMessageInfo> messages;
            cachedPlayerMessageOther.TryGetValue(normalFsmIndex, uid, out messages);
            return messages;
        }

        private void BroadcastPlayerCachedMessage(int fpsNum)
        {
            PcList.ForEach(x => BroadcastPlayerCachedMessage(x.Value, fpsNum));
        }

        private void BroadcastPlayerCachedMessage(PlayerChar player, int fpsNum)
        {
            if (player == null) return;

            //正常服务器更新帧率值不可能大于加速的
            if (fpsNum >= speedUpFsmIndex)
            {
                if (speedUpFsmIndex > 0 && !setedKickPlayerTime)
                {
                    if (battleResultMessage != null)
                    {
                        //追上了加速后的帧率
                        SetKickPlayerDelayTime(35f);
                        
                        setedKickPlayerTime = true;

                        player.Gate.Write(battleResultMessage, player.Uid, Id<MSG_ZGC_DUNGEON_REWARD>.Value);
                        Logger.Log.Debug("speed reward --------------------- ");
                    }
                }
            }

            //只发送正常速度当前帧的数据
            List<CachedMessageWithId> messages = GetPlayerMessages(fpsNum, player.Uid);
            if (messages != null && messages.Count > 0)
            {
                foreach (var msg in messages)
                {
                    if (msg == null || msg.Message == null) continue;
                    player.Gate.Write(msg.Message, player.Uid, msg.Id);
                }

//#if DEBUG
                cachedPlayerMessage.Remove(fpsNum, player.Uid);
//#endif
            }

            List<CachedMessageInfo> messagesOther = GetPlayerOtherMessages(fpsNum, player.Uid);
            if (messagesOther?.Count > 0)
            {
                messagesOther.ForEach(msg => player.Gate.Write(msg.First, msg.Second));

//#if DEBUG
                cachedPlayerMessageOther.Remove(fpsNum, player.Uid);
//#endif
            }
        }

        public bool CheckSpeedUp()
        {
            switch (Model.MapType)
            {
                case MapType.Arena:
                case MapType.CrossBattle:
                case MapType.CrossFinals:
                case MapType.CampDefense:
                case MapType.CrossBoss:
                case MapType.CrossBossSite:
                case MapType.PushFigure:
                case MapType.IslandChallenge:
                case MapType.CrossChallenge:
                case MapType.CrossChallengeFinals:
                case MapType.SpaceTimeTower:
                    return true;
                case MapType.SecretArea:
                    SecretAreaModel secretAreaModel = SecretAreaLibrary.GetModelByDungeonId(DungeonModel.Id);
                    return secretAreaModel?.Id >= SecretAreaLibrary.SpeedUpLimitId;
                default:
                    return false;
            }
        }

        public void SetSpeedUp(bool speedUp)
        {
            IsSpeedUpIng = speedUp;
            IsSpeedUpDungeon = speedUp;

            if (speedUp)
            {
                HadSpeedUp = true;
            }

            //不能加速完成后30s就踢除，前端可能还在演算
            SetKickPlayerDelayTime(99999f);
        }

        public void SkipBattle(PlayerChar player)
        {
            if (!HadSpeedUp) return;

            OnSkipBattle(player);
        }

        protected virtual void OnSkipBattle(PlayerChar player)
        {
            //跳过战斗直接发送给前端战斗结果

            if (battleResultMessage == null)
            {
                player.Write(new MSG_ZGC_DUNGEON_SKIP_BATTLE() { Result = (int)ErrorCode.CanNotSkipBattle });
                return;
            }

            cachedPlayerMessage.Clear();
            cachedPlayerMessageOther.Clear();

            player.Gate.Write(battleResultMessage, player.Uid, Id<MSG_ZGC_DUNGEON_REWARD>.Value);
            battleResultMessage = null;
        }

        protected void NotifySpeedUpEnd(PlayerChar player)
        {
            if (HadSpeedUp)
            { 
                player.Gate.Write(new MSG_ZGC_DUNGEON_SPEEDUP_END(), player.Uid, Id<MSG_ZGC_DUNGEON_SPEEDUP_END>.Value);
            }
        }
    }
}
