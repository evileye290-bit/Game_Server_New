using CommonUtility;
using DataProperty;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class ArenaLibrary
    {
        private static Dictionary<int, RankLevelInfo> rankLevelInfos = new Dictionary<int, RankLevelInfo>();
        private static Dictionary<int, ArenaRandomInfo> randomInfos = new Dictionary<int, ArenaRandomInfo>();
        private static Dictionary<int, RankRewardInfo> firstRankRewards = new Dictionary<int, RankRewardInfo>();
        private static Dictionary<int, RankRewardInfo> dailyRankRewards = new Dictionary<int, RankRewardInfo>();
        
        public static CurrenciesType ResetCostType { get; set; }
        public static int ResetCostNum { get; set; }
        public static int FightCD { get; set; }
        public static int ChangeTimeCD { get; set; }
        public static int RankMax { get; set; }
        public static int RankPerPage { get; set; }
        public static int RankRefreshTime { get; set; }
        public static int ShowRefreshTime { get; set; }
        public static int InfoRefreshTime { get; set; }
        public static int MapId { get; set; }
        public static int VersusMapId { get; set; }
        public static int WinScore { get; set; }
        public static int LoseScore { get; set; }
        public static string ChallengeWinReward { get; set; }
        public static string ChallengeLoseReward { get; set; }
        private static int fiirstRankRewardMaxId { get; set; }
        public static string DailyRewardTime { get; set; }
        public static int LoseEmail { get; set; }
        public static int LoseEmailRank { get; set; }

        private static Dictionary<int, int> winStreakScore = new Dictionary<int, int>();
        public static Vec2 ChallengerWalkVec { get; private set; }
        public static Vec2 DefenderWalkVec { get; private set; }

        public static void Init()
        {

            InitArenaConfig();

            InitRankLevelInfos();

            InitRandomInfos();

            InitArenaRankFirstRewards();

            InitArenaRankDailyRewards();
        }

        private static void InitArenaConfig()
        {
            Dictionary<int, int> winStreakScore = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("ArenaConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                ResetCostType = (CurrenciesType)data.GetInt("FightTimeResetCostType");
                ResetCostNum = data.GetInt("FightTimeResetCostNum");
                FightCD = data.GetInt("FightTimeCD");
                ChangeTimeCD = data.GetInt("ChangeTimeCD");
                RankMax = data.GetInt("RankMax");
                RankPerPage = data.GetInt("RankPerPage");
                RankRefreshTime = data.GetInt("RankRefreshTime");
                InfoRefreshTime = data.GetInt("InfoRefreshTime");
                ShowRefreshTime = data.GetInt("ShowRefreshTime");
                MapId = data.GetInt("MapId");
                VersusMapId = data.GetInt("VersusMapId");

                WinScore = data.GetInt("WinScore");
                LoseScore = data.GetInt("LoseScore");
                ChallengeWinReward = data.GetString("ChallengeWinReward");
                ChallengeLoseReward = data.GetString("ChallengeLoseReward");
                DailyRewardTime = data.GetString("DailyRewardTime");
                LoseEmail = data.GetInt("LoseEmail");
                LoseEmailRank = data.GetInt("LoseEmailRank");
                winStreakScore.Clear();
                string scoreString = data.GetString("WinStreakScore");
                string[] scoreArray = CommonUtility.StringSplit.GetArray("|", scoreString);
                foreach (var scoreItem in scoreArray)
                {
                    string[] score = CommonUtility.StringSplit.GetArray(":", scoreItem);
                    int key = int.Parse(score[0]);
                    int value = int.Parse(score[1]);
                    winStreakScore[key] = value;
                }

                ChallengerWalkVec = new Vec2(0.0f, data.GetFloat("ChallengerWalkPosY"));
                DefenderWalkVec = new Vec2(0.0f, data.GetFloat("DefenderWalkPosY"));
            }
            ArenaLibrary.winStreakScore = winStreakScore;
        }

        private static void InitRankLevelInfos()
        {
            Dictionary<int, RankLevelInfo> rankLevelInfos = new Dictionary<int, RankLevelInfo>();
            //rankLevelInfos.Clear();
            RankLevelInfo info;
            DataList dataList = DataListManager.inst.GetDataList("RankLevel");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new RankLevelInfo();
                info.Level = data.ID;
                info.IntegralCurrent = data.GetInt("IntegralCurrent");
                info.IntegralNext = data.GetInt("IntegralNext");
                info.Rewards = data.GetString("Rewards");

                if (!rankLevelInfos.ContainsKey(info.Level))
                {
                    rankLevelInfos.Add(info.Level, info);
                }
                else
                {
                    Logger.Log.Warn("InitRankLevelInfos has same level {0}", info.Level);
                }
            }
            ArenaLibrary.rankLevelInfos = rankLevelInfos;
        }

        private static void InitRandomInfos()
        {
            //randomInfos.Clear();
            Dictionary<int, ArenaRandomInfo> randomInfos = new Dictionary<int, ArenaRandomInfo>();

            ArenaRandomInfo info;
            DataList dataList = DataListManager.inst.GetDataList("RandomChallenge");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                info = new ArenaRandomInfo();
                info.RankMin = data.GetInt("RankMin");
                info.RankMax = data.GetInt("RankMax");
                info.FristMin = data.GetInt("FristMin");
                info.FristMax = data.GetInt("FristMax");
                info.SecondMin = data.GetInt("SecondMin");
                info.SecondMax = data.GetInt("SecondMax");
                info.ThirdMin = data.GetInt("ThirdMin");
                info.ThirdMax = data.GetInt("ThirdMax");
                info.FourthMin = data.GetInt("FourthMin");
                info.FourthMax = data.GetInt("FourthMax");

                if (!randomInfos.ContainsKey(data.ID))
                {
                    randomInfos.Add(data.ID, info);
                }
                else
                {
                    Logger.Log.Warn("InitRandomInfos has same id {0}", data.ID);
                }
            }
            ArenaLibrary.randomInfos = randomInfos;
        }


        private static void InitArenaRankFirstRewards()
        {
            //firstRankRewards.Clear();
            Dictionary<int, RankRewardInfo> firstRankRewards = new Dictionary<int, RankRewardInfo>();

            RankRewardInfo info;
            DataList dataList = DataListManager.inst.GetDataList("ArenaRankFirstReward");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new RankRewardInfo();
                info.Id = data.ID;
                info.EmailId = data.GetInt("EmailId");
                info.RankMin = data.GetInt("RankMin");
                info.RankMax = data.GetInt("RankMax");
                info.Rewards = data.GetString("Rewards");

                if (!firstRankRewards.ContainsKey(info.Id))
                {
                    firstRankRewards.Add(info.Id, info);
                }
                else
                {
                    Logger.Log.Warn("InitArenaRankFirstRewards has same Id {0}", info.Id);
                }
            }
            ArenaLibrary.firstRankRewards = firstRankRewards;
        }

        private static void InitArenaRankDailyRewards()
        {
            //dailyRankRewards.Clear();
            Dictionary<int, RankRewardInfo> dailyRankRewards = new Dictionary<int, RankRewardInfo>();

            RankRewardInfo info;
            DataList dataList = DataListManager.inst.GetDataList("ArenaRankDailyReward");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new RankRewardInfo();
                info.Id = data.ID;
                info.EmailId = data.GetInt("EmailId");
                info.RankMin = data.GetInt("RankMin");
                info.RankMax = data.GetInt("RankMax");
                info.Rewards = data.GetString("Rewards");

                if (!dailyRankRewards.ContainsKey(info.Id))
                {
                    dailyRankRewards.Add(info.Id, info);
                    fiirstRankRewardMaxId = data.ID;
                }
                else
                {
                    Logger.Log.Warn("InitArenaRankDailyRewards has same Id {0}", info.Id);
                }
            }
            ArenaLibrary.dailyRankRewards = dailyRankRewards;
        }

        public static Dictionary<int, RankRewardInfo> GetDailyRankRewards()
        {
            return dailyRankRewards;
        }

        public static int GetWinStreakScore(int num)
        {
            int score = 0;
            foreach (var item in winStreakScore)
            {
                if (num >= item.Key)
                {
                    score = item.Value;
                }
                else
                {
                    break;
                }
            }
            return score;
        }

        public static RankRewardInfo GetDailyRankRewardInfoById(int id)
        {
            RankRewardInfo info;
            dailyRankRewards.TryGetValue(id, out info);
            return info;
        }

        public static RankRewardInfo GetDailyRankRewardInfo(int rank)
        {
            foreach (var item in dailyRankRewards)
            {
                if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static List<RankRewardInfo> GetFirstRankRewards(int oldId, int newId)
        {
            List<RankRewardInfo> list = new List<RankRewardInfo>();
            if (oldId != newId && newId> 0)
            {
                int endId = fiirstRankRewardMaxId;
                if (oldId > 0)
                {
                    endId = oldId - 1;
                }
                for (int i = newId; i <= endId; i++)
                {
                    RankRewardInfo info = GetFirstRankReward(i);
                    if (info != null)
                    {
                        list.Add(info);
                    }
                    else
                    {
                        Logger.Log.Warn("GetFirstRankReward error not find Id {0}, old {1} new {2}", i, oldId, newId);
                    }
                }
            }
            return list;
        }

        public static RankRewardInfo GetFirstRankReward(int id)
        {
            RankRewardInfo info;
            firstRankRewards.TryGetValue(id, out info);
            return info;
        }

        public static int GetFirstRankRewardId(int rank)
        {
            if (rank > 0)
            {
                foreach (var item in firstRankRewards)
                {
                    if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                    {
                        return item.Value.Id;
                    }
                }
            }
            return 0;
        }

        public static ArenaRandomInfo GetArenaRandomInfo(int rank)
        {
            foreach (var item in randomInfos)
            {
                if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static RankLevelInfo GetRankLevelInfo(int level)
        {
            RankLevelInfo info;
            rankLevelInfos.TryGetValue(level, out info);
            return info;
        }

        public static int CheckRankLevel(int score)
        {
            int level = 1;
            foreach (var item in rankLevelInfos)
            {
                if (item.Value.IntegralCurrent <= score)
                {
                    level = item.Key;
                }
                else
                {
                    break;
                }
            }
            return level;
        }
    }
}
