using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class CarnivalLibrary
    {
        private static Dictionary<int, AccumulateRechargeReward> accumulateRechargeDic = new Dictionary<int, AccumulateRechargeReward>();
        private static Dictionary<int, CarnivalMallModel> carnivalMallDic = new Dictionary<int, CarnivalMallModel>();

        public static void Init()
        {
            InitAccumulateRecharge();
            InitCarnivalMall();
        }

        private static void InitAccumulateRecharge()
        {
            Dictionary<int, AccumulateRechargeReward> accumulateRechargeDic = new Dictionary<int, AccumulateRechargeReward>();

            DataList dataList = DataListManager.inst.GetDataList("CarnivalRecharge");          
            foreach (var kv in dataList)
            {
                AccumulateRechargeReward model = new AccumulateRechargeReward(kv.Value);
                accumulateRechargeDic.Add(model.Id, model);
            }

            CarnivalLibrary.accumulateRechargeDic = accumulateRechargeDic;
        }

        private static void InitCarnivalMall()
        {
            Dictionary<int, CarnivalMallModel> carnivalMallDic = new Dictionary<int, CarnivalMallModel>();

            DataList dataList = DataListManager.inst.GetDataList("CarnivalMall");
            foreach (var kv in dataList)
            {
                CarnivalMallModel model = new CarnivalMallModel(kv.Value);
                carnivalMallDic.Add(model.Id, model);
            }

            CarnivalLibrary.carnivalMallDic = carnivalMallDic;
        }

        public static AccumulateRechargeReward GetAccumulateRechargeReward(int id)
        {
            AccumulateRechargeReward model;
            accumulateRechargeDic.TryGetValue(id, out model);
            return model;
        }

        public static CarnivalMallModel GetCarnivalMallModel(int id)
        {
            CarnivalMallModel model;
            carnivalMallDic.TryGetValue(id, out model);
            return model;
        }
    }
}
