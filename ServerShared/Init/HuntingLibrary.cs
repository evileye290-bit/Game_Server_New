using System;
using CommonUtility;
using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class HuntingLibrary
    {
        private static Dictionary<int, HuntingModel> huntingList = new Dictionary<int, HuntingModel>();
        private static Dictionary<int, HuntingModel> huntingListIndexByMapId = new Dictionary<int, HuntingModel>();
        private static Dictionary<int, float> huntingDungeonGrowth = new Dictionary<int, float>();

        private static Dictionary<int, HuntingActivityModel> huntingActivityList = new Dictionary<int, HuntingActivityModel>();
        private static Dictionary<int, HuntingActivityModel> huntingActivityListIndexByMapId = new Dictionary<int, HuntingActivityModel>();

        private static int HuntingIntrudeWeightSum;
        private static DoubleDepthMap<int, int, HuntingBuffSuitModel> huntingBuffSuitList = new DoubleDepthMap<int, int, HuntingBuffSuitModel>();
        private static Dictionary<int, HuntingIntrudeModel> HuntingIntrudeList = new Dictionary<int, HuntingIntrudeModel>();
        private static Dictionary<int, HuntingIntrudeBuffSuitModel> HuntingIntrudeBuffSuitList = new Dictionary<int, HuntingIntrudeBuffSuitModel>();

        private static int HuntingIntrudeRewardWeightSum;
        private static Dictionary<int, List<ItemBasicInfo>> HuntingIntrudeRewardList = new Dictionary<int, List<ItemBasicInfo>>();


        public static int ResearchMax { get; private set; }
        public static int ResearchEasy { get; private set; }
        public static int ResearchHard { get; private set; }
        public static int ResearchDevil { get; private set; }
        public static int MaxHuntingCount { get; private set; }
        public static int OfflineBrotherEmailId { get; private set; }
        public static float Discount { get; private set; }
        public static int SweepItem { get; private set; }
        public static int OfflineHelpEmailId { get; private set; }
        public static string OfflineHelpReward { get; private set; }

        public static int MaxHuntingResearch { get; private set; }

        public static ItemBasicInfo UnlockSweepItem { get; private set; }

        public static DateTime HuntingBuffStartTime { get; private set; }
        /// <summary>
        /// 入侵数量
        /// </summary>
        public static int HuntingIntrudeNum { get; private set; }
        public static int HuntingIntrudeResearchLimit { get; private set; }
        public static int HuntingIntrudeExistHour { get; private set; }
        public static int HuntingIntrudeProbability { get; private set; }
        public static int PeriodBuffResearchLimit { get; private set; }

        public static void Init()
        {
            InitConfig();
            InitHuntingBase();
            InitHuntingDungeonGrowth();

            InitHuntingActivity();

            InitHuntingBuffSuit();
            InitHuntingIntrude();
            InitHuntingIntrudeBuffSuit();
            InitHuntingIntrudeReward();
        }

        private static void InitHuntingBase()
        {
            Dictionary<int, HuntingModel> huntingList = new Dictionary<int, HuntingModel>();
            Dictionary<int, HuntingModel> huntingListIndexByMapId = new Dictionary<int, HuntingModel>();

            Data data;
            HuntingModel model = null;
            DataList dataList = DataListManager.inst.GetDataList("HuntingBase");
            int count = 0;
            foreach (var item in dataList)
            {
                data = item.Value;
                model = new HuntingModel(data);
                huntingList.Add(item.Key, model);
                AddToMapIndex(huntingListIndexByMapId, model);
                count++;
            }
            MaxHuntingCount = count;
            HuntingLibrary.huntingList = huntingList;
            HuntingLibrary.huntingListIndexByMapId = huntingListIndexByMapId;
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("Hunting", 1);
            ResearchEasy = data.GetInt("ResearchEasy");
            ResearchHard = data.GetInt("ResearchHard");
            ResearchDevil = data.GetInt("ResearchDevil");
            ResearchMax = data.GetInt("Research");
            OfflineBrotherEmailId = data.GetInt("OfflineBrotherEmailId");
            Discount = data.GetFloat("Discount");
            SweepItem = data.GetInt("SweepItem");
            OfflineHelpEmailId = data.GetInt("OfflineHelpEmailId");
            OfflineHelpReward = data.GetString("OfflineHelpReward");
            UnlockSweepItem = ItemBasicInfo.Parse(data.GetString("UnlockSweepItem"));

            HuntingBuffStartTime = DateTime.Parse(data.GetString("HuntingBuffStartTime"));
            HuntingIntrudeNum = data.GetInt("HuntingIntrudeNum");
            HuntingIntrudeResearchLimit = data.GetInt("HuntingIntrudeResearchLimit");
            HuntingIntrudeExistHour = data.GetInt("HuntingIntrudeExistHour");
            HuntingIntrudeProbability = data.GetInt("HuntingIntrudeProbability");
            PeriodBuffResearchLimit = data.GetInt("PeriodBuffResearchLimit");
        }

        private static void InitHuntingDungeonGrowth()
        {
            Dictionary<int, float> huntingDungeonGrowth = new Dictionary<int, float>();
            DataList dataList = DataListManager.inst.GetDataList("HuntingDungeonGrowth");
            foreach (var item in dataList)
            {
                huntingDungeonGrowth[item.Value.ID] = item.Value.GetFloat("Growth");
            }
            HuntingLibrary.huntingDungeonGrowth = huntingDungeonGrowth;
            MaxHuntingResearch = huntingDungeonGrowth.Keys.Max();
        }

        public static float GetGrowth(int research)
        {
            float growth = 1.0f;
            research = Math.Min(MaxHuntingResearch, research);
            huntingDungeonGrowth.TryGetValue(research, out growth);
            return growth;
        }

        public static HuntingModel Get(int id)
        {
            HuntingModel model = null;
            huntingList.TryGetValue(id, out model);
            return model;
        }

        public static HuntingModel GetByMapId(int mapId)
        {
            HuntingModel model = null;
            huntingListIndexByMapId.TryGetValue(mapId, out model);
            return model;
        }

        public static int GetResearch(DungeonDifficulty diff)
        {
            switch (diff)
            {
                case DungeonDifficulty.Easy: return ResearchEasy;
                case DungeonDifficulty.Hard: return ResearchHard;
                case DungeonDifficulty.Devil: return ResearchDevil;
            }
            return 0;
        }

        private static void AddToMapIndex(Dictionary<int, HuntingModel> huntingListIndexByMapId, HuntingModel model)
        {
            huntingListIndexByMapId.Add(model.EasyMapId, model);
            huntingListIndexByMapId.Add(model.HardMapId, model);
            huntingListIndexByMapId.Add(model.DevilMapId, model);
        }

        #region 凶兽森林-猎杀魂兽

        private static void InitHuntingActivity()
        {
            Dictionary<int, HuntingActivityModel> huntingActivityList = new Dictionary<int, HuntingActivityModel>();
            Dictionary<int, HuntingActivityModel> huntingActivityListIndexByMapId = new Dictionary<int, HuntingActivityModel>();

            Data data;
            HuntingActivityModel model = null;
            DataList dataList = DataListManager.inst.GetDataList("HuntingActivity");
            foreach (var item in dataList)
            {
                data = item.Value;
                model = new HuntingActivityModel(data);
                huntingActivityList.Add(item.Key, model);
                huntingActivityListIndexByMapId.Add(model.SingleMapId, model);
                huntingActivityListIndexByMapId.Add(model.TeamMapId, model);
            }
            HuntingLibrary.huntingActivityList = huntingActivityList;
            HuntingLibrary.huntingActivityListIndexByMapId = huntingActivityListIndexByMapId;
        }

        public static HuntingActivityModel GetHuntingActivityModel(int id)
        {
            HuntingActivityModel model = null;
            huntingActivityList.TryGetValue(id, out model);
            return model;
        }

        public static HuntingActivityModel GetHuntingActivityModelByMapId(int id)
        {
            HuntingActivityModel model = null;
            huntingActivityListIndexByMapId.TryGetValue(id, out model);
            return model;
        }

        public static bool IsActivityDungeon(int dungeonId)
        {
            return huntingActivityListIndexByMapId.ContainsKey(dungeonId);
        }

        #endregion

        private static void InitHuntingBuffSuit()
        {
            DoubleDepthMap<int, int, HuntingBuffSuitModel> huntingBuffSuitList = new DoubleDepthMap<int, int, HuntingBuffSuitModel>();

            DataList dataList = DataListManager.inst.GetDataList("HuntingBuffSuit");
            foreach (var item in dataList)
            {
                HuntingBuffSuitModel model = new HuntingBuffSuitModel(item.Value);
                huntingBuffSuitList.Add(model.Week, model.DungeonId, model);
            }
            HuntingLibrary.huntingBuffSuitList = huntingBuffSuitList;
        }

        private static void InitHuntingIntrude()
        {
            Dictionary<int, HuntingIntrudeModel> huntingIntrudelList = new Dictionary<int, HuntingIntrudeModel>();

            DataList dataList = DataListManager.inst.GetDataList("HuntingIntrude");
            foreach (var item in dataList)
            {
                HuntingIntrudeModel model = new HuntingIntrudeModel(item.Value);
                huntingIntrudelList.Add(item.Key, model);
            }
            HuntingIntrudeWeightSum = huntingIntrudelList.Values.Sum(x => x.Weight);
            HuntingLibrary.HuntingIntrudeList = huntingIntrudelList;
        }

        private static void InitHuntingIntrudeBuffSuit()
        {
            Dictionary<int, HuntingIntrudeBuffSuitModel> list = new Dictionary<int, HuntingIntrudeBuffSuitModel>();

            DataList dataList = DataListManager.inst.GetDataList("HuntingIntrudeBuffSuit");
            foreach (var item in dataList)
            {
                HuntingIntrudeBuffSuitModel model = new HuntingIntrudeBuffSuitModel(item.Value);
                list.Add(item.Key, model);
            }
            HuntingLibrary.HuntingIntrudeBuffSuitList = list;
        }

        private static void InitHuntingIntrudeReward()
        {
            HuntingIntrudeRewardWeightSum = 0;
            Dictionary<int, List<ItemBasicInfo>> list = new Dictionary<int, List<ItemBasicInfo>>();

            DataList dataList = DataListManager.inst.GetDataList("HuntingIntrudeReward");
            foreach (var item in dataList)
            {
                int weight = item.Value.GetInt("Weight");
                HuntingIntrudeRewardWeightSum += weight;
                list.Add(HuntingIntrudeRewardWeightSum, RewardDropLibrary.GetSimpleRewards(item.Value.GetString("Reward")));
            }
            HuntingLibrary.HuntingIntrudeRewardList = list;
        }

        public static int GetWeekIndex(DateTime time)
        {
            int week = (time.Date - HuntingBuffStartTime.Date).Days / 7 + 1;
            int index = week % huntingBuffSuitList.Count;
            if (index == 0)
            {
                index = huntingBuffSuitList.Count;
            }
            return index;
        }

        public static HuntingBuffSuitModel GetHuntingBuffSuit(int index, int dungeonId)
        {
            HuntingBuffSuitModel model;
            huntingBuffSuitList.TryGetValue(index, dungeonId, out model);
            return model;
        }

        public static HuntingIntrudeModel GetHuntingIntrudeModel()
        {
            int weight = RAND.Range(1, HuntingIntrudeWeightSum);
            foreach (var kv in HuntingIntrudeList)
            {
                if (weight <= kv.Value.Weight) return kv.Value;
                weight -= kv.Value.Weight;
            }
            return null;
        }

        public static HuntingIntrudeModel GetHuntingIntrudeModel(int id)
        {
            HuntingIntrudeModel model;
            HuntingIntrudeList.TryGetValue(id, out model);
            return model;
        }

        public static void RandomHuntingIntrude(out HuntingIntrudeModel model, out HuntingIntrudeBuffSuitModel buffSuitModel)
        {
            buffSuitModel = null;
            model = GetHuntingIntrudeModel();
            if(model == null) return;

            buffSuitModel = RandomIntrudeBuffSuitModel(model.BuffList);
        }

        public static HuntingIntrudeBuffSuitModel RandomIntrudeBuffSuitModel(List<int> buffIds)
        {
            var ienum = buffIds.Where(x => HuntingIntrudeBuffSuitList.ContainsKey(x)).Select(x => HuntingIntrudeBuffSuitList[x]).ToList();
            int weightSum = ienum.Sum(x => x.Weight);

            int weight = RAND.Range(1, weightSum);

            foreach (var kv in ienum)
            {
                if (weight <= kv.Weight) return kv;
                weight -= kv.Weight;
            }
            return null;
        }

        public static HuntingIntrudeBuffSuitModel GetIntrudeBuffSuitModel(int id)
        {
            HuntingIntrudeBuffSuitModel model;
            HuntingIntrudeBuffSuitList.TryGetValue(id, out model);
            return model;
        }

        public static List<ItemBasicInfo> RandomHuntingIntrudeReward()
        {
            int weight = RAND.Range(1, HuntingIntrudeRewardWeightSum);

            return HuntingIntrudeRewardList.FirstOrDefault(x => weight <= x.Key).Value ?? new List<ItemBasicInfo>();
        }
    }
}
