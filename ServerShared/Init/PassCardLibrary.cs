using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public static class PassCardLibrary
    {
        private static Dictionary<string, PassTask> taskList = new Dictionary<string, PassTask>();
        private static Dictionary<int, PassRechargeLevel> rechargeLevelList = new Dictionary<int, PassRechargeLevel>();
        private static Dictionary<string, PassReward> passLevelRewardList = new Dictionary<string, PassReward>();
        private static SortedDictionary<string, PassReward> passDailyRewardList = new SortedDictionary<string, PassReward>();
        //private static int MaxDailyRewardLevel = 0;
        private static SortedDictionary<int, PassLevel> passLevelList = new SortedDictionary<int, PassLevel>();

        private static Dictionary<int, Dictionary<int, PassTask>> periodTasks = new Dictionary<int, Dictionary<int, PassTask>>();
        private static Dictionary<int, SortedDictionary<int, PassReward>> periodPassLevelRewards = new Dictionary<int, SortedDictionary<int, PassReward>>();
        private static Dictionary<int, SortedDictionary<int, PassReward>> periodPassDailyRewards = new Dictionary<int, SortedDictionary<int, PassReward>>();
        private static SortedDictionary<int, PassLevel> eachLevelInfo = new SortedDictionary<int, PassLevel>();

        public static int PeriodDays = 30;

        public static int MaxPassLevel = 500;

        public static int CurPeriod = 1;

        public static int levelRewardLoopBegin = 100;
        public static int levelRewardLoopGap = 10;
        public static int dailyRewardLoopBegin = 100;
        public static int dailyRewardLoopGap = 10;

        public static bool DefaultBought = false;

        #region Inits

        public static void Init()
        {
            Data config = DataListManager.inst.GetData("PassConfig", 1);
            InitConst(config);

            DataList rechargeLevelDatas = DataListManager.inst.GetDataList("PassRechargeLevel");
            InitRechargeLevel(rechargeLevelDatas);

            DataList passLevelDatas = DataListManager.inst.GetDataList("PassLevel");
            InitPassLevel(passLevelDatas);

            Dictionary<string, PassReward> passLevelRewardList = new Dictionary<string, PassReward>();

            int periodCount = 1;
            while (true)
            {
                DataList passrewardDatas = DataListManager.inst.GetDataList("PassLevelReward_" + periodCount);
                if (passrewardDatas != null)
                {
                    InitPassLevelReward(passLevelRewardList, passrewardDatas, periodCount);
                }
                else
                {
                    Logger.Log.Info($"PassReward inited with max period {periodCount - 1}");
                    break;
                }
                periodCount++;
            }
            PassCardLibrary.passLevelRewardList = passLevelRewardList;

            Dictionary<string, PassTask> taskList = new Dictionary<string, PassTask>();

            periodCount = 1;
            while (true)
            {
                DataList passTaskDatas = DataListManager.inst.GetDataList("PassTask_" + periodCount);
                if (passTaskDatas != null)
                {
                    //InitPassTask(passTaskDatas, periodCount);
                    InitPassTaskNew(taskList, passTaskDatas, periodCount);
                }
                else
                {
                    Logger.Log.Info($"PassTask inited with max period {periodCount - 1}");
                    break;
                }
                periodCount++;
            }
            PassCardLibrary.taskList = taskList;

            SortedDictionary<string, PassReward> passDailyRewardList = new SortedDictionary<string, PassReward>();

            periodCount = 1;
            while (true)
            {
                DataList passTaskDatas = DataListManager.inst.GetDataList("PassDailyReward_" + periodCount);
                if (passTaskDatas != null)
                {
                    InitPassDailyReward(passDailyRewardList, passTaskDatas, periodCount);
                }
                else
                {
                    Logger.Log.Info($"PassDailyReward inited with max period {periodCount - 1}");
                    break;
                }
                periodCount++;
            }
            PassCardLibrary.passDailyRewardList = passDailyRewardList;

            InitPeriods();
            InitEachPassLevel();
        }

        private static void InitPeriods()
        {
            InitTaskPeriods();
            InitRewardPeriods();
            InitDailyRewardPeriods();
        }

        private static void InitTaskPeriods()
        {
            Dictionary<int, Dictionary<int, PassTask>> periodTasks = new Dictionary<int, Dictionary<int, PassTask>>();
            
            Dictionary<int, PassTask> dic = null;
            foreach (var kv in taskList)
            {
                if (!periodTasks.TryGetValue(kv.Value.PassPeriod, out dic))
                {
                    dic = new Dictionary<int, PassTask>();
                    periodTasks.Add(kv.Value.PassPeriod, dic);
                }

                dic.Add(kv.Value.Id, kv.Value);
            }

            PassCardLibrary.periodTasks = periodTasks;
        }

        private static void InitRewardPeriods()
        {
            Dictionary<int, SortedDictionary<int, PassReward>> periodPassLevelRewards = new Dictionary<int, SortedDictionary<int, PassReward>>();
            
            SortedDictionary<int, PassReward> dic = null;
            foreach (var kv in passLevelRewardList)
            {
                if (!periodPassLevelRewards.TryGetValue(kv.Value.PassPeriod, out dic))
                {
                    dic = new SortedDictionary<int, PassReward>();
                    periodPassLevelRewards.Add(kv.Value.PassPeriod, dic);
                }

                dic.Add(kv.Value.AvailableLevel + kv.Value.PassRewardType * 10000, kv.Value);
            }

            PassCardLibrary.periodPassLevelRewards = periodPassLevelRewards;
        }


        private static void InitDailyRewardPeriods()
        {
            SortedDictionary<int, PassReward> dic = null;
            Dictionary<int, SortedDictionary<int, PassReward>> periodPassDailyRewards = new Dictionary<int, SortedDictionary<int, PassReward>>();
            foreach (var kv in passDailyRewardList)
            {
                if (!periodPassDailyRewards.TryGetValue(kv.Value.PassPeriod, out dic))
                {
                    dic = new SortedDictionary<int, PassReward>();
                    periodPassDailyRewards.Add(kv.Value.PassPeriod, dic);
                    dic.Add(kv.Value.AvailableLevel, kv.Value);
                }
                else
                {
                    dic.Add(kv.Value.AvailableLevel, kv.Value);
                }
            }

            PassCardLibrary.periodPassDailyRewards = periodPassDailyRewards;
        }

        private static void InitConst(Data data)
        {
            PeriodDays = data.GetInt("PeriodDays");
            MaxPassLevel = data.GetInt("MaxPassLevel");
            DefaultBought = data.GetBoolean("DefaultBought");
        }

        private static void InitRechargeLevel(DataList datas)
        {
            Dictionary<int, PassRechargeLevel> rechargeLevelList = new Dictionary<int, PassRechargeLevel>();
            foreach (var kv in datas)
            {
                Data data = kv.Value;
                PassRechargeLevel item = new PassRechargeLevel();
                item.Id = data.ID;
                item.RechargeId = data.GetInt("RechargeId");
                item.AddLevel = data.GetInt("AddLevel");
                item.Reward = data.GetString("Reward");

                rechargeLevelList.Add(item.RechargeId, item);
            }

            PassCardLibrary.rechargeLevelList = rechargeLevelList;
        }

        private static void InitPassLevelReward(Dictionary<string, PassReward> passLevelRewardList, DataList datas, int period)
        { 
            foreach (var kv in datas)
            {
                Data data = kv.Value;
                PassReward item = new PassReward();
                item.Id = data.ID;

                item.PassRewardType = data.GetInt("PassRewardType");
                item.Reward = data.GetString("Reward");
                item.AvailableLevel = data.GetInt("AvailableLevel");
                item.IsLoop = data.GetInt("IsLoop");
                item.PassPeriod = period;
                if (item.IsLoop > 0 && item.AvailableLevel < levelRewardLoopBegin)
                {
                    levelRewardLoopBegin = item.AvailableLevel;
                    levelRewardLoopGap = item.LoopGap;
                }

                if (!string.IsNullOrWhiteSpace(item.Reward))
                {
                    passLevelRewardList.Add(item.PassPeriod + "_" + item.Id, item);
                }
            }

        }

        private static void InitPassDailyReward(SortedDictionary<string, PassReward> passDailyRewardList, DataList datas, int period)
        {
            foreach (var kv in datas)
            {
                Data data = kv.Value;
                PassReward item = new PassReward();
                item.Id = data.ID;

                item.PassRewardType = data.GetInt("PassRewardType");
                item.Reward = data.GetString("Reward");
                item.AvailableLevel = data.GetInt("AvailableLevel");
                item.IsLoop = data.GetInt("IsLoop");
                item.PassPeriod = period;

                if (item.IsLoop > 0 && item.AvailableLevel < dailyRewardLoopBegin)
                {
                    dailyRewardLoopBegin = item.AvailableLevel;
                    dailyRewardLoopGap = item.LoopGap;

                }
                if (!string.IsNullOrWhiteSpace(item.Reward))
                {
                    passDailyRewardList.Add(item.PassPeriod + "_" + item.Id, item);
                }
            }

        }

        //private static void InitPassTask(DataList datas, int period)
        //{
        //    foreach (var kv in datas)
        //    {
        //        Data data = kv.Value;
        //        PassTask item = new PassTask();
        //        item.Id = data.ID;
        //        item.PassTaskType = data.GetInt("PassTaskType");
        //        item.TaskParam = data.GetString("TaskParam");
        //        string[] param = item.TaskParam.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
        //        item.ParamNum = param[0].Split(':')[1].ToInt();
        //        if (param.Length > 1)
        //        {
        //            for (int i = 1; i < param.Length; i++)
        //            {
        //                string[] pair = param[i].Split(':');
        //                item.ParamChecks.Add(pair[0], pair[1].ToInt());
        //            }
        //        }
        //        item.LoopType = data.GetInt("LoopType");
        //        item.AvailableLevel = data.GetInt("AvailableLevel");
        //        //item.Exp = data.GetInt("Exp");
        //        item.Reward = data.GetString("Reward");
        //        item.PassPeriod = period;
        //        taskList.Add(item.PassPeriod + "_" + item.Id, item);
        //    }
        //}

        private static void InitPassTaskNew(Dictionary<string, PassTask> taskList, DataList datas, int period)
        {
            
            foreach (var kv in datas)
            {
                Data data = kv.Value;
                PassTask item = new PassTask();
                item.Id = data.ID;
                item.PassTaskType = data.GetInt("PassTaskType");
                item.TaskParam = data.GetString("TaskParam");
                string[] param = item.TaskParam.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                item.ParamNum = param[0].Split(':')[1].ToInt();
                if (param.Length > 1)
                {
                    for (int i = 1; i < param.Length; i++)
                    {
                        string[] pair = param[i].Split(':');
                        string[] nums = pair[1].Split('|');
                        HashSet<int> list = new HashSet<int>();
                        nums.ForEach(temp => list.Add(temp.ToInt()));

                        item.ParamChecksList.Add(pair[0], list);
                    }
                }
                item.LoopType = data.GetInt("LoopType");
                item.AvailableLevel = data.GetInt("AvailableLevel");
                //item.Exp = data.GetInt("Exp");
                item.Reward = data.GetString("Reward");
                item.PassPeriod = period;
                taskList.Add(item.PassPeriod + "_" + item.Id, item);
            }

        }

        private static void InitPassLevel(DataList datas)
        {
            SortedDictionary<int, PassLevel> passLevelList = new SortedDictionary<int, PassLevel>();
            foreach (var kv in datas)
            {
                Data data = kv.Value;
                PassLevel item = new PassLevel();
                item.Id = data.ID;
 
                item.Level = data.GetInt("Level");
                item.DeltaExp = data.GetInt("DeltaExp");

                passLevelList.Add(item.Level, item);
            }
            PassCardLibrary.passLevelList = passLevelList;
        }

        private static void InitEachPassLevel()
        {
            SortedDictionary<int, PassLevel> eachLevelInfo = new SortedDictionary<int, PassLevel>();
            PassLevel first = null;
            foreach (var kv in passLevelList)
            {
                //eachLevelInfo
                PassLevel second = kv.Value;
                if (first != null)
                {

                    for (int i = first.Level + 1; i <= second.Level; i++)
                    {
                        PassLevel item = new PassLevel();
                        item.Id = first.Id;
                        item.DeltaExp = second.DeltaExp / (second.Level - first.Level);
                        item.Level = i;
                        eachLevelInfo.Add(item.Level, item);
                    }
                }
                else
                {
                    for (int i = 1; i <= second.Level; i++)
                    {
                        PassLevel item = new PassLevel();
                        item.Id = 0;
                        if (i > 1)
                        {
                            item.DeltaExp = second.DeltaExp / (second.Level - 1);
                        }
                        item.Level = i;
                        eachLevelInfo.Add(item.Level, item);
                    }
                }

                first = kv.Value;
            }

            int exp = 0;
            foreach (var kv in eachLevelInfo)
            {
                if (kv.Value.Level == 1)
                {
                    continue;
                }
                exp += kv.Value.DeltaExp;
                kv.Value.Exp = exp;
            }
            PassCardLibrary.eachLevelInfo = eachLevelInfo;
        }

        #endregion

        #region publics

        public static SortedSet<int> GetAllRewardLevelUnderOrEqual(int passcardLevel, bool isSuper)
        {
            SortedSet<int> set = new SortedSet<int>();
            int type = 1;
            if (isSuper)
            {
                type = 2;
            }

            SortedDictionary<int, PassReward> dic = null;

            if (periodPassLevelRewards.TryGetValue(CurPeriod, out dic))
            {
                foreach (var kv in dic)
                {
                    if (kv.Value.AvailableLevel <= passcardLevel && kv.Value.PassRewardType == type)
                    {
                        set.Add(kv.Value.AvailableLevel);
                    }
                }
            }
            return set;
        }

        public static List<string> GetLeftLevelRewardUnderOrEqual(SortedSet<int> rewarded, int passcardLevel, bool isSuper)
        {
            SortedSet<int> set = new SortedSet<int>();
            List<string> list = new List<string>();
            int type = 1;
            if (isSuper)
            {
                type = 2;
            }

            SortedDictionary<int, PassReward> dic = null;

            if (periodPassLevelRewards.TryGetValue(CurPeriod, out dic))
            {
                foreach (var kv in dic)
                {
                    if (kv.Value.AvailableLevel <= passcardLevel && kv.Value.PassRewardType == type && !rewarded.Contains(kv.Value.AvailableLevel))
                    {
                        set.Add(kv.Value.AvailableLevel);
                    }
                }
            }

            foreach (var item in set)
            {
                string reward = dic[item + 10000 * type].Reward;
                list.Add(reward);
            }

            return list;
        }

        public static SortedSet<int> GetLeftRewardLevelUnderOrEqual(SortedSet<int> rewarded, int passcardLevel, bool isSuper)
        {
            SortedSet<int> set = new SortedSet<int>();
            int type = 1;
            if (isSuper)
            {
                type = 2;
            }

            SortedDictionary<int, PassReward> dic = null;

            if (periodPassLevelRewards.TryGetValue(CurPeriod, out dic))
            {
                foreach (var kv in dic)
                {
                    if (kv.Value.AvailableLevel <= passcardLevel && kv.Value.PassRewardType == type && !rewarded.Contains(kv.Value.AvailableLevel))
                    {
                        set.Add(kv.Value.AvailableLevel);
                    }
                }
            }
            return set;
        }

        public static string GetLevelReward(int passcardLevel, bool isSuper)
        {
            SortedDictionary<int, PassReward> rewards = null;
            string reward = "";
            int tempLevel = 0;
            int type = 1;
            if (isSuper)
            {
                type = 2;
            }
            if (periodPassLevelRewards.TryGetValue(CurPeriod, out rewards))
            {
                foreach (var kv in rewards)
                {
                    if (kv.Key % 10000 == passcardLevel && kv.Value.PassRewardType == type)
                    {
                        tempLevel = passcardLevel;
                        reward = kv.Value.Reward;
                        break;
                    }
                }
            }
            return reward;
        }

        public static string GetDailyRewardString(int passcardLevel)
        {
            SortedDictionary<int, PassReward> rewards = null;
            string reward = "";
            int tempLevel = 0;
            if (periodPassDailyRewards.TryGetValue(CurPeriod, out rewards))
            {
                foreach (var kv in rewards)
                {
                    if (kv.Key <= passcardLevel)
                    {
                        tempLevel = kv.Key;
                        reward = kv.Value.Reward;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return reward;
        }

        public static int GetRechargeLevelExp(int passcardLevel, int addLevel)
        {
            int exp = 0;
            PassLevel info = null;
            for (int i = passcardLevel + 1; i <= passcardLevel + addLevel; i++)
            {
                if (eachLevelInfo.TryGetValue(i, out info))
                {
                    exp += info.DeltaExp;
                }
                else
                {
                    break;
                }
            }
            return exp;
        }

        public static PassRechargeLevel GetRechargeLevel(int rechargeId)
        {
            PassRechargeLevel info = null;
            rechargeLevelList.TryGetValue(rechargeId, out info);
            return info;
        }

        public static int GetCurLevel(int exp, int curLevel)
        {
            PassLevel temp = null;
            int level = curLevel;
            for (int i = curLevel; ; i++)
            {
                if (!eachLevelInfo.TryGetValue(i, out temp))
                {
                    break;
                }
                if (exp < temp.Exp)
                {
                    break;
                }
                level++;
            }
            if (level >= MaxPassLevel)
            {
                return MaxPassLevel;
            }
            return level > curLevel ? level - 1 : curLevel;
        }

        public static int GetMaxLevelExp()
        {
            return eachLevelInfo.Last().Value.Exp;
        }

        public static PassTask GetTask(int taskId)
        {
            PassTask temp = null;
            Dictionary<int, PassTask> dic = null;
            if (periodTasks.TryGetValue(CurPeriod, out dic) && dic.TryGetValue(taskId, out temp))
            {

            }
            else
            {
                //Logger.Log.Warn($"try get taskId {taskId} with null {CurPeriod} count period {periodTasks.Count} dic {dic?.Count}");
            }
            return temp;
        }

        public static IReadOnlyDictionary<int, PassTask> GetCurPeriodTasks()
        {
            Dictionary<int, PassTask> dic = null;
            periodTasks.TryGetValue(CurPeriod, out dic);
            return dic;
        }


        public static int GetPeriod()
        {
            return CurPeriod;
        }

        //每次passDay检查
        public static int CheckPeriod(DateTime open)
        {
            DateTime now = DateTime.Now;
            int days = (now - open.Date).Days;
            int period = days / PeriodDays + 1;
            CurPeriod = period;
            return period;
        }

        public static bool CheckPeriodUpdate(DateTime open, DateTime now)
        {
            int days = (now - open.Date).Days;
            int period = days / PeriodDays + 1;
            if (CurPeriod != period)
            {
                CurPeriod = period;
                return true;
            }
            return false;
        }

        public static DateTime GetEndTime(DateTime open)
        {
            int days = PeriodDays * CurPeriod;
            return open.Date.AddDays(days);
        }
        #endregion
    }
}
