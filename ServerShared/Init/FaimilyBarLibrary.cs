using DataProperty;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FamilyBarTime
    {
        public DayOfWeek Day;
        public int Hour;
        public int Min;
    }

    static public class FamilyBarLibrary
    {
        // key 开始时间 value 结束时间
        public static List<FamilyBarTime> StartTimeList = new List<FamilyBarTime>();
        public static int UpdatePeriod = 10;
        public static int DungeonId;
        public static int Duration = 20;
        public static Dictionary<int, int> RandomRewards = new Dictionary<int, int>();
        public static int RewardRate = 0;
        public static Dictionary<int, FamilyDungeonRewardModel> DungeonRewards = new Dictionary<int, FamilyDungeonRewardModel>();
        public static void Init()
        {
            DataList dataList = DataListManager.inst.GetDataList("FamilyBarConfig");
            Data data = dataList.Get(1);
            if (data == null)
            {
                Log.Warn("Init Family Bar Template failed: data is null");
                return;
            }
            // 加载开启时间
            StartTimeList.Clear();
            UpdatePeriod = data.GetInt("UpdatePeriod");
            DungeonId = data.GetInt("DungeonId");
            Duration = data.GetInt("Duration");
            string[] startTimeStrings = data.GetString("StartTime").Split('+');
            int count = startTimeStrings.Length;
            for (int i = 0; i < count; i++)
            {
                FamilyBarTime startTime = GetStartTime(startTimeStrings[i]);
                StartTimeList.Add(startTime);
            }

            // 加载随机奖励
            RandomRewards.Clear();
            string[] rewardStrings = data.GetString("Rewards").Split('|');
            foreach (var reward in rewardStrings)
            {
                string[] info = reward.Split(':');
                RandomRewards.Add(int.Parse(info[0]), int.Parse(info[1]));
                RewardRate += int.Parse(info[1]);
            }

            BindRewardData();
        }

        /// <summary>
        /// 家族副本奖励
        /// </summary>
        public static void BindRewardData()
        {
            DungeonRewards.Clear();
            FamilyDungeonRewardModel reward;

            DataList dataList = DataListManager.inst.GetDataList("FamilyDungeon");
            foreach (var data in dataList)
            {
                int dungeonId = data.Value.GetInt("DungeonId");
                int type = data.Value.GetInt("Type");
                string rewardString = data.Value.GetString("Reward");
                switch (type)
                {
                    case 1:
                        if (DungeonRewards.TryGetValue(dungeonId, out reward))
                        {
                            reward.DungeonId = dungeonId;
                            reward.KillReward = rewardString;
                        }
                        else
                        {
                            reward = new FamilyDungeonRewardModel();
                            reward.DungeonId = dungeonId;
                            reward.KillReward = rewardString;
                            DungeonRewards.Add(dungeonId, reward);
                        }
                        break;
                    case 2:
                        {
                            FamilyRewardItem item = new FamilyRewardItem();
                            item.Start = data.Value.GetInt("Num1");
                            item.End = data.Value.GetInt("Num2");
                            item.Reward = rewardString;
                            if (DungeonRewards.TryGetValue(dungeonId, out reward))
                            {
                                reward.PassReward.Add(item);
                            }
                            else
                            {
                                reward = new FamilyDungeonRewardModel();
                                reward.PassReward.Add(item);
                                DungeonRewards.Add(dungeonId, reward);
                            }
                        }
                        break;
                    case 3:
                        {
                            FamilyRewardItem item = new FamilyRewardItem();
                            item.Start = data.Value.GetInt("Num1");
                            item.End = data.Value.GetInt("Num2");
                            item.Reward = rewardString;
                            if (DungeonRewards.TryGetValue(dungeonId, out reward))
                            {
                                reward.ContributionReward.Add(item);
                            }
                            else
                            {
                                reward = new FamilyDungeonRewardModel();
                                reward.ContributionReward.Add(item);
                                DungeonRewards.Add(dungeonId, reward);
                            }
                        }
                        break;
                    case 4:
                        {
                            FamilyRewardItem item = new FamilyRewardItem();
                            item.Start = data.Value.GetInt("Num1");
                            //item.End = data.Value.GetInt("Num2");
                            item.Reward = rewardString;
                            if (DungeonRewards.TryGetValue(dungeonId, out reward))
                            {
                                reward.FirstKillReward[item.Start] = item;
                            }
                            else
                            {
                                reward = new FamilyDungeonRewardModel();
                                reward.FirstKillReward[item.Start] = item;
                                DungeonRewards.Add(dungeonId, reward);
                            }
                        }
                        break;
                    default:
                        return;
                }


            }
        }

        public static FamilyDungeonRewardModel GetDungeonRewards(int stage)
        {
            FamilyDungeonRewardModel rewards;
            DungeonRewards.TryGetValue(stage, out rewards);
            return rewards;
        }

        private static FamilyBarTime GetStartTime(string info)
        {
            FamilyBarTime time = new FamilyBarTime();
            string[] dayStrings = info.Split('|');
            time.Day = (DayOfWeek)((int.Parse(dayStrings[0])) % 7);
            string[] timeStrings = dayStrings[1].Split(':');
            time.Hour = int.Parse(timeStrings[0]);
            time.Min = int.Parse(timeStrings[1]);
            return time;
        }

        public static bool IsFamilyBarTime()
        {
            foreach (var time in StartTimeList)
            {
                if (IsFamilyBarTime(time) == true)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsFamilyBarTime(FamilyBarTime bar_start_time)
        {
            if (Api.now.DayOfWeek != bar_start_time.Day)
            {
                return false;
            }
            DateTime now = Api.now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, bar_start_time.Hour, bar_start_time.Min, 0);
            DateTime end = start.AddMinutes(Duration);
            return (start <= now && now <= end);
        }

        public static bool ReachFamilyBarStopTime()
        {
            foreach (var time in StartTimeList)
            {
                DateTime now = Api.now;
                DateTime end = new DateTime(now.Year, now.Month, now.Day, time.Hour, time.Min, 0).AddMinutes(Duration);
                if (now.Hour == end.Hour && now.Minute == end.Minute)
                {
                    return true;
                }
            }
            return false;
        }

        public static DateTime GetFamilyBarStartTime()
        {
            foreach (var time in StartTimeList)
            {
                DateTime now = Api.now;
                DateTime start = new DateTime(now.Year, now.Month, now.Day, time.Hour, time.Min, 0);
                DateTime end = start.AddMinutes(Duration);
                if (start <= now && now <= end)
                {
                    return start;
                }
            }
            return DateTime.MinValue;
        }

    }

}
