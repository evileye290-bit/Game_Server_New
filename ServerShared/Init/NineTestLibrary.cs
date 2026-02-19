using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class NineTestLibrary
    {
        private static Dictionary<int, NineTestConfig> configDic = new Dictionary<int, NineTestConfig>();
        //key:period, rewardType
        private static Dictionary<int, Dictionary<int, List<RandomRewardModel>>> randomRewardDic = new Dictionary<int, Dictionary<int, List<RandomRewardModel>>>();
        private static Dictionary<int, RandomRewardModel> randomRewardDic2 = new Dictionary<int, RandomRewardModel>();
        private static Dictionary<int, ScoreRewardModel> scoreRewardDic = new Dictionary<int, ScoreRewardModel>();
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
            Dictionary<int, NineTestConfig> configDic = new Dictionary<int, NineTestConfig>();

            DataList dataList = DataListManager.inst.GetDataList("NineTestConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                NineTestConfig configModel = new NineTestConfig(data);
                configDic.Add(configModel.Id, configModel);
            }

            NineTestLibrary.configDic = configDic;
        }

        private static void InitRandomReward()
        {
            Dictionary<int, Dictionary<int, List<RandomRewardModel>>> randomRewardDic = new Dictionary<int, Dictionary<int, List<RandomRewardModel>>>();
            Dictionary<int, RandomRewardModel> randomRewardDic2 = new Dictionary<int, RandomRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("NineTestRandomReward");

            List<RandomRewardModel> list;
            Dictionary<int, List<RandomRewardModel>> dic;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                RandomRewardModel rewardModel = new RandomRewardModel(data);
                randomRewardDic2.Add(rewardModel.Id, rewardModel);

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

            NineTestLibrary.randomRewardDic = randomRewardDic;
            NineTestLibrary.randomRewardDic2 = randomRewardDic2;
        }

        private static void InitScoreReward()
        {
            Dictionary<int, ScoreRewardModel> scoreRewardDic = new Dictionary<int, ScoreRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("NineTestScoreReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                ScoreRewardModel rewardModel = new ScoreRewardModel(data);
                scoreRewardDic.Add(rewardModel.Id, rewardModel);
            }

            NineTestLibrary.scoreRewardDic = scoreRewardDic;
        }

        private static void InitRankReward()
        {
            Dictionary<int, Dictionary<int, RankRewardInfo>> rankRewards = new Dictionary<int, Dictionary<int, RankRewardInfo>>();

            DataList dataList = DataListManager.inst.GetDataList("NineTestRankReward");

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

            NineTestLibrary.rankRewards = rankRewards;
        }

        public static NineTestConfig GetConfig(int period)
        {
            NineTestConfig config;
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

        public static RandomRewardModel GetRandomReward(int id)
        {
            RandomRewardModel reward;
            randomRewardDic2.TryGetValue(id, out reward);
            return reward;
        }

        public static ScoreRewardModel GetScoreReward(int rewardId)
        {
            ScoreRewardModel reward;
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
