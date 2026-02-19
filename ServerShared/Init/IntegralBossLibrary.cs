using CommonUtility;
using DataProperty;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class IntegralBossRefreshInfo
    {
        public TimeSpan PreOpenTime { get; private set; }
        public TimeSpan OpenTime { get; private set; }
        public TimeSpan StopTime { get; private set; }

        public IntegralBossRefreshInfo(TimeSpan openTime, int preTime, int lastMin)
        {
            OpenTime = openTime;
            StopTime = openTime.Add(new TimeSpan(0, lastMin, 0));
            PreOpenTime = openTime.Add(new TimeSpan(0, -preTime, 0));
        }
    }

    public class IntegralBossLibrary
    {
        public static int Lastime { get; private set; }//持续时长
        public static int PreStartTime { get; private set; }//预开启

        public static List<IntegralBossRefreshInfo> RefreshTimeList = new List<IntegralBossRefreshInfo>();

        public static void Init()
        {
            List<IntegralBossRefreshInfo> RefreshTimeList = new List<IntegralBossRefreshInfo>();

            //RefreshTimeList.Clear();
            Data data = DataListManager.inst.GetData("IntegralBossConfig", 1);
            Lastime = data.GetInt("Lastime");
            PreStartTime = data.GetInt("PreStartTime");

            TimeSpan startTime;
            string[] timeStr = data.GetString("Opentime").Split('|');
            foreach (var kv in timeStr)
            {
                string[] hourAndMinute = kv.Split(':');
                int hour = int.Parse(hourAndMinute[0]);
                int minute = int.Parse(hourAndMinute[1]);

                startTime = TimeSpan.FromHours(hour).Add(new TimeSpan(0, minute, 0));
                RefreshTimeList.Add(new IntegralBossRefreshInfo(startTime, PreStartTime, Lastime));
            }
            IntegralBossLibrary.RefreshTimeList = RefreshTimeList;
        }

        public static bool HaveBossPreOpen(DateTime time, ref IntegralBossRefreshInfo timeInfo)
        {
            timeInfo = RefreshTimeList.Where(x =>
            time.TimeOfDay.Ticks >= x.PreOpenTime.Ticks &&
            time.TimeOfDay.Ticks <= x.OpenTime.Ticks).FirstOrDefault();
            return timeInfo != null;
        }

        public static bool HaveBossOpening(DateTime time, ref IntegralBossRefreshInfo timeInfo)
        {
            timeInfo = RefreshTimeList.Where(x =>
            time.TimeOfDay.Ticks >= x.OpenTime.Ticks &&
            time.TimeOfDay.Ticks <= x.StopTime.Ticks).FirstOrDefault();
            return timeInfo != null;
        }

        public static bool GetNextOpenInfo(DateTime time, ref IntegralBossRefreshInfo timeInfo)
        {
            bool nextDay = false;
            foreach (var kv in RefreshTimeList)
            {
                if (time.TimeOfDay.Ticks <= kv.StopTime.Ticks)
                {
                    timeInfo = kv;
                    break;
                }
            }

            //当天最后一次挑战结束了，跳转到下一天第一次开启时间
            if (timeInfo == null)
            {
                nextDay = true;
                timeInfo = RefreshTimeList[0];
            }

            return nextDay;
        }
    }
}
