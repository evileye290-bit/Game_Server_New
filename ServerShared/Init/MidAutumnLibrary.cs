using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class MidAutumnLibrary
    {
        private static Dictionary<int, MidAutumnConfig> configDic = new Dictionary<int, MidAutumnConfig>();
        //key:period, rewardType
        private static Dictionary<int, Dictionary<int, List<MidAutumnRandomReward>>> randomRewardDic = new Dictionary<int, Dictionary<int, List<MidAutumnRandomReward>>>();    
        private static Dictionary<int, MidAutumnScoreReward> scoreRewardDic = new Dictionary<int, MidAutumnScoreReward>();
        //key:period,id
        private static Dictionary<int, Dictionary<int, RankRewardInfo>> rankRewards = new Dictionary<int, Dictionary<int, RankRewardInfo>>();

        public static void Init()
        {
            InitConfig();
            InitRandomReward();
            InitScoreReward();
            InitRankReward();
        }

        private static void InitConfig()
        {
            Dictionary<int, MidAutumnConfig> configDic = new Dictionary<int, MidAutumnConfig>();

            DataList dataList = DataListManager.inst.GetDataList("MidAutumnConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                MidAutumnConfig configModel = new MidAutumnConfig(data);
                configDic.Add(configModel.Id, configModel);
            }

            MidAutumnLibrary.configDic = configDic;
        }

        private static void InitRandomReward()
        {
            Dictionary<int, Dictionary<int, List<MidAutumnRandomReward>>> randomRewardDic = new Dictionary<int, Dictionary<int, List<MidAutumnRandomReward>>>();

            DataList dataList = DataListManager.inst.GetDataList("MidAutumnRandomReward");

            List<MidAutumnRandomReward> list;
            Dictionary<int, List<MidAutumnRandomReward>> dic;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                MidAutumnRandomReward rewardModel = new MidAutumnRandomReward(data);
                if (!randomRewardDic.TryGetValue(rewardModel.Period, out dic))
                {
                    dic = new Dictionary<int, List<MidAutumnRandomReward>>();
                    list = new List<MidAutumnRandomReward>() { rewardModel };
                    dic.Add(rewardModel.RewardType, list);
                    randomRewardDic.Add(rewardModel.Period, dic);
                }
                else
                {
                    if (!dic.TryGetValue(rewardModel.RewardType, out list))
                    {
                        list = new List<MidAutumnRandomReward>();
                        dic.Add(rewardModel.RewardType, list);
                    }
                    list.Add(rewardModel);//
                }
            }

            MidAutumnLibrary.randomRewardDic = randomRewardDic;
        }

        private static void InitScoreReward()
        {
            Dictionary<int, MidAutumnScoreReward> scoreRewardDic = new Dictionary<int, MidAutumnScoreReward>();

            DataList dataList = DataListManager.inst.GetDataList("MidAutumnScoreReward");     
            foreach (var item in dataList)
            {
                Data data = item.Value;
                MidAutumnScoreReward rewardModel = new MidAutumnScoreReward(data);
                scoreRewardDic.Add(rewardModel.Id, rewardModel);
            }

            MidAutumnLibrary.scoreRewardDic = scoreRewardDic;
        }

        private static void InitRankReward()
        {
            Dictionary<int, Dictionary<int, RankRewardInfo>> rankRewards = new Dictionary<int, Dictionary<int, RankRewardInfo>>();

            DataList dataList = DataListManager.inst.GetDataList("MidAutumnRankReward");

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

            MidAutumnLibrary.rankRewards = rankRewards;
        }

        public static MidAutumnConfig GetConfig(int period)
        {
            MidAutumnConfig config;
            configDic.TryGetValue(period, out config);
            return config;
        }

        public static List<MidAutumnRandomReward> GetRandomRewardList(int period, int rewardType)
        {
            Dictionary<int, List<MidAutumnRandomReward>> dic;
            List<MidAutumnRandomReward> list;
            randomRewardDic.TryGetValue(period, out dic);
            if (dic == null)
            {
                return null;
            }
            dic.TryGetValue(rewardType, out list);
            return list;
        }

        public static MidAutumnScoreReward GetScoreReward(int rewardId)
        {
            MidAutumnScoreReward reward;
            scoreRewardDic.TryGetValue(rewardId, out reward);
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
    }
}
