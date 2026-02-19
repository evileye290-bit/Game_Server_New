using CommonUtility;
using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public static class XuanBoxLibrary
    {
        private static Dictionary<int, XuanBoxItemModel> xuanBoxItemModels = new Dictionary<int, XuanBoxItemModel>();

        //每周期总权重
        private static Dictionary<int, int> periodWeightSum = new Dictionary<int, int>();
        //周期，奖励池子
        private static ListMap<int, XuanBoxItemModel> periodModels = new ListMap<int, XuanBoxItemModel>();

        //周期lucky
        private static Dictionary<int, int> periodLuckyWeightSum = new Dictionary<int, int>();
        private static ListMap<int, XuanBoxItemModel> periodLuckyModels = new ListMap<int, XuanBoxItemModel>();

        private static Dictionary<int, XuanBoxScoreReward> scoreReward = new Dictionary<int, XuanBoxScoreReward>();

        private static List<int> luckyPool = new List<int>();

        public static int LuckyLimit { get; private set; }
        public static ItemBasicInfo CostOneItem { get; private set; }
        public static ItemBasicInfo CostTenItem { get; private set; }

        public static void Init()
        {
            InitConfig();
            InitItem();
            InitScoreReward();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("XuanBoxConfig", 1);
            LuckyLimit = data.GetInt("LuckyLimit");
            luckyPool = data.GetIntList("LuckyPool", ":");
            CostOneItem = ItemBasicInfo.Parse(data.GetString("CostOneItem"));
            CostTenItem = ItemBasicInfo.Parse(data.GetString("CostTenItem"));
        }

        private static void InitItem()
        {
            Dictionary<int, XuanBoxItemModel> boxItemModels = new Dictionary<int, XuanBoxItemModel>();

            ListMap<int, XuanBoxItemModel> piePools = new ListMap<int, XuanBoxItemModel>();
            Dictionary<int, int> periodWeightSum = new Dictionary<int, int>();

            Dictionary<int, int> periodLuckyWeightSum = new Dictionary<int, int>();
            ListMap<int, XuanBoxItemModel> periodLuckyModels = new ListMap<int, XuanBoxItemModel>();

            DataList dataList = DataListManager.inst.GetDataList("XuanBoxItem");
            foreach (var data in dataList)
            {
                XuanBoxItemModel model = new XuanBoxItemModel(data.Value);
                boxItemModels.Add(model.Id, model);

                piePools.Add(model.Period, model);
                periodWeightSum.AddValue(model.Period, model.Weight);

                if (luckyPool.Contains(model.Pool))
                {
                    periodLuckyModels.Add(model.Period, model);
                    periodLuckyWeightSum.AddValue(model.Period, model.Weight);
                }
            }

            XuanBoxLibrary.xuanBoxItemModels = boxItemModels;

            XuanBoxLibrary.periodModels = piePools;
            XuanBoxLibrary.periodWeightSum = periodWeightSum;

            XuanBoxLibrary.periodLuckyModels = periodLuckyModels;
            XuanBoxLibrary.periodLuckyWeightSum = periodLuckyWeightSum;
        }

        private static void InitScoreReward()
        {
            Dictionary<int, XuanBoxScoreReward> scoreReward = new Dictionary<int, XuanBoxScoreReward>();

            DataList dataList = DataListManager.inst.GetDataList("XuanBoxScoreReward");
            foreach (var kv in dataList)
            {
                XuanBoxScoreReward model = new XuanBoxScoreReward(kv.Value);
                scoreReward.Add(model.Id, model);
            }

            XuanBoxLibrary.scoreReward = scoreReward;
        }

        public static XuanBoxItemModel GeItemModel(int id)
        {
            XuanBoxItemModel model;
            xuanBoxItemModels.TryGetValue(id, out model);
            return model;
        }

        public static XuanBoxItemModel RandomReward(int period, bool isLucky)
        {
            var modelPools = isLucky ? periodLuckyModels : periodModels;
            var weightPool = isLucky ? periodLuckyWeightSum : periodWeightSum;

            int weightSum;
            List<XuanBoxItemModel> modelList;
            if (!modelPools.TryGetValue(period, out modelList) || !weightPool.TryGetValue(period, out weightSum))
            {
                return null;
            }

            int weight = RAND.Range(0, weightSum);
            foreach (var item in modelList)
            {
                if (weight <= item.Weight) return item;

                weight -= item.Weight;
            }
            return null;
        }

        public static XuanBoxScoreReward GetXuanBoxScoreReward(int Id)
        {
            XuanBoxScoreReward model;
            scoreReward.TryGetValue(Id, out model);
            return model;
        }
    }
}
