using DataProperty;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;
using ServerModels;
using EnumerateUtility;

namespace ServerShared
{
    public class RechargeLibrary
    {
        /// <summary>
        /// todo 
        ///     db组成transaction 回调做额外的东西
        ///     额外的东西可以额外发，先把原价的东西给了
        ///     支付补单逻辑
        ///     额外的内容补发？
        /// </summary>

        #region new Lib

        //key:itemId
        private static Dictionary<int, RechargeItemModel> rechargeItemDic = new Dictionary<int, RechargeItemModel>();
        private static Dictionary<int, RechargeItemModel> rechargeItemDicSdk = new Dictionary<int, RechargeItemModel>();
        private static Dictionary<string, int> productIdDic = new Dictionary<string, int>();
        //key:giftType, itemId
        private static Dictionary<RechargeGiftType, Dictionary<int, RechargeItemModel>> rechargeGiftItems = new Dictionary<RechargeGiftType, Dictionary<int, RechargeItemModel>>();

        private static Dictionary<int, RechargePriceModel> rechargePriceDic = new Dictionary<int, RechargePriceModel>();

        private static Dictionary<int, float> rechargeTokenDic = new Dictionary<int, float>();

        private static ListMap<CommonGiftType, RechargeItemModel> dailyWeeklyMonthlyItemsList = new ListMap<CommonGiftType, RechargeItemModel>();
        public static ListMap<CommonGiftType, RechargeItemModel> DailyWeeklyMonthlyItemsList => dailyWeeklyMonthlyItemsList;

        private static Dictionary<RechargeGiftType, Dictionary<int, RechargeGiftModel>> rechargeGifts = new Dictionary<RechargeGiftType, Dictionary<int, RechargeGiftModel>>();
        private static Dictionary<RechargeGiftType, List<int>> rechargeRewardCheckDic = new Dictionary<RechargeGiftType, List<int>>();

        private static Dictionary<int, int> monthCardDic = new Dictionary<int, int>();
        private static Dictionary<int, AccumulateRechargeModel> accumulateRechargeDic = new Dictionary<int, AccumulateRechargeModel>();
        private static Dictionary<int, ScoreRewardModel> newRechargGifteDic = new Dictionary<int, ScoreRewardModel>();
        private static Dictionary<int, int> newRechargGifteScoreDic = new Dictionary<int, int>();

        private static List<CampBuildRankRewardData> rankRewards = new List<CampBuildRankRewardData>();
        private static Dictionary<RechargeGiftType, Dictionary<int, RechargeGiftModel>> rechargeGiftTimeDic = new Dictionary<RechargeGiftType, Dictionary<int, RechargeGiftModel>>();

        private static Dictionary<int, RechargeGiftModel> directPurchaseDic = new Dictionary<int, RechargeGiftModel>();

        public static int GiftLimitTime;
        public static float OnhookSoulPowerUp;
        public static float OnhookGoldUp;
        public static int OrderPageCount;
        public static int MonthCardDays;
        public static int FirstRechargeAccumulate;
        public static int PettyGiftTypeNum;
        public static int AccumulateRechargeOpenDay;
        public static bool IgnoreNewServerActivity;
        public static int TreasureFlipCardMaxRound;

        public static void Init()
        {
            /// 需要检查所有的RechargeId是否都被使用，双向绑定是否完成，否则报错

            // rechargeItemDic.Clear();
            // rechargeGiftItems.Clear();
            // rechargePriceDic.Clear();
            // rechargeTokenDic.Clear();
            // dailyWeeklyMonthlyItemsList.Clear();
            // rechargeGifts.Clear();
            // rechargeRewardCheckDic.Clear();
            // GiftLimitTime = 0;
            // monthCardDic.Clear();
            // accumulateRechargeDic.Clear();
            //invertedIndex.Clear();

            //DataList rechargeList = DataListManager.inst.GetDataList("Recharge");
            //InitRechargeInfo(rechargeList);

            ///////////添加具体的支付相关内容        

            DataList rechargeItemList = DataListManager.inst.GetDataList("RechargeGiftItem");
            InitRechargeItem(rechargeItemList);

            DataList rechargePriceList = DataListManager.inst.GetDataList("RechargeGiftPrice");
            InitRechargePrice(rechargePriceList);

            Data rechargeConfig = DataListManager.inst.GetData("RechargeConfig", 1);
            InitRechargeConfig(rechargeConfig, DateTime.MinValue);

            DataList rechargeTokenList = DataListManager.inst.GetDataList("RechargeToken");
            InitRechargeToekn(rechargeTokenList);

            DataList rechargeGiftList = DataListManager.inst.GetDataList("RechargeGiftTab");
            InitRechargeGift(rechargeGiftList, DateTime.MinValue);

            DataList rechargeRewardCheckList = DataListManager.inst.GetDataList("GetRechargeRewardCheck");
            InitGetRechargeRewardCheck(rechargeRewardCheckList);

            DataList monthCardList = DataListManager.inst.GetDataList("MonthCardActivate");
            InitMonthCard(monthCardList);

            DataList accumulateRechargeList = DataListManager.inst.GetDataList("AccumulatedRecharge");
            InitAccumulateRecharge(accumulateRechargeList);

            InitRankRewards();

            DataList rechargeGiftTime = DataListManager.inst.GetDataList("RechargeGiftTime");
            InitRechargeGiftTime(rechargeGiftTime, DateTime.MinValue);

            InitScoreReward();
            InitNewRechargeGiftScore();

            InitSdkRechargeGiftItem();

            DataList directPurchaseTime = DataListManager.inst.GetDataList("DirectPurchaseTime");
            InitDirectPurchaseTime(directPurchaseTime, DateTime.MinValue);
        }

        public static void Init(DateTime openServerTime)
        {
            DataList rechargeItemList = DataListManager.inst.GetDataList("RechargeGiftItem");
            InitRechargeItem(rechargeItemList);

            DataList rechargePriceList = DataListManager.inst.GetDataList("RechargeGiftPrice");
            InitRechargePrice(rechargePriceList);

            Data rechargeConfig = DataListManager.inst.GetData("RechargeConfig", 1);
            InitRechargeConfig(rechargeConfig, openServerTime);

            DataList rechargeTokenList = DataListManager.inst.GetDataList("RechargeToken");
            InitRechargeToekn(rechargeTokenList);

            DataList rechargeGiftList = DataListManager.inst.GetDataList("RechargeGiftTab");
            InitRechargeGift(rechargeGiftList, openServerTime.Date);

            DataList rechargeRewardCheckList = DataListManager.inst.GetDataList("GetRechargeRewardCheck");
            InitGetRechargeRewardCheck(rechargeRewardCheckList);

            DataList monthCardList = DataListManager.inst.GetDataList("MonthCardActivate");
            InitMonthCard(monthCardList);

            DataList accumulateRechargeList = DataListManager.inst.GetDataList("AccumulatedRecharge");
            InitAccumulateRecharge(accumulateRechargeList);

            InitRankRewards();

            DataList rechargeGiftTime = DataListManager.inst.GetDataList("RechargeGiftTime");
            InitRechargeGiftTime(rechargeGiftTime, openServerTime.Date);

            InitScoreReward();
            InitNewRechargeGiftScore();

            InitSdkRechargeGiftItem();

            DataList directPurchaseTime = DataListManager.inst.GetDataList("DirectPurchaseTime");
            InitDirectPurchaseTime(directPurchaseTime, openServerTime.Date);
        }

        private static void InitRechargeItem(DataList list)
        {
            Dictionary<int, RechargeItemModel> rechargeItemDic = new Dictionary<int, RechargeItemModel>();
            Dictionary<RechargeGiftType, Dictionary<int, RechargeItemModel>> rechargeGiftItems = new Dictionary<RechargeGiftType, Dictionary<int, RechargeItemModel>>();
            ListMap<CommonGiftType, RechargeItemModel> dailyWeeklyMonthlyItemsList = new ListMap<CommonGiftType, RechargeItemModel>();
            Dictionary<string, int> productIdDic = new Dictionary<string, int>();

            List<int> dailyWeeklyMonthlyType = new List<int> {(int)CommonGiftType.Daily, (int)CommonGiftType.Weekly, (int)CommonGiftType.Monthly };
            foreach (var item in list)
            {
                Data data = item.Value;
                RechargeItemModel model = new RechargeItemModel(data);
                rechargeItemDic.Add(model.Id, model);
                Dictionary<int, RechargeItemModel> itemList;
                if (!rechargeGiftItems.TryGetValue(model.GiftType, out itemList))
                {
                    itemList = new Dictionary<int, RechargeItemModel>();
                    itemList.Add(model.Id, model);
                    rechargeGiftItems.Add(model.GiftType, itemList);
                }
                else
                {
                    if (!itemList.ContainsKey(model.Id))
                    {
                        itemList.Add(model.Id, model);
                    }
                }

                if (model.GiftType == RechargeGiftType.Common && dailyWeeklyMonthlyType.Contains(model.SubType))
                {
                    dailyWeeklyMonthlyItemsList.Add((CommonGiftType)model.SubType, model);
                }

                if (!string.IsNullOrEmpty(model.ProductId))
                {
                    productIdDic[model.ProductId] = model.Id;
                }
            }
            RechargeLibrary.productIdDic = productIdDic;
            RechargeLibrary.rechargeItemDic = rechargeItemDic;
            RechargeLibrary.rechargeGiftItems = rechargeGiftItems;
            RechargeLibrary.dailyWeeklyMonthlyItemsList = dailyWeeklyMonthlyItemsList;
        }

        private static void InitSdkRechargeGiftItem()
        {
            Dictionary<int, RechargeItemModel> rechargeItemDicSdk = new Dictionary<int, RechargeItemModel>();
            DataList rechargeItemList = DataListManager.inst.GetDataList("SdkRechargeGiftItem");

            foreach (var item in rechargeItemList)
            {
                Data data = item.Value;
                RechargeItemModel model = new RechargeItemModel(data);
                model.IsSdkGift = true;
                rechargeItemDicSdk.Add(model.Id, model);
            }

            RechargeLibrary.rechargeItemDicSdk = rechargeItemDicSdk;
        }

        public static void InitRechargePrice(DataList list)
        {
            Dictionary<int, RechargePriceModel> rechargePriceDic = new Dictionary<int, RechargePriceModel>();
            foreach (var item in list)
            {
                Data data = item.Value;
                RechargePriceModel model = new RechargePriceModel(data);
                rechargePriceDic.Add(model.Id, model);
            }

            RechargeLibrary.rechargePriceDic = rechargePriceDic;
        }

        public static void InitRechargeToekn(DataList list)
        {
            Dictionary<int, float> rechargeTokenDic = new Dictionary<int, float>();
            foreach (var item in list)
            {
                Data data = item.Value;
                float price = data.GetFloat("Price");
                rechargeTokenDic.Add(data.ID, price);
            }

            RechargeLibrary.rechargeTokenDic = rechargeTokenDic;
        }

        public static void InitRechargeConfig(Data data, DateTime openServerTime)
        {
            GiftLimitTime = data.GetInt("GiftLimitTime");
            OnhookSoulPowerUp = data.GetFloat("OnhookSoulPowerUp") * 0.01f;
            OnhookGoldUp = data.GetFloat("OnhookGoldUp") * 0.01f;
            OrderPageCount = data.GetInt("OrderPageCount");
            MonthCardDays = data.GetInt("MonthCardDays");
            FirstRechargeAccumulate = data.GetInt("FirstRechargeAccumulate");
            PettyGiftTypeNum = data.GetInt("PettyGiftTypeNum");
            AccumulateRechargeOpenDay = data.GetInt("AccumulateRechargeOpenDay");
            TreasureFlipCardMaxRound = data.GetInt("TreasureFlipCardMaxRound");
            string newServerActivityTime = data.GetString("IgnoreNewServerActivityTime");
            if (openServerTime != DateTime.MinValue && !string.IsNullOrEmpty(newServerActivityTime))
            {
                DateTime ignoreNewServerActivityTime = DateTime.Parse(newServerActivityTime);
                if (openServerTime < ignoreNewServerActivityTime)
                {
                    IgnoreNewServerActivity = true;
                }
                else
                {
                    IgnoreNewServerActivity = false;
                }
            }
        }
        //private static void CheckRechargeIdBindExist()
        //{
        //    foreach(var kv in rechargeIdIndex)
        //    {
        //        string temp = kv.Value.InvertedIndex;
        //        if (!string.IsNullOrWhiteSpace(temp))
        //        {
        //            string[] keys = temp.Split('_');
        //            Data data = DataListManager.inst.GetData(keys[0], Convert.ToInt32(keys[1]));
        //            if (data == null)
        //            {
        //                Log.Warn($"check recharge data in rechargeId {kv.Value.RechargeId}");
        //            }
        //        }

        //    }
        //}

        public static void InitRechargeGift(DataList list, DateTime openServerDate)
        {
            Dictionary<RechargeGiftType, Dictionary<int, RechargeGiftModel>> rechargeGifts = new Dictionary<RechargeGiftType, Dictionary<int, RechargeGiftModel>>();
            foreach (var item in list)
            {
                Data data = item.Value;
                RechargeGiftModel model = new RechargeGiftModel(data, openServerDate);
                Dictionary<int, RechargeGiftModel> dic;
                if (rechargeGifts.TryGetValue(model.GiftType, out dic))
                {
                    dic.Add(model.SubType, model);
                }
                else
                {
                    dic = new Dictionary<int, RechargeGiftModel>();
                    dic.Add(model.SubType, model);
                    rechargeGifts.Add(model.GiftType, dic);
                }
            }

            RechargeLibrary.rechargeGifts = rechargeGifts;
        }

        public static void InitGetRechargeRewardCheck(DataList list)
        {
            Dictionary<RechargeGiftType, List<int>> rechargeRewardCheckDic = new Dictionary<RechargeGiftType, List<int>>();
            foreach (var item in list)
            {
                Data data = item.Value;
                List<int> giftItemList;
                if (!rechargeRewardCheckDic.TryGetValue((RechargeGiftType)data.ID, out giftItemList))
                {
                    giftItemList = new List<int>();
                    string giftItemsStr = data.GetString("GiftItems");
                    string[] giftItems = StringSplit.GetArray("|", giftItemsStr);
                    foreach (var itemId in giftItems)
                    {
                        giftItemList.Add(itemId.ToInt());
                    }
                    rechargeRewardCheckDic.Add((RechargeGiftType)data.ID, giftItemList);
                }
            }

            RechargeLibrary.rechargeRewardCheckDic = rechargeRewardCheckDic;
        }

        public static void InitMonthCard(DataList list)
        {
            Dictionary<int, int> monthCardDic = new Dictionary<int, int>();
            foreach (var item in list)
            {
                Data data = item.Value;
                int rechargeItemId = data.GetInt("RechargeItemId");
                monthCardDic.Add(data.ID, rechargeItemId);
            }

            RechargeLibrary.monthCardDic = monthCardDic;
        }

        private static void InitAccumulateRecharge(DataList list)
        {
            Dictionary<int, AccumulateRechargeModel> accumulateRechargeDic = new Dictionary<int, AccumulateRechargeModel>();

            foreach (var item in list)
            {
                AccumulateRechargeModel model = new AccumulateRechargeModel(item.Value);
                accumulateRechargeDic.Add(model.Id, model);
            }

            RechargeLibrary.accumulateRechargeDic = accumulateRechargeDic;
        }

        public static void InitRankRewards()
        {
            List<CampBuildRankRewardData> rankRewards = new List<CampBuildRankRewardData>();
            //rankRewards.Clear();
            DataList dataList = DataListManager.inst.GetDataList("GardenRankReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int id = data.ID;
                CampBuildRankRewardData itemInfo = new CampBuildRankRewardData();

                string rankString = data.GetString("Rank");
                string[] rankArr = rankString.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (rankArr.Length > 1)
                {
                    itemInfo.RankMin = rankArr[0].ToInt();
                    itemInfo.RankMax = rankArr[1].ToInt();
                }
                else
                {
                    itemInfo.RankMin = rankArr[0].ToInt();
                    itemInfo.RankMax = rankArr[0].ToInt();
                }

                itemInfo.Rewards = data.GetString("Rewards");
                itemInfo.EmailId = data.GetInt("EmailId");
                itemInfo.Period = data.GetInt("Period");

                rankRewards.Add(itemInfo);
            }

            RechargeLibrary.rankRewards = rankRewards;
        }


        private static void InitScoreReward()
        {
            Dictionary<int, ScoreRewardModel> newRechargGifteDic = new Dictionary<int, ScoreRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("NewRechargeGift");
            foreach (var kv in dataList)
            {
                ScoreRewardModel model = new ScoreRewardModel(kv.Value);
                newRechargGifteDic.Add(model.Id, model);
            }

            RechargeLibrary.newRechargGifteDic = newRechargGifteDic;
        }

        private static void InitNewRechargeGiftScore()
        {
            Dictionary<int, int> newRechargGifteScoreDic = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("NewRechargeGiftScore");
            foreach (var kv in dataList)
            {
                Data data = kv.Value;
                int id = data.ID;
                int score = data.GetInt("Score");
                newRechargGifteScoreDic.Add(id, score);
            }

            RechargeLibrary.newRechargGifteScoreDic = newRechargGifteScoreDic;
        }

        public static int GetNewRechargGifteScore(int id)
        {
            int model;
            newRechargGifteScoreDic.TryGetValue(id, out model);
            return model;
        }

        public static ScoreRewardModel GetNewRechargGifteItem(int id)
        {
            ScoreRewardModel model;
            newRechargGifteDic.TryGetValue(id, out model);
            return model;
        }


        public static void InitRechargeGiftTime(DataList list, DateTime openServerDate)
        {
            Dictionary<RechargeGiftType, Dictionary<int, RechargeGiftModel>> rechargeGiftTimeDic = new Dictionary<RechargeGiftType, Dictionary<int, RechargeGiftModel>>();
            foreach (var item in list)
            {
                Data data = item.Value;
                RechargeGiftModel model = new RechargeGiftModel(data, openServerDate);
                Dictionary<int, RechargeGiftModel> dic;
                if (rechargeGiftTimeDic.TryGetValue(model.GiftType, out dic))
                {
                    dic.Add(model.SubType, model);
                }
                else
                {
                    dic = new Dictionary<int, RechargeGiftModel>();
                    dic.Add(model.SubType, model);
                    rechargeGiftTimeDic.Add(model.GiftType, dic);
                }
            }

            RechargeLibrary.rechargeGiftTimeDic = rechargeGiftTimeDic;
        }
       
        public static void InitDirectPurchaseTime(DataList list, DateTime openServerDate)
        {
            Dictionary<int, RechargeGiftModel> directPurchaseDic = new Dictionary<int, RechargeGiftModel>();
            foreach (var item in list)
            {
                Data data = item.Value;
                RechargeGiftModel model = new RechargeGiftModel(data, openServerDate);
                if (!directPurchaseDic.ContainsKey(model.SubType))
                {
                    directPurchaseDic.Add(model.SubType, model);
                }
            }

            RechargeLibrary.directPurchaseDic = directPurchaseDic;
        }

        public static CampBuildRankRewardData GetRankRewardInfo(int period, int rank)
        {
            foreach (var item in rankRewards)
            {
                if (item.Period == period && item.RankMin <= rank && rank <= item.RankMax)
                {
                    return item;
                }
            }
            return null;
        }

        public static RechargeItemModel GetRechargeItem(int id)
        {
            RechargeItemModel model;
            rechargeItemDic.TryGetValue(id, out model);
            return model;
        }

        public static RechargeItemModel GetRechargeItemOrSdkItem(int id)
        {
            RechargeItemModel model;
            if (rechargeItemDic.TryGetValue(id, out model))
            {
                return model;
            }
            return GetRechargeItem(id, true);
        }

        public static RechargeItemModel GetRechargeItem(int id, bool isSdkGift)
        {
            RechargeItemModel model;
            if (isSdkGift)
            {
                rechargeItemDicSdk.TryGetValue(id, out model);
            }
            else
            {
                rechargeItemDic.TryGetValue(id, out model);
            }
            return model;
        }


        public static RechargePriceModel GetRechargePrice(int rechargeId)
        {
            RechargePriceModel model;
            rechargePriceDic.TryGetValue(rechargeId, out model);
            return model;
        }

        public static RechargeGiftType GetRechargeGiftType(int itemId)
        {
            RechargeItemModel model;
            rechargeItemDic.TryGetValue(itemId, out model);
            if (model != null)
            {
                return model.GiftType;
            }
            return RechargeGiftType.None;
        }

        public static int GetRechargeGiftSubType(int itemId)
        {
            RechargeItemModel model;
            rechargeItemDic.TryGetValue(itemId, out model);
            if (model != null)
            {
                return model.SubType;
            }
            return 0;
        }

        public static int GetRechargeItemId(string productId)
        {
            int itemId = 0;
            productIdDic.TryGetValue(productId, out itemId);
            return itemId;
        }

        public static float GetRechargeTokenPrice(int itemId)
        {
            float price = 0;
            rechargeTokenDic.TryGetValue(itemId, out price);
            return price;
        }

        public static int GetDiamondRechargeItemTotalCount()
        {
            Dictionary<int, RechargeItemModel> commonItems;
            rechargeGiftItems.TryGetValue(RechargeGiftType.Common, out commonItems);
            int count = 0;
            if (commonItems != null)
            {
                foreach (var item in commonItems)
                {
                    if ((CommonGiftType)item.Value.SubType == CommonGiftType.Diamond)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static Dictionary<int, RechargeGiftModel> GetRechargeGiftModelByGiftType(RechargeGiftType giftType)
        {
            Dictionary<int, RechargeGiftModel> dic;
            rechargeGifts.TryGetValue(giftType, out dic);
            return dic;
        }

        public static bool CheckInRechargeGiftTime(RechargeGiftType type, DateTime time)
        {
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftModelByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.EndTime)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckInRechargeGiftTime(RechargeGiftType type, DateTime time, out RechargeGiftModel model)
        {
            model = null;
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftModelByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.EndTime)
                    {
                        model = gift.Value;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckInRechargeGiftTime(RechargeGiftType type, DateTime time, out List<int> list)
        {
            list = new List<int>();
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftModelByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.EndTime)
                    {
                        RechargeGiftModel model = gift.Value;
                        list.Add(model.SubType);
                    }
                }
                if (list.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool InitRechargeGiftTime(RechargeGiftType type, DateTime time, out RechargeGiftModel model)
        {
            model = null;
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftModelByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.ShowEnd)
                    {
                        model = gift.Value;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckIsRechargeReward(RechargeGiftType giftType, int giftItemId)
        {
            List<int> list;
            if (rechargeRewardCheckDic.TryGetValue(giftType, out list) && list.Contains(giftItemId))
            {
                return true;
            }
            return false;
        }

        public static RechargeItemModel GetSuitableGiftItem(RechargeGiftType giftType, int period, int subType)
        {
            RechargeItemModel model = null;
            Dictionary<int, RechargeItemModel> dic;
            if (rechargeGiftItems.TryGetValue(giftType, out dic))
            {
                model = dic.Values.Where(x => x.SubType == period && x.Day == subType).First();
            }
            return model;
        }

        public static int GetMonthCardRechargeItem(int id)
        {
            int rechargeItemId;
            monthCardDic.TryGetValue(id, out rechargeItemId);
            return rechargeItemId;
        }

        public static AccumulateRechargeModel GetAccumulateRechargeById(int id)
        {
            AccumulateRechargeModel model;
            accumulateRechargeDic.TryGetValue(id, out model);
            return model;
        }
        #endregion

        public static Dictionary<DateTime, List<RechargeGiftTimeType>> GetTimingLists(DateTime now)
        {
            Dictionary<DateTime, List<RechargeGiftTimeType>> tasks = new Dictionary<DateTime, List<RechargeGiftTimeType>>();

            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.HiddenWeapon);

            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.HiddenWeaponStart);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.SeaTreasure);

            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.SeaTreasureStart);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.Garden);

            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.GardenStart);
            CheckRewardDateTiming(now, tasks, gifts, RechargeGiftTimeType.GardenEndReward);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.DivineLove);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.DivineLoveStart);

            //海岛登高
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.IslandHigh);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighStart);
            CheckRewardDateTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighEnd);

            CheckIslandRankStageRewardTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighFianlStage1, 1);
            CheckIslandRankStageRewardTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighFianlStage2, 2);
            CheckIslandRankStageRewardTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighFianlStage3, 3);
            CheckIslandRankStageRewardTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighFianlStage4, 4);
            CheckIslandRankStageRewardTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighFianlStage5, 5);
            CheckIslandRankStageRewardTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighFianlStage6, 6);
            CheckIslandRankStageRewardTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighFianlStage7, 7);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.StoneWall);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.StoneWallStart);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.CarnivalBoss);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.CarnivalBossStart);

            //轮盘
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.Roulette);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.RouletteStart);
            CheckRewardDateTiming(now, tasks, gifts, RechargeGiftTimeType.RouletteEnd);

            //皮划艇
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.Canoe);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.CanoeStart);
            CheckRewardDateTiming(now, tasks, gifts, RechargeGiftTimeType.CanoeEnd);

            //中秋
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.MidAutumn);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.MidAutumnStart);
            CheckRewardDateTiming(now, tasks, gifts, RechargeGiftTimeType.MidAutumnEnd);

            //主题烟花
            gifts = GetRechargeGiftTimeByGiftType(RechargeGiftType.ThemeFirework);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.ThemeFireworkStart);
            CheckRewardDateTiming(now, tasks, gifts, RechargeGiftTimeType.ThemeFireworkEnd);

            //九考试炼
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.NineTest);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.NineTestStart);
            CheckRewardDateTiming(now, tasks, gifts, RechargeGiftTimeType.NineTestEnd);

            //排序
            tasks = tasks.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

            return tasks;
        }

        private static void CheckStartDateTiming(DateTime now, Dictionary<DateTime, List<RechargeGiftTimeType>> tasks, Dictionary<int, RechargeGiftModel> gifts, RechargeGiftTimeType type)
        {
            if (gifts != null)
            {
                List<RechargeGiftTimeType> list;
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime.Date == now.Date && now.TimeOfDay <= gift.Value.StartTime.TimeOfDay)
                    {
                        if (tasks.TryGetValue(gift.Value.StartTime, out list))
                        {
                            list.Add(type);
                        }
                        else
                        {
                            list = new List<RechargeGiftTimeType>();
                            list.Add(type);
                            tasks.Add(gift.Value.StartTime, list);
                        }
                        break;
                    }
                }
            }
        }

        private static void CheckStartDateTiming(DateTime now, Dictionary<DateTime, List<RechargeGiftTimeType>> tasks, Dictionary<int, RechargeGiftModel> gifts, RechargeGiftTimeType type, bool checkWeekTime)
        {
            DateTime startTime;
            if (gifts != null)
            {
                List<RechargeGiftTimeType> list;
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartWeekTime != DateTime.MinValue && !IgnoreNewServerActivity)
                    {
                        startTime = gift.Value.StartWeekTime;
                    }
                    else
                    {
                        startTime = gift.Value.StartTime;
                    }
                    if (startTime.Date == now.Date && now.TimeOfDay <= startTime.TimeOfDay)
                    {
                        if (tasks.TryGetValue(startTime, out list))
                        {
                            list.Add(type);
                        }
                        else
                        {
                            list = new List<RechargeGiftTimeType>();
                            list.Add(type);
                            tasks.Add(startTime, list);
                        }
                        break;
                    }
                }
            }
        }

        private static void CheckRewardDateTiming(DateTime now, Dictionary<DateTime, List<RechargeGiftTimeType>> tasks, Dictionary<int, RechargeGiftModel> gifts, RechargeGiftTimeType type)
        {
            if (gifts == null) return;

            List<RechargeGiftTimeType> list;
            foreach (var gift in gifts)
            {
                if (gift.Value.GiveReward.Date == now.Date && now.TimeOfDay <= gift.Value.GiveReward.TimeOfDay)
                {
                    if (tasks.TryGetValue(gift.Value.GiveReward, out list))
                    {
                        list.Add(type);
                    }
                    else
                    {
                        list = new List<RechargeGiftTimeType>();
                        list.Add(type);
                        tasks.Add(gift.Value.GiveReward, list);
                    }
                    break;
                }
            }
        }

        private static void CheckEndDateTiming(DateTime now, Dictionary<DateTime, List<RechargeGiftTimeType>> tasks, Dictionary<int, RechargeGiftModel> gifts, RechargeGiftTimeType type)
        {
            if (gifts == null) return;

            List<RechargeGiftTimeType> list;
            foreach (var gift in gifts)
            {
                if (gift.Value.EndTime.Date == now.Date && now.TimeOfDay <= gift.Value.EndTime.TimeOfDay)
                {
                    if (tasks.TryGetValue(gift.Value.EndTime, out list))
                    {
                        list.Add(type);
                    }
                    else
                    {
                        list = new List<RechargeGiftTimeType>();
                        list.Add(type);
                        tasks.Add(gift.Value.EndTime, list);
                    }
                    break;
                }
            }
        }

        private static void CheckNewServerActivityEndDateTiming(DateTime now, Dictionary<DateTime, List<RechargeGiftTimeType>> tasks, Dictionary<int, RechargeGiftModel> gifts, RechargeGiftTimeType type)
        {
            //走新服活动配置才需要处理
            if (IgnoreNewServerActivity) return;

            if (gifts == null) return;

            List<RechargeGiftTimeType> list;
            foreach (var gift in gifts)
            {
                if (gift.Value.EndWeekTime == DateTime.MinValue)
                {
                    continue;
                }
                DateTime endTime = gift.Value.EndWeekTime;
                if (endTime.Date == now.Date && now.TimeOfDay <= endTime.TimeOfDay)
                {
                    if (tasks.TryGetValue(endTime, out list))
                    {
                        list.Add(type);
                    }
                    else
                    {
                        list = new List<RechargeGiftTimeType>();
                        list.Add(type);
                        tasks.Add(endTime, list);
                    }
                    break;
                }
                //else
                //{
                //    if (gift.Value.StartWeek > 0)
                //    {
                //        endTime = DateTime.MaxValue;
                //    }
                //    else
                //    {
                //        endTime = gift.Value.EndTime;
                //    }
                //}
            }
        }

        //海岛登高周期奖励定时器
        private static void CheckIslandRankStageRewardTiming(DateTime now, Dictionary<DateTime, List<RechargeGiftTimeType>> tasks, Dictionary<int, RechargeGiftModel> gifts, RechargeGiftTimeType type, int stage)
        {
            if (gifts == null) return;

            foreach (var gift in gifts)
            {
                //该周期活动已经结束了
                if (gift.Value.EndTime <= now) continue;

                Dictionary<int, IslandHighRankRewardModel> periodModels = IslandHighLibrary.GetCurPeriodRewardModels(gift.Value.SubType);
                if (periodModels == null) continue; ;

                foreach (var model in periodModels)
                {
                    if (model.Value.Stage == stage)
                    {
                        //最终结算阶段
                        DateTime time = gift.Value.StartTime.Date.Add(model.Value.RewardTime);
                        if (time.Date == now.Date && now.TimeOfDay <= time.TimeOfDay)
                        {
                            List<RechargeGiftTimeType> list;
                            if (tasks.TryGetValue(time, out list))
                            {
                                list.Add(type);
                            }
                            else
                            {
                                list = new List<RechargeGiftTimeType>();
                                list.Add(type);
                                tasks.Add(time, list);
                            }
                        }
                        break;
                    }
                }
            }

        }

        public static Dictionary<DateTime, List<RechargeGiftTimeType>> GetRechargeTimingLists(DateTime now)
        {
            Dictionary<DateTime, List<RechargeGiftTimeType>> tasks = new Dictionary<DateTime, List<RechargeGiftTimeType>>();

            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.ThemeBoss);

            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.ThemeBossStart);
            CheckEndDateTiming(now, tasks, gifts, RechargeGiftTimeType.ThemeBossEnd);

            gifts = GetRechargeSubTypeGiftTime(RechargeGiftType.IslandHighGift, (int)IslandHighGiftSubType.IslandHigh);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.IslandHighGiftStart, true);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.Trident);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.Trident, true);
            CheckNewServerActivityEndDateTiming(now, tasks, gifts, RechargeGiftTimeType.NewServerTridentEnd);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.DragonBoat);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.DragonBoatStart);

            gifts = GetRechargeGiftTimeByGiftType(RechargeGiftType.CarnivalRecharge);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.CarnivalRechargeStart, true);

            gifts = GetRechargeSubTypeGiftTime(RechargeGiftType.CarnivalMall, 1);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.CarnivalMallStart, true);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.HeroDaysRewards);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.HeroDaysRewardsStart, true);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.ShrekInvitation);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.ShrekInvitaionStart);

            gifts = GetRechargeSubTypeGiftTime(RechargeGiftType.IslandHighGift, (int)IslandHighGiftSubType.Canoe);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.CanoeGiftStart, true);

            gifts = GetRechargeSubTypeGiftTime(RechargeGiftType.IslandHighGift, (int)IslandHighGiftSubType.Three);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.IslandGiftThreeStart, true);

            //中秋
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.MidAutumn);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.MidAutumnStart);

            //主题烟花
            gifts = GetRechargeGiftTimeByGiftType(RechargeGiftType.ThemeFirework);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.ThemeFireworkStart);

            //活动商店
            CommonShopLibrary.CheckStartDateTiming(now, tasks, RechargeGiftTimeType.ActivityShopStart);
            CommonShopLibrary.CheckEndDateTiming(now, tasks, RechargeGiftTimeType.ActivityShopEnd);

            //九考试炼
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.NineTest);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.NineTestStart);

            //钻石返利
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.DiamondRebate);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.DiamondRebateStart);

            //玄天宝箱
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.XuanBox);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.XuanBoxStart);

            //九笼祈愿
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.WishLantern);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.WishLanternStart);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.NewRechargeGift);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.NewRechargeGiftStart);

            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.DaysRecharge);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.DaysRechargeStart);

            //史莱克乐园
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.Shrekland);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.ShreklandStart, true);

            //魔鬼训练
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.DevilTraining);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.DevilTrainingStart, true);
            
            //神域赐福
            gifts = GetRechargeGiftModelByGiftType(RechargeGiftType.DomainBenediction);
            CheckStartDateTiming(now, tasks, gifts, RechargeGiftTimeType.DomainBenedictionStart, true);

            //排序
            tasks = tasks.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

            return tasks;
        }

        public static Dictionary<int, RechargeGiftModel> GetRechargeGiftTimeByGiftType(RechargeGiftType giftType)
        {
            Dictionary<int, RechargeGiftModel> dic;
            rechargeGiftTimeDic.TryGetValue(giftType, out dic);
            return dic;
        }

        //非充值活动页签的充值时间检查
        public static bool CheckInSpecialRechargeGiftTime(RechargeGiftType type, DateTime time)
        {
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.EndTime)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckInSpecialRechargeGiftShowTime(RechargeGiftType type, DateTime time, out RechargeGiftModel model)
        {
            model = null;
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.ShowEnd)
                    {
                        model = gift.Value;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckInSpecialRechargeGiftTime(RechargeGiftType type, DateTime time, out List<int> list)
        {
            list = new List<int>();
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.EndTime)
                    {
                        RechargeGiftModel model = gift.Value;
                        list.Add(model.SubType);
                    }
                }
                if (list.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static Dictionary<int, RechargeGiftModel> GetRechargeSubTypeGiftTime(RechargeGiftType giftType, int subType)
        {
            RechargeGiftModel activity;
            Dictionary<int, RechargeGiftModel> subDic = null;
            Dictionary<int, RechargeGiftModel> dic;
            rechargeGiftTimeDic.TryGetValue(giftType, out dic);
            if (dic == null)
            {
                return subDic;
            }
            dic.TryGetValue(subType, out activity);
            if (activity == null)
            {
                return subDic;
            }
            subDic = new Dictionary<int, RechargeGiftModel>();
            subDic.Add(subType, activity);
            return subDic;
        }

        public static bool CheckInSpecialRechargeGiftSubTypeTime(RechargeGiftType type, int subType, DateTime time, out RechargeGiftModel model)
        {
            model = null;
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.EndTime && subType == gift.Value.SubType)
                    {
                        model = gift.Value;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckInSpecialRechargeGiftTime(RechargeGiftType type, DateTime time, out RechargeGiftModel activity)
        {
            activity = null;
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    if (gift.Value.StartTime <= time && time <= gift.Value.EndTime)
                    {
                        activity = gift.Value;
                        return true;
                    }
                }
            }
            return false;
        }
        #region 新服活动
        #region 活动页签
        //根据活动结束时间结束
        public static bool CheckInRechargeActivityTime(RechargeGiftType activityType, DateTime now)
        {
            //老服活动检查
            if (IgnoreNewServerActivity)
            {
                return CheckInRechargeGiftTime(activityType, now);              
            }
            else
            {
                //走新服配置检查
                return CheckNewServerActivityTime(activityType, now);
            }
        }

        public static bool CheckInRechargeActivityTime(RechargeGiftType activityType, DateTime now, out RechargeGiftModel activityModel)
        {
            //老服活动检查
            if (IgnoreNewServerActivity)
            {
                return CheckInRechargeGiftTime(activityType, now, out activityModel);            
            }
            else
            {
                //走新服配置检查
                return CheckNewServerActivityTime(activityType, now, out activityModel);
            }
        }

        private static bool CheckNewServerActivityTime(RechargeGiftType activityType, DateTime now)
        {
            Dictionary<int, RechargeGiftModel> activitys = GetRechargeGiftModelByGiftType(activityType);
            if (activitys == null)
            {
                return false;
            }

            foreach (var activity in activitys.Values)
            {
                //忽略没有屏蔽的老服活动
                if (activity.StartWeekTime != DateTime.MinValue)
                {
                    //老服活动需要屏蔽
                    if (activity.EndWeekTime == DateTime.MinValue)
                    {
                        if (now >= activity.StartWeekTime && now <= activity.EndTime)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        //新服活动                     
                        if (now >= activity.StartWeekTime && now <= activity.EndWeekTime)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool CheckNewServerActivityTime(RechargeGiftType activityType, DateTime now, out RechargeGiftModel activityModel)
        {
            activityModel = null;
            Dictionary<int, RechargeGiftModel> activitys = GetRechargeGiftModelByGiftType(activityType);
            if (activitys == null)
            {
                return false;
            }

            foreach (var activity in activitys.Values)
            {
                //忽略没有屏蔽的老服活动
                if (activity.StartWeekTime != DateTime.MinValue)
                {
                    //老服活动需要屏蔽
                    if (activity.EndWeekTime == DateTime.MinValue)
                    {
                        if (now >= activity.StartWeekTime && now <= activity.EndTime)
                        {
                            activityModel = activity;
                            return true;
                        }
                    }
                    else
                    {
                        //新服活动
                        if (now >= activity.StartWeekTime && now <= activity.EndWeekTime)
                        {
                            activityModel = activity;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //根据页签显示结束时间结束
        public static bool CheckInRechargeActivityShowTime(RechargeGiftType type, DateTime time, out RechargeGiftModel model)
        {
            //老服活动检查
            if (IgnoreNewServerActivity)
            {
                return InitRechargeGiftTime(type, time, out model);
            }
            else
            {
                //走新服配置检查
                return CheckNewServerActivityShowTime(type, time, out model);
            }
        }
    
        private static bool CheckNewServerActivityShowTime(RechargeGiftType activityType, DateTime now, out RechargeGiftModel activityModel)
        {
            activityModel = null;
            Dictionary<int, RechargeGiftModel> activitys = GetRechargeGiftModelByGiftType(activityType);
            if (activitys == null)
            {
                return false;
            }

            foreach (var activity in activitys.Values)
            {
                //忽略没有屏蔽的老服活动
                if (activity.StartWeekTime != DateTime.MinValue && now >= activity.StartWeekTime && now <= activity.ShowEnd)
                {
                    activityModel = activity;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 活动累充，直购礼包
        public static bool CheckInSpecialRechargeActivityTime(RechargeGiftType type, DateTime time)
        {
            //老服活动检查
            if (IgnoreNewServerActivity)
            {
                return CheckInSpecialRechargeGiftTime(type, time);
            }
            else
            {
                //走新服配置检查
                return CheckNewSpecialRechargeGiftTime(type, time);
            }
        }

        private static bool CheckNewSpecialRechargeGiftTime(RechargeGiftType type, DateTime time)
        {
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    //忽略没有屏蔽的老服活动
                    if (gift.Value.StartWeekTime != DateTime.MinValue)
                    {
                        //老服活动需要屏蔽
                        if (gift.Value.EndWeekTime == DateTime.MinValue)
                        {
                            if (time >= gift.Value.StartWeekTime && time <= gift.Value.EndTime)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            //新服活动
                            if (time >= gift.Value.StartWeekTime && time <= gift.Value.EndWeekTime)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckInSpecialRechargeActivityTime(RechargeGiftType type, DateTime time, out List<int> list)
        {
            //老服活动检查
            if (IgnoreNewServerActivity)
            {
                return CheckInSpecialRechargeGiftTime(type, time, out list);
            }
            else
            {
                //走新服配置检查
                return CheckNewSpecialRechargeGiftTime(type, time, out list);
            }
        }

        private static bool CheckNewSpecialRechargeGiftTime(RechargeGiftType type, DateTime time, out List<int> list)
        {
            RechargeGiftModel giftModel = null;

            list = new List<int>();
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts.Values)
                {
                    //忽略没有屏蔽的老服活动
                    if (gift.StartWeekTime != DateTime.MinValue)
                    {
                        //老服活动需要屏蔽
                        if (gift.EndWeekTime == DateTime.MinValue)
                        {
                            if (time >= gift.StartWeekTime && time <= gift.EndTime)
                            {
                                giftModel = gift;
                                list.Add(giftModel.SubType);
                            }
                        }
                        else
                        {
                            //新服活动
                            if (time >= gift.StartWeekTime && time <= gift.EndWeekTime)
                            {
                                giftModel = gift;
                                list.Add(giftModel.SubType);
                            }
                        }
                    }
                }
                if (list.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        
        public static bool CheckInSpecialRechargeActivitySubTypeTime(RechargeGiftType type, int subType, DateTime time, out RechargeGiftModel model)
        {
            //老服活动检查
            if (IgnoreNewServerActivity)
            {
                return CheckInSpecialRechargeGiftSubTypeTime(type, subType, time, out model);
            }
            else
            {
                //走新服配置检查
                return CheckNewSpecialRechargeGiftSubTypeTime(type, subType, time, out model);
            }
        }

        private static bool CheckNewSpecialRechargeGiftSubTypeTime(RechargeGiftType type, int subType, DateTime time, out RechargeGiftModel model)
        {
            model = null;
         
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts.Values)
                {
                    //忽略没有屏蔽的老服活动
                    if (gift.StartWeekTime != DateTime.MinValue)
                    {
                        //老服活动需要屏蔽
                        if (gift.EndWeekTime == DateTime.MinValue)
                        {
                            if (time >= gift.StartWeekTime && time <= gift.EndTime && subType == gift.SubType)
                            {
                                model = gift;
                                return true;
                            }
                        }
                        else
                        {
                            //新服活动
                            if (time >= gift.StartWeekTime && time <= gift.EndWeekTime && subType == gift.SubType)
                            {
                                model = gift;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool CheckNewSpecialRechargeGiftTime(RechargeGiftType type, DateTime time, out RechargeGiftModel activity)
        {
            activity = null;
            Dictionary<int, RechargeGiftModel> gifts = GetRechargeGiftTimeByGiftType(type);
            if (gifts != null)
            {
                foreach (var gift in gifts)
                {
                    //忽略没有屏蔽的老服活动
                    if (gift.Value.StartWeekTime != DateTime.MinValue)
                    {
                        //老服活动需要屏蔽
                        if (gift.Value.EndWeekTime == DateTime.MinValue)
                        {
                            if (time >= gift.Value.StartWeekTime && time <= gift.Value.EndTime)
                            {
                                activity = gift.Value;
                                return true;
                            }
                        }
                        else
                        {
                            //新服活动
                            if (time >= gift.Value.StartWeekTime && time <= gift.Value.EndWeekTime)
                            {
                                activity = gift.Value;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckInSpecialRechargeActivityTime(RechargeGiftType type, DateTime time, out RechargeGiftModel activity)
        {
            //老服活动检查
            if (IgnoreNewServerActivity)
            {
                return CheckInSpecialRechargeGiftTime(type, time, out activity);
            }
            else
            {
                //走新服配置检查
                return CheckNewSpecialRechargeGiftTime(type, time, out activity);
            }
        }
        #endregion

        #region giftTpye = 20的直购礼包（原为前端礼包，后端没有验证）
        public static bool CheckInDirectPurchaseTime(DateTime now, out RechargeGiftModel activityModel)
        {
            //老服活动检查
            if (IgnoreNewServerActivity)
            {
                return CheckInOldDirectPurchaseTime(now, out activityModel);
            }
            else
            {
                //走新服配置检查
                return CheckInNewDirectPurchaseTime(now, out activityModel);
            }
        }

        public static bool CheckInOldDirectPurchaseTime(DateTime time, out RechargeGiftModel activityModel)
        {
            activityModel = null;
            foreach (var gift in directPurchaseDic)
            {
                if (gift.Value.StartTime <= time && time <= gift.Value.EndTime)
                {
                    activityModel = gift.Value;
                    return true;
                }
            }
            return false;
        }

        private static bool CheckInNewDirectPurchaseTime(DateTime now, out RechargeGiftModel activityModel)
        {
            activityModel = null;
            foreach (var activity in directPurchaseDic.Values)
            {
                //忽略没有屏蔽的老服活动
                if (activity.StartWeekTime != DateTime.MinValue)
                {
                    //老服活动需要屏蔽
                    if (activity.EndWeekTime == DateTime.MinValue)
                    {
                        if (now >= activity.StartWeekTime && now <= activity.EndTime)
                        {
                            activityModel = activity;
                            return true;
                        }
                    }
                    else
                    {
                        //新服活动                     
                        if (now >= activity.StartWeekTime && now <= activity.EndWeekTime)
                        {
                            activityModel = activity;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        public static DateTime GetActivityStartTime(RechargeGiftModel model)
        {
            DateTime startTime = DateTime.MinValue;
            if (model == null)
            {
                return startTime;
            }
            if (model.StartWeekTime != DateTime.MinValue && !RechargeLibrary.IgnoreNewServerActivity)
            {
                startTime = model.StartWeekTime;
            }
            else
            {
                startTime = model.StartTime;
            }
            return startTime;
        }
        #endregion
    }
}
