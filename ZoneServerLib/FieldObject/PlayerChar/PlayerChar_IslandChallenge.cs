using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public IslandChallengeManager IslandChallengeManager { get; private set; }

        private void InitIslandChallenge()
        {
            IslandChallengeManager = new IslandChallengeManager(this);
        }

        private void UpdateIslandChallenge()
        {
            IslandChallengeManager.Update();
        }

        private void IslandChallengeLimitOpen()
        {
            IslandChallengeManager.CheckTime();
        }

        public void IslandChallengeFail(int dungeonId, int period)
        {
            IslandChallengeManager.SetDungeonResult(dungeonId, false, period);
        }

        public void IslandChallengeSuccess(RewardManager manager, int dungeonId, int time, int period)
        {
            manager.BreakupRewards();

            AddRewards(manager, ObtainWay.IslandChallenge, dungeonId.ToString());

            if (CurrentMap.IsDungeon)
            {
                MSG_ZGC_DUNGEON_REWARD msg = new MSG_ZGC_DUNGEON_REWARD();
                msg.PassTime = time;
                msg.DungeonId = dungeonId;
                msg.Result = (int)DungeonResult.Success;
                manager.GenerateRewardMsg(msg.Rewards);

                CheckCacheRewardMsg(msg);
            }

            IslandChallengeManager.SetDungeonResult(dungeonId, true, period);

            //爬塔
            //AddTaskNumForType(TaskType.TowerStage, dungeonId, false);
            //AddTaskNumForType(TaskType.TowerCount);
            //AddPassCardTaskNum(TaskType.TowerCount);

            //if (IslandChallengeManager.NodeId == IslandChallengeLibrary.MaxNode)
            //{ 
            //    AddTaskNumForType(TaskType.TowerFinish);
            //}
        }

        public void GetIslandChallengeInfo()
        {
            if (!IslandChallengeManager.IsOpening())
            {
                MSG_ZGC_ISLAND_CHALLENGE_INFO msg = new MSG_ZGC_ISLAND_CHALLENGE_INFO();
                msg.Result = (int)ErrorCode.TowerNotOpen;
                Write(msg);
                return;
            }

            SendIslandChallengeInfo();
            SendIslandChallengeHeroInfo();
            SendIslandChallengeDungeonGrowth();
        }

        #region MyRegion

        public void SendIslandChallengeInfo()
        {
            MSG_ZGC_ISLAND_CHALLENGE_INFO msg = IslandChallengeManager.GenerateMsg();
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void SendIslandChallengeTime()
        {
            MSG_ZGC_ISLAND_CHALLENGE_TIME msg = new MSG_ZGC_ISLAND_CHALLENGE_TIME();
            msg.Status = IslandChallengeManager.IsOpening();
            msg.Time = msg.Status ? Timestamp.GetUnixTimeStampSeconds(IslandChallengeManager.StopTime) : Timestamp.GetUnixTimeStampSeconds(IslandChallengeManager.StartTime);
            Write(msg);
        }

        public void SendIslandChallengeHeroInfo()
        {
            MSG_ZGC_ISLAND_CHALLENGE_HERO_INFO msg = IslandChallengeManager.GenerateHeroInfo();
            Write(msg);
        }

        public void SendIslandChallengeDungeonGrowth()
        {
            MSG_ZGC_ISLAND_CHALLENGE_DUNGOEN_GROWTH msg = IslandChallengeManager.GenerateDungeonGrowth();
            Write(msg);
        }

        public void SendIslandChallengeWinInfo()
        {
            MSG_ZGC_ISLAND_CHALLENGE_UPDATE_WININFO msg = IslandChallengeManager.GenerateWinInfo();
            Write(msg);
        }

        #endregion

        public ErrorCode CheckCanCreateIslandChallengeDungeon(int towerDungeonId, bool canBeNull = false)
        {
            if (IslandChallengeManager.WinInfo.ContainsKey(towerDungeonId)) return ErrorCode.Fail;

            if (IslandChallengeManager.HeroPos.Count == 0) return ErrorCode.TowerFormationNoHero;

            BaseIslandChallengeTask task = IslandChallengeManager.Task;
            if (task != null)
            {
                if (task.Type != TowerTaskType.Dungeon || task.TaskInfo.param.Count < 1) return ErrorCode.Fail;

                IslandChallengeDungeonModel dungeonModel = IslandChallengeLibrary.GetIslandChallengeDungeonModel(task.TaskInfo.param[0]);
                if (dungeonModel == null) return ErrorCode.Fail;

                if (!dungeonModel.Dungeon2Queue.ContainsKey(towerDungeonId)) return ErrorCode.Fail;

                Dictionary<int, int> heroPos = new Dictionary<int, int>();
                if (!IslandChallengeManager.GetHeroPos(towerDungeonId, heroPos) || heroPos == null || heroPos.Count <= 0) return ErrorCode.IslandChallengeNeedSetHeroPos;

                if (IslandChallengeManager.CheckPosDeadHero(heroPos)) return ErrorCode.TowerEquipDeadHero;
            }
            else
            {
                if (!canBeNull) return ErrorCode.Fail;

                if (IslandChallengeManager.HeroPos.Count<=0) return ErrorCode.IslandChallengeNeedSetHeroPos;

                if (IslandChallengeManager.CheckDeadHero()) return ErrorCode.TowerEquipDeadHero;
            }

            return ErrorCode.Success;
        }

        public void ExecuteIslandChallengeTask(int taskId, int param)
        {
            MSG_ZGC_ISLAND_CHALLENGE_EXECUTE_TASK msg = new MSG_ZGC_ISLAND_CHALLENGE_EXECUTE_TASK();
            if (!IslandChallengeManager.IsOpening())
            {
                Log.Warn($"player {Uid} execute IslandChallenge task {taskId} failed: IslandChallenge not open");
                msg.Result = (int)ErrorCode.TowerNotOpen;
                Write(msg);
                return;
            }

            IslandChallengeTaskModel model = IslandChallengeLibrary.GetIslandChallengeTaskModel(taskId);
            if (model == null)
            {
                Log.Warn($"player {Uid} execute IslandChallenge task {taskId} failed: not find task");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.GroupId != IslandChallengeManager.GroupId)
            {
                Log.Warn($"player {Uid}  execute IslandChallenge task {taskId} failed: groupId {IslandChallengeManager.GroupId} model groupId {model.GroupId} not equals");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.GroupId == IslandChallengeLibrary.GetMaxNodeId(IslandChallengeManager.GroupId))
            {
                Log.Warn($"player {Uid} execute IslandChallenge task {taskId} failed: IslandChallenge group {model.GroupId} all passed");
                msg.Result = (int)ErrorCode.TowerPassAll;
                Write(msg);
                return;
            }

            if (model.NodeId <= IslandChallengeManager.NodeId || model.NodeId> IslandChallengeManager.NodeId + 1)
            {
                Log.Warn($"player {Uid}  execute IslandChallenge task {taskId} failed: model nodeId {model.NodeId} nodeId {IslandChallengeManager.NodeId}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (IslandChallengeManager.Task == null)
            {
                if (model.Type == TowerTaskType.Dungeon)
                {
                    ErrorCode code = CheckCanCreateIslandChallengeDungeon(param, true);
                    if (code != ErrorCode.Success)
                    {
                        Log.Warn($"player {Uid} execute IslandChallenge task {taskId} failed: errorCode {(int)code}");
                        msg.Result = (int)code;
                        Write(msg);
                        return;
                    }
                }

                IslandChallengeManager.SetTaskId(model.Id);
                IslandChallengeManager.SyncTaskInfoToDB();
            }
            else
            {
                //当前有正在进行的任务，只能做该任务
                if (IslandChallengeManager.TaskId != taskId)
                {
                    Log.Warn($"player {Uid} execute IslandChallenge task {taskId} failed: task {IslandChallengeManager.TaskId} not finish");
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
            }

            ErrorCode errorCode = IslandChallengeManager.Task.Execute(param, msg);
            if (errorCode != ErrorCode.Success)
            {
                Log.Warn($"player {Uid} execute IslandChallenge task {taskId} failed: excute errorCode {(int)errorCode}");
                msg.Result = (int)errorCode;
                Write(msg);
                return;
            }
            msg.TaskId = taskId;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void GetIslandChallengeReward(int id, int type)
        {
            //type =1 是分支奖励，2-节点奖励
            MSG_ZGC_ISLAND_CHALLENGE_REWARD msg = new MSG_ZGC_ISLAND_CHALLENGE_REWARD() {Id = id};
            string reward = string.Empty;

            bool gotoNextNode = false;
            switch (type)
            {
                case 1:
                    TowerRewardModel model = IslandChallengeLibrary.GetIslandChallengeRewardModel(id);
                    if (id <= 0 || model == null)
                    {
                        Log.Warn($"player {Uid} get IslandChallenge reward {id} failed : not find in xml");
                        msg.Result = (int)ErrorCode.Fail;
                        Write(msg);
                        return;
                    }

                    if (!IslandChallengeManager.RewardList.Contains(id))
                    {
                        Log.Warn($"player {Uid} get IslandChallenge reward {id} failed : had rewarded");
                        msg.Result = (int)ErrorCode.TowerHadRewarded;
                        Write(msg);
                        return;
                    }

                    reward = model.Data.GetString("Reward");
                    break;
                case 2:

                    if (!IslandChallengeManager.IsPassedDungeonNode())
                    {
                        Log.Warn($"player {Uid} get IslandChallenge reward {id} failed: win count not enough");
                        msg.Result = (int)ErrorCode.IslandChallengeWinCountNotEnough;
                        Write(msg);
                        return;
                    }

                    if (!IslandChallengeManager.NodeRewarded)
                    {
                        Log.Warn($"player {Uid} get IslandChallenge reward challengeTaskModel {IslandChallengeManager.TaskId} failed : have no reward");
                        msg.Result = (int)ErrorCode.Fail;
                        Write(msg);
                        return;
                    }

                    IslandChallengeTaskModel challengeTaskModel = IslandChallengeLibrary.GetIslandChallengeTaskModel(IslandChallengeManager.TaskId);
                    if (id <= 0 || challengeTaskModel == null)
                    {
                        Log.Warn($"player {Uid} get IslandChallenge reward challengeTaskModel {IslandChallengeManager.TaskId} failed : not find in xml");
                        msg.Result = (int)ErrorCode.Fail;
                        Write(msg);
                        return;
                    }

                    gotoNextNode = true;
                    reward = challengeTaskModel.Reward;
                    break;
            }

            if (string.IsNullOrEmpty(reward))
            {
                Log.Warn($"player {Uid} get IslandChallenge reward {id} failed : not find reward in xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            manager.AddSimpleReward(reward);
            manager.BreakupRewards();

            if (type == 1)
            {
                IslandChallengeManager.RemoveId(id);
            }

            AddRewards(manager, ObtainWay.IslandChallenge);

            manager.GenerateRewardItemInfo(msg.Rewards);

            if (gotoNextNode)
            {
                IslandChallengeManager.SetNodeRewarded(false);
                IslandChallengeManager.ResetWinInfo(false);
                IslandChallengeManager.GotoNextNode();
            }

            msg.Result = (int)ErrorCode.Success;
            msg.RewardList.AddRange(IslandChallengeManager.RewardList);
            Write(msg);
        }

        public void IslandChallengeShopItemList(int taskId)
        {
            MSG_ZGC_ISLAND_CHALLENGE_SHOP_ITEM msg = new MSG_ZGC_ISLAND_CHALLENGE_SHOP_ITEM() { TaskId = taskId };
            if (!IslandChallengeManager.IsOpening())
            {
                Log.Warn($"player {Uid} get IslandChallenge {taskId} shopItemList failed: IslandChallenge not open");
                msg.Result = (int)ErrorCode.TowerNotOpen;
                Write(msg);
                return;
            }

            TowerTaskModel model = IslandChallengeLibrary.GetIslandChallengeTaskModel(taskId);

            if (model == null || model.Type != TowerTaskType.Shop)
            {
                Log.Warn($"player {Uid} get IslandChallenge {taskId} shopItemList failed: not find task");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.GroupId != IslandChallengeManager.GroupId)
            {
                Log.Warn($"player {Uid} get IslandChallenge {taskId} shopItemList failed: groupId {IslandChallengeManager.GroupId} model groupId {model.GroupId} not equals");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.GroupId == IslandChallengeLibrary.GetMaxNodeId(IslandChallengeManager.GroupId))
            {
                Log.Warn($"player {Uid} get IslandChallenge {taskId} shopItemList failed: IslandChallenge group {model.GroupId} all passed");
                msg.Result = (int)ErrorCode.TowerPassAll;
                Write(msg);
                return;
            }

            if (model.NodeId <= IslandChallengeManager.NodeId)
            {
                Log.Warn($"player {Uid} get IslandChallenge {taskId} shopItemList failed: model nodeId {model.NodeId} nodeId {IslandChallengeManager.NodeId}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            IslandChallengeTaskInfo info = IslandChallengeManager.GetTaskInfo(model.NodeId);
            if (info == null)
            {
                Log.Warn($"player {Uid} get IslandChallenge {taskId} shopItemList failed: not find task info");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (IslandChallengeManager.RandomShopItem(info))
            {
                info.BuildMsg(msg.ShopItems);
            }
            SendIslandChallengeInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void IslandChallengeUpdateHeroPos(RepeatedField<MSG_GateZ_HERO_POS> heroPos)
        {
            MSG_ZGC_ISLAND_CHALLENGE_HERO_POS msg = new MSG_ZGC_ISLAND_CHALLENGE_HERO_POS();
            if (heroPos.Count <= 0)
            {
                msg.Result = (int)ErrorCode.Success;
                Write(msg);
                SendIslandChallengeHeroInfo();
                return;
            }

            int queue = 0;
            if (heroPos.Count > 0)
            {
                queue = heroPos.First().Queue;
            }

            bool allow = true;
            List<int> updateQueue = new List<int>() { 1, 2, 3 };

            if (IslandChallengeManager.WinInfo.Count > 0)
            {
                BaseIslandChallengeTask task = IslandChallengeManager.Task;
                if (task?.Type == TowerTaskType.Dungeon)
                {
                    IslandChallengeDungeonModel model = IslandChallengeLibrary.GetIslandChallengeDungeonModel(task.TaskInfo.param[0]);
                    if (model == null)
                    {
                        msg.Result = (int)ErrorCode.Fail;
                        Write(msg);
                        SendIslandChallengeHeroInfo();
                        return;
                    }

                    foreach (var win in IslandChallengeManager.WinInfo)
                    {
                        int tempQueue;
                        if (model.Dungeon2Queue.TryGetValue(win.Key, out tempQueue))
                        {
                            updateQueue.Remove(tempQueue);
                        }
                    }
                }
            }

            //已经挑战过的副本不允许调整
            if (!updateQueue.Contains(queue))
            {
                Log.Warn($"player {Uid} update IslandChallenge heroPos failed : need pass or reset");
                msg.Result = (int)ErrorCode.IslandChallengeCannotSetPos;
                Write(msg);
                SendIslandChallengeHeroInfo();
                return;
            }

            bool haveDeadHero = heroPos.FirstOrDefault(x => IslandChallengeManager.DeadHeroList.Contains(x.HeroId)) != null;
            if (haveDeadHero)
            {
                Log.Warn($"player {Uid} update IslandChallenge heroPos failed : hero  dead hero");
                msg.Result = (int)ErrorCode.TowerEquipDeadHero;
                SendIslandChallengeHeroInfo();
                Write(msg);
                return;
            }

            Dictionary<int, int> heroPosBefore = new Dictionary<int, int>();
            Dictionary<int, int> queueHero = IslandChallengeManager.GetQueue(queue);
            if (queueHero != null)
            {
                heroPosBefore = new Dictionary<int, int>(queueHero);
            }

            if (!IslandChallengeManager.SetHeroPos(heroPos, queue))
            {
                msg.Result = (int)ErrorCode.Fail;
                SendIslandChallengeHeroInfo();
                Write(msg);
                return;
            }

            TrackDungeonQueueLog(HeroQueueType.IslandChallenge, null, IslandChallengeManager.GetTrackHeroPosStr());

            SendIslandChallengeHeroInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            //komoeLog
            KomoeLogRecordBattleteamFlow("迷岛挑战", heroPosBefore, IslandChallengeManager.GetQueue(queue));
        }

        public void IslandChallengeSwapQueue(int queue1, int queue2)
        {
            MSG_ZGC_ISLAND_CHALLENGE_SWAP_QUEUE msg = new MSG_ZGC_ISLAND_CHALLENGE_SWAP_QUEUE();
            if (!IslandChallengeManager.IsOpening())
            {
                Log.Warn($"player {Uid} IslandChallengeSwapQueue failed: IslandChallenge not open");
                msg.Result = (int)ErrorCode.TowerNotOpen;
                Write(msg);
                return;
            }

            if (!IslandChallengeManager.HeroPos.ContainsKey(queue1) ||
                !IslandChallengeManager.HeroPos.ContainsKey(queue2))
            {
                msg.Result = (int)ErrorCode.IslandChallengeCannotSetPos;
                Write(msg);
                return;
            }

            if (IslandChallengeManager.WinInfo.ContainsKey(queue1) ||
                IslandChallengeManager.WinInfo.ContainsKey(queue2))
            {
                msg.Result = (int)ErrorCode.IslandChallengeCannotSetPos;
                Write(msg);
                return;
            }

            IslandChallengeManager.SwapQueue(queue1, queue2);
            SendIslandChallengeHeroInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void IslandChallengeReviveHero()
        {
            MSG_ZGC_ISLAND_CHALLENGE_HERO_REVIVE msg = new MSG_ZGC_ISLAND_CHALLENGE_HERO_REVIVE();

            int cost = IslandChallengeLibrary.GetReviveCost(IslandChallengeManager.ReviveCount + 1);

            if (!CheckCoins(CurrenciesType.diamond, cost))
            {
                Log.Warn($"player {Uid} revive IslandChallenge hero failed : coin not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {cost}");
                msg.Result = (int)ErrorCode.NoCoin;
                Write(msg);
                return;
            }

            if (IslandChallengeManager.DeadHeroList.Count == 0)
            {
                Log.Warn($"player {Uid} revive IslandChallenge hero failed: no dead hero");
                msg.Result = (int)ErrorCode.TowerNoDeadHero;
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.diamond, cost, ConsumeWay.Tower, IslandChallengeManager.ReviveCount.ToString());

            IslandChallengeManager.ReviveAllHero();
            SendIslandChallengeHeroInfo();

            msg.ReviveCount = IslandChallengeManager.ReviveCount;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void IslandChallengeReset()
        {
            MSG_ZGC_ISLAND_CHALLENGE_RESET msg = new MSG_ZGC_ISLAND_CHALLENGE_RESET();
            if (!IslandChallengeManager.IsOpening())
            {
                Log.Warn($"player {Uid} IslandChallengeReset failed: IslandChallenge not open");
                msg.Result = (int)ErrorCode.TowerNotOpen;
                Write(msg);
                return;
            }

            IslandChallengeManager.ResetWinInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

    }
}
