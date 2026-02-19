using System;
using CommonUtility;
using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class GodHeroLibrary
    {
        private static Dictionary<int, HeroGodModel> heroGodList = new Dictionary<int, HeroGodModel>();
        private static Dictionary<int, HeroGodDetailModel> heroGodCostList = new Dictionary<int, HeroGodDetailModel>();
        private static Dictionary<int, HeroGodStepUpModel> heroGodStepUpModels = new Dictionary<int, HeroGodStepUpModel>();
        private static DoubleDepthMap<int, int, HeroGodStepUpGrowthModel> heroStepGrowthModels = new DoubleDepthMap<int, int, HeroGodStepUpGrowthModel>();

        private static List<NatureType> natureTypes = new List<NatureType>
        {
            NatureType.PRO_MAX_HP,
            NatureType.PRO_ATK,
            NatureType.PRO_DEF,
            NatureType.PRO_HIT,
            NatureType.PRO_FLEE,
            NatureType.PRO_CRI,
            NatureType.PRO_RES,
            NatureType.PRO_IMP,
            NatureType.PRO_ARM,
        };

        public static List<NatureType> NatureTypes => natureTypes;

        public static void Init()
        {
            //heroGodList.Clear();
            //heroGodCostList.Clear();

            InitHeroGod();
            InitHeroGodCost();
            InitHeroGodStepUp();
            InitStepUpGrowthModel();
        }

        public static HeroGodModel GetHeroGodModel(int heroId)
        {
            HeroGodModel model;
            heroGodList.TryGetValue(heroId, out model);
            return model;
        }

        public static HeroGodDetailModel GetHeroGodDetailModel(int godType)
        {
            HeroGodDetailModel model;
            heroGodCostList.TryGetValue(godType, out model);
            return model;
        }

        public static HeroGodStepUpModel GetHeroGodStepUpModel(int heroId)
        {
            HeroGodStepUpModel model;
            heroGodStepUpModels.TryGetValue(heroId, out model);
            return model;
        }

        public static HeroGodStepUpGrowthModel GetGodStepUpGrowthModel(int godType, int step)
        {
            //如果当前装备的是基础神位，即使神阶非常高也只有基础神位的效果
            HeroGodDetailModel detailModel = GetHeroGodDetailModel(godType);
            if (step < HeroLibrary.HeroStepMax || detailModel?.IsPrimaryGod == true)
            {
                step = HeroLibrary.HeroStepMax;
            }

            HeroGodStepUpGrowthModel model;
            heroStepGrowthModels.TryGetValue(godType, step, out model);
            return model;
        }

        private static void InitHeroGod()
        {
            Dictionary<int, HeroGodModel> heroGodList = new Dictionary<int, HeroGodModel>();

            DataList dataList = DataListManager.inst.GetDataList("HeroGod");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                heroGodList.Add(item.Key, new HeroGodModel(data));
            }
            GodHeroLibrary.heroGodList = heroGodList;
        }

        private static void InitHeroGodCost()
        {
            Dictionary<int, HeroGodDetailModel> heroGodCostList = new Dictionary<int, HeroGodDetailModel>();

            DataList dataList = DataListManager.inst.GetDataList("HeroGodDetail");
            foreach (var item in dataList)
            {
                HeroGodDetailModel model = new HeroGodDetailModel(item.Value);
                heroGodCostList.Add(model.Id, model);
            }
            GodHeroLibrary.heroGodCostList = heroGodCostList;
        }

        private static void InitHeroGodStepUp()
        {
            Dictionary<int, HeroGodStepUpModel> heroGodStepUpModels = new Dictionary<int, HeroGodStepUpModel>();

            DataList dataList = DataListManager.inst.GetDataList("HeroGodStepUp");
            foreach (var item in dataList)
            {
                HeroGodStepUpModel model = new HeroGodStepUpModel(item.Value);
                heroGodStepUpModels.Add(model.HeroId, model);
            }
            GodHeroLibrary.heroGodStepUpModels = heroGodStepUpModels;
        }

        private static void InitStepUpGrowthModel()
        {
            DoubleDepthMap<int, int, HeroGodStepUpGrowthModel> heroStepGrowthModels = new DoubleDepthMap<int, int, HeroGodStepUpGrowthModel>();

            DataList groValFactorDataList = DataListManager.inst.GetDataList("HeroGodStepUpGrowth");
            foreach (var item in groValFactorDataList)
            {
                HeroGodStepUpGrowthModel model = new HeroGodStepUpGrowthModel(item.Value);
                heroStepGrowthModels.Add(model.GodType, model.Step, model);
            }
            GodHeroLibrary.heroStepGrowthModels = heroStepGrowthModels;
        }

    }
}
