using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public SecretAreaManager SecretAreaManager { get; private set; }
        public void InitSecretAreaManager()
        {
            SecretAreaManager = new SecretAreaManager(this);
        }

        public void SecretAreaFail()
        {
            //扣除挑战次数
            //UpdateCounter(CounterType.SecretAreaCount, 1);
        }

        public void SecretAreaReward(RewardManager manager, DungeonModel model, SecretAreaModel secretAreaModel, int finishTime)
        {
            bool isFirstPass = secretAreaModel.Id > SecretAreaManager.Id;

            //SecretAreaState state = finishTime > secretAreaModel.TimeOut ? SecretAreaState.TimeOut : SecretAreaState.Passed;
            SecretAreaState state = SecretAreaState.Passed;
            SecretAreaManager.UpdateSecretAreaInfo(secretAreaModel.Id, state, finishTime);

            DungeonResult result = DungeonResult.Success;
            //if (finishTime >= secretAreaModel.TimeOut)
            //{
            //    //扣除挑战次数
            //    //UpdateCounter(CounterType.SecretAreaCount, 1);
            //    result = DungeonResult.TimeOut;
            //}
            List<int> rwardDropIds;
            if (isFirstPass)
            {
                GodPathManager.OnFinishedSecretArea();
                //manager.AddSimpleRewardWithSoulBoneCheck(model.Data.GetString("FirstReward"));
                rwardDropIds = model.Data.GetIntList("FirstRewardId", "|");
            }
            else
            {
                //manager.AddSimpleRewardWithSoulBoneCheck(model.Data.GetString("GeneralReward"));
                rwardDropIds = model.Data.GetIntList("GeneralRewardId", "|");
            }
            List<ItemBasicInfo> getList = AddRewardDrop(rwardDropIds);
            manager.AddReward(getList);
            manager.BreakupRewards();

            AddRewards(manager, ObtainWay.SecretAreaDungeon, model.Data.Name);

            //玩家还在副本中，通知前端奖励, 避免出现玩家在主城弹出结算面板
            if (CurrentMap.IsDungeon)
            {
                //先添加魂环奖励  再添加其他奖励
                MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
                manager.GenerateRewardMsg(rewardMsg.Rewards);
                rewardMsg.PassTime = finishTime;
                rewardMsg.DungeonId = model.Id;
                rewardMsg.Result = (int)result;

                CheckCacheRewardMsg(rewardMsg);
            }

            //komoelog
            KomoeLogRecordPveFight(0, 1, model.Id.ToString(), manager.RewardList, 1, finishTime, null, isFirstPass ? 1 : 0);           
        }

        public void GetSecretAreaInfo()
        {
            MSG_ZGC_SECRET_AREA_INFO msg = new MSG_ZGC_SECRET_AREA_INFO();
            if (!CheckLimitOpen(LimitType.SecretArea))
            {
                Logger.Log.Warn($"player {Uid} get secret area info failed: not open");
                msg.Result = (int)ErrorCode.LevelLimit;
                Write(msg);
                return;
            }

            msg.Id = SecretAreaManager.Id;
            msg.State = (int)SecretAreaManager.State;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void SecretAreaSweep(int id)
        {
            MSG_ZGC_SECRET_AREA_SWEEP msg = new MSG_ZGC_SECRET_AREA_SWEEP();
            SecretAreaModel secModel = SecretAreaLibrary.Get(id);
            if (secModel == null)
            {
                Logger.Log.Warn($"player {Uid} secret area sweep {id} failed : not find in xml");
                msg.Result = (int)ErrorCode.NotFindModel;
                Write(msg);
                return;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(secModel.DungeonId);
            if (model == null)
            {
                Logger.Log.Warn($"player {Uid} secret area sweep {id} failed : not find dungeon");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (CheckCoinUpperLimit(CurrenciesType.secretAreaCoin))
            {
                Logger.Log.Warn($"player {Uid} secret area sweep {id} failed : secret area coin max");
                msg.Result = (int)ErrorCode.SecretAreaCoinUpperLimit;
                Write(msg);
                return;
            }

            int restSweepCount = GetCounterRestCount(CounterType.SecretAreaSweepCount, CounterType.SecretAreaSweepCountBuy);
            if (restSweepCount <= 0)
            {
                Logger.Log.Warn($"player {Uid} secret area sweep {id} failed : sweep count not enough");
                msg.Result = (int)ErrorCode.SweepCountNotEnough;
                Write(msg);
                return;
            }

            if (!SecretAreaManager.CheckSweep(id))
            {
                Logger.Log.Warn($"player {Uid} secret area sweep {id} failed : can not sweep");
                msg.Result = (int)ErrorCode.CanNotSweepSecretArea;
                Write(msg);
                return;
            }

            UpdateCounter(CounterType.SecretAreaSweepCount, 1);

            RewardManager manager = new RewardManager();
            //manager.AddSimpleRewardWithSoulBoneCheck(model.Data.GetString("GeneralReward"));
            List<ItemBasicInfo> getList = AddRewardDrop(model.Data.GetIntList("GeneralRewardId", "|"));
            manager.AddReward(getList);
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.SecretAreaSweep);

            //扫荡任务完成
            MapModel tempMapModel = MapLibrary.GetMap(secModel.DungeonId);
            if (tempMapModel != null)
            {
                AddTaskNumForType(TaskType.CompleteDungeons, 1, true, tempMapModel.MapType);
                AddTaskNumForType(TaskType.CompleteDungeonTypes, 1, true, tempMapModel.MapType);

                AddPassCardTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
                AddPassCardTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);

                AddSchoolTaskNum(TaskType.CompleteDungeons, (int)tempMapModel.MapType, TaskParamType.TYPE);
                AddSchoolTaskNum(TaskType.CompleteDungeonTypes, (int)tempMapModel.MapType, TaskParamType.DUNGEON_TYPES);
                
                AddDriftExploreTaskNum(TaskType.CompleteDungeons, 1, false, tempMapModel.MapType);
                AddDriftExploreTaskNum(TaskType.CompleteDungeonTypes, 1, false, tempMapModel.MapType);
            }
            else
            {
                Logger.Log.Warn("player {0} SecretAreaSweep id {1} not find DungeonId {2}", Uid, id, secModel.DungeonId);
            }
            AddTaskNumForType(TaskType.CompleteOneDungeon, 1, true, model.Id);
            AddTaskNumForType(TaskType.CompleteDungeonList, 1, true, model.Id);

            //完成通行证任务
            AddPassCardTaskNum(TaskType.CompleteOneDungeon, model.Id, TaskParamType.DUNGEON);

            //完成学院任务
            AddSchoolTaskNum(TaskType.CompleteOneDungeon, model.Id, TaskParamType.DUNGEON);
            AddSchoolTaskNum(TaskType.CompleteDungeonList, model.Id, TaskParamType.DUNGEON_LIST);

            BIRecordCheckPointLog(MapType.SecretAreaSweep, model.Id.ToString(), 1, 0);

            //komoelog
            KomoeLogRecordPveFight(0, 2, model.Id.ToString(), manager.RewardList, 1);

            manager.GenerateRewardMsg(msg.Rewards);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
            AddRunawayActivityNumForType(RunawayAction.Fight);
        }

        //改变连续战斗状态
        public void ChangeSecretAreaContinueFightState(bool continueFight)
        {
            MSG_ZGC_SECRET_AREA_CONT_FIGHT response = new MSG_ZGC_SECRET_AREA_CONT_FIGHT();

            if (!CheckLimitOpen(LimitType.SecretArea))
            {
                Logger.Log.Warn($"player {Uid} change secret area continue fight state failed: not open");
                response.Result = (int)ErrorCode.LevelLimit;
                Write(response);
                return;
            }

            if (SecretAreaManager.ContinueFight == continueFight)
            {
                Logger.Log.Warn($"player {Uid} change secret area continue fight state failed: already in state {continueFight}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            SecretAreaManager.ChangeContinueFightState(continueFight);

            response.Result = (int)ErrorCode.Success;
            response.ContinueFight = continueFight;
            Write(response);
        }

        #region Releation

        public void SendSecretAreaRankInfo(MSG_RZ_SECRET_AREA_RANK_LIST msg)
        {
            MSG_ZGC_SECRET_AREA_RANK_LIST infos = new MSG_ZGC_SECRET_AREA_RANK_LIST();
            infos.Page = msg.Page;
            infos.TotalCount = msg.TotalCount;
            infos.RankType = msg.RankType;

            int rank = 0;
            foreach (var item in msg.RankList)
            {
                MSG_ZGC_SECRET_AREA_RANK_INFO info = new MSG_ZGC_SECRET_AREA_RANK_INFO();
                info.Uid = item.Uid;
                info.Name = item.Name;
                info.ShowDIYIcon = item.ShowDIYIcon;
                info.Icon = item.Icon;
                info.IconFrame = item.IconFrame;
                info.Level = item.Level;
                info.HisPrestige = item.HisPrestige;
                info.Family = item.Family;
                info.SecretAreaId = item.SecretAreaId;
                info.Rank = item.Rank;
                info.PassTime = item.PassTime;
                info.BattlePower = item.BattlePower;
                infos.RankInfos.Add(info);

                if (item.Uid == this.uid)
                {
                    rank = item.Rank;
                }
            }

            MSG_ZGC_SECRET_AREA_RANK_INFO self = new MSG_ZGC_SECRET_AREA_RANK_INFO();
            self.Uid = Uid;
            self.Name = Name;
            self.Icon = Icon;
            self.Level = Level;
            self.HisPrestige = HisPrestige;
            self.Family = FamilyId;
            self.SecretAreaId = SecretAreaManager.Id;
            self.PassTime = (int)SecretAreaManager.PassTime;
            self.BattlePower = HeroMng.CalcBattlePower();

            OperateGetSecretRank operate = new OperateGetSecretRank(MainId, uid);
            server.GameRedis.Call(operate, result =>
           {
               if ((int)result == 1)
               {
                   self.Rank = operate.Rank;
                   infos.OwnerInfo = self;
                   Write(infos);
               }
           });
        }

        #endregion
    }
}
