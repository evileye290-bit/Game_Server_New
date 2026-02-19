using System.Collections.Generic;
using CommonUtility;
using DataProperty;
using Message.Manager.Protocol.MZ;
using System;
using System.Linq;
using Logger;
using EpPathFinding;
using System.IO;
using Message.Relation.Protocol.RZ;
using DBUtility;
using EnumerateUtility;
using ServerModels;
using ServerShared;
using Message.Zone.Protocol.ZM;
using ServerShared.Map;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public partial class FieldMap : BaseMap
    {

        private Dictionary<int, PlayerChar> pcList = new Dictionary<int, PlayerChar>();

        /// <summary>
        /// 玩家
        /// </summary>
        public IReadOnlyDictionary<int, PlayerChar> PcList
        {
            get { return pcList; }
        }

        private List<int> playerRemoveList = new List<int>();

        private void UpdatePc(float dt)
        {
            foreach (var pc in pcList)
            {
                try
                {
                    if (pc.Value.IsOnline())
                    {
                        pc.Value.SendCatchedBroadcastPacket();
                    }
                    else
                    {
                        pc.Value.BroadcastList.List.Clear();
                    }
                    pc.Value.Update(dt);
                    pc.Value.SendDelayMessage();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }
        private void RemovePc()
        {
            if (playerRemoveList.Count > 0)
            {
                foreach (var instanceId in playerRemoveList)
                {
                    try
                    {
                        RemoveObjectSimpleInfo(instanceId);
                        pcList.Remove(instanceId);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                playerRemoveList.Clear();
            }
        }

        #region 添加
        private void AddPlayer(PlayerChar player, bool reEnter = false)
        {
            Log.Write("player {0} enter map {1} channel {2} reEnter {3}", player.Uid, MapId, Channel, reEnter);

            if (!reEnter)
            {
                player.SetInstanceId(TokenId);

                pcList.Add(player.InstanceId, player);

                AddObjectSimpleInfo(player.InstanceId, TYPE.PC);
            }

            if(this is DungeonMap)
            {
                if (heroList.Count >= player.HeroMng.CallHeroCount())
                {
                    (this as DungeonMap).OnePlayerDone = true;//此时至少有一个玩家连同其hero加载完了
                }
                if (petList.Count >= player.PetManager.GetCallPetCount(MapId))
                {
                    (this as DungeonMap).OnePlayerPetDone = true;//此时至少有一个玩家连同其pet加载完了
                }
                else if (!player.CheckLimitOpen(LimitType.PetBattle))
                {
                    (this as DungeonMap).OnePlayerPetDone = true;//此时至少有一个玩家连同其pet加载完了
                }
            }

            // 通知已进入游戏地图
            MSG_ZM_CLIENT_ENTER_MAP notify = new MSG_ZM_CLIENT_ENTER_MAP();
            notify.CharacterUid = player.Uid;
            notify.MapId = MapId;
            notify.Channel = Channel;
            server.ManagerServer.Write(notify);

            // 清除待进入世界的playerEnter列表
            server.PCManager.RemovePlayerEnter(player.Uid);
        }

        #endregion

        #region 删除

        private void RemovePlayer(PlayerChar player, bool cache = true)
        {
            if (player == null) return;
            if (!cache)
            {
                player.HeroMng.TakeBackHeroFromMap();
                player.PetManager.TakeBackPetFromMap();
                // 通知已离开地图
                Log.Write("player {0} leave map {1} channel {2}", player.Uid, MapId, Channel);
                MSG_ZM_CLIENT_LEAVE_MAP notify = new MSG_ZM_CLIENT_LEAVE_MAP();
                notify.CharacterUid = player.Uid;
                notify.MapId = MapId;
                notify.Channel = Channel;
                server.ManagerServer.Write(notify);
                playerRemoveList.Add(player.InstanceId);
            }
        }

        #endregion

        public virtual void OnPlayerEnter(PlayerChar player, bool reEnter = false)
        {
            if (player == null) return;
            
            if (reEnter)
            {
                AddPlayer(player, true);
                Log.Debug($"player {player.Uid} OnPlayerEnter reEnterDungeon {reEnter}");
                player.SetReEnterDungeon(true);
                NotifyPlayerMapInfo(player);
                SyncAOI2Client(player);
                return;
            }
            player.EnterMap(this);
            AddPlayer(player);

            NotifyPlayerMapInfo(player);
            SyncAOI2Client(player);
        }

        private void SyncAOI2Client(PlayerChar player)
        {
            //player.SetIsMapLoadingDone(true);

            //假如从缓存重进的 也要获取自己的aoi，但是不能通知别人
            if (player.GetReEnterDungeon())
            {
                Log.Debug($"player {player.Uid} maploading done reEnterDungeon");
                player.GetDungeonAoi();
            }
            else
            {
                player.AddToAoi();
                player.HeroMng.CallHero2Map();
                player.PetManager.CallPetsToMap();
            }

            //player.CurrentMap.OnPlayerMapLoadingDone(player);
        }

        private void NotifyPlayerMapInfo(PlayerChar player)
        {
            // 通知player地图相关信息
            MSG_GC_ENTER_ZONE msg = new MSG_GC_ENTER_ZONE();
            msg.Result = (int)ErrorCode.Success;
            msg.MapId = player.EnterMapInfo.MapId;
            msg.Channel = player.EnterMapInfo.Channel;
            msg.PosX = player.EnterMapInfo.Position.x;
            msg.PosY = player.EnterMapInfo.Position.y;
            msg.Angle = player.NextAngle;

            msg.IsVisable = player.IsVisable;
            msg.NeedAnim = player.EnterMapInfo.NeedAnim;
            player.EnterMapInfo.ClearAnimInfo();

            MapChannelInfo channelInfo = server.ManagerServer.GetMapChannelInfo(player.EnterMapInfo.MapId);
            if (channelInfo != null)
            {
                msg.MinChannel = channelInfo.MinChannel;
                msg.MaxChannel = channelInfo.MaxChannel;
            }

            msg.InstanceId = player.InstanceId;

            foreach (var item in npcList)
            {
                if (item.Value.NeedSync() && item.Value.IsVisable)
                {
                    MSG_ZGC_NPC_INFO info = item.Value.GetNpcPacketInfo();
                    msg.NpcList.Add(info);
                }
            }

            foreach (var item in GoodsList)
            {
                MSG_ZGC_GOODS_INFO info = item.Value.GetGoodsPacketInfo();
                msg.GoodsList.Add(info);
            }
            player.Write(msg);
        }

        public virtual void OnPlayerLeave(PlayerChar player, bool cache = false)
        {
            RemovePlayer(player, cache);
            player.LeaveMap(cache);
        }

        public virtual bool CanEnter()
        {
            if (PcList.Count >= Model.MaxNum)
            {
                return false;
            }
            return true;
        }

        public virtual bool CanEnter(int instanceId)
        {
            if (CanEnter() || PcList.ContainsKey(instanceId))
            {
                return true;
            }
            return false;
        }

        public virtual void OnPlayerMapLoadingDone(PlayerChar player)
        {

        }
    }
}