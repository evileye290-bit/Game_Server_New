using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public partial class BattleFpsManager
    {
        private ZoneServerApi server;
        private VideoManager videoManager;

        private bool isCachedMsg;
        private int elapsedTime;
        private MSG_ZGC_BATTLE_FPS currFpsMsg;

        protected DungeonMap curMap;

        public bool IsBattleStart { get; set; }
        public bool IsBattleEnd { get; set; }


        public BattleFpsManager(DungeonMap curMap)
        {
            this.curMap = curMap;
            this.server = curMap.server;

            this.currFpsMsg = BuildFpsInfo();

            videoManager = new VideoManager(curMap.Model.MapType);
        }

        public void SetBattleInfo(PlayerChar attacker, PlayerRankBaseInfo defencer)
        {
            if (!videoManager.Valid) return;

            videoManager.SetBattleUid(server.Now(), attacker.Uid, defencer.Uid);

            MSG_ZGC_VIDEO_BTTLE_INFO msg = GenerateVideoBattleInfo(attacker, defencer);
            Write(msg);
        }

        public void StartRecordVedio()
        {
            if (!videoManager.Valid) return;

            IsBattleStart = true;
            videoManager.Start();
        }

        public void Update(float dt)
        {
            if (IsBattleEnd) return;

            if (IsBattleStart)
            {
                elapsedTime += (int)(dt * 1000);
            }

            CheckChanged();
        }

        public void CheckChanged()
        {
            if (IsFpsChanged())
            {
                if (isCachedMsg)
                {
                    Write(currFpsMsg);
                    currFpsMsg = BuildFpsInfo();
                }

                UpdateFpsInfo();
            }
        }

        private bool IsFpsChanged()
        {
            return currFpsMsg.Fps != curMap.FpsNum;
        }

        private void UpdateFpsInfo()
        {
            currFpsMsg.Fps = curMap.FpsNum;
            currFpsMsg.ElapsedTime = elapsedTime;
        }

        public void WriteBroadcastMsg(MSG_GC_BROADCAST_INFO msg)
        {
            if (!videoManager.Valid) return;

            isCachedMsg = true;

            if (currFpsMsg.BroadcastList.List.Count >= 50)
            {
                MSG_ZGC_BATTLE_FPS tempMsg = currFpsMsg;
                Write(tempMsg);
                currFpsMsg = BuildFpsInfo();
            }

            currFpsMsg.BroadcastList.List.Add(msg);
        }

        public void WriteBroadcastMsg<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (!videoManager.Valid) return;

            isCachedMsg = true;
            BuildBroadcastMsg(msg);
        }

        private void BuildBroadcastMsg<T>(T msg) where T : Google.Protobuf.IMessage
        {
            string msgName = msg.GetType().FullName;

            switch (msgName)
            {
                case "Message.Gate.Protocol.GateC.MSG_ZGC_SKILL_ENERGY_CHANGE":
                    currFpsMsg.SkillEnergyChange.Add(msg as MSG_ZGC_SKILL_ENERGY_CHANGE);
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_SKILL_ALARM":
                    currFpsMsg.SkillAlarm.Add(msg as MSG_ZGC_SKILL_ALARM);
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_BUFF_SPEC_END":
                    currFpsMsg.BuffSpecEnd.Add(msg as MSG_ZGC_BUFF_SPEC_END);
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_DAMAGE":
                    currFpsMsg.Damage.Add(msg as MSG_ZGC_DAMAGE);
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_MARK":
                    currFpsMsg.Mark.Add(msg as MSG_ZGC_MARK);
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_HATE_INFO":
                    currFpsMsg.HateInfo.Add(msg as MSG_ZGC_HATE_INFO);
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_MIX_SKILL":
                    currFpsMsg.MixSkill.Add(msg as MSG_ZGC_MIX_SKILL);
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_MIX_SKILL_EFFECT":
                    currFpsMsg.MixSkillEffect.Add(msg as MSG_ZGC_MIX_SKILL_EFFECT);
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_DUNGEON_START":
                    currFpsMsg.DungeonStart = msg as MSG_ZGC_DUNGEON_START;
                    break;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_MONSTER_GENERATED_WALK":
                    currFpsMsg.MonsterGenerateWalk.Add(msg as MSG_ZGC_MONSTER_GENERATED_WALK);
                    break;
                default:
                    Log.Warn($"video msg {msgName} not record");
                    break;
            }
        }

        private MSG_ZGC_BATTLE_FPS BuildFpsInfo()
        {
            isCachedMsg = false;

            MSG_ZGC_BATTLE_FPS msg = new MSG_ZGC_BATTLE_FPS()
            {
                Fps = curMap.FpsNum,
                ElapsedTime = elapsedTime
            };

            msg.BroadcastList = new MSG_GC_BROADCAST_LIST();
            return msg;
        }

        private void Write<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (IsBattleEnd) return;

            videoManager.Write(msg);
        }

        public string Close(DungeonResult result, int winUid, int attacker)
        {
            IsBattleStart = false;
            IsBattleEnd = true;

            videoManager.Write(currFpsMsg);

            //战斗数据统计
            MSG_ZGC_DUNGEON_BATTLE_DATA battleData = curMap.BattleDataManager.GenerateBattleDataMsg(attacker);
            videoManager.Write(battleData);
            //curMap.BattleDataManager.Clear();

            MSG_ZGC_BATTLE_RESULT_FPS msg = new MSG_ZGC_BATTLE_RESULT_FPS() { Result = (int)result, WinUid = winUid };
            videoManager.Write(msg);

            videoManager.Close();

            return videoManager.FilePath;
        }
    }
}

