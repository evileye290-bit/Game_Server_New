using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class TeamDungeonMap : DungeonMap
    {
        /// <summary>
        /// 理论上应该进入组队副本的小组人数
        /// </summary>
        private int theoryMemberCount = 0;
        public int TheoryMemberCount => theoryMemberCount;

        public int TeamId { get; private set; }//当前队伍id
        public long VerifyQuitTeamTimerId { get; set; }
        public bool IsHelpDungeon { get; private set; }
        public int AskHelpUid { get; private set; }

        public TeamDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        public void InitTeamDungeonMap(int theoryMemberCount, int teamId)
        {
            this.theoryMemberCount = theoryMemberCount;
            this.TeamId = teamId;
        }

        public void OnTeamDungeonFinished()
        {
            NotifyTeamQuitDungeon();
        }

        // 对于组队副本，副本在准备阶段，如果所有队员均map loading done，则直接开始无需再等待
        public override void OnPlayerMapLoadingDone(PlayerChar player)
        {
            NotifyPlayerLoadingDone(player);
            NotifyDungeonStopTime(player);
            NotifyEnrgyCanInfo(player);
            if (IsFirstStartFight(player))
            {
                switch (State)
                {
                    case DungeonState.Open:
                        CheckAndStartFigting(player);
                        break;
                    case DungeonState.Started:
                        // 副本已经开始 则开始战斗
                        player.StartFighting();
                        StartHeros(player);
                        break;
                    default:
                        break;
                }
            }
            NotifyIsFirstStart(player);

            if (player.GetReEnterDungeon())
            {
                SyncReEnterDungeonInfo(player);
                player.SetReEnterDungeon(false);
            }
        }

        public override void OnPlayerLeave(PlayerChar player, bool cache = false)
        {
            CheckAllPlayerLeave(player);
            base.OnPlayerLeave(player, cache);
        }

        protected override void Success()
        {
            OnTeamDungeonFinished();

            PlayerChar player = null;
            foreach (var kv in PcList)
            {
                try
                {
                    player = kv.Value;
                    //副本类型任务计数
                    PlayerAddTaskNum(player);

                    //增加伙伴经验
                    player.AddHeroExp(DungeonModel.HeroExp);

                    player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
                }
                catch (Exception ex)
                {
                    Log.Alert(ex);
                }
            }
        }

        protected override void Failed()
        {
            base.Failed();
            OnTeamDungeonFinished();
            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }
            PcList.ForEach(x => x.Value.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime()));
        }

        private void CheckAndStartFigting(PlayerChar player)
        {
            if (PcList.Count < theoryMemberCount)
            {
                player.Write(new MSG_ZGC_BATTLE_START_TIME() { StartTime = Timestamp.GetUnixTimeStampSeconds(StartTime) });
                return;
            }

            // 副本还在准备期间，并未开始,如果所有player均loading done，则无需再等待，直接开始
            foreach (var item in PcList)
            {
                if (!item.Value.IsMapLoadingDone)
                {
                    return;
                }
            }
            this.Start();
        }

        private void CheckAllPlayerLeave(PlayerChar player)
        {
            //人全部退出副本失败
            if (DungeonResult != DungeonResult.None)
            {
                return;
            }

            //自己是最后一个离开副本的玩家时候结算副本
            bool needStopDungeon = PcList.Count == 1 && PcList.ContainsKey(player.InstanceId);

            //掉线，没有从PCList移除的情况
            if (!needStopDungeon)
            {
                needStopDungeon = CheckAllPlayerLeave();
            }

            if (needStopDungeon)
            {
                Stop(DungeonResult.Failed);
            }
        }

        private bool CheckAllPlayerLeave()
        {
            if (PcList.Count == 0)
            {
                return true;
            }
            foreach (var kv in PcList)
            {
                if (kv.Value.IsOnline())
                {
                    return false;
                }
            }
            return true;
        }

        internal void NotifyTeamMembersEnter(int captainUid)
        {
            MSG_ZR_NEW_TEAM_DUNGEON msg = new MSG_ZR_NEW_TEAM_DUNGEON();
            msg.OwnerUid = captainUid;
            msg.MapId = MapId;
            msg.Channel = Channel;

            msg.MainId = server.MainId;
            msg.SubId = server.SubId;

            server.SendToRelation(msg);
        }

        private void NotifyTeamQuitDungeon()
        {
            server.SendToRelation(new MSG_ZR_TEAM_QUIT_DUNGEON() { TeamId = this.TeamId });
        }

        private void NotifyPlayerLoadingDone(PlayerChar player)
        {
            //通知自己，队友队友状态
            MSG_ZGC_DUNGEON_LOADINGDONE msg = new MSG_ZGC_DUNGEON_LOADINGDONE();
            msg.Uid = player.Uid;
            PcList.ForEach(mb =>
            {
                if (mb.Value.Uid != player.Uid && mb.Value.IsOnline() && mb.Value.IsMapLoadingDone)
                {
                    msg.OnlineTeamMember.Add(mb.Value.Uid);
                }
            });
            player.Write(msg);

            //loadingdone后通知队友
            foreach (var kv in PcList)
            {
                if (kv.Key != player.InstanceId)
                {
                    kv.Value.Write(new MSG_ZGC_TEAMMEMBER_LOADINGDONE() { LoadingDoneUid = player.Uid });
                }
            }

        }

        private void NotifyRobotLoadingDone(Robot robot)
        {
            foreach (var kv in PcList)
            {
                if (kv.Key != robot.InstanceId)
                {
                    kv.Value.Write(new MSG_ZGC_TEAMMEMBER_LOADINGDONE() { LoadingDoneUid = robot.Uid });
                }
            }
        }

        public void AddAttackerTeamRobot(List<HeroInfo> infos, int Uid, Dictionary<int, int> natureValues, Dictionary<int, int> natureRatios, Dictionary<int, int> heroPos)
        {
            AddRobotAndHeros(true, infos, Uid, natureValues, natureRatios, heroPos);
            foreach (var item in RobotList)
            {
                NotifyRobotLoadingDone(item.Value);
            }
        }

        internal void AddAttackerMirror(PlayerChar brother, Dictionary<int, int> heroPos)
        {
            AddMirrorRobot(true, brother, heroPos);
        }

        /// <summary>
        /// 是否为求援状态
        /// </summary>
        internal void SetIsHelpState(bool state, int askUid)
        {
            IsHelpDungeon = state;
            AskHelpUid = askUid;
        }

        internal bool IsHelpAsker(int uid)
        {
            return IsHelpDungeon && AskHelpUid == uid;
        }
    }
}
