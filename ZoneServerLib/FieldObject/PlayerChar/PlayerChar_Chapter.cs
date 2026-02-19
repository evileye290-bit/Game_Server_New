using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerShared;
using ServerModels;
using CommonUtility;
using DataProperty;
using System.Linq;
using System.Collections.Generic;
using EnumerateUtility.Activity;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        private ChapterManager ChapterManager { get;  set; }

        public void InitChapterManager()
        {
            ChapterManager = new ChapterManager(this);
        }

        public void DeleteSpaceTimePower(int count)
        {
            DelCoins(CurrenciesType.spaceTimePower, count, ConsumeWay.Chapter, "");
        }

        public void UpdateChapter(double dt)
        {
            ChapterManager.Update();
        }

        public void CheckChapterTask(int taskId)
        {
            ChapterManager.CheckPowerLimit(taskId);
            CheckChapterReward();
        }

        public bool CheckTaskFinished(int taskId)
        {
            if(TaskMng.CurrMaxMainTaskId > taskId)
            {
                return true;
            }

            //任务完成条件已经达成，但是没有提交不算完成
            //TaskItem task = TaskMng.GetTaskItemForId(taskId);
            //if (task != null)
            //{
            //    return TaskMng.CheckTaskComplete(task);
            //}
            return false;
        }

        public void ChapterReward(RewardManager manager, DungeonModel model)
        {
            DeleteSpaceTimePower(model.Power);



            StoryLineModel storyLine = ChapterLibrary.GetStoryLineModelByDungeonId(model.Id);
            if (storyLine != null)
            {
                ChapterManager.CheckPowerLimit(storyLine.TaskId);
            }

            Chapter chapter = ChapterManager.GetChapter(storyLine.Chapter);
            if (chapter != null)
            {
                chapter.AddPassedDungeon(model.Id);
            }

            //章节奖励随机掉落
            //manager.AddReward(RewardDropLibrary.GetProbability(1, rewardStr));
            List<int> rwardDropIds;
            if (ChapterManager.HadPassedDungeon(model.Id))
            {
                rwardDropIds = model.Data.GetIntList("FirstRewardId", "|");
            }
            else
            {
                rwardDropIds = model.Data.GetIntList("GeneralRewardId", "|");
            }
            List<ItemBasicInfo> getList = AddRewardDrop(rwardDropIds);
            manager.AddReward(getList);
            manager.BreakupRewards(true);

            // 发放奖励
            AddRewards(manager, ObtainWay.Chapter, model.Data.Name);

            //通知前端奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = GetRewardSyncMsg(manager);
            rewardMsg.DungeonId = model.Id;
            rewardMsg.Result = (int)DungeonResult.Success;
            Write(rewardMsg);
        }

        //章节红点检测
        public void CheckChapterReward()
        {
            MSG_ZGC_CHAPTER_REWATRD_REDDOT msg = ChapterManager.GetReward();
            Write(msg);
        }

        public void GetChapterInfo(int chapterId)
        {
            MSG_ZGC_CHAPTER_INFO msg = new MSG_ZGC_CHAPTER_INFO
            {
                PowerLimit = ChapterManager.TimeSpacePowerUpperLimit
            };

            if (!CheckLimitOpen(LimitType.Tower))
            {
                Logger.Log.Warn($"player {Uid} get chapter {chapterId} info failed: tower not open");
                msg.ErrorCode = (int)ErrorCode.NotOpen;
                Write(msg);
                return;
            }

            if (!server.WorldLevelManager.CheckChapterOpend(chapterId))
            {
                Logger.Log.Warn($"player {Uid} get chapter {chapterId} info failed: chapter not open");
                msg.ErrorCode = (int)ErrorCode.ChapterNotOpen;
                Write(msg);
                return;
            }

            Chapter chapter = ChapterManager.GetChapter(chapterId);
            if (chapter == null)
            {
                chapter = ChapterManager.AddChapter(chapterId);
            }

            chapter.GenerateChapterMsg(msg);
            msg.ErrorCode = (int)ErrorCode.Success;
            msg.ServerLevel = server.WorldLevelManager.ServerLevel;
            msg.ServerOpenDays = server.WorldLevelManager.CurrLevelDays;
            msg.OpenServerDate = Timestamp.GetUnixTimeStampSeconds(server.OpenServerDate);

            Write(msg);
        }

        public void GetChapterNextPageInfo(int chapterId)
        {
            MSG_ZGC_CHAPTER_NEXT_PAGE msg = new MSG_ZGC_CHAPTER_NEXT_PAGE
            {
                ChapterId = chapterId
            };

            if (!CheckLimitOpen(LimitType.Tower))
            {
                Logger.Log.Warn($"player {Uid} get chapter {chapterId} next page info failed: tower not open");
                msg.ErrorCode = (int)ErrorCode.NotOpen;
                Write(msg);
                return;
            }

            if (!server.WorldLevelManager.CheckChapterOpend(chapterId))
            {
                Logger.Log.Warn($"player {Uid} get chapter {chapterId} next page info failed: chapter not open");
                msg.ErrorCode = (int)ErrorCode.ChapterNotOpen;
                Write(msg);
                return;
            }

            Chapter chapter = ChapterManager.GetChapter(chapterId);
            if (chapter == null)
            {
                chapter = ChapterManager.AddChapter(chapterId);
            }

            msg.ErrorCode = (int)ErrorCode.Success;
            msg.BoxReward.AddRange(chapter.BoxRewardList);
            msg.DungeonIds.AddRange(chapter.PassedDungeonIds);
            msg.StoryReward.AddRange(chapter.StoryRewardList);

            Write(msg);
        }

        public void GetChapterReward(int chapterId, int rewardType, int id)
        {
            MSG_ZGC_CHAPTER_REWARD msg = new MSG_ZGC_CHAPTER_REWARD();
            Chapter chapter = ChapterManager.GetChapter(chapterId);
            if (chapter == null)
            {
                Logger.Log.Warn($"player {Uid} get chapter {chapterId} reward failed: not find chapter");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
            msg.ChapterId = chapter.ChapterId;

            if (rewardType == 1)
            {
                GetChapterBoxReward(msg, chapter, id);
            }
            else if (rewardType == 2)
            {
                GetChapterStoryReward(msg, chapter, id);
            }
        }

        private void GetChapterStoryReward(MSG_ZGC_CHAPTER_REWARD msg, Chapter chapter, int id)
        {
            StoryLineModel model = ChapterLibrary.GetStoryLine(id);
            if (model == null)
            {
                Logger.Log.Warn($"player {Uid} get chapter story reward {id} failed: not find reward");
                msg.ErrorCode = (int)ErrorCode.ChapterNotExitTheReward;
                Write(msg);
                return;
            }

            if (!CheckTaskFinished(model.TaskId))
            {
                Logger.Log.Warn($"player {Uid} get chapter story reward {id} failed: task not finished");
                msg.ErrorCode = (int)ErrorCode.EarlierTaskNotFinished;
                Write(msg);
                return;
            }

            if (chapter.StoryRewardList.Contains(id))
            {
                Logger.Log.Warn($"player {Uid} get chapter story reward {id} failed: reward had gived");
                msg.ErrorCode = (int)ErrorCode.ChapterRewardHadGived;
                Write(msg);
                return;
            }
            msg.TaskChain = model.TaskChain;
            chapter.AddStoryReward(id);
            ChapterReward(msg, chapter, model.GetReward());
            CheckChapterReward();
        }

        private void GetChapterBoxReward(MSG_ZGC_CHAPTER_REWARD msg, Chapter chapter, int id)
        {
            StoryRewardModel model = ChapterLibrary.GetStoryRewardModel(id);
            if (model == null)
            {
                Logger.Log.Warn($"player {Uid} get chapter box reward {id} failed: not find reward");
                msg.ErrorCode = (int)ErrorCode.ChapterNotExitTheReward;
                Write(msg);
                return;
            }

            if (!CheckTaskFinished(model.TaskId))
            {
                Logger.Log.Warn($"player {Uid} get chapter box reward {id} failed: task not finished");
                msg.ErrorCode = (int)ErrorCode.EarlierTaskNotFinished;
                Write(msg);
                return;
            }

            if (chapter.BoxRewardList.Contains(id))
            {
                Logger.Log.Warn($"player {Uid} get chapter box reward {id} failed: reward had gived");
                msg.ErrorCode = (int)ErrorCode.ChapterRewardHadGived;
                Write(msg);
                return;
            }

            chapter.AddBoxReward(id);
            ChapterReward(msg, chapter, model.GetReward());
            CheckChapterReward();
        }

        private void ChapterReward(MSG_ZGC_CHAPTER_REWARD msg, Chapter chapter, string rewardStr)
        {
            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(rewardStr, true);

            AddRewards(manager, ObtainWay.Chapter);
            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.ErrorCode = (int)ErrorCode.Success;
            msg.BoxReward.AddRange(chapter.BoxRewardList);
            msg.StoryReward.AddRange(chapter.StoryRewardList);
            Write(msg);
        }

        public void ChapterSweep(int dungeonId, int count)
        {
            MSG_ZGC_CHAPTER_SWEEP msg = new MSG_ZGC_CHAPTER_SWEEP();
            StoryLineModel model = ChapterLibrary.GetStoryLineModelByDungeonId(dungeonId);
            if (model == null || count <= 0)
            {
                Logger.Log.Warn($"player {Uid} chapter sweep {dungeonId} failed: not find story line");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(dungeonId);
            if (dungeonModel == null)
            {
                Logger.Log.Warn($"player {Uid} chapter sweep {dungeonId} failed: not find dungeon");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!ChapterManager.HadPassedDungeon(dungeonId))
            {
                Logger.Log.Warn($"player {Uid} chapter sweep {dungeonId} failed: not pass dungeon");
                msg.ErrorCode = (int)ErrorCode.DungeonHadNotPassed;
                Write(msg);
                return;
            }

            int needCoint = dungeonModel.Power * count;
            int coin = GetCoins(CurrenciesType.spaceTimePower);
            if (coin < needCoint)
            {
                Logger.Log.Warn($"player {Uid} chapter sweep {dungeonId} failed: coin not enough, curCoin {coin} cost {needCoint}");
                msg.ErrorCode = (int)ErrorCode.SpaceTimePowerNotEnough;
                Write(msg);
                return;
            }

            RewardManager rewardManager = new RewardManager();
            //string rewardStr = dungeonModel.Data.GetString("GeneralReward");
            //rewardManager.InitBatchReward(1, rewardStr, count);
            List<ItemBasicInfo> getList = AddRewardDrop(model.Data.GetIntList("GeneralRewardId", "|"));
            rewardManager.AddReward(getList);
            rewardManager.BreakupRewards(true);

            DelCoins(CurrenciesType.spaceTimePower, needCoint, ConsumeWay.Chapter, dungeonId.ToString());
            AddRewards(rewardManager, ObtainWay.Chapter);

            rewardManager.GenerateRewardMsg(msg.Rewards);
            msg.ErrorCode = (int)ErrorCode.Success;
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
            AddRunawayActivityNumForType(RunawayAction.Fight);
        }

        public void BuyTimeSpacePower(int count)
        {
            MSG_ZGC_CHAPTER_BUY_POWER response = new MSG_ZGC_CHAPTER_BUY_POWER();

            Data buyData = DataListManager.inst.GetData("Counter", (int)CounterType.TimeSpacePowerBuyCount);
            if (buyData == null)
            {
                Logger.Log.Warn($"player {Uid} buy time spacePower failed: not find in counter xml");
                response.ErrorCode = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int currPower = GetCoins(CurrenciesType.spaceTimePower);
            int addPower = ChapterLibrary.PowerGiveCount * count;
            int maxPower = CurrenciesLibrary.GetMaxNum((int)CurrenciesType.spaceTimePower);
            if (currPower >= maxPower)
            {
                Logger.Log.Warn($"player {Uid} buy time spacePower failed: current power already max");
                response.ErrorCode = (int)ErrorCode.CurrenciesMaxLimit;
                Write(response);
                return;
            }

            //最低一次
            if (count <= 0) count = 1;

            int buyedCount = GetCounterValue(CounterType.TimeSpacePowerBuyCount);
            if (buyedCount + count > buyData.GetInt("MaxCount"))
            {
                Logger.Log.Warn($"player {Uid} buy time spacePower failed: time space power count already max");
                response.ErrorCode = (int)ErrorCode.MaxBuyCount;
                Write(response);
                return;
            }

            string costStr = buyData.GetString("Price");
            if (string.IsNullOrEmpty(costStr))
            {
                Logger.Log.Warn($"player {Uid} buy time spacePower failed: not have price");
                response.ErrorCode = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int costCoin = 0;
            for (int i = 1; i <= count; i++)
            {
                costCoin += CounterLibrary.GetBuyCountCost(costStr, buyedCount + i);
            }

            if (!CheckCoins(CurrenciesType.diamond, costCoin))
            {
                Logger.Log.Warn($"player {Uid} buy time spacePower failed: coins not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {costCoin}");
                response.ErrorCode = (int)ErrorCode.DiamondNotEnough;
                Write(response);
                return;
            }

            DelCoins(CurrenciesType.diamond, costCoin, ConsumeWay.BuyTimeSpacePower, count.ToString());

            if (currPower + addPower > maxPower)
            {
                addPower = maxPower - currPower;
            }

            AddCoins(CurrenciesType.spaceTimePower, addPower, ObtainWay.BuyTimeSpacePower);

            UpdateCounter(CounterType.TimeSpacePowerBuyCount, count);
            response.ErrorCode = (int)ErrorCode.Success;
            Write(response);
        }

    }
}
