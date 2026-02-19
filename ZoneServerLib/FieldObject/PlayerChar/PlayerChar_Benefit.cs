using CommonUtility;
using DBUtility.Sql;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        private List<int> passedBenefigDungeon = new List<int>();

        public void InitBenefitPassedDungeon(List<int> passedDungeonList)
        {
            passedBenefigDungeon.AddRange(passedDungeonList);
        }

        public bool IsPassedBenefitDungeon(int dungeonId)
        {
            return passedBenefigDungeon.Contains(dungeonId);
        }

        public void PassedBenefitDungeon(int dungeonId)
        {
            if (!IsPassedBenefitDungeon(dungeonId))
            {
                passedBenefigDungeon.Add(dungeonId);
                SyncDBBenefitDungeonIds();
                BenefitInfo();
            }
        }

        public void BenefitReward(RewardManager manager, DungeonModel model, bool passed, int battleStage)
        {
            if (passed)
            {
                PassedBenefitDungeon(model.Id);
            }

            manager.BreakupRewards();

            // 发放奖励
            AddRewards(manager, ObtainWay.Benefit, model.Data.Name);

            //更新挑战次数
            UpdateCounter((MapType)model.Type, -1);

            //通知前端奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = GetRewardSyncMsg(manager);
            rewardMsg.DungeonId = model.Id;
            rewardMsg.Stage = battleStage;
            rewardMsg.Result = (int)DungeonResult.Success;
            Write(rewardMsg);
        }

        public void BenefitInfo()
        {
            MSG_ZGC_BENEFIT_INFO msg = new MSG_ZGC_BENEFIT_INFO();
            msg.Result = (int)ErrorCode.Success;
            msg.DungeonIds.AddRange(passedBenefigDungeon);
            Write(msg);
        }

        public void BenefitSweep(int dungeonId)
        {
            MSG_ZGC_BENEFIT_SWEEP msg = new MSG_ZGC_BENEFIT_SWEEP();
            DungeonModel model = DungeonLibrary.GetDungeon(dungeonId);
            if (model == null)
            {
                Logger.Log.Warn($"player {Uid} benefit sweep {dungeonId} failed: not find dungeon");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!IsPassedBenefitDungeon(dungeonId))
            {
                Logger.Log.Warn($"player {Uid} benefit sweep {dungeonId} failed: dungeon not passed");
                msg.Result = (int)ErrorCode.DungeonHadNotPassed;
                Write(msg);
                return;
            }

            MapType mapType = (MapType)model.Type;
            //int resCount = GetDungeonChallengeRestCount(mapType);
            MapCounterModel mapModel = CounterLibrary.GetCounterType(mapType);
            int resCount = GetCounterValue(mapModel.Counter);
            if (resCount <= 0)
            {
                Logger.Log.Warn($"player {Uid} benefit sweep {dungeonId} failed: challenge count not enough");
                msg.Result = (int)ErrorCode.ChallengeCountNotEnough;
                Write(msg);
                return;
            }

            UpdateCounter(mapType, -1);

            RewardManager manager = new RewardManager();
            //manager.AddSimpleReward(model.Data.GetString("BenefitSweepReward"));
            List<ItemBasicInfo> getList = AddRewardDrop(model.Data.GetIntList("BenefitSweepReward", "|"));
            manager.AddReward(getList);
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.SecretAreaSweep);

            manager.GenerateRewardMsg(msg.Rewards);
            msg.Result = (int)ErrorCode.Success;
            //扫荡任务完成
            MapModel tempMapModel = MapLibrary.GetMap(dungeonId);
            AddTaskNumForType(TaskType.CompleteDungeons, 1, true, tempMapModel.MapType);
            AddTaskNumForType(TaskType.CompleteOneDungeon, 1, true, model.Id);
            AddTaskNumForType(TaskType.CompleteDungeonList, 1, true, model.Id);
            AddTaskNumForType(TaskType.CompleteDungeonTypes, 1, true, tempMapModel.MapType);

            //完成通行证任务
            AddPassCardTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
            AddPassCardTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);
            AddPassCardTaskNum(TaskType.CompleteOneDungeon, model.Id, TaskParamType.DUNGEON);

            //完成学院任务
            AddSchoolTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
            AddSchoolTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);
            AddSchoolTaskNum(TaskType.CompleteOneDungeon, model.Id, TaskParamType.DUNGEON);
            AddSchoolTaskNum(TaskType.CompleteDungeonList, model.Id, TaskParamType.DUNGEON_LIST);

            //漂流探宝
            AddDriftExploreTaskNum(TaskType.CompleteDungeons, 1, false, tempMapModel.MapType);
            AddDriftExploreTaskNum(TaskType.CompleteDungeonTypes, 1, false, tempMapModel.MapType);

            Write(msg);

            BIRecordCheckPointLog(tempMapModel.MapType, dungeonId.ToString(), 1, 0);
            AddRunawayActivityNumForType(RunawayAction.Fight);
            //komoelog         
            switch (mapType)
            {       
                case MapType.SoulPower:
                    KomoeLogRecordPveFight(4, 2, model.Id.ToString(), manager.RewardList, 1);
                    break;
                case MapType.SoulBreath:
                    KomoeLogRecordPveFight(3, 2, model.Id.ToString(), manager.RewardList, 1);
                    break;
                default:
                    break;
            }           
        }

        private void SyncDBBenefitDungeonIds()
        {
            QueryUpdateBenefit query = new QueryUpdateBenefit(passedBenefigDungeon, uid);
            server.GameDBPool.Call(query);
        }

        public MSG_ZMZ_BENEFIT_INFO GenerateBenefitTransformMsg()
        {
            MSG_ZMZ_BENEFIT_INFO msg = new MSG_ZMZ_BENEFIT_INFO();
            msg.PassedDungeons.AddRange(passedBenefigDungeon);
            return msg;
        }

        public void LoadBenefitTransform(MSG_ZMZ_BENEFIT_INFO info)
        {
            if (info.PassedDungeons.Count > 0)
            {
                info.PassedDungeons.ForEach(x => this.passedBenefigDungeon.Add(x));
            }
        }
    }
}
