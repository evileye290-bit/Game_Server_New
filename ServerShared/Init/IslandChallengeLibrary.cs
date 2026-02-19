using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using DataProperty;
using EnumerateUtility;
using ServerModels;

namespace ServerShared
{
    public class IslandChallengeLibrary
    {
        private static Dictionary<int, IslandChallengeRewardModel> towerList = new Dictionary<int, IslandChallengeRewardModel>();

        private static int MaxGroupWeight = 0;
        private static Dictionary<int, int> groupWeight = new Dictionary<int, int>();
        private static Dictionary<int, int> groupMaxNodeId = new Dictionary<int, int>();

        //groupid,nodeid,
        private static Dictionary<int, IslandChallengeTaskModel> taskList = new Dictionary<int, IslandChallengeTaskModel>();
        private static DoubleDepthMap<int, int, IslandChallengeTaskModel> groupNodeTask = new DoubleDepthMap<int, int, IslandChallengeTaskModel>();

        //shop
        private static int MaxShopItemTypeWeight;
        private static Dictionary<TowerShopItemType, int> TowerShopItemTypeLimit = new Dictionary<TowerShopItemType, int>();
        private static Dictionary<TowerShopItemType, int> TowerShopItemTypeWeight = new Dictionary<TowerShopItemType, int>();
        private static Dictionary<int, IslandChallengeShopItemModel> TowerShopItemList = new Dictionary<int, IslandChallengeShopItemModel>();
        private static Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>> TowerTypeShopItemList = new Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>>();

        //dungeon
        private static Dictionary<int, int> dungeonWeight = new Dictionary<int, int>();//不同难度副本的总权重
        private static Dictionary<int, IslandChallengeDungeonModel> DungeonList = new Dictionary<int, IslandChallengeDungeonModel>();
        private static DoubleDepthMap<int, int, IslandChallengeDungeonModel> DifficutyDungeonList = new DoubleDepthMap<int, int, IslandChallengeDungeonModel>();

        private static Dictionary<int, int> buyReviveCost = new Dictionary<int, int>();

        public static int ShopItemCount { get; private set; }
        public static int OpenDays { get; private set; }
        public static int CloseDays { get; private set; }
        public static int MaxNode { get; private set; }
        public static int FirstBattlePower { get; private set; }//第一期开启时候战斗力
        public static int FirstHeroLevel { get; private set; }

        public static void Init()
        {
            MaxGroupWeight = 0;
            MaxShopItemTypeWeight = 0;

            InitConfig();
            InitRewardInfo();
            InitGroup();
            InitTask();
            InitShop();
            InitDungeon();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("IslandChallengeConfig", 1);
            OpenDays = data.GetInt("OpenDays");
            CloseDays = data.GetInt("CloseDays");
            ShopItemCount = data.GetInt("ShopItemCount");
            FirstBattlePower = data.GetInt("FirstBattlePower");
            FirstHeroLevel = data.GetInt("FirstHeroLevel");

            //1:100|3:200|6:1000，表示，第1、2次购买100钻、第3、4、5次购买200钻，第6次以上1000钻
            Dictionary<int, int> countCost = StringSplit.GetKVPairs(data.GetString("RevivePrice"));

            buyReviveCost = countCost.OrderByDescending(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public static int GetReviveCost(int count)
        {
            foreach (var kv in buyReviveCost)
            {
                if (count >= kv.Key)
                {
                    return kv.Value;
                }
            }
            return buyReviveCost.First().Value;
        }

        #region reward
        private static void InitRewardInfo()
        {
            IslandChallengeRewardModel model;
            Dictionary<int, IslandChallengeRewardModel> towerList = new Dictionary<int, IslandChallengeRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("IslandChallengeNodeReward");
            foreach (var kv in dataList)
            {
                model = new IslandChallengeRewardModel(kv.Value);
                towerList.Add(model.Id, model);
            }

            IslandChallengeLibrary.towerList = towerList;
        }

        public static IslandChallengeRewardModel GetIslandChallengeRewardModel(int id)
        {
            IslandChallengeRewardModel model;
            towerList.TryGetValue(id, out model);
            return model;
        }

        #endregion

        #region group

        private static void InitGroup()
        {
            Dictionary<int, int> groupWeight = new Dictionary<int, int>();

            MaxGroupWeight = 0;

            DataList dataList = DataListManager.inst.GetDataList("IslandChallengeGroup");
            foreach (var kv in dataList)
            {
                MaxGroupWeight += kv.Value.GetInt("Weight");
                groupWeight[kv.Value.GetInt("GroupId")] = MaxGroupWeight;
            }
            IslandChallengeLibrary.groupWeight = groupWeight;
        }

        public static int RandomGroup()
        {
            int ratio = RAND.Range(0, MaxGroupWeight - 1);
            int groupId = groupWeight.Where(x => ratio < x.Value).First().Key;
            return groupId;
        }

        #endregion

        #region task

        private static void InitTask()
        {
            Dictionary<int, int> groupMaxNodeId = new Dictionary<int, int>();
            Dictionary<int, IslandChallengeTaskModel> taskList = new Dictionary<int, IslandChallengeTaskModel>();
            DoubleDepthMap<int, int, IslandChallengeTaskModel> groupNodeTask = new DoubleDepthMap<int, int, IslandChallengeTaskModel>();

            IslandChallengeTaskModel model;
            DataList dataList = DataListManager.inst.GetDataList("IslandChallengeTask");
            foreach (var kv in dataList)
            {
                model = new IslandChallengeTaskModel(kv.Value);

                if (!groupMaxNodeId.ContainsKey(model.GroupId))
                {
                    groupMaxNodeId[model.GroupId] = model.NodeId;
                }
                else 
                { 
                    groupMaxNodeId[model.GroupId] = Math.Max(model.NodeId, groupMaxNodeId[model.GroupId]);
                }

                MaxNode = Math.Max(MaxNode, model.NodeId);

                taskList[model.Id] = model;
                groupNodeTask.Add(model.GroupId, model.NodeId, model);
            }

            IslandChallengeLibrary.taskList = taskList;
            IslandChallengeLibrary.groupNodeTask = groupNodeTask;
            IslandChallengeLibrary.groupMaxNodeId = groupMaxNodeId;
        }

        public static IslandChallengeTaskModel GetIslandChallengeTaskModel(int id)
        {
            IslandChallengeTaskModel model;
            taskList.TryGetValue(id, out model);
            return model;
        }

        public static Dictionary<int, IslandChallengeTaskModel> GetGroupTasks(int groupId)
        {
            Dictionary<int, IslandChallengeTaskModel> nodeTask;
            groupNodeTask.TryGetValue(groupId, out nodeTask);
            return nodeTask;
        }

        public static int GetMaxNodeId(int group)
        {
            int nodeId;
            groupMaxNodeId.TryGetValue(group, out nodeId);
            return nodeId;
        }

        #endregion

        #region dungeon

        private static void InitDungeon()
        {
            Dictionary<int, int> dungeonWeight = new Dictionary<int, int>();
            Dictionary<int, IslandChallengeDungeonModel> DungeonList = new Dictionary<int, IslandChallengeDungeonModel>();
            DoubleDepthMap<int, int, IslandChallengeDungeonModel> DifficutyDungeonList = new DoubleDepthMap<int, int, IslandChallengeDungeonModel>();

            IslandChallengeDungeonModel model;
            DataList dataList = DataListManager.inst.GetDataList("IslandChallengeDungeon");
            foreach (var kv in dataList)
            {
                model = new IslandChallengeDungeonModel(kv.Value);

                DungeonList[model.Id] = model;

                if (!dungeonWeight.ContainsKey(model.Difficulty))
                {
                    dungeonWeight[model.Difficulty] = model.Weight;
                }
                else
                { 
                    dungeonWeight[model.Difficulty] += model.Weight;
                }

                DifficutyDungeonList.Add(model.Difficulty, model.Id, model);
            }
            IslandChallengeLibrary.dungeonWeight = dungeonWeight;
            IslandChallengeLibrary.DungeonList = DungeonList;
            IslandChallengeLibrary.DifficutyDungeonList = DifficutyDungeonList;
        }

        public static IslandChallengeDungeonModel GetIslandChallengeDungeonModel(int id)
        {
            IslandChallengeDungeonModel model;
            DungeonList.TryGetValue(id, out model);
            return model;
        }

        public static IslandChallengeDungeonModel RandomDungeon(int difficulty)
        {
            int maxWeight;
            if (!dungeonWeight.TryGetValue(difficulty, out maxWeight))
            {
                return null;
            }

            int ratio = RAND.Range(0, maxWeight - 1);
            foreach (var kv in DifficutyDungeonList[difficulty])
            {
                if (ratio < kv.Value.Weight) return kv.Value;
                ratio -= kv.Value.Weight;
            }

            return null;
        }

        #endregion

        #region shop
        private static void InitShop()
        {
            Dictionary<TowerShopItemType, int> TowerShopItemTypeLimit = new Dictionary<TowerShopItemType, int>();
            Dictionary<TowerShopItemType, int> TowerShopItemTypeWeight = new Dictionary<TowerShopItemType, int>();
            Dictionary<int, IslandChallengeShopItemModel> TowerShopItemList = new Dictionary<int, IslandChallengeShopItemModel>();
            Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>> TowerTypeShopItemList = new Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>>();

            DataList dataList = DataListManager.inst.GetDataList("IslandChallengeShopItemTypeRandom");
            foreach (var kv in dataList)
            {
                TowerShopItemType type = (TowerShopItemType)kv.Value.GetInt("Type");
                TowerShopItemTypeWeight[type] = kv.Value.GetInt("Weight");
                TowerShopItemTypeLimit[type] = kv.Value.GetInt("TaskLimit");
            }

            dataList = DataListManager.inst.GetDataList("IslandChallengeShopItem");
            foreach (var kv in dataList)
            {
                IslandChallengeShopItemModel model = new IslandChallengeShopItemModel(kv.Value);
                TowerShopItemList.Add(model.Id, model);

                Dictionary<TowerShopItemQuality, TowerShopItemQualityItems> modelist;
                if (!TowerTypeShopItemList.TryGetValue(model.Type, out modelist))
                {
                    modelist = new Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>();
                    TowerTypeShopItemList[model.Type] = modelist;
                }

                bool added = false;
                foreach (var item in modelist)
                {
                    if (model.QualityMin == item.Key.Min && model.QualityMax == item.Key.Max)
                    {
                        added = true;
                        item.Value.AddItem(model);
                    }
                }
                if (!added)
                {
                    modelist.Add(new TowerShopItemQuality() { Min = model.QualityMin, Max = model.QualityMax }, new TowerShopItemQualityItems(model));
                }
            }
            IslandChallengeLibrary.TowerShopItemTypeLimit = TowerShopItemTypeLimit;
            IslandChallengeLibrary.TowerShopItemTypeWeight = TowerShopItemTypeWeight;
            IslandChallengeLibrary.TowerShopItemList = TowerShopItemList;
            IslandChallengeLibrary.TowerTypeShopItemList = TowerTypeShopItemList;
        }

        public static IslandChallengeShopItemModel GetIslandChallengeShopItemModel(int id)
        {
            IslandChallengeShopItemModel model;
            TowerShopItemList.TryGetValue(id, out model);
            return model;
        }

        public static TowerShopItemType RandomShopItemType(int mainTaskId, bool notSoulBone)
        {
            int weight = 0;
            Dictionary<TowerShopItemType, int> itemTypeWeight = new Dictionary<TowerShopItemType, int>();
            TowerShopItemTypeLimit.ForEach(x =>
            {
                if (x.Key == TowerShopItemType.SoulBone && notSoulBone) return;
                if (mainTaskId >= x.Value)
                {
                    weight += TowerShopItemTypeWeight[x.Key];
                    itemTypeWeight[x.Key] = weight;
                }
            });

            int ratio = RAND.Range(0, weight - 1);
            TowerShopItemType rewardType = itemTypeWeight.First(x => ratio < x.Value).Key;
            return rewardType;
        }

        public static TowerShopItemModel RandomShopItem(TowerShopItemType rewardType, int quality)
        {
            Dictionary<TowerShopItemQuality, TowerShopItemQualityItems> modelist;
            if (!TowerTypeShopItemList.TryGetValue(rewardType, out modelist)) return null;

            foreach (var kv in modelist)
            {
                if (quality >= kv.Key.Min && quality <= kv.Key.Max) return kv.Value.RandomItem();
            }

            return null;
        }

        #endregion
    }
}
