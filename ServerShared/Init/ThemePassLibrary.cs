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
    public static class ThemePassLibrary
    {
        //key1: themeType, key2:rewardId
        private static Dictionary<int, SortedDictionary<int, ThemePassReward>> themePassRewardList = new Dictionary<int, SortedDictionary<int, ThemePassReward>>();
        //key1:passLevelXmlId, key2:passLevelId
        private static Dictionary<int, SortedDictionary<int, ThemePassLevel>> themePassLevelList = new Dictionary<int, SortedDictionary<int, ThemePassLevel>>();
        //key:themeType, value:passLevelXmlId
        private static Dictionary<int, int> passLevelMappingList = new Dictionary<int, int>();
        //key:themeType
        private static Dictionary<int, int> expItemList = new Dictionary<int, int>();

        public static int MaxPassLevel;
        public static int ExpRatio;
        public static int OverTimeRewardEmail;
        public static int PerEmailRewardCount;

        public static void Init()
        {
            InitThemePassConfig();

            InitThemePassReward();

            InitThemePassLevelXmls();

            InitThemePassLevelMapping();

            InitThemePassExpItem();
        }

        private static void InitThemePassConfig()
        {
            Data data = DataListManager.inst.GetData("ThemePassConfig", 1);
       
            MaxPassLevel = data.GetInt("MaxPassLevel");
            ExpRatio = data.GetInt("ExpRatio");
            OverTimeRewardEmail = data.GetInt("OverTimeRewardEmail");
            PerEmailRewardCount = data.GetInt("PerEmailRewardCount");
        }

        private static void InitThemePassReward()
        {
            Dictionary<int, SortedDictionary<int, ThemePassReward>> themePassRewardList = new Dictionary<int, SortedDictionary<int, ThemePassReward>>();
            //themePassRewardList.Clear();

            int themeType = 1;
            while (true)
            {
                DataList dataList = DataListManager.inst.GetDataList("ThemePassReward_" + themeType);
                if (dataList != null)
                {
                    InitThemeTypeThemePassReward(dataList, themeType, themePassRewardList);
                }
                else
                {
                    Logger.Log.Info($"ThemePassReward inited with max themeType {themeType - 1}");
                    break;
                }
                themeType++;
            }
            ThemePassLibrary.themePassRewardList = themePassRewardList;
        }

        private static void InitThemeTypeThemePassReward(DataList dataList, int themeType, Dictionary<int, SortedDictionary<int, ThemePassReward>> themePassRewardList)
        {
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                ThemePassReward item = new ThemePassReward();
                item.Id = data.ID;

                item.PassRewardType = data.GetInt("PassRewardType");
                item.Reward = data.GetString("Reward");
                item.AvailableLevel = data.GetInt("AvailableLevel");
                //item.IsLoop = data.GetInt("IsLoop");
                item.ThemeType = themeType;

                //if (item.IsLoop > 0 && item.AvailableLevel < dailyRewardLoopBegin)
                //{
                //    dailyRewardLoopBegin = item.AvailableLevel;
                //    dailyRewardLoopGap = item.LoopGap;

                //}
                SortedDictionary<int, ThemePassReward> dic;
                if (!themePassRewardList.TryGetValue(item.ThemeType, out dic))
                {
                    dic = new SortedDictionary<int, ThemePassReward>();
                    themePassRewardList.Add(item.ThemeType, dic);
                }
                dic.Add(item.Id, item);
            }
        }

        private static void InitThemePassLevelXmls()
        {
            Dictionary<int, SortedDictionary<int, ThemePassLevel>> themePassLevelList = new Dictionary<int, SortedDictionary<int, ThemePassLevel>>();
            //themePassLevelList.Clear();

            int passLevelXmlId = 1;
            while (true)
            {
                DataList dataList = DataListManager.inst.GetDataList("ThemePassLevel_" + passLevelXmlId);
                if (dataList != null)
                {
                    InitThemePassLevel(dataList, passLevelXmlId, themePassLevelList);
                }
                else
                {
                    Logger.Log.Info($"ThemePassLevel inited with max passLevelXmlId {passLevelXmlId - 1}");
                    break;
                }
                passLevelXmlId++;
            }
            ThemePassLibrary.themePassLevelList = themePassLevelList;
        }

        private static void InitThemePassLevel(DataList dataList, int passLevelXmlId, Dictionary<int, SortedDictionary<int, ThemePassLevel>> themePassLevelList)
        {
            int exp = 0;
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                ThemePassLevel item = new ThemePassLevel();
                item.Id = data.ID;
                item.Level = data.GetInt("Level");
                item.DeltaExp = data.GetInt("DeltaExp");
                exp += item.DeltaExp;
                item.Exp = exp;
                item.XmlId = passLevelXmlId;

                SortedDictionary<int, ThemePassLevel> dic;
                if (!themePassLevelList.TryGetValue(item.XmlId, out dic))
                {
                    dic = new SortedDictionary<int, ThemePassLevel>();
                    themePassLevelList.Add(item.XmlId, dic);
                }
                dic.Add(item.Level, item);
            }
        }

        private static void InitThemePassLevelMapping()
        {
            Dictionary<int, int> passLevelMappingList = new Dictionary<int, int>();
            //passLevelMappingList.Clear();

            DataList dataList = DataListManager.inst.GetDataList("ThemePassLevelMapping");

            foreach (var item in dataList)
            {
                Data data = item.Value;              
                int xmlId = data.GetInt("LevelXmlNameId");
                passLevelMappingList.Add(data.ID, xmlId);
            }
            ThemePassLibrary.passLevelMappingList = passLevelMappingList;
        }

        private static void InitThemePassExpItem()
        {
            Dictionary<int, int> expItemList = new Dictionary<int, int>();
            //expItemList.Clear();
            DataList dataList = DataListManager.inst.GetDataList("ThemePassExpItem");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int themeType = data.GetInt("ThemeType");
                expItemList.Add(data.ID, themeType);
            }
            ThemePassLibrary.expItemList = expItemList;
        }

        public static SortedDictionary<int, ThemePassLevel> GetThemePassLevelByThemeType(int themeType)
        {
            int passLevelXmlId;
            if (passLevelMappingList.TryGetValue(themeType, out passLevelXmlId))
            {
                SortedDictionary<int, ThemePassLevel> dic;
                themePassLevelList.TryGetValue(passLevelXmlId, out dic);
                return dic;
            }
            return null;
        }      

        public static List<string> GetAllLeftThemePassLevelReward(int themeType, int passLevel, SortedSet<int> rewardedSet, bool isSuper)
        {
            List<string> list = new List<string>();
            SortedDictionary<int, string> rewardDic = new SortedDictionary<int, string>();        
            int type = 1;
            if (isSuper)
            {
                type = 2;
            }

            SortedDictionary<int, ThemePassReward> dic;
            if (themePassRewardList.TryGetValue(themeType, out dic))
            {
                foreach (var kv in dic)
                {
                    if (kv.Value.AvailableLevel <= passLevel && kv.Value.PassRewardType == type && !rewardedSet.Contains(kv.Value.AvailableLevel))
                    {
                        rewardDic.Add(kv.Value.AvailableLevel, kv.Value.Reward);
                    }
                }
            }

            foreach (var item in rewardDic)
            {
                string reward = item.Value;
                list.Add(reward);
            }
            return list;
        }

        public static SortedSet<int> GetCurrentAllThemePassLevels(int themeType, int passLevel, bool isSuper)
        {
            List<string> list = new List<string>();
            SortedSet<int> set = new SortedSet<int>();
            int type = 1;
            if (isSuper)
            {
                type = 2;
            }

            SortedDictionary<int, ThemePassReward> dic;
            if (themePassRewardList.TryGetValue(themeType, out dic))
            {
                foreach (var kv in dic)
                {
                    if (kv.Value.AvailableLevel <= passLevel && kv.Value.PassRewardType == type)
                    {
                        set.Add(kv.Value.AvailableLevel);
                    }
                }
            }         
            return set;
        }

        public static string GetLevelReward(int themeType, int rewardLevel, bool isSuper)
        {
            string reward = "";  
            int type = 1;
            if (isSuper)
            {
                type = 2;
            }

            SortedDictionary<int, ThemePassReward> dic;
            if (themePassRewardList.TryGetValue(themeType, out dic))
            {
                foreach (var kv in dic)
                {
                    if (kv.Value.AvailableLevel == rewardLevel && kv.Value.PassRewardType == type)
                    {
                        reward = kv.Value.Reward;
                        break;
                    }
                }
            }
            return reward;
        }

        public static int GetThemePassTypeByItemId(int itemId)
        {
            int themeType;
            expItemList.TryGetValue(itemId, out themeType);
            return themeType;
        }

        public static bool CheckIsThemePassExpItem(int itemId)
        {
            if (expItemList.Keys.Contains(itemId))
            {
                return true;
            }
            return false;
        }

        public static int GetThemePassItemByType(int themeType)
        {
            foreach (var item in expItemList)
            {
                if (item.Value == themeType)
                {
                    return item.Key;
                }
            }
            return 0;
        }
    }
}
