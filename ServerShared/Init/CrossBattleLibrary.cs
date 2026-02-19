using DataProperty;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class CrossBattleLibrary
    {
        //private static Dictionary<int, CrossSeasonInfo> crossSeasonInfos = new Dictionary<int, CrossSeasonInfo>();

        private static Dictionary<int, CrossLevelInfo> crossLevelInfos = new Dictionary<int, CrossLevelInfo>();

        private static Dictionary<int, int> crossGroups = new Dictionary<int, int>();
        public static Dictionary<int, List<int>> GroupList = new Dictionary<int, List<int>>();


        public static Dictionary<int, List<CrossFightGroup>> FightGroupList = new Dictionary<int, List<CrossFightGroup>>();
        public static Dictionary<int, CrossFightIndex> FightIndexList = new Dictionary<int, CrossFightIndex>();
        private static Dictionary<CrossBattleTiming, Dictionary<int, List<int>>> fightList = new Dictionary<CrossBattleTiming, Dictionary<int, List<int>>>();
        private static Dictionary<CrossBattleTiming, WeekTimeSpan> timingList = new Dictionary<CrossBattleTiming, WeekTimeSpan>();

        private static Dictionary<int, RankRewardInfo> finalsRewards = new Dictionary<int, RankRewardInfo>();


        private static Dictionary<CrossBattleTiming, CrossBattleTiming> prepareTimeDic = new Dictionary<CrossBattleTiming, CrossBattleTiming>();
        private static Dictionary<CrossBattleTiming, CrossBattleTiming> guessingTimeDic = new Dictionary<CrossBattleTiming, CrossBattleTiming>();



        //private static Dictionary<int, int> crossFightList = new Dictionary<int, int>();
        //private static Dictionary<int, ArenaRandomInfo> randomInfos = new Dictionary<int, ArenaRandomInfo>();
        //public static Dictionary<int, CrossFightInfo> CrossFight = new Dictionary<int, CrossFightInfo>();

        //public static int RankMax { get; set; }
        //public static int RankPerPage { get; set; }
        //public static int RankRefreshTime { get; set; }
        //public static int InfoRefreshTime { get; set; }
        public static int ShowRefreshTime { get; set; }
        public static int MapId { get; set; }

        public static int WinStar { get; set; }
        public static int LoseStar { get; set; }
        public static int WinStreakStar { get; set; }
        public static int WinStreakNum { get; set; }
        public static string ChallengeWinReward { get; set; }
        public static string ChallengeLoseReward { get; set; }
        public static string ServerReward { get; set; }
        public static int GuessingOnhookReward { get; set; }
        public static float GuessingOnhookRatio { get; set; }
        public static int FightPlayerCount { get; set; }
        public static TimeSpan PrepareTimeSpan { get; set; }
        public static TimeSpan BattleTimeSpan { get; set; }
        public static TimeSpan GuessingTimeSpan { get; set; }
        public static int ActiveNum { get; set; }
        public static int CrossQueueCount { get; set; }
        public static float CrossWalkDistance { get; set; }
        public static float CrossInitPointOffsetDistance { get; set; }


        public static int BattleEmail64 { get; set; }
        public static int BattleEmail32 { get; set; }
        public static int BattleEmail16 { get; set; }
        public static int BattleEmail8 { get; set; }

        public static int BattleEmail4 { get; set; }
        public static int BattleEmail2 { get; set; }
        public static int PrepareEmail { get; set; }
        public static int GuessingEmail { get; set; }

        public static void Init()
        {
            InitTimingDic();

            InitCrossConfig();

            //InitCrossSeasonInfos();

            InitCrossLevelInfos();

            InitCrossGroups();

            InitCrossFight();

            InitCrossFightIndex();
            //InitRobotInfos();

            InitCrossFinalsReward();
        }

        private static void InitCrossConfig()
        {
            Dictionary<CrossBattleTiming, WeekTimeSpan> timingList = new Dictionary<CrossBattleTiming, WeekTimeSpan>();
            DataList dataList = DataListManager.inst.GetDataList("CrossConfig");
            foreach (var item in dataList)
            {
                Data data = item.Value;

                //RankMax = data.GetInt("RankMax");
                //RankPerPage = data.GetInt("RankPerPage");
                //RankRefreshTime = data.GetInt("RankRefreshTime");
                //InfoRefreshTime = data.GetInt("InfoRefreshTime");

                ShowRefreshTime = data.GetInt("ShowRefreshTime");
                MapId = data.GetInt("MapId");

                WinStar = data.GetInt("WinStar");
                LoseStar = data.GetInt("LoseStar");
                ChallengeWinReward = data.GetString("ChallengeWinReward");
                ChallengeLoseReward = data.GetString("ChallengeLoseReward");
                ServerReward = data.GetString("ServerReward");
                FightPlayerCount = data.GetInt("FightPlayerCount");
                GuessingOnhookReward = data.GetInt("GuessingOnhookReward");
                GuessingOnhookRatio = data.GetFloat("GuessingOnhookRatio");

                WinStreakStar = data.GetInt("WinStreakStar");
                WinStreakNum = data.GetInt("WinStreakNum");

                PrepareTimeSpan = TimeSpan.Parse(data.GetString("PrepareTimeSpan"));
                BattleTimeSpan = TimeSpan.Parse(data.GetString("BattleTimeSpan"));
                GuessingTimeSpan = TimeSpan.Parse(data.GetString("GuessingTimeSpan"));

                ActiveNum = data.GetInt("ActiveNum");
                CrossQueueCount = data.GetInt("CrossQueueCount");
                CrossWalkDistance = data.GetFloat("CrossWalkDistance");
                CrossInitPointOffsetDistance = data.GetFloat("CrossInitPointOffsetDistance");

                BattleEmail64 = data.GetInt("BattleEmail64");
                BattleEmail32 = data.GetInt("BattleEmail32");
                BattleEmail16 = data.GetInt("BattleEmail16");
                BattleEmail8 = data.GetInt("BattleEmail8");
                BattleEmail4 = data.GetInt("BattleEmail4");
                BattleEmail2 = data.GetInt("BattleEmail2");
                PrepareEmail = data.GetInt("PrepareEmail");
                GuessingEmail = data.GetInt("GuessingEmail");

                AddWeekTimeSpan(timingList, data, CrossBattleTiming.Start.ToString(), CrossBattleTiming.Start);
                AddWeekTimeSpan(timingList, data, CrossBattleTiming.FinalsStart.ToString(), CrossBattleTiming.FinalsStart);
                AddWeekTimeSpan(timingList, data, CrossBattleTiming.FinalsReward.ToString(), CrossBattleTiming.FinalsReward);
                AddWeekTimeSpan(timingList, data, CrossBattleTiming.End.ToString(), CrossBattleTiming.End);


                AddWeekTimeSpan(timingList, data, "BattleTime1", CrossBattleTiming.ShowTime1);
                AddWeekTimeSpan(timingList, data, "BattleTime2", CrossBattleTiming.ShowTime2);
                AddWeekTimeSpan(timingList, data, "BattleTime3", CrossBattleTiming.ShowTime3);
                AddWeekTimeSpan(timingList, data, "BattleTime4", CrossBattleTiming.ShowTime4);
                AddWeekTimeSpan(timingList, data, "BattleTime5", CrossBattleTiming.ShowTime5);
                AddWeekTimeSpan(timingList, data, "BattleTime6", CrossBattleTiming.ShowTime6);

                AddWeekTimeSpan(timingList, CrossBattleTiming.GuessingTime, CrossBattleTiming.FinalsStart, -GuessingTimeSpan);

                AddWeekTimeSpan(timingList, CrossBattleTiming.PrepareTime1, CrossBattleTiming.ShowTime1, PrepareTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.PrepareTime2, CrossBattleTiming.ShowTime2, PrepareTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.PrepareTime3, CrossBattleTiming.ShowTime3, PrepareTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.PrepareTime4, CrossBattleTiming.ShowTime4, PrepareTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.PrepareTime5, CrossBattleTiming.ShowTime5, PrepareTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.PrepareTime6, CrossBattleTiming.ShowTime6, PrepareTimeSpan);

                AddWeekTimeSpan(timingList, CrossBattleTiming.BattleTime1, CrossBattleTiming.ShowTime1, BattleTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.BattleTime2, CrossBattleTiming.ShowTime2, BattleTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.BattleTime3, CrossBattleTiming.ShowTime3, BattleTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.BattleTime4, CrossBattleTiming.ShowTime4, BattleTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.BattleTime5, CrossBattleTiming.ShowTime5, BattleTimeSpan);
                AddWeekTimeSpan(timingList, CrossBattleTiming.BattleTime6, CrossBattleTiming.ShowTime6, BattleTimeSpan);


                var newDic = from n in timingList orderby n.Value.AddWeek ascending, n.Value.WDay ascending, n.Value.TSpan ascending select n;
                timingList = newDic.ToDictionary(k => k.Key, v => v.Value);
            }
            CrossBattleLibrary.timingList = timingList;

        }

        private static void AddWeekTimeSpan(Dictionary<CrossBattleTiming, WeekTimeSpan> timingList, CrossBattleTiming key, CrossBattleTiming timeKey, TimeSpan addTime)
        {
            WeekTimeSpan weekimeSpan;// = GetWeekTime(timeKey);
            if (timingList.TryGetValue(timeKey, out weekimeSpan))
            {
                WeekTimeSpan newWeek = new WeekTimeSpan((int)weekimeSpan.Week, weekimeSpan.TSpan - addTime);
                newWeek.AddWeek = weekimeSpan.AddWeek;
                timingList[key] = newWeek;
            }
        }

        //private static void InitCrossSeasonInfos()
        //{
        //    crossSeasonInfos.Clear();
        //    CrossSeasonInfo info;
        //    DataList dataList = DataListManager.inst.GetDataList("CrossSeason");
        //    foreach (var item in dataList)
        //    {

        //        Data data = item.Value;
        //        info = new CrossSeasonInfo(data);
        //        if (!crossSeasonInfos.ContainsKey(info.Id))
        //        {
        //            crossSeasonInfos.Add(info.Id, info);
        //        }
        //        else
        //        {
        //            Logger.Log.Warn("InitCrossSeasonInfos has same Id {0}", info.Id);
        //        }
        //    }
        //}

        private static void InitCrossLevelInfos()
        {
            Dictionary<int, CrossLevelInfo> crossLevelInfos = new Dictionary<int, CrossLevelInfo>();
            CrossLevelInfo info;
            DataList dataList = DataListManager.inst.GetDataList("CrossLevel");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                info = new CrossLevelInfo(data);
                if (!crossLevelInfos.ContainsKey(info.Level))
                {
                    crossLevelInfos.Add(info.Level, info);
                }
                else
                {
                    Logger.Log.Warn("InitCrossLevelInfos has same level {0}", info.Level);
                }
            }

            CrossBattleLibrary.crossLevelInfos = crossLevelInfos;
        }

        private static void InitCrossGroups()
        {
            Dictionary<int, int> crossGroups = new Dictionary<int, int>();
            Dictionary<int, List<int>> GroupList = new Dictionary<int, List<int>>();
            
            List<int> list = new List<int>();
            DataList dataList = DataListManager.inst.GetDataList("ServerList");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                int group = data.GetInt("crossGroup");
                if (!crossGroups.ContainsKey(data.ID))
                {
                    crossGroups.Add(data.ID, group);
                }
                else
                {
                    Logger.Log.Warn("InitCrossGroups has same Id {0}", data.ID);
                }

                if (GroupList.TryGetValue(group, out list))
                {
                    list.Add(data.ID);
                    if (list.Count > 8)
                    {
                        Logger.Log.Error("InitCrossGroups {0} server group count is {1}", data.ID, list.Count);
                    }
                }
                else
                {
                    list = new List<int>();
                    list.Add(data.ID);
                    GroupList.Add(group, list);
                }
            }

            CrossBattleLibrary.crossGroups = crossGroups;
            CrossBattleLibrary.GroupList = GroupList;
        }

        private static void InitCrossFight()
        {
            Dictionary<int, List<CrossFightGroup>> FightGroupList = new Dictionary<int, List<CrossFightGroup>>();
            List<CrossFightGroup> list;
            CrossFightGroup info;
            DataList dataList = DataListManager.inst.GetDataList("CrossFightGroup");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                info = new CrossFightGroup(data);
                if (FightGroupList.TryGetValue(info.Team, out list))
                {
                    list.Add(info);
                }
                else
                {
                    list = new List<CrossFightGroup>();
                    list.Add(info);
                    FightGroupList.Add(info.Team, list);
                }
            }

            CrossBattleLibrary.FightGroupList = FightGroupList;
        }

        private static void InitCrossFightIndex()
        {
            Dictionary<int, CrossFightIndex> FightIndexList = new Dictionary<int, CrossFightIndex>();
            Dictionary<CrossBattleTiming, Dictionary<int, List<int>>> fightList = new Dictionary<CrossBattleTiming, Dictionary<int, List<int>>>();
            
            Dictionary<int, List<int>> dic;
            List<int> list;
            CrossFightIndex info;
            DataList dataList = DataListManager.inst.GetDataList("CrossFightIndex");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                info = new CrossFightIndex(data);
                if (!FightIndexList.ContainsKey(info.Rank))
                {
                    FightIndexList.Add(info.Rank, info);
                }
                else
                {
                    Logger.Log.Warn("InitCrossFightIndex has same id {0}", info.Rank);
                }


                foreach (CrossBattleTiming timeingKey in Enum.GetValues(typeof(CrossBattleTiming)))
                {
                    switch (timeingKey)
                    {
                        case CrossBattleTiming.BattleTime1:
                        case CrossBattleTiming.BattleTime4:
                            if (fightList.TryGetValue(timeingKey, out dic))
                            {
                                if (dic.TryGetValue(info.Fight1, out list))
                                {
                                    list.Add(info.Index);
                                }
                                else
                                {
                                    list = new List<int>();
                                    list.Add(info.Index);
                                    dic.Add(info.Fight1, list);
                                }
                            }
                            else
                            {
                                dic = new Dictionary<int, List<int>>();
                                list = new List<int>();
                                list.Add(info.Index);
                                dic.Add(info.Fight1, list);
                                fightList.Add(timeingKey, dic);
                            }
                            break;
                        case CrossBattleTiming.BattleTime2:
                        case CrossBattleTiming.BattleTime5:
                            if (fightList.TryGetValue(timeingKey, out dic))
                            {
                                if (dic.TryGetValue(info.Fight2, out list))
                                {
                                    list.Add(info.Index);
                                }
                                else
                                {
                                    list = new List<int>();
                                    list.Add(info.Index);
                                    dic.Add(info.Fight2, list);
                                }
                            }
                            else
                            {
                                dic = new Dictionary<int, List<int>>();
                                list = new List<int>();
                                list.Add(info.Index);
                                dic.Add(info.Fight2, list);
                                fightList.Add(timeingKey, dic);
                            }
                            break;
                        case CrossBattleTiming.BattleTime3:
                        case CrossBattleTiming.BattleTime6:
                            if (fightList.TryGetValue(timeingKey, out dic))
                            {
                                if (dic.TryGetValue(info.Fight3, out list))
                                {
                                    list.Add(info.Index);
                                }
                                else
                                {
                                    list = new List<int>();
                                    list.Add(info.Index);
                                    dic.Add(info.Fight3, list);
                                }
                            }
                            else
                            {
                                dic = new Dictionary<int, List<int>>();
                                list = new List<int>();
                                list.Add(info.Index);
                                dic.Add(info.Fight3, list);
                                fightList.Add(timeingKey, dic);
                            }
                            break;
                        default:
                            break;
                    }

                }
            }

            CrossBattleLibrary.FightIndexList = FightIndexList;
            CrossBattleLibrary.fightList = fightList;
        }

        private static void InitCrossFinalsReward()
        {
            Dictionary<int, RankRewardInfo> finalsRewards = new Dictionary<int, RankRewardInfo>();
            RankRewardInfo info;
            DataList dataList = DataListManager.inst.GetDataList("CrossFinalsReward");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new RankRewardInfo();
                info.Id = data.ID;
                info.EmailId = data.GetInt("EmailId");
                info.RankMin = data.GetInt("RankMin");
                info.RankMax = data.GetInt("RankMax");
                info.Rewards = data.GetString("Rewards");

                if (!finalsRewards.ContainsKey(info.Id))
                {
                    finalsRewards.Add(info.Id, info);
                    //fiirstRankRewardMaxId = data.ID;
                }
                else
                {
                    Logger.Log.Warn("InitCrossFinalsReward has same Id {0}", info.Id);
                }
            }

            CrossBattleLibrary.finalsRewards = finalsRewards;
        }

        private static void InitTimingDic()
        {
            Dictionary<CrossBattleTiming, CrossBattleTiming> prepareTimeDic = new Dictionary<CrossBattleTiming, CrossBattleTiming>();
            Dictionary<CrossBattleTiming, CrossBattleTiming> guessingTimeDic = new Dictionary<CrossBattleTiming, CrossBattleTiming>();

            prepareTimeDic.Add(CrossBattleTiming.PrepareTime1, CrossBattleTiming.ShowTime1);
            prepareTimeDic.Add(CrossBattleTiming.PrepareTime2, CrossBattleTiming.ShowTime2);
            prepareTimeDic.Add(CrossBattleTiming.PrepareTime3, CrossBattleTiming.ShowTime3);
            prepareTimeDic.Add(CrossBattleTiming.PrepareTime4, CrossBattleTiming.ShowTime4);
            prepareTimeDic.Add(CrossBattleTiming.PrepareTime5, CrossBattleTiming.ShowTime5);
            prepareTimeDic.Add(CrossBattleTiming.PrepareTime6, CrossBattleTiming.ShowTime6);

            guessingTimeDic.Add(CrossBattleTiming.FinalsStart, CrossBattleTiming.BattleTime1);
            guessingTimeDic.Add(CrossBattleTiming.ShowTime1, CrossBattleTiming.BattleTime2);
            guessingTimeDic.Add(CrossBattleTiming.ShowTime2, CrossBattleTiming.BattleTime3);
            guessingTimeDic.Add(CrossBattleTiming.ShowTime3, CrossBattleTiming.BattleTime4);
            guessingTimeDic.Add(CrossBattleTiming.ShowTime4, CrossBattleTiming.BattleTime5);
            guessingTimeDic.Add(CrossBattleTiming.ShowTime5, CrossBattleTiming.BattleTime6);

            CrossBattleLibrary.prepareTimeDic = prepareTimeDic;
            CrossBattleLibrary.guessingTimeDic = guessingTimeDic;
        }

        public static int GetGroupId(int mainId)
        {
            int group;
            crossGroups.TryGetValue(mainId, out group);
            return group;
        }

        public static CrossLevelInfo CheckCrossLevel(int star)
        {
            CrossLevelInfo info = null;
            int tempStar = star;
            foreach (var kv in crossLevelInfos)
            {
                info = kv.Value;
                tempStar -= kv.Value.LimitStar;
                if (tempStar <= 0)
                {
                    break;
                }
            }
            return info;
        }

        public static CrossLevelInfo GetCrossLevelInfo(int id)
        {
            CrossLevelInfo info;
            crossLevelInfos.TryGetValue(id, out info);
            return info;
        }

        public static int GetGroupServerId(int mainId)
        {
            int index = -1;
            int group = GetGroupId(mainId);
            List<int> list = GetGroupServers(group);
            if (list != null)
            {
                index = list.IndexOf(mainId) + 1;
            }
            return index;
        }

        //private static int GetGroupMainId(int group, int serverId)
        //{
        //    int mainId = 0;
        //    if (serverId > 0)
        //    {
        //        List<int> list = GetGroupServers(group);
        //        mainId = GetGroupMainId(serverId, list);
        //    }
        //    return mainId;
        //}

        public static int GetGroupMainId(int serverId, List<int> list)
        {
            if (list != null)
            {
                if (list.Count >= serverId)
                {
                    return list[serverId - 1];
                }
            }
            return 0;
        }

        public static List<int> GetGroupServers(int group)
        {
            List<int> list;
            GroupList.TryGetValue(group, out list);
            return list;
        }

        public static CrossFightGroup GetFightGroup(int serverId, int rank)
        {
            foreach (var kv in FightGroupList)
            {
                foreach (var item in kv.Value)
                {
                    if (item.Server == serverId && item.Rank == rank)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public static int GetFightIndex(int rank)
        {
            CrossFightIndex info = null;
            if (FightIndexList.TryGetValue(rank, out info))
            {
                return info.Index;
            }
            //foreach (var kv in FightIndexList)
            //{
            //    if (kv.Value.Rank == rank)
            //    {
            //        return kv.Value.Index;
            //    }
            //}
            return -1;
        }

        public static int GetFightRank(int index)
        {
            foreach (var kv in FightIndexList)
            {
                if (kv.Value.Index == index)
                {
                    return kv.Value.Rank;
                }
            }
            return -1;
        }

        public static Dictionary<int, List<int>> GetFightInfo(CrossBattleTiming timing)
        {
            Dictionary<int, List<int>> info;
            fightList.TryGetValue(timing, out info);
            return info;
        }
        public static int GetFightId(CrossBattleTiming timing, int index)
        {
            Dictionary<int, List<int>> info;
            if (fightList.TryGetValue(timing, out info))
            {
                foreach (var item in info)
                {
                    if (item.Value.Contains(index))
                    {
                        return item.Key;
                    }
                }
            }
            return 0;
        }
        //public static CrossSeasonInfo GetCrossSeasonInfo(int id)
        //{
        //    CrossSeasonInfo info;
        //    crossSeasonInfos.TryGetValue(id, out info);
        //    return info;
        //}

        //public static CrossSeasonInfo GetCrossSeasonInfoByTime(DateTime time)
        //{
        //    CrossSeasonInfo info = null;
        //    foreach (var item in crossSeasonInfos)
        //    {
        //        info = item.Value;

        //        if (time <= item.Value.End)
        //        {
        //            break;
        //        }
        //    }
        //    return info;
        //}

        //public static int GetCrossTimeKey(DateTime time)
        //{
        //    int timestamp = Timestamp.GetUnixTimeStampSeconds(time);
        //    int timeKey = int.MaxValue - timestamp + 1000000000;
        //    return timeKey;                        //1588484447
        //}


        private static void AddWeekTimeSpan(Dictionary<CrossBattleTiming, WeekTimeSpan> timingList, Data data, string name, CrossBattleTiming key)
        {
            List<string> timeList = data.GetStringList(name, "|");
            if (timeList.Count > 1)
            {
                int addWeek = int.Parse(timeList[0]);
                int week = int.Parse(timeList[1]);
                //if (week == 0)
                //{
                //    week = 7;
                //}
                TimeSpan tmpSpan = TimeSpan.Parse(timeList[2]);
                WeekTimeSpan weekimeSpan = new WeekTimeSpan(week, tmpSpan);
                weekimeSpan.AddWeek = addWeek * 7;
                timingList[key] = weekimeSpan;
            }
        }

        public static WeekTimeSpan GetWeekTime(CrossBattleTiming key)
        {
            WeekTimeSpan weekimeSpan = null;
            timingList.TryGetValue(key, out weekimeSpan);
            return weekimeSpan;
        }

        public static bool CheckWeekTime(CrossTimeCheck key, DateTime startTime, DateTime time)
        {
            switch (key)
            {
                case CrossTimeCheck.Preliminary:
                    {
                        WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
                        DateTime endTime = GetTimingDate(CrossBattleTiming.FinalsStart, startTime, start);
                        if (startTime < time && time < endTime)
                        {
                            //说明在时间内
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case CrossTimeCheck.Finals:
                    {
                        WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
                        DateTime finalsStartTime = GetTimingDate(CrossBattleTiming.FinalsStart, startTime, start);
                        DateTime endTime = GetTimingDate(CrossBattleTiming.End, startTime, start);
                        if (finalsStartTime < time && time < endTime)
                        {
                            //说明在时间内
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case CrossTimeCheck.PrepareTime:
                    {
                        WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
                        foreach (var kv in prepareTimeDic)
                        {
                            DateTime beginTime = GetTimingDate(kv.Key, startTime, start);
                            DateTime endTime = GetTimingDate(kv.Value, startTime, start);
                            if (beginTime < time && time < endTime)
                            {
                                //说明在时间内
                                return false;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        return true;
                    }
                case CrossTimeCheck.GuessingTime:
                    {
                        WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
                        foreach (var kv in guessingTimeDic)
                        {
                            DateTime beginTime = GetTimingDate(kv.Key, startTime, start);
                            DateTime endTime = GetTimingDate(kv.Value, startTime, start);
                            if (beginTime < time && time < endTime)
                            {
                                //说明在时间内
                                return true;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
            return false;
        }

        public static CrossBattleTiming GetCurrentGuessingTime(DateTime startTime, DateTime time)
        {
            WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);
            foreach (var kv in guessingTimeDic)
            {
                DateTime beginTime = GetTimingDate(kv.Key, startTime, start);
                DateTime endTime = GetTimingDate(kv.Value, startTime, start);
                if (beginTime < time && time < endTime)
                {
                    //说明在时间内
                    return kv.Value;
                }
            }
            return CrossBattleTiming.Start;
        }
        public static CrossBattleTiming GetGuessingTime(DateTime startTime, DateTime time)
        {
            CrossBattleTiming currentTime = CrossBattleTiming.Start;
            WeekTimeSpan start = GetWeekTime(CrossBattleTiming.Start);

            DateTime startStop = GetTimingDate(CrossBattleTiming.FinalsStart, startTime, start);
            if (startTime <= time && time <= startStop)
            {
                //说明在时间内
                return CrossBattleTiming.Start;
            }

            DateTime endStart = GetTimingDate(CrossBattleTiming.ShowTime6, startTime, start);
            DateTime endStop = GetTimingDate(CrossBattleTiming.End, startTime, start);
            if (endStart < time && time < endStop)
            {
                //说明在时间内
                return CrossBattleTiming.End;
            }

            foreach (var kv in guessingTimeDic)
            {
                DateTime beginTime = GetTimingDate(kv.Key, startTime, start);
                DateTime endTime = GetTimingDate(kv.Value, startTime, start);
                if (beginTime <= time)
                {
                    //说明在时间内
                    currentTime = kv.Value;
                }
                else
                {
                    break;
                }
            }
            return currentTime;
        }



        private static DateTime GetTimingDate(CrossBattleTiming timing, DateTime startTime, WeekTimeSpan start)
        {
            WeekTimeSpan timingStart = GetWeekTime(timing);
            DateTime timingStartTime = startTime.AddDays(timingStart.AddWeek);
            timingStartTime = timingStartTime.AddDays(timingStart.WDay - start.WDay);
            timingStartTime = timingStartTime + timingStart.TSpan;
            return timingStartTime;
        }

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

        //public static CrossBattleTiming CheckCurrentTiming(DateTime time)
        //{
        //    foreach (var kv in timingList)
        //    {
        //        if (time.DayOfWeek == kv.Value.Week)
        //        {
        //            if (time.TimeOfDay == kv.Value.TSpan)
        //            {
        //                return kv.Key;
        //            }
        //        }
        //    }
        //    return CrossBattleTiming.Start;
        //}

        public static CrossBattleTiming CheckNextTiming(CrossBattleTiming timing)
        {
            bool getNext = false;
            foreach (var kv in timingList)
            {
                if (getNext)
                {
                    return kv.Key;
                }
                if (timing == kv.Key)
                {
                    getNext = true;
                }
            }
            return CrossBattleTiming.Start;
        }

        /// <summary>
        /// 下一个时间
        /// </summary>
        /// <param name="nextTiming"></param>
        /// <returns></returns>
        public static DateTime GetNextTime(CrossBattleTiming lstTiming, CrossBattleTiming nextTiming, DateTime time)
        {
            WeekTimeSpan lastTimeSpan = CrossBattleLibrary.GetWeekTime(lstTiming);
            if (lastTimeSpan == null)
            {
                return time;
            }
            WeekTimeSpan nextTimeSpan = CrossBattleLibrary.GetWeekTime(nextTiming);
            if (nextTimeSpan == null)
            {
                return time;
            }
            int timeWDay = (int)time.DayOfWeek;
            if (timeWDay == 0)
            {
                timeWDay = 7;
            }
            int addDay = nextTimeSpan.WDay - timeWDay;
            if (lastTimeSpan.AddWeek < nextTimeSpan.AddWeek)
            {
                addDay += nextTimeSpan.AddWeek;
                if (nextTiming == CrossBattleTiming.End)
                {
                    addDay -= 7;
                }
            }
            if (addDay < 0)
            {
                addDay += 7;
            }
            return time.Date.AddDays(addDay) + nextTimeSpan.TSpan;
        }
        /// <summary>
        /// 上一个时间
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DateTime GetBeforeTime(CrossBattleTiming type, DateTime time)
        {
            WeekTimeSpan weekTimeSpan = CrossBattleLibrary.GetWeekTime(type);
            int addDay = weekTimeSpan.Week - time.DayOfWeek;
            if (addDay > 0)
            {
                addDay -= 7;
            }
            addDay += weekTimeSpan.AddWeek;
            return time.Date.AddDays(addDay) + weekTimeSpan.TSpan;
        }

        public static RankRewardInfo GetRankRewardInfo(int rank)
        {
            foreach (var item in finalsRewards)
            {
                if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
                {
                    return item.Value;
                }
            }
            return null;
        }
        //private static void InitRandomInfos()
        //{
        //    randomInfos.Clear();
        //    ArenaRandomInfo info;
        //    DataList dataList = DataListManager.inst.GetDataList("RandomChallenge");
        //    foreach (var item in dataList)
        //    {
        //        Data data = item.Value;

        //        info = new ArenaRandomInfo();
        //        info.RankMin = data.GetInt("RankMin");
        //        info.RankMax = data.GetInt("RankMax");
        //        info.FristMin = data.GetInt("FristMin");
        //        info.FristMax = data.GetInt("FristMax");
        //        info.SecondMin = data.GetInt("SecondMin");
        //        info.SecondMax = data.GetInt("SecondMax");
        //        info.ThirdMin = data.GetInt("ThirdMin");
        //        info.ThirdMax = data.GetInt("ThirdMax");
        //        info.FourthMin = data.GetInt("FourthMin");
        //        info.FourthMax = data.GetInt("FourthMax");

        //        if (!randomInfos.ContainsKey(data.ID))
        //        {
        //            randomInfos.Add(data.ID, info);
        //        }
        //        else
        //        {
        //            Logger.Log.Warn("InitRandomInfos has same id {0}", data.ID);
        //        }
        //    }
        //}






        //public static Dictionary<int, RankRewardInfo> GetDailyRankRewards()
        //{
        //    return dailyRankRewards;
        //}

        //public static int GetWinStreakScore(int num)
        //{
        //    int score = 0;
        //    foreach (var item in winStreakScore)
        //    {
        //        if (num >= item.Key)
        //        {
        //            score = item.Value;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    return score;
        //}

        //public static RankRewardInfo GetDailyRankRewardInfoById(int id)
        //{
        //    RankRewardInfo info;
        //    dailyRankRewards.TryGetValue(id, out info);
        //    return info;
        //}

        //public static RankRewardInfo GetDailyRankRewardInfo(int rank)
        //{
        //    foreach (var item in dailyRankRewards)
        //    {
        //        if (item.Value.RankMin <= rank && rank <= item.Value.RankMax)
        //        {
        //            return item.Value;
        //        }
        //    }
        //    return null;
        //}




        //public static RankLevelInfo GetRankLevelInfo(int level)
        //{
        //    RankLevelInfo info;
        //    rankLevelInfos.TryGetValue(level, out info);
        //    return info;
        //}

        public static CrossBattleTiming GetCrossBattleTiming(CrossBattleTiming timing)
        {
            switch (timing)
            {
                case CrossBattleTiming.GuessingTime:
                case CrossBattleTiming.PrepareTime1:
                    return CrossBattleTiming.BattleTime1;
                case CrossBattleTiming.ShowTime1:
                case CrossBattleTiming.PrepareTime2:
                    return CrossBattleTiming.BattleTime2;
                case CrossBattleTiming.ShowTime2:
                case CrossBattleTiming.PrepareTime3:
                    return CrossBattleTiming.BattleTime3;
                case CrossBattleTiming.ShowTime3:
                case CrossBattleTiming.PrepareTime4:
                    return CrossBattleTiming.BattleTime4;
                case CrossBattleTiming.ShowTime4:
                case CrossBattleTiming.PrepareTime5:
                    return CrossBattleTiming.BattleTime5;
                case CrossBattleTiming.ShowTime5:
                case CrossBattleTiming.PrepareTime6:
                    return CrossBattleTiming.BattleTime6;
                default:
                    return timing;
            }
        }

        public static int GetBattleEmailId(CrossBattleTiming timing)
        {
            switch (timing)
            {
                case CrossBattleTiming.ShowTime1:
                case CrossBattleTiming.BattleTime1:
                    return BattleEmail32;
                case CrossBattleTiming.ShowTime2:
                case CrossBattleTiming.BattleTime2:
                    return BattleEmail16;
                case CrossBattleTiming.ShowTime3:
                case CrossBattleTiming.BattleTime3:
                    return BattleEmail8;
                case CrossBattleTiming.ShowTime4:
                case CrossBattleTiming.BattleTime4:
                    return BattleEmail4;
                case CrossBattleTiming.ShowTime5:
                case CrossBattleTiming.BattleTime5:
                    return BattleEmail2;
                case CrossBattleTiming.PrepareTime1:
                case CrossBattleTiming.PrepareTime2:
                case CrossBattleTiming.PrepareTime3:
                case CrossBattleTiming.PrepareTime4:
                case CrossBattleTiming.PrepareTime5:
                case CrossBattleTiming.PrepareTime6:
                    return PrepareEmail;
                //case CrossBattleTiming.ShowTime6:
                //case CrossBattleTiming.BattleTime6:
                //    return CrossBattleTiming.BattleTime6;
                default:
                    return 0;
            }
        }
    }
}
