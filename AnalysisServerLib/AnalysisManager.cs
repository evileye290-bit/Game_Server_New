using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Analysis.Protocol.AZ;
using Message.Zone.Protocol.ZA;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalysisServerLib
{
    //    【脚本】筛选礼包类别
    //步骤  说明	
    //1	触发条件，权重加值	
    //2	常规礼包购买情况，购买的常驻礼包对应项的权重增加，日礼包增加5，周礼包增加10，月礼包增加10 最近7天
    //3	索托城商店购买情况，购买的对应项的权重增加5（多次购买相同类别不会连续加权）	最近2天
    //4	各玩法额外次数购买情况，对应项权重增加，增加值为花费钻石数除以50 最近2天
    //5	1.5 > 玩家资源消耗数量/购买量 > 1，权重乘以1.5（对应消耗数）	最近2天
    //6	上三期限时礼包购买情况，如果购买，则对应项的权重翻倍，如果未购买，则对应项的权重缩小到50%	
    //7	根据权重排名第一，分配礼包

    //update 20210312 新增限时礼包限制次数只针对648挡位

    public class AnalysisManager
    {
        private ZoneServer server;
        private static int LatestTimingGiftBuyInfoCount = 3;//限时礼包购买情况期数
        private RecommendGiftManager recommendGiftManager = new RecommendGiftManager();

        private DateTime now => server.Api.Now();

        public AnalysisManager(ZoneServer server)
        {
            this.server = server;
        }

        public string BuildRechargeTableName(int day)
        {
            return "recharge_" + BuildTime(day);
        }

        public string BuildShopTableName(int day)
        {
            return "shop_" + BuildTime(day);
        }

        public string BuildObtainCurrencyTableName(int day)
        {
            return "obtaincurrency_" + BuildTime(day);
        }

        public string BuildConsumeCurrencyTableName(int day)
        {
            return "consumecurrency_" + BuildTime(day);
        }

        public string BuildLoginTableName(int day)
        {
            return "login_" + BuildTime(day);
        }

        public string BuildConsumeitemTableName(int day)
        {
            return "itemconsume_" + BuildTime(day);
        }

        private string BuildTime(int day)
        {
            DateTime time = now.AddDays(day * -1);
            return time.ToString("yyyy_MM_dd");
        }

        public void Update()
        {
            recommendGiftManager.Update();
        }

        public void CaculateRecommendGift(MSG_ZA_GET_TIMING_GIFT msg, int uid)
        {
            try
            {
                Log.Info($"player {uid} CaculateRecommendGift action {msg.ActionId}");
                List<AbstractDBQuery> queries = new List<AbstractDBQuery>();

                // 1 基础权重
                ActionModel actionModel = ActionLibrary.GetActionModel(msg.ActionId);
                Dictionary<TimingGiftType, float> weight = ActionLibrary.GetActionWeight(msg.ActionId);
                if (weight == null || actionModel == null)
                {
                    Log.Warn($"player {uid} CaculateRecommendGift error, error info have not action {msg.ActionId} weight");
                    return;
                }

                // 2 7日日/周/月充值 (常规礼包购买情况，购买的常驻礼包对应项的权重增加，日礼包增加5，周礼包增加10，月礼包增加10)
                for (int i = 0; i < Math.Min(7, msg.CreateDays); i++)
                {
                    queries.Add(BuildLoadRechargeTypeCount(uid, CommonGiftType.Daily, i));
                    queries.Add(BuildLoadRechargeTypeCount(uid, CommonGiftType.Weekly, i));
                    queries.Add(BuildLoadRechargeTypeCount(uid, CommonGiftType.Monthly, i));
                }

                //3 所托城商店购买  索托城商店购买情况，购买的对应项的权重增加5（多次购买相同类别不会连续加值）              
                BuildShopBuyQueries(queries, uid, msg.CreateDays);

                //4 各个玩法额外购买 各玩法额外次数购买情况，对应项权重增加，增加值为花费钻石数除以50
                BuildPlayTypeBuyQueries(queries, uid, msg.CreateDays);

                //5 资源消耗/购买 0.5 > 玩家资源消耗数量/购买量 > 0，权重乘以0.5（对应消耗数）

                //钻石购买消耗
                QueryLoadConsumeCurrency queryConsumeDiamond0 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.diamond, 0);
                QueryLoadConsumeCurrency queryConsumeDiamond1 = null;
                queries.Add(queryConsumeDiamond0);

                //银币购买消耗
                QueryLoadConsumeCurrency queryConsumeGold0 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.gold, 0);
                QueryLoadConsumeCurrency queryConsumeGold1 = null;
                queries.Add(queryConsumeGold0);

                //魂力购买消耗
                QueryLoadConsumeCurrency queryConsumeSoulPower0 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.soulPower, 0);
                QueryLoadConsumeCurrency queryConsumeSoulPower1 = null;
                queries.Add(queryConsumeSoulPower0);

                //魂尘购买消耗
                QueryLoadConsumeCurrency queryConsumeSoulDust0 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.soulDust, 0);
                QueryLoadConsumeCurrency queryConsumeSoulDust1 = null;
                queries.Add(queryConsumeSoulDust0);

                //魂息购买消耗
                QueryLoadConsumeCurrency queryConsumeSoulBreath0 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.soulBreath, 0);
                QueryLoadConsumeCurrency queryConsumeSoulBreath1 = null;
                queries.Add(queryConsumeSoulBreath0);

                //魂晶购买消耗
                QueryLoadConsumeCurrency queryConsumeSoulCrystal0 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.soulCrystal, 0);
                QueryLoadConsumeCurrency queryConsumeSoulCrystal1 = null;
                queries.Add(queryConsumeSoulCrystal0);

                //七连抽券
                int itemId = (int)ConsumableType.DrawCard7;
                QueryLoadConsumeItem queryConsume70 = BuildQueryLoadConsumeCurrency(uid, RewardType.NormalItem, itemId, 0);
                QueryLoadConsumeItem queryConsume71 = null;
                queries.Add(queryConsume70);

                //猎杀魂兽扫荡券
                itemId = (int)ConsumableType.HuntingSweep;
                QueryLoadConsumeItem queryConsumeHuntingSweep0 = BuildQueryLoadConsumeCurrency(uid, RewardType.NormalItem, itemId, 0);
                QueryLoadConsumeItem queryConsumeHuntingSweep1 = null;
                queries.Add(queryConsumeHuntingSweep0);

                //强化石
                List<QueryLoadConsumeItem> queryConsumeEquipUpgradeList = new List<QueryLoadConsumeItem>();

                if (msg.CreateDays > 1)
                {
                    queryConsumeDiamond1 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.diamond, 1);
                    queries.Add(queryConsumeDiamond1);
                    
                    queryConsumeGold1 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.gold, 1);
                    queries.Add(queryConsumeGold1);
                    
                    queryConsumeSoulPower1 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.soulPower, 1);
                    queries.Add(queryConsumeSoulPower1);
                    
                    queryConsumeSoulDust1 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.soulDust, 1);
                    queries.Add(queryConsumeSoulDust1);
                    
                    queryConsumeSoulBreath1 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.soulBreath, 1);
                    queries.Add(queryConsumeSoulBreath1);
                    
                    queryConsumeSoulCrystal1 = BuildQueryLoadConsumeCurrency(uid, CurrenciesType.soulCrystal, 1);
                    queries.Add(queryConsumeSoulCrystal1);
                    
                    queryConsume71 = BuildQueryLoadConsumeCurrency(uid, RewardType.NormalItem, itemId, 1);
                    queries.Add(queryConsume71);
                    
                    queryConsumeHuntingSweep1 = BuildQueryLoadConsumeCurrency(uid, RewardType.NormalItem, itemId, 1);
                    queries.Add(queryConsumeHuntingSweep1);
                    
                    //强化石
                    itemId = (int)ConsumableType.EquipmentGhost3; //史诗兽魂
                    QueryLoadConsumeItem queryConsumeEquipUpgrade1 = BuildQueryLoadConsumeCurrency(uid, RewardType.NormalItem, itemId, 1);
                    queries.Add(queryConsumeEquipUpgrade1);
                    queryConsumeEquipUpgradeList.Add(queryConsumeEquipUpgrade1);

                    if (msg.CreateDays > 2)
                    {
                        QueryLoadConsumeItem queryConsumeEquipUpgrade2 = BuildQueryLoadConsumeCurrency(uid, RewardType.NormalItem, itemId, 2);
                        queries.Add(queryConsumeEquipUpgrade2);
                        queryConsumeEquipUpgradeList.Add(queryConsumeEquipUpgrade2);
                    }

                    itemId = (int)ConsumableType.EquipmentSpar;
                    QueryLoadConsumeItem queryConsumeEquipUpgrade3 = BuildQueryLoadConsumeCurrency(uid, RewardType.NormalItem, itemId, 1);
                    queries.Add(queryConsumeEquipUpgrade3);
                    queryConsumeEquipUpgradeList.Add(queryConsumeEquipUpgrade3);

                    if (msg.CreateDays > 2)
                    {
                        QueryLoadConsumeItem queryConsumeEquipUpgrade4 = BuildQueryLoadConsumeCurrency(uid, RewardType.NormalItem, itemId, 2);
                        queries.Add(queryConsumeEquipUpgrade4);
                        queryConsumeEquipUpgradeList.Add(queryConsumeEquipUpgrade4);
                    }
                }



                //最近7天登陆情况
                for (int i = 0; i < Math.Min(msg.CreateDays, 7); i++)
                {
                    queries.Add(new QueryLoadLogin(uid, BuildLoginTableName(i)));
                }

                DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(queries, true);

                Log.Debug($"player {uid} actionId {msg.ActionId} action type {msg.ActionType} setp 1 : enter call db queries start, basic weight {weight.ToString("|", ":")}");

                server.Api.LogDBPool.Call(dBQuerysWithoutTransaction, ret =>
                {
                    if (true)
                    {
                        int loginDaysInRecent = 0;
                        // 2 7日日/周/月充值 (常规礼包购买情况，购买的常驻礼包对应项的权重增加，日礼包增加5，周礼包增加10，月礼包增加10)
                        //3	索托城商店购买情况，购买的对应项的权重增加5（多次购买相同类别不会连续加权）	最近2天
                        //4 各个玩法额外购买 各玩法额外次数购买情况，对应项权重增加，增加值为花费钻石数除以50
                        AddRechargeGiftCount(uid, msg.ActionId, msg.ActionType, weight, queries, out loginDaysInRecent);

                        //5 资源消耗/购买 0.5 > 玩家资源消耗数量/购买量 > 0，权重乘以0.5（对应消耗数）
                        AddResouceRatio(weight, TimingGiftType.GainCharacter, msg.DrawHero7, 
                            queryConsume70.Count + (queryConsume71 == null ? 0 : queryConsume71.Count));
                        
                        AddResouceRatio(weight, TimingGiftType.LevelUp, GetCurrencyNum(msg, CurrenciesType.soulPower), 
                            queryConsumeSoulPower0.Count + (queryConsumeSoulPower1 == null ? 0 : queryConsumeSoulPower1.Count));

                        AddResouceRatio(weight, TimingGiftType.Break, GetCurrencyNum(msg, CurrenciesType.soulCrystal), 
                            queryConsumeSoulCrystal0.Count + (queryConsumeSoulCrystal1 == null ? 0 : queryConsumeSoulCrystal1.Count));

                        AddResouceRatio(weight, TimingGiftType.SoulRing, msg.HuntingSweet, 
                            queryConsumeHuntingSweep0.Count + (queryConsumeHuntingSweep1 == null ? 0 : queryConsumeHuntingSweep1.Count));

                        AddResouceRatio(weight, TimingGiftType.SoulRingLevelUp, GetCurrencyNum(msg, CurrenciesType.soulDust), 
                            queryConsumeSoulDust0.Count + (queryConsumeSoulDust1 == null ? 0 : queryConsumeSoulDust1.Count));

                        AddResouceRatio(weight, TimingGiftType.SoulRingBreak, GetCurrencyNum(msg, CurrenciesType.soulBreath), 
                            queryConsumeSoulBreath0.Count + (queryConsumeSoulBreath1 == null ? 0 : queryConsumeSoulBreath1.Count));

                        AddResouceRatio(weight, TimingGiftType.Gold, GetCurrencyNum(msg, CurrenciesType.gold),
                            queryConsumeGold0.Count + (queryConsumeGold1 == null ? 0 : queryConsumeGold1.Count));

                        AddResouceRatio(weight, TimingGiftType.Diamond, GetCurrencyNum(msg, CurrenciesType.diamond),
                            queryConsumeDiamond0.Count + (queryConsumeDiamond1 == null ? 0 : queryConsumeDiamond1.Count));
                        
                        AddResouceRatio(weight, TimingGiftType.EquipmentUpgrade, msg.EquipentUpgrade, 
                            queryConsumeEquipUpgradeList.Sum(x=>x.Count));

                        //6 上三期限时礼包购买情况，如果购买，则对应项的权重翻倍，如果未购买，则对应项的权重缩小到50%
                        List<MSG_ZA_TIMING_GIFT_INFO> historyGifts = msg.TimingGiftInfo.OrderByDescending(x => x.CreateTime).ToList();
                        for (int i = 0; i < Math.Min(LatestTimingGiftBuyInfoCount, historyGifts.Count); i++)
                        {
                            AddLatestBuyTimeGiftWeight(uid, msg.ActionId, msg.ActionType, weight, historyGifts[i]);
                        }

                        //移除限制的gift
                        RemoveLimitedTimingGiftType(msg, uid, weight);

                        weight = weight.OrderByDescending(x => x.Value).ToDictionary(key => key.Key, value => value.Value);

#if DEBUG
                        Log.Debug($"player {uid} actionId {msg.ActionId} action type {msg.ActionType} enter setp 6 , weight {weight.ToString("|", ":")}");
#endif

                        //7 根据权重排名第一，分配礼包
                        TimingGiftType giftType = weight.First().Key;

                        //获取对应挡位
                        bool needResetRecentMaxMoney;
                        int level = GetTimingGiftLevel(msg, actionModel, giftType, uid, loginDaysInRecent, msg.CreateDays, out needResetRecentMaxMoney);
                        int rechargeId = ActionLibrary.GetGiftItemId(level, giftType);

                        //20210312 新增限时礼包限制次数只针对648挡位
                        if (level >= ActionLibrary.MaxStep)
                        {
                            //缓存今天购买的次数
                            recommendGiftManager.AddPlayerTimingGiftBuyedCount(uid, giftType);
#if DEBUG
                            Log.Debug($"player {uid} actionId {msg.ActionId} action type {msg.ActionType} enter setp7 , gift type {giftType}, weight {weight.ToString("|", ":")}");
#endif
                        }

                        Log.Warn($"player {uid} recommend gift type {giftType} level {level} id {rechargeId}");

                        MSG_AZ_GET_TIMING_GIFT response = new MSG_AZ_GET_TIMING_GIFT()
                        {
                            ActionId = msg.ActionId,
                            TimingGiftType = (int)giftType,
                            Level = level,
                            ProductId = rechargeId,
                            ResetRecentMaxMoney = needResetRecentMaxMoney,
                            DataBox = msg.DataBox,
                        };

                        server.Write(response, uid);
                    }
                    else
                    {
                        Log.Warn($"player {uid} CaculateRecommendGift error, error info have not action {msg.ActionId} in db");
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        #region query build

        private void BuildShopBuyQueries(List<AbstractDBQuery> queries, int uid, int days)
        {
            QueryLoadBuyShop queryGainCharacter0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.GainCharacter, 0);
            queries.Add(queryGainCharacter0);

            QueryLoadBuyShop queryLevelUp0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.LevelUp, 0);
            queries.Add(queryLevelUp0);

            QueryLoadBuyShop queryBreak0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.Break, 0);
            queries.Add(queryBreak0);

            QueryLoadBuyShop querySoulRing0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.SoulRing, 0);
            queries.Add(querySoulRing0);

            QueryLoadBuyShop querySoulRingLevelUp0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.SoulRingLevelUp, 0);
            queries.Add(querySoulRingLevelUp0);

            QueryLoadBuyShop querySoulRingBreak0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.SoulRingBreak, 0);
            queries.Add(querySoulRingBreak0);

            QueryLoadBuyShop queryDiamond0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.Diamond, 0);
            queries.Add(queryDiamond0);

            QueryLoadBuyShop queryGold0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.Gold, 0);
            queries.Add(queryGold0);

            QueryLoadBuyShop querySoulBone0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.SoulBone, 0);
            queries.Add(querySoulBone0);

            QueryLoadBuyShop queryEquipmentUpgrade0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.EquipmentUpgrade, 0);
            queries.Add(queryEquipmentUpgrade0);

            QueryLoadBuyShop queryEquipmentInject0 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.EquipmentInject, 0);
            queries.Add(queryEquipmentInject0);
            
            if (days > 1)
            {
                QueryLoadBuyShop queryGainCharacter1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.GainCharacter, 1);
                queries.Add(queryGainCharacter1);
                
                QueryLoadBuyShop queryLevelUp1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.LevelUp, 1);
                queries.Add(queryLevelUp1);
                
                QueryLoadBuyShop queryBreak1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.Break, 1);
                queries.Add(queryBreak1);

                QueryLoadBuyShop querySoulRing1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.SoulRing, 1);
                queries.Add(querySoulRing1);

                QueryLoadBuyShop querySoulRingLevelUp1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.SoulRingLevelUp, 1);
                queries.Add(querySoulRingLevelUp1);
                
                QueryLoadBuyShop querySoulRingBreak1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.SoulRingBreak, 1);
                queries.Add(querySoulRingBreak1);

                QueryLoadBuyShop querySoulBone1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.SoulBone, 1);
                queries.Add(querySoulBone1);

                QueryLoadBuyShop queryGold1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.Gold, 1);
                queries.Add(queryGold1);
                
                QueryLoadBuyShop queryDiamond1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.Diamond, 1);
                queries.Add(queryDiamond1);

                QueryLoadBuyShop queryEquipmentUpgrade1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.EquipmentUpgrade, 1);
                queries.Add(queryEquipmentUpgrade1);

                QueryLoadBuyShop queryEquipmentInject1 = BuildQueryLoadBuyShopItem(uid, TimingGiftType.EquipmentInject, 1);
                queries.Add(queryEquipmentInject1);
            }
        }

        private void BuildPlayTypeBuyQueries(List<AbstractDBQuery> queries, int uid, int days)
        {
            //抽卡
            QueryLoadConsumeCurrencyByWay queryGainHero0 = BuildQueryLoadConsumeCurrency(uid, PlayType.Draw, ConsumeWay.DrawHeroCard, 0);
            queries.Add(queryGainHero0);

            //拟态训练 => 银币，魂力，魂骨
            QueryLoadConsumeCurrencyByWay queryOnhook0 = BuildQueryLoadConsumeCurrency(uid, PlayType.OnHook, ConsumeWay.OnHook, 0);
            queries.Add(queryOnhook0);

            //猎杀魂兽 => 魂环
            QueryLoadConsumeCurrencyByWay queryHunting0 = BuildQueryLoadConsumeCurrency(uid, PlayType.Hunting, ConsumeWay.Hunting, 0);
            queries.Add(queryHunting0);

            //整点boss => 魂骨
            QueryLoadConsumeCurrencyByWay queryIntegralBossBuy0 = BuildQueryLoadConsumeCurrency(uid, PlayType.IntegralBoss, ConsumeWay.IntegralBossBuy, 0);
            queries.Add(queryIntegralBossBuy0);

            //高原之乡 => 装备强化
            QueryLoadConsumeCurrencyByWay queryIntegralEquipUpgrade0 = BuildQueryLoadConsumeCurrency(uid, PlayType.BenefitSoulPower, ConsumeWay.BenefitsSoulBreath, 0);
            queries.Add(queryIntegralEquipUpgrade0);

            //冰火两仪 => 魂尘=>魂环升级
            QueryLoadConsumeCurrencyByWay queryPlaySoulRingLevelUp0 = BuildQueryLoadConsumeCurrency(uid, PlayType.BenefitSoulBreath, ConsumeWay.BenefitsSoulPower, 0);
            queries.Add(queryPlaySoulRingLevelUp0);

            //秘境 => 装备
            QueryLoadConsumeCurrencyByWay queryEquip0 = BuildQueryLoadConsumeCurrency(uid, PlayType.SecretArea, ConsumeWay.SecretAreaSweepCountBuy, 0);
            queries.Add(queryEquip0);

            //阵营建设 => 装备强化
            QueryLoadConsumeCurrencyByWay queryCampBuild0 = BuildQueryLoadConsumeCurrency(uid, PlayType.CampBuild, ConsumeWay.CampBuildBuyDiceCount, 0);
            queries.Add(queryCampBuild0);

            //阵营战 => 装备强化，其他
            QueryLoadConsumeCurrencyByWay queryCampBattle0 = BuildQueryLoadConsumeCurrency(uid, PlayType.CampBattle, ConsumeWay.CampFortAddNature, 0);
            queries.Add(queryCampBattle0);

            if (days > 1)
            {
                //抽卡
                QueryLoadConsumeCurrencyByWay queryGainHero1 = BuildQueryLoadConsumeCurrency(uid, PlayType.Draw, ConsumeWay.DrawHeroCard, 1);
                queries.Add(queryGainHero1);

                //拟态训练 => 银币，魂力，魂骨
                QueryLoadConsumeCurrencyByWay queryOnhook1 = BuildQueryLoadConsumeCurrency(uid, PlayType.OnHook, ConsumeWay.OnHook, 1);
                queries.Add(queryOnhook1);

                //猎杀魂兽 => 魂环
                QueryLoadConsumeCurrencyByWay queryHunting1 = BuildQueryLoadConsumeCurrency(uid, PlayType.Hunting, ConsumeWay.Hunting, 1);
                queries.Add(queryHunting1);

                //整点boss => 魂骨
                QueryLoadConsumeCurrencyByWay queryIntegralBossBuy1 = BuildQueryLoadConsumeCurrency(uid, PlayType.IntegralBoss, ConsumeWay.IntegralBossBuy, 1);
                queries.Add(queryIntegralBossBuy1);

                //高原之乡 => 装备强化
                QueryLoadConsumeCurrencyByWay queryIntegralEquipUpgrade1 = BuildQueryLoadConsumeCurrency(uid, PlayType.BenefitSoulPower, ConsumeWay.BenefitsSoulBreath, 1);
                queries.Add(queryIntegralEquipUpgrade1);

                //冰火两仪 => 魂尘=>魂环升级
                QueryLoadConsumeCurrencyByWay queryPlaySoulRingLevelUp1 = BuildQueryLoadConsumeCurrency(uid, PlayType.BenefitSoulBreath, ConsumeWay.BenefitsSoulPower, 1);
                queries.Add(queryPlaySoulRingLevelUp1);

                //秘境 => 装备
                QueryLoadConsumeCurrencyByWay queryEquip1 = BuildQueryLoadConsumeCurrency(uid, PlayType.SecretArea, ConsumeWay.SecretAreaSweepCountBuy, 1);
                queries.Add(queryEquip1);

                //阵营建设 => 装备强化
                QueryLoadConsumeCurrencyByWay queryCampBuild1 = BuildQueryLoadConsumeCurrency(uid, PlayType.CampBuild, ConsumeWay.CampBuildBuyDiceCount, 1);
                queries.Add(queryCampBuild1);

                //阵营战 => 装备强化，其他
                QueryLoadConsumeCurrencyByWay queryCampBattle1 = BuildQueryLoadConsumeCurrency(uid, PlayType.CampBattle, ConsumeWay.CampFortAddNature, 1);
                queries.Add(queryCampBattle1);

            }
        }


        #endregion

        #region 礼包类型

        private void RemoveLimitedTimingGiftType(MSG_ZA_GET_TIMING_GIFT msg, int uid, Dictionary<TimingGiftType, float> weight)
        {
            int diamond = (int)TimingGiftType.Diamond;

            //移除limit限制的gift
            msg.LimitedGiftTypes.Where(x => x != diamond).ForEach(x => weight.Remove((TimingGiftType)x));

            int startTime = Timestamp.GetUnixTimeStampSeconds(now.Date);
            int endTime = Timestamp.GetUnixTimeStampSeconds(now);

            Dictionary<TimingGiftType, int> todayTimigGiftTypeCount = new Dictionary<TimingGiftType, int>();
            //当天各种类礼包出现次数
            //当天该类型的礼包（钻石礼包除外）出现超过三次（只要出现过，不论买没买） 

            //update 20210312 新增限时礼包限制次数只针对648挡位
            msg.TimingGiftInfo.Where(x => 
            x.CreateTime >= startTime && 
            x.CreateTime <= endTime &&
            x.TimingGiftType != diamond &&
            x.ProductMoney >= ActionLibrary.MaxMoney
            ).ForEach(x =>
            {
                TimingGiftType type = (TimingGiftType)x.TimingGiftType;
                if (todayTimigGiftTypeCount.ContainsKey(type))
                {
                    todayTimigGiftTypeCount[type] += 1;
                }
                else
                {
                    todayTimigGiftTypeCount.Add(type, 1);
                }
            });

            Dictionary<TimingGiftType, int> todayCachedCount = recommendGiftManager.GetPlayerTimingGiftBuyedCount(uid);

#if DEBUG
            if (todayCachedCount == null)
            {
                todayCachedCount = new Dictionary<TimingGiftType, int>();
            }
            todayTimigGiftTypeCount = todayTimigGiftTypeCount.OrderByDescending(x => x.Value).ToDictionary(key => key.Key, value => value.Value);

            Log.Debug($"player {uid} actionId {msg.ActionId} action type {msg.ActionType} today each type gift had recommended count {todayCachedCount.ToString("|", ":")}, weight {weight.ToString("|", ":")}");
            Log.Debug($"player {uid} actionId {msg.ActionId} action type {msg.ActionType} remove limited gift {string.Join("|", msg.LimitedGiftTypes)} remove today limit gift, each type gift had recommended count {todayTimigGiftTypeCount.ToString("|", ":")}, weight {weight.ToString("|", ":")}");
#endif

            //移除超过ActionLibrary.TodaySameTimingGiftTypeMaxCount次数的礼包类型
            todayCachedCount?.Where(x => x.Value >= ActionLibrary.GetTimingGiftDailtLimitCount(x.Key) && x.Key != TimingGiftType.Diamond).ForEach(x => weight.Remove(x.Key));
            todayTimigGiftTypeCount.Where(x => x.Value >= ActionLibrary.GetTimingGiftDailtLimitCount(x.Key)).ForEach(x => weight.Remove(x.Key));
        }

        private int GetCurrencyNum(MSG_ZA_GET_TIMING_GIFT msg, CurrenciesType type)
        {
            int count;
            if (msg.Currencies.TryGetValue((int)type, out count))
            {
                return count;
            }
            return 0;
        }

        private void AddRechargeGiftCount(int uid, int actionId, int actionType, Dictionary<TimingGiftType, float> weight, List<AbstractDBQuery> queries, out int loginDays)
        {
            loginDays = 0;

#if DEBUG
            Dictionary<CommonGiftType, int> giftBuyedCount = new Dictionary<CommonGiftType, int>()
            {
                {  CommonGiftType.Daily, 0},
                {  CommonGiftType.Weekly, 0},
                {  CommonGiftType.Monthly, 0}
            };

            Dictionary<TimingGiftType, int> shopBuyedTimingGiftType = new Dictionary<TimingGiftType, int>();
            Dictionary<TimingGiftType, int> playTypeBuyedTimingGiftType = new Dictionary<TimingGiftType, int>();
#endif

            foreach (var kv in queries)
            {
                if (kv is QueryLoadRechargeTypeCount)
                {
                    QueryLoadRechargeTypeCount query = kv as QueryLoadRechargeTypeCount;
                    foreach (var productId in query.ProductIds)
                    {
                        RechargeItemModel model = RechargeLibrary.GetRechargeItem(productId);
                        if (model == null) continue;

                        if (model.GiftType != RechargeGiftType.Common) continue;

                        CommonGiftType giftType = (CommonGiftType)model.SubType;
                        //int addWeight = GetRechargeGiftWeight(giftType);//需求修改 根据付费金额

                        int addWeight = 0;
                        RechargePriceModel priceModel = RechargeLibrary.GetRechargePrice(model.RechargeId);
                        if (priceModel != null)
                        {
                            //addWeight = (int)(priceModel.Money / 100 / 2);
                            addWeight = (int)(priceModel.Money *10);
                        }

#if DEBUG
                        if (addWeight > 0)
                        {
                            giftBuyedCount[giftType] += 1;
                        }
#endif

                        model.TimingGiftTypes.ForEach(x => AddWeight(weight, x, addWeight));
                    }
                }
                else if (kv is QueryLoadBuyShop)
                {
                    QueryLoadBuyShop query = kv as QueryLoadBuyShop;
                    if (query.Count <= 0) continue;

#if DEBUG
                    if (!shopBuyedTimingGiftType.ContainsKey(query.TimingGiftType))
                    {
                        shopBuyedTimingGiftType.Add(query.TimingGiftType, query.Count);
                    }
                    else
                    {
                        shopBuyedTimingGiftType[query.TimingGiftType] += query.Count;
                    }
#endif

                    AddWeight(weight, query.TimingGiftType, query.Count * ActionLibrary.ShopBuyAddWeight);
                }
                else if (kv is QueryLoadConsumeCurrencyByWay)
                {
                    QueryLoadConsumeCurrencyByWay query = kv as QueryLoadConsumeCurrencyByWay;
                    if (query.Count <= 0) continue;

#if DEBUG
                    AddBuyPlayTypeWeight(weight, query.PlayType, query.Count, playTypeBuyedTimingGiftType);
#else
                    AddBuyPlayTypeWeight(weight, query.PlayType, query.Count);
#endif
                }
                else if (kv is QueryLoadLogin)
                {
                    QueryLoadLogin query = kv as QueryLoadLogin;
                    loginDays += query.Count;
                }
            }

#if DEBUG
            Log.Debug($"player {uid} actionId {actionId} action type {actionType} enter setp2 recharge buy count {giftBuyedCount.ToString("|", ":")}, weight {weight.ToString("|", ":")}");
            Log.Debug($"player {uid} actionId {actionId} action type {actionType} enter setp3 , shop buy count {shopBuyedTimingGiftType.ToString("|", ":")} weight {weight.ToString("|", ":")} each add weight {ActionLibrary.ShopBuyAddWeight}");
            Log.Debug($"player {uid} actionId {actionId} action type {actionType} enter setp4 , cost diamond {playTypeBuyedTimingGiftType.ToString("|", ":")} weight {weight.ToString("|", ":")} divid param {ActionLibrary.PlayTypeCostDiamondDivideParam}");
#endif
        }

        public int GetRechargeGiftWeight(CommonGiftType type)
        {
            switch (type)
            {
                case CommonGiftType.Daily: return ActionLibrary.DailyTimingBagAddWeight;
                case CommonGiftType.Weekly: return ActionLibrary.WeeklyTimingBagAddWeight;
                case CommonGiftType.Monthly: return ActionLibrary.MonthlyTimingBagAddWeight;
            }
            return 0;
        }

        public void AddBuyPlayTypeWeight(Dictionary<TimingGiftType, float> weights, PlayType playType, int costDiamond, Dictionary<TimingGiftType, int> currAddWeight = null)
        {
            float addWeight = costDiamond * 1f / ActionLibrary.PlayTypeCostDiamondDivideParam;
            ActionLibrary.GetPlayType2TimingGiftTypes(playType)?.ForEach(x =>
            {
                AddWeight(weights, x, addWeight);

#if DEBUG
                if (!currAddWeight.ContainsKey(x))
                {
                    currAddWeight.Add(x, costDiamond);
                }
                else
                {
                    currAddWeight[x] += costDiamond;
                }
#endif
            });
        }

        public void AddResouceRatio(Dictionary<TimingGiftType, float> weights, TimingGiftType type, int currentCount, int consumeCount, bool log = true)
        {
#if DEBUG
            if (log)
            {
                Log.Debug($"enter setp5 , resouce gift type {type} current count {currentCount} consume count {consumeCount}");
            }
#endif
            if (consumeCount == 0)
            {
                return;
            }

            float ratio = currentCount * 1f / consumeCount;

#if DEBUG
            if (log)
            {
                Log.Debug($"enter setp5 , resouce ratio gift type {type} ratio {ratio} weight {weights.ToString("|", ":")}");
            }
#endif

            if (ratio < ActionLibrary.ResourceCurrDivideCostMin || ratio > ActionLibrary.ResourceCurrDivideCostMax) return;

            AddResouceRatio(weights, type, ActionLibrary.ResourceCurrDivideCostRatio, log);
        }

        public void AddResouceRatio(Dictionary<TimingGiftType, float> weights, TimingGiftType type, float ratio, bool log = true)
        {
            if (weights.ContainsKey(type))
            {
                weights[type] *= ratio;
            }
        }

        public void AddLatestBuyTimeGiftWeight(int uid, int actionId, int actionType, Dictionary<TimingGiftType, float> weights, MSG_ZA_TIMING_GIFT_INFO info)
        {
            TimingGiftType giftType = (TimingGiftType)info.TimingGiftType;
            float ratio = info.Buyed ? ActionLibrary.BuyTimingGiftTypeAddWeightRatio : ActionLibrary.NotBuyTimingGiftTypeAddWeightRatio;

            AddResouceRatio(weights, giftType, ratio, false);

#if DEBUG
            Log.Debug($"player {uid} actionId {actionId} action type {actionType} setp 6 : buy last timing gift TimingGiftType {giftType} buyed {info.Buyed} ratio {giftType} weight {weights.ToString("|", ":")}");
#endif
        }

        private void AddWeight(Dictionary<TimingGiftType, float> weights, TimingGiftType type, float weight)
        {
            if (!weights.ContainsKey(type))
            {
                weights.Add(type, weight);
            }
            else
            {
                weights[type] += weight;
            }
        }

        private QueryLoadRechargeTypeCount BuildLoadRechargeTypeCount(int uid, CommonGiftType commonGiftType, int day)
        {
            return new QueryLoadRechargeTypeCount(uid, RechargeGiftType.Common.ToString(), commonGiftType, BuildRechargeTableName(day));
        }

        private QueryLoadBuyShop BuildQueryLoadBuyShopItem(int uid, TimingGiftType timingGiftType, int day)
        {
            return new QueryLoadBuyShop(uid, BuildShopTableName(day), timingGiftType);
        }

        private QueryLoadConsumeCurrencyByWay BuildQueryLoadConsumeCurrency(int uid, PlayType playType, ConsumeWay consumeWay, int day)
        {
            return new QueryLoadConsumeCurrencyByWay(uid, BuildConsumeCurrencyTableName(day), CurrenciesType.diamond.ToString(), consumeWay.ToString(), playType);
        }

        private QueryLoadConsumeCurrency BuildQueryLoadConsumeCurrency(int uid, CurrenciesType type, int day)
        {
            return new QueryLoadConsumeCurrency(uid, BuildConsumeCurrencyTableName(day), type.ToString());
        }

        private QueryLoadObtainCurrency BuildQueryLoadObtainCurrencyByBuy(int uid, CurrenciesType type, ObtainWay obtainWay, int day)
        {
            return new QueryLoadObtainCurrency(uid, BuildObtainCurrencyTableName(day), type.ToString(), obtainWay.ToString());
        }

        private QueryLoadConsumeItem BuildQueryLoadConsumeCurrency(int uid, RewardType type, int modelId, int day)
        {
            return new QueryLoadConsumeItem(uid, BuildConsumeitemTableName(day), type.ToString(), modelId.ToString());
        }

        #endregion

        #region 挡位

        private bool IsBuyedOrTimeout(MSG_ZA_TIMING_GIFT_INFO info, DateTime now)
        {
            return info.Buyed || (now - Timestamp.TimeStampToDateTime(info.CreateTime)).TotalHours >= 2;
        }

        public int GetTimingGiftLevel(MSG_ZA_GET_TIMING_GIFT msg, ActionModel mode, TimingGiftType giftType, int uid, int loginDaysin7, int createDays, out bool needResetRecentMaxMoney)
        {
            int level = 0;
            DateTime now = DateTime.Now;
            needResetRecentMaxMoney = false;
            ActionModel model = ActionLibrary.GetActionModel(msg.ActionId);

            bool isFirstGift = msg.TimingGiftInfo.Count <= 0;

            //相同频率只取买了的或者过期了的
            List<MSG_ZA_TIMING_GIFT_INFO> recentOrderList = msg.TimingGiftInfo.OrderByDescending(x=>x.CreateTime).Where(x => IsBuyedOrTimeout(x, now)).ToList();

            //倒数一二的礼包
            MSG_ZA_TIMING_GIFT_INFO lastTimingGift= null, lastSecondTimingGift = null;

            //最近最大付费金额对应的挡位（连续两次推荐未购买money会重置为0）
            int maxMoneyLevel = ActionLibrary.GetStepByMoney(msg.MaxProductMoney);

            foreach (var kv in recentOrderList)
            {
                if (lastTimingGift == null)
                {
                    lastTimingGift = kv;
                    continue;
                }

                if (lastSecondTimingGift== null)
                {
                    lastSecondTimingGift = kv;
                    break; ;
                }
            }

            //上次未购买
            bool lastTimingGiftNotBuyed = CheckedNotBuyed(lastTimingGift, now);
            //上上次未购买
            bool lastSecondTimingGiftNotBuyed = CheckedNotBuyed(lastSecondTimingGift, now);

            if (lastTimingGift != null)
            {
                //买过上次礼包
                //TimingGiftStepInfo info = ActionLibrary.GetGiftStepInfoByItemId(lastTimingGift.ProduceId);
                //if (info == null)
                //{
                //    Logger.Log.Warn($"recharge item info error productId {info}");
                //}
                //else
                {
                    level = ActionLibrary.GetStepByMoney(lastTimingGift.ProductMoney);

#if DEBUG
                    Log.Debug($"player {uid} gift type {giftType} setp1, lastSameFrequence gift type {lastTimingGift.TimingGiftType} buyed last productId {lastTimingGift.ProduceId} {lastTimingGift.Buyed} level by lastSameFrequence, gift level {level}");
#endif
                }
            }
            else
            {
                //未买过上次礼包 取玩家当前最大单笔付费金额，根据区间定档n
                level = maxMoneyLevel;

#if DEBUG
                Log.Debug($"player {uid} gift type {giftType} setp1, level by MaxProductMoney, gift level {level}");
#endif
            }

            bool isBatter = false;

            //上次是否购买
            if (lastTimingGift?.Buyed == true)
            {
                DateTime lastCreateTime = Timestamp.TimeStampToDateTime(lastTimingGift.CreateTime);
                DateTime lastBuyTime = Timestamp.TimeStampToDateTime(lastTimingGift.BuyedTime);

                //十分钟内购买
                if ((lastBuyTime - lastCreateTime).TotalMinutes < 10)
                {
                    //+1是破冰挡位（相同付款额度，奖励更好），+2是真正提升一个挡位（提升一个付款额度）
                    level += 2;

#if DEBUG
                    Log.Debug($"player {uid} gift type {giftType} setp2, lastTimingGift gift type {lastTimingGift.TimingGiftType} buyed last productId {lastTimingGift.ProduceId} level by lastSameFrequence and in {10} min, gift level {level}");
#endif
                }
            }
            else
            {
                //上次未购买（超过两小时没买），判断上上次
                if (lastTimingGiftNotBuyed && lastSecondTimingGiftNotBuyed)
                {
                    //上上次也没买
                    //-1是破冰挡位（相同付款额度，奖励更好），-2是真正降低一个挡位（降低一个付款额度）
                    level -= 2;

                    //降档的同时需要重置最近购买的最大挡位
                    needResetRecentMaxMoney = true;
#if DEBUG
                    Log.Debug($"player {uid} gift type {giftType} setp4, level by lastSecondSameFrequence not buyed , gift level {level}");
#endif
                }
            }

            //是否高于等于礼包历史推荐最高档次，是则为下一挡位的破冰挡位
            //float thisMoney = ActionLibrary.GetStepMoney(level);
            //var orderedByMoney = msg.TimingGiftInfo.OrderByDescending(x => x.ProductMoney);
            var orderedByMoney = msg.TimingGiftInfo.OrderByDescending(x => x.Step);
            MSG_ZA_TIMING_GIFT_INFO highRecharge = orderedByMoney.FirstOrDefault();
            //MSG_ZA_TIMING_GIFT_INFO highRechargeBuyedOrTimeout = orderedByMoney.Where(x => IsBuyedOrTimeout(x, now)).FirstOrDefault();
            if (highRecharge != null)
            {
                if (level >= highRecharge.Step)
                {
                    //下一挡位的破冰挡
                    level = level + 2 + 1;
                    isBatter = true;
#if DEBUG
                    Log.Debug($"player {uid} gift type {giftType} setp3, level by highest recommend recharge {highRecharge.ProduceId} money {highRecharge.ProductMoney}, gift level {level}");
#endif
                }
            }

            level = Math.Max(1, level);

            //玩家是否最近7日上线天数低于5天，改为破冰挡位  或者
            //玩家是否连续2次未购买限时礼包 改为破冰挡位
            if (!isBatter)
            {
                //7日类累计登录少于5天
                bool loginLessDays = createDays >= 7 && loginDaysin7 < 5;

                if (isFirstGift || loginLessDays || HadNotBuyedGiftTwice(recentOrderList, now))
                {
                    level += 1;
                    isBatter = true;
#if DEBUG
                    Log.Debug($"player {uid} gift type {giftType} setp5, level by login Days < 5 in 7 days or last twice timing gift had not buyed , gift level {level}");
#endif
                }
            }

            //当前推荐的档位是历史最高档位(包含没有过期的礼包)
            if (!isBatter && level > highRecharge?.Step)
            {
                level += 1;
            }

            //如果当前挡位小于常规礼包的挡位，则推荐常规礼包的挡位
            if (level < maxMoneyLevel)
            {
                if (CheckRecentNormalRecharge(lastTimingGift, lastSecondTimingGift, msg.LastCommonRechargeTime))
                { 
                    level = maxMoneyLevel;
                }
            }

            return level > ActionLibrary.MaxStep + 1 ? ActionLibrary.MaxStep : level;
        }

        private bool CheckedNotBuyed(MSG_ZA_TIMING_GIFT_INFO info, DateTime time)
        {
            if (info == null) return false;

            // 没有过期（超过2小时）不算未购买
            if (info.Buyed || (time - Timestamp.TimeStampToDateTime(info.CreateTime)).TotalHours < 2) return false;

            return true;
        }

        private bool HadNotBuyedGiftTwice(List<MSG_ZA_TIMING_GIFT_INFO> recentOrderList, DateTime time)
        {
            int count = 0;
            if (recentOrderList.Count < 2) return false;
            foreach (var kv in recentOrderList)
            {
                if (!CheckedNotBuyed(kv, time)) return false;

                count++;

                if (count >= 2) return true;
            }
            return false;
        }

        /// <summary>
        /// 两次推荐礼包之间是否买过常规礼包
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="lastTimingGift"></param>
        /// <param name="lastSecondTimingGift"></param>
        /// <returns></returns>
        private bool CheckRecentNormalRecharge(MSG_ZA_TIMING_GIFT_INFO lastTimingGift, MSG_ZA_TIMING_GIFT_INFO lastSecondTimingGift, int lastCommonRechargeTime)
        {
            //购买常规礼包在最近两次限时礼包之后
            if (lastTimingGift != null)
            {
                if (lastCommonRechargeTime > lastTimingGift.CreateTime) return true;
            }

            if (lastSecondTimingGift != null)
            { 
                if (lastCommonRechargeTime > lastSecondTimingGift.CreateTime) return true;
            }

            return false;
        }

#endregion
    }
}
