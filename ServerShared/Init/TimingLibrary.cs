using CommonUtility;
using DataProperty;
using EnumerateUtility.Timing;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public static class TimingLibrary
    {
        //public static List<TimeSpan> HuntingTimings = new List<TimeSpan>();
        /// <summary>
        /// 固定日期触发器
        /// </summary>
        public static Dictionary<DateTime, List<TimingType>> DateTimings = new Dictionary<DateTime, List<TimingType>>();
        public static Dictionary<DateTime, List<TimingType>> RelationDateTimings = new Dictionary<DateTime, List<TimingType>>();
        /// <summary>
        /// 每天刷新触发器
        /// </summary>
        public static Dictionary<TimeSpan, List<TimingType>> DailyTimings = new Dictionary<TimeSpan, List<TimingType>>();
        public static Dictionary<TimeSpan, List<TimingType>> RelationDailyTimings = new Dictionary<TimeSpan, List<TimingType>>();

        /// <summary>
        /// 每周刷新触发器
        /// </summary>
        public static Dictionary<WeekTimeSpan, List<TimingType>> WeekTimings = new Dictionary<WeekTimeSpan, List<TimingType>>();
        public static Dictionary<WeekTimeSpan, List<TimingType>> RelationWeekTimings = new Dictionary<WeekTimeSpan, List<TimingType>>();

        /// <summary>
        /// 每月刷新触发器
        /// </summary>
        public static Dictionary<MonthTimeSpan, List<TimingType>> MonthTimings = new Dictionary<MonthTimeSpan, List<TimingType>>();
        public static Dictionary<MonthTimeSpan, List<TimingType>> RelationMonthTimings = new Dictionary<MonthTimeSpan, List<TimingType>>();

        ///// <summary>
        ///// 上次刷新时间
        ///// </summary>
        //public static DateTime LastRefresh { get; set; }

        /// <summary>
        /// 初始化定时刷新
        /// <para>设计思路：</para>
        /// <para>初始化RefershTiming.xml配置表，生成所有需要刷新的事件。---->随着系统运行进行轮询，用当前时间和所有事件进行匹配，（精确到分钟，设定lastMinute字段，同一分钟第二次询问会返回false，不会触发
        /// 事件）---->如果任何事件触发都会doRefresh置为true，在所有事件询问后，进行SaveLastRefreshTime，同时保存数据库，防止同一分钟内两个事件触发导致SaveLastRefreshTime两次，以减少压力。---->SaveLastRefreshTime的作用：
        /// 用于pc上线时检测，当pc上线时会访问这个时间，如果下线期间事件发生，补偿触发。</para>
        /// <para>关于集中推送:可能造成服务器瞬间卡顿，已经取得策划认可，这是可以接受的设计之一。</para>
        /// <para>货币刷新，商店刷新在此处改写，原有功能废弃。时间：2015年9月16日11:30:34</para>
        /// </summary>
        public static void BindTimingData()
        {

            DateTime now = DateTime.Now;

            InitDateTimings(now);

            //InitCrossSeasonDateTimings(now);

            InitDailyTimings();

            InitWeekTimings();

            InitMonthTimings();
        }

        private static void InitMonthTimings()
        {
            //MonthTimings.Clear();
            //RelationMonthTimings.Clear();
            Dictionary<MonthTimeSpan, List<TimingType>> MonthTimings = new Dictionary<MonthTimeSpan, List<TimingType>>();
            Dictionary<MonthTimeSpan, List<TimingType>> RelationMonthTimings = new Dictionary<MonthTimeSpan, List<TimingType>>();
            DataList monthList = DataListManager.inst.GetDataList("MonthTiming");
            foreach (var item in monthList)
            {
                Data data = item.Value;
                int id = data.ID;
                int day = data.GetInt("Day");
                string times = data.GetString("Timing");

                string[] list = StringSplit.GetArray("|", times);
                foreach (var time in list)
                {
                    if (!string.IsNullOrEmpty(time))
                    {
                        TimeSpan tmpSpan = TimeSpan.Parse(time);
                        MonthTimeSpan monthTimeSpan = new MonthTimeSpan(day, tmpSpan);

                        if (data.GetInt("Zone") == 1)
                        {
                            AddMonthTimeSpanTimingType(MonthTimings, id, monthTimeSpan);
                        }
                        if (data.GetInt("Relation") == 1)
                        {
                            AddMonthTimeSpanTimingType(RelationMonthTimings, id, monthTimeSpan);
                        }
                    }
                }
            }
            TimingLibrary.MonthTimings = MonthTimings;
            TimingLibrary.RelationMonthTimings = RelationMonthTimings;
        }

        private static void AddMonthTimeSpanTimingType(Dictionary<MonthTimeSpan, List<TimingType>> timings, int id, MonthTimeSpan monthTimeSpan)
        {
            List<TimingType> tasks;
            bool found = false;
            foreach (var month in timings)
            {
                if (month.Key.Day == monthTimeSpan.Day && month.Key.TSpan == monthTimeSpan.TSpan)
                {
                    month.Value.Add((TimingType)id);
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                tasks = new List<TimingType>();
                tasks.Add((TimingType)id);
                timings.Add(monthTimeSpan, tasks);
            }
        }

        private static void InitWeekTimings()
        {
            //WeekTimings.Clear();
            //RelationWeekTimings.Clear();
            Dictionary<WeekTimeSpan, List<TimingType>> WeekTimings = new Dictionary<WeekTimeSpan, List<TimingType>>();
            Dictionary<WeekTimeSpan, List<TimingType>> RelationWeekTimings = new Dictionary<WeekTimeSpan, List<TimingType>>();
            DataList dataList = DataListManager.inst.GetDataList("WeekTiming");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int id = data.ID;
                string weeks = data.GetString("Week");

                string[] weekList = StringSplit.GetArray("|", weeks);

                string times = data.GetString("Timing");

                string[] timeList = StringSplit.GetArray("|", times);
                foreach (var week in weekList)
                {
                    foreach (var time in timeList)
                    {
                        if (!string.IsNullOrEmpty(time))
                        {
                            TimeSpan tmpSpan = TimeSpan.Parse(time);
                            WeekTimeSpan weekimeSpan = new WeekTimeSpan(week.ToInt(), tmpSpan);

                            if (data.GetInt("Zone") == 1)
                            {
                                AddWeekTimeSpanTimingType(WeekTimings, id, weekimeSpan);
                            }
                            if (data.GetInt("Relation") == 1)
                            {
                                AddWeekTimeSpanTimingType(RelationWeekTimings, id, weekimeSpan);
                            }
                        }
                    }
                }
            }
            TimingLibrary.WeekTimings = WeekTimings;
            TimingLibrary.RelationWeekTimings = RelationWeekTimings;
        }

        private static void AddWeekTimeSpanTimingType(Dictionary<WeekTimeSpan, List<TimingType>> timings, int id, WeekTimeSpan weekimeSpan)
        {
            List<TimingType> tasks;
            bool found = false;
            foreach (var month in timings)
            {
                if (month.Key.Week == weekimeSpan.Week && month.Key.TSpan == weekimeSpan.TSpan)
                {
                    month.Value.Add((TimingType)id);
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                tasks = new List<TimingType>();
                tasks.Add((TimingType)id);
                timings.Add(weekimeSpan, tasks);
            }
        }

        private static void InitDailyTimings()
        {
            //DailyTimings.Clear();
            //RelationDailyTimings.Clear();
            Dictionary<TimeSpan, List<TimingType>> DailyTimings = new Dictionary<TimeSpan, List<TimingType>>();
            Dictionary<TimeSpan, List<TimingType>> RelationDailyTimings = new Dictionary<TimeSpan, List<TimingType>>();
            DataList dailyList = DataListManager.inst.GetDataList("DailyTiming");
            foreach (var item in dailyList)
            {
                Data data = item.Value;
                int id = data.ID;
                string times = data.GetString("Timing");

                string[] list = StringSplit.GetArray("|", times);
                foreach (var time in list)
                {
                    if (!string.IsNullOrEmpty(time))
                    {
                        TimeSpan timeSpan = TimeSpan.Parse(time);

                        if (data.GetInt("Zone") == 1)
                        {
                            AddTimeSpanTimingType(DailyTimings, id, timeSpan);
                        }
                        if (data.GetInt("Relation") == 1)
                        {
                            AddTimeSpanTimingType(RelationDailyTimings, id, timeSpan);
                        }

                        //if (id == (int)TimingType.HuntingTiming)
                        //{
                        //    HuntingTimings.Add(timeSpan);
                        //}
                    }
                }
            }
            TimingLibrary.DailyTimings = DailyTimings;
            TimingLibrary.RelationDailyTimings = RelationDailyTimings;
        }

        private static void AddTimeSpanTimingType(Dictionary<TimeSpan, List<TimingType>> timings, int id, TimeSpan timeSpan)
        {
            List<TimingType> tasks;
            if (timings.TryGetValue(timeSpan, out tasks))
            {
                tasks.Add((TimingType)id);
            }
            else
            {
                tasks = new List<TimingType>();
                tasks.Add((TimingType)id);
                timings.Add(timeSpan, tasks);
            }
        }

        private static void InitDateTimings(DateTime now)
        {
            //DateTimings.Clear();
            //RelationDateTimings.Clear();
            Dictionary<DateTime, List<TimingType>> DateTimings = new Dictionary<DateTime, List<TimingType>>();
            Dictionary<DateTime, List<TimingType>> RelationDateTimings = new Dictionary<DateTime, List<TimingType>>();
            DataList dateList = DataListManager.inst.GetDataList("DateTiming");
            foreach (var item in dateList)
            {
                Data data = item.Value;
                int id = data.ID;
                string times = data.GetString("Timing");

                string[] list = StringSplit.GetArray("|", times);
                foreach (var time in list)
                {
                    if (!string.IsNullOrEmpty(time))
                    {
                        DateTime dateTime = DateTime.Parse(time);
                        if (dateTime > now)
                        {
                            if (data.GetInt("Zone") == 1)
                            {
                                AddDateTimeTimingType(DateTimings, id, dateTime);
                            }
                            if (data.GetInt("Relation") == 1)
                            {
                                AddDateTimeTimingType(RelationDateTimings, id, dateTime);
                            }
                        }
                    }
                }
            }

            TimingLibrary.DateTimings = DateTimings;
            TimingLibrary.RelationDateTimings = RelationDateTimings;
        }

        private static void AddDateTimeTimingType(Dictionary<DateTime, List<TimingType>> timings, int id, DateTime dateTime)
        {
            List<TimingType> tasks;
            if (timings.TryGetValue(dateTime, out tasks))
            {
                tasks.Add((TimingType)id);
            }
            else
            {
                tasks = new List<TimingType>();
                tasks.Add((TimingType)id);
                timings.Add(dateTime, tasks);
            }
        }

        //private static void InitCrossSeasonDateTimings(DateTime now)
        //{
        //    DataList dateList = DataListManager.inst.GetDataList("CrossSeason");
        //    foreach (var item in dateList)
        //    {
        //        Data data = item.Value;
        //        string time = data.GetString("Start");

        //        if (!string.IsNullOrEmpty(time))
        //        {
        //            DateTime dateTime = DateTime.Parse(time);
        //            if (dateTime > now)
        //            {
        //                AddDateTimeTimingType(DateTimings, (int)TimingType.CrossSeason, dateTime);
        //            }
        //        }
        //    }
        //}

        public static List<TimingType> GetTimingListToRefresh(DateTime LastRefresh, DateTime now)
        {
            List<TimingType> tasks = new List<TimingType>();

            CheckDateTiming(LastRefresh, now, tasks);

            CheckDailyTiming(LastRefresh, now, tasks);

            CheckWeekTiming(LastRefresh, now, tasks);

            CheckMonthTiming(LastRefresh, now, tasks);

            return tasks;
        }

        public static List<TimingType> GetRelationTimingListToRefresh(DateTime LastRefresh, DateTime now)
        {
            List<TimingType> tasks = new List<TimingType>();

            CheckRelationDateTiming(LastRefresh, now, tasks);

            CheckRelationDailyTiming(LastRefresh, now, tasks);

            CheckRelationWeekTiming(LastRefresh, now, tasks);

            CheckRelationMonthTiming(LastRefresh, now, tasks);

            return tasks;
        }

        public static Dictionary<DateTime, List<TimingType>> GetTimingLists(DateTime now)
        {
            Dictionary<DateTime, List<TimingType>> tasks = new Dictionary<DateTime, List<TimingType>>();

            CheckDateTiming(now, tasks, DateTimings);

            CheckDailyTiming(now, tasks, DailyTimings);

            CheckWeekTiming(now, tasks, WeekTimings);

            CheckMonthTiming(now, tasks, MonthTimings);

            //排序
            tasks = tasks.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

            return tasks;
        }

        public static Dictionary<DateTime, List<TimingType>> GetRelationTimingLists(DateTime now)
        {
            Dictionary<DateTime, List<TimingType>> tasks = new Dictionary<DateTime, List<TimingType>>();

            CheckDateTiming(now, tasks, RelationDateTimings);

            CheckDailyTiming(now, tasks, RelationDailyTimings);

            CheckWeekTiming(now, tasks, RelationWeekTimings);

            CheckMonthTiming(now, tasks, RelationMonthTimings);

            //排序
            tasks = tasks.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

            return tasks;
        }
        private static void CheckDateTiming(DateTime now, Dictionary<DateTime, List<TimingType>> tasks, Dictionary<DateTime, List<TimingType>> dateTimings)
        {
            List<TimingType> list;
            foreach (var dateTasks in dateTimings)
            {
                if (dateTasks.Key.Date == now.Date && now.TimeOfDay <= dateTasks.Key.TimeOfDay)
                {
                    foreach (var dailyTask in dateTasks.Value)
                    {

                        if (tasks.TryGetValue(dateTasks.Key, out list))
                        {
                            list.Add(dailyTask);
                        }
                        else
                        {
                            list = new List<TimingType>();
                            list.Add(dailyTask);
                            tasks.Add(dateTasks.Key, list);
                        }
                    }
                }
            }
        }

        private static void CheckDailyTiming(DateTime now, Dictionary<DateTime, List<TimingType>> tasks, Dictionary<TimeSpan, List<TimingType>> dailyTimings)
        {
            List<TimingType> list;
            foreach (var dailyTasks in dailyTimings)
            {
                if (now.TimeOfDay <= dailyTasks.Key)
                {
                    foreach (var dailyTask in dailyTasks.Value)
                    {
                        DateTime time = now.Date.Add(dailyTasks.Key);
                        if (tasks.TryGetValue(time, out list))
                        {
                            list.Add(dailyTask);
                        }
                        else
                        {
                            list = new List<TimingType>();
                            list.Add(dailyTask);
                            tasks.Add(time, list);
                        }
                    }
                }
            }
        }

        private static void CheckWeekTiming(DateTime now, Dictionary<DateTime, List<TimingType>> tasks, Dictionary<WeekTimeSpan, List<TimingType>> weekTimings)
        {
            List<TimingType> list;
            foreach (var weekTasks in weekTimings)
            {
                if (weekTasks.Key.Week == now.DayOfWeek && now.TimeOfDay <= weekTasks.Key.TSpan)
                {
                    foreach (var weekTask in weekTasks.Value)
                    {
                        DateTime time = now.Date.Add(weekTasks.Key.TSpan);
                        if (tasks.TryGetValue(time, out list))
                        {
                            list.Add(weekTask);
                        }
                        else
                        {
                            list = new List<TimingType>();
                            list.Add(weekTask);
                            tasks.Add(time, list);
                        }
                    }
                }
            }
        }

        private static void CheckMonthTiming(DateTime now, Dictionary<DateTime, List<TimingType>> tasks, Dictionary<MonthTimeSpan, List<TimingType>> monthTimings)
        {
            List<TimingType> list;
            foreach (var monthTasks in monthTimings)
            {
                if (now.Day == monthTasks.Key.Day && now.TimeOfDay <= monthTasks.Key.TSpan)
                {
                    foreach (var monthTask in monthTasks.Value)
                    {
                        DateTime time = now.Date.Add(monthTasks.Key.TSpan);
                        if (tasks.TryGetValue(time, out list))
                        {
                            list.Add(monthTask);
                        }
                        else
                        {
                            list = new List<TimingType>();
                            list.Add(monthTask);
                            tasks.Add(time, list);
                        }
                    }
                }
            }
        }

        private static void CheckDateTiming(DateTime LastRefresh, DateTime now, List<TimingType> tasks)
        {
            foreach (var dateTasks in DateTimings)
            {
                //刷新时间只要大于上次刷新时间并且小于当前时间就可以刷新
                if (LastRefresh < dateTasks.Key && dateTasks.Key <= now)
                {
                    foreach (var dailyTask in dateTasks.Value)
                    {
                        if (!tasks.Contains(dailyTask))
                        {
                            tasks.Add(dailyTask);
                        }
                    }
                }
            }
        }

        private static void CheckRelationDateTiming(DateTime LastRefresh, DateTime now, List<TimingType> tasks)
        {
            foreach (var dateTasks in RelationDateTimings)
            {
                //刷新时间只要大于上次刷新时间并且小于当前时间就可以刷新
                if (LastRefresh < dateTasks.Key && dateTasks.Key <= now)
                {
                    foreach (var dailyTask in dateTasks.Value)
                    {
                        if (!tasks.Contains(dailyTask))
                        {
                            tasks.Add(dailyTask);
                        }
                    }
                }
            }
        }

        private static void CheckMonthTiming(DateTime lastRefresh, DateTime now, List<TimingType> tasks)
        {
            foreach (var monthTasks in MonthTimings)
            {
                DateTime markTime = now.Date.Add(monthTasks.Key.TSpan);
                //刷新时间只要大于上次刷新时间并且小于当前时间就可以刷新
                if (now.Day >= monthTasks.Key.Day && lastRefresh < markTime && markTime <= now && (lastRefresh.Month < markTime.Month || lastRefresh.Year < markTime.Year))
                {
                    foreach (var monthTask in monthTasks.Value)
                    {
                        if (!tasks.Contains(monthTask))
                        {
                            tasks.Add(monthTask);
                        }
                    }
                }
            }
        }

        private static void CheckRelationMonthTiming(DateTime lastRefresh, DateTime now, List<TimingType> tasks)
        {
            foreach (var monthTasks in RelationMonthTimings)
            {
                DateTime markTime = now.Date.Add(monthTasks.Key.TSpan);
                //刷新时间只要大于上次刷新时间并且小于当前时间就可以刷新
                if (now.Day == monthTasks.Key.Day && lastRefresh < markTime && markTime <= now)
                {
                    foreach (var monthTask in monthTasks.Value)
                    {
                        if (!tasks.Contains(monthTask))
                        {
                            tasks.Add(monthTask);
                        }
                    }
                }
            }
        }

        private static void CheckWeekTiming(DateTime lastRefresh, DateTime now, List<TimingType> tasks)
        {
            foreach (var weekTasks in WeekTimings)
            {
                double totalDays = (now.Date - lastRefresh.Date).TotalDays;
                if (totalDays >= 7)
                {
                    //超过七天，直接刷新
                    foreach (var weekTask in weekTasks.Value)
                    {
                        if (!tasks.Contains(weekTask))
                        {
                            tasks.Add(weekTask);
                        }
                    }
                }
                else
                {
                    OneWeekCheckTime(lastRefresh, now, tasks, weekTasks);
                }
            }
        }


        private static void CheckRelationWeekTiming(DateTime lastRefresh, DateTime now, List<TimingType> tasks)
        {
            foreach (var weekTasks in RelationWeekTimings)
            {
                OneWeekCheckTime(lastRefresh, now, tasks, weekTasks);
            }
        }

        private static void OneWeekCheckTime(DateTime lastRefresh, DateTime now, List<TimingType> tasks, KeyValuePair<WeekTimeSpan, List<TimingType>> weekTasks)
        {
            //小于1周，计算刷新时间
            DateTime markTime;
            if (weekTasks.Key.Week == now.DayOfWeek)
            {
                //与当前时间同一天，获取当天的刷新时间
                markTime = now.Date.Add(weekTasks.Key.TSpan);
            }
            else if (weekTasks.Key.Week == lastRefresh.DayOfWeek)
            {
                //与上次刷新时间同一天，获取当天的刷新时间
                markTime = lastRefresh.Date.Add(weekTasks.Key.TSpan);
            }
            else
            {
                int day = 0;
                //不同星期
                if (weekTasks.Key.Week > lastRefresh.DayOfWeek)
                {
                    //说明刷新时间星期大
                    day = weekTasks.Key.Week - lastRefresh.DayOfWeek;
                }
                else
                {
                    //刷新时间星期小需要+7
                    day = weekTasks.Key.Week + 7 - lastRefresh.DayOfWeek;
                }
                //刷新日期
                markTime = lastRefresh.Date.AddDays(day);
                markTime = markTime.Add(weekTasks.Key.TSpan);
            }

            //刷新时间只要大于上次刷新时间并且小于当前时间就可以刷新
            if (lastRefresh < markTime && markTime <= now)
            {
                foreach (var weekTask in weekTasks.Value)
                {
                    if (!tasks.Contains(weekTask))
                    {
                        tasks.Add(weekTask);
                    }
                }
            }
        }

        private static void CheckDailyTiming(DateTime LastRefresh, DateTime now, List<TimingType> tasks)
        {
            //检查是否是隔天了
            TimeSpan span = now - LastRefresh;
            //查看时间间隔
            if (span.TotalDays >= 1)
            {
                //如果大于1，说明超过了24小时，每日刷新都应该执行
                foreach (var dailyTasks in DailyTimings)
                {
                    foreach (var dailyTask in dailyTasks.Value)
                    {
                        if (!tasks.Contains(dailyTask))
                        {
                            tasks.Add(dailyTask);
                        }
                    }
                }
            }
            else
            {
                OneDayCheckTime(DailyTimings, LastRefresh, now, tasks);
            }
        }

        private static void OneDayCheckTime(Dictionary<TimeSpan, List<TimingType>> dailyTimings,
            DateTime LastRefresh, DateTime now, List<TimingType> tasks)
        {
            if (LastRefresh.Date == now.Date)
            {
                //日期相同，上次是同一天刷新时间
                foreach (var dailyTasks in dailyTimings)
                {
                    //刷新时间只要大于上次刷新时间并且小于当前时间就可以刷新
                    if (LastRefresh.TimeOfDay < dailyTasks.Key && dailyTasks.Key <= now.TimeOfDay)
                    {
                        foreach (var dailyTask in dailyTasks.Value)
                        {
                            if (!tasks.Contains(dailyTask))
                            {
                                tasks.Add(dailyTask);
                            }
                        }
                    }
                }
            }
            else
            {
                //如果没大于1，说明0点前登录或者刷新过，
                foreach (var dailyTasks in dailyTimings)
                {
                    //刷新时间只要大于上次刷新时间或者小于当前时间就可以刷新
                    if (LastRefresh.TimeOfDay < dailyTasks.Key || dailyTasks.Key <= now.TimeOfDay)
                    {
                        foreach (var dailyTask in dailyTasks.Value)
                        {
                            if (!tasks.Contains(dailyTask))
                            {
                                tasks.Add(dailyTask);
                            }
                        }
                    }
                }
            }
        }

        private static void CheckRelationDailyTiming(DateTime LastRefresh, DateTime now, List<TimingType> tasks)
        {
            OneDayCheckTime(RelationDailyTimings, LastRefresh, now, tasks);
        }
    }
}
