using DataProperty;
using EnumerateUtility.Timing;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public static class WishPoolLibrary
    {
        public static Dictionary<int, WishPoolItem1> wishPoolItem1s = new Dictionary<int, WishPoolItem1>();
        public static WishPoolItem2 stage2Item = null;

        public static SortedDictionary<int, WishPoolItem1> levelItems = new SortedDictionary<int, WishPoolItem1>();

        public static int EndDay = 1;

        public static TimeSpan stage2Update = new TimeSpan(5, 0, 0);

        public static void Init()
        {
            // wishPoolItem1s.Clear();
            // levelItems.Clear();

            DataList item1sDataList = DataListManager.inst.GetDataList("WishPoolItem1");

            Data item2 = DataListManager.inst.GetData("WishPoolItem2",1);

            InitItem1s(item1sDataList);

            InitItem2(item2);

            InitLevelItems();

            Data updateTimeData = DataListManager.inst.GetData("DailyTiming",(int)TimingType.WishPool);

            InitStage2Time(updateTimeData);
        }

        private static void InitStage2Time(Data updateTimeData)
        {
            stage2Update =TimeSpan.Parse(updateTimeData.GetString("Timing"));
        }

        private static void InitLevelItems()
        {
            SortedDictionary<int, WishPoolItem1> levelItems = new SortedDictionary<int, WishPoolItem1>();
            foreach(var item in wishPoolItem1s)
            {
                levelItems.Add(item.Value.Level, item.Value);
            }

            WishPoolLibrary.levelItems = levelItems;
        }

        private static void InitItem2(Data item2)
        {
            stage2Item = new WishPoolItem2();
            stage2Item.ObtainBase = item2.GetInt("ObtainBase");
            stage2Item.ObtainRule = item2.GetString("Obtain");
            stage2Item.OpenTimeRule = item2.GetString("OpenTime");
            stage2Item.Id = item2.ID;
            stage2Item.LimitDiamond = item2.GetInt("LimitDiamond");
            stage2Item.OverLimitObtain = item2.GetString("OverLimitObtain");
            stage2Item.Generate();
        }

        private static void InitItem1s(DataList item1sDataList)
        {
            Dictionary<int, WishPoolItem1> wishPoolItem1s = new Dictionary<int, WishPoolItem1>();
            foreach(var item in item1sDataList)
            {
                Data data = item.Value;
                WishPoolItem1 temp = new WishPoolItem1();
                temp.Consume = data.GetString("Consume");
                temp.Obtain = data.GetString("Obtain");
                temp.Level = data.GetInt("Level");
                if (data.GetInt("EndDay") > EndDay)
                {
                    EndDay = data.GetInt("EndDay");
                }
                temp.Id = data.ID;
                temp.Generate();
                wishPoolItem1s.Add(temp.Id, temp);
            }

            WishPoolLibrary.wishPoolItem1s = wishPoolItem1s;
        }

        public static WishPoolItem2 GetWishPoolItem2()
        {
            return stage2Item.Clone();
        }

        public static WishPoolItem1 GetWishPoolItem1(int level)
        {
            WishPoolItem1 item = null;
            levelItems.TryGetValue(level, out item);
            return item;
        }

        public static int GetMaxLevel4Stage1()
        {
            return levelItems.Keys.Max();
        }

        public static DateTime GetEndTime(DateTime serverStartTime)
        {
            DateTime temp = serverStartTime.Date;
            return temp.AddDays(EndDay);
        }

        public static DateTime GetTodayRefreshTime(DateTime now)
        {
            DateTime temp = now.Date;
            return temp.Add(stage2Update);
        }
    }
}
