using System;
using CommonUtility;
using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;
using Logger;

namespace ServerShared
{
    public static class EquipLibrary
    {
        private static Dictionary<int, EquipmentModel> equipItems = new Dictionary<int, EquipmentModel>();

        private static SortedDictionary<int, EquipInjectionModel> injections = new SortedDictionary<int, EquipInjectionModel>();

        private static Dictionary<int, SortedDictionary<int, EquipInjectionModel>> partIdInjections = new Dictionary<int, SortedDictionary<int, EquipInjectionModel>>();
        private static Dictionary<int, SortedDictionary<int, EquipInjectionModel>> partOrderInjections = new Dictionary<int, SortedDictionary<int, EquipInjectionModel>>();
        private static Dictionary<int, SortedDictionary<int, EquipInjectionModel>> partSlotInjections = new Dictionary<int, SortedDictionary<int, EquipInjectionModel>>();

        private static Dictionary<int, EquipUpgradeModel> upgrades = new Dictionary<int, EquipUpgradeModel>();

        private static Dictionary<int, ItemXuanyuModel> xuanyus = new Dictionary<int, ItemXuanyuModel>();
        private static List<ItemXuanyuModel> xuanyu9UpList = new List<ItemXuanyuModel>();

        private static Dictionary<NatureType, int> natureOrder = new Dictionary<NatureType, int>();

        private static Dictionary<int, string> levelReward = new Dictionary<int, string>();

        public static string AllXuanyuString = string.Empty;

        private static SortedDictionary<int, Dictionary<NatureType, int>> injectionLevelLimit = new SortedDictionary<int, Dictionary<NatureType, int>>();

        private static Dictionary<int, EquipAdvanceModel> advanceModels = new Dictionary<int, EquipAdvanceModel>();

        private static Dictionary<int, XuanyuAdvanceModel> xuanyuAdvanceModels = new Dictionary<int, XuanyuAdvanceModel>();

        private static Dictionary<int, int> equipSuit = new Dictionary<int, int>();
        //job,quality,part,equipId
        private static Dictionary<int, DoubleDeapthListMap<int, int, int>> jobQualityPartEquipList = new Dictionary<int, DoubleDeapthListMap<int, int, int>>();
        private static Dictionary<int, EquipmentSuitModel> equipmentSuitModels = new Dictionary<int, EquipmentSuitModel>();
        private static Dictionary<int, EquipmentSuitEnchantItemModel> equipmentSuitEnchantItemModels = new Dictionary<int, EquipmentSuitEnchantItemModel>();
        private static Dictionary<int, EquipmentSpecialModel> equipmentSpecialModels = new Dictionary<int, EquipmentSpecialModel>();

        public static int UpgradeToLevel { get; private set; }
        public static int InlayXuanyuLevel { get; private set; }
        public static int MaxOnkeyNum { get; private set; }

        public static float ReturnNum { get; private set; }

        public static int LimitNum { get; private set; }
        public static int CostDiamond { get; private set; }
        public static int GoldMin { get; private set; }

        public static int Step1Num { get; private set; }
        public static float Discount1Num { get; private set; }
        public static int Step2Num { get; private set; }
        public static float Discount2Num { get; private set; }
        public static int Step3Num { get; private set; }
        public static float Discount3Num { get; private set; }
        public static int Step4Num { get; private set; }
        public static float Discount4Num { get; private set; }

        public static int ReturnItem { get; private set; }

        public static int XuanyuBreakLevel = 9;

        public static void Init()
        {
            DataList equipDataList = DataListManager.inst.GetDataList("Equipment");
            DataList upgradeDataList = DataListManager.inst.GetDataList("EquipUpgrade");
            DataList injectionDataList = DataListManager.inst.GetDataList("EquipInjection");
            DataList xuanyuDataList = DataListManager.inst.GetDataList("ItemXuanyu");//subtype = 2
            Data config = DataListManager.inst.GetData("EquipConfig", 1);

            DataList levelRewardDataList = DataListManager.inst.GetDataList("EquipLevelReward");

            DataList injectionNature_LevelLimit = DataListManager.inst.GetDataList("EquipInjectionLimit");

            InitNatureOrder();
            InitEquipment(equipDataList);
            InitUpgrade(upgradeDataList);
            InitInjection(injectionDataList);
            InitPartInjections();
            InitItemXuanyu(xuanyuDataList);
            InitConfig(config);

            InitLevelReward(levelRewardDataList);

            InitInjectionLevelLimits(injectionNature_LevelLimit);

            InitAdvance();

            InitXuanyuAdvance();

            InitEquipmentSuit();
            InitEquipmentEnchantItem();
            InitSpec();
        }

        private static void InitInjectionLevelLimits(DataList dataList)
        {
            SortedDictionary<int, Dictionary<NatureType, int>> injectionLevelLimit = new SortedDictionary<int, Dictionary<NatureType, int>>();
            Data data = null;
            Dictionary<NatureType, int> dic = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;
                dic = new Dictionary<NatureType, int>();
                dic.Add(NatureType.PRO_MAX_HP, data.GetInt("PRO_MAX_HP"));
                dic.Add(NatureType.PRO_ATK, data.GetInt("PRO_ATK"));
                dic.Add(NatureType.PRO_DEF, data.GetInt("PRO_DEF"));
                dic.Add(NatureType.PRO_HIT, data.GetInt("PRO_HIT"));
                dic.Add(NatureType.PRO_FLEE, data.GetInt("PRO_FLEE"));
                dic.Add(NatureType.PRO_CRI, data.GetInt("PRO_CRI"));
                dic.Add(NatureType.PRO_RES, data.GetInt("PRO_RES"));
                dic.Add(NatureType.PRO_IMP, data.GetInt("PRO_IMP"));
                dic.Add(NatureType.PRO_ARM, data.GetInt("PRO_ARM"));
                injectionLevelLimit.Add(data.GetInt("Level"), dic);
            }
            EquipLibrary.injectionLevelLimit = injectionLevelLimit;
        }

        private static void InitConfig(Data config)
        {
            UpgradeToLevel = config.GetInt("UpgradeToLevel");
            InlayXuanyuLevel = config.GetInt("InlayXuanyuLevel");
            MaxOnkeyNum = config.GetInt("MaxOnkeyNum");

            Step1Num = config.GetInt("Step1Num");
            Discount1Num = config.GetFloat("Discount1Num");
            Step2Num = config.GetInt("Step2Num");
            Discount2Num = config.GetFloat("Discount2Num");
            Step3Num = config.GetInt("Step3Num");
            Discount3Num = config.GetFloat("Discount3Num");
            Step4Num = config.GetInt("Step4Num");
            Discount4Num = config.GetFloat("Discount4Num");

            ReturnNum = config.GetFloat("ReturnNum");
            LimitNum = config.GetInt("LimitNum");
            CostDiamond = config.GetInt("CostDiamond");
            ReturnItem = config.GetInt("ReturnItem");
            GoldMin = config.GetInt("GoldMin");
        }

        private static void InitNatureOrder()
        {
            //natureOrder.Clear();
            //orderNature.Clear();
            Dictionary<NatureType, int> natureOrder = new Dictionary<NatureType, int>();
            //SortedDictionary<int, NatureType> orderNature = new SortedDictionary<int, NatureType>();

            natureOrder.Add(NatureType.PRO_MAX_HP, 1);
            natureOrder.Add(NatureType.PRO_ATK, 2);
            natureOrder.Add(NatureType.PRO_DEF, 3);
            natureOrder.Add(NatureType.PRO_HIT, 4);
            natureOrder.Add(NatureType.PRO_FLEE, 5);
            natureOrder.Add(NatureType.PRO_CRI, 6);
            natureOrder.Add(NatureType.PRO_RES, 7);
            natureOrder.Add(NatureType.PRO_IMP, 8);
            natureOrder.Add(NatureType.PRO_ARM, 9);

            natureOrder.Add(NatureType.PRO_POW, 10);
            natureOrder.Add(NatureType.PRO_CON, 11);
            natureOrder.Add(NatureType.PRO_EXP, 12);
            natureOrder.Add(NatureType.PRO_AGI, 13);

            //foreach (var kv in natureOrder)
            //{
            //    orderNature.Add(kv.Value, kv.Key);
            //}
            EquipLibrary.natureOrder = natureOrder;
            //EquipLibrary.orderNature = orderNature;
        }

        private static void InitPartInjections()
        {
            Dictionary<int, SortedDictionary<int, EquipInjectionModel>> partIdInjections = new Dictionary<int, SortedDictionary<int, EquipInjectionModel>>();
            Dictionary<int, SortedDictionary<int, EquipInjectionModel>> partOrderInjections = new Dictionary<int, SortedDictionary<int, EquipInjectionModel>>();
            Dictionary<int, SortedDictionary<int, EquipInjectionModel>> partSlotInjections = new Dictionary<int, SortedDictionary<int, EquipInjectionModel>>();

            for (int i = 1; i <= 4; i++)
            {
                partIdInjections.Add(i, new SortedDictionary<int, EquipInjectionModel>());
                partOrderInjections.Add(i, new SortedDictionary<int, EquipInjectionModel>());
                partSlotInjections.Add(i, new SortedDictionary<int, EquipInjectionModel>());
            }
            foreach (var kv in injections)
            {
                partIdInjections[kv.Value.Part][kv.Value.Id] = kv.Value;
                int order = natureOrder[(NatureType)kv.Value.NatureType];
                partOrderInjections[kv.Value.Part][order] = kv.Value;
                partSlotInjections[kv.Value.Part][kv.Value.Slot] = kv.Value;
            }

            EquipLibrary.partIdInjections = partIdInjections;
            EquipLibrary.partOrderInjections = partOrderInjections;
            EquipLibrary.partSlotInjections = partSlotInjections;
        }

        private static void InitEquipment(DataList dataList)
        {
            Dictionary<int, EquipmentModel> equipItems = new Dictionary<int, EquipmentModel>();
            Data data = null;
            EquipmentModel model = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;
                model = new EquipmentModel()
                {
                    ID = data.ID,
                    Part = data.GetInt("Part"),
                    Job = data.GetInt("Job"),
                    Grade = data.GetInt("Grade"),
                    Color = data.GetInt("Color"),
                    Score = data.GetInt("Score"),
                    Star = data.GetInt("Star"),
                    Suit = data.GetInt("Suit"),
                    IsEnchant = data.GetBoolean("IsEnchant")
                };

                model.BaseNatureDic[NatureType.PRO_MAX_HP] = data.GetInt("PRO_MAX_HP");
                model.BaseNatureDic[NatureType.PRO_ATK] = data.GetInt("PRO_ATK");
                model.BaseNatureDic[NatureType.PRO_DEF] = data.GetInt("PRO_DEF");
                model.BaseNatureDic[NatureType.PRO_HIT] = data.GetInt("PRO_HIT");
                model.BaseNatureDic[NatureType.PRO_FLEE] = data.GetInt("PRO_FLEE");
                model.BaseNatureDic[NatureType.PRO_CRI] = data.GetInt("PRO_CRI");
                model.BaseNatureDic[NatureType.PRO_RES] = data.GetInt("PRO_RES");
                model.BaseNatureDic[NatureType.PRO_IMP] = data.GetInt("PRO_IMP");
                model.BaseNatureDic[NatureType.PRO_ARM] = data.GetInt("PRO_ARM");

                model.Data = data;

                equipItems.Add(model.ID, model);

                if (model.Suit > 0)
                {
                    equipSuit[model.ID] = model.Suit;

                    DoubleDeapthListMap<int, int, int> info = null;
                    if (!jobQualityPartEquipList.TryGetValue(model.Job, out info))
                    {
                        info = new DoubleDeapthListMap<int, int, int>();
                        jobQualityPartEquipList.Add(model.Job, info);
                    }

                    info.Add(model.Grade, model.Part, model.ID);
                }
            }
            EquipLibrary.equipItems = equipItems;
        }

        private static void InitUpgrade(DataList dataList)
        {
            Dictionary<int, EquipUpgradeModel> upgrades = new Dictionary<int, EquipUpgradeModel>();
            Data data = null;
            EquipUpgradeModel model = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;
                model = new EquipUpgradeModel()
                {
                    Id = data.ID,
                    UpgradeLevel = data.GetInt("UpgradeLevel"),
                    RollBackLevel = data.GetInt("RollBackLevel"),
                    DestroyLimit = data.GetInt("DestroyLimit"),
                    CostGold = data.GetInt("CostGold"),
                    BaseRate = data.GetInt("BaseRate"),
                    SlotCount = data.GetInt("SlotCount"),
                    Cost = data.GetString("MaterialCount"),
                    SuccessCost = data.GetString("LSCount"),
                    StrengthRatio = data.GetInt("StrengthRatio"),
                };
                model.Generate();

                upgrades.Add(model.Id, model);
            }
            EquipLibrary.upgrades = upgrades;
        }

        private static void InitInjection(DataList dataList)
        {

            SortedDictionary<int, EquipInjectionModel> injections = new SortedDictionary<int, EquipInjectionModel>();
            Data data = null;
            EquipInjectionModel model = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;
                model = new EquipInjectionModel()
                {
                    Id = data.ID,
                    Part = data.GetInt("Part"),
                    LevelLimit = data.GetInt("LevelLimit"),
                    NatureType = data.GetInt("Nature"),
                    //NatureValue=data.GetInt("NatureValue"),
                    Slot = data.GetInt("InjectSlot"),
                };

                injections.Add(model.Id, model);
            }
            EquipLibrary.injections = injections;
        }

        private static void InitItemXuanyu(DataList dataList)
        {
            Dictionary<int, ItemXuanyuModel> xuanyus = new Dictionary<int, ItemXuanyuModel>();
            List<ItemXuanyuModel> xuanyu9UpList = new List<ItemXuanyuModel>();

            AllXuanyuString = string.Empty;
            Data data = null;
            ItemXuanyuModel model = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;
                model = new ItemXuanyuModel()
                {
                    Id = data.ID,
                    Percent = data.GetInt("Percent"),
                    Level = data.GetInt("Level")
                };

                if (model.Level - 1 == XuanyuBreakLevel)
                {
                    xuanyu9UpList.Add(model);
                }

                foreach (NatureType item in Enum.GetValues(typeof(NatureType)))
                {
                    Property property = data.Get(item.ToString());
                    if (property != null)
                    {
                        try
                        {
                            model.NatureList[item] = property.GetLong();
                        }
                        catch (Exception)
                        {
                            Log.Error(($"model id {kv.Key} item {item} data {property.GetString()}"));
                        }
                    }
                }

                xuanyus.Add(model.Id, model);

                AllXuanyuString += "," + data.ID;
            }
            if (AllXuanyuString.Length > 0)
            {
                AllXuanyuString = AllXuanyuString.Substring(1);
            }

            EquipLibrary.xuanyus = xuanyus;
            EquipLibrary.xuanyu9UpList = xuanyu9UpList;
        }

        private static void InitLevelReward(DataList dataList)
        {
            Dictionary<int, string> levelReward = new Dictionary<int, string>();
            foreach (var data in dataList)
            {
                levelReward.Add(data.Value.GetInt("Level"), data.Value.GetString("Reward"));
            }
            EquipLibrary.levelReward = levelReward;
        }

        private static void InitAdvance()
        {
            Dictionary<int, EquipAdvanceModel> advanceModels = new Dictionary<int, EquipAdvanceModel>();
            DataList dataList = DataListManager.inst.GetDataList("EquipAdvance");
            foreach (var pair in dataList)
            {
                EquipAdvanceModel model = new EquipAdvanceModel(pair.Value);
                advanceModels.Add(model.Id, model);
            }

            EquipLibrary.advanceModels = advanceModels;
        }

        private static void InitXuanyuAdvance()
        {
            Dictionary<int, XuanyuAdvanceModel> xuanyuAdvanceModels = new Dictionary<int, XuanyuAdvanceModel>();
            DataList dataList = DataListManager.inst.GetDataList("XuanyuAdvance");
            foreach (var pair in dataList)
            {
                XuanyuAdvanceModel model = new XuanyuAdvanceModel(pair.Value);
                xuanyuAdvanceModels.Add(model.Id, model);
            }

            EquipLibrary.xuanyuAdvanceModels = xuanyuAdvanceModels;
        }

        private static void InitEquipmentSuit()
        {
            Dictionary<int, EquipmentSuitModel> equipmentSuitModels = new Dictionary<int, EquipmentSuitModel>();
            DataList dataList = DataListManager.inst.GetDataList("EquipmentSuit");
            foreach (var pair in dataList)
            {
                EquipmentSuitModel model = new EquipmentSuitModel(pair.Value);
                equipmentSuitModels.Add(model.Id, model);
            }

            EquipLibrary.equipmentSuitModels = equipmentSuitModels;
        }

        private static void InitEquipmentEnchantItem()
        {
            Dictionary<int, EquipmentSuitEnchantItemModel> equipmentSuitEnchantItemModels = new Dictionary<int, EquipmentSuitEnchantItemModel>();
            DataList dataList = DataListManager.inst.GetDataList("EquipmentEnchantItem");
            foreach (var pair in dataList)
            {
                EquipmentSuitEnchantItemModel model = new EquipmentSuitEnchantItemModel(pair.Value);
                equipmentSuitEnchantItemModels.Add(model.Id, model);
            }

            EquipLibrary.equipmentSuitEnchantItemModels = equipmentSuitEnchantItemModels;
        }

        private static void InitSpec()
        {
            Dictionary<int, EquipmentSpecialModel> equipmentSpecialModels = new Dictionary<int, EquipmentSpecialModel>();
            DataList dataList = DataListManager.inst.GetDataList("EquipmentSpec");

            foreach (var data in dataList)
            {
                EquipmentSpecialModel model = new EquipmentSpecialModel(data.Value);
                equipmentSpecialModels.Add(model.Id, model);
            }

            EquipLibrary.equipmentSpecialModels = equipmentSpecialModels;
        }


        #region 外部使用方法

        public static string GetLevelReward(int level)
        {
            string ans = null;
            levelReward.TryGetValue(level, out ans);
            return ans;
        }
        public static EquipmentModel GetEquipModel(int id)
        {
            EquipmentModel temp;
            equipItems.TryGetValue(id, out temp);
            return temp;
        }

        public static EquipUpgradeModel GetEquipUpgradeModel(int id)
        {
            EquipUpgradeModel model;
            upgrades.TryGetValue(id, out model);
            return model;
        }

        public static EquipmentModel GetEquipModel(int job, int part, int grade)
        {
            EquipmentModel model = null;
            foreach (var kv in equipItems)
            {
                model = kv.Value;
                if (model.Data.GetInt("Job") == job && model.Part == part && model.Data.GetInt("Grade") == grade)
                {
                    return model;
                }
            }
            return model;
        }

        public static EquipInjectionModel GetMaxInjectionSlot(int equipLevel, int part)
        {
            EquipInjectionModel model = null;
            SortedDictionary<int, EquipInjectionModel> models = null;
            if (partIdInjections.TryGetValue(part, out models))
            {
                foreach (var item in models)
                {
                    if (item.Value.LevelLimit <= equipLevel)
                    {
                        model = item.Value;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return model;
        }

        public static Dictionary<NatureType, int> GetLevelNatureBase4Injection(int level)
        {
            Dictionary<NatureType, int> ret = new Dictionary<NatureType, int>(); ;
            foreach (var kv in injectionLevelLimit)
            {
                if (level >= kv.Key)
                {
                    ret = kv.Value;
                }
                else
                {
                    break;
                }
            }
            return ret;
        }

        public static ItemXuanyuModel GetXuanyuItem(int id)
        {
            ItemXuanyuModel model;
            xuanyus.TryGetValue(id, out model);
            return model;
        }

        /// <summary>
        /// 玄玉9级别突破后随机一个方向
        /// </summary>
        /// <returns></returns>
        public static ItemXuanyuModel GetXuanyuLevl9BreakModel()
        {
            int index = RAND.Range(0, xuanyu9UpList.Count - 1);
            return xuanyu9UpList[index];
        }

        public static Dictionary<int, int> GetNatureTypesFromInjections(HashSet<int> set, int part)//传入的是属性顺序,返回的是槽的顺序
        {
            Dictionary<int, int> ans = new Dictionary<int, int>();
            EquipInjectionModel model = null;
            SortedDictionary<int, EquipInjectionModel> models = null;
            if (partSlotInjections.TryGetValue(part, out models))
            {
                foreach (var item in set)
                {
                    if (models.TryGetValue(item, out model))
                    {
                        ans.Add(model.Slot, model.NatureType);
                    }
                }
            }

            return ans;
        }

        public static EquipmentModel GetEquipment(int job, int part, int grade)
        {
            EquipmentModel model = null;
            foreach (var kv in equipItems)
            {
                if (kv.Value.Job == job && kv.Value.Part == part && kv.Value.Grade == grade)
                {
                    model = kv.Value;
                    break;
                }
            }
            return model;
        }

        public static EquipAdvanceModel GetEquipAdvanceModel(int id)
        {
            EquipAdvanceModel model;
            advanceModels.TryGetValue(id, out model);
            return model;
        }

        public static XuanyuAdvanceModel GetXuanyuAdvanceModel(int id)
        {
            XuanyuAdvanceModel model;
            xuanyuAdvanceModels.TryGetValue(id, out model);
            return model;
        }

        /// <summary>
        /// 装备附魔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EquipmentModel EquipmentEnchant(EquipmentModel model, EquipmentSuitEnchantItemModel itemModel)
        {
            if (model == null || itemModel == null) return null;

            DoubleDeapthListMap<int, int, int> info;
            if (!jobQualityPartEquipList.TryGetValue(model.Job, out info)) return null;

            List<int> ids;
            if (!info.TryGetValue(model.Grade, model.Part, out ids)) return null;

            int suitId = RAND.RandValue(itemModel.SuitWeight, itemModel.Weight);
            if (suitId == 0) return null;

            EquipmentModel equipmentModel = ids.Select(id => GetEquipModel(id)).FirstOrDefault(tempModel => tempModel.Suit == suitId);

            return equipmentModel;
        }

        public static EquipmentSuitModel GetEquipmentSuitModel(int id)
        {
            EquipmentSuitModel model;
            equipmentSuitModels.TryGetValue(id, out model);
            return model;
        }

        public static EquipmentSuitEnchantItemModel GetEquipmentSuitEnchantItemModel(int id)
        {
            EquipmentSuitEnchantItemModel model;
            equipmentSuitEnchantItemModels.TryGetValue(id, out model);
            return model;
        }

        public static EquipmentSpecialModel GetEquipmentSpecialModel(int id)
        {
            EquipmentSpecialModel model;
            equipmentSpecialModels.TryGetValue(id, out model);
            return model;
        }

        public static List<EquipmentSpecialModel> GetEquipSeSpecialModelsByEquipIds(IEnumerable<int> equipIdList)
        {
            return GetEquipSeSpecialModels(equipIdList.Select(x => GetEquipModel(x).Suit));
        }

        public static List<EquipmentSpecialModel> GetEquipSeSpecialModels(IEnumerable<int> suitList)
        {
            List<EquipmentSpecialModel> specialModels = new List<EquipmentSpecialModel>();
            var groupBy = suitList.GroupBy(x=>x,(key,value)=>new{id = key, count = value.Count()});
            foreach (var item in groupBy)
            {
                if (item.count >= 2)
                {
                    EquipmentSuitModel model = GetEquipmentSuitModel(item.id);
                    if(model == null) continue;

                    EquipmentSpecialModel specialModel = GetEquipmentSpecialModel(model.SpecialEffect2);
                    if (specialModel != null)
                    {
                        specialModels.Add(specialModel);
                    }

                    if (item.count >= 4)
                    {
                        specialModel = GetEquipmentSpecialModel(model.SpecialEffect4);
                        if (specialModel != null)
                        {
                            specialModels.Add(specialModel);
                        }
                    }
                }
            }

            return specialModels;
        }

        #endregion
    }
}
