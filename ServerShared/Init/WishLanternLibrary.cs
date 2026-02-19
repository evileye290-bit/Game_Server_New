using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using DataProperty;
using ServerModels.WishLantern;

namespace ServerShared
{
    public class WishLanternLibrary
    {
        private static ListMap<int, WishLanternItemModel> itemModels = new ListMap<int, WishLanternItemModel>();

        private static DoubleDepthMap<int, int, WishLanternBoxItemModel> boxItemModels = new DoubleDepthMap<int, int, WishLanternBoxItemModel>();
        private static Dictionary<int, WishLanternCostModel> costModels= new Dictionary<int, WishLanternCostModel>();

        private static Dictionary<int, int> itemPeriodWeight = new Dictionary<int, int>();

        private static Dictionary<int, int> resetCost = new Dictionary<int, int>();

        public static void Init()
        {
            InitConfig();
            InitLanternItem();
            InitLanternBoxItem();
            InitLanternCost();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("WishLanternConfig", 1);

            //1:100|3:200|6:1000，表示，第1、2次购买100钻、第3、4、5次购买200钻，第6次以上1000钻
            Dictionary<int, int> countCost = data.GetString("ResetCost").GetKVPairs();

            resetCost = countCost.OrderByDescending(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static void InitLanternItem()
        {
            ListMap<int, WishLanternItemModel> itemModels = new ListMap<int, WishLanternItemModel>();
            Dictionary<int, int> itemPeriodWeight = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("WishLanternItem");
            foreach (var data in dataList)
            {
                var model = new WishLanternItemModel(data.Value);

                itemModels.Add(model.Period, model);
                itemPeriodWeight.AddValue(model.Period, model.Weight);
            }

            WishLanternLibrary.itemModels = itemModels;
            WishLanternLibrary.itemPeriodWeight = itemPeriodWeight;
        }

        private static void InitLanternBoxItem()
        {
            DoubleDepthMap<int, int, WishLanternBoxItemModel> boxItemModels = new DoubleDepthMap<int, int, WishLanternBoxItemModel>();
            DataList dataList = DataListManager.inst.GetDataList("WishLanternBoxItem");
            foreach (var data in dataList)
            {
                WishLanternBoxItemModel model = new WishLanternBoxItemModel(data.Value);
                boxItemModels.Add(model.Period, model.Index, model);
            }

            WishLanternLibrary.boxItemModels = boxItemModels;
        }

        private static void InitLanternCost()
        {
            Dictionary<int, WishLanternCostModel> costModels = new Dictionary<int, WishLanternCostModel>();
            DataList dataList = DataListManager.inst.GetDataList("WishLanternCost");
            foreach (var data in dataList)
            {
                costModels.Add(data.Key, new WishLanternCostModel(data.Value));
            }

            WishLanternLibrary.costModels = costModels;
        }


        public static int GetResetCost(int curCount)
        {
            foreach (var kv in resetCost)
            {
                if (curCount >= kv.Key)
                {
                    return kv.Value;
                }
            }

            return 0;
        }

        public static int GetBoxItemCount(int period)
        {
            Dictionary<int, WishLanternBoxItemModel> models;
            if (!boxItemModels.TryGetValue(period, out models)) return 0;

            return models.Count;
        }

        public static WishLanternCostModel GetLanternCostModel(int count)
        {
            WishLanternCostModel model;
            costModels.TryGetValue(count, out model);
            return model;
        }

        public static WishLanternItemModel RandomItemModel(int period)
        {
            int weightSum;
            List<WishLanternItemModel> models;

            if (!itemPeriodWeight.TryGetValue(period, out weightSum) || 
                !itemModels.TryGetValue(period, out models)) 
                return null;

            int weight = RAND.Range(0, weightSum);
            foreach (var item in models)
            {
                if (weight <= item.Weight) return item;

                weight -= item.Weight;
            }

            return null;
        }

        public static WishLanternBoxItemModel GetLanternBoxItemModel(int period, int index)
        {
            WishLanternBoxItemModel model;
            Dictionary<int, WishLanternBoxItemModel> models;
            if (!boxItemModels.TryGetValue(period, out models) || !models.TryGetValue(index, out model)) return null;

            return model;
        }
    }
}
