using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.ModelBinding;

namespace ServerShared
{
    public static class SoulSkillLibrary
    {
        //private static Dictionary<int, NatureDataModel> basicNatureList = new Dictionary<int, NatureDataModel>();
        private static Dictionary<int, NatureDataModel> natureIncrList = new Dictionary<int, NatureDataModel>();
        private static Dictionary<int, SoulSkillEnhanceModel> soulSkillEnhances = new Dictionary<int, SoulSkillEnhanceModel>();
        private static Dictionary<int, SoulSkillBreakModel> soulSkillBreaks = new Dictionary<int, SoulSkillBreakModel>();

        public static void Init()
        {
            //soulSkillEnhances.Clear();
            //soulSkillBreaks.Clear();
            //basicNatureList.Clear();
            //natureIncrList.Clear();

            DataList enhanceDataList = DataListManager.inst.GetDataList("SoulSkillEnhance");
            DataList breakDataList = DataListManager.inst.GetDataList("SoulSkillBreak");
            //DataList basicNatureDataList = DataListManager.inst.GetDataList("SoulSkillBasicAttribute");
            DataList basicNatureIncrDataList = DataListManager.inst.GetDataList("SoulSkillBasicNatureIncreasement");

            //InitSoulRingBasicNature(basicNatureDataList);
            InitSoulRingNatureIncreasement(basicNatureIncrDataList);
            InitSoulSkillEnhance(enhanceDataList);
            InitSoulSkillBreak(breakDataList);
        }

        private static void InitSoulSkillEnhance(DataList dataList)
        {
            Dictionary<int, SoulSkillEnhanceModel> soulSkillEnhances = new Dictionary<int, SoulSkillEnhanceModel>();
            Data data = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;

                SoulSkillEnhanceModel model = new SoulSkillEnhanceModel()
                {
                    Level = data.ID,
                    DustCost = data.GetInt("DustCost"),
                    GoldCost = data.GetInt("GoldCost"),
                    //DustTotalCost = data.GetInt("DustTotalCost"),
                    //GoldTotalCost = data.GetInt("GoldTotalCost"),
                };
                soulSkillEnhances.Add(model.Level, model);
            }
            SoulSkillLibrary.soulSkillEnhances = soulSkillEnhances;
        }

        private static void InitSoulSkillBreak(DataList dataList)
        {
            Dictionary<int, SoulSkillBreakModel> soulSkillBreaks = new Dictionary<int, SoulSkillBreakModel>();
            Data data = null;
            foreach (var kv in dataList)
            {
                data = kv.Value;

                SoulSkillBreakModel model = new SoulSkillBreakModel()
                {
                    Level = data.ID,
                    BreathCost = data.GetInt("BreathCost"),
                    GoldCost = data.GetInt("GoldCost"),
                    //BreathTotalCost = data.GetInt("BreathTotalCost"),
                    //GoldTotalCost = data.GetInt("GoldTotalCost"),
                };
                soulSkillBreaks.Add(model.Level, model);
            }
            SoulSkillLibrary.soulSkillBreaks = soulSkillBreaks;
        }

        //public static NatureDataModel GetBasicNatureModel(int id)
        //{
        //    NatureDataModel model;
        //    basicNatureList.TryGetValue(id, out model);
        //    return model;
        //}

        public static NatureDataModel GetBasicNatureIncrModel(int id)
        {
            NatureDataModel model;
            natureIncrList.TryGetValue(id, out model);
            return model;
        }

        //private static void InitSoulRingBasicNature(DataList dataList)
        //{
        //    foreach (var kv in dataList)
        //    {
        //        basicNatureList[kv.Key] = new NatureDataModel(kv.Value);
        //    }
        //}

        private static void InitSoulRingNatureIncreasement(DataList dataList)
        {
            Dictionary<int, NatureDataModel> natureIncrList = new Dictionary<int, NatureDataModel>();
            foreach (var kv in dataList)
            {
                natureIncrList[kv.Key] = new NatureDataModel(kv.Value);
            }
            SoulSkillLibrary.natureIncrList = natureIncrList;
        }

        public static SoulSkillEnhanceModel GetSoulSkillEnhanceMode(int level)
        {
            SoulSkillEnhanceModel temp;
            soulSkillEnhances.TryGetValue(level, out temp);
            return temp;
        }

        public static SoulSkillBreakModel GetSoulSkillBreakMode(int level)
        {
            SoulSkillBreakModel temp;
            soulSkillBreaks.TryGetValue(level, out temp);
            return temp;
        }

    }
}
