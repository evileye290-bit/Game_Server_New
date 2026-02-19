using CommonUtility;
using DataProperty;
using EnumerateUtility;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class CampBattleLibrary
    {
        public static Dictionary<int, CampBattleRewardModel> ScoreRewards = new Dictionary<int, CampBattleRewardModel>();
        public static Dictionary<int, CampBattleRewardModel> LeaderRewards = new Dictionary<int, CampBattleRewardModel>();
        public static Dictionary<int, CampBattleRewardModel> CollectionRewards = new Dictionary<int, CampBattleRewardModel>();
        public static Dictionary<int, CampBattleRewardModel> FightRewards = new Dictionary<int, CampBattleRewardModel>();
        private static Dictionary<int, CampBattleExpendModel> expends = new Dictionary<int, CampBattleExpendModel>();

        public static List<CampBattleBoxReward> BoxRewards = new List<CampBattleBoxReward>();

        public static List<CampBattleDifficultyModel> dungeonDifficulty = new List<CampBattleDifficultyModel>();


        public static Dictionary<int, CampBattleNatureItemModel> natureItems = new Dictionary<int, CampBattleNatureItemModel>();

        public static Dictionary<int, CampBattleAttributeIntensifyModel> intensifyAttributeDic = new Dictionary<int, CampBattleAttributeIntensifyModel>();
        public static Dictionary<int, int> holdFortMaxCount2ScoreDic = new Dictionary<int, int>();

        public static int ScoreMin = 100000000;

        public static int CollectionMax = 0;
        public static int FightMax = 0;
        public static int LeaderMax = 0;
        public static void Init()
        {
            InitScoreRewards();

            InitCollectionRewards();

            InitFightRewards();

            InitExpends();

            InitCampBattleBox();

            InitLeaderRewards();

            InitCampBattleDifficulty();

            InitCampBattleNatureItems();

            InitCampBattleAttributeIntensify();

            InitCampBattleStrongPointControlLimit();
        }


        private static void InitCampBattleStrongPointControlLimit()
        {
            Dictionary<int, int> holdFortMaxCount2ScoreDic = new Dictionary<int, int>();
            //holdFortMaxCount2ScoreDic.Clear();
            DataList dataList = DataListManager.inst.GetDataList("CampBattleStrongPointControlLimit");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                holdFortMaxCount2ScoreDic.Add(data.ID, data.GetInt("score"));
            }
            CampBattleLibrary.holdFortMaxCount2ScoreDic = holdFortMaxCount2ScoreDic;
        }

        public static int GetCampBattleStrongPointControlLimitScore(int id)
        {
            int score;
            if (holdFortMaxCount2ScoreDic.TryGetValue(id, out score))
            {
                return score;
            }
            return 0;
        }

        private static void InitCampBattleAttributeIntensify()
        {
            Dictionary<int, CampBattleAttributeIntensifyModel> intensifyAttributeDic = new Dictionary<int, CampBattleAttributeIntensifyModel>();
            //intensifyAttributeDic.Clear();
            DataList dataList = DataListManager.inst.GetDataList("CampBattleAttributeIntensify");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampBattleAttributeIntensifyModel info = new CampBattleAttributeIntensifyModel(data);
                intensifyAttributeDic.Add(data.ID, info);
            }
            CampBattleLibrary.intensifyAttributeDic = intensifyAttributeDic;
        }

        public static CampBattleAttributeIntensifyModel GetCampBattleAttributeIntenisfy(int count)
        {
            CampBattleAttributeIntensifyModel model;
            if (intensifyAttributeDic.TryGetValue(count, out model))
            {
                return model;
            }
            return null;
        }



        private static void InitCampBattleDifficulty()
        {
            List<CampBattleDifficultyModel> dungeonDifficulty = new List<CampBattleDifficultyModel>();
            //dungeonDifficulty.Clear();
            DataList dataList = DataListManager.inst.GetDataList("CampBattleDifficulty");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampBattleDifficultyModel info = new CampBattleDifficultyModel(data);
                dungeonDifficulty.Add(info);
            }
            CampBattleLibrary.dungeonDifficulty = dungeonDifficulty;
        }

        private static void InitCampBattleNatureItems()
        {
            Dictionary<int, CampBattleNatureItemModel> natureItems = new Dictionary<int, CampBattleNatureItemModel>();
            //natureItems.Clear();
            DataList dataList = DataListManager.inst.GetDataList("CampBattleNatureItem");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampBattleNatureItemModel info = new CampBattleNatureItemModel(data);
                natureItems.Add(info.ItemId, info);
            }
            CampBattleLibrary.natureItems = natureItems;
        }

        //public static CampBattleNatureItemModel GetNatureItems(int itemId)
        //{
        //    CampBattleNatureItemModel item;
        //    if (natureItems.TryGetValue(itemId, out item))
        //    {
        //        return item;
        //    }
        //    return null;
        //}


        public static float GetDifficultyRatio(int phaseNum)
        {
            foreach (var item in dungeonDifficulty)
            {
                if (phaseNum >= item.Min && phaseNum <= item.Max)
                {
                    return item.GrowthRatio;
                }
            }
            return 1;
        }


        private static void InitCampBattleBox()
        {
            List<CampBattleBoxReward> BoxRewards = new List<CampBattleBoxReward>();
            //BoxRewards.Clear();
            CampBattleBoxReward info;
            DataList dataList = DataListManager.inst.GetDataList("CampBattleBox");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new CampBattleBoxReward(data);
                BoxRewards.Add(info);
            }
            CampBattleLibrary.BoxRewards = BoxRewards;
        }

        public static string GetCampBoxReward(int phaseNum)
        {
            foreach (var item in BoxRewards)
            {
                if (phaseNum >= item.Min && phaseNum <= item.Max)
                {
                    return item.Rewards;
                }
            }
            return string.Empty;
        }

        private static void InitLeaderRewards()
        {
            Dictionary<int, CampBattleRewardModel> LeaderRewards = new Dictionary<int, CampBattleRewardModel>();
            //LeaderRewards.Clear();
            CampBattleRewardModel info;
            DataList dataList = DataListManager.inst.GetDataList("CampBattleLeaderReward");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new CampBattleRewardModel(data);

                if (!LeaderRewards.ContainsKey(info.Id))
                {
                    LeaderRewards.Add(info.Id, info);

                    if (CollectionMax < info.Max)
                    {
                        CollectionMax = info.Max;
                    }
                }
                else
                {
                    Logger.Log.Warn("InitLeaderRewards has same Id {0}", info.Id);
                }
            }
            CampBattleLibrary.LeaderRewards = LeaderRewards;
        }

        private static void InitScoreRewards()
        {
            Dictionary<int, CampBattleRewardModel> ScoreRewards = new Dictionary<int, CampBattleRewardModel>();
            //ScoreRewards.Clear();
            CampBattleRewardModel info;
            DataList dataList = DataListManager.inst.GetDataList("CampBattleScoreReward");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new CampBattleRewardModel(data);

                if (!ScoreRewards.ContainsKey(info.Id))
                {
                    ScoreRewards.Add(info.Id, info);

                    if (ScoreMin > info.Min)
                    {
                        ScoreMin = info.Min;
                    }
                }
                else
                {
                    Logger.Log.Warn("InitScoreRewards has same Id {0}", info.Id);
                }
            }
            CampBattleLibrary.ScoreRewards = ScoreRewards;
        }

        private static void InitCollectionRewards()
        {
            Dictionary<int, CampBattleRewardModel> CollectionRewards = new Dictionary<int, CampBattleRewardModel>();
            //CollectionRewards.Clear();
            CampBattleRewardModel info;
            DataList dataList = DataListManager.inst.GetDataList("CampBattleCollectionReward");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new CampBattleRewardModel(data);

                if (!CollectionRewards.ContainsKey(info.Id))
                {
                    CollectionRewards.Add(info.Id, info);

                    if (CollectionMax < info.Max)
                    {
                        CollectionMax = info.Max;
                    }
                }
                else
                {
                    Logger.Log.Warn("InitCollectionRewards has same Id {0}", info.Id);
                }
            }
            CampBattleLibrary.CollectionRewards = CollectionRewards;
        }

        private static void InitFightRewards()
        {
            Dictionary<int, CampBattleRewardModel> FightRewards = new Dictionary<int, CampBattleRewardModel>();
            //FightRewards.Clear();
            CampBattleRewardModel info;
            DataList dataList = DataListManager.inst.GetDataList("CampBattleFightReward");
            foreach (var item in dataList)
            {

                Data data = item.Value;
                info = new CampBattleRewardModel(data);

                if (!FightRewards.ContainsKey(info.Id))
                {
                    FightRewards.Add(info.Id, info);

                    if (FightMax < info.Max)
                    {
                        FightMax = info.Max;
                    }
                }
                else
                {
                    Logger.Log.Warn("InitFightRewards has same Id {0}", info.Id);
                }
            }
            CampBattleLibrary.FightRewards = FightRewards;
        }

        private static void InitExpends()
        {
            Dictionary<int, CampBattleExpendModel> expends = new Dictionary<int, CampBattleExpendModel>();
            //expends.Clear();

            DataList dataList = DataListManager.inst.GetDataList("CampBattleConfig");

            foreach (var item in dataList)
            {
                Data data = item.Value;
                CampBattleExpendModel model = new CampBattleExpendModel(data);
                if (!expends.ContainsKey(model.Id))
                {
                    expends.Add(model.Id, model);
                }
                else
                {
                    Logger.Log.Warn("InitCampBattleConfig has same Id {0}", model.Id);
                }
            }
            CampBattleLibrary.expends = expends;
        }



        //public static CampBattleRewardModel GetScoreRewardInfoById(int id)
        //{
        //    CampBattleRewardModel info;
        //    ScoreRewards.TryGetValue(id, out info);
        //    return info;
        //}

        public static CampBattleRewardModel GetScoreRewardInfo(int score)
        {
            CampBattleRewardModel info = null;
            foreach (var item in ScoreRewards)
            {
                if (item.Value.Min <= score)
                {
                    info = item.Value;
                }
                else
                {
                    break;
                }
            }
            return info;
        }

        //public static CampBattleRewardModel GetCollectionRewardInfoById(int id)
        //{
        //    CampBattleRewardModel info;
        //    CollectionRewards.TryGetValue(id, out info);
        //    return info;
        //}

        public static CampBattleRewardModel GetCollectionRewardInfo(int score)
        {
            CampBattleRewardModel info = null;
            foreach (var item in CollectionRewards)
            {
                if (item.Value.Min <= score && score <= item.Value.Max)
                {
                    return item.Value;
                }
            }
            return info;
        }

        //public static CampBattleRewardModel GetFightRewardInfoById(int id)
        //{
        //    CampBattleRewardModel info;
        //    FightRewards.TryGetValue(id, out info);
        //    return info;
        //}

        public static CampBattleRewardModel GetFightRewardInfo(int score)
        {
            CampBattleRewardModel info = null;
            foreach (var item in FightRewards)
            {
                if (item.Value.Min <= score && score <= item.Value.Max)
                {
                    return item.Value;
                }
            }
            return info;
        }

        public static CampBattleRewardModel GetLeaderRewardInfo(int score)
        {
            CampBattleRewardModel info = null;
            foreach (var item in LeaderRewards)
            {
                if (item.Value.Min <= score && score <= item.Value.Max)
                {
                    return item.Value;
                }
            }
            return info;
        }

        public static CampBattleExpendModel GetCampBattleExpend(int id = 1)
        {
            CampBattleExpendModel model;
            expends.TryGetValue(1, out model);
            return model;
        }

        //public static int GetBattleStrongPointSpendGrain(CampBattleStep battleStep)
        //{
        //    var expend = GetCampBattleExpend();
        //    if (expend == null)
        //    {
        //        return 0;
        //    }
        //    return expend.StrongPoint.Item2;
        //}

        public static int GetBattleSpendGrain(CampBattleStep battleStep, MapType mapType)
        {
            var expend = GetCampBattleExpend();
            if (expend == null)
            {
                return 0;
            }

            switch (mapType)
            {
                case MapType.CampBattle:
                case MapType.CampDefense:
                    return expend.StrongPoint.Item2;
                case MapType.CampBattleNeutral:
                    return expend.NeutralityPoint.Item2;
                default:
                    break;
            }
            return 0;
        }

        //public static int GetBattleNeutralityPointExpendGrain(CampBattleStep battleStep)
        //{
        //    var expend = GetCampBattleExpend();
        //    if (expend == null)
        //    {
        //        return 0;
        //    }
        //    return expend.NeutralityPoint.Item2;
        //}

        public static int GetCampBattleFortGuardTime()
        {
            var expend = GetCampBattleExpend();
            if (expend == null)
            {
                return 0;
            }
            return expend.CDTime;
        }

        public static int GetCampBattleFortGiveUpTime()
        {
            var expend = GetCampBattleExpend();
            if (expend == null)
            {
                return 0;
            }
            return expend.GiveUpTime;
        }


        public static int GetFortHoldEmailId()
        {
            var expend = GetCampBattleExpend();
            if (expend == null)
            {
                return 0;
            }
            return expend.FortHoldEmailId;
        }

        public static int GetBuyOneActionCount()
        {
            var expend = GetCampBattleExpend();
            if (expend == null)
            {
                return 0;
            }
            return expend.BuyOneActionCount;
        }

        public static int GetCampBattleBaseCampScore()
        {
            var expend = GetCampBattleExpend();
            if (expend == null)
            {
                return 0;
            }
            return expend.CampBattleBaseCampScore;
        }

        public static int GetAttributeOneIntensifyValue()
        {
            var expend = GetCampBattleExpend();
            if (expend == null)
            {
                return 0;
            }
            return expend.AttributeOneIntensifyValue;
        }

        public static int GetAttributeIntensifyTimeSec()
        {
            var expend = GetCampBattleExpend();
            if (expend == null)
            {
                return 0;
            }
            return expend.AttributeIntensifyTimeSec;
        }


    }
}
