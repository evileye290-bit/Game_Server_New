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
    public class CarnivalBossLibrary
    {
        private static Dictionary<int, CarnivalBossReward> carnivalBossRewards = new Dictionary<int, CarnivalBossReward>();
        private static Dictionary<int, int> carnivalBossDungeons = new Dictionary<int, int>();
        private static Dictionary<int, RankRewardInfo> carnivalBossRankRewards = new Dictionary<int, RankRewardInfo>();
        public static int RankRewardCount { get; private set; }
        public static int MaxLevel { get; private set; }
        public static int MaxDegree { get; private set; }
        public static int QueueCount { get; private set; }
        public static int BossNpcId { get; private set; }
        public static string DefaultRankReward { get; private set; }

        public static void Init()
        {
            InitCarnivalBossReward();
            InitCarnivalBossDungeon();
            InitCarnivalBossRankReward();
            InitCarnivalBossConfig();
        }

        private static void InitCarnivalBossReward()
        {
            Dictionary<int, CarnivalBossReward> carnivalBossRewards = new Dictionary<int, CarnivalBossReward>();

            DataList dataList = DataListManager.inst.GetDataList("CarnivalBossReward");
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                CarnivalBossReward item = new CarnivalBossReward(data);
                carnivalBossRewards.Add(data.ID, item);
            }

            CarnivalBossLibrary.carnivalBossRewards = carnivalBossRewards;
        }

        private static void InitCarnivalBossDungeon()
        {
            Dictionary<int, int> carnivalBossDungeons = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("CarnivalBossDungeon");
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                int dungeonId = data.GetInt("DungeonId");
                carnivalBossDungeons.Add(data.ID, dungeonId);
            }

            CarnivalBossLibrary.carnivalBossDungeons = carnivalBossDungeons;
        }

        private static void InitCarnivalBossRankReward()
        {
            Dictionary<int, RankRewardInfo> carnivalBossRankRewards = new Dictionary<int, RankRewardInfo>();

            DataList dataList = DataListManager.inst.GetDataList("CarnivalBossRankReward");
            RankRewardInfo info;
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                info = new RankRewardInfo();
                info.Id = data.ID;                
                string rank = data.GetString("Rank");
                string[] rankArr = StringSplit.GetArray(":", rank);
                info.RankMin = int.Parse(rankArr[0]);
                info.RankMax = int.Parse(rankArr[1]);
                info.Rewards = data.GetString("Rewards");
                carnivalBossRankRewards.Add(data.ID, info);
            }

            CarnivalBossLibrary.carnivalBossRankRewards = carnivalBossRankRewards;
        }

        private static void InitCarnivalBossConfig()
        {
            Data data = DataListManager.inst.GetData("CarnivalBossConfig", 1);

            RankRewardCount = data.GetInt("RankRewardCount");
            MaxLevel = data.GetInt("MaxLevel");
            MaxDegree = data.GetInt("MaxDegree");
            QueueCount = data.GetInt("QueueCount");
            BossNpcId = data.GetInt("BossNpcId");
            DefaultRankReward = data.GetString("DefaultRankReward");
        }

        public static string GetLevelDegreeReward(int level, int degree)
        {
            string reward = string.Empty;
            CarnivalBossReward rewardModel;
            carnivalBossRewards.TryGetValue(level, out rewardModel);
            if (rewardModel == null)
            {
                return reward;
            }
            rewardModel.RewardList.TryGetValue(degree, out reward);
            return reward;
        }

        public static int GetDungeonByLevel(int level)
        {
            int dungeonId;
            carnivalBossDungeons.TryGetValue(level, out dungeonId);
            return dungeonId;
        }

        public static int GetBossLevelRewardCount(int level)
        {
            int count = 0;
            CarnivalBossReward reward;
            carnivalBossRewards.TryGetValue(level, out reward);
            if (reward != null)
            {
                count = reward.RewardList.Count;
            }
            return count;
        }

        public static string GetRankReward(int rank)
        {
            string reward = string.Empty;      
            foreach (var item in carnivalBossRankRewards)
            {
                if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                {
                    reward = item.Value.Rewards;
                    break;
                }
            }
            if (string.IsNullOrEmpty(reward))
            {
                reward = DefaultRankReward;
            }
            return reward;
        }
    }
}
