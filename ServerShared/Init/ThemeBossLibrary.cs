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
    public static class ThemeBossLibrary
    {
        //key:period, level
        private static Dictionary<int, Dictionary<int, ThemeBossReward>> themeBossRewardList = new Dictionary<int, Dictionary<int, ThemeBossReward>>();
        //key:id
        private static Dictionary<int, ThemeBossBuffTime> themeBossBuffTimeList = new Dictionary<int, ThemeBossBuffTime>();
        //key:period, value:buffId
        private static Dictionary<int, int> themeBossBuffList = new Dictionary<int, int>();
        //key:period, level, value:dungeonId
        private static Dictionary<int, Dictionary<int, int>> themeBossDungeonList = new Dictionary<int, Dictionary<int, int>>();

        private static Dictionary<int, RankRewardInfo> themeBossRankRewards = new Dictionary<int, RankRewardInfo>();

        private static Dictionary<int, int> themeBossNpcList = new Dictionary<int, int>();

        public static int RankRewardCount { get; set; }
        public static int MaxDegree { get; set; }
        public static int ThemeQueueCount { get; set; }

        public static void Init()
        {
            InitThemeBossReward();
            InitThemeBossBuffTime();
            InitThemeBossBuff();
            InitThemeBossDungeon();
            InitThemeBossRankRewards();
            InitThemeBossConfig();
            InitThemeBossNpc();
        }

        private static void InitThemeBossReward()
        {
            Dictionary<int, Dictionary<int, ThemeBossReward>> themeBossRewardList = new Dictionary<int, Dictionary<int, ThemeBossReward>>();
            //themeBossRewardList.Clear();

            int period = 1;
            while (true)
            {
                DataList dataList = DataListManager.inst.GetDataList("ThemeBossReward_" + period);
                if (dataList != null)
                {
                    InitPeriodThemeBossReward(dataList, period, themeBossRewardList);
                }
                else
                {
                    Logger.Log.Info($"ThemeBossReward inited with max period {period - 1}");
                    break;
                }
                period++;
            }
            ThemeBossLibrary.themeBossRewardList = themeBossRewardList;
        }

        private static void InitPeriodThemeBossReward(DataList dataList, int period, Dictionary<int, Dictionary<int, ThemeBossReward>> themeBossRewardList)
        {
            Dictionary<int, string> rewardList = new Dictionary<int, string>();      
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                string degree = data.GetString("Degree");
                string reward = data.GetString("Reward");
                string[] degreeArr = StringSplit.GetArray("|", degree);
                string[] rewardArr = StringSplit.GetArray("|", reward);
                int count = Math.Min(degreeArr.Length, rewardArr.Length);
                int i = 0;
                while (i < count)
                {
                    rewardList.Add(degreeArr[i].ToInt(), rewardArr[i]);
                    i++;
                }
                ThemeBossReward item = new ThemeBossReward(data, period, rewardList);          
                Dictionary<int, ThemeBossReward> dic;
                if (themeBossRewardList.TryGetValue(item.Period, out dic))
                {
                    if (!dic.ContainsKey(data.ID))
                    {
                        dic.Add(item.Id, item);
                    }                  
                }
                else
                {
                    dic = new Dictionary<int, ThemeBossReward>();                   
                    dic.Add(item.Id, item);
                    themeBossRewardList.Add(item.Period, dic);
                }
                rewardList.Clear();
            }
        }

        private static void InitThemeBossBuffTime()
        {
            Dictionary<int, ThemeBossBuffTime> themeBossBuffTimeList = new Dictionary<int, ThemeBossBuffTime>();
            //themeBossBuffTimeList.Clear();

            DataList dataList = DataListManager.inst.GetDataList("ThemeBossBuffTime");
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                ThemeBossBuffTime item = new ThemeBossBuffTime(data);
                themeBossBuffTimeList.Add(data.ID, item);
            }
            ThemeBossLibrary.themeBossBuffTimeList = themeBossBuffTimeList;
        }

        private static void InitThemeBossBuff()
        {
            Dictionary<int, int> themeBossBuffList = new Dictionary<int, int>();
            //themeBossBuffList.Clear();

            DataList dataList = DataListManager.inst.GetDataList("ThemeBossBuff");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int buffId = data.GetInt("Buff");
                themeBossBuffList.Add(item.Key, buffId);
            }
            ThemeBossLibrary.themeBossBuffList = themeBossBuffList;
        }

        private static void InitThemeBossDungeon()
        {
            Dictionary<int, Dictionary<int, int>> themeBossDungeonList = new Dictionary<int, Dictionary<int, int>>();
            //themeBossDungeonList.Clear();

            int period = 1;
            while (true)
            {
                DataList dataList = DataListManager.inst.GetDataList("ThemeBossDungeon_" + period);
                if (dataList != null)
                {
                    InitPeriodThemeBossDungeon(dataList, period, themeBossDungeonList);
                }
                else
                {
                    Logger.Log.Info($"ThemeBossDungeon inited with max period {period - 1}");
                    break;
                }
                period++;
            }
            ThemeBossLibrary.themeBossDungeonList = themeBossDungeonList;
        }

        private static void InitPeriodThemeBossDungeon(DataList dataList, int period, Dictionary<int, Dictionary<int, int>> themeBossDungeonList)
        {
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int dungeonId = data.GetInt("DungeonId");
                Dictionary<int, int> dic;
                if (!themeBossDungeonList.TryGetValue(period, out dic))
                {
                    dic = new Dictionary<int, int>();
                    themeBossDungeonList.Add(period, dic);
                }
                dic.Add(data.ID, dungeonId);
            }
        }

        private static void InitThemeBossRankRewards()
        {
            Dictionary<int, RankRewardInfo> themeBossRankRewards = new Dictionary<int, RankRewardInfo>();
            //themeBossRankRewards.Clear();

            RankRewardInfo info;
            DataList dataList = DataListManager.inst.GetDataList("ThemeBossRankReward");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new RankRewardInfo();
                info.Id = data.ID;
                info.EmailId = data.GetInt("EmailId");
                info.RankMin = data.GetInt("RankMin");
                info.RankMax = data.GetInt("RankMax");
                info.Rewards = data.GetString("Rewards");

                if (!themeBossRankRewards.ContainsKey(info.Id))
                {
                    themeBossRankRewards.Add(info.Id, info);
                }
                else
                {
                    Logger.Log.Warn("InitThemeBossRankRewards has same Id {0}", info.Id);
                }
            }
            ThemeBossLibrary.themeBossRankRewards = themeBossRankRewards;
        }

        private static void InitThemeBossConfig()
        {
            Data data = DataListManager.inst.GetData("ThemeBossConfig", 1);
            RankRewardCount = data.GetInt("RankRewardCount");
            MaxDegree = data.GetInt("MaxDegree");
            ThemeQueueCount = data.GetInt("ThemeQueueCount");
        }

        private static void InitThemeBossNpc()
        {
            Dictionary<int, int> themeBossNpcList = new Dictionary<int, int>();         

            DataList dataList = DataListManager.inst.GetDataList("ThemeBossNpc");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int npcId = data.GetInt("NpcId");
                themeBossNpcList.Add(data.ID, npcId);
            }
            ThemeBossLibrary.themeBossNpcList = themeBossNpcList;
        }

        public static Dictionary<int, ThemeBossBuffTime> GetThemeBossBuffTimeList()
        {
            return themeBossBuffTimeList;   
        }

        public static int GetThemeBossDungeon(int period, int level)
        {
            Dictionary<int, int> dic;
            themeBossDungeonList.TryGetValue(period, out dic);
            if (dic == null)
            {
                return 0;
            }
            int dungeonId;
            dic.TryGetValue(level, out dungeonId);
            return dungeonId;
        }

        public static int GetThemeBossBuffByPeriod(int period)
        {
            int buffId;
            themeBossBuffList.TryGetValue(period, out buffId);
            return buffId;
        }

        public static Dictionary<int, string> GetThemeBossAllDegreeRewards(int period, int level, double degree)
        {
            Dictionary<int, ThemeBossReward> dic;
            themeBossRewardList.TryGetValue(period, out dic);
            if (dic == null)
            {
                return null;
            }
            ThemeBossReward reward;
            dic.TryGetValue(level, out reward);
            if (reward == null)
            {
                return null;
            }
            Dictionary<int, string> canRewardList = new Dictionary<int, string>();           
            foreach (var item in reward.RewardList)
            {
                if (item.Key <= degree)
                {
                    canRewardList.Add(item.Key ,item.Value);
                }               
            }
            return canRewardList;
        }

        public static RankRewardInfo GetThemeBossRankRewardInfo(int rank)
        {
            foreach (var item in themeBossRankRewards)
            {
                if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static int GetThemeBossRewardRewardCount(int period, int level)
        {
            Dictionary<int, ThemeBossReward> dic;
            themeBossRewardList.TryGetValue(period, out dic);
            if (dic == null)
            {
                return 0;
            }
            ThemeBossReward reward;
            dic.TryGetValue(level, out reward);
            if (reward == null)
            {
                return 0;
            }
            return reward.RewardList.Count;
        }

        public static int GetThemeBossMaxLevel(int period)
        {
            Dictionary<int, int> dic;
            themeBossDungeonList.TryGetValue(period, out dic);
            if (dic == null)
            {
                return 0;
            }
            return dic.Keys.Max();
        }

        public static int GetThemeBossNpcByPeriod(int period)
        {
            int npcId;
            themeBossNpcList.TryGetValue(period, out npcId);
            return npcId;
        }
    }
}
