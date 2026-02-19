using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    class TowerShopItemQuality
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }

    class TowerShopItemQualityItems
    {
        public int Weight { get; set; }

        public List<TowerShopItemModel> ShopItems = new List<TowerShopItemModel>();

        public TowerShopItemQualityItems(TowerShopItemModel model)
        {
            AddItem(model);
        }

        public void AddItem(TowerShopItemModel model)
        {
            Weight += model.Weight;
            ShopItems.Add(model);
        }

        public TowerShopItemModel RandomItem()
        {
            int ratio = RAND.Range(0, Weight - 1);
            foreach (var kv in ShopItems)
            {
                if (ratio < kv.Weight) return kv;
                ratio -= kv.Weight;
            }
            return null;
        }
    }

    public class TowerLibrary
    {
        private static Dictionary<int, TowerRewardModel> towerList = new Dictionary<int, TowerRewardModel>();
        //private static Dictionary<int, Dictionary<int, TowerRewardModel>> groupTowerList = new Dictionary<int, Dictionary<int, TowerRewardModel>>();

        private static int MaxGroupWeight = 0;
        private static Dictionary<int, int> groupWeight = new Dictionary<int, int>();
        private static Dictionary<int, int> groupMaxNodeId = new Dictionary<int, int>();

        //groupid,nodeid,
        private static Dictionary<int, TowerTaskModel> taskList = new Dictionary<int, TowerTaskModel>();
        private static Dictionary<int, Dictionary<int, List<TowerTaskModel>>> groupNodeTask = new Dictionary<int, Dictionary<int, List<TowerTaskModel>>>();

        //shop
        private static int MaxShopItemTypeWeight;
        private static Dictionary<TowerShopItemType, int> TowerShopItemTypeLimit = new Dictionary<TowerShopItemType, int>();
        private static Dictionary<TowerShopItemType, int> TowerShopItemTypeWeight = new Dictionary<TowerShopItemType, int>();
        private static Dictionary<int, TowerShopItemModel> TowerShopItemList = new Dictionary<int, TowerShopItemModel>();
        private static Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>> TowerTypeShopItemList = new Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>>();

        //buff
        private static Dictionary<int, int> buffQualityWeight = new Dictionary<int, int>();//不同品质的buff总权重
        private static Dictionary<int, TowerBuffModel> BuffList = new Dictionary<int,TowerBuffModel>();
        private static Dictionary<int, Dictionary<int, TowerBuffModel>> QualityBuffList = new Dictionary<int, Dictionary<int, TowerBuffModel>>();
        private static Dictionary<int, int> limitHeroBuff = new Dictionary<int, int>();

        //dungeon
        private static Dictionary<int, int> dungeonWeight = new Dictionary<int, int>();//不同难度副本的总权重
        private static Dictionary<int, TowerDungeonModel> DungeonList = new Dictionary<int, TowerDungeonModel>();
        private static Dictionary<int, Dictionary<int, TowerDungeonModel>> DifficutyDungeonList = new Dictionary<int, Dictionary<int, TowerDungeonModel>>();

        private static Dictionary<int, int> buyReviveCost = new Dictionary<int, int>();

        /// <summary>
        /// 每周期限制的职业
        /// </summary>
        public static List<int> PeriodLimitHeroJob = new List<int>();

        public static int ShopItemCount { get; private set; }
        public static int OpenDays { get; private set; }
        public static int CloseDays { get; private set; }
        public static float HpRatio { get; private set; }//回复血量百分比
        public static int MaxNode { get; private set; }
        public static int FirstBattlePower { get; private set; }//第一期开启时候战斗力
        public static int FirstHeroLevel { get; private set; }
        public static int EquipMaxQuality { get; private set; }
        public static int SoulBoneMaxQuality { get; private set; }

        public static void Init()
        {
            //towerList.Clear();
            //groupTowerList.Clear();

            MaxGroupWeight = 0;
            //groupWeight.Clear();
            //groupNodeTask.Clear();
            //taskList.Clear();

            //groupMaxNodeId.Clear();
            MaxShopItemTypeWeight = 0;
            //TowerShopItemList.Clear();
            //TowerShopItemTypeLimit.Clear();
            //TowerShopItemTypeWeight.Clear();
            //TowerTypeShopItemList.Clear();

            //buffQualityWeight.Clear();
            //BuffList.Clear();
            //QualityBuffList.Clear();
            //limitHeroBuff.Clear();

            //dungeonWeight.Clear();
            //DungeonList.Clear();
            //DifficutyDungeonList.Clear();

            //buyReviveCost.Clear();
            //PeriodLimitHeroJob.Clear();

            InitConfig();
            InitRewardInfo();
            InitGroup();
            InitTask();
            InitShop();
            InitBuff();
            InitDungeon();
        }

        private static void InitConfig()
        {
            List<int> PeriodLimitHeroJob = new List<int>();
            Data data = DataListManager.inst.GetData("TowerConfig", 1);
            OpenDays = data.GetInt("OpenDays");
            CloseDays = data.GetInt("CloseDays");
            HpRatio = data.GetFloat("HpRatio");
            ShopItemCount = data.GetInt("ShopItemCount");
            FirstBattlePower = data.GetInt("FirstBattlePower");
            FirstHeroLevel = data.GetInt("FirstHeroLevel");
            EquipMaxQuality = data.GetInt("EquipMaxQuality");
            SoulBoneMaxQuality = data.GetInt("SoulBoneMaxQuality");

            //1:100|3:200|6:1000，表示，第1、2次购买100钻、第3、4、5次购买200钻，第6次以上1000钻
            Dictionary<int, int> countCost = StringSplit.GetKVPairs(data.GetString("RevivePrice"));
                buyReviveCost = countCost.OrderByDescending(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

            string jobStr = data.GetString("JobLimit");
            if (!string.IsNullOrEmpty(jobStr))
            {
                string[] jobsStr = jobStr.Split('|');
                PeriodLimitHeroJob.AddRange(jobsStr.ToList().ConvertAll(x => int.Parse(x)));
            }
            TowerLibrary.PeriodLimitHeroJob = PeriodLimitHeroJob;
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
            Dictionary<int, TowerRewardModel> towerList = new Dictionary<int, TowerRewardModel>();
            //Dictionary<int, Dictionary<int, TowerRewardModel>> groupTowerList = new Dictionary<int, Dictionary<int, TowerRewardModel>>();
            TowerRewardModel model;
            DataList dataList = DataListManager.inst.GetDataList("TowerNodeReward");
            foreach (var kv in dataList)
            {
                model = new TowerRewardModel(kv.Value);
                towerList.Add(model.Id, model);

                //Dictionary<int, TowerRewardModel> groupModel;
                //if (!groupTowerList.TryGetValue(model.GroupId, out groupModel))
                //{
                //    groupModel = new Dictionary<int, TowerRewardModel>();
                //    groupTowerList.Add(model.GroupId, groupModel);
                //}

                //groupModel[model.Id] = model;

            }
            TowerLibrary.towerList = towerList;
            //TowerLibrary.groupTowerList = groupTowerList;
        }

        public static TowerRewardModel GetTowerModel(int id)
        {
            TowerRewardModel model;
            towerList.TryGetValue(id, out model);
            return model;
        }

        #endregion 

        #region task

        private static void InitTask()
        {
            Dictionary<int, int> groupMaxNodeId = new Dictionary<int, int>();
            Dictionary<int, TowerTaskModel> taskList = new Dictionary<int, TowerTaskModel>();
            Dictionary<int, Dictionary<int, List<TowerTaskModel>>> groupNodeTask = new Dictionary<int, Dictionary<int, List<TowerTaskModel>>>();

            TowerTaskModel model;
            DataList dataList = DataListManager.inst.GetDataList("TowerTask");
            foreach (var kv in dataList)
            {
                model = new TowerTaskModel(kv.Value);

                if (!groupMaxNodeId.ContainsKey(model.GroupId))
                {
                    groupMaxNodeId[model.GroupId] = model.NodeId;
                }
                else 
                { 
                    groupMaxNodeId[model.GroupId] = Math.Max(model.NodeId, groupMaxNodeId[model.GroupId]);
                }

                MaxNode = Math.Max(MaxNode, model.NodeId);

                Dictionary<int, List<TowerTaskModel>> nodeTask;
                if (!groupNodeTask.TryGetValue(model.GroupId, out nodeTask))
                {
                    nodeTask = new Dictionary<int, List<TowerTaskModel>>();
                    groupNodeTask[model.GroupId] = nodeTask;
                }

                List<TowerTaskModel> taskModels;
                if (!nodeTask.TryGetValue(model.NodeId, out taskModels))
                {
                    taskModels = new List<TowerTaskModel>();
                    nodeTask[model.NodeId] = taskModels;
                }

                taskList[model.Id] = model;

                taskModels.Add(model);
            }
            TowerLibrary.groupMaxNodeId = groupMaxNodeId;
            TowerLibrary.taskList = taskList;
            TowerLibrary.groupNodeTask = groupNodeTask;


        }

        public static TowerTaskModel GetTaskModel(int id)
        {
            TowerTaskModel model;
            taskList.TryGetValue(id, out model);
            return model;
        }

        public static Dictionary<int, List<TowerTaskModel>> GetGroupTasks(int groupId)
        {
            Dictionary<int, List<TowerTaskModel>> nodeTask;
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

        #region group

        private static void InitGroup()
        {
            Dictionary<int, int> groupWeight = new Dictionary<int, int>();

            MaxGroupWeight = 0;

            DataList dataList = DataListManager.inst.GetDataList("TowerGroup");
            foreach (var kv in dataList)
            {
                MaxGroupWeight += kv.Value.GetInt("Weight");
                groupWeight[kv.Value.GetInt("GroupId")] = MaxGroupWeight;
            }
            TowerLibrary.groupWeight = groupWeight;
        }

        public static int RandomGroup()
        {
            int ratio = RAND.Range(0, MaxGroupWeight - 1);
            int groupId = groupWeight.Where(x => ratio < x.Value).First().Key;
            return groupId;
        }

        #endregion

        #region dungeon

        private static void InitDungeon()
        {
            Dictionary<int, int> dungeonWeight = new Dictionary<int, int>();
            Dictionary<int, TowerDungeonModel> DungeonList = new Dictionary<int, TowerDungeonModel>();
            Dictionary<int, Dictionary<int, TowerDungeonModel>> DifficutyDungeonList = new Dictionary<int, Dictionary<int, TowerDungeonModel>>();

            TowerDungeonModel model;
            DataList dataList = DataListManager.inst.GetDataList("TowerDungeon");
            foreach (var kv in dataList)
            {
                model = new TowerDungeonModel(kv.Value);

                DungeonList[model.Id] = model;

                if (!dungeonWeight.ContainsKey(model.Difficuty))
                {
                    dungeonWeight[model.Difficuty] = model.Weight;
                }
                else
                { 
                    dungeonWeight[model.Difficuty] += model.Weight;
                }

                Dictionary<int, TowerDungeonModel> dungeonList;
                if (!DifficutyDungeonList.TryGetValue(model.Difficuty, out dungeonList))
                {
                    dungeonList = new Dictionary<int, TowerDungeonModel>();
                    DifficutyDungeonList[model.Difficuty] = dungeonList;
                }

                dungeonList[model.Id] = model;
            }
            TowerLibrary.dungeonWeight = dungeonWeight;
            TowerLibrary.DungeonList = DungeonList;
            TowerLibrary.DifficutyDungeonList = DifficutyDungeonList;
        }

        public static TowerDungeonModel GetTowerDungeonModel(int id)
        {
            TowerDungeonModel model;
            DungeonList.TryGetValue(id, out model);
            return model;
        }

        public static TowerDungeonModel RandomDungeon(int difficuty)
        {
            int maxWeight;
            if (!dungeonWeight.TryGetValue(difficuty, out maxWeight))
            {
                return null;
            }

            int ratio = RAND.Range(0, maxWeight - 1);
            foreach (var kv in DifficutyDungeonList[difficuty])
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
            Dictionary<int, TowerShopItemModel> TowerShopItemList = new Dictionary<int, TowerShopItemModel>();
            Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>> TowerTypeShopItemList = new Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>>();

            DataList dataList = DataListManager.inst.GetDataList("TowerShopItemTypeRandom");
            foreach (var kv in dataList)
            {
                TowerShopItemType type = (TowerShopItemType)kv.Value.GetInt("Type");
                TowerShopItemTypeWeight[type] = kv.Value.GetInt("Weight");
                TowerShopItemTypeLimit[type] = kv.Value.GetInt("TaskLimit");
            }

            dataList = DataListManager.inst.GetDataList("TowerShopItem");
            foreach (var kv in dataList)
            {
                TowerShopItemModel model = new TowerShopItemModel(kv.Value);
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
            TowerLibrary.TowerShopItemTypeLimit = TowerShopItemTypeLimit;
            TowerLibrary.TowerShopItemTypeWeight = TowerShopItemTypeWeight;
            TowerLibrary.TowerShopItemList = TowerShopItemList;
            TowerLibrary.TowerTypeShopItemList = TowerTypeShopItemList;
        }

        public static TowerShopItemModel GetShopItemModel(int id)
        {
            TowerShopItemModel model;
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

        public static TowerShopItemModel RandopShopItem(TowerShopItemType rewardType, int quality)
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

        #region buff

        private static void InitBuff()
        {
            Dictionary<int, int> buffQualityWeight = new Dictionary<int, int>();
            Dictionary<int, TowerBuffModel> BuffList = new Dictionary<int, TowerBuffModel>();
            Dictionary<int, Dictionary<int, TowerBuffModel>> QualityBuffList = new Dictionary<int, Dictionary<int, TowerBuffModel>>();
            Dictionary<int, int> limitHeroBuff = new Dictionary<int, int>();

            TowerBuffModel model;
            DataList dataList = DataListManager.inst.GetDataList("TowerBuff");
            foreach (var kv in dataList)
            {
                model = new TowerBuffModel(kv.Value);

                BuffList[model.Id] = model;

                if (!buffQualityWeight.ContainsKey(model.Quality))
                {
                    buffQualityWeight[model.Quality] = model.Weight;
                }
                else
                {
                    buffQualityWeight[model.Quality] += model.Weight;
                }

                Dictionary<int, TowerBuffModel> qualityBuffList;
                if (!QualityBuffList.TryGetValue(model.Quality, out qualityBuffList))
                {
                    qualityBuffList = new Dictionary<int, TowerBuffModel>();
                    QualityBuffList[model.Quality] = qualityBuffList;
                }

                qualityBuffList[model.Id] = model;

                if (model.JobLimit == TowerBuffLimit.Hero)
                {
                    limitHeroBuff[model.Id] = model.LimitParam;
                }
            }
            TowerLibrary.buffQualityWeight = buffQualityWeight;
            TowerLibrary.BuffList = BuffList;
            TowerLibrary.QualityBuffList = QualityBuffList;
            TowerLibrary.limitHeroBuff = limitHeroBuff;
        }

        public static TowerBuffModel GetTowerBuffModel(int id)
        {
            TowerBuffModel model;
            BuffList.TryGetValue(id, out model);
            return model;
        }

        public static TowerBuffModel RandomBuff(Dictionary<int, int> qualityWeight, List<int> currBuffList, Dictionary<int, int> heroPos)
        {
            int maxWeight = qualityWeight.Values.Sum();
            int qualityRatio = RAND.Range(0, maxWeight - 1);
            int quality = 1;
            foreach (var kv in qualityWeight)
            {
                if (qualityRatio < kv.Value)
                {
                    quality = kv.Key;
                    break;
                }
                else
                {
                    qualityRatio -= kv.Value;
                }
            }

            int maxQuelityWeight;
            if (!buffQualityWeight.TryGetValue(quality, out maxQuelityWeight))
            {
                Log.Warn($"Towerlibrary RandomBuff error have not quality {quality} buff pool，check it !");
                return null;
            }

            Dictionary<int, TowerBuffModel> randomBuffPool;
            if (!QualityBuffList.TryGetValue(quality, out randomBuffPool)) return null;

            //本次池中不包含已经随机到的buff
            Dictionary<int, TowerBuffModel> thisTimeBuffPool = new Dictionary<int, TowerBuffModel>(randomBuffPool);
            currBuffList.ForEach(x =>
            {
                TowerBuffModel existModel;
                if (thisTimeBuffPool.TryGetValue(x, out existModel))
                {
                    maxQuelityWeight -= existModel.Weight;
                    thisTimeBuffPool.Remove(x);
                }
            });

            //排除限定herobuff，(必须有该hero上阵才能随机到该buff)
            limitHeroBuff.ForEach(x =>
            {
                TowerBuffModel heroLimitModel;
                if (thisTimeBuffPool.TryGetValue(x.Key, out heroLimitModel))
                {
                    if (!heroPos.ContainsKey(x.Value))
                    {
                        thisTimeBuffPool.Remove(x.Key);
                        maxQuelityWeight -= heroLimitModel.Weight;
                    }
                }
            });

            int ratio = RAND.Range(0, maxQuelityWeight - 1);
            foreach (var kv in thisTimeBuffPool)
            {
                if (ratio < kv.Value.Weight) return kv.Value;
                ratio -= kv.Value.Weight;
            }

            return null;
        }

        #endregion

    }
}
