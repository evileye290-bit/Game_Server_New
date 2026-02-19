using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public static class RouletteLibrary
    {
        private static Dictionary<int, RouletteItemModel> rouletteItemModels = new Dictionary<int, RouletteItemModel>();
        private static DoubleDepthMap<int, int, RoulettePiePool> piePools = new DoubleDepthMap<int, int, RoulettePiePool>();
        private static List<CampBuildRankRewardData> rankRewards = new List<CampBuildRankRewardData>();

        private static DoubleDepthMap<int, int, int> nextRewardId = new DoubleDepthMap<int, int, int>();
        private static Dictionary<int, RouletteScoreReward> scoreReward = new Dictionary<int, RouletteScoreReward>();

        public static ItemBasicInfo CostItemId { get; private set; }
        public static ItemBasicInfo RefreshCost { get; private set; }
        public static int MaxPoolIndex { get; private set; }


        public static void Init()
        {
            InitConfig();
            InitItem();
            InitRankRewards();
            InitScoreReward();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("RouletteConfig", 1);
            CostItemId = ItemBasicInfo.Parse(data.GetString("CostItem"));
            RefreshCost = ItemBasicInfo.Parse(data.GetString("RefreshCost"));
        }

        private static void InitItem()
        {
            Dictionary<int, RouletteItemModel> rouletteItemModels = new Dictionary<int, RouletteItemModel>();
            DoubleDepthMap<int, int, RoulettePiePool> piePools = new DoubleDepthMap<int, int, RoulettePiePool>();

            DataList dataList = DataListManager.inst.GetDataList("RouletteItem");
            foreach (var data in dataList)
            {
                RouletteItemModel model = new RouletteItemModel(data.Value);
                rouletteItemModels.Add(model.Id, model);

                RoulettePiePool pool;
                Dictionary<int, RoulettePiePool> pools;
                if (!piePools.TryGetValue(model.Period, out pools))
                {
                    pool = new RoulettePiePool(model.Period);
                    pools = new Dictionary<int, RoulettePiePool> {{model.Pool, pool}};
                    piePools[model.Period] = pools;
                }

                if (!pools.TryGetValue(model.Pool, out pool))
                {
                    pool = new RoulettePiePool(model.Pool);
                    piePools.Add(model.Period, model.Pool, pool);
                }

                MaxPoolIndex = Math.Max(MaxPoolIndex, model.Pool);
                pool.Add(model);
            }

            RouletteLibrary.rouletteItemModels = rouletteItemModels;
            RouletteLibrary.piePools = piePools;
        }

        public static void InitRankRewards()
        {
            List<CampBuildRankRewardData> rankRewards = new List<CampBuildRankRewardData>();
            DataList dataList = DataListManager.inst.GetDataList("RouletteRankReward");
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

            RouletteLibrary.rankRewards = rankRewards;
        }

        private static void InitScoreReward()
        {
            DoubleDepthMap<int, int, int> nextRewardId = new DoubleDepthMap<int, int, int>();
            Dictionary<int, RouletteScoreReward> scoreReward = new Dictionary<int, RouletteScoreReward>();

            DataList dataList = DataListManager.inst.GetDataList("RouletteScoreReward");
            int lastId = 0, lastPeriod = 0;
            foreach (var kv in dataList)
            {
                RouletteScoreReward model = new RouletteScoreReward(kv.Value);
                if (lastPeriod != model.Period)
                {
                    lastId = 0;
                    lastPeriod = model.Period;
                }

                nextRewardId.Add(model.Period, lastId, kv.Key);
                scoreReward.Add(model.Id, model);

                lastId = kv.Key;
            }

            RouletteLibrary.scoreReward = scoreReward;
            RouletteLibrary.nextRewardId = nextRewardId;
        }

        public static RouletteItemModel GeItemModel(int id)
        {
            RouletteItemModel model;
            rouletteItemModels.TryGetValue(id, out model);
            return model;
        }

        /// <summary>
        /// 随机一组奖励
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public static List<int> RandomGroupList(int period)
        {
            Dictionary<int, RoulettePiePool> pools;
            if (!piePools.TryGetValue(period, out pools)) return new List<int>();

            List<RouletteItemModel> idList = new List<RouletteItemModel>();
            pools.ForEach(x => idList.Add(x.Value.Random()));
            return idList.OrderBy(x=>x.Pool).Select(x=>x.Id).ToList();
        }

        public static bool  Check(int period, List<int> idList, List<RouletteItemModel> models)
        {
            foreach (var id in idList)
            {
                RouletteItemModel model = GeItemModel(id);
                if (model == null || model.Period != period) return false;

                models.Add(model);
            }

            return true;
        }

        public static CampBuildRankRewardData GetRankRewardInfo(int period, int rank)
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

        public static RouletteScoreReward GetNextReward(int period, int Id)
        {
            int nextId;
            if (nextRewardId.TryGetValue(period, Id, out nextId))
            {
                RouletteScoreReward model;
                if (scoreReward.TryGetValue(nextId, out model))
                {
                    return model;
                }
            }
            return null;
        }

    }
}
