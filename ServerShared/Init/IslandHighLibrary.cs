using System;
using CommonUtility;
using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;
using EnumerateUtility;

namespace ServerShared
{
    public class IslandHighLibrary
    {
        private static List<int> validControlNum = new List<int>();

        private static DoubleDepthMap<int, int, IslandHighGridModel> highGridModels = new DoubleDepthMap<int, int, IslandHighGridModel>();
        private static Dictionary<int, int> periodMaxGridIndex = new Dictionary<int, int>();

        private static int highEventWeightSum;
        private static Dictionary<int, IslandHighEventModel> highEventInfos = new Dictionary<int, IslandHighEventModel>();

        //1-6天阶段奖励
        //赛季，奖励格子数，model
        private static DoubleDepthMap<int, int, IslandHighRewardModel> highStageRewardGrid2Id= new DoubleDepthMap<int, int, IslandHighRewardModel>();
        private static DoubleDepthMap<int, int, IslandHighRewardModel> highStageReward = new DoubleDepthMap<int, int, IslandHighRewardModel>();

        //累计奖励
        private static DoubleDepthMap<int, int, IslandHighRewardModel> highTotalRewardGrid2Id = new DoubleDepthMap<int, int, IslandHighRewardModel>();
        private static DoubleDepthMap<int, int, IslandHighRewardModel> highTotalReward = new DoubleDepthMap<int, int, IslandHighRewardModel>();

        //排行奖励
        private static DoubleDepthMap<int, int, IslandHighRankRewardModel> highRankStageReward = new DoubleDepthMap<int, int, IslandHighRankRewardModel>();

        //最终排行奖励
        private static List<CampBuildRankRewardData> rankRewards = new List<CampBuildRankRewardData>();

        public static int HighItemNormal { get; private set; }
        public static int HighItemControl { get; private set; }
        public static int HighItemDouble { get; private set; }

        #region init

        public static void Init()
        {
            InitConfig();
            InitHighGrid();
            InitHighEvent();
            InitHighStageReward();
            InitHighTotalReward();
            InitHighRankReward();
            InitHighRankFinalReward();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("IslandHighConfig", 1);
            HighItemNormal = data.GetInt("HighItemNormal");
            HighItemControl = data.GetInt("HighItemControl");
            HighItemDouble = data.GetInt("HighItemDouble");

            validControlNum = StringSplit.GetInts(data.GetString("HighControlNum"));
        }

        private static void InitHighGrid()
        {
            DoubleDepthMap<int, int, IslandHighGridModel> highGridModels = new DoubleDepthMap<int, int, IslandHighGridModel>();

            DataList list = DataListManager.inst.GetDataList("IslandHighGrid");
            foreach (var kv in list)
            {
                Data data = kv.Value;
                IslandHighGridModel model = new IslandHighGridModel(data);

                highGridModels.Add(model.Period, model.Id, model);

                if (!periodMaxGridIndex.ContainsKey(model.Period) || periodMaxGridIndex[model.Period] < model.Id)
                {
                    periodMaxGridIndex[model.Period] = model.Id;
                }
            }

            IslandHighLibrary.highGridModels = highGridModels;
        }

        private static void InitHighEvent()
        {
            highEventWeightSum = 0;
            Dictionary<int, IslandHighEventModel> heightEventInfos = new Dictionary<int, IslandHighEventModel>();

            DataList list = DataListManager.inst.GetDataList("IslandHighEvent");
            foreach (var kv in list)
            {
                Data data = kv.Value;
                IslandHighEventModel model = new IslandHighEventModel(kv.Value);
                heightEventInfos.Add(model.Id, model);
                if (model.EventType != HighMainEventType.Random)
                {
                    highEventWeightSum += model.Weight;
                }
            }

            IslandHighLibrary.highEventInfos = heightEventInfos;
        }

        private static void InitHighStageReward()
        {
            DoubleDepthMap<int, int, IslandHighRewardModel> highTotalReward = new DoubleDepthMap<int, int, IslandHighRewardModel>();
            DoubleDepthMap<int, int, IslandHighRewardModel> highTotalRewardGrid2Id = new DoubleDepthMap<int, int, IslandHighRewardModel>();

            DataList list = DataListManager.inst.GetDataList("IslandHighTotalReward");
            foreach (var kv in list)
            {
                Data data = kv.Value;

                IslandHighRewardModel model = new IslandHighRewardModel(kv.Value);

                highTotalReward.Add(model.Period, model.Id, model);
                highTotalRewardGrid2Id.Add(model.Period, model.Grid, model);
            }

            IslandHighLibrary.highTotalReward = highTotalReward;
            IslandHighLibrary.highTotalRewardGrid2Id = highTotalRewardGrid2Id;
        }

        private static void InitHighTotalReward()
        {
            DoubleDepthMap<int, int, IslandHighRewardModel> highStageReward = new DoubleDepthMap<int, int, IslandHighRewardModel>();
            DoubleDepthMap<int, int, IslandHighRewardModel> highStageRewardGrid2Id = new DoubleDepthMap<int, int, IslandHighRewardModel>();

            DataList list = DataListManager.inst.GetDataList("IslandHighStageReward");
            foreach (var kv in list)
            {
                Data data = kv.Value;

                IslandHighRewardModel model = new IslandHighRewardModel(kv.Value);

                highStageReward.Add(model.Period, model.Id, model);
                highStageRewardGrid2Id.Add(model.Period, model.Grid, model);
            }

            IslandHighLibrary.highStageRewardGrid2Id = highStageRewardGrid2Id;
            IslandHighLibrary.highStageReward = highStageReward;
        }

        private static void InitHighRankReward()
        {
            DoubleDepthMap<int, int, IslandHighRankRewardModel> highRankStageReward = new DoubleDepthMap<int, int, IslandHighRankRewardModel>();
            DataList list = DataListManager.inst.GetDataList("IslandHighRankReward");
            foreach (var kv in list)
            {
                Data data = kv.Value;

                IslandHighRankRewardModel model = new IslandHighRankRewardModel(kv.Value);

                highRankStageReward.Add(model.Period, model.Id, model);
            }

            IslandHighLibrary.highRankStageReward = highRankStageReward;
        }

        private static void InitHighRankFinalReward()
        {
            List<CampBuildRankRewardData> rankRewards = new List<CampBuildRankRewardData>();
            //rankRewards.Clear();
            DataList dataList = DataListManager.inst.GetDataList("IslandHighRankFinalReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int id = data.ID;
                CampBuildRankRewardData itemInfo = new CampBuildRankRewardData();

                string rankString = data.GetString("Rank");
                string[] rankArr = rankString.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (rankArr.Length > 1)
                {
                    itemInfo.RankMin = rankArr[0].ToInt();
                    itemInfo.RankMax = rankArr[1].ToInt();
                }
                else
                {
                    itemInfo.RankMin = rankArr[0].ToInt();
                    itemInfo.RankMax = rankArr[0].ToInt();
                }

                itemInfo.Rewards = data.GetString("Rewards");
                itemInfo.EmailId = data.GetInt("EmailId");
                itemInfo.Period = data.GetInt("Period");

                rankRewards.Add(itemInfo);
            }

            IslandHighLibrary.rankRewards = rankRewards;
        }

        #endregion

        public static int GetMaxGridIndex(int period)
        {
            if (period <= 0)
            {
                period = periodMaxGridIndex.Values.FirstOrDefault();
            }

            int id;
            periodMaxGridIndex.TryGetValue(period, out id);
            return id;
        }

        public static IslandHighGridModel GetIslandHighGridModel(int period, int index)
        {
            IslandHighGridModel model;
            highGridModels.TryGetValue(period, index, out model);
            return model;
        }

        public static IslandHighEventModel HighGetEvent(int period, int id)
        {
            IslandHighEventModel model;
            highEventInfos.TryGetValue(id, out model);

            return model;
        }

        public static IslandHighEventModel HighRandomEvent()
        {
            int random = RAND.Range(1, highEventWeightSum);
            foreach (var kv in highEventInfos)
            {
                if(kv.Value.EventType == HighMainEventType.Random) continue;
                
                if (random <= kv.Value.Weight)
                {
                    return kv.Value;
                }
                else
                {
                    random -= kv.Value.Weight;
                }
            }

            return null;
        }


        public static IslandHighRewardModel GetPathHighStageRewardModel(int period, int id)
        {
            IslandHighRewardModel model;
            highStageReward.TryGetValue(period, id, out model);
            return model;
        }

        public static IslandHighRewardModel GetPathHighTotalRewardModel(int period, int grid)
        {
            IslandHighRewardModel model;
            highTotalReward.TryGetValue(period, grid, out model);
            return model;
        }

        public static bool IsLegalControlNum(int num)
        {
            return validControlNum.Contains(num);
        }


        public static Dictionary<int, IslandHighRankRewardModel> GetCurPeriodRewardModels(int period)
        {
            Dictionary<int, IslandHighRankRewardModel> rewardModels;
            highRankStageReward.TryGetValue(period, out rewardModels);
            return rewardModels;
        }

        public static IslandHighRankRewardModel GetRankStageRewardInfo(int period, int stage)
        {
            Dictionary<int, IslandHighRankRewardModel> rewardModels;
            if (highRankStageReward.TryGetValue(period, out rewardModels))
            {
                return rewardModels.Values.Where(x => x.Stage == stage).FirstOrDefault();
            }

            return null;
        }

        public static IslandHighRankRewardModel GetRankRewardInfo(int period, int rank)
        {
            Dictionary<int, IslandHighRankRewardModel> rewardModels;
            if (highRankStageReward.TryGetValue(period, out rewardModels))
            {
                foreach (var item in rewardModels)
                {
                    if (item.Value.Period == period && rank <= item.Value.Rank)
                    {
                        return item.Value;
                    }
                }
            }
        
            return null;
        }


        public static CampBuildRankRewardData GetRankFinalRewardInfo(int period, int rank)
        {
            foreach (var item in rankRewards)
            {
                if (item.Period == period && item.RankMin <= rank && rank <= item.RankMax)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
