using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar : FieldObject
    {
        /// <summary>
        /// 登出
        /// </summary>
        public void BIRecordLogoutLog()
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RecordLogoutLog(uid, AccountName, Name, DeviceId, ChannelId, server.MainId, ClientIp, 
                GetCoins(CurrenciesType.gold), GetCoins(CurrenciesType.diamond), GetCoins(CurrenciesType.exp), GetCoins(CurrenciesType.friendlyHeart), 
                GetCoins(CurrenciesType.sotoCoin), GetCoins(CurrenciesType.resonanceCrystal), GetCoins(CurrenciesType.shellCoin), 
                HeroMng.CalcBattlePower(), (int)(server.Now() - LastLoginTime).TotalSeconds, Level, SDKUuid, HuntingManager.Research);
        }
        /// <summary>
        /// 登录
        /// </summary>
        public void BIRecordLoginLog()
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RecordLoginLog(uid, AccountName, Name, DeviceId, ChannelId, server.MainId, ClientIp, Level, SDKUuid, 1, GetCoins(CurrenciesType.diamond), GetCoins(CurrenciesType.gold), GetCoins(CurrenciesType.exp), HeroMng.CalcBattlePower(), Idfa, Caid, Idfv, Imei, Oaid, Imsi, Anid, PackageName, ExtendId);
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="money"></param>
        /// <param name="orderId"></param>
        /// <param name="moneyType"></param>
        /// <param name="payWay"></param>
        /// <param name="productId"></param>
        public void BIRecordRechargeLog(float money, long orderId, string orderInfo, string productId, string rechargeGiftType, string rechargeSubType, string payWay = "1", string moneyType = "CNY")
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RecordRechargeLog(uid, AccountName, DeviceId, ChannelId, server.MainId, money, orderId.ToString(), orderInfo, orderInfo, moneyType, payWay, productId, rechargeGiftType, rechargeSubType, Level, SDKUuid);

            server.BILoggerMng.RechargeTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, money, orderId.ToString(), orderInfo, rechargeGiftType, moneyType, payWay, productId, Level, SDKUuid, Idfa, Caid, Idfv, Imei, Oaid, Imsi, Anid, PackageName);

            RecordRechargeLog(money, orderId.ToString(), orderInfo, orderInfo, moneyType, payWay, productId);
        }

        public void BIRecordTokenConsumeLog(float money, long orderId, string orderInfo, string productId, string rechargeGiftType, string rechargeSubType, string payWay = "1", string moneyType = "CNY")
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RecordRechargeLog(uid, AccountName, DeviceId, ChannelId, server.MainId, money, orderId.ToString(), orderInfo, orderInfo, moneyType, payWay, productId, rechargeGiftType, rechargeSubType, Level, SDKUuid);

            server.BILoggerMng.TokenConsumeTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, money, orderId.ToString(), orderInfo, rechargeGiftType, moneyType, payWay, productId, Level, ExtendId, 0, SDKUuid);

            RecordRechargeLog(money, orderId.ToString(), orderInfo, orderInfo, moneyType, payWay, productId);
        }
        

        /// <summary>
        /// 物品获得
        /// </summary>
        /// <param name="count"></param>
        /// <param name="type"></param>
        /// <param name="obtainType"></param>
        public void BIRecordObtainItem(RewardType type, ObtainWay obtainType, int moduleId, int count, int currNum, int year = 0)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //server.BILoggerMng.RecordItemAddLog(ChannelName, server.MainId, ZoneServerApi.now, AccountName, Uid, Level, Name, obtainType.ToString(), type.ToString(), id, count, currCount ,extParam);
            server.BILoggerMng.RecordObtainItem(uid, AccountName, DeviceId, ChannelId, server.MainId, count, currNum, type.ToString(), obtainType.ToString(), moduleId.ToString(), Level, year, SDKUuid);
        }

        /// <summary>
        /// 物品消耗
        /// </summary>
        /// <param name="count"></param>
        /// <param name="type"></param>
        /// <param name="consumeType"></param>
        public void BIRecordConsumeItem(RewardType type, ConsumeWay consumeType, int moduleId, int count, int currCount, BaseItem item)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            int ringLevel = 0;
            SoulRingItem soulRing = item as SoulRingItem;
            if (soulRing != null)
            {
                ringLevel = soulRing.Year;
            }
            //server.BILoggerMng.RecordItemSubLog(ChannelName, server.MainId, ZoneServerApi.now, AccountName, Uid, Level, Name, consumeType.ToString(), type.ToString(), id, count, extParam);
            server.BILoggerMng.RecordConsumeItem(uid, AccountName, DeviceId, ChannelId, server.MainId, count, currCount, type.ToString(), consumeType.ToString(), moduleId.ToString(), Level, SDKUuid, ringLevel);
        }

        /// <summary>
        /// 货币获得
        /// </summary>
        /// <param name="count"></param>
        /// <param name="currencyType"></param>
        /// <param name="obtainType"></param>
        public void BIRecordObtainCurrency(int count, int currCount, CurrenciesType currencyType, ObtainWay obtainType, string moduleId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RecordObtainCurrency(uid, AccountName, DeviceId, ChannelId, server.MainId, count, currCount, currencyType.ToString(), obtainType.ToString(), moduleId, Level, SDKUuid);
            //server.BILoggerMng.RecordCurrencyAddLog(ChannelName, server.MainId, ZoneServerApi.now, AccountName, Uid, Level, Name, obtainType.ToString(), currencyType.ToString(), (int)currencyType, count);
        }


        /// <summary>
        /// 仓库货币获得
        /// </summary>
        /// <param name="count"></param>
        /// <param name="currCount"></param>
        /// <param name="currencyType"></param>
        /// <param name="obtainType"></param>
        /// <param name="moduleId"></param>
        public void BIRecordObtainWarehouseCurrency(int count, long currCount, CurrenciesType currencyType, ObtainWay obtainType, string moduleId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RecordObtainWarehouseCurrency(uid, AccountName, DeviceId, ChannelId, server.MainId, count, currCount, currencyType.ToString(), obtainType.ToString(), moduleId, Level, SDKUuid);
        }

        /// <summary>
        /// 货币消耗
        /// </summary>
        /// <param name="count"></param>
        /// <param name="currencyType"></param>
        /// <param name="consumeType"></param>
        public void BIRecordConsumeCurrency(int count, int currCount, CurrenciesType currencyType, ConsumeWay consumeType, string moduleId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RecordConsumeCurrency(uid, AccountName, DeviceId, ChannelId, server.MainId, count, currCount, currencyType.ToString(), consumeType.ToString(), moduleId, Level, SDKUuid);
            //server.BILoggerMng.RecordCurrencySubLog(ChannelName, server.MainId, ZoneServerApi.now, AccountName, Uid, Level, Name, consumeType.ToString(), currencyType.ToString(), (int)currencyType, count);
        }

        /// <summary>
        /// 仓库货币消耗
        /// </summary>
        /// <param name="count"></param>
        /// <param name="currencyType"></param>
        /// <param name="consumeType"></param>
        public void BIRecordConsumeWarehouseCurrency(int count, long currCount, CurrenciesType currencyType, ConsumeWay consumeType, string moduleId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RecordConsumeWarehouseCurrency(uid, AccountName, DeviceId, ChannelId, server.MainId, count, currCount, currencyType.ToString(), consumeType.ToString(), moduleId, Level, SDKUuid);
        }

        /// <summary>
        /// 任务
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="taskId"></param>
        /// <param name="taskState"></param>
        public void BIRecordRecordTaskLog(TaskType taskType, int taskId, int taskState)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //state  1.接受  2.放弃 3.失败 4.完成
            //server.BILoggerMng.RecordTaskLog(uid, AccountName, DeviceId, ChannelName, server.MainId, taskType.ToString(), taskId, taskState, HeroMng.CalcBattlePower(), Level, SDKUuid);
            //server.BILoggerMng.TaskTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, taskType.ToString(), taskId, taskState, HeroMng.CalcBattlePower(), Level, SDKUuid);
            //if (taskState == 1)
            //{
            //    server.BILoggerMng.RecordTaskAddLog(ChannelName, server.MainId, ZoneServerApi.now, AccountName, Uid, Level, Name, taskId);
            //}
            //else if (taskState == 4)
            //{
            //    server.BILoggerMng.RecordTaskSubLog(ChannelName, server.MainId, ZoneServerApi.now, AccountName, Uid, Level, Name, taskId);
            //}

        }

        /// <summary>
        /// 关卡
        /// </summary>
        /// <param name="pointType"></param>
        /// <param name="pointId"></param>
        /// <param name="pointState"></param>
        /// <param name="useTime"></param>
        public void BIRecordCheckPointLog(MapType pointType, string pointId, int pointState, int useTime)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //state  1 成功 2 失败
            //server.BILoggerMng.RecordCheckPointLog(uid, AccountName, DeviceId, ChannelName, server.MainId, pointType.ToString(), pointId, pointState, useTime, HeroMng.CalcBattlePower(), Level, SDKUuid);
            server.BILoggerMng.CheckPointTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, pointType.ToString(), pointId, pointState, useTime, HeroMng.CalcBattlePower(), Level, SDKUuid);

            //server.BILoggerMng.RecordTaskSubLog(ChannelName, server.MainId, ZoneServerApi.now, AccountName, Uid, Level, Name, taskId);
        }
        /// <summary>
        /// 活动
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="activityId"></param>
        public void BIRecordActivityLog(ActivityAction activityType, int activityId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //state  1 成功 2 失败
            //server.BILoggerMng.RecordActivityLog(uid, AccountName, DeviceId, ChannelName, server.MainId, activityType.ToString(), activityId, SDKUuid);
            server.BILoggerMng.ActivityTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, activityType.ToString(), activityId, SDKUuid);
        }

        /// <summary>
        /// 养成
        /// </summary>
        /// <param name="developType"></param>
        /// <param name="targetId"></param>
        /// <param name="beforeLevel"></param>
        /// <param name="afterLevel"></param>
        /// <param name="heroId"></param>
        /// <param name="heroLevel"></param>
        public void BIRecordDevelopLog(DevelopType developType, int targetId, int beforeLevel, int afterLevel, int heroId = 0, int heroLevel = 0)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //state  1 成功 2 失败
            //server.BILoggerMng.RecordDevelopLog(uid, AccountName, DeviceId, ChannelName, server.MainId, developType.ToString(), targetId.ToString(), beforeLevel, afterLevel, heroId, heroLevel, Level, SDKUuid);
            server.BILoggerMng.DevelopTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, developType.ToString(), targetId.ToString(), beforeLevel, afterLevel, heroId, heroLevel, Level, SDKUuid);
        }


        /// <summary>
        /// 商店
        /// </summary>
        public void BIRecordShopByItemLog(ShopType shopType, string currencyType, int currencyCount, ObtainWay obtainType, RewardType moduleId, int itemType, int itemCount, TimingGiftType timingGiftType, int buyCount)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }

            server.BILoggerMng.RecordShopByItem(uid, AccountName, DeviceId, ChannelId, server.MainId, shopType.ToString(), currencyType, currencyCount, obtainType.ToString(), moduleId.ToString(), itemType, itemCount, Level, SDKUuid, timingGiftType.ToString(), buyCount);

        }

        /// <summary>
        /// 抽卡
        /// </summary>
        public void BIRecordRecruitHeroLog(int drawType, string currencyType, int currencyCount, List<int> heroIds)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }

            //server.BILoggerMng.RecordRecruitHero(uid, AccountName, DeviceId, ChannelName, server.MainId, drawType.ToString(), currencyType, currencyCount, heroIds, Level, SDKUuid);
            server.BILoggerMng.RecruitHeroTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, drawType.ToString(), currencyType, currencyCount, heroIds, Level, SDKUuid);

        }

        /// <summary>
        /// 魂环替换
        /// </summary>
        public void BIRecordRingReplaceLog(int heroId, int heroLevel, int ringIndex, int oldId, int oldYyear, int newId, int newYear)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }

            //server.BILoggerMng.RecordRingReplace(uid, AccountName, DeviceId, ChannelName, server.MainId, heroId, heroLevel, ringIndex, oldId, oldYyear, newId, newYear, Level, SDKUuid);
            server.BILoggerMng.RingReplaceTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, heroId, heroLevel, ringIndex, oldId, oldYyear, newId, newYear, Level, SDKUuid);
        }

        /// <summary>
        /// 魂骨替换
        /// </summary>
        public void BIRecordBoneReplaceLog(int heroId, int heroLevel, int ringIndex, int oldId, int oldPower, int newId, int newPower)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }

            //server.BILoggerMng.RecordBoneReplace(uid, AccountName, DeviceId, ChannelName, server.MainId, heroId, heroLevel, ringIndex, oldId, oldPower, newId, newPower, Level, SDKUuid);
            server.BILoggerMng.BoneReplaceTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, heroId, heroLevel, ringIndex, oldId, oldPower, newId, newPower, Level, SDKUuid);
        }

        /// <summary>
        /// 装备替换
        /// </summary>
        public void BIRecordEquipmentReplaceLog(int heroId, int heroLevel, int ringIndex, int oldId, int oldPower, int newId, int newPower)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }

            //server.BILoggerMng.RecordEquipmentReplace(uid, AccountName, DeviceId, ChannelName, server.MainId, heroId, heroLevel, ringIndex, oldId, oldPower, newId, newPower, Level, SDKUuid);
            server.BILoggerMng.EquipmentReplaceTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, heroId, heroLevel, ringIndex, oldId, oldPower, newId, newPower, Level, SDKUuid);
        }

        public void BIRecordEquipmentUpgradeLog(int heroId, int heroLevel, int equipmentIndex, int id, int state, int oldPower, int newPower, int oldlevel, int newlevel, int stoneNum)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //state   1 成功  2 失败
            //server.BILoggerMng.RecordEquipmentUpgrade(uid, AccountName, DeviceId, ChannelName, server.MainId, heroId, heroLevel, equipmentIndex, id, state, oldPower, newPower, Level, SDKUuid);
            server.BILoggerMng.EquipmentTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, heroId, heroLevel, equipmentIndex, id, state, oldPower, newPower, oldlevel, newlevel, stoneNum, Level, SDKUuid);
        }

        public void BIRecordLimitPackLog(float currencyCount, string currencyType, int operationType, int operationTime, int launchId, int packId, ulong sequence)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //server.BILoggerMng.RecordLimitPack(uid, AccountName, DeviceId, ChannelName, server.MainId, Level, currencyCount, currencyType, launchId, packId, operationType, operationTime, SDKUuid);
            server.BILoggerMng.LimitPackTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, currencyCount, currencyType, launchId, packId, operationType, operationTime, sequence, SDKUuid);
        }

        public void BIRecordGodReplaceLog(int cardId, int cardLevel, int oldGodId, int oldBattlePower, int newGodId, int newBattlePower)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //server.BILoggerMng.RecordGodReplace(uid, Level, AccountName, DeviceId, ChannelName, server.MainId, cardId, cardLevel, oldGodId, oldBattlePower, newGodId, newBattlePower, SDKUuid);
            server.BILoggerMng.GodReplaceTaLog(uid, Level, AccountName, DeviceId, ChannelId, server.MainId, cardId, cardLevel, oldGodId, oldBattlePower, newGodId, newBattlePower, SDKUuid);
        }

        public void BIRecordGodLog(int cardId, int cardLevel, int godId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //server.BILoggerMng.RecordGod(uid, Level, AccountName, DeviceId, ChannelName, server.MainId, cardId, cardLevel, godId, SDKUuid);
            server.BILoggerMng.GodTaLog(uid, Level, AccountName, DeviceId, ChannelId, server.MainId, cardId, cardLevel, godId, SDKUuid);
        }


        public void BIRecordWishingWellLog(int wellState, CurrenciesType currencyIn, CurrenciesType currencyOut, int quantityIn, int quantityOut)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //server.BILoggerMng.RecordWishingWell(uid, AccountName, DeviceId, ChannelName, server.MainId, Level, wellState, currencyIn.ToString(), currencyOut.ToString(), quantityIn, quantityOut, SDKUuid);
            server.BILoggerMng.WishingWellTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, wellState, currencyIn.ToString(), currencyOut.ToString(), quantityIn, quantityOut, SDKUuid);
        }

        public void BIRecordTreasureMapLog(int passState, int leftHp, int gotMapCount)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //server.BILoggerMng.RecordTreasureMap(uid, AccountName, DeviceId, ChannelName, server.MainId, Level, passState, leftHp, gotMapCount, SDKUuid);
            server.BILoggerMng.TreasureMapTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, passState, leftHp, gotMapCount, SDKUuid);
        }

        public void BIRecordLevelupLog(string levelType, int beforeLevel, int afterLevel, int expTime)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.LevelupTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, levelType, beforeLevel, afterLevel, expTime, SDKUuid);
        }

        public void BIRecordEquipRedeemLog(int quantity, float discount, int beastSoul, int gold)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.EquipRedeemTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, quantity, discount, beastSoul, gold, SDKUuid);
        }

        //刷新
        public void BIRecordRefreshLog(string lastTime, string timingType, int timingId, string way)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.TrackingLoggerMng.RecordRefreshLog(uid, AccountName, DeviceId, ChannelId, server.MainId, lastTime, timingType, timingId, way, server.Now());
        }

        public void BIRecordPackageCodeLog(string packageCude, string packageChannel, bool allOrOne, int packageId)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.PackageCodeTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, packageCude, packageChannel, allOrOne, packageId, SDKUuid);
        }

        public void BIRecordThemeBossLog(int period, int bossLevel, int degree)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.ThemeBossTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, period, bossLevel, degree);
        }

        public void BIRecordCampBuildLog(int phaseNum, int realStep, int stepCount)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.CampBuildTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, phaseNum, realStep, stepCount);
        }

        public void BIRecordGardenLog(int period, int diamondNum, bool useDiamond, int score, int totalScore)
        {
            // LOG 记录开关 
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            int gainType = 1;
            if (useDiamond)
            {
                gainType = 2;
            }
            server.BILoggerMng.GardenTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, period, diamondNum, gainType, score, totalScore);
        }

        public void BIRecordRenameLog(string oldName, string newName)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }    
            server.BILoggerMng.RenameTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, oldName, newName);
        }

        public void BIRecordContributionLog(int contribution, int phaseNum, int currentValue)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.ContributionTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, contribution, phaseNum, currentValue);
        }

        public void BIRecordIslandHighLog(int itemId, int random, int before, int totalScore, RechargeGiftModel activityModel)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            int stage = 0;
            if (activityModel != null)
            {
                Dictionary<int, IslandHighRankRewardModel> periodModels = IslandHighLibrary.GetCurPeriodRewardModels(activityModel.SubType);
                foreach (var model in periodModels)
                {
                    DateTime time = activityModel.StartTime.Date.Add(model.Value.RewardTime);
                    if (time.Date == ZoneServerApi.now.Date && ZoneServerApi.now.TimeOfDay <= time.TimeOfDay)
                    {
                        stage = model.Value.Stage;
                        break;
                    }
                }
            }
            int curScore = totalScore - before;
            server.BILoggerMng.IslandHighTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, stage, itemId, random, curScore, totalScore);
        }

        public void BIRecordTravelLog(int solt, int heroId, string eventType, int eventId)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
           
            server.BILoggerMng.IsTravelTaLog(uid, AccountName, DeviceId, ChannelId, server.MainId, Level, solt, heroId, eventType, eventId.ToString());
        }

        public void BIRecordRankActiveLog(string[] serverIdArr, string rankType, int firstUid, int firstValue, int luckyUid)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.RankActiveTaLog(Uid, ChannelId, server.MainId, serverIdArr, rankType, firstUid, firstValue, luckyUid);
        }

        public void CreateInheritBiEquipsInfo(Dictionary<int, int[]> equipsInfo)
        {
            //1:toOldEquipsId
            //2:toOldEquipsLevel
            //3:fromOldEquipsId
            //4:fromOldEquipsLevel
            //5:toNewEquipsId
            //6.toNewEquipsLevel
            //7.fromNewEquipsId
            //8.fromNewEquipsLevel

            int[] infoArr;
            for (int i = 1; i <= 8; i++)
            {
                infoArr = new int[5];
                equipsInfo.Add(i, infoArr);
            }
        }

        public void BIRecordInheritLog(int toId, int fromId, Dictionary<int, int[]> equipsInfo)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            //传承 from 被传承 to
            //1:toOldEquipsId  2:toOldEquipsLevel  3:fromOldEquipsId  4:fromOldEquipsLevel
            //5:toNewEquipsId  6.toNewEquipsLevel  7.fromNewEquipsId  8.fromNewEquipsLevel         

            server.BILoggerMng.InheritTaLog(Uid, Level, AccountName, DeviceId, ChannelId, server.MainId, toId, fromId, equipsInfo[1], equipsInfo[2], equipsInfo[3], equipsInfo[4], equipsInfo[5], equipsInfo[6], equipsInfo[7], equipsInfo[8], SDKUuid);
        }

        public void BIRecordGodStepUpLog(int heroId, int godLevel)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.GodLevelUpTaLog(Uid, Level, AccountName, DeviceId, ChannelId, server.MainId, heroId.ToString(), godLevel, SDKUuid);
        }

        public void BIRecordPointGameLog(int score, int totalScore, string activityType, int activityNum)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.PointGameTaLog(Uid, AccountName, DeviceId, ChannelId, server.MainId, Level, score, totalScore, activityType, activityNum);
        }

        public void BIRecordPetTowerLog(int towerLevel, int pointState, int petId, int petLevel, int petAptitude, int petBreakLevel)
        {
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            server.BILoggerMng.PetTowerTaLog(Uid, AccountName, DeviceId, ChannelId, server.MainId, Level, towerLevel, pointState, petId, petLevel, petAptitude, petBreakLevel); 
        }

        public void BIRecordPetDevelopLog(string developType, PetInfo petInfo, int changeNum, int consumeItemId, int consumeNum)
        {
            if (petInfo == null)
            {
                return;
            }
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            int afterNum = 0;
            switch (developType)
            {
                case "level_up":
                    afterNum = petInfo.Level;
                    break;
                case "feed":
                    afterNum = petInfo.Satiety;
                    break;
                case "break":
                    afterNum = petInfo.BreakLevel;
                    break;
                default:
                    break;
            }
            server.BILoggerMng.PetDevelopTaLog(Uid, AccountName, DeviceId, ChannelId, server.MainId, Level, developType, petInfo.PetId, petInfo.Level, petInfo.Aptitude, petInfo.BreakLevel, (afterNum - changeNum).ToString(), afterNum.ToString() , consumeItemId.ToString(), consumeNum);
        }

        public void BIRecordPetSkillLog(PetInfo petInfo, string beforeSkill, string afterSkill, string beforeRarity, string afterRarity, int protect)
        {
            if (petInfo == null)
            {
                return;
            }
            // LOG 记录开关
            if (!GameConfig.TrackingLogSwitch)
            {
                return;
            }
            List<Dictionary<string, object>> skillList = GetPetSkillsBILogInfo(petInfo.PassiveSkills);
            server.BILoggerMng.PetSkillTaLog(Uid, AccountName, DeviceId, ChannelId, server.MainId, Level, petInfo.PetId, petInfo.Level, petInfo.Aptitude, petInfo.BreakLevel, beforeSkill, afterSkill , beforeRarity, afterRarity, protect, skillList);
        }

        private List<Dictionary<string, object>> GetPetSkillsBILogInfo(Dictionary<int, int> passiveSkills)
        {
            List<Dictionary<string, object>> skillList = new List<Dictionary<string, object>>();
            foreach (int skillId in passiveSkills.Values)
            {
                Dictionary<string, object> skillInfo = new Dictionary<string, object>();
                skillInfo.Add("skill", skillId);
                PetPassiveSkillModel skillModel = PetLibrary.GetPetPassiveSkillModel(skillId);
                int rarity = skillModel != null ? skillModel.Quality : 1;
                skillInfo.Add("rarity", rarity);
                skillList.Add(skillInfo);
            }
            return skillList;
        }
    }
}
