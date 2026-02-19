using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ServerShared
{
    public static class NatureLibrary
    {

        //private static Dictionary<NatureType, BasicNatureFactorModel> basicNatureFactorList = new Dictionary<NatureType, BasicNatureFactorModel>();
        private static Dictionary<NatureType, Dictionary<NatureType, float>> basicNatureMapping = new Dictionary<NatureType, Dictionary<NatureType, float>>();
        //public static CommonNatureParamModel CommonNatureParamModel
        //{ get; private set; }

        private static Dictionary<int, NatureDataModel> basicNatureIncrList = new Dictionary<int, NatureDataModel>();

        private static Dictionary<int, GroValFactorModel> groValFactorList = new Dictionary<int, GroValFactorModel>();

        private static Dictionary<int, NatureDataModel> heroBasicNatureList = new Dictionary<int, NatureDataModel>();
        private static Dictionary<int, NatureDataModel> heroBasicAddedNatureList = new Dictionary<int, NatureDataModel>();
        private static Dictionary<int, Dictionary<int, HeroStepNatureModel>> heroStepNatureList = new Dictionary<int, Dictionary<int, HeroStepNatureModel>>();

        public static Dictionary<NatureType, int> Basic4Nature = new Dictionary<NatureType, int>();
        public static Dictionary<NatureType, int> Basic9Nature = new Dictionary<NatureType, int>();
        public static Dictionary<NatureType, int> BasicSpeedNature = new Dictionary<NatureType, int>();
        public static void Init()
        {
            //InitBasicNatureFactor();

            InitBasicNatureMapping();

            //DataList natureParamDataList = DataListManager.inst.GetDataList("CommonNatureParam");
            //foreach (var item in natureParamDataList)
            //{
            //    Data data = item.Value;
            //    CommonNatureParamModel = new CommonNatureParamModel(data);
            //    break;
            //}

            InitBasicNatureIncreasement();

            InitGroValFactor();

            InitHeroBasicAttribute();

            InitHeroBasicAddedAttribute();

            InitBasic49Nature();

            InitStepNature();
        }

        public static void InitBasic49Nature()
        {
            Basic4Nature[NatureType.PRO_POW] = (int)NatureType.PRO_POW;
            Basic4Nature[NatureType.PRO_CON] = (int)NatureType.PRO_CON;
            Basic4Nature[NatureType.PRO_EXP] = (int)NatureType.PRO_EXP;
            Basic4Nature[NatureType.PRO_AGI] = (int)NatureType.PRO_AGI;

            Basic9Nature[NatureType.PRO_MAX_HP] = (int)NatureType.PRO_MAX_HP;
            Basic9Nature[NatureType.PRO_ATK] = (int)NatureType.PRO_ATK;
            Basic9Nature[NatureType.PRO_DEF] = (int)NatureType.PRO_DEF;
            Basic9Nature[NatureType.PRO_HIT] = (int)NatureType.PRO_HIT;
            Basic9Nature[NatureType.PRO_FLEE] = (int)NatureType.PRO_FLEE;
            Basic9Nature[NatureType.PRO_CRI] = (int)NatureType.PRO_CRI;
            Basic9Nature[NatureType.PRO_RES] = (int)NatureType.PRO_RES;
            Basic9Nature[NatureType.PRO_IMP] = (int)NatureType.PRO_IMP;
            Basic9Nature[NatureType.PRO_ARM] = (int)NatureType.PRO_ARM;

            BasicSpeedNature[NatureType.PRO_SPD] = (int)NatureType.PRO_SPD;
            BasicSpeedNature[NatureType.PRO_RUN_IN_BATTLE] = (int)NatureType.PRO_RUN_IN_BATTLE;
            BasicSpeedNature[NatureType.PRO_RUN_OUT_BATTLE] = (int)NatureType.PRO_RUN_OUT_BATTLE;
        }
        private static void InitHeroBasicAttribute()
        {
            //heroBasicNatureList.Clear();
            Dictionary<int, NatureDataModel> heroBasicNatureList = new Dictionary<int, NatureDataModel>();

            DataList heroNatureDataList = DataListManager.inst.GetDataList("HeroBasicAttribute");
            foreach (var item in heroNatureDataList)
            {
                Data data = item.Value;
                if (!heroBasicNatureList.ContainsKey(data.ID))
                {
                    heroBasicNatureList.Add(data.ID, new NatureDataModel(data));
                }
            }
            NatureLibrary.heroBasicNatureList = heroBasicNatureList;
        }

        private static void InitHeroBasicAddedAttribute()
        {
            //heroBasicAddedNatureList.Clear();
            Dictionary<int, NatureDataModel> heroBasicAddedNatureList = new Dictionary<int, NatureDataModel>();

            DataList heroNatureDataList = DataListManager.inst.GetDataList("HeroBasicAddAttribute");
            foreach (var item in heroNatureDataList)
            {
                Data data = item.Value;
                if (!heroBasicAddedNatureList.ContainsKey(data.ID))
                {
                    heroBasicAddedNatureList.Add(data.ID, new NatureDataModel(data));
                }
            }
            NatureLibrary.heroBasicAddedNatureList = heroBasicAddedNatureList;
        }

        private static void InitGroValFactor()
        {
            Dictionary<int, GroValFactorModel> groValFactorList = new Dictionary<int, GroValFactorModel>();
            //groValFactorList.Clear();
            DataList groValFactorDataList = DataListManager.inst.GetDataList("GroValFactor");
            foreach (var item in groValFactorDataList)
            {
                Data data = item.Value;
                if (!groValFactorList.ContainsKey(item.Key))
                {
                    groValFactorList.Add(item.Key, new GroValFactorModel(data));
                }
            }
            NatureLibrary.groValFactorList = groValFactorList;
        }

        private static void InitStepNature()
        {
            Dictionary<int, Dictionary<int, HeroStepNatureModel>> heroStepNatureList = new Dictionary<int, Dictionary<int, HeroStepNatureModel>>();
            //heroStepNatureList.Clear();
            HeroStepNatureModel model = null;
            DataList groValFactorDataList = DataListManager.inst.GetDataList("HeroStepNature");
            foreach (var item in groValFactorDataList)
            {
                model = new HeroStepNatureModel(item.Value);

                Dictionary<int, HeroStepNatureModel> level;
                if (!heroStepNatureList.TryGetValue(model.CardQuality, out level))
                {
                    level = new Dictionary<int, HeroStepNatureModel>();
                    heroStepNatureList.Add(model.CardQuality, level);
                }
                level[model.StepLevel] = model;
            }
            NatureLibrary.heroStepNatureList = heroStepNatureList;
        }

        private static void InitBasicNatureIncreasement()
        {
            //basicNatureIncrList.Clear();
            Dictionary<int, NatureDataModel> basicNatureIncrList = new Dictionary<int, NatureDataModel>();

            DataList natureInrDataList = DataListManager.inst.GetDataList("BasicNatureIncreasement");
            foreach (var item in natureInrDataList)
            {
                Data data = item.Value;
                if (!basicNatureIncrList.ContainsKey(item.Key))
                {
                    basicNatureIncrList.Add(item.Key, new NatureDataModel(data));
                }
            }
            NatureLibrary.basicNatureIncrList = basicNatureIncrList;
        }

        private static void InitBasicNatureMapping()
        {
            //basicNatureMapping.Clear();
            Dictionary<NatureType, Dictionary<NatureType, float>> basicNatureMapping = new Dictionary<NatureType, Dictionary<NatureType, float>>();

            Dictionary<NatureType, float> natureList;
            DataList mappingDataList = DataListManager.inst.GetDataList("BasicNatureMapping");
            foreach (var item in mappingDataList)
            {
                Data data = item.Value;
                natureList = new Dictionary<NatureType, float>();

                data.Foreach(property =>
                {
                    if (property.Type == DataProperty.ValueType.FLOAT || property.Type == DataProperty.ValueType.INT)
                    {
                        NatureType mappingType;
                        if (Enum.TryParse(property.Key, out mappingType))
                        {
                            natureList.Add(mappingType, property.GetFloat());
                        }
                    }
                });

                NatureType type = (NatureType)Enum.Parse(typeof(NatureType), data.GetString("NatureType"));
                basicNatureMapping.Add(type, natureList);
            }
            NatureLibrary.basicNatureMapping = basicNatureMapping;
        }

        //private static void InitBasicNatureFactor()
        //{
        //    basicNatureFactorList.Clear();
        //    DataList natureFactorDataList = DataListManager.inst.GetDataList("BasicNatureFactor");
        //    foreach (var item in natureFactorDataList)
        //    {
        //        Data data = item.Value;
        //        NatureType type = (NatureType)Enum.Parse(typeof(NatureType), data.GetString("NatureType"));
        //        if (!basicNatureFactorList.ContainsKey(type))
        //        {
        //            basicNatureFactorList.Add(type, new BasicNatureFactorModel(data));
        //        }
        //    }
        //}

        public static Dictionary<NatureType, float> GetNature9List(NatureType type)
        {
            Dictionary<NatureType, float> list = null;
            basicNatureMapping.TryGetValue(type, out list);
            return list;
        }

        public static Dictionary<NatureType, Dictionary<NatureType, float>> GetNature4To9List()
        {
            return basicNatureMapping;
        }
        //public static List<BasicNatureFactorModel> GetBasicNature(NatureType type)
        //public static BasicNatureFactorModel GetBasicNatureFactor(NatureType type)
        //{
        //    //List<BasicNatureFactorModel> natureList = new List<BasicNatureFactorModel>();
        //    BasicNatureFactorModel model = null;
        //    basicNatureFactorList.TryGetValue(type, out model);
        //    return model;
        //}

        //public static CommonNatureParamModel GetCommonNatureParam()
        //{
        //    return CommonNatureParamModel;
        //}

        public static NatureDataModel GetBasicNatureIncrModel(int id)
        {
            NatureDataModel model = null;
            basicNatureIncrList.TryGetValue(id, out model);
            return model;
        }

        public static GroValFactorModel GetGroValFactorModel(int id)
        {
            GroValFactorModel model = null;
            groValFactorList.TryGetValue(id, out model);
            return model;
        }

        public static NatureDataModel GetHeroBasicNatureModel(int heroId)
        {
            NatureDataModel model = null;
            heroBasicNatureList.TryGetValue(heroId, out model);
            return model;
        }

        public static NatureDataModel GetHeroBasicAddedNatureModel(int heroId)
        {
            NatureDataModel model = null;
            heroBasicAddedNatureList.TryGetValue(heroId, out model);
            return model;
        }

        public static bool IsBasic4Nature(NatureType type)
        {
            return Basic4Nature.ContainsKey(type);
        }

        public static bool IsBasic9Nature(NatureType type)
        {
            return Basic9Nature.ContainsKey(type);
        }

        public static HeroStepNatureModel GetHeroStepNatureModel(int quelity, int level)
        {
            Dictionary<int, HeroStepNatureModel> levelList;
            if (heroStepNatureList.TryGetValue(quelity, out levelList))
            {
                HeroStepNatureModel model;
                levelList.TryGetValue(level, out model);
                return model;
            }
            return null;
        }
    }

}
