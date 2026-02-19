using ServerModels;
using System.Collections.Generic;
using DataProperty;
using System.Linq;
using EnumerateUtility;
using CommonUtility;

namespace ServerShared
{
    /// <summary>
    /// 对弈七斗罗某一场战斗信息
    /// </summary>
    public class GodPathCardFightInfo
    {
        public List<int> CardList { get; set; }
        public bool IsReward { get; set; }

        public int RandomCard()
        {
            return CardList[RAND.Range(0, CardList.Count - 1)];
        }
    }

    /// <summary>
    /// 对弈七斗罗某一阶段信息
    /// </summary>
    public class GodPathStageFightInfo
    {
        Dictionary<int, GodPathCardFightInfo> fightCards = new Dictionary<int, GodPathCardFightInfo>();
        List<string> rewards = new List<string>();//当前阶段所有场次奖励
        public List<string> Rewards => rewards;

        public void AddFightCards(int fight, List<int> cardList, bool isReward, string reward)
        {
            fightCards.Add(fight, new GodPathCardFightInfo() {  CardList = cardList, IsReward = isReward });
            rewards.Add(reward);
        }

        public GodPathCardFightInfo GetCardFightInfo(int fight)
        {
            GodPathCardFightInfo info;
            fightCards.TryGetValue(fight, out info);
            return info;
        }
    }


    public class GodPathStageShieldHero
    {
        Dictionary<int, List<int>> fightShieldHeroIds = new Dictionary<int, List<int>>();

        public void AddFightShieldHero(int fight, List<int> shieldHeroIds)
        {
            fightShieldHeroIds.Add(fight, shieldHeroIds);
        }

        public List<int> GetShieldHeroIds(int fight)
        {
            List<int> ids;
            fightShieldHeroIds.TryGetValue(fight, out ids);
            return ids;
        }
    }

    public class GodPathLibrary
    {
        private static Dictionary<int, Dictionary<int, List<GodPathTaskModel>>> heroGodPathStageTask = new Dictionary<int, Dictionary<int, List<GodPathTaskModel>>>();
        private static Dictionary<int, int> stageNeedAssign = new Dictionary<int, int>();//每个结算需要的亲和力
        private static Dictionary<int, int> drugAddAssign = new Dictionary<int, int>();//药水添加的亲和力

        private static Dictionary<int, Dictionary<int, GodPathStageFightInfo>> heroSevenFightCard = new Dictionary<int, Dictionary<int, GodPathStageFightInfo>>();//<heroid,<stage>>

        private static Dictionary<int, int> trainBodyWaterPower = new Dictionary<int, int>();//炼体海流冲击力
        private static Dictionary<int, GodPathHeroShieldModel> trainBodyHeroShield = new Dictionary<int, GodPathHeroShieldModel>();
        private static Dictionary<int, Dictionary<int, GodPathStageShieldHero>> trainBodyShieldHero = new Dictionary<int, Dictionary<int, GodPathStageShieldHero>>();//<heroid,<stage>>

        private static Dictionary<int, GodPathHeartMode> oceanHeartList = new Dictionary<int, GodPathHeartMode>();

        private static Dictionary<int, Dictionary<int, int>> acrossOceanExternPicture = new Dictionary<int, Dictionary<int, int>>();


        public static int HeroOpenLevel { get; private set; }
        public static int MaxStage { get; private set; }
        public static int CardPrice { get; private set; }

        public static int AddAffinityItemIdSmall { get; private set; }
        public static int AddAffinityItemIdNormal { get; private set; }
        public static int AddAffinityItemIdBig { get; private set; }

        #region 七战

        public static int SevenFightPower { get; private set; }
        public static int SevenFightGivePower { get; private set; }
        public static int SevenFightCostPower { get; private set; }
        public static int SevenFightNextStagePower { get; private set; }
        public static int SevenFightMaxStage { get; private set; }
        public static int SevenFightWinCountToNextStage { get; private set; }

        #endregion

        #region 炼体

        public static int TrainBodyHP { get; private set; }
        public static int TrainBodyMaxStage { get; private set; }
        public static int TrainBodyBasicShield { get; private set; }
        public static int TrainBodyAddShield { get; private set; }
        public static int TrainBodyBuyCost { get; private set; }

        #endregion


        #region 海洋之心
        public static int HeartMaxValue { get; private set; }
        public static int HeartRewardDouble { get; private set; }
        public static int HeartRewardDoubleNum { get; private set; }
        public static int HeartRewardTreble { get; private set; }
        public static int HeartRewardTrebleNum { get; private set; }
        public static int HeartMaxBuyCount { get; private set; }
        public static string HeartBuyCountPrice { get; private set; }
        public static int HeartUseCount { get; private set; }
        #endregion


        #region 海神三叉戟
        public static int TridentMaxValue { get; private set; }
        public static int TridentUseCount { get; private set; }
        public static int TridentMaxBuyCount { get; private set; }
        public static string TridentBuyCountPrice { get; private set; }
        public static int TridentSuccessAdd { get; private set; }
        public static int TridentFailAdd { get; private set; }
        public static int TridentGreatAdd { get; private set; }
        public static int TridentTime { get; private set; }
        public static int TridentCheckTime { get; private set; }
        public static int TridentStrategyType { get; private set; }
        public static int TridentStrategyPrice { get; private set; }
        public static int TridentRandomType { get; private set; }
        public static int TridentRandomPrice { get; private set; }

        private static Dictionary<int, int> TridentRandomRatio = new Dictionary<int, int>();
        private static int TridentRandomTotalRatio { get; set; }

        #endregion

        #region 穿越海洋

        public static int AcrossOceanItemId { get; private set; }
        public static int AcrossOceanPuzzleCost { get; private set; }
        public static int AcrossOceanPuzzleCount { get; private set; }


        #endregion

        public static void Init()
        {
            // heroGodPathStageTask.Clear();
            // stageNeedAssign.Clear();
            // drugAddAssign.Clear();
            // heroSevenFightCard.Clear();
            // trainBodyWaterPower.Clear();
            // trainBodyHeroShield.Clear();
            // trainBodyShieldHero.Clear();
            // oceanHeartList.Clear();
            // acrossOceanExternPicture.Clear();
            // heightEventGridList.Clear();
            //
            // heightEventInfos.Clear();
            // heightEventWeight.Clear();
            // heightBuyInfos.Clear();
            // heightBuyWeight.Clear();
            // validControlNum.Clear();
            // heightReward.Clear();

            InitGodPathConfig();
            InitGodPathTask();
            InitSeventFightCard();
            InitTrainBodyHeroShield();
            InitOceanHeart();
            InitAcrossOcean();
        }

        public static int GetWaterPower(int stage)
        {
            int power = 0;
            if (trainBodyWaterPower.TryGetValue(stage, out power));
            return power;
        }

        #region task

        private static void InitGodPathTask()
        {
            Dictionary<int, Dictionary<int, List<GodPathTaskModel>>> heroGodPathStageTask = new Dictionary<int, Dictionary<int, List<GodPathTaskModel>>>();
            DataList list = DataListManager.inst.GetDataList("GodPathTask");
            foreach (var kv in list)
            {
                GodPathTaskModel model = new GodPathTaskModel(kv.Value);

                List<int> heroList = StringSplit.GetInts(kv.Value.GetString("HeroId"));
                foreach (int heroId in heroList)
                {
                    Dictionary<int, List<GodPathTaskModel>> heroTasks;
                    if (!heroGodPathStageTask.TryGetValue(heroId, out heroTasks))
                    {
                        heroTasks = new Dictionary<int, List<GodPathTaskModel>>();
                        heroGodPathStageTask.Add(heroId, heroTasks);
                    }

                    List<GodPathTaskModel> models;
                    if (!heroTasks.TryGetValue(model.Stage, out models))
                    {
                        models = new List<GodPathTaskModel>();
                        heroTasks.Add(model.Stage, models);
                    }
                    models.Add(model);
                }
            }

            GodPathLibrary.heroGodPathStageTask = heroGodPathStageTask;
        }

        public static List<GodPathTaskModel> GetHeroStageTask(int heroId, int stage)
        {
            Dictionary<int, List<GodPathTaskModel>> heroTasks;
            if (heroGodPathStageTask.TryGetValue(heroId, out heroTasks))
            {
                List<GodPathTaskModel> models;
                if (heroTasks.TryGetValue(stage, out models))
                {
                    return models;
                }
            }
            return null;
        }

        public static int GetCostAffinity(int stage)
        {
            int value;
            stageNeedAssign.TryGetValue(stage, out value);
            return value;
        }

        public static int GetDrugAddAffinity(int id)
        {
            int value;
            drugAddAssign.TryGetValue(id, out value);
            return value;
        }

        #endregion

        private static void InitGodPathConfig()
        {
            Dictionary<int, int> stageNeedAssign = new Dictionary<int, int>();
            Dictionary<int, int> drugAddAssign = new Dictionary<int, int>();
            Dictionary<int, int> trainBodyWaterPower = new Dictionary<int, int>();
            //List<int> validControlNum = new List<int>();
            
            Data data = DataListManager.inst.GetData("GodPathConfig", 1);
            HeroOpenLevel = data.GetInt("HeroOpenLevel");

            string[] param = data.GetString("Affinity").Split('|');
            if (param.Length != 9)
            {
                Logger.Log.Error($"InitGodPathConfig Affinbity error, {data.GetString("Affinity")}");
            }
            for (int i = 0; i < param.Length; ++i) 
            {
                stageNeedAssign.Add(i + 1, int.Parse(param[i]));
            }
            MaxStage = stageNeedAssign.Keys.Max();

            CardPrice = data.GetInt("CardPrice");
            SevenFightPower = data.GetInt("SevenFightPower");
            SevenFightGivePower = data.GetInt("SevenFightPower");
            SevenFightCostPower = data.GetInt("SevenFightCostPower");
            SevenFightNextStagePower = data.GetInt("SevenFightNextStagePower");
            SevenFightMaxStage = data.GetInt("SevenFightMaxStage");
            SevenFightWinCountToNextStage = data.GetInt("SevenFightWinCountToNextStage");
            TrainBodyHP = data.GetInt("TrainBodyHP");
            TrainBodyAddShield = data.GetInt("TrainBodyAddShield");
            TrainBodyBasicShield = data.GetInt("TrainBodyBasicShield");
            TrainBodyBuyCost = data.GetInt("TrainBodyBuyCost");
            List<int> power = StringSplit.GetInts(data.GetString("TrainBodyWaterPower"));
            for (int i = 0; i < power.Count; ++i)
            {
                trainBodyWaterPower.Add(i + 1, power[i]);
            }
            TrainBodyMaxStage = trainBodyWaterPower.Keys.Max();

            string[] itemIds = data.GetString("AddAffinityItemId").Split(':');
            drugAddAssign.Add(int.Parse(itemIds[0]), data.GetInt("DrugSmallAddAffinity"));
            drugAddAssign.Add(int.Parse(itemIds[1]), data.GetInt("DrugNormalAddAffinity"));
            drugAddAssign.Add(int.Parse(itemIds[2]), data.GetInt("DrugBigAddAffinity"));


            HeartMaxValue = data.GetInt("HeartMaxValue");
            HeartRewardDouble = data.GetInt("HeartRewardDouble");
            HeartRewardDoubleNum = data.GetInt("HeartRewardDoubleNum");
            HeartRewardTreble = data.GetInt("HeartRewardTreble");
            HeartRewardTrebleNum = data.GetInt("HeartRewardTrebleNum");
            HeartMaxBuyCount = data.GetInt("HeartMaxBuyCount");
            HeartBuyCountPrice = data.GetString("HeartBuyCountPrice");
            HeartUseCount = data.GetInt("HeartUseCount");

            TridentMaxValue = data.GetInt("TridentMaxValue");
            TridentUseCount = data.GetInt("TridentUseCount");
            TridentMaxBuyCount = data.GetInt("TridentMaxBuyCount");
            TridentBuyCountPrice = data.GetString("TridentBuyCountPrice");
            TridentSuccessAdd = data.GetInt("TridentSuccessAdd");
            TridentFailAdd = data.GetInt("TridentFailAdd");
            TridentGreatAdd = data.GetInt("TridentGreatAdd");
            TridentTime = data.GetInt("TridentTime");
            TridentCheckTime = data.GetInt("TridentCheckTime");
            TridentStrategyType = data.GetInt("TridentStrategyType");
            TridentStrategyPrice = data.GetInt("TridentStrategyPrice");
            TridentRandomType = data.GetInt("TridentRandomType");
            TridentRandomPrice = data.GetInt("TridentRandomPrice");

            TridentRandomRatio.Clear();
            string ratioString = data.GetString("TridentRandomPro");
            TridentRandomTotalRatio = AddRatio(TridentRandomRatio, ratioString);

            AcrossOceanItemId = data.GetInt("AcrossOceanItemId");
            AcrossOceanPuzzleCost = data.GetInt("AcrossOceanPuzzleCost");
            AcrossOceanPuzzleCount = data.GetInt("AcrossOceanPuzzleCount");

            GodPathLibrary.stageNeedAssign = stageNeedAssign;
            GodPathLibrary.drugAddAssign = drugAddAssign;
            GodPathLibrary.trainBodyWaterPower = trainBodyWaterPower;
        }

        private static int AddRatio(Dictionary<int, int> ratio, string ratioString)
        {
            int total = 0;
            ratio.Clear();
            if (!string.IsNullOrEmpty(ratioString))
            {
                string[] ratioArray = CommonUtility.StringSplit.GetArray("||", ratioString);
                foreach (var ratioItem in ratioArray)
                {
                    string[] score = CommonUtility.StringSplit.GetArray(":", ratioItem);
                    int key = int.Parse(score[0]);
                    int value = int.Parse(score[1]);
                    ratio[key] = total;
                    total += value;
                }
            }
            return total;
        }

        public static int GeTridentRandomRatio()
        {
            int quelity = 0;
            int rand = NewRAND.Next(1, TridentRandomTotalRatio);
            foreach (var item in TridentRandomRatio)
            {
                if (rand >= item.Value)
                {
                    quelity = item.Key;
                }
                else
                {
                    break;
                }
            }

            return quelity;
        }

        #region seven fight

        private static void InitSeventFightCard()
        {
            Dictionary<int, Dictionary<int, GodPathStageFightInfo>> heroSevenFightCard = new Dictionary<int, Dictionary<int, GodPathStageFightInfo>>();
            Data data;
            DataList list = DataListManager.inst.GetDataList("GodPathSevenFightCard");
            foreach (var kv in list)
            {
                data = kv.Value;

                int heroId = data.GetInt("HeroId");
                int stage = data.GetInt("Stage");
                int fight = data.GetInt("Fight");

                Dictionary<int, GodPathStageFightInfo> stages;
                if (!heroSevenFightCard.TryGetValue(heroId, out stages))
                {
                    stages = new Dictionary<int, GodPathStageFightInfo>();
                    heroSevenFightCard.Add(heroId, stages);
                }

                GodPathStageFightInfo stageInfo;
                if (!stages.TryGetValue(stage, out stageInfo))
                {
                    stages.Add(stage, stageInfo = new GodPathStageFightInfo());
                }

                stageInfo.AddFightCards(fight, StringSplit.GetInts(data.GetString("Card")), data.GetBoolean("IsReward"), data.GetString("Reward"));

                GodPathLibrary.heroSevenFightCard = heroSevenFightCard;
            }
        }

        public static GodPathStageFightInfo GetStageCardList(int heroId, int stage)
        {
            Dictionary<int, GodPathStageFightInfo> stages;
            if (heroSevenFightCard.TryGetValue(heroId, out stages))
            {
                GodPathStageFightInfo cardList;
                stages.TryGetValue(stage, out cardList);
                return cardList;
            }
            return null;
        }

        public static bool IsRestrain(GodPathCardType type1, GodPathCardType type2)
        {
            switch (type1)
            {
                case GodPathCardType.Super: return true;
                case GodPathCardType.Power: return type2 == GodPathCardType.Skill;
                case GodPathCardType.Skill: return type2 == GodPathCardType.Speed;
                case GodPathCardType.Speed: return type2 == GodPathCardType.Power;
                default: return false;
            }
        }

        #endregion

        #region Train body

        private static void InitTrainBodyHeroShield()
        {
            Dictionary<int, GodPathHeroShieldModel> trainBodyHeroShield = new Dictionary<int, GodPathHeroShieldModel>();
            Dictionary<int, Dictionary<int, GodPathStageShieldHero>> trainBodyShieldHero = new Dictionary<int, Dictionary<int, GodPathStageShieldHero>>();
            
            Data data;
            DataList list = DataListManager.inst.GetDataList("GodPathHeroShield");
            foreach (var kv in list)
            {
                data = kv.Value;

                GodPathHeroShieldModel model = new GodPathHeroShieldModel();
                model.Init(data);
                trainBodyHeroShield.Add(data.ID, model);
            }

            list = DataListManager.inst.GetDataList("GodPathTrainBody");
            foreach (var kv in list)
            {
                data = kv.Value;

                int heroId = data.GetInt("HeroId");
                int stage = data.GetInt("Stage");
                int fight = data.GetInt("Fight");

                Dictionary<int, GodPathStageShieldHero> stages;
                if (!trainBodyShieldHero.TryGetValue(heroId, out stages))
                {
                    stages = new Dictionary<int, GodPathStageShieldHero>();
                    trainBodyShieldHero.Add(heroId, stages);
                }

                GodPathStageShieldHero stageInfo;
                if (!stages.TryGetValue(stage, out stageInfo))
                {
                    stages.Add(stage, stageInfo = new GodPathStageShieldHero());
                }

                stageInfo.AddFightShieldHero(fight, StringSplit.GetInts(data.GetString("ShieldHero")));
            }
            GodPathLibrary.trainBodyHeroShield = trainBodyHeroShield;
            GodPathLibrary.trainBodyShieldHero = trainBodyShieldHero;
        }

        public static GodPathHeroShieldModel GetPathHeroShieldModel(int id)
        {
            GodPathHeroShieldModel model;
            trainBodyHeroShield.TryGetValue(id, out model);
            return model;
        }

        public static GodPathStageShieldHero GetStageShieldHero(int heroId, int stage)
        {
            Dictionary<int, GodPathStageShieldHero> stages;
            if (trainBodyShieldHero.TryGetValue(heroId, out stages))
            {
                GodPathStageShieldHero heroList;
                stages.TryGetValue(stage, out heroList);
                return heroList;
            }
            return null;
        }

        #endregion

        #region ocean heart

        private static void InitOceanHeart()
        {
            Dictionary<int, GodPathHeartMode> oceanHeartList = new Dictionary<int, GodPathHeartMode>();
            
            Data data;
            DataList list = DataListManager.inst.GetDataList("GodPathHeartRepaint");
            foreach (var kv in list)
            {
                data = kv.Value;

                GodPathHeartMode model = new GodPathHeartMode(data);
                oceanHeartList.Add(data.ID, model);
            }

            GodPathLibrary.oceanHeartList = oceanHeartList;
        }

        public static GodPathHeartMode GetPathHeartModel(int value)
        {
            GodPathHeartMode model = null;
            foreach (var item in oceanHeartList)
            {
                if (value<= item.Value.Interval)
                {
                    model = item.Value;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return model;
        }

        #endregion

        #region 穿越海洋

        private static void InitAcrossOcean()
        {
            Dictionary<int, Dictionary<int, int>> acrossOceanExternPicture = new Dictionary<int, Dictionary<int, int>>();
            
            Data data;
            DataList list = DataListManager.inst.GetDataList("GodPathAcrossOcean");
            foreach (var kv in list)
            {
                data = kv.Value;

                int heroId = data.GetInt("HeroId");
                int Index = data.GetInt("Index");
                int Extra = data.GetInt("Extra");

                if (Extra <= 0) continue;

                Dictionary<int, int> heroPuzzle;

                if (!acrossOceanExternPicture.TryGetValue(heroId, out heroPuzzle))
                {
                    heroPuzzle = new Dictionary<int, int>();
                    acrossOceanExternPicture[heroId] = heroPuzzle;
                }

                heroPuzzle[Index] = Extra;
            }

            GodPathLibrary.acrossOceanExternPicture = acrossOceanExternPicture;
        }

        public static int GetExtraLight(int heroId, int index)
        {
            Dictionary<int, int> heroPuzzle;
            if (!acrossOceanExternPicture.TryGetValue(heroId, out heroPuzzle)) return 0;

            int extra;
            heroPuzzle.TryGetValue(index, out extra);

            return extra;
        }

        #endregion
    }
}
