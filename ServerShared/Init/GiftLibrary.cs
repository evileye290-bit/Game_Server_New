using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public static class GiftLibrary
    {
        private static Dictionary<int, GiftCodeInfo> giftCodeInfoDic = new Dictionary<int, GiftCodeInfo>();
        private static Dictionary<string, GiftCodeInfo> giftCodeInfoStrDic = new Dictionary<string, GiftCodeInfo>();
        private static Dictionary<int, CommonGiftCode> commonGiftCodeDic = new Dictionary<int, CommonGiftCode>();
        private static Dictionary<string, CommonGiftCode> commonGiftCodeStrDic = new Dictionary<string, CommonGiftCode>();
        //key:id
        private static Dictionary<int, GiftItemModel> cultivateGiftDic = new Dictionary<int, GiftItemModel>();
        //key:giftType
        private static Dictionary<int, List<GiftItemModel>> culGiftsByType = new Dictionary<int, List<GiftItemModel>>();
        //key:triggerType
        private static Dictionary<int, List<GiftItemModel>> culGiftsByTriType = new Dictionary<int, List<GiftItemModel>>();
        //key:id
        private static Dictionary<int, PettyGiftModel> pettyGiftDic = new Dictionary<int, PettyGiftModel>();
        //key:mainType
        private static Dictionary<int, Dictionary<int, PettyGiftModel>> pettyGiftDicByType = new Dictionary<int, Dictionary<int, PettyGiftModel>>();
        //key:id
        private static Dictionary<int, DailyRechargeModel> dailyRechargeDic = new Dictionary<int, DailyRechargeModel>();
        //key:id
        private static Dictionary<int, int> daysRewardsHeros = new Dictionary<int, int>();
        //key:id
        private static Dictionary<int, HeroDaysRewardsModel> heroDaysRewardsDic = new Dictionary<int, HeroDaysRewardsModel>();
        //key:id
        private static Dictionary<int, NewServerPromotionModel> newServerPromotionDic = new Dictionary<int, NewServerPromotionModel>();
        //key:id
        private static Dictionary<int, LuckyFlipCardRewardModel> luckyFlipCardRewardDic = new Dictionary<int, LuckyFlipCardRewardModel>();
        //key:id
        private static Dictionary<int, LuckyFlipCardCumulateRewardModel> luckyFlipCardCumulateRewards = new Dictionary<int, LuckyFlipCardCumulateRewardModel>();
        //key:period, id
        private static Dictionary<int, Dictionary<int, LuckyFlipCardRewardModel>> luckyFCRewardsByPeriod = new Dictionary<int, Dictionary<int, LuckyFlipCardRewardModel>>();
        //key:period, id, value:nextId
        private static Dictionary<int, Dictionary<int, int>> luckyFlipCardRechargeCycle = new Dictionary<int, Dictionary<int, int>>();

        private static Dictionary<int, Dictionary<int, SpecialGiftConfig>> giftConfigDic = new Dictionary<int, Dictionary<int, SpecialGiftConfig>>();

        //key:id
        private static Dictionary<int, TreasureFlipCardRewardModel> treasureFlipCardRewardDic = new Dictionary<int, TreasureFlipCardRewardModel>();
        //key:id
        private static Dictionary<int, TreasureFlipCardCumulateRewardModel> treasureFlipCardCumulateRewards = new Dictionary<int, TreasureFlipCardCumulateRewardModel>();
        //key:period, id
        private static Dictionary<int, Dictionary<int, TreasureFlipCardRewardModel>> treasureFCRewardsByPeriod = new Dictionary<int, Dictionary<int, TreasureFlipCardRewardModel>>();
        //key:period, id, value:nextId
        private static Dictionary<int, Dictionary<int, int>> treasureFlipCardRechargeCycle = new Dictionary<int, Dictionary<int, int>>();

        //key: round, subtype
        private static Dictionary<int, Dictionary<int, TreasureFlipCardFlipCardRatioModel>> treasureFlipCardRatioDic = new Dictionary<int, Dictionary<int, TreasureFlipCardFlipCardRatioModel>>();

        public static int FirstPettyGift;
        private static Dictionary<int, int> firstLuckFlipRechargeIdDic = new Dictionary<int, int>();
        private static Dictionary<int, int> firstTreasureFlipRechargeIdDic = new Dictionary<int, int>();
        private static Dictionary<int, DiamondRatioCardInfo> diamondRatioChangeCards = new Dictionary<int, DiamondRatioCardInfo>();

        public static void Init()
        {
            BindGiftCodeInfo();
            BindCommonGiftCode();
            BindCultivateGift();
            BindPettyGift();
            BindDailyRecharge();
            BindDaysRewardsHeros();
            BindHeroDaysRewards();
            BindNewServerPromotion();
            BindLuckyFlipCardReward();
            BindLuckyFlipCardCumulateReward();
            BindLuckyFlipCardRechargeCycle();
            BindSpecialGiftConfig();
            BindTreasureFlipCardReward();
            BindTreasureFlipCardCumulateReward();
            BindTreasureFlipCardRechargeCycle();
            BindTreasureFlipCardFlipCardRatio();
            BindDiamondRatioChangeCard();
        }

        private static void BindGiftCodeInfo()
        {
            Dictionary<int, GiftCodeInfo> giftCodeInfoDic = new Dictionary<int, GiftCodeInfo>();
            Dictionary<string, GiftCodeInfo> giftCodeInfoStrDic = new Dictionary<string, GiftCodeInfo>();
            //giftCodeInfoDic.Clear();
            //giftCodeInfoStrDic.Clear();
           
            DataList dataList = DataListManager.inst.GetDataList("GiftCodeInfo");
            foreach (var item in dataList)
            {
                GiftCodeInfo gift = new GiftCodeInfo(item.Value);
                giftCodeInfoDic.Add(gift.Id, gift);
                giftCodeInfoStrDic.Add(gift.CodeTop4, gift);
            }
            GiftLibrary.giftCodeInfoDic = giftCodeInfoDic;
            GiftLibrary.giftCodeInfoStrDic = giftCodeInfoStrDic;
        }

        private static void BindCommonGiftCode()
        {
            Dictionary<int, CommonGiftCode> commonGiftCodeDic = new Dictionary<int, CommonGiftCode>();
            Dictionary<string, CommonGiftCode> commonGiftCodeStrDic = new Dictionary<string, CommonGiftCode>();
            //commonGiftCodeDic.Clear();
            //commonGiftCodeStrDic.Clear();

            DataList dataList = DataListManager.inst.GetDataList("CommonGiftCode");
            foreach (var item in dataList)
            {
                CommonGiftCode model = new CommonGiftCode(item.Value);
                commonGiftCodeDic.Add(model.Id, model);
                string code = model.Code.ToUpper();
                commonGiftCodeStrDic.Add(code, model);
            }
            GiftLibrary.commonGiftCodeDic = commonGiftCodeDic;
            GiftLibrary.commonGiftCodeStrDic = commonGiftCodeStrDic;
        }

        private static void BindCultivateGift()
        {
            Dictionary<int, GiftItemModel> cultivateGiftDic = new Dictionary<int, GiftItemModel>();
            Dictionary<int, List<GiftItemModel>> culGiftsByType = new Dictionary<int, List<GiftItemModel>>();
            Dictionary<int, List<GiftItemModel>> culGiftsByTriType = new Dictionary<int, List<GiftItemModel>>();
            //cultivateGiftDic.Clear();
            //culGiftsByType.Clear();
            //culGiftsByTriType.Clear();

            DataList dataList = DataListManager.inst.GetDataList("CultivateGift");
            foreach (var item in dataList)
            {
                GiftItemModel model = new GiftItemModel(item.Value);
                cultivateGiftDic.Add(model.Id, model);

                List<GiftItemModel> list;
                if (culGiftsByType.TryGetValue(model.Type, out list))
                {
                    list.Add(model);
                }
                else
                {
                    list = new List<GiftItemModel>() { model };              
                    culGiftsByType.Add(model.Type, list);
                }

                List<GiftItemModel> triggerTypeGifts;               
                if (culGiftsByTriType.TryGetValue(model.TriggerType, out triggerTypeGifts))
                {
                    triggerTypeGifts.Add(model);
                }
                else
                {
                    triggerTypeGifts = new List<GiftItemModel>() { model};
                    culGiftsByTriType.Add(model.TriggerType, triggerTypeGifts);
                }
            }
            GiftLibrary.cultivateGiftDic = cultivateGiftDic;
            GiftLibrary.culGiftsByType = culGiftsByType;
            GiftLibrary.culGiftsByTriType = culGiftsByTriType;
        }

        private static void BindPettyGift()
        {
            Dictionary<int, PettyGiftModel> pettyGiftDic = new Dictionary<int, PettyGiftModel>();
            Dictionary<int, Dictionary<int, PettyGiftModel>> pettyGiftDicByType = new Dictionary<int, Dictionary<int, PettyGiftModel>>();
            //pettyGiftDic.Clear();
            //pettyGiftDicByType.Clear();

            DataList dataList = DataListManager.inst.GetDataList("PettyGift");
            foreach (var item in dataList)
            {
                PettyGiftModel model = new PettyGiftModel(item.Value);
                pettyGiftDic.Add(model.Id, model);
                if (model.IsFirst == 1)
                {
                    FirstPettyGift = model.Id;
                }
                Dictionary<int, PettyGiftModel> dic;
                if (pettyGiftDicByType.TryGetValue(model.MainType, out dic))
                {
                    if (!dic.ContainsKey(model.Type))
                    {
                        dic.Add(model.Type, model);
                    }
                }
                else
                {
                    dic = new Dictionary<int, PettyGiftModel>();
                    dic.Add(model.Type, model);
                    pettyGiftDicByType.Add(model.MainType, dic);
                }
            }
            GiftLibrary.pettyGiftDic = pettyGiftDic;
            GiftLibrary.pettyGiftDicByType = pettyGiftDicByType;
        }

        private static void BindDailyRecharge()
        {
            Dictionary<int, DailyRechargeModel> dailyRechargeDic = new Dictionary<int, DailyRechargeModel>();
            //dailyRechargeDic.Clear();

            DataList dataList = DataListManager.inst.GetDataList("DailyRecharge");
            foreach (var item in dataList)
            {
                DailyRechargeModel model = new DailyRechargeModel(item.Value);
                dailyRechargeDic.Add(model.Id, model);
            }
            GiftLibrary.dailyRechargeDic = dailyRechargeDic;
        }

        private static void BindDaysRewardsHeros()
        {
            Dictionary<int, int> daysRewardsHeros = new Dictionary<int, int>();
            //daysRewardsHeros.Clear();

            DataList dataList = DataListManager.inst.GetDataList("DaysRewardsPeriodHero");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int heroId = data.GetInt("Hero");
                daysRewardsHeros.Add(data.ID, heroId);
            }
            GiftLibrary.daysRewardsHeros = daysRewardsHeros;
        }

        private static void BindHeroDaysRewards()
        {
            Dictionary<int, HeroDaysRewardsModel> heroDaysRewardsDic = new Dictionary<int, HeroDaysRewardsModel>();
            //heroDaysRewardsDic.Clear();

            DataList dataList = DataListManager.inst.GetDataList("HeroDaysRewards");
            foreach (var item in dataList)
            {
                HeroDaysRewardsModel model = new HeroDaysRewardsModel(item.Value);
                heroDaysRewardsDic.Add(model.Id, model);
                //Dictionary<int, HeroDaysRewardsModel> dic;
                //if (heroDaysRewardsDic.TryGetValue(model.Period, out dic))
                //{
                //    if (!dic.ContainsKey(model.Id))
                //    {
                //        dic.Add(model.Id, model);
                //    }
                //}
                //else
                //{
                //    dic = new Dictionary<int, HeroDaysRewardsModel>();
                //    dic.Add(model.Id, model);
                //    heroDaysRewardsDic.Add(model.Period, dic);
                //}              
            }
            GiftLibrary.heroDaysRewardsDic = heroDaysRewardsDic;
        }

        private static void BindNewServerPromotion()
        {
            Dictionary<int, NewServerPromotionModel> newServerPromotionDic = new Dictionary<int, NewServerPromotionModel>();         

            DataList dataList = DataListManager.inst.GetDataList("NewServerPromotion");
            foreach (var item in dataList)
            {
                NewServerPromotionModel model = new NewServerPromotionModel(item.Value);
                newServerPromotionDic.Add(model.Id, model);
            }
            GiftLibrary.newServerPromotionDic = newServerPromotionDic;
        }

        private static void BindLuckyFlipCardReward()
        {
            Dictionary<int, LuckyFlipCardRewardModel> luckyFlipCardRewardDic = new Dictionary<int, LuckyFlipCardRewardModel>();
            Dictionary<int, Dictionary<int, LuckyFlipCardRewardModel>> luckyFCRewardsByPeriod = new Dictionary<int, Dictionary<int, LuckyFlipCardRewardModel>>();

            DataList dataList = DataListManager.inst.GetDataList("LuckyFlipCardReward");

            Dictionary<int, LuckyFlipCardRewardModel> dic;
            foreach (var item in dataList)
            {
                LuckyFlipCardRewardModel model = new LuckyFlipCardRewardModel(item.Value);
                luckyFlipCardRewardDic.Add(model.Id, model);

                if (luckyFCRewardsByPeriod.TryGetValue(model.Period, out dic))
                {
                    dic.Add(model.Id, model);
                }
                else
                {
                    dic = new Dictionary<int, LuckyFlipCardRewardModel>();
                    dic.Add(model.Id, model);
                    luckyFCRewardsByPeriod.Add(model.Period, dic);
                }
            }
            GiftLibrary.luckyFlipCardRewardDic = luckyFlipCardRewardDic;
            GiftLibrary.luckyFCRewardsByPeriod = luckyFCRewardsByPeriod;
        }

        private static void BindLuckyFlipCardCumulateReward()
        {
            Dictionary<int, LuckyFlipCardCumulateRewardModel> luckyFlipCardCumulateRewards = new Dictionary<int, LuckyFlipCardCumulateRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("LuckyFlipCardCumulateReward");

            foreach (var item in dataList)
            {
                LuckyFlipCardCumulateRewardModel model = new LuckyFlipCardCumulateRewardModel(item.Value);
                luckyFlipCardCumulateRewards.Add(model.Id, model);                
            }
            GiftLibrary.luckyFlipCardCumulateRewards = luckyFlipCardCumulateRewards;
        }

        private static void BindLuckyFlipCardRechargeCycle()
        {
            Dictionary<int, Dictionary<int, int>> luckyFlipCardRechargeCycle = new Dictionary<int, Dictionary<int, int>>();

            DataList dataList = DataListManager.inst.GetDataList("LuckyFlipCardRechargeCycle");

            Dictionary<int, int> dic;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int period = data.GetInt("Period");
                if (data.GetInt("First") == 1 && !firstLuckFlipRechargeIdDic.ContainsKey(period))
                {
                    firstLuckFlipRechargeIdDic.Add(period, data.ID);
                }
                if (!luckyFlipCardRechargeCycle.TryGetValue(period, out dic))
                {
                    dic = new Dictionary<int, int>();
                    luckyFlipCardRechargeCycle.Add(period, dic);
                }
                dic.Add(data.ID, data.GetInt("NextId"));
            }
            GiftLibrary.luckyFlipCardRechargeCycle = luckyFlipCardRechargeCycle;
        }

        private static void BindSpecialGiftConfig()
        {
            Dictionary<int, Dictionary<int, SpecialGiftConfig>> giftConfigDic = new Dictionary<int, Dictionary<int, SpecialGiftConfig>>();

            DataList dataList = DataListManager.inst.GetDataList("SpecialGiftConfig");
            Dictionary<int, SpecialGiftConfig> dic;

            foreach (var item in dataList)
            {
                SpecialGiftConfig config = new SpecialGiftConfig(item.Value);
                if (!giftConfigDic.TryGetValue(config.GiftType, out dic))
                {
                    dic = new Dictionary<int, SpecialGiftConfig>();
                    giftConfigDic.Add(config.GiftType, dic);
                }
                dic.Add(config.SubType, config);
            }
            GiftLibrary.giftConfigDic = giftConfigDic;
        }

        private static void BindTreasureFlipCardReward()
        {
            Dictionary<int, TreasureFlipCardRewardModel> treasureFlipCardRewardDic = new Dictionary<int, TreasureFlipCardRewardModel>();
            Dictionary<int, Dictionary<int, TreasureFlipCardRewardModel>> treasureFCRewardsByPeriod = new Dictionary<int, Dictionary<int, TreasureFlipCardRewardModel>>();

            DataList dataList = DataListManager.inst.GetDataList("TreasureFlipCardReward");

            Dictionary<int, TreasureFlipCardRewardModel> dic;
            foreach (var item in dataList)
            {
                TreasureFlipCardRewardModel model = new TreasureFlipCardRewardModel(item.Value);
                treasureFlipCardRewardDic.Add(model.Id, model);

                if (treasureFCRewardsByPeriod.TryGetValue(model.Period, out dic))
                {
                    dic.Add(model.Id, model);
                }
                else
                {
                    dic = new Dictionary<int, TreasureFlipCardRewardModel>();
                    dic.Add(model.Id, model);
                    treasureFCRewardsByPeriod.Add(model.Period, dic);
                }
            }
            GiftLibrary.treasureFlipCardRewardDic = treasureFlipCardRewardDic;
            GiftLibrary.treasureFCRewardsByPeriod = treasureFCRewardsByPeriod;
        }

        private static void BindTreasureFlipCardCumulateReward()
        {
            Dictionary<int, TreasureFlipCardCumulateRewardModel> treasureFlipCardCumulateRewards = new Dictionary<int, TreasureFlipCardCumulateRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("TreasureFlipCardCumulateReward");

            foreach (var item in dataList)
            {
                TreasureFlipCardCumulateRewardModel model = new TreasureFlipCardCumulateRewardModel(item.Value);
                treasureFlipCardCumulateRewards.Add(model.Id, model);
            }
            GiftLibrary.treasureFlipCardCumulateRewards = treasureFlipCardCumulateRewards;
        }

        private static void BindTreasureFlipCardRechargeCycle()
        {
            Dictionary<int, Dictionary<int, int>> treasureFlipCardRechargeCycle = new Dictionary<int, Dictionary<int, int>>();
            firstTreasureFlipRechargeIdDic = new Dictionary<int, int>();
            DataList dataList = DataListManager.inst.GetDataList("TreasureFlipCardRechargeCycle");
            firstTreasureFlipRechargeIdDic = new Dictionary<int, int>();
            Dictionary<int, int> dic;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int period = data.GetInt("Period");
                if (data.GetInt("First") == 1 && !firstTreasureFlipRechargeIdDic.ContainsKey(period))
                {
                    firstTreasureFlipRechargeIdDic.Add(period, data.ID);
                }
                if (!treasureFlipCardRechargeCycle.TryGetValue(period, out dic))
                {
                    dic = new Dictionary<int, int>();
                    treasureFlipCardRechargeCycle.Add(period, dic);
                }
                dic.Add(data.ID, data.GetInt("NextId"));
            }
            GiftLibrary.treasureFlipCardRechargeCycle = treasureFlipCardRechargeCycle;
        }
        private static void BindTreasureFlipCardFlipCardRatio()
        {
            Dictionary<int, Dictionary<int, TreasureFlipCardFlipCardRatioModel>> treasureFlipCardFlipCardRatioDic = new Dictionary<int, Dictionary<int, TreasureFlipCardFlipCardRatioModel>>();

            DataList dataList = DataListManager.inst.GetDataList("TreasureFlipCardFlipCardRatio");

            Dictionary<int, TreasureFlipCardFlipCardRatioModel> dic;
            foreach (var item in dataList)
            {
                TreasureFlipCardFlipCardRatioModel model = new TreasureFlipCardFlipCardRatioModel(item.Value);
                if (treasureFlipCardFlipCardRatioDic.TryGetValue(model.Round, out dic))
                {
                    dic.Add(model.Subtype, model);
                }
                else
                {
                    dic = new Dictionary<int, TreasureFlipCardFlipCardRatioModel>();
                    dic.Add(model.Subtype, model);
                    treasureFlipCardFlipCardRatioDic.Add(model.Round, dic);
                }
            }
            GiftLibrary.treasureFlipCardRatioDic = treasureFlipCardFlipCardRatioDic;
        }

        private static void BindDiamondRatioChangeCard()
        {
            Dictionary<int, DiamondRatioCardInfo> diamondRatioChangeCards = new Dictionary<int, DiamondRatioCardInfo>();

            DataList dataList = DataListManager.inst.GetDataList("DiamondRatioChangeCard");
            foreach (var item in dataList)
            {
                DiamondRatioCardInfo info = new DiamondRatioCardInfo(item.Value);
                diamondRatioChangeCards.Add(info.Id, info);
            }
            GiftLibrary.diamondRatioChangeCards = diamondRatioChangeCards;
        }
        
        public static GiftCodeInfo GetGiftCodeInfoByCodeId(int codeId)
        {
            GiftCodeInfo gift;
            giftCodeInfoDic.TryGetValue(codeId, out gift);
            return gift;
        }

        public static GiftCodeInfo GetGiftCodeInfoByCodeStr(string codeTop4)
        {
            GiftCodeInfo gift;
            giftCodeInfoStrDic.TryGetValue(codeTop4, out gift);
            return gift;
        }

        public static CommonGiftCode GetCommonGiftCodeById(int codeId)
        {
            CommonGiftCode model;
            commonGiftCodeDic.TryGetValue(codeId, out model);
            return model;
        }

        public static CommonGiftCode GetCommonGiftCodeByCode(string code)
        {
            CommonGiftCode model;
            commonGiftCodeStrDic.TryGetValue(code, out model);
            return model;
        }

        public static GiftItemModel GetGiftItemModel(int id)
        {
            GiftItemModel giftItem;
            cultivateGiftDic.TryGetValue(id, out giftItem);
            return giftItem;
        }
                  
        public static List<GiftItemModel> GetGiftItemsByTriggerType(int triggerType)
        {
            List<GiftItemModel> giftItems;
            culGiftsByTriType.TryGetValue(triggerType, out giftItems);
            return giftItems;
        }

        public static PettyGiftModel GetPettyGiftModel(int id)
        {
            PettyGiftModel model;
            pettyGiftDic.TryGetValue(id, out model);
            return model;
        }

        public static PettyGiftModel GetPettyGiftModelByType(int mainType, int type)
        {
            PettyGiftModel model;
            Dictionary<int, PettyGiftModel> dic;
            pettyGiftDicByType.TryGetValue(mainType, out dic);
            if (dic == null)
            {
                return null;
            }
            dic.TryGetValue(type, out model);
            return model;
        }

        public static List<GiftItemModel> GetGiftItemModelsByType(int type)
        {
            List<GiftItemModel> list;
            culGiftsByType.TryGetValue(type, out list);
            return list;
        }

        public static DailyRechargeModel GetDailyRechargeModel(int id)
        {
            DailyRechargeModel model;
            dailyRechargeDic.TryGetValue(id, out model);
            return model;
        }

        public static List<int> GetDailyRechargeIds(int period, int rechargeCount)
        {
            return dailyRechargeDic.Values.Where(x => x.Period == period && x.Days <= rechargeCount).Select(x => x.Id).ToList();
        }

        public static int GetHeroIdByPeriod(int period)
        {
            int heroId;
            daysRewardsHeros.TryGetValue(period, out heroId);
            return heroId;
        }

        public static HeroDaysRewardsModel GetHeroDaysReward(int id)
        {
            HeroDaysRewardsModel model;         
            heroDaysRewardsDic.TryGetValue(id, out model);      
            return model;
        }

        public static NewServerPromotionModel GetNewServerPromotionModel(int id)
        {
            NewServerPromotionModel model;
            newServerPromotionDic.TryGetValue(id, out model);
            return model;
        }

        public static LuckyFlipCardRewardModel GetLuckyFlipCardRewardModel(int id)
        {
            LuckyFlipCardRewardModel model;
            luckyFlipCardRewardDic.TryGetValue(id, out model);
            return model;
        }

        public static LuckyFlipCardCumulateRewardModel GetLuckyFlipCardCumulateRewardModel(int id)
        {
            LuckyFlipCardCumulateRewardModel model;
            luckyFlipCardCumulateRewards.TryGetValue(id, out model);
            return model;
        }

        public static void RemoveRewardIdsOnRefund(List<int> rewardedList, int period, int num, int subNum)
        {
            if (rewardedList.Count == 0) return;

            int curNum = num - subNum;
            foreach (var kv in luckyFlipCardCumulateRewards.Values.Where(x=>x.Period== period).OrderByDescending(x => x.CumulateCount))
            {
                if (kv.CumulateCount > curNum && kv.CumulateCount <= num)
                {
                    rewardedList.Remove(kv.Id);
                }
            }
        }

        public static List<int> GetLuckyFlipCardCurPeriodIds(int period)
        {
            Dictionary<int,LuckyFlipCardRewardModel> model;
            if (luckyFCRewardsByPeriod.TryGetValue(period, out model))
            {
                return model.Keys.ToList();
            }

            return new List<int>();
        }

        public static int GetLuckyFlipCardNextRechargeId(int period, int rechargeId)
        {
            int nextId = 0;
            Dictionary<int, int> dic;
            if (luckyFlipCardRechargeCycle.TryGetValue(period, out dic))
            {
                dic.TryGetValue(rechargeId, out nextId);
            }
            return nextId;
        }

        public static int GetLuckFlipCardRewardMaxSubType(int period)
        {
            int maxSubType = 0;
            Dictionary<int, LuckyFlipCardRewardModel> dic;
            luckyFCRewardsByPeriod.TryGetValue(period, out dic);
            if (dic != null)
            {
                foreach (var item in dic)
                {
                    if (item.Value.SubType > maxSubType)
                    {
                        maxSubType = item.Value.SubType;
                    }
                }
            }
            return maxSubType;
        }

        public static int GetFirstLuckFlipRechargeId(int period)
        {
            int rechargeId;
            firstLuckFlipRechargeIdDic.TryGetValue(period, out rechargeId);
            return rechargeId;
        }

        public static SpecialGiftConfig GetSpecialGiftConfigByType(int giftType, int subType)
        {
            Dictionary<int, SpecialGiftConfig> dic;
            giftConfigDic.TryGetValue(giftType, out dic);
            if (dic == null)
            {
                return null;
            }
            SpecialGiftConfig config;
            dic.TryGetValue(subType, out config);
            return config;
        }

        public static TreasureFlipCardRewardModel GetTreasureFlipCardRewardModel(int id)
        {
            TreasureFlipCardRewardModel model;
            treasureFlipCardRewardDic.TryGetValue(id, out model);
            return model;
        }

        public static TreasureFlipCardCumulateRewardModel GetTreasureFlipCardCumulateRewardModel(int id)
        {
            TreasureFlipCardCumulateRewardModel model;
            treasureFlipCardCumulateRewards.TryGetValue(id, out model);
            return model;
        }

        public static TreasureFlipCardFlipCardRatioModel GetTreasureFlipCardRatioModel(int subtype, int round)
        {
            Dictionary<int, TreasureFlipCardFlipCardRatioModel> dic;
            treasureFlipCardRatioDic.TryGetValue(round, out dic);
            if (dic == null)
            {
                return null;
            }
            TreasureFlipCardFlipCardRatioModel model;
            dic.TryGetValue(subtype, out model);
            return model;
        }
        
        public static void RemoveTreasureRewardIdsOnRefund(List<int> rewardedList, int period, int num, int subNum)
        {
            if (rewardedList.Count == 0) return;

            int curNum = num - subNum;
            foreach (var kv in luckyFlipCardCumulateRewards.Values.Where(x => x.Period == period).OrderByDescending(x => x.CumulateCount))
            {
                if (kv.CumulateCount > curNum && kv.CumulateCount <= num)
                {
                    rewardedList.Remove(kv.Id);
                }
            }
        }

        public static List<int> GetTreasureFlipCardCurPeriodIds(int period)
        {
            Dictionary<int, TreasureFlipCardRewardModel> model;
            if (treasureFCRewardsByPeriod.TryGetValue(period, out model))
            {
                return model.Keys.ToList();
            }

            return new List<int>();
        }

        public static int GetTreasureFlipCardNextRechargeId(int period, int rechargeId)
        {
            int nextId = 0;
            Dictionary<int, int> dic;
            if (treasureFlipCardRechargeCycle.TryGetValue(period, out dic))
            {
                dic.TryGetValue(rechargeId, out nextId);
            }
            return nextId;
        }

        public static int GetTreasureFlipCardRewardMaxSubType(int period)
        {
            int maxSubType = 0;
            Dictionary<int, TreasureFlipCardRewardModel> dic;
            treasureFCRewardsByPeriod.TryGetValue(period, out dic);
            if (dic != null)
            {
                foreach (var item in dic)
                {
                    if (item.Value.SubType > maxSubType)
                    {
                        maxSubType = item.Value.SubType;
                    }
                }
            }
            return maxSubType;
        }

        public static int GetFirstTreasureFlipRechargeId(int period)
        {
            int rechargeId;
            firstTreasureFlipRechargeIdDic.TryGetValue(period, out rechargeId);
            return rechargeId;
        }

        public static DiamondRatioCardInfo GetDiamondRatioChangeCardInfo(int id)
        {
            DiamondRatioCardInfo info;
            diamondRatioChangeCards.TryGetValue(id, out info);
            return info;
        }
    }
}
