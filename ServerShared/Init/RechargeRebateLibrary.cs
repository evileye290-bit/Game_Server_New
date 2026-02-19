using DataProperty;
using ServerModels;
using Logger;
using System.Collections.Generic;
using System;

namespace ServerShared
{
    public static class RechargeRebateLibrary
    {
        private static Dictionary<int, RechargeRebateModel> rebateInfo = new Dictionary<int, RechargeRebateModel>();
        private static Dictionary<string, int> rebateChannelUid = new Dictionary<string, int>();
        private static HashSet<int> RebateServerList = new HashSet<int>();

        public static int RewardStartDays { get; private set; }
        public static int RewardEndDays { get; private set; }

        public static void Init(int mainId)
        {
            RebateServerList.Clear();
            
            InitConfig();

            if (!IsCurrServerRebateAvailable(mainId))
            {
                //当前服务器不需要返利，则不需要加载
                Log.Warn($"server {mainId} need not reccharge rebate !");
                return;
            }

            //该表不需要重新加载
            if (rebateInfo.Count > 0) return;

            Log.Warn($"server {mainId} need reccharge rebate !");
            InitRechargeRebateReward();
            Log.Warn($"server {mainId} init reccharge rebate finish !");
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("RechargeRebateConfig", 1);
            if (data == null) return;

            RewardStartDays = data.GetInt("RewardStartDays");
            RewardEndDays = data.GetInt("RewardEndDays");

            foreach (var i in data.GetString("RebateServerLimit").ToList('|'))
            {
                RebateServerList.Add(i);
            }
        }

        private static void InitRechargeRebateReward()
        {
            DataList dataList = DataListManager.inst.GetDataList("RechargeRebate");
            foreach (var kv in dataList)
            {
                RechargeRebateModel model = new RechargeRebateModel(kv.Value);

                rebateInfo.Add(model.Id, model);

                rebateChannelUid[model.Account] = model.Id;
            }
        }


        public static bool IsCurrServerRebateAvailable(int mainId)
        {
            //return RebateServerList.Contains(mainId);
            return true;
        }

        public static RechargeRebateModel GetRechargeRebateModel(string channelUid)
        {
            int id;
            RechargeRebateModel model = null;
            if (rebateChannelUid.TryGetValue(channelUid, out id))
            {
                rebateInfo.TryGetValue(id, out model);
            }
            return model;
        }

        public static bool IsNeedRebate(string account)
        {
            return rebateChannelUid.ContainsKey(account);
        }
    }
}
