using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class ThemeFireworkLibrary
    {
        private static Dictionary<int, ThemeFireworkConfig> configDic = new Dictionary<int, ThemeFireworkConfig>();
        //key:period, rewardType
        private static Dictionary<int, Dictionary<int, List<RandomRewardModel>>> randomRewardDic = new Dictionary<int, Dictionary<int, List<RandomRewardModel>>>();
        //key:period, rewardType
        private static Dictionary<int, Dictionary<int, List<RandomRewardModel>>> randomCouponDic = new Dictionary<int, Dictionary<int, List<RandomRewardModel>>>();
        //key:period, rewardType
        private static Dictionary<int, Dictionary<int, List<RandomRewardModel>>> randomFireworkDic = new Dictionary<int, Dictionary<int, List<RandomRewardModel>>>();
        private static Dictionary<int, ThemeFireworkAccumulateReward> accumulateRewardDic = new Dictionary<int, ThemeFireworkAccumulateReward>();
        //key:period,id
        private static Dictionary<int, Dictionary<int, RankRewardInfo>> rankRewards = new Dictionary<int, Dictionary<int, RankRewardInfo>>();

        public static void Init()
        {
            InitConfig();
            InitRandomReward();
            InitRadomCoupon();
            InitRandomFirework();
            InitAccumulateReward();
            InitRankReward();
        }

        private static void InitConfig()
        {
            Dictionary<int, ThemeFireworkConfig> configDic = new Dictionary<int, ThemeFireworkConfig>();

            DataList dataList = DataListManager.inst.GetDataList("ThemeFireworkConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                ThemeFireworkConfig configModel = new ThemeFireworkConfig(data);
                configDic.Add(configModel.Id, configModel);
            }

            ThemeFireworkLibrary.configDic = configDic;
        }

        private static void InitRandomReward()
        {
            Dictionary<int, Dictionary<int, List<RandomRewardModel>>> randomRewardDic = new Dictionary<int, Dictionary<int, List<RandomRewardModel>>>();

            DataList dataList = DataListManager.inst.GetDataList("ThemeFireworkRandReward");

            List<RandomRewardModel> list;
            Dictionary<int, List<RandomRewardModel>> dic;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                RandomRewardModel rewardModel = new RandomRewardModel(data);
                if (!randomRewardDic.TryGetValue(rewardModel.Period, out dic))
                {
                    dic = new Dictionary<int, List<RandomRewardModel>>();
                    list = new List<RandomRewardModel>() { rewardModel };
                    dic.Add(rewardModel.RewardType, list);
                    randomRewardDic.Add(rewardModel.Period, dic);
                }
                else
                {
                    if (!dic.TryGetValue(rewardModel.RewardType, out list))
                    {
                        list = new List<RandomRewardModel>();
                        dic.Add(rewardModel.RewardType, list);
                    }
                    list.Add(rewardModel);
                }
            }

            ThemeFireworkLibrary.randomRewardDic = randomRewardDic;
        }

        private static void InitRadomCoupon()
        {
            Dictionary<int, Dictionary<int, List<RandomRewardModel>>> randomCouponDic = new Dictionary<int, Dictionary<int, List<RandomRewardModel>>>();

            DataList dataList = DataListManager.inst.GetDataList("ThemeFireworkRandCoupon");

            List<RandomRewardModel> list;
            Dictionary<int, List<RandomRewardModel>> dic;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                RandomRewardModel rewardModel = new RandomRewardModel(data);
                if (!randomCouponDic.TryGetValue(rewardModel.Period, out dic))
                {
                    dic = new Dictionary<int, List<RandomRewardModel>>();
                    list = new List<RandomRewardModel>() { rewardModel };
                    dic.Add(rewardModel.RewardType, list);
                    randomCouponDic.Add(rewardModel.Period, dic);
                }
                else
                {
                    if (!dic.TryGetValue(rewardModel.RewardType, out list))
                    {
                        list = new List<RandomRewardModel>();
                        dic.Add(rewardModel.RewardType, list);
                    }
                    list.Add(rewardModel);
                }
            }

            ThemeFireworkLibrary.randomCouponDic = randomCouponDic;
        }

        private static void InitRandomFirework()
        {
            Dictionary<int, Dictionary<int, List<RandomRewardModel>>> randomFireworkDic = new Dictionary<int, Dictionary<int, List<RandomRewardModel>>>();

            DataList dataList = DataListManager.inst.GetDataList("ThemeFireworkRandFirework");

            List<RandomRewardModel> list;
            Dictionary<int, List<RandomRewardModel>> dic;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                RandomRewardModel rewardModel = new RandomRewardModel(data);
                if (!randomFireworkDic.TryGetValue(rewardModel.Period, out dic))
                {
                    dic = new Dictionary<int, List<RandomRewardModel>>();
                    list = new List<RandomRewardModel>() { rewardModel };
                    dic.Add(rewardModel.RewardType, list);
                    randomFireworkDic.Add(rewardModel.Period, dic);
                }
                else
                {
                    if (!dic.TryGetValue(rewardModel.RewardType, out list))
                    {
                        list = new List<RandomRewardModel>();
                        dic.Add(rewardModel.RewardType, list);
                    }
                    list.Add(rewardModel);
                }
            }

            ThemeFireworkLibrary.randomFireworkDic = randomFireworkDic;
        }

        private static void InitAccumulateReward()
        {
            Dictionary<int, ThemeFireworkAccumulateReward> accumulateRewardDic = new Dictionary<int, ThemeFireworkAccumulateReward>();

            DataList dataList = DataListManager.inst.GetDataList("ThemeFireworkAcumulateReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                ThemeFireworkAccumulateReward rewardModel = new ThemeFireworkAccumulateReward(data);
                accumulateRewardDic.Add(rewardModel.Id, rewardModel);
            }

            ThemeFireworkLibrary.accumulateRewardDic = accumulateRewardDic;
        }

        private static void InitRankReward()
        {
            Dictionary<int, Dictionary<int, RankRewardInfo>> rankRewards = new Dictionary<int, Dictionary<int, RankRewardInfo>>();

            DataList dataList = DataListManager.inst.GetDataList("ThemeFireworkRankReward");

            Dictionary<int, RankRewardInfo> dic;
            RankRewardInfo info;
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                info = new RankRewardInfo(data);
                if (!rankRewards.TryGetValue(info.Period, out dic))
                {
                    dic = new Dictionary<int, RankRewardInfo>();
                    rankRewards.Add(info.Period, dic);
                }
                dic.Add(info.Id, info);
            }

            ThemeFireworkLibrary.rankRewards = rankRewards;
        }


        public static ThemeFireworkConfig GetConfig(int period)
        {
            ThemeFireworkConfig config;
            configDic.TryGetValue(period, out config);
            return config;
        }

        public static List<RandomRewardModel> GetRandomRewardList(int period, int rewardType)
        {
            Dictionary<int, List<RandomRewardModel>> dic;
            List<RandomRewardModel> list;
            randomRewardDic.TryGetValue(period, out dic);
            if (dic == null)
            {
                return null;
            }
            dic.TryGetValue(rewardType, out list);
            return list;
        }

        public static List<RandomRewardModel> GetRandomCouponList(int period, int rewardType)
        {
            Dictionary<int, List<RandomRewardModel>> dic;
            List<RandomRewardModel> list;
            randomCouponDic.TryGetValue(period, out dic);
            if (dic == null)
            {
                return null;
            }
            dic.TryGetValue(rewardType, out list);
            return list;
        }

        public static List<RandomRewardModel> GetRandomFireworkList(int period, int rewardType)
        {
            Dictionary<int, List<RandomRewardModel>> dic;
            List<RandomRewardModel> list;
            randomFireworkDic.TryGetValue(period, out dic);
            if (dic == null)
            {
                return null;
            }
            dic.TryGetValue(rewardType, out list);
            return list;
        }

        public static ThemeFireworkAccumulateReward GetAccumulateReward(int rewardId)
        {
            ThemeFireworkAccumulateReward reward;
            accumulateRewardDic.TryGetValue(rewardId, out reward);
            return reward;
        }

        public static RankRewardInfo GetRankRewardInfo(int period, int rank)
        {
            RankRewardInfo info = null;
            Dictionary<int, RankRewardInfo> dic;
            if (rankRewards.TryGetValue(period, out dic))
            {
                foreach (var item in dic)
                {
                    if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                    {
                        info = item.Value;
                        break;
                    }
                }
            }
            return info;
        }

        public static int GetThemeFireworkPeriod(int itemId)
        {
            foreach (var config in configDic)
            {
                if (config.Value.HasThemeFirework(itemId))
                {
                    return config.Value.Id;
                }
            }
            return 0;
        }
    }
}
