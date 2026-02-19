using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.ModelBinding;

namespace ServerShared
{
    public static class SoulRingLibrary
    {
        private static Dictionary<int, SoulRingModel> soulRingItems = new Dictionary<int, SoulRingModel>();
        private static Dictionary<int, SoulRingEnhanceModel> soulRingEnhances = new Dictionary<int, SoulRingEnhanceModel>();
        private static Dictionary<int, SoulRingBreakModel> soulRingBreaks= new Dictionary<int, SoulRingBreakModel>();
        private static Dictionary<int, SoulRingSlotUnlockModel> soulRingSlotUnlocks= new Dictionary<int, SoulRingSlotUnlockModel>();
        private static Dictionary<int, SoulRingSpecModel> soulRingSpec = new Dictionary<int, SoulRingSpecModel>();
        private static SoulRingConfigModel soulRingConfig = new SoulRingConfigModel();
        private static Dictionary<int, string> soulRingReverts = new Dictionary<int, string>();

        private static Dictionary<int, NatureDataModel> basicNatureList = new Dictionary<int, NatureDataModel>();
        private static Dictionary<int, NatureDataModel> natureIncrList = new Dictionary<int, NatureDataModel>();

        private static List<SoulRingAbsorbTimeModel> soulRingAbsorbTimeModelList = new List<SoulRingAbsorbTimeModel>();

        private static Dictionary<int, SoulRingElementModel> soulRingElementList = new Dictionary<int, SoulRingElementModel>();

        private static Dictionary<int, SoulRingAdditionalNatrueModel> additionalNatrueList = new Dictionary<int, SoulRingAdditionalNatrueModel>();
        private static DoubleDepthMap<int, int, int> additionalNatrueBattlePowers = new DoubleDepthMap<int, int, int>();

        public static SoulRingConfigModel SoulRingConfig
        {
            get
            {
                return soulRingConfig;
            }
        }

        public static void Init()
        {
            //soulRingItems.Clear();
            //soulRingEnhances.Clear();
            //soulRingBreaks.Clear();
            //soulRingSlotUnlocks.Clear();
            //soulRingSpec.Clear();
            //basicNatureList.Clear();
            //natureIncrList.Clear();
            //soulRingElementList.Clear();

            DataList ringDataList = DataListManager.inst.GetDataList("SoulRing");
            DataList enhanceDataList = DataListManager.inst.GetDataList("SoulRingEnhance");
            DataList breakDataList = DataListManager.inst.GetDataList("SoulRingBreak");
            DataList slotUnlockDataList = DataListManager.inst.GetDataList("SoulRingSlotUnlock");
            DataList specDataList = DataListManager.inst.GetDataList("SoulRingSpec");
            Data configData = DataListManager.inst.GetData("SoulRingConfig",1);
            DataList basicNatureDataList = DataListManager.inst.GetDataList("SoulRingBasicAttribute");
            DataList basicNatureIncrDataList = DataListManager.inst.GetDataList("SoulRingBasicNatureIncreasement");
            DataList soulRingAbsorbTimeList = DataListManager.inst.GetDataList("SoulRingAbsorbTime");

            InitSoulRingItems(ringDataList);
            InitSoulRingEnhance(enhanceDataList);
            InitSoulRingBreak(breakDataList);
            InitSoulRingSlotUnlock(slotUnlockDataList);
            InitSoulRingSpec(specDataList);
            InitSoulRingConfig(configData);
            InitSoulRingBasicNature(basicNatureDataList);
            InitSoulRingNatureIncreasement(basicNatureIncrDataList);
            InitSoulRingbsorbTime(soulRingAbsorbTimeList);
            InitSoulRingRevert();
            InitSoulRingElement();
            InitSoulRingAdditionalNatrue();
        }

        private static void InitSoulRingbsorbTime(DataList dataList)
        {
            List<SoulRingAbsorbTimeModel> soulRingAbsorbTimeModelList = new List<SoulRingAbsorbTimeModel>();
            foreach (var kv in dataList)
            {
                soulRingAbsorbTimeModelList.Add(new SoulRingAbsorbTimeModel(kv.Value));
            }
            SoulRingLibrary.soulRingAbsorbTimeModelList = soulRingAbsorbTimeModelList;
        }

        public static TimeSpan GetAbsorbTime(int year)
        {
            TimeSpan timeSpan = new TimeSpan(0,0,10);
            foreach (var item in soulRingAbsorbTimeModelList)
            {
                if (item.MinYear <= year && year <= item.MaxYear)
                {
                    return item.AbsorbTime;
                }
            }
            return timeSpan;
        }

        public static NatureDataModel GetBasicNatureModel(int id)
        {
            NatureDataModel model;
            basicNatureList.TryGetValue(id, out model);
            return model;
        }

        public static NatureDataModel GetBasicNatureIncrModel(int id)
        {
            NatureDataModel model;
            natureIncrList.TryGetValue(id, out model);
            return model;
        }

        private static void InitSoulRingBasicNature(DataList dataList)
        {
            Dictionary<int, NatureDataModel> basicNatureList = new Dictionary<int, NatureDataModel>();
            foreach (var kv in dataList)
            {
                SoulRingModel model = GetSoulRingMode(kv.Key);
                if (model == null)
                {
                    Logger.Log.WarnLine($"InitSoulRingBasicNature error have no soul ring {kv.Value}");
                    continue;
                }

                basicNatureList[kv.Key] = new NatureDataModel(kv.Value);
            }
            SoulRingLibrary.basicNatureList = basicNatureList;
        }

        private static void InitSoulRingNatureIncreasement(DataList dataList)
        {
            Dictionary<int, NatureDataModel> natureIncrList = new Dictionary<int, NatureDataModel>();
            foreach (var kv in dataList)
            {
                natureIncrList[kv.Key] = new NatureDataModel(kv.Value);
            }
            SoulRingLibrary.natureIncrList = natureIncrList;
        }

        private static void InitSoulRingRevert()
        {
            //soulRingReverts.Clear();
            Dictionary<int, string> soulRingReverts = new Dictionary<int, string>();

            DataList dataList = DataListManager.inst.GetDataList("SoulRingRevert");
            foreach (var kv in dataList)
            {
                soulRingReverts[kv.Value.ID] = kv.Value.GetString("Reward");
            }
            SoulRingLibrary.soulRingReverts = soulRingReverts;
        }

        public static string GetSoulRingRevert(int level)
        {
            string temp;
            soulRingReverts.TryGetValue(level, out temp);
            return temp;
        }

        public static SoulRingModel GetSoulRingMode(int id)
        {
            SoulRingModel temp;
            soulRingItems.TryGetValue(id, out temp);
            return temp;
        }

        public static SoulRingEnhanceModel GetSoulRingEnhanceMode(int id)
        {
            SoulRingEnhanceModel temp;
            soulRingEnhances.TryGetValue(id, out temp);
            return temp;
        }

        public static SoulRingBreakModel GetSoulRingBreakCostMode(int level)
        {
            SoulRingBreakModel model = null;
            foreach (var kv in soulRingBreaks)
            {
                if (kv.Key > level) break;
                model = kv.Value;
            }
            return model;
        }

        public static SoulRingBreakModel GetSoulRingBreakMode(int id)
        {
            SoulRingBreakModel temp;
            soulRingBreaks.TryGetValue(id, out temp);
            return temp;
        }


        public static SoulRingSlotUnlockModel GetSoulRingSlotUnlockMode(int id)
        {
            SoulRingSlotUnlockModel temp;
            soulRingSlotUnlocks.TryGetValue(id, out temp);
            return temp;
        }

        public static SoulRingSpecModel GetSoulRingSpecModel(int id)
        {
            SoulRingSpecModel model;
            soulRingSpec.TryGetValue(id, out model);
            return model;
        }

        public static void InitSoulRingConfig(Data data)
        {
            SoulRingConfig.Init(data);
        }

        private static void InitSoulRingItems(DataList dataList)
        {
            Dictionary<int, SoulRingModel> soulRingItems = new Dictionary<int, SoulRingModel>();

            Data data = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;

                SoulRingModel model = new SoulRingModel()
                {
                    Id = data.ID,
                    Position = data.GetInt("SubType"),
                    MaxYear = data.GetInt("MaxYear"),
                    IniLevel = data.GetInt("IniLevel"),
                    SPAttrType = data.GetInt("SPAttrType"),
                    MainAttrTypes = new List<NatureType>(),
                    UltAttrValue = new Dictionary<NatureType, long>(),
                    Data = data
                };

                string strNatureTypes = data.GetString("MainAttrType");
                string [] attrTypes = strNatureTypes.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string attrType in attrTypes)
                {
                    //FIXME:BOIL 严格检测配表属性和NatureType的一致性，如果对不上这里就是个错
                    int type = int.Parse(attrType);
                    model.MainAttrTypes.Add((NatureType)type);
                }

                strNatureTypes = data.GetString("UltAttrType");
                attrTypes = strNatureTypes.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                string[] values = data.GetString("UltAttrValue").Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (attrTypes.Length <= values.Length)
                {
                    for(int i =0; i  < attrTypes.Length; i++)
                    {
                        //FIXME:BOIL 严格检测配表属性和NatureType的一致性，如果对不上这里就是个错
                        int type = int.Parse(attrTypes[i]);
                        model.UltAttrValue[(NatureType)type] = int.Parse(values[i]);
                    }
                }

                string elementStr = data.GetString("Element");
                if (!string.IsNullOrEmpty(elementStr))
                {
                    model.Element = elementStr.ToList('|');
                }
             
                string[] additionNatureArr = StringSplit.GetArray("|", data.GetString("AdditionalNature"));
                if (additionNatureArr.Length > 0)
                {
                    model.AdditionalNature = new int[additionNatureArr.Length];
                    for (int i = 0; i < additionNatureArr.Length; i++)
                    {
                        model.AdditionalNature[i] = int.Parse(additionNatureArr[i]);
                    }
                }

                soulRingItems.Add(model.Id, model);
            }
            SoulRingLibrary.soulRingItems = soulRingItems;
        }

        private static void InitSoulRingEnhance(DataList dataList)
        {
            Dictionary<int, SoulRingEnhanceModel> soulRingEnhances = new Dictionary<int, SoulRingEnhanceModel>();
            Data data = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;

                SoulRingEnhanceModel model = new SoulRingEnhanceModel()
                {
                    Level = data.ID,
                    DustCost = data.GetInt("DustCost"),
                    GoldCost = data.GetInt("GoldCost"),
                };
                soulRingEnhances.Add(model.Level, model);
            }
            SoulRingLibrary.soulRingEnhances = soulRingEnhances;
        }

        private static void InitSoulRingBreak(DataList dataList)
        {
            Dictionary<int, SoulRingBreakModel> soulRingBreaks = new Dictionary<int, SoulRingBreakModel>();
            Data data = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;

                SoulRingBreakModel model = new SoulRingBreakModel()
                {
                    Level = data.ID,
                    BreathCost = data.GetInt("BreathCost"),
                    GoldCost = data.GetInt("GoldCost"),
                    //BreathTotalCost = data.GetInt("BreathTotalCost"),
                    //GoldTotalCost = data.GetInt("GoldTotalCost"),
                };
                soulRingBreaks.Add(model.Level, model);
            }
            SoulRingLibrary.soulRingBreaks = soulRingBreaks;
        }



        private static void InitSoulRingSlotUnlock(DataList dataList)
        {
            Dictionary<int, SoulRingSlotUnlockModel> soulRingSlotUnlocks = new Dictionary<int, SoulRingSlotUnlockModel>();
            Data data = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;

                SoulRingSlotUnlockModel model = new SoulRingSlotUnlockModel()
                {
                    Slot = data.ID,
                    AwakeLevel = data.GetInt("AwakeLevel"),
                    Level = data.GetInt("Level"),
                };
                soulRingSlotUnlocks.Add(model.Slot, model);
            }
            SoulRingLibrary.soulRingSlotUnlocks = soulRingSlotUnlocks;
        }

        private static void InitSoulRingSpec(DataList dataList)
        {
            Dictionary<int, SoulRingSpecModel> soulRingSpec = new Dictionary<int, SoulRingSpecModel>();
            foreach (var data in dataList)
            {
                SoulRingSpecModel model = new SoulRingSpecModel(data.Value);
                soulRingSpec.Add(model.Id, model);
            }
            SoulRingLibrary.soulRingSpec = soulRingSpec;
        }
        
        private static void InitSoulRingElement()
        {
            //soulRingElementList.Clear();
            Dictionary<int, SoulRingElementModel> soulRingElementList = new Dictionary<int, SoulRingElementModel>();

            DataList dataList = DataListManager.inst.GetDataList("SoulRingElement");
            foreach (var kv in dataList)
            {
                soulRingElementList[kv.Value.ID] = new SoulRingElementModel(kv.Value);
            }
            SoulRingLibrary.soulRingElementList = soulRingElementList;
        }

        private static void InitSoulRingAdditionalNatrue()
        {
            Dictionary<int, SoulRingAdditionalNatrueModel> additionalNatrueList = new Dictionary<int, SoulRingAdditionalNatrueModel>();
            DoubleDepthMap<int, int, int> additionalNatrueBattlePowers = new DoubleDepthMap<int, int, int>();

            DataList dataList = DataListManager.inst.GetDataList("SoulRingAdditionalNatrue");
            foreach (var kv in dataList)
            {
                SoulRingAdditionalNatrueModel model = new SoulRingAdditionalNatrueModel(kv.Value);
                additionalNatrueList.Add(model.Id, model);
                additionalNatrueBattlePowers.Add(model.NatureType, model.Ratio, model.BattlePower);
            }
            SoulRingLibrary.additionalNatrueList = additionalNatrueList;
            SoulRingLibrary.additionalNatrueBattlePowers = additionalNatrueBattlePowers;
        }

        public static SoulRingElementModel GetSoulRingElementModel(int id)
        {
            SoulRingElementModel model;
            soulRingElementList.TryGetValue(id, out model);
            return model;
        }

        public static SoulRingAdditionalNatrueModel GetSoulRingAdditionalNatrueModel(int id)
        {
            SoulRingAdditionalNatrueModel model;
            additionalNatrueList.TryGetValue(id, out model);
            return model;
        }

        public static int GetAdditionalNatrueBattlePower(int natureType, int ratio)
        {
            int battlePower;
            additionalNatrueBattlePowers.TryGetValue(natureType, ratio, out battlePower);
            return battlePower;
        }
    }
}
