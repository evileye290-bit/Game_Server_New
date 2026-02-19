using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;

namespace ServerShared
{
    public class TridentLibrary
    {
        private static ListMap<int, TridentRechargeModel> periodRechargeModelList = new ListMap<int, TridentRechargeModel>();
        private static DoubleDepthMap<int, int, TridentRechargeModel> rechargeModels = new DoubleDepthMap<int, int, TridentRechargeModel>();
        private static ListMap<int, TridentRechargeModel> rechargeModelsLoop = new ListMap<int, TridentRechargeModel>();
        private static Dictionary<int, TridentShovelModel> shovelModels = new Dictionary<int, TridentShovelModel>();


        private static DoubleDepthMap<int, int, TridentTierRewardModel> tierRewardModels = new DoubleDepthMap<int, int, TridentTierRewardModel>();
        private static DoubleDepthMap<int, int, TridentTotalRewardModel> totalRewardModels = new DoubleDepthMap<int, int, TridentTotalRewardModel>();


        public static void Init()
        {
            InitTridentRecharge();
            InitTridentTierReward();
            InitTridentTotalReward();
            InitTridentShovel();
        }

        private static void InitTridentRecharge()
        {
            ListMap<int, TridentRechargeModel> periodRechargeModelList = new ListMap<int, TridentRechargeModel>();
            DoubleDepthMap<int, int, TridentRechargeModel> rechargeModels = new DoubleDepthMap<int, int, TridentRechargeModel>();
            ListMap<int, TridentRechargeModel> rechargeModelsLoop = new ListMap<int, TridentRechargeModel>();
            DataList dataList = DataListManager.inst.GetDataList("TridentRecharge");
            foreach (var data in dataList)
            {
                TridentRechargeModel model = new TridentRechargeModel(data.Value);
                if(model.GiftId == 0) continue;

                periodRechargeModelList.Add(model.Period, model);
                rechargeModels.Add(model.Period, model.GiftId, model);

                if (model.Loop)
                {
                    rechargeModelsLoop.Add(model.Period, model);
                }
            }

            TridentLibrary.rechargeModels = rechargeModels;
            TridentLibrary.rechargeModelsLoop = rechargeModelsLoop;
            TridentLibrary.periodRechargeModelList = periodRechargeModelList;
        }

        private static void InitTridentTierReward()
        {
            DoubleDepthMap<int, int, TridentTierRewardModel> tierRewardModels = new DoubleDepthMap<int, int, TridentTierRewardModel>();
            DataList dataList = DataListManager.inst.GetDataList("TridentTierReward");
            foreach (var data in dataList)
            {
                TridentTierRewardModel model = new TridentTierRewardModel(data.Value);
                tierRewardModels.Add(model.Period, model.Tier, model);
            }

            TridentLibrary.tierRewardModels = tierRewardModels;
        }

        private static void InitTridentTotalReward()
        {
            DoubleDepthMap<int, int, TridentTotalRewardModel> totalRewardModels = new DoubleDepthMap<int, int, TridentTotalRewardModel>();
            DataList dataList = DataListManager.inst.GetDataList("TridentTotalReward");
            foreach (var data in dataList)
            {
                TridentTotalRewardModel model = new TridentTotalRewardModel(data.Value);
                totalRewardModels.Add(model.Period, model.Id, model);
            }
            TridentLibrary.totalRewardModels = totalRewardModels;
        }

        private static void InitTridentShovel()
        {
            Dictionary<int, TridentShovelModel> shovelModels = new Dictionary<int, TridentShovelModel>();
            DataList dataList = DataListManager.inst.GetDataList("TridentShovel");
            foreach (var data in dataList)
            {
                TridentShovelModel model = new TridentShovelModel(data.Value);
                shovelModels.Add(model.Id, model);
            }
            TridentLibrary.shovelModels = shovelModels;
        }

        public static TridentRechargeModel GetTridentRechargeModel(int period, int giftId)
        {
            TridentRechargeModel model;
            rechargeModels.TryGetValue(period, giftId, out model);
            return model;
        }

        public static TridentTierRewardModel GetTridentTierRewardModel(int period, int tire)
        {
            if (tire > 4)
            {
                tire = 4;
            }

            TridentTierRewardModel model;
            tierRewardModels.TryGetValue(period, tire, out model);
            return model;
        }

        public static TridentTotalRewardModel GetTridentTotalRewardModel(int period, int id)
        {
            TridentTotalRewardModel model;
            totalRewardModels.TryGetValue(period, id, out model);
            return model;
        }
        public static TridentShovelModel GetTridentShovelModel(int id)
        {
            TridentShovelModel model;
            shovelModels.TryGetValue(id, out model);
            return model;
        }

        public static void RemoveRewardIdsOnRefund(List<int> rewardedList, int period, int num, int subNum)
        {
            if(rewardedList.Count == 0) return;

            Dictionary<int, TridentTotalRewardModel> rewardModels;
            if(!totalRewardModels.TryGetValue(period, out  rewardModels)) return;

            int curNum = num - subNum;
            foreach (var kv in rewardModels.OrderByDescending(x=>x.Value.PullCount))
            {
                if (kv.Value.PullCount > curNum && kv.Value.PullCount <= num)
                {
                    rewardedList.Remove(kv.Key);
                }
            }
        }

        public static int GetNextRechargeId(int period, int rechargeTotalCount)
        {
            List<TridentRechargeModel> models;
            if (!periodRechargeModelList.TryGetValue(period,  out models) || models.Count == 0)
            {
                return 0;
            }

            if (rechargeTotalCount == 0)
            {
                return models.First().GiftId;
            }

            if (rechargeTotalCount < models.Count)
            {
                return models[rechargeTotalCount].GiftId;
            }
            else
            {
                List<TridentRechargeModel> loopModels;
                if (!rechargeModelsLoop.TryGetValue(period, out loopModels))
                {
                    return 0;
                }

                int index = (rechargeTotalCount - models.Count) % loopModels.Count;
                return loopModels[index].GiftId;
            }
        }
        public static bool GetShovelProbability(int period,int shovelNum, int rechargeTotalCount)
        {
            if ((period % 3 != 0) || shovelNum >= 3)
            {
                return false;
            }

            int probability;
            
            List<TridentRechargeModel> models;
            if (!periodRechargeModelList.TryGetValue(period,  out models) || models.Count == 0)
            {
                return false;
            }

            if (rechargeTotalCount < models.Count)
            {
                probability = GetTridentShovelModel(rechargeTotalCount + 1).ServerProbability;
            }
            else
            {
                List<TridentRechargeModel> loopModels;
                if (!rechargeModelsLoop.TryGetValue(period, out loopModels))
                {
                    return false;
                }

                int index = models.Count - loopModels.Count + ((rechargeTotalCount - models.Count) % loopModels.Count);
                probability = GetTridentShovelModel(index + 1).ServerProbability;
            }

            int tmp = RAND.Range(0, 10000);
            if (tmp <= probability)
            {
                return true;
            }
            
            return false;
        }
    }
}
