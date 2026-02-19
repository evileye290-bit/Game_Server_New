using System.Collections.Generic;
using DataProperty;
using ServerModels;

namespace ServerShared
{
    public class DaysRechargeLibrary
    {
        private static Dictionary<int, DaysRechargeModel> itemModels = new Dictionary<int, DaysRechargeModel>();

        private static DoubleDepthMap<int, int, DaysRechargeModel> periodGiftModels = new DoubleDepthMap<int, int, DaysRechargeModel>();

        public static void Init()
        {
            InitConfig();
            InitLanternItem();
        }

        private static void InitConfig()
        {
            //Data data = DataListManager.inst.GetData("WishLanternConfig", 1);
        }

        private static void InitLanternItem()
        {
            Dictionary<int, DaysRechargeModel> itemModels = new Dictionary<int, DaysRechargeModel>();
            DoubleDepthMap<int, int, DaysRechargeModel> periodGiftModels = new DoubleDepthMap<int, int, DaysRechargeModel>();

            DataList dataList = DataListManager.inst.GetDataList("DaysRecharge");
            foreach (var data in dataList)
            {
                var model = new DaysRechargeModel(data.Value);

                itemModels.Add(model.Id, model);
                periodGiftModels.Add(model.Period, model.GiftId, model);
            }

            DaysRechargeLibrary.itemModels = itemModels;
            DaysRechargeLibrary.periodGiftModels = periodGiftModels;
        }

        public static DaysRechargeModel GetDaysRechargeModel(int period, int giftId)
        {
            DaysRechargeModel model;
            periodGiftModels.TryGetValue(period, giftId, out model);
            return model;
        }

    }
}
