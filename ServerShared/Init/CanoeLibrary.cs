using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class CanoeLibrary
    {
        private static Dictionary<int, CanoeConfig> canoeConfigs = new Dictionary<int, CanoeConfig>();
        private static Dictionary<int, List<int>> directionDic = new Dictionary<int, List<int>>();
        private static Dictionary<int, CanoeMathchReward> matchRewards = new Dictionary<int, CanoeMathchReward>();
        private static Dictionary<int, Dictionary<int, CanoeOnceDistanceReward>> onceDistanceRewards = new Dictionary<int, Dictionary<int, CanoeOnceDistanceReward>>();
        private static Dictionary<int, RankRewardInfo> canoeRankRewards = new Dictionary<int, RankRewardInfo>();

        public static void Init()
        {
            InitCanoeConfig();
            InitCanoeDirection();
            InitCanoeMatchReward();
            InitCanoeOnceDistanceReward();
            InitCanoeRankReward();
        }

        private static void InitCanoeConfig()
        {
            Dictionary<int, CanoeConfig> canoeConfigs = new Dictionary<int, CanoeConfig>();

            DataList dataList = DataListManager.inst.GetDataList("CanoeConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CanoeConfig configModel = new CanoeConfig(data);
                canoeConfigs.Add(configModel.Id, configModel);
            }
            CanoeLibrary.canoeConfigs = canoeConfigs;
        }

        private static void InitCanoeDirection()
        {
            Dictionary<int, List<int>> directionDic = new Dictionary<int, List<int>>();

            DataList dataList = DataListManager.inst.GetDataList("CanoeDirection");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int index = data.GetInt("Index");
                List<int> list;
                if (!directionDic.TryGetValue(index, out list))
                {
                    list = new List<int>();
                    directionDic.Add(index, list);
                }
                list.Add(data.ID);
            }
            CanoeLibrary.directionDic = directionDic;
        }

        private static void InitCanoeMatchReward()
        {
            Dictionary<int, CanoeMathchReward> matchRewards = new Dictionary<int, CanoeMathchReward>();

            DataList dataList = DataListManager.inst.GetDataList("CanoeMatchReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CanoeMathchReward rewardModel = new CanoeMathchReward(data);
                matchRewards.Add(rewardModel.Id, rewardModel);
            }
            CanoeLibrary.matchRewards = matchRewards;
        }

        private static void InitCanoeOnceDistanceReward()
        {
            Dictionary<int, Dictionary<int, CanoeOnceDistanceReward>> onceDistanceRewards = new Dictionary<int, Dictionary<int, CanoeOnceDistanceReward>>();

            DataList dataList = DataListManager.inst.GetDataList("CanoeOnceDistanceReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CanoeOnceDistanceReward rewardModel = new CanoeOnceDistanceReward(data);

                Dictionary<int, CanoeOnceDistanceReward> dic;
                if (!onceDistanceRewards.TryGetValue(rewardModel.RacingType, out dic))
                {
                    dic = new Dictionary<int, CanoeOnceDistanceReward>();
                    onceDistanceRewards.Add(rewardModel.RacingType, dic);
                }
                dic.Add(rewardModel.Id, rewardModel);
            }
            CanoeLibrary.onceDistanceRewards = onceDistanceRewards;
        }

        private static void InitCanoeRankReward()
        {
            Dictionary<int, RankRewardInfo> canoeRankRewards = new Dictionary<int, RankRewardInfo>();

            DataList dataList = DataListManager.inst.GetDataList("CanoeRankReward");
            
            RankRewardInfo info;
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                info = new RankRewardInfo(data);             
                canoeRankRewards.Add(info.Id, info);
            }

            CanoeLibrary.canoeRankRewards = canoeRankRewards;
        }

        public static CanoeConfig GetCanoeConfig(int type)
        {
            CanoeConfig config;
            canoeConfigs.TryGetValue(type, out config);
            return config;
        }

        public static List<int> RandDirections(int maxOperateCount)
        {
            List<int> randList = new List<int>();
            List<int> indexList;
            for (int i = 1; i <= maxOperateCount; i++)
            {
                if (directionDic.TryGetValue(i, out indexList))
                {
                    int index = NewRAND.Next(0, indexList.Count - 1);
                    randList.Add(indexList[index]);
                }
            }
            return randList;
        }

        public static List<string> GetOnceDistanceRewardByType(int type, int distance)
        {
            List<string> rewards = new List<string>();
            Dictionary<int, CanoeOnceDistanceReward> dic;
            onceDistanceRewards.TryGetValue(type, out dic);
            if (dic == null)
            {
                return rewards;
            }

            foreach (var kv in dic)
            {               
                if (distance >= kv.Value.AvailableDistance)
                {
                    rewards.Add(kv.Value.Reward);
                }
            }

            return rewards;
        }

        public static CanoeMathchReward GetCanoeMatchReward(int rewardId)
        {
            CanoeMathchReward reward;
            matchRewards.TryGetValue(rewardId, out reward);
            return reward;
        }      

        public static RankRewardInfo GetRankRewardInfo(int rank)
        {
            RankRewardInfo info = null;
            foreach (var item in canoeRankRewards)
            {
                if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                {
                    info = item.Value;
                    break;
                }
            }
            return info;
        }
    }
}
