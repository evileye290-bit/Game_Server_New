using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using EnumerateUtility;
using EnumerateUtility.SpaceTimeTower;
using ServerModels;

namespace ServerShared
{
    public class SpaceTimeTowerLibrary
    {
        public static int TowerMaxLevel;
        public static int HeroMaxNum;
        public static int HeroLevel;
        public static int SoulRingSkillLevel;
        public static int StepInitLevel;
        public static int StepMaxLevel;
        public static int iRefreshHeroMaxNum; /*/ 卡池刷新的最大英雄数量 /*/
        public static int iRefreshCardPoolMaxNum; /*/ 手动刷新卡池的最大次数 /*/
        public static int OpenDays;
        public static int StartWeekDay;
        public static string StartWeekTime;
        public static int InitSpaceTimeCoins;
        public static int ChallengeMaxCount;
        private static List<int> growthEffectMonsterNatures = new List<int>();
        public static int iBuyExpendType;
        public static int MonsterDifficultyLimit;
        public static DateTime StartDateTime;
        public static int MaxWeek;

        /*\ <组id, <英雄信息>> /*/
        private static DoubleDeapthListMap<int, int, SpaceTimeHeroRandom> dicHeroRandom =
            new DoubleDeapthListMap<int, int, SpaceTimeHeroRandom>();
        private static Dictionary<int, SpaceTimeTowerLevel> towerLevelDic = new Dictionary<int, SpaceTimeTowerLevel>();
        private static Dictionary<int, NatureDataModel> heroBasicNatureList = new Dictionary<int, NatureDataModel>();
        private static Dictionary<int, GroValFactorModel> heroStepGrowthList = new Dictionary<int, GroValFactorModel>();

        private static Dictionary<int, Dictionary<int, HeroStepNatureModel>> heroStepNatureList =
            new Dictionary<int, Dictionary<int, HeroStepNatureModel>>();

        private static Dictionary<int, Dictionary<int, float>> monsterNatureGrowths = new Dictionary<int, Dictionary<int, float>>();

        /*\ <当前难度(阶段), <页签, 表数据>> /*/
        private static Dictionary<int, Dictionary<int, List<SpaceTimeTowerStageAward>>> dicSpaceTimeStageAward =
            new Dictionary<int, Dictionary<int, List<SpaceTimeTowerStageAward>>>();

        /*\ 最大阶段 （如果当前超过最大阶段按照最大阶段处理） /*/
        public static int iMaxStage;

        #region [时空塔商城时空屋相关]

        /*\ <tbId, 数据> /*/
        private static Dictionary<int, SpaceTimeTowerProduct> dicSpaceTimeProductInfo =
            new Dictionary<int, SpaceTimeTowerProduct>();
        /*\ <商店id, 商城数据> /*/
        private static Dictionary<int, SpaceTimeTowerShop> dicSpaceTimeShopInfo =
            new Dictionary<int, SpaceTimeTowerShop>();
        
        #endregion

        private static Dictionary<int, GuideSoulItemModel> guideSoulItems = new Dictionary<int, GuideSoulItemModel>();

        private static Dictionary<int, GuideSoulEffectModel> guideSoulEffects =
            new Dictionary<int, GuideSoulEffectModel>();
        
        private static Dictionary<int, SpaceTimeRecycleHeroRewards> recycleHeroRewards = new Dictionary<int,SpaceTimeRecycleHeroRewards>();

        private static SortedDictionary<int, string> pastRewards = new SortedDictionary<int, string>();

        public static void Init()
        {
            InitConfig();
            InitTowerLevel();
            initHeroRandom();
            InitHeroBasicNature();
            InitHeroStepGrowth();
            InitStepNature();
            InitMonsterNatureGrowth();
            initStageAward();
            initSpaceTimeProduct();
            initSpaceTimeShop();
            InitGuideSoulItems();
            InitGuideSoulEffect();
            InitRecycleHeroRewards();
            InitPastRewards();
        }

        public static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("SpaceTimeTowerConfig", 1);
            TowerMaxLevel = data.GetInt("TowerMaxLevel");
            HeroMaxNum = data.GetInt("HeroMaxNum");
            HeroLevel = data.GetInt("HeroLevel");
            SoulRingSkillLevel = data.GetInt("SoulRingSkillLevel");
            StepInitLevel = data.GetInt("StepInitLevel");
            StepMaxLevel = data.GetInt("StepMaxLevel");
            iRefreshHeroMaxNum = data.GetInt("RefreshHeroMaxNum");
            iRefreshCardPoolMaxNum = data.GetInt("RefreshCardPoolMaxNum");
            growthEffectMonsterNatures = data.GetIntList("GrowthEffectMonsterNatures", "|");
            OpenDays = data.GetInt("OpenDays");
            StartWeekDay = data.GetInt("StartWeekDay");
            StartWeekTime = data.GetString("StartWeekTime");
            InitSpaceTimeCoins = data.GetInt("InitSpaceTimeCoins");
            ChallengeMaxCount = data.GetInt("ChallengeMaxCount");
            iBuyExpendType = data.GetInt("BuyExpendType");
            MonsterDifficultyLimit = data.GetInt("MonsterDifficultyLimit");
            string startDt = data.GetString("StartDateTime");
            DateTime.TryParse(startDt, out StartDateTime);
        }

        #region [表预处理]

        /// <summary>
        /// 预处理表 SpaceTimeHeroRandom.xml
        /// </summary>
        private static void initHeroRandom()
        {
            DoubleDeapthListMap<int, int, SpaceTimeHeroRandom> dicHeroRandom =
                new DoubleDeapthListMap<int, int, SpaceTimeHeroRandom>();
            DataList lstXmlData = DataListManager.inst.GetDataList("SpaceTimeHeroRandom");
            foreach (var info in lstXmlData)
            {
                SpaceTimeHeroRandom oRandomHero = new SpaceTimeHeroRandom(info.Value);
                dicHeroRandom.Add(oRandomHero.iWeek, oRandomHero.iGroupId, oRandomHero);
                if (oRandomHero.iWeek > MaxWeek)
                {
                    MaxWeek = oRandomHero.iWeek;
                }
            }

            SpaceTimeTowerLibrary.dicHeroRandom = dicHeroRandom;
        }

        /// <summary>
        /// 初始化表 SpaceTimeTowerStageAward
        /// </summary>
        private static void initStageAward()
        {
            Dictionary<int, Dictionary<int, List<SpaceTimeTowerStageAward>>> dicStageAward =
                new Dictionary<int, Dictionary<int, List<SpaceTimeTowerStageAward>>>();

            DataList lstXmlData = DataListManager.inst.GetDataList("SpaceTimeTowerStageAward");
            foreach (var info in lstXmlData)
            {
                SpaceTimeTowerStageAward oStageAward = new SpaceTimeTowerStageAward(info.Value);
                if (oStageAward.iStage > iMaxStage)
                {
                    iMaxStage = oStageAward.iStage;
                }
                if (!dicStageAward.ContainsKey(oStageAward.iStage))
                {
                    dicStageAward.Add(oStageAward.iStage, new Dictionary<int, List<SpaceTimeTowerStageAward>>());
                }

                if (!dicStageAward[oStageAward.iStage].ContainsKey(oStageAward.iPage))
                {
                    dicStageAward[oStageAward.iStage].Add(oStageAward.iPage, new List<SpaceTimeTowerStageAward>());
                }

                if (!dicStageAward[oStageAward.iStage][oStageAward.iPage].Contains(oStageAward))
                {
                    dicStageAward[oStageAward.iStage][oStageAward.iPage].Add(oStageAward);
                }
            }

            SpaceTimeTowerLibrary.dicSpaceTimeStageAward = dicStageAward;
        }

        /// <summary>
        /// 初始化表 initSpaceTimeProduct
        /// </summary>
        private static void initSpaceTimeProduct()
        {
            Dictionary<int, SpaceTimeTowerProduct> dicProduct = new Dictionary<int, SpaceTimeTowerProduct>();

            DataList lstXmlData = DataListManager.inst.GetDataList("SpaceTimeTowerProduct");
            foreach (var info in lstXmlData)
            {
                SpaceTimeTowerProduct oProductInfo = new SpaceTimeTowerProduct(info.Value);
                if (!dicProduct.ContainsKey(oProductInfo.iId))
                {
                    dicProduct.Add(oProductInfo.iId, oProductInfo);
                }
            }

            SpaceTimeTowerLibrary.dicSpaceTimeProductInfo = dicProduct;
        }

        /// <summary>
        /// 初始化表 SpaceTimeTowerShop
        /// </summary>
        private static void initSpaceTimeShop()
        {
            Dictionary<int, SpaceTimeTowerShop> dicShopInfo = new Dictionary<int, SpaceTimeTowerShop>();

            DataList lstXmlData = DataListManager.inst.GetDataList("SpaceTimeTowerShop");
            foreach (var info in lstXmlData)
            {
                SpaceTimeTowerShop oShopInfo = new SpaceTimeTowerShop(info.Value);
                if (!dicShopInfo.ContainsKey(oShopInfo.iId))
                {
                    dicShopInfo.Add(oShopInfo.iId, oShopInfo);
                }
            }

            SpaceTimeTowerLibrary.dicSpaceTimeShopInfo = dicShopInfo;
        }

        /// <summary>
        /// 初始化表 SpaceTimeRecycleHeroReward
        /// </summary>
        private static void InitRecycleHeroRewards()
        {
            Dictionary<int, SpaceTimeRecycleHeroRewards> recycleHeroRewards = new Dictionary<int,SpaceTimeRecycleHeroRewards>();

            DataList dataList = DataListManager.inst.GetDataList("SpaceTimeRecycleHeroReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                SpaceTimeRecycleHeroRewards recycleModel = new SpaceTimeRecycleHeroRewards(data);
                recycleHeroRewards.Add(recycleModel.Id, recycleModel);
            }

            SpaceTimeTowerLibrary.recycleHeroRewards = recycleHeroRewards;
        }
        
        /// <summary>
        /// 初始化表 SpaceTimeTowerPastRewards
        /// </summary>
        private static void InitPastRewards()
        {
            SortedDictionary<int, string> pastRewards = new SortedDictionary<int, string>();

            DataList dataList = DataListManager.inst.GetDataList("SpaceTimeTowerPastRewards");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                pastRewards.Add(data.ID, data.GetString("Rewards"));
            }

            SpaceTimeTowerLibrary.pastRewards = pastRewards;
        }
        #endregion

        #region [公共方法]

        /// <summary>
        /// 根据组id返回随机英雄列表
        /// </summary>
        /// <param name="iWeek">活动周数</param>
        /// <param name="iGroupId">组id</param>
        /// <returns></returns>
        public static List<SpaceTimeHeroRandom> GetRandomHeroInfoWithGroupId(int iWeek, int iGroupId)
        {
            iWeek = iWeek > MaxWeek ? MaxWeek : iWeek;
            List<SpaceTimeHeroRandom> dicInfo;
            if (!dicHeroRandom.TryGetValue(iWeek, iGroupId, out dicInfo))
            {
                return null;
            }

            return dicInfo;
        }

        /// <summary>
        /// 根据阶段和页签返回对应的奖励数据信息
        /// </summary>
        /// <param name="iStage">阶段</param>
        /// <param name="iPage">页签</param>
        /// <returns></returns>
        public static List<SpaceTimeTowerStageAward> GetStageAwardInfo(int iStage, int iPage)
        {
            Dictionary<int, List<SpaceTimeTowerStageAward>> dicInfo;
            if (!dicSpaceTimeStageAward.TryGetValue(iStage, out dicInfo))
            {
                return null;
            }

            List<SpaceTimeTowerStageAward> lstInfo;
            if (!dicInfo.TryGetValue(iPage, out lstInfo))
            {
                return null;
            }

            return lstInfo;
        }

        /// <summary>
        /// 根据商品id返回商品信息
        /// </summary>
        /// <param name="iProductId">商品id</param>
        /// <returns></returns>
        public static SpaceTimeTowerProduct GetProductInfo(int iProductId)
        {
            SpaceTimeTowerProduct oProduct;
            dicSpaceTimeProductInfo.TryGetValue(iProductId, out oProduct);
            return oProduct;
        }

        /// <summary>
        /// 返回商城信息
        /// </summary>
        /// <param name="iShopId"></param>
        /// <returns></returns>
        public static SpaceTimeTowerShop GetShopInfo(int iShopId)
        {
            SpaceTimeTowerShop oShop;
            dicSpaceTimeShopInfo.TryGetValue(iShopId, out oShop);
            return oShop;
        }

        /// <summary>
        /// 根据突破等级返回回收英雄奖励
        /// </summary>
        /// <param name="stepLevel">突破星级</param>
        public static SpaceTimeRecycleHeroRewards GetRecycleHeroRewards(int stepLevel)
        {
            SpaceTimeRecycleHeroRewards rewardModel;
            recycleHeroRewards.TryGetValue(stepLevel, out rewardModel);
            return rewardModel;
        }
        
        /// <summary>
        /// 根据阶段返回对应的奖励数据信息
        /// </summary>
        /// <param name="iStage"></param>
        /// <returns></returns>
        public static int GetStageAwardListCount(int iStage)
        {
            int count = 0;
            Dictionary<int, List<SpaceTimeTowerStageAward>> dicInfo;
            if (!dicSpaceTimeStageAward.TryGetValue(iStage, out dicInfo))
            {
                return 0;
            }
            foreach (var kv in dicInfo)
            {
                count += kv.Value.Count;
            }
            return count;
        }

        /// <summary>
        /// 根据往期奖励阶段返回数据
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static List<string> GetPastRewardsByStage(int stage)
        {
            List<string> rewards = new List<string>();
            foreach (var stageRewards in pastRewards)
            {
                if (stageRewards.Key > stage)break;
                rewards.Add(stageRewards.Value);
            }

            return rewards;
        }
        #endregion


        private static void InitTowerLevel()
        {
            Dictionary<int, SpaceTimeTowerLevel> towerLevelDic = new Dictionary<int, SpaceTimeTowerLevel>();

            DataList dataList = DataListManager.inst.GetDataList("SpaceTimeTowerLevel");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                SpaceTimeTowerLevel towerLevel = new SpaceTimeTowerLevel(data);
                towerLevelDic.Add(towerLevel.Id, towerLevel);
            }

            SpaceTimeTowerLibrary.towerLevelDic = towerLevelDic;
        }

        public static SpaceTimeTowerLevel GetTowerLevelModel(int level)
        {
            SpaceTimeTowerLevel model;
            towerLevelDic.TryGetValue(level, out model);
            return model;
        }

        private static void InitHeroBasicNature()
        {
            Dictionary<int, NatureDataModel> heroBasicNatureList = new Dictionary<int, NatureDataModel>();

            DataList heroNatureDataList = DataListManager.inst.GetDataList("SpaceTimeHeroNature");
            foreach (var item in heroNatureDataList)
            {
                Data data = item.Value;
                if (!heroBasicNatureList.ContainsKey(data.ID))
                {
                    heroBasicNatureList.Add(data.ID, new NatureDataModel(data));
                }
            }

            SpaceTimeTowerLibrary.heroBasicNatureList = heroBasicNatureList;
        }

        public static NatureDataModel GetHeroBasicNatureModel(int heroId)
        {
            NatureDataModel model = null;
            heroBasicNatureList.TryGetValue(heroId, out model);
            return model;
        }

        private static void InitHeroStepGrowth()
        {
            Dictionary<int, GroValFactorModel> heroStepGrowthList = new Dictionary<int, GroValFactorModel>();
            //groValFactorList.Clear();
            DataList groValFactorDataList = DataListManager.inst.GetDataList("SpaceTimeHeroStepGrowth");
            foreach (var item in groValFactorDataList)
            {
                Data data = item.Value;
                if (!heroStepGrowthList.ContainsKey(item.Key))
                {
                    heroStepGrowthList.Add(item.Key, new GroValFactorModel(data));
                }
            }

            SpaceTimeTowerLibrary.heroStepGrowthList = heroStepGrowthList;
        }

        public static GroValFactorModel GetHeroStepGrowthModel(int id)
        {
            GroValFactorModel model = null;
            heroStepGrowthList.TryGetValue(id, out model);
            return model;
        }

        private static void InitStepNature()
        {
            Dictionary<int, Dictionary<int, HeroStepNatureModel>> heroStepNatureList =
                new Dictionary<int, Dictionary<int, HeroStepNatureModel>>();
            HeroStepNatureModel model = null;
            DataList groValFactorDataList = DataListManager.inst.GetDataList("SpaceTimeHeroStepNature");
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

            SpaceTimeTowerLibrary.heroStepNatureList = heroStepNatureList;
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

        public static List<int> GetGrowthEffectMonsterNatures()
        {
            return growthEffectMonsterNatures;
        }

        private static void InitMonsterNatureGrowth()
        {
            Dictionary<int, Dictionary<int, float>> monsterNatureGrowths = new Dictionary<int, Dictionary<int, float>>();

            Dictionary<int, float> dic;
            DataList dataList = DataListManager.inst.GetDataList("SpaceTimeMonsterNatureGrowth");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int period = data.GetInt("Period");
                int towerLevel = data.GetInt("TowerLevel");
                float factor = data.GetFloat("Growth");
                
                if (!monsterNatureGrowths.TryGetValue(period, out dic))
                {
                    dic = new Dictionary<int, float>();
                    monsterNatureGrowths.Add(period, dic);
                }
                dic.Add(towerLevel, factor);
            }

            SpaceTimeTowerLibrary.monsterNatureGrowths = monsterNatureGrowths;
        }

        public static float GetMonsterNatureGrowth(int period, int towerLevel)
        {
            float growth = 0.0f;
            Dictionary<int, float> dic;
            if (monsterNatureGrowths.TryGetValue(period, out dic) && dic.TryGetValue(towerLevel, out growth))
            {
                return growth;
            }
            return growth;
        }

        private static void InitGuideSoulItems()
        {
            Dictionary<int, GuideSoulItemModel> guideSoulItems = new Dictionary<int, GuideSoulItemModel>();
            
            DataList dataList = DataListManager.inst.GetDataList("GuideSoulItems");
            GuideSoulItemModel model;
            foreach (var item in dataList)
            {
                model = new GuideSoulItemModel(item.Value);
                guideSoulItems.Add(model.Id, model);
            }

            SpaceTimeTowerLibrary.guideSoulItems = guideSoulItems;
        }

        public static GuideSoulItemModel GetGuideSoulItemModel(int id)
        {
            GuideSoulItemModel item;
            guideSoulItems.TryGetValue(id, out item);
            return item;
        }

        private static void InitGuideSoulEffect()
        {
            Dictionary<int, GuideSoulEffectModel> guideSoulEffects = new Dictionary<int, GuideSoulEffectModel>();
            
            DataList dataList = DataListManager.inst.GetDataList("GuideSoulEffect");
            GuideSoulEffectModel model;
            foreach (var item in dataList)
            {
                model = new GuideSoulEffectModel(item.Value);
                guideSoulEffects.Add(model.Id, model);
            }

            SpaceTimeTowerLibrary.guideSoulEffects = guideSoulEffects;
        }
        
        public static GuideSoulEffectModel GetGuideSoulEffectModel(int id)
        {
            GuideSoulEffectModel model;
            guideSoulEffects.TryGetValue(id, out model);
            return model;
        }
    }
}