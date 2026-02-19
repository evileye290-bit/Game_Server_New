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
    public class RankLibrary
    {
        //public static DateTime startedTime = DateTime.Now;
        //private static Dictionary<int, RankConfigInfo> rankTypeAndInfos = new Dictionary<int, RankConfigInfo>();
        private static Dictionary<int, RankConfigInfo> configs = new Dictionary<int, RankConfigInfo>();
        private static Dictionary<int, RankRewardModel> rewards = new Dictionary<int, RankRewardModel>();
        private static Dictionary<RankType, List<int>> rewardsByTypes = new Dictionary<RankType, List<int>>();
        public static int RewardPageCount;

        public static void Init()
        {
            //rankTypeAndInfos.Clear();
            InitRankInfos();
            //InitConfigs();
            InitRankRewards();
            InitRankConfig();
        }

        public static void InitRankInfos()
        {
            Dictionary<int, RankConfigInfo> configs = new Dictionary<int, RankConfigInfo>();
            //configs.Clear();

            DataList rankDatas = DataListManager.inst.GetDataList("Rank");
            foreach (var item in rankDatas)
            {
                Data data = item.Value;
                RankConfigInfo info = new RankConfigInfo(data);
                configs.Add((int)info.Type, info);
            }
            RankLibrary.configs = configs;
        }

        public static void InitRankRewards()
        {
            Dictionary<int, RankRewardModel> rewards = new Dictionary<int, RankRewardModel>();
            Dictionary<RankType, List<int>> rewardsByTypes = new Dictionary<RankType, List<int>>();
            //rewards.Clear();
            //rewardsByTypes.Clear();
            List<int> list;
            DataList rankDatas = DataListManager.inst.GetDataList("RankReward");
            foreach (var item in rankDatas)
            {
                Data data = item.Value;
                RankRewardModel info = new RankRewardModel(data);
                rewards.Add(data.ID, info);
                if (rewardsByTypes.TryGetValue(info.type, out list))
                {
                    list.Add(data.ID);
                }
                else
                {
                    list = new List<int>();
                    list.Add(data.ID);
                    rewardsByTypes.Add(info.type, list);
                }
            }
            RankLibrary.rewards = rewards;
            RankLibrary.rewardsByTypes = rewardsByTypes;
        }

        private static void InitRankConfig()
        {
            Data data = DataListManager.inst.GetData("RankConfig", 1);
            RewardPageCount = data.GetInt("RewardPageCount");
        }

        public static RankConfigInfo GetConfig(RankType type)
        {
            RankConfigInfo config = null;
            configs.TryGetValue((int)type, out config);
            return config;
        }

        public static List<int> GetRewardList(RankType type)
        {
            List<int> list = null;
            rewardsByTypes.TryGetValue(type, out list);
            return list;
        }

        public static Dictionary<RankType, List<int>> GetAllRewardList()
        {
            return rewardsByTypes;
        }

        public static RankRewardModel GetReward(int id)
        {
            RankRewardModel config = null;
            rewards.TryGetValue((int)id, out config);
            return config;
        }

        /// <summary>
        /// 前两个周期和后边的周期算法均不同，必要时需要重构一下
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serverOpenTime"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static Tuple<DateTime, DateTime> GetNextPeriod(RankType type, DateTime serverOpenTime, int period)
        {
            DateTime now = DateTime.Now;
            RankConfigInfo config = null;
            DateTime begin;
            DateTime end;

            if (configs.TryGetValue((int)type, out config))
            {
                //第一个周期
                if (period == 1 || now - serverOpenTime < new TimeSpan(3, 0, 0, 0))
                {
                    return Tuple.Create(serverOpenTime, serverOpenTime + new TimeSpan(3, 0, 0, 0));
                }
                if (period == 2)
                {
                    //第二个周期
                    //DayOfWeek beginDay = (DayOfWeek)(((int)(serverOpenTime.DayOfWeek) + 3) % 7);

                    DateTime temp2E = serverOpenTime.Date + new TimeSpan(3, 0, 0, 0);

                    if (config.EndWeekDay == 0)
                    {
                        temp2E = temp2E.AddDays(7 - (int)temp2E.DayOfWeek);
                        end = temp2E.AddHours(config.EndHour);
                    }
                    else
                    {
                        temp2E = temp2E.AddDays(config.EndWeekDay - temp2E.DayOfWeek);
                        end = temp2E.AddHours(config.EndHour);
                    }

                    if (now < end)//不能只通过period参数判断
                    {
                        return Tuple.Create(now, end);
                    }
                }


                //正常周期
                DateTime tempB = DateTime.Today;
                DateTime tempE = DateTime.Today;

                if (config.BeginWeekDay == 0)
                {
                    tempB = tempB.AddDays((7 - (int)tempB.DayOfWeek) % 7);
                    begin = tempB.AddHours(config.BeginHour);
                }
                else
                {
                    tempB = tempB.AddDays(config.BeginWeekDay - tempB.DayOfWeek);
                    begin = tempB.AddHours(config.BeginHour);
                }

                if (config.EndWeekDay == 0)
                {
                    tempE = tempE.AddDays(7 - (int)tempE.DayOfWeek);
                    end = tempE.AddHours(config.EndHour);
                }
                else
                {
                    tempE = tempE.AddDays(config.EndWeekDay - tempE.DayOfWeek);
                    end = tempE.AddHours(config.EndHour);
                }

                begin = begin.AddMinutes(config.BeginMinute);
                begin = begin.AddSeconds(config.BeginSecond);
                end = end.AddMinutes(config.EndMinute);
                end = end.AddSeconds(config.EndSecond);

                if (begin < now&&end<now)
                {
                    begin = begin.AddDays(7);
                }
                while (begin >= end)
                {
                    end = end.AddDays(7);
                }

                return Tuple.Create(begin, end);

            }
            else
            {
                Logger.Log.Warn($"check {type} config in rank.xml");
                return Tuple.Create(now, now + new TimeSpan(7, 0, 0, 0));
            }
        }

        /// <summary>
        /// 这个是用于campbuild的。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="now"></param>
        /// <param name="serverOpenTime"></param>
        /// <param name="phase"></param>
        /// <returns></returns>
        public static Tuple<DateTime, DateTime> GetNextPeriod(RankType type,DateTime now, DateTime serverOpenTime, ref int phase)
        {
            int curPhase = phase;
            RankConfigInfo config = null;
            DateTime begin;
            DateTime end;
            ///周期为7天
            int period = 7;

            if (configs.TryGetValue((int)type, out config))
            {
                phase = 1;

                DateTime srcBeginDay = serverOpenTime.Date - TimeSpan.FromDays(serverOpenTime.DayOfWeek - config.BeginWeekDay);
                TimeSpan deltaDay = now.Date - srcBeginDay;

                int delta = (int)(deltaDay.TotalDays / period);

                if (delta>0)
                {
                    DateTime beginDay = srcBeginDay.AddDays(delta * period);
                    begin = beginDay.AddHours(config.BeginHour).AddMinutes(config.BeginMinute).AddSeconds(config.BeginSecond);
                    if (begin.DayOfWeek != config.BeginWeekDay)
                    {
                        Logger.Log.Error($"rank {type}  phase {phase} GetNextPeriod fail {begin.ToString()}");
                        return Tuple.Create(now, now + new TimeSpan(7, 0, 0, 0));
                    }
                    else
                    {
                        phase = phase + (int)delta;
                        end = beginDay.AddDays(period-1).AddHours(config.EndHour).AddMinutes(config.EndMinute).AddSeconds(config.EndSecond);
                        return Tuple.Create(begin, end);
                    }
                }
                else
                {
                    //开服时间在第一期里
                    begin = srcBeginDay.AddHours(config.BeginHour).AddMinutes(config.BeginMinute).AddSeconds(config.BeginSecond);
                    end = srcBeginDay.AddDays(period-1).AddHours(config.EndHour).AddMinutes(config.EndMinute).AddSeconds(config.EndSecond);
                    return Tuple.Create(begin, end);
                }

            }
            else
            {
                Logger.Log.Warn($"check {type} config in rank.xml");
                return Tuple.Create(now, now + new TimeSpan(7, 0, 0, 0));
            }
             
        }


        //public static void InitConfigs()
        //{
        //    foreach (var item in rankTypeAndInfos)
        //    {
        //        GeneratedRankConfig config = item.Value.GenerateConfig();
        //        configs.Add(item.Key, config);
        //    }
        //}

        //public static RankConfigInfo GetRankInfo(RankType type)
        //{
        //    RankConfigInfo info = null;
        //    rankTypeAndInfos.TryGetValue((int)type, out info);
        //    return info;
        //}


        //public static Tuple<DateTime, DateTime> GetNextPeriod(RankType type)
        //{
        //    DateTime now = DateTime.Now;
        //    GeneratedRankConfig config = null;
        //    DateTime begin;
        //    DateTime end;

        //    if (configs.TryGetValue((int)type, out config))
        //    {
        //        //第一个周期
        //        if (now - startedTime < new TimeSpan(3, 0, 0, 0))
        //        {
        //            return Tuple.Create(now, now + new TimeSpan(3, 0, 0, 0));
        //        }

        //        //第二个周期
        //        DayOfWeek today = now.DayOfWeek;
        //        if (today < config.BeginWeekDay ||(today==config.BeginWeekDay && now.Hour<config.BeginHour))
        //        {
        //            DateTime period2E = DateTime.Today;

        //            if (config.EndWeekDay == 0)
        //            {
        //                period2E = period2E.AddDays(7 - (int)period2E.DayOfWeek);
        //                end = period2E.AddHours(config.EndHour);
        //            }
        //            else
        //            {
        //                period2E = period2E.AddDays(config.EndWeekDay - period2E.DayOfWeek);
        //                end = period2E.AddHours(config.EndHour);
        //            }

        //            return Tuple.Create(now,end);
        //        }

        //        //正常周期
        //        DateTime tempB = DateTime.Today;
        //        DateTime tempE = DateTime.Today;

        //        if (config.EndWeekDay == 0)
        //        {
        //            tempB = tempB.AddDays(7 - (int)tempB.DayOfWeek);
        //            begin = tempB.AddHours(config.EndHour);
        //        }
        //        else
        //        {
        //            tempB = tempB.AddDays(config.EndWeekDay - tempB.DayOfWeek);
        //            begin = tempB.AddHours(config.EndHour);
        //        }

        //        if (config.EndWeekDay == 0)
        //        {
        //            tempE = tempE.AddDays(7 - (int)tempE.DayOfWeek);
        //            end = tempE.AddHours(config.EndHour);
        //        }
        //        else
        //        {
        //            tempE = tempE.AddDays(config.EndWeekDay - tempE.DayOfWeek);
        //            end = tempE.AddHours(config.EndHour);
        //        }

        //        return Tuple.Create(begin, end);

        //    }
        //    else
        //    {
        //        Logger.Log.Warn($"check {type} config in rank.xml");
        //        return Tuple.Create(now, now + new TimeSpan(7, 0, 0, 0));
        //    }

        //}

        //public static Tuple<DateTime, DateTime>  GetNextPeriod(RankType type,DateTime serverOpenTime)
        //{
        //    DateTime now = DateTime.Now;
        //    GeneratedRankConfig config = null;
        //    DateTime begin;
        //    DateTime end;

        //    if (configs.TryGetValue((int)type, out config))
        //    {
        //        //第一个周期
        //        if (now - serverOpenTime < new TimeSpan(3, 0, 0, 0))
        //        {
        //            return Tuple.Create(now, now + new TimeSpan(3, 0, 0, 0));
        //        }

        //        //第二个周期
        //        DayOfWeek today = now.DayOfWeek;
        //        if (today < config.BeginWeekDay || (today == config.BeginWeekDay && now.Hour < config.BeginHour))
        //        {
        //            DateTime period2E = DateTime.Today;

        //            if (config.EndWeekDay == 0)
        //            {
        //                period2E = period2E.AddDays(7 - (int)period2E.DayOfWeek);
        //                end = period2E.AddHours(config.EndHour);
        //            }
        //            else
        //            {
        //                period2E = period2E.AddDays(config.EndWeekDay - period2E.DayOfWeek);
        //                end = period2E.AddHours(config.EndHour);
        //            }

        //            return Tuple.Create(now, end);
        //        }

        //        //正常周期
        //        DateTime tempB = DateTime.Today;
        //        DateTime tempE = DateTime.Today;

        //        if (config.EndWeekDay == 0)
        //        {
        //            tempB = tempB.AddDays(7 - (int)tempB.DayOfWeek);
        //            begin = tempB.AddHours(config.EndHour);
        //        }
        //        else
        //        {
        //            tempB = tempB.AddDays(config.EndWeekDay - tempB.DayOfWeek);
        //            begin = tempB.AddHours(config.EndHour);
        //        }

        //        if (config.EndWeekDay == 0)
        //        {
        //            tempE = tempE.AddDays(7 - (int)tempE.DayOfWeek);
        //            end = tempE.AddHours(config.EndHour);
        //        }
        //        else
        //        {
        //            tempE = tempE.AddDays(config.EndWeekDay - tempE.DayOfWeek);
        //            end = tempE.AddHours(config.EndHour);
        //        }

        //        return Tuple.Create(begin, end);

        //    }
        //    else
        //    {
        //        Logger.Log.Warn($"check {type} config in rank.xml");
        //        return Tuple.Create(now, now + new TimeSpan(7, 0, 0, 0));
        //    }
        //}
    }







    //public class RankConfigInfo
    //{
    //    public int Id;
    //    public RankType Type;
    //    public string BeginTime;
    //    public string EndTime;
    //    public string RankUpdateTimeSpan;
    //    public string InfoUpdateTimeSpan;
    //    public bool SyncUpdate;
    //    public int ShowCount;
    //    public int CountPerPage;

    //    public GeneratedRankConfig GenerateConfig()
    //    {
    //        GeneratedRankConfig config = new GeneratedRankConfig();
    //        config.Id = Id;
    //        config.Type = Type;
    //        string[] beginTemp = BeginTime.Split(':');
    //        config.BeginWeekDay = (DayOfWeek)int.Parse(beginTemp[0]);
    //        config.BeginHour = int.Parse(beginTemp[1]);
    //        config.BeginMinute = int.Parse(beginTemp[2]);
    //        config.BeginSecond = int.Parse(beginTemp[3]);
    //        string[] endTemp = EndTime.Split(':');
    //        config.EndWeekDay = (DayOfWeek)int.Parse(endTemp[0]);
    //        config.EndHour = int.Parse(endTemp[1]);
    //        config.EndMinute = int.Parse(endTemp[2]);
    //        config.EndSecond = int.Parse(endTemp[3]);
    //        string[] rankSpan = RankUpdateTimeSpan.Split(':');
    //        config.RankUpdateTimeSpan = new TimeSpan(int.Parse(rankSpan[0]), int.Parse(rankSpan[1]), int.Parse(rankSpan[2]));
    //        string[] infoSpan = InfoUpdateTimeSpan.Split(':');
    //        config.InfoUpdateTimeSpan = new TimeSpan(int.Parse(rankSpan[0]), int.Parse(rankSpan[1]), int.Parse(rankSpan[2]));
    //        config.SyncUpdate = SyncUpdate;
    //        config.ShowCount = ShowCount;
    //        config.CountPerPage = CountPerPage;
    //        return config;
    //    }
    //}


    //public class RankPeriod
    //{
    //    public RankType Type;
    //    public int Period;
    //    public DateTime Start;
    //    public DateTime End;
    //}
}
