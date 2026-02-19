using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        //战斗完成后待选的buff
        private List<int> randomSelectBuffPool = new List<int>();

        public TowerManager TowerManager { get; private set; }
        public void InitTower()
        {
            TowerManager = new TowerManager(this);
        }

        public void UpdateTower()
        {
            TowerManager.Update();
        }

        public void TowerLimitOpen()
        {
            TowerManager.CheckTime();
        }

        private void ClearTowerRandomBuffPool()
        {
            randomSelectBuffPool.Clear();
        }

        public void TowerSuccess(RewardManager manager, int dungeonId, int time, int period)
        {
            manager.BreakupRewards();

            AddRewards(manager, ObtainWay.Tower, dungeonId.ToString());

            if (CurrentMap.IsDungeon)
            {
                MSG_ZGC_DUNGEON_REWARD msg = new MSG_ZGC_DUNGEON_REWARD();
                msg.PassTime = time;
                msg.DungeonId = dungeonId;
                msg.Result = (int)DungeonResult.Success;
                manager.GenerateRewardMsg(msg.Rewards);

                Write(msg);
            }

            //跨期了则只发奖励
            if (period == TowerManager.Period)
            {
                //随机buff
                TowerRandomBuff(dungeonId);

                TowerManager.GotoNextNode();

                //爬塔
                AddTaskNumForType(TaskType.TowerStage, dungeonId, false);
                AddTaskNumForType(TaskType.TowerCount);
                AddPassCardTaskNum(TaskType.TowerCount);
                AddSchoolTaskNum(TaskType.TowerStage);
                AddSchoolTaskNum(TaskType.TowerCount);

                if (TowerManager.NodeId == TowerLibrary.MaxNode)
                {
                    AddTaskNumForType(TaskType.TowerFinish);
                }
            }
        }

        private void TowerRandomBuff(int dungeonId)
        {
            //先保存上次没选的buff
            TowerSelectBuffAuto();

            TowerDungeonModel model = TowerLibrary.GetTowerDungeonModel(dungeonId);
            if (model == null) return;

            List<int> buffList = new List<int>(TowerManager.BuffInfoList);

            for (int i = 0; i < 3; i++)
            {
                var buffModel = TowerLibrary.RandomBuff(model.BuffQualityWeight, buffList, TowerManager.HeroPos);
                if (buffModel == null)
                {
                    Log.Warn($"TowerRandomBuff error have not random buff tower dungeon model id {model.Id}，check it !");
                    continue;
                }
                buffList.Add(buffModel.Id);
                randomSelectBuffPool.Add(buffModel.Id);
            }

            SendTowerRandomBuff();
        }

        public void TowerSelectBuffAuto(int index = 0)
        {
            if (index >= randomSelectBuffPool.Count) return;

            TowerManager.AddBuff(randomSelectBuffPool[index]);

            ClearTowerRandomBuffPool();

            TowerManager.SyncBuffToDB();
        }

        public void GetTowerInfo()
        {
            if (!TowerManager.IsOpening())
            {
                MSG_ZGC_TOWER_INFO msg = new MSG_ZGC_TOWER_INFO();
                msg.Result = (int)ErrorCode.TowerNotOpen;
                Write(msg);
                return;
            }

            SendTowerInfo();
            SendTowerHeroInfo();
            SendTowerDungeonGrowth();
        }

        public void SendTowerInfo()
        {
            MSG_ZGC_TOWER_INFO msg = TowerManager.GenerateMsg();
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void SendTowerTime()
        {
            MSG_ZGC_TOWER_TIME msg = new MSG_ZGC_TOWER_TIME();
            msg.Status = TowerManager.IsOpening();
            msg.Time = msg.Status ? Timestamp.GetUnixTimeStampSeconds(TowerManager.StopTime) : Timestamp.GetUnixTimeStampSeconds(TowerManager.StartTime);
            Write(msg);
        }

        public void SendTowerRandomBuff()
        {
            MSG_ZGC_TOWER_RANDOM_BUFF msg = new MSG_ZGC_TOWER_RANDOM_BUFF();
            msg.BuffList.AddRange(randomSelectBuffPool);
            Write(msg);
        }

        public void SendTowerHeroInfo()
        {
            MSG_ZGC_INIT_TOWER_HERO_INFO msg = TowerManager.GenerateHeroInfo();
            Write(msg);
        }

        public void SendTowerDungeonGrowth()
        {
            MSG_ZGC_TOWER_DUNGOEN_GROWTH msg = TowerManager.GenerateDungeonGrowth();
            Write(msg);
        }

        public ErrorCode CheckCanCreateTowerDungeon(int towerDungeonId)
        {
            TowerDungeonModel dungeonModel = TowerLibrary.GetTowerDungeonModel(towerDungeonId);
            if (dungeonModel == null) return ErrorCode.Fail;

            if (TowerManager.CheckPosDeadHero()) return ErrorCode.TowerEquipDeadHero;

            if (TowerManager.HeroPos.Count == 0) return ErrorCode.TowerFormationNoHero;

            return ErrorCode.Success;
        }

        public void ExecuteTowerTask(int taskId, int param)
        {
            MSG_ZGC_TOWER_EXECUTE_TASK msg = new MSG_ZGC_TOWER_EXECUTE_TASK();
            if (!TowerManager.IsOpening())
            {
                Log.Warn($"player {Uid} execute tower task {taskId} failed: tower not open");
                msg.Result = (int)ErrorCode.TowerNotOpen;
                Write(msg);
                return;
            }

             TowerTaskModel model = TowerLibrary.GetTaskModel(taskId);

            if (model == null)
            {
                Log.Warn($"player {Uid} execute tower task {taskId} failed: not find task");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.GroupId != TowerManager.GroupId)
            {
                Log.Warn($"player {Uid}  execute tower task {taskId} failed: groupId {TowerManager.GroupId} model groupId {model.GroupId} not equals");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.GroupId == TowerLibrary.GetMaxNodeId(TowerManager.GroupId))
            {
                Log.Warn($"player {Uid} execute tower task {taskId} failed: tower group {model.GroupId} all passed");
                msg.Result = (int)ErrorCode.TowerPassAll;
                Write(msg);
                return;
            }

            if (model.NodeId <= TowerManager.NodeId || model.NodeId > TowerManager.NodeId + 1)
            {
                Log.Warn($"player {Uid}  execute tower task {taskId} failed: model nodeId {model.NodeId} nodeId {TowerManager.NodeId}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (TowerManager.TowerTask == null)
            {
                if (model.Type == TowerTaskType.Dungeon)
                {
                    ErrorCode code = CheckCanCreateTowerDungeon(param);
                    if (code != ErrorCode.Success)
                    {
                        Log.Warn($"player {Uid} execute tower task {taskId} failed: errorCode {(int)code}");
                        msg.Result = (int)code;
                        Write(msg);
                        return;
                    }
                }

                TowerManager.SetTaskId(model.Id);
                TowerManager.SyncTaskInfoToDB();
            }
            else
            {
                //当前有正在进行的任务，只能做该任务
                if (TowerManager.TaskId != taskId)
                {
                    Log.Warn($"player {Uid} execute tower task {taskId} failed: task {TowerManager.TaskId} not finish");
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
            }

            ErrorCode errorCode = TowerManager.TowerTask.Execute(param, msg);
            if (errorCode != ErrorCode.Success)
            {
                Log.Warn($"player {Uid} execute tower task {taskId} failed: excute errorCode {(int)errorCode}");
                msg.Result = (int)errorCode;
                Write(msg);
                return;
            }
            msg.TaskId = taskId;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void TowerSelectBuff(int index)
        {
            MSG_ZGC_TOWER_SELECT_BUFF msg = new MSG_ZGC_TOWER_SELECT_BUFF();
            if (randomSelectBuffPool.Count == 0)
            {
                Log.Warn($"player {Uid} tower select buff {index} failed: buff pool not have buff");
                msg.Result = (int)ErrorCode.TowerHaveNoCachedBuff;
                Write(msg);
                return;
            }

            if (index >= randomSelectBuffPool.Count)
            {
                Log.Warn($"player {Uid} tower select buff {index} failed: index not in buff pool");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            TowerBuffModel model = TowerLibrary.GetTowerBuffModel(randomSelectBuffPool[index]);
            if (model == null)
            {
                Log.Warn($"player {Uid} tower select buff {index} failed: not find buff in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            TowerSelectBuffAuto(index);

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void TowerBuffList()
        {
            MSG_ZGC_TOWER_BUFF msg = new MSG_ZGC_TOWER_BUFF();
            msg.BuffList.AddRange(TowerManager.BuffInfoList);
            Write(msg);
        }

        public void GetTowerReward(int id)
        {
            MSG_ZGC_TOWER_REWARD msg = new MSG_ZGC_TOWER_REWARD()
            {
                Id = id,
            };

            TowerRewardModel model = TowerLibrary.GetTowerModel(id);
            if (id <= 0 || model == null)
            {
                Log.Warn($"player {Uid} get tower reward {id} failed : not find in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (id > TowerManager.NodeId)
            {
                Log.Warn($"player {Uid} get tower reward {id} failed : tower not passed");
                msg.Result = (int)ErrorCode.TowerNeedPassed;
                Write(msg);
                return;
            }

            if (!TowerManager.RewardList.Contains(id))
            {
                Log.Warn($"player {Uid} get tower reward {id} failed : had rewarded");
                msg.Result = (int)ErrorCode.TowerHadRewarded;
                Write(msg);
                return;
            }

            string reward = model.Data.GetString("Reward");
            if (string.IsNullOrEmpty(reward))
            {
                Log.Warn($"player {Uid} get tower reward {id} failed : not find reward in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            manager.AddSimpleReward(reward);
            manager.BreakupRewards();

            TowerManager.RemoveId(id);

            AddRewards(manager, ObtainWay.Tower);

            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.Result = (int)ErrorCode.Success;
            msg.RewardList.AddRange(TowerManager.RewardList);
            Write(msg);
        }

        public void TowerShopItemList(int taskId)
        {
            MSG_ZGC_TOWER_SHOP_ITEM msg = new MSG_ZGC_TOWER_SHOP_ITEM() { TaskId = taskId };
            if (!TowerManager.IsOpening())
            {
                Log.Warn($"player {Uid} get tower {taskId} shopItemList failed: tower not open");
                msg.Result = (int)ErrorCode.TowerNotOpen;
                Write(msg);
                return;
            }

            TowerTaskModel model = TowerLibrary.GetTaskModel(taskId);

            if (model == null || model.Type != TowerTaskType.Shop)
            {
                Log.Warn($"player {Uid} get tower {taskId} shopItemList failed: not find task");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.GroupId != TowerManager.GroupId)
            {
                Log.Warn($"player {Uid} get tower {taskId} shopItemList failed: groupId {TowerManager.GroupId} model groupId {model.GroupId} not equals");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.GroupId == TowerLibrary.GetMaxNodeId(TowerManager.GroupId))
            {
                Log.Warn($"player {Uid} get tower {taskId} shopItemList failed: tower group {model.GroupId} all passed");
                msg.Result = (int)ErrorCode.TowerPassAll;
                Write(msg);
                return;
            }

            if (model.NodeId <= TowerManager.NodeId)
            {
                Log.Warn($"player {Uid} get tower {taskId} shopItemList failed: model nodeId {model.NodeId} nodeId {TowerManager.NodeId}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            TowerTaskInfo info = TowerManager.GetTaskInfo(model.NodeId, model.Id);
            if (info == null)
            {
                Log.Warn($"player {Uid} get tower {taskId} shopItemList failed: not find task info");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (TowerManager.RandomShopItem(info))
            {
                info.BuildMsg(msg.ShopItems);
            }
            SendTowerInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void TowerUpdateHeroPos(RepeatedField<MSG_GateZ_HERO_POS> heroPos)
        {
            MSG_ZGC_UPDATE_TOWER_HERO_POS msg = new MSG_ZGC_UPDATE_TOWER_HERO_POS();
            foreach (var kv in heroPos)
            {
                if (TowerManager.DeadHeroList.Contains(kv.HeroId))
                {
                    Log.Warn($"player {Uid} update tower heroPos failed : hero {kv.HeroId} dead");
                    msg.Result = (int)ErrorCode.TowerEquipDeadHero;
                    Write(msg);
                    return;
                }

                if (TowerManager.CheckJobLimited(kv.HeroId))
                {
                    Log.Warn($"player {Uid} update tower heroPos failed : hero {kv.HeroId} limited");
                    msg.Result = (int)ErrorCode.HeroJobLimited;
                    Write(msg);
                    return;
                }
            }
            Dictionary<int, int> heroPosBefore = new Dictionary<int, int>(TowerManager.HeroPos);

            if (heroPos.Count > 5)
            {
                Log.Warn($"player {Uid} update tower heroPos failed : over limit");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            TowerManager.SetHeroPos(heroPos);

            SendTowerHeroInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            //komoeLog
            KomoeLogRecordBattleteamFlow("迷岛", heroPosBefore, TowerManager.HeroPos);
        }

        public void TowerReviveHero()
        {
            MSG_ZGC_TOWER_HERO_REVIVE msg = new MSG_ZGC_TOWER_HERO_REVIVE();

            int cost = TowerLibrary.GetReviveCost(TowerManager.ReviveCount + 1);

            if (!CheckCoins(CurrenciesType.diamond, cost))
            {
                Log.Warn($"player {Uid} revive tower hero failed : coin not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {cost}");
                msg.Result = (int)ErrorCode.NoCoin;
                Write(msg);
                return;
            }

            if (TowerManager.DeadHeroList.Count == 0)
            {
                Log.Warn($"player {Uid} revive tower hero failed: no dead hero");
                msg.Result = (int)ErrorCode.TowerNoDeadHero;
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.diamond, cost, ConsumeWay.Tower, TowerManager.ReviveCount.ToString());

            TowerManager.ReviveAllHero();
            SendTowerHeroInfo();

            msg.ReviveCount = TowerManager.ReviveCount;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }
    }
}
