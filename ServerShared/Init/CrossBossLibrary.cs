using DataProperty;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class CrossBossLibrary
    {
        public static Dictionary<int, List<int>> ServerDungeonList = new Dictionary<int, List<int>>();

        private static Dictionary<int, CrossBossDungeonModel> dungeonModelList = new Dictionary<int, CrossBossDungeonModel>();
        public static Dictionary<int, int> chapterList = new Dictionary<int, int>();
        private static List<CampBuildRankRewardData> rankRewards = new List<CampBuildRankRewardData>();
        //private static Dictionary<int, CrossBossPassReward> passRewardList = new Dictionary<int, CrossBossPassReward>();
        public static int CrossBossQueueCount { get; set; }
        public static float InfoUpdateTime { get; set; }

        public static float ScoreParamA { get; set; }
        public static float ScoreParamB { get; set; }
        public static float ScoreParamC { get; set; }

        public static int DefenseMap { get; set; }
        public static int DefenseEmail { get; set; }

        //public static float GoldCoinGrowthFactor { get; private set; }

        public static void Init()
        {
            InitCrossBossGroups();

            InitCrossBossdDungeon();

            InitCrossBossConfig();

            InitRankRewards();

            ////InitCrossSeasonInfos();

            //InitCrossLevelInfos();

            //InitCrossGroups();

            //InitCrossFight();

            //InitCrossFightIndex();
            ////InitRobotInfos();

            //InitCrossFinalsReward();
        }


        private static void InitCrossBossGroups()
        {
            Dictionary<int, List<int>> ServerDungeonList = new Dictionary<int, List<int>>();
            //ServerDungeonList.Clear();
            DataList dataList = DataListManager.inst.GetDataList("CrossBossGroup");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                List<int> dungeons = data.GetIntList("dungeons", "|");

                if (!ServerDungeonList.ContainsKey(data.ID))
                {
                    ServerDungeonList.Add(data.ID, dungeons);
                }
                else
                {
                    Logger.Log.Warn("InitCrossBossGroups has same Id {0}", data.ID);
                }
            }
            CrossBossLibrary.ServerDungeonList = ServerDungeonList;

        }
        public static List<int> GetDungeonIds(int serverId)
        {
            List<int> info;
            ServerDungeonList.TryGetValue(serverId, out info);
            return info;
        }

        private static void InitCrossBossdDungeon()
        {
            Dictionary<int, CrossBossDungeonModel> dungeonModelList = new Dictionary<int, CrossBossDungeonModel>();
            //dungeonModelList.Clear();
            CrossBossDungeonModel info;
            DataList dataList = DataListManager.inst.GetDataList("CrossBossDungeon");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                info = new CrossBossDungeonModel(data);
                if (!dungeonModelList.ContainsKey(info.Id))
                {
                    dungeonModelList.Add(info.Id, info);
                }
                else
                {
                    Logger.Log.Warn("InitCrossBossdDungeon has same level {0}", info.Id);
                }

                if (info.Type == CrossBossSiteType.Boss)
                {
                    chapterList[info.Chapter] = info.Id;
                }
            }
            CrossBossLibrary.dungeonModelList = dungeonModelList;
        }
        public static int GetBossDungeonId(int chapter)
        {
            int siteId;
            chapterList.TryGetValue(chapter, out siteId);
            return siteId;
        }

        public static CrossBossDungeonModel GetDungeonModel(int dungeonId)
        {
            CrossBossDungeonModel info;
            dungeonModelList.TryGetValue(dungeonId, out info);
            return info;
        }

        public static CrossBossDungeonModel GetDefenseDungon(int serverId, int dungeonId)
        {
            CrossBossDungeonModel info = null;
            List<int> list = GetDungeonIds(serverId);
            if (list != null)
            {
                CrossBossDungeonModel temp = null;
                foreach (var id in list)
                {
                    if (id != dungeonId)
                    {
                        temp = GetDungeonModel(id);
                        if (temp != null)
                        {
                            if (temp.DefenseTo.Contains(dungeonId))
                            {
                                info = temp;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return info;
        }

        private static void InitCrossBossConfig()
        {
            Data data = DataListManager.inst.GetData("CrossBossConfig", 1);
            CrossBossQueueCount = data.GetInt("CrossBossQueueCount");
            InfoUpdateTime = data.GetFloat("InfoUpdateTime");
            ScoreParamA = data.GetFloat("ScoreParamA");
            ScoreParamB = data.GetFloat("ScoreParamB");
            ScoreParamC = data.GetFloat("ScoreParamC");
            DefenseMap = data.GetInt("DefenseMap");
            DefenseEmail = data.GetInt("DefenseEmail");
        }


        public static void InitRankRewards()
        {
            List<CampBuildRankRewardData> rankRewards = new List<CampBuildRankRewardData>();
            //rankRewards.Clear();
            DataList dataList = DataListManager.inst.GetDataList("CrossBossRankReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int id = data.ID;
                CampBuildRankRewardData itemInfo = new CampBuildRankRewardData();

                string phasesString = data.GetString("ActivityNumber");
                string[] phaseList = phasesString.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (phaseList.Length > 1)
                {
                    itemInfo.PhaseMin = phaseList[0].ToInt();
                    itemInfo.PhaseMax = phaseList[1].ToInt();
                }
                else
                {
                    itemInfo.PhaseMin = phaseList[0].ToInt();
                    itemInfo.PhaseMax = phaseList[0].ToInt();
                }

                string rankString = data.GetString("Rank");
                string[] rankArr = rankString.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (rankArr.Length > 1)
                {
                    itemInfo.RankMin = rankArr[0].ToInt();
                    itemInfo.RankMax = rankArr[1].ToInt();
                }
                else
                {
                    itemInfo.RankMin = rankArr[0].ToInt();
                    itemInfo.RankMax = rankArr[0].ToInt();
                }

                itemInfo.Rewards = data.GetString("Rewards");
                itemInfo.EmailId = data.GetInt("EmailId");

                rankRewards.Add(itemInfo);
            }
            CrossBossLibrary.rankRewards = rankRewards;
        }

        public static CampBuildRankRewardData GetRankRewardInfo(int phase, int rank)
        {
            foreach (var item in rankRewards)
            {
                if (item.PhaseMin <= phase && phase <= item.PhaseMax)
                {
                    if (item.RankMin <= rank && rank <= item.RankMax)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        //private static void InitCrossBossPassReward()
        //{
        //    passRewardList.Clear();

        //    DataList dataList = DataListManager.inst.GetDataList("CrossBossReward");
        //    foreach (var item in dataList)
        //    {
        //        CrossBossPassReward reward = new CrossBossPassReward(item.Value);
        //        passRewardList.Add(reward.Id, reward);
        //    }
        //}

        //public static CrossBossPassReward GetCrossBossPassReward(int id)
        //{
        //    CrossBossPassReward reward;
        //    passRewardList.TryGetValue(id, out reward);
        //    return reward;
        //}

        //private static void InitCrossConfig()
        //{
        //    timingList.Clear();
        //    DataList dataList = DataListManager.inst.GetDataList("CrossConfig");
        //    foreach (var item in dataList)
        //    {
        //        Data data = item.Value;

        //        //RankMax = data.GetInt("RankMax");
        //        //RankPerPage = data.GetInt("RankPerPage");
        //        //RankRefreshTime = data.GetInt("RankRefreshTime");
        //        //InfoRefreshTime = data.GetInt("InfoRefreshTime");

        //        ShowRefreshTime = data.GetInt("ShowRefreshTime");
        //        MapId = data.GetInt("MapId");

        //        WinStar = data.GetInt("WinStar");
        //        LoseStar = data.GetInt("LoseStar");
        //        ChallengeWinReward = data.GetString("ChallengeWinReward");
        //        ChallengeLoseReward = data.GetString("ChallengeLoseReward");
        //        ServerReward = data.GetString("ServerReward");
        //        FightPlayerCount = data.GetInt("FightPlayerCount");
        //        GuessingOnhookReward = data.GetInt("GuessingOnhookReward");
        //        GuessingOnhookRatio = data.GetFloat("GuessingOnhookRatio");

        //        WinStreakStar = data.GetInt("WinStreakStar");
        //        WinStreakNum = data.GetInt("WinStreakNum");

        //        PrepareTimeSpan = TimeSpan.Parse(data.GetString("PrepareTimeSpan"));
        //        BattleTimeSpan = TimeSpan.Parse(data.GetString("BattleTimeSpan"));
        //        GuessingTimeSpan = TimeSpan.Parse(data.GetString("GuessingTimeSpan"));

        //        ActiveNum = data.GetInt("ActiveNum");
        //        CrossQueueCount = data.GetInt("CrossQueueCount");
        //        CrossWalkDistance = data.GetFloat("CrossWalkDistance");
        //        CrossInitPointOffsetDistance = data.GetFloat("CrossInitPointOffsetDistance");

        //        BattleEmail64 = data.GetInt("BattleEmail64");
        //        BattleEmail32 = data.GetInt("BattleEmail32");
        //        BattleEmail16 = data.GetInt("BattleEmail16");
        //        BattleEmail8 = data.GetInt("BattleEmail8");
        //        BattleEmail4 = data.GetInt("BattleEmail4");
        //        BattleEmail2 = data.GetInt("BattleEmail2");
        //        PrepareEmail = data.GetInt("PrepareEmail");
        //        GuessingEmail = data.GetInt("GuessingEmail");

        //        AddWeekTimeSpan(data, CrossBattleTiming.Start.ToString(), CrossBattleTiming.Start);
        //        AddWeekTimeSpan(data, CrossBattleTiming.FinalsStart.ToString(), CrossBattleTiming.FinalsStart);
        //        AddWeekTimeSpan(data, CrossBattleTiming.FinalsReward.ToString(), CrossBattleTiming.FinalsReward);
        //        AddWeekTimeSpan(data, CrossBattleTiming.End.ToString(), CrossBattleTiming.End);


        //        AddWeekTimeSpan(data, "BattleTime1", CrossBattleTiming.ShowTime1);
        //        AddWeekTimeSpan(data, "BattleTime2", CrossBattleTiming.ShowTime2);
        //        AddWeekTimeSpan(data, "BattleTime3", CrossBattleTiming.ShowTime3);
        //        AddWeekTimeSpan(data, "BattleTime4", CrossBattleTiming.ShowTime4);
        //        AddWeekTimeSpan(data, "BattleTime5", CrossBattleTiming.ShowTime5);
        //        AddWeekTimeSpan(data, "BattleTime6", CrossBattleTiming.ShowTime6);

        //        AddWeekTimeSpan(CrossBattleTiming.GuessingTime, CrossBattleTiming.FinalsStart, -GuessingTimeSpan);

        //        AddWeekTimeSpan(CrossBattleTiming.PrepareTime1, CrossBattleTiming.ShowTime1, PrepareTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.PrepareTime2, CrossBattleTiming.ShowTime2, PrepareTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.PrepareTime3, CrossBattleTiming.ShowTime3, PrepareTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.PrepareTime4, CrossBattleTiming.ShowTime4, PrepareTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.PrepareTime5, CrossBattleTiming.ShowTime5, PrepareTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.PrepareTime6, CrossBattleTiming.ShowTime6, PrepareTimeSpan);

        //        AddWeekTimeSpan(CrossBattleTiming.BattleTime1, CrossBattleTiming.ShowTime1, BattleTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.BattleTime2, CrossBattleTiming.ShowTime2, BattleTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.BattleTime3, CrossBattleTiming.ShowTime3, BattleTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.BattleTime4, CrossBattleTiming.ShowTime4, BattleTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.BattleTime5, CrossBattleTiming.ShowTime5, BattleTimeSpan);
        //        AddWeekTimeSpan(CrossBattleTiming.BattleTime6, CrossBattleTiming.ShowTime6, BattleTimeSpan);


        //        var newDic = from n in timingList orderby n.Value.AddWeek ascending, n.Value.WDay ascending, n.Value.TSpan ascending select n;
        //        timingList = newDic.ToDictionary(k => k.Key, v => v.Value);
        //        //foreach (CrossBattleTiming timeingKey in Enum.GetValues(typeof(CrossBattleTiming)))
        //        //{
        //        //    AddWeekTimeSpan(data, timeingKey);
        //        //}
        //    }
        //}

        //private static void AddWeekTimeSpan(CrossBattleTiming key, CrossBattleTiming timeKey, TimeSpan addTime)
        //{
        //    WeekTimeSpan weekimeSpan = GetWeekTime(timeKey);
        //    if (weekimeSpan != null)
        //    {
        //        WeekTimeSpan newWeek = new WeekTimeSpan((int)weekimeSpan.Week, weekimeSpan.TSpan - addTime);
        //        newWeek.AddWeek = weekimeSpan.AddWeek;
        //        timingList[key] = newWeek;
        //    }
        //}

        ////private static void InitCrossSeasonInfos()
        ////{
        ////    crossSeasonInfos.Clear();
        ////    CrossSeasonInfo info;
        ////    DataList dataList = DataListManager.inst.GetDataList("CrossSeason");
        ////    foreach (var item in dataList)
        ////    {

        ////        Data data = item.Value;
        ////        info = new CrossSeasonInfo(data);
        ////        if (!crossSeasonInfos.ContainsKey(info.Id))
        ////        {
        ////            crossSeasonInfos.Add(info.Id, info);
        ////        }
        ////        else
        ////        {
        ////            Logger.Log.Warn("InitCrossSeasonInfos has same Id {0}", info.Id);
        ////        }
        ////    }
        ////}

        //private static void InitCrossLevelInfos()
        //{
        //    crossLevelInfos.Clear();
        //    CrossLevelInfo info;
        //    DataList dataList = DataListManager.inst.GetDataList("CrossLevel");
        //    foreach (var item in dataList)
        //    {
        //        Data data = item.Value;
        //        info = new CrossLevelInfo(data);
        //        if (!crossLevelInfos.ContainsKey(info.Level))
        //        {
        //            crossLevelInfos.Add(info.Level, info);
        //        }
        //        else
        //        {
        //            Logger.Log.Warn("InitCrossLevelInfos has same level {0}", info.Level);
        //        }
        //    }
        //}

        //private static void InitCrossGroups()
        //{
        //    crossGroups.Clear();
        //    GroupList.Clear();
        //    List<int> list = new List<int>();
        //    DataList dataList = DataListManager.inst.GetDataList("ServerList");
        //    foreach (var item in dataList)
        //    {

        //        Data data = item.Value;
        //        int group = data.GetInt("crossGroup");
        //        if (!crossGroups.ContainsKey(data.ID))
        //        {
        //            crossGroups.Add(data.ID, group);
        //        }
        //        else
        //        {
        //            Logger.Log.Warn("InitCrossGroups has same Id {0}", data.ID);
        //        }

        //        if (GroupList.TryGetValue(group, out list))
        //        {
        //            list.Add(data.ID);
        //        }
        //        else
        //        {
        //            list = new List<int>();
        //            list.Add(data.ID);
        //            GroupList.Add(group, list);
        //        }
        //    }
        //}

        //private static void InitCrossFight()
        //{
        //    FightGroupList.Clear();
        //    List<CrossFightGroup> list;
        //    CrossFightGroup info;
        //    DataList dataList = DataListManager.inst.GetDataList("CrossFightGroup");
        //    foreach (var item in dataList)
        //    {
        //        Data data = item.Value;
        //        info = new CrossFightGroup(data);
        //        if (FightGroupList.TryGetValue(info.Team, out list))
        //        {
        //            list.Add(info);
        //        }
        //        else
        //        {
        //            list = new List<CrossFightGroup>();
        //            list.Add(info);
        //            FightGroupList.Add(info.Team, list);
        //        }
        //    }
        //}

        //private static void InitCrossFightIndex()
        //{
        //    FightIndexList.Clear();
        //    fightList.Clear();
        //    //crossFightList.Clear();
        //    Dictionary<int, List<int>> dic;
        //    List<int> list;
        //    CrossFightIndex info;
        //    DataList dataList = DataListManager.inst.GetDataList("CrossFightIndex");
        //    foreach (var item in dataList)
        //    {
        //        Data data = item.Value;
        //        info = new CrossFightIndex(data);
        //        if (!FightIndexList.ContainsKey(info.Rank))
        //        {
        //            FightIndexList.Add(info.Rank, info);
        //        }
        //        else
        //        {
        //            Logger.Log.Warn("InitCrossFightIndex has same id {0}", info.Rank);
        //        }


        //        foreach (CrossBattleTiming timeingKey in Enum.GetValues(typeof(CrossBattleTiming)))
        //        {
        //            switch (timeingKey)
        //            {
        //                case CrossBattleTiming.BattleTime1:
        //                case CrossBattleTiming.BattleTime4:
        //                    if (fightList.TryGetValue(timeingKey, out dic))
        //                    {
        //                        if (dic.TryGetValue(info.Fight1, out list))
        //                        {
        //                            list.Add(info.Index);
        //                        }
        //                        else
        //                        {
        //                            list = new List<int>();
        //                            list.Add(info.Index);
        //                            dic.Add(info.Fight1, list);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        dic = new Dictionary<int, List<int>>();
        //                        list = new List<int>();
        //                        list.Add(info.Index);
        //                        dic.Add(info.Fight1, list);
        //                        fightList.Add(timeingKey, dic);
        //                    }
        //                    break;
        //                case CrossBattleTiming.BattleTime2:
        //                case CrossBattleTiming.BattleTime5:
        //                    if (fightList.TryGetValue(timeingKey, out dic))
        //                    {
        //                        if (dic.TryGetValue(info.Fight2, out list))
        //                        {
        //                            list.Add(info.Index);
        //                        }
        //                        else
        //                        {
        //                            list = new List<int>();
        //                            list.Add(info.Index);
        //                            dic.Add(info.Fight2, list);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        dic = new Dictionary<int, List<int>>();
        //                        list = new List<int>();
        //                        list.Add(info.Index);
        //                        dic.Add(info.Fight2, list);
        //                        fightList.Add(timeingKey, dic);
        //                    }
        //                    break;
        //                case CrossBattleTiming.BattleTime3:
        //                case CrossBattleTiming.BattleTime6:
        //                    if (fightList.TryGetValue(timeingKey, out dic))
        //                    {
        //                        if (dic.TryGetValue(info.Fight3, out list))
        //                        {
        //                            list.Add(info.Index);
        //                        }
        //                        else
        //                        {
        //                            list = new List<int>();
        //                            list.Add(info.Index);
        //                            dic.Add(info.Fight3, list);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        dic = new Dictionary<int, List<int>>();
        //                        list = new List<int>();
        //                        list.Add(info.Index);
        //                        dic.Add(info.Fight3, list);
        //                        fightList.Add(timeingKey, dic);
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }

        //        }
        //    }
        //}

        //private static void InitCrossFinalsReward()
        //{
        //    finalsRewards.Clear();
        //    RankRewardInfo info;
        //    DataList dataList = DataListManager.inst.GetDataList("CrossFinalsReward");
        //    foreach (var item in dataList)
        //    {

        //        Data data = item.Value;
        //        info = new RankRewardInfo();
        //        info.Id = data.ID;
        //        info.EmailId = data.GetInt("EmailId");
        //        info.RankMin = data.GetInt("RankMin");
        //        info.RankMax = data.GetInt("RankMax");
        //        info.Rewards = data.GetString("Rewards");

        //        if (!finalsRewards.ContainsKey(info.Id))
        //        {
        //            finalsRewards.Add(info.Id, info);
        //            //fiirstRankRewardMaxId = data.ID;
        //        }
        //        else
        //        {
        //            Logger.Log.Warn("InitCrossFinalsReward has same Id {0}", info.Id);
        //        }
        //    }
        //}

        //private static void InitTimingDic()
        //{
        //    prepareTimeDic.Clear();
        //    guessingTimeDic.Clear();

        //    prepareTimeDic.Add(CrossBattleTiming.PrepareTime1, CrossBattleTiming.ShowTime1);
        //    prepareTimeDic.Add(CrossBattleTiming.PrepareTime2, CrossBattleTiming.ShowTime2);
        //    prepareTimeDic.Add(CrossBattleTiming.PrepareTime3, CrossBattleTiming.ShowTime3);
        //    prepareTimeDic.Add(CrossBattleTiming.PrepareTime4, CrossBattleTiming.ShowTime4);
        //    prepareTimeDic.Add(CrossBattleTiming.PrepareTime5, CrossBattleTiming.ShowTime5);
        //    prepareTimeDic.Add(CrossBattleTiming.PrepareTime6, CrossBattleTiming.ShowTime6);

        //    guessingTimeDic.Add(CrossBattleTiming.FinalsStart, CrossBattleTiming.BattleTime1);
        //    guessingTimeDic.Add(CrossBattleTiming.ShowTime1, CrossBattleTiming.BattleTime2);
        //    guessingTimeDic.Add(CrossBattleTiming.ShowTime2, CrossBattleTiming.BattleTime3);
        //    guessingTimeDic.Add(CrossBattleTiming.ShowTime3, CrossBattleTiming.BattleTime4);
        //    guessingTimeDic.Add(CrossBattleTiming.ShowTime4, CrossBattleTiming.BattleTime5);
        //    guessingTimeDic.Add(CrossBattleTiming.ShowTime5, CrossBattleTiming.BattleTime6);
        //}

        ////public static Dictionary<int, CrossFightInfo> GetCrossFightList()
        ////{
        ////    //Dictionary<int, int> dic = new Dictionary<int, int>();
        ////    //foreach (var item in crossFight)
        ////    //{
        ////    //    if (!dic.ContainsKey(item.Value.Player2) || !dic.ContainsKey(item.Value.Player2))
        ////    //    {
        ////    //        Logger.Log.Warn("GetCrossFightList has same id {0}", item.Key);
        ////    //        continue;
        ////    //    }
        ////    //    dic[item.Value.Player1] = item.Value.Player2;
        ////    //    dic[item.Value.Player2] = item.Value.Player1;
        ////    //}
        ////    return crossFight;
        ////}

        //public static int GetGroupId(int mainId)
        //{
        //    int group;
        //    crossGroups.TryGetValue(mainId, out group);
        //    return group;
        //}

        //public static CrossLevelInfo CheckCrossLevel(int star)
        //{
        //    CrossLevelInfo info = null;
        //    int tempStar = star;
        //    foreach (var kv in crossLevelInfos)
        //    {
        //        info = kv.Value;
        //        tempStar -= kv.Value.LimitStar;
        //        if (tempStar <= 0)
        //        {
        //            break;
        //        }
        //    }
        //    return info;
        //}

        //public static CrossLevelInfo GetCrossLevelInfo(int id)
        //{
        //    CrossLevelInfo info;
        //    crossLevelInfos.TryGetValue(id, out info);
        //    return info;
        //}

        //public static int GetGroupServerId(int mainId)
        //{
        //    int index = -1;
        //    int group = GetGroupId(mainId);
        //    List<int> list = GetGroupServers(group);
        //    if (list != null)
        //    {
        //        index = list.IndexOf(mainId) + 1;
        //    }
        //    return index;
        //}

        //public static List<int> GetGroupServers(int group)
        //{
        //    List<int> list;
        //    GroupList.TryGetValue(group, out list);
        //    return list;
        //}

        //public static CrossFightGroup GetFightGroup(int serverId, int rank)
        //{
        //    foreach (var kv in FightGroupList)
        //    {
        //        foreach (var item in kv.Value)
        //        {
        //            if (item.Server == serverId && item.Rank == rank)
        //            {
        //                return item;
        //            }
        //        }
        //    }
        //    return null;
        //}

        //public static int GetFightIndex(int rank)
        //{
        //    CrossFightIndex info = null;
        //    if (FightIndexList.TryGetValue(rank, out info))
        //    {
        //        return info.Index;
        //    }
        //    //foreach (var kv in FightIndexList)
        //    //{
        //    //    if (kv.Value.Rank == rank)
        //    //    {
        //    //        return kv.Value.Index;
        //    //    }
        //    //}
        //    return -1;
        //}

        //public static int GetFightRank(int index)
        //{
        //    foreach (var kv in FightIndexList)
        //    {
        //        if (kv.Value.Index == index)
        //        {
        //            return kv.Value.Rank;
        //        }
        //    }
        //    return -1;
        //}

        //public static Dictionary<int, List<int>> GetFightInfo(CrossBattleTiming timing)
        //{
        //    Dictionary<int, List<int>> info;
        //    fightList.TryGetValue(timing, out info);
        //    return info;
        //}
        //public static int GetFightId(CrossBattleTiming timing, int index)
        //{
        //    Dictionary<int, List<int>> info;
        //    if (fightList.TryGetValue(timing, out info))
        //    {
        //        foreach (var item in info)
        //        {
        //            if (item.Value.Contains(index))
        //            {
        //                return item.Key;
        //            }
        //        }
        //    }
        //    return 0;
        //}
        ////public static CrossSeasonInfo GetCrossSeasonInfo(int id)
        ////{
        ////    CrossSeasonInfo info;
        ////    crossSeasonInfos.TryGetValue(id, out info);
        ////    return info;
        ////}

        ////public static CrossSeasonInfo GetCrossSeasonInfoByTime(DateTime time)
        ////{
        ////    CrossSeasonInfo info = null;
        ////    foreach (var item in crossSeasonInfos)
        ////    {
        ////        info = item.Value;

        ////        if (time <= item.Value.End)
        ////        {
        ////            break;
        ////        }
        ////    }
        ////    return info;
        ////}

        ////public static int GetCrossTimeKey(DateTime time)
        ////{
        ////    int timestamp = Timestamp.GetUnixTimeStampSeconds(time);
        ////    int timeKey = int.MaxValue - timestamp + 1000000000;
        ////    return timeKey;                        //1588484447
        ////}


        //private static void AddWeekTimeSpan(Data data, string name, CrossBattleTiming key)
        //{
        //    List<string> timeList = data.GetStringList(name, "|");
        //    if (timeList.Count > 1)
        //    {
        //        int addWeek = int.Parse(timeList[0]);
        //        int week = int.Parse(timeList[1]);
        //        //if (week == 0)
        //        //{
        //        //    week = 7;
        //        //}
        //        TimeSpan tmpSpan = TimeSpan.Parse(timeList[2]);
        //        WeekTimeSpan weekimeSpan = new WeekTimeSpan(week, tmpSpan);
        //        weekimeSpan.AddWeek = addWeek * 7;
        //        timingList[key] = weekimeSpan;
        //    }
        //}

        //public static WeekTimeSpan GetWeekTime(CrossBattleTiming key)
        //{
        //    WeekTimeSpan weekimeSpan = null;
        //    timingList.TryGetValue(key, out weekimeSpan);
        //    return weekimeSpan;
        //}

        //public static bool CheckWeekTime(CrossTimeCheck key, DateTime startTime, DateTime time)
        //{
        //    switch (key)
        //    {
        //        case CrossTimeCheck.Preliminary:
        //            {
        //                WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
        //                DateTime endTime = GetTimingDate(CrossBattleTiming.FinalsStart, startTime, start);
        //                if (startTime < time && time < endTime)
        //                {
        //                    //说明在时间内
        //                    return true;
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }
        //        case CrossTimeCheck.Finals:
        //            {
        //                WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
        //                DateTime finalsStartTime = GetTimingDate(CrossBattleTiming.FinalsStart, startTime, start);
        //                DateTime endTime = GetTimingDate(CrossBattleTiming.End, startTime, start);
        //                if (finalsStartTime < time && time < endTime)
        //                {
        //                    //说明在时间内
        //                    return true;
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }
        //        case CrossTimeCheck.PrepareTime:
        //            {
        //                WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
        //                foreach (var kv in prepareTimeDic)
        //                {
        //                    DateTime beginTime = GetTimingDate(kv.Key, startTime, start);
        //                    DateTime endTime = GetTimingDate(kv.Value, startTime, start);
        //                    if (beginTime < time && time < endTime)
        //                    {
        //                        //说明在时间内
        //                        return false;
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }
        //                }
        //                return true;
        //            }
        //        case CrossTimeCheck.GuessingTime:
        //            {
        //                WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
        //                foreach (var kv in guessingTimeDic)
        //                {
        //                    DateTime beginTime = GetTimingDate(kv.Key, startTime, start);
        //                    DateTime endTime = GetTimingDate(kv.Value, startTime, start);
        //                    if (beginTime < time && time < endTime)
        //                    {
        //                        //说明在时间内
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }
        //                }
        //                break;
        //            }
        //        default:
        //            break;
        //    }
        //    return false;
        //}

        //public static CrossBattleTiming GetCurrentGuessingTime(DateTime startTime, DateTime time)
        //{
        //    WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
        //    foreach (var kv in guessingTimeDic)
        //    {
        //        DateTime beginTime = GetTimingDate(kv.Key, startTime, start);
        //        DateTime endTime = GetTimingDate(kv.Value, startTime, start);
        //        if (beginTime < time && time < endTime)
        //        {
        //            //说明在时间内
        //            return kv.Value;
        //        }
        //    }
        //    return CrossBattleTiming.Start;
        //}
        //public static CrossBattleTiming GetGuessingTime(DateTime startTime, DateTime time)
        //{
        //    CrossBattleTiming currentTime = CrossBattleTiming.Start;
        //    WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);

        //    DateTime startStop = GetTimingDate(CrossBattleTiming.FinalsStart, startTime, start);
        //    if (startTime <= time && time <= startStop)
        //    {
        //        //说明在时间内
        //        return CrossBattleTiming.Start;
        //    }

        //    DateTime endStart = GetTimingDate(CrossBattleTiming.ShowTime6, startTime, start);
        //    DateTime endStop = GetTimingDate(CrossBattleTiming.End, startTime, start);
        //    if (endStart < time && time < endStop)
        //    {
        //        //说明在时间内
        //        return CrossBattleTiming.End;
        //    }

        //    foreach (var kv in guessingTimeDic)
        //    {
        //        DateTime beginTime = GetTimingDate(kv.Key, startTime, start);
        //        DateTime endTime = GetTimingDate(kv.Value, startTime, start);
        //        if (beginTime <= time)
        //        {
        //            //说明在时间内
        //            currentTime = kv.Value;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    return currentTime;
        //}



        //private static DateTime GetTimingDate(CrossBattleTiming timing, DateTime startTime, WeekTimeSpan start)
        //{
        //    WeekTimeSpan timingStart = GetWeekTime(timing);
        //    DateTime timingStartTime = startTime.AddDays(timingStart.AddWeek);
        //    timingStartTime = timingStartTime.AddDays(timingStart.WDay - start.WDay);
        //    timingStartTime = timingStartTime + timingStart.TSpan;
        //    return timingStartTime;
        //}

        //public static bool CheckWeekTime(DateTime now, WeekTimeSpan start, WeekTimeSpan end)
        //{
        //    int nowWeek = (int)now.DayOfWeek;
        //    if (now.DayOfWeek == 0)
        //    {
        //        nowWeek = 7;
        //    }
        //    if (nowWeek < start.WDay || end.WDay < nowWeek)
        //    {
        //        //说明不在时间内
        //        return false;
        //    }
        //    else
        //    {
        //        if (start.WDay < nowWeek && nowWeek < end.WDay)
        //        {
        //            //说明在时间内
        //            return true;
        //        }
        //        else
        //        {
        //            if (start.WDay == nowWeek)
        //            {
        //                //说明在开始日期，需要判断时间
        //                if (now.TimeOfDay < start.TSpan)
        //                {
        //                    return false;
        //                }
        //            }
        //            if (end.WDay == nowWeek)
        //            {
        //                //说明在开始日期，需要判断时间
        //                if (now.TimeOfDay > end.TSpan)
        //                {
        //                    return false;
        //                }
        //            }
        //            return true;
        //        }
        //    }
        //}

        ////public static CrossBattleTiming CheckCurrentTiming(DateTime time)
        ////{
        ////    foreach (var kv in timingList)
        ////    {
        ////        if (time.DayOfWeek == kv.Value.Week)
        ////        {
        ////            if (time.TimeOfDay == kv.Value.TSpan)
        ////            {
        ////                return kv.Key;
        ////            }
        ////        }
        ////    }
        ////    return CrossBattleTiming.Start;
        ////}

        //public static CrossBattleTiming CheckNextTiming(CrossBattleTiming timing)
        //{
        //    bool getNext = false;
        //    foreach (var kv in timingList)
        //    {
        //        if (getNext)
        //        {
        //            return kv.Key;
        //        }
        //        if (timing == kv.Key)
        //        {
        //            getNext = true;
        //        }
        //    }
        //    return CrossBattleTiming.Start;
        //}

        ///// <summary>
        ///// 下一个时间
        ///// </summary>
        ///// <param name="nextTiming"></param>
        ///// <returns></returns>
        //public static DateTime GetNextTime(CrossBattleTiming lstTiming, CrossBattleTiming nextTiming, DateTime time)
        //{
        //    WeekTimeSpan lastTimeSpan = CrossBattleLibrary.GetWeekTime(lstTiming);
        //    if (lastTimeSpan == null)
        //    {
        //        return time;
        //    }
        //    WeekTimeSpan nextTimeSpan = CrossBattleLibrary.GetWeekTime(nextTiming);
        //    if (nextTimeSpan == null)
        //    {
        //        return time;
        //    }
        //    int timeWDay = (int)time.DayOfWeek;
        //    if (timeWDay == 0)
        //    {
        //        timeWDay = 7;
        //    }
        //    int addDay = nextTimeSpan.WDay - timeWDay;
        //    if (lastTimeSpan.AddWeek < nextTimeSpan.AddWeek)
        //    {
        //        addDay += nextTimeSpan.AddWeek;
        //    }
        //    if (addDay < 0)
        //    {
        //        addDay += 7;
        //    }
        //    return time.Date.AddDays(addDay) + nextTimeSpan.TSpan;
        //}
        ///// <summary>
        ///// 上一个时间
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public static DateTime GetBeforeTime(CrossBattleTiming type, DateTime time)
        //{
        //    WeekTimeSpan weekTimeSpan = CrossBattleLibrary.GetWeekTime(type);
        //    int addDay = weekTimeSpan.Week - time.DayOfWeek;
        //    if (addDay > 0)
        //    {
        //        addDay -= 7;
        //    }
        //    addDay += weekTimeSpan.AddWeek;
        //    return time.Date.AddDays(addDay) + weekTimeSpan.TSpan;
        //}

        //public static RankRewardInfo GetRankRewardInfo(int rank)
        //{
        //    foreach (var item in finalsRewards)
        //    {
        //        if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
        //        {
        //            return item.Value;
        //        }
        //    }
        //    return null;
        //}
        ////private static void InitRandomInfos()
        ////{
        ////    randomInfos.Clear();
        ////    ArenaRandomInfo info;
        ////    DataList dataList = DataListManager.inst.GetDataList("RandomChallenge");
        ////    foreach (var item in dataList)
        ////    {
        ////        Data data = item.Value;

        ////        info = new ArenaRandomInfo();
        ////        info.RankMin = data.GetInt("RankMin");
        ////        info.RankMax = data.GetInt("RankMax");
        ////        info.FristMin = data.GetInt("FristMin");
        ////        info.FristMax = data.GetInt("FristMax");
        ////        info.SecondMin = data.GetInt("SecondMin");
        ////        info.SecondMax = data.GetInt("SecondMax");
        ////        info.ThirdMin = data.GetInt("ThirdMin");
        ////        info.ThirdMax = data.GetInt("ThirdMax");
        ////        info.FourthMin = data.GetInt("FourthMin");
        ////        info.FourthMax = data.GetInt("FourthMax");

        ////        if (!randomInfos.ContainsKey(data.ID))
        ////        {
        ////            randomInfos.Add(data.ID, info);
        ////        }
        ////        else
        ////        {
        ////            Logger.Log.Warn("InitRandomInfos has same id {0}", data.ID);
        ////        }
        ////    }
        ////}






        ////public static Dictionary<int, RankRewardInfo> GetDailyRankRewards()
        ////{
        ////    return dailyRankRewards;
        ////}

        ////public static int GetWinStreakScore(int num)
        ////{
        ////    int score = 0;
        ////    foreach (var item in winStreakScore)
        ////    {
        ////        if (num >= item.Key)
        ////        {
        ////            score = item.Value;
        ////        }
        ////        else
        ////        {
        ////            break;
        ////        }
        ////    }
        ////    return score;
        ////}

        ////public static RankRewardInfo GetDailyRankRewardInfoById(int id)
        ////{
        ////    RankRewardInfo info;
        ////    dailyRankRewards.TryGetValue(id, out info);
        ////    return info;
        ////}

        ////public static RankRewardInfo GetDailyRankRewardInfo(int rank)
        ////{
        ////    foreach (var item in dailyRankRewards)
        ////    {
        ////        if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
        ////        {
        ////            return item.Value;
        ////        }
        ////    }
        ////    return null;
        ////}




        ////public static RankLevelInfo GetRankLevelInfo(int level)
        ////{
        ////    RankLevelInfo info;
        ////    rankLevelInfos.TryGetValue(level, out info);
        ////    return info;
        ////}

        //public static CrossBattleTiming GetCrossBattleTiming(CrossBattleTiming timing)
        //{
        //    switch (timing)
        //    {
        //        case CrossBattleTiming.GuessingTime:
        //        case CrossBattleTiming.PrepareTime1:
        //            return CrossBattleTiming.BattleTime1;
        //        case CrossBattleTiming.ShowTime1:
        //        case CrossBattleTiming.PrepareTime2:
        //            return CrossBattleTiming.BattleTime2;
        //        case CrossBattleTiming.ShowTime2:
        //        case CrossBattleTiming.PrepareTime3:
        //            return CrossBattleTiming.BattleTime3;
        //        case CrossBattleTiming.ShowTime3:
        //        case CrossBattleTiming.PrepareTime4:
        //            return CrossBattleTiming.BattleTime4;
        //        case CrossBattleTiming.ShowTime4:
        //        case CrossBattleTiming.PrepareTime5:
        //            return CrossBattleTiming.BattleTime5;
        //        case CrossBattleTiming.ShowTime5:
        //        case CrossBattleTiming.PrepareTime6:
        //            return CrossBattleTiming.BattleTime6;
        //        default:
        //            return timing;
        //    }
        //}

        //public static int GetBattleEmailId(CrossBattleTiming timing)
        //{
        //    switch (timing)
        //    {
        //        case CrossBattleTiming.ShowTime1:
        //        case CrossBattleTiming.BattleTime1:
        //            return BattleEmail32;
        //        case CrossBattleTiming.ShowTime2:
        //        case CrossBattleTiming.BattleTime2:
        //            return BattleEmail16;
        //        case CrossBattleTiming.ShowTime3:
        //        case CrossBattleTiming.BattleTime3:
        //            return BattleEmail8;
        //        case CrossBattleTiming.ShowTime4:
        //        case CrossBattleTiming.BattleTime4:
        //            return BattleEmail4;
        //        case CrossBattleTiming.ShowTime5:
        //        case CrossBattleTiming.BattleTime5:
        //            return BattleEmail2;
        //        case CrossBattleTiming.PrepareTime1:
        //        case CrossBattleTiming.PrepareTime2:
        //        case CrossBattleTiming.PrepareTime3:
        //        case CrossBattleTiming.PrepareTime4:
        //        case CrossBattleTiming.PrepareTime5:
        //        case CrossBattleTiming.PrepareTime6:
        //            return PrepareEmail;
        //        //case CrossBattleTiming.ShowTime6:
        //        //case CrossBattleTiming.BattleTime6:
        //        //    return CrossBattleTiming.BattleTime6;
        //        default:
        //            return 0;
        //    }
        //}
    }
}
