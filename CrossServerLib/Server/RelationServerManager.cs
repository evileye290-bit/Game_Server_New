using System;
using System.Collections.Generic;
using System.Linq;
using Logger;
using CommonUtility;
using ServerShared;
using DBUtility;
using ServerFrame;
using EnumerateUtility.Timing;
using Message.Corss.Protocol.CorssR;
using EnumerateUtility;
using Google.Protobuf.Collections;

namespace CrossServerLib
{
    public partial class RelationServerManager : FrontendServerManager
    {

        public RelationServerManager(BaseApi api, ServerType serverType):base(api, serverType)
        {
            showMng = new ShowManager();
            challengerMng = new ChallengerManager();
            crossBattleMng = new CrossBattleManager(Api);
            CrossChallengeMng = new CrossChallengeManager(Api);

            InitTimerManager(Api.Now(), 0);
        }

        private new CrossServerApi Api
        { get { return (CrossServerApi)api; } }
      
        private ShowManager showMng;
        public ShowManager ShowMng
        { get { return showMng; } }

        private ChallengerManager challengerMng;
        public ChallengerManager ChallengerMng
        { get { return challengerMng; } }

        private CrossBattleManager crossBattleMng;
        public CrossBattleManager CrossBattleMng
        { get { return crossBattleMng; } }

        public CrossChallengeManager CrossChallengeMng { get; private set; }

        public override void DestroyServer(FrontendServer server)
        {
            base.DestroyServer(server);
        }

        public override void UpdateServers(double dt)
        {
            base.UpdateServers(dt);
            //定时检测删除缓存玩家信息数据
            ShowMng.DeletePlayerShowInfo(dt);
            //crossBattleMng.OnUpdate(dt);
        }

        public void InitTimerManager(DateTime time, int addDay)
        {
            time = time.AddDays(addDay);
            //获取刷新任务
            Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic = RechargeLibrary.GetTimingLists(time);
            if (taskDic.Count > 0)
            {
                var kv = taskDic.First();
                AddTaskTimer(taskDic, kv.Key);
            }
            else
            {
                if (addDay > 0)
                {
                    DateTime nextTime = time.Date.AddDays(0.5);
                    taskDic.Add(nextTime, new List<RechargeGiftTimeType>());
                    //说明已经增加过1天
                    AddTaskTimer(taskDic, nextTime);
                }
                else
                {
                    //当天没有了，下一天
                    InitTimerManager(time.Date, 1);
                }
            }
        }

        private void AddTaskTimer(Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic, DateTime time)
        {
            double interval = (time - DateTime.Now).TotalMilliseconds;
            Log.Info($"relatrion add task timer {time} ：after {interval}");
            CrossTimerQuery counterTimer = new CrossTimerQuery(interval, taskDic);
            Api.TaskTimerMng.Call(counterTimer, (ret) =>
            {
                TimingRefresh(counterTimer.TaskDic);
            });
        }

        private void CallBackNextTask(DateTime time, Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic)
        {
            if (taskDic.Count > 0)
            {
                var firstTask = taskDic.First();
                AddTaskTimer(taskDic, firstTask.Key);
            }
            else
            {
                InitTimerManager(time.AddSeconds(1), 0);
            }
        }

        private void TimingRefresh(Dictionary<DateTime, List<RechargeGiftTimeType>> taskDic)
        {
            var firstTask = taskDic.First();
            taskDic.Remove(firstTask.Key);
            CallBackNextTask(firstTask.Key, taskDic);

            //有刷新任务
            foreach (var giftType in firstTask.Value)
            {
                Api.TrackingLoggerMng.TrackTimerLog(Api.MainId, "cross", giftType.ToString(), Api.Now());
                switch (giftType)
                {
                    case RechargeGiftTimeType.HiddenWeaponStart:
                        Api.HidderWeaponMng.Clear();
                        break;
                    case RechargeGiftTimeType.SeaTreasureStart:
                        Api.SeaTreasureMng.Clear();
                        break;
                    case RechargeGiftTimeType.GardenStart:
                        Api.GardenMng.Clear();
                        Api.GardenMng.UpdatePeriod();
                        break;
                    case RechargeGiftTimeType.GardenEndReward:
                        Api.GardenMng.SendReward();
                        break;
                    case RechargeGiftTimeType.DivineLoveStart:
                        Api.DivineLoveMng.Clear();
                        break;
                    case RechargeGiftTimeType.IslandHighStart:
                        Api.IslandHighMng.Clear();
                        Api.IslandHighMng.UpdatePeriod();
                        break;
                    case RechargeGiftTimeType.IslandHighEnd:
                        Api.IslandHighMng.SendFinalReward();
                        break;
                    case RechargeGiftTimeType.IslandHighFianlStage1:
                        Api.IslandHighMng.SendReward(1);
                        break;
                    case RechargeGiftTimeType.IslandHighFianlStage2:
                        Api.IslandHighMng.SendReward(2);
                        break;
                    case RechargeGiftTimeType.IslandHighFianlStage3:
                        Api.IslandHighMng.SendReward(3);
                        break;
                    case RechargeGiftTimeType.IslandHighFianlStage4:
                        Api.IslandHighMng.SendReward(4);
                        break;
                    case RechargeGiftTimeType.IslandHighFianlStage5:
                        Api.IslandHighMng.SendReward(5);
                        break;
                    case RechargeGiftTimeType.IslandHighFianlStage6:
                        Api.IslandHighMng.SendReward(6);
                        break;
                    case RechargeGiftTimeType.IslandHighFianlStage7:
                        Api.IslandHighMng.SendReward(7);
                        break;
                    case RechargeGiftTimeType.StoneWallStart:
                        Api.StoneWallMng.Clear();
                        break;
                    case RechargeGiftTimeType.CarnivalBossStart:
                        Api.CarnivalBossMng.Clear();
                        break;
                    case RechargeGiftTimeType.RouletteStart:
                        Api.RouletteManager.Clear();
                        Api.RouletteManager.UpdatePeriod();
                        break;
                    case RechargeGiftTimeType.RouletteEnd:
                        Api.RouletteManager.SendReward();
                        break;
                    case RechargeGiftTimeType.CanoeStart:
                        Api.CanoeMng.Clear();
                        break;
                    case RechargeGiftTimeType.CanoeEnd:
                        Api.CanoeMng.SendReward();
                        break;
                    case RechargeGiftTimeType.MidAutumnStart:
                        Api.MidAutumnMng.Clear();
                        break;
                    case RechargeGiftTimeType.MidAutumnEnd:
                        Api.MidAutumnMng.SendReward();
                        break;
                    case RechargeGiftTimeType.ThemeFireworkStart:
                        Api.ThemeFireworkMng.Clear();
                        break;
                    case RechargeGiftTimeType.ThemeFireworkEnd:
                        Api.ThemeFireworkMng.SendReward();
                        break;
                    case RechargeGiftTimeType.NineTestStart:
                        Api.NineTestMng.Clear();
                        break;
                    case RechargeGiftTimeType.NineTestEnd:
                        Api.NineTestMng.SendReward();
                        break;
                    default:
                        break;
                }
            }
        }

        public bool WriteToRelation<T>(T msg, int mainId, int uid = 0) where T : Google.Protobuf.IMessage
        {
            FrontendServer relation = GetSinglePointServer(mainId);
            if (relation != null)
            {
                relation.Write(msg, uid);
                return true;
            }
            else
            {
                //没有找到玩家，直接算输
                return false;
            }
        }

        public void BroadcastToGroupRelationByMainId<T>(T msg, int mainId) where T : Google.Protobuf.IMessage
        {
            int groupId = CrossBattleLibrary.GetGroupId(mainId);
            BroadcastToGroupRelation(msg, groupId);
        }

        public void BroadcastToGroupRelation<T>(T msg, int groupId) where T : Google.Protobuf.IMessage
        {
            List<int> serverIds = CrossBattleLibrary.GetGroupServers(groupId);
            foreach (var item in serverIds)
            {
                WriteToRelation(msg, item);
            }
        }

        public void BroadcastAnnouncement(ANNOUNCEMENT_TYPE type, int mainId, params object[] list)
        {
            MSG_CorssR_BROADCAST_ANNOUNCEMENT msg = new MSG_CorssR_BROADCAST_ANNOUNCEMENT();
            msg.Type = (int)type;
            foreach (var item in list)
            {
                msg.List.Add(item.ToString());
            }
            BroadcastToGroupRelationByMainId(msg, mainId);
        }

        public void BroadcastAnnouncement(int type, int mainId, RepeatedField<string> list)
        {
            MSG_CorssR_BROADCAST_ANNOUNCEMENT msg = new MSG_CorssR_BROADCAST_ANNOUNCEMENT();
            msg.Type = type;
            foreach (var item in list)
            {
                msg.List.Add(item.ToString());
            }
            BroadcastToGroupRelationByMainId(msg, mainId);
        }

        public void SendRankInfoToRelation(string rankType, List<string> rankInfoList, int randMainId, int stage = 0)
        {
            MSG_CorssR_RECORD_RANK_INFO msg = new MSG_CorssR_RECORD_RANK_INFO();
            msg.RankType = rankType;
            msg.RankInfo.AddRange(rankInfoList);
            msg.Stage = stage;

            WriteToRelation(msg, randMainId);
        }
    }
}