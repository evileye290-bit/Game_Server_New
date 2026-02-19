using DBUtility;
using DBUtility.Sql;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //货币
        private Dictionary<CurrenciesType, int> currencies = new Dictionary<CurrenciesType, int>();
        /// <summary>
        /// 货币
        /// </summary>
        public Dictionary<CurrenciesType, int> Currencies
        {
            get { return currencies; }
        }

        /// <summary>
        /// 延迟货币更改
        /// </summary>
        private bool currenciesChanged = false;
        /// <summary>
        /// 最后一次同步货币时间
        /// </summary>
        private DateTime lastSyncCurrenciesTime = BaseApi.now;

        public DateTime LastLevelUpTime = BaseApi.now;

        public void BindCurrencies(Dictionary<CurrenciesType, int> currencies)
        {
            this.currencies = currencies;
        }
        /// <summary>
        /// 是否延迟同步DB
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        public bool DelaySyncDb(CurrenciesType coinType)
        {
            switch (coinType)
            {
                case CurrenciesType.exp:
                case CurrenciesType.gold:
                case CurrenciesType.soulPower:
                case CurrenciesType.PasscardExp:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 同步延迟货币
        /// </summary>
        /// <param name="force"></param>
        public void SyncDbDelayCurrencies(bool force = false)
        {
            bool sync = false;
            if (currenciesChanged)
            {
                if (force || (BaseApi.now - lastSyncCurrenciesTime).TotalSeconds >= CONST.SYNC_CURRIENCIES_TIME)
                {
                    sync = true;
                }
            }
            if (sync)
            {
                // 同步db经验和金币
                Dictionary<CurrenciesType, int> currenciesList = new Dictionary<CurrenciesType, int>();
                currenciesList.Add(CurrenciesType.exp, GetCoins(CurrenciesType.exp));
                currenciesList.Add(CurrenciesType.gold, GetCoins(CurrenciesType.gold));
                currenciesList.Add(CurrenciesType.soulPower, GetCoins(CurrenciesType.soulPower));
                currenciesList.Add(CurrenciesType.PasscardExp, GetCoins(CurrenciesType.PasscardExp));
                SynchronizeCurrienciesToDB(currenciesList);

                currenciesChanged = false;
                lastSyncCurrenciesTime = BaseApi.now;
            }
        }

        public bool CheckCoins(Dictionary<int, int> costCoins)
        {
            int coinNum = 0;
            bool haveEnoughCoin = true;
            foreach (var kv in costCoins)
            {
                if (!this.currencies.TryGetValue((CurrenciesType)kv.Key, out coinNum))
                {
                    haveEnoughCoin = false;
                    break;
                }
                else
                {
                    if (coinNum < kv.Value)
                    {
                        haveEnoughCoin = false;
                        break;
                    }
                }
            }

            return haveEnoughCoin;
        }

        public bool CheckCoins(Dictionary<CurrenciesType, int> costCoins)
        {
            int coinNum = 0;
            bool haveEnoughCoin = true;
            foreach (var kv in costCoins)
            {
                if (!this.currencies.TryGetValue(kv.Key, out coinNum))
                {
                    haveEnoughCoin = false;
                    break;
                }
                else
                {
                    if (coinNum < kv.Value)
                    {
                        haveEnoughCoin = false;
                        break;
                    }
                }
            }

            return haveEnoughCoin;
        }


        public bool CheckCoins(CurrenciesType type, int value)
        {
            int coinNum = 0;
            if (!this.currencies.TryGetValue(type, out coinNum))
            {
                return false;
            }
            else
            {
                if (coinNum < value)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取货币数量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetCoins(CurrenciesType type)
        {
            int count = 0;
            currencies.TryGetValue(type, out count);
            return count;
        }
        public int GetCoins(int type)
        {
            return GetCoins((CurrenciesType)type);
        }

        /// <summary>
        /// 添加多组货币数量
        /// </summary>
        /// <param name="currencies"></param>
        /// <param name="way"></param>
        /// <param name="extraParam"></param>
        public void AddCoins(RewardManager rewards, RewardResult resulet, ObtainWay way, string extraParam = "")
        {
            Dictionary<int, int> currencies = rewards.GetRewardList(RewardType.Currencies);
            if (currencies != null)
            {
                foreach (var coin in currencies)
                {
                    CurrenciesType type = (CurrenciesType)coin.Key;

                    //超过携带上限存到资源仓库
                    int realAddNum = coin.Value;
                    int storeNum = 0;
                    if (CheckBeyondCarryMaxNum(type, coin.Value, out realAddNum, out storeNum))
                    {
                        AddWarehouseCoinWithoutSync(type, storeNum, way, extraParam);
                        resulet.WarehouseCurrencies.Add(coin.Key, storeNum);
                    }

                    // 经验单独处理
                    if (type == CurrenciesType.exp)
                    {
                        int oldLevel = Level;
                        int oldExp = GetCoins(CurrenciesType.exp);
                        AddExp(coin.Value, way, extraParam);
                        int newExp = GetCoins(CurrenciesType.exp);
                        //BI：经验
                        KomoeEventLogPlayereExp("exp", oldLevel, oldExp, newExp, coin.Value, 0, 0, way.ToString());
                        if (oldLevel != Level)
                        {
                            DateTime lastUp = LastLevelUpTime;
                            LastLevelUpTime = server.Now();
                            // sync db
                            server.GameDBPool.Call(new QueryUpdatePlayerLevel(uid, Level, LastLevelUpTime));

                            double costTime = (LastLevelUpTime - lastUp).TotalSeconds;
                            BIRecordLevelupLog("level", oldLevel, Level, (int)costTime);
                            //BI：经验
                            KomoeEventLogPlayereExp("level", oldLevel, oldExp, newExp, coin.Value, 0, (int)costTime, way.ToString());

                            SyncLevel(oldLevel);

                            //主角达到某个等级
                            AddTaskNumForType(TaskType.PlayerLevel, Level, false);
                            //等级触发福利邮件
                            AddWelfareTriggerItem(WelfareConditionType.Level, Level);
                            //活动：等级
                            //AddActivityNumForType(ActivityAction.PlayerLevel, Level);
                            ////成长基金
                            //AddActivityNumForType(ActivityAction.GrowthFund, Level);
                            //AddActivityNumForType(ActivityAction.GrowthFundEx, Level);
                        }
                        continue;
                    }
                    if (type == CurrenciesType.PasscardExp)
                    {
                        int realCoin = PassCardMng.AddExp(coin.Value);
                        AddCoinWithoutSync(type, realCoin, way, extraParam);
                        resulet.Currencies.Add(coin.Key, realCoin);
                        continue;
                    }

                    AddCoinWithoutSync(type, realAddNum, way, extraParam);
                    resulet.Currencies.Add(coin.Key, realAddNum);
                }

                if (resulet.Currencies.Count > 0)
                {
                    SynchronizeCurrienciesChange(resulet.Currencies);
                }

                if (resulet.WarehouseCurrencies.Count > 0)
                {
                    SynchronizeWarehouseCurrienciesChange(resulet.WarehouseCurrencies);
                }
            }
        }

        /// <summary>
        /// 添加货币数量
        /// </summary>
        /// <param name="type"></param>
        /// <param name="addCoins"></param>
        /// <param name="way"></param>
        /// <param name="extraParam"></param>
        public void AddCoins(CurrenciesType type, int addCoins, ObtainWay way, string extraParam = "")
        {
            if (addCoins > 0)
            {
                int realAddNum = addCoins;
                int storeNum = 0;
                if (CheckBeyondCarryMaxNum(type, addCoins, out realAddNum, out storeNum))
                {
                    AddWarehouseCoinWithoutSync(type, storeNum, way, extraParam);
                    SynchronizeWarehouseCurrienciesChange(type);
                }

                AddCoinWithoutSync(type, realAddNum, way, extraParam);
                SynchronizeCurrienciesChange(type);
            }
        }

        private bool AddCoinWithoutSync(CurrenciesType type, int addcount, ObtainWay way, string extraParam)
        {
            //当前没有达到上限，允许添加后达到上限的情况
            int original = 0;
            if (currencies.TryGetValue(type, out original))
            {
                if (currencies[type] < CurrenciesLibrary.GetMaxNum((int)type))
                {
                    currencies[type] = original + addcount;
                }
            }
            else
            {
                currencies[type] = addcount;
            }
            //RecordObtainLog(way, RewardType.Currencies, (int)type, original, addcount, extraParam);
            //货币获取埋点
            BIRecordObtainCurrency(addcount, currencies[type], type, way, extraParam);
            //BI 新增物品
            KomoeEventLogGoldFlow("add", "", (int)type, type.ToString(), (int)type, addcount, original, currencies[type], (int)way, 0, 0);
            return true;
        }
        /// <summary>
        /// 添加粮草
        /// </summary>
        /// <param name="rewards"></param>
        /// <param name="way"></param>
        /// <param name="extraParam"></param>
        public void AddGrain(RewardManager rewards, ObtainWay way, string extraParam = "")
        {
            Dictionary<int, int> grainList = rewards.GetRewardList(RewardType.Grain);
            if (grainList != null)
            {
                foreach (var grain in grainList)
                {
                    AddGrain(RewardType.Grain, grain.Value, way, extraParam);
                }
            }
        }

        public void AddGrain(RewardType type, int addCount, ObtainWay way, string extraParam = "")
        {
            int original = server.RelationServer.GetGrain(Camp);
            server.RelationServer.AddGrain((int)Camp, addCount);
            RecordObtainLog(way, RewardType.Grain, (int)type, original, addCount, extraParam);
            //获得超额粮草发公告
            if (GetCampBattleStep() == CampBattleStep.Final && addCount >= CampLibrary.GainExtraGrain)
            {
                BroadcastCampBattleGrain();
            }

            switch (way)
            {
                case ObtainWay.CampGather:
                    AddCampBattleRankScore(RankType.CampBattleCollection, addCount);
                    break;
                default:
                    break;
            }
        }

        private int GetAddValue(CurrenciesType type, int num)
        {
            return Math.Min(num, CurrenciesLibrary.GetMaxNum((int)type));
        }

        /// <summary>
        /// 消耗货币数量
        /// </summary>
        /// <param name="type"></param>
        /// <param name="delCoins"></param>
        /// <param name="way"></param>
        /// <param name="extraParam"></param>
        public bool DelCoins(CurrenciesType type, int delCoins, ConsumeWay way, string extraParam)
        {
            if (delCoins > 0)
            {
                if (DelCoinWithoutSync(type, delCoins, way, extraParam))
                {
                    SynchronizeCurrienciesChange(type);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 如果使用货币请使用delcoins方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="delCoins"></param>
        /// <param name="way"></param>
        /// <param name="extraParam"></param>
        /// <returns></returns>
        private bool DelCoinWithoutSync(CurrenciesType type, int delCoins, ConsumeWay way, string extraParam)
        {
            int original = 0;
            if (currencies.TryGetValue(type, out original))
            {
                if (original - delCoins < 0)
                {
                    return false;
                    //currencies[type] = 0;
                }
                else
                {
                    currencies[type] = original - delCoins;
                }

                CheckDelDiamond(type, delCoins, way);

                //RecordConsumeLog(way, RewardType.Currencies, (int)type, original, delCoins, extraParam);
                //货币消耗埋点
                BIRecordConsumeCurrency(delCoins, currencies[type], type, way, extraParam);
                //BI 货币
                KomoeEventLogGoldFlow("reduce", "", (int)type, type.ToString(), (int)type, delCoins, original, currencies[type], (int)way, 0, 0);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool DelCoinWithoutSyncWhenOverLimit(CurrenciesType type, int delCoins, ConsumeWay way, string extraParam)
        {
            int original = 0;
            if (currencies.TryGetValue(type, out original))
            {
                if (original - delCoins < 0)
                {
                    return false;
                    //currencies[type] = 0;
                }
                else
                {
                    currencies[type] = original - delCoins;
                }

                RecordConsumeLog(way, RewardType.Currencies, (int)type, original, delCoins, extraParam);
                //货币消耗埋点
                BIRecordConsumeCurrency(delCoins, currencies[type], type, way, extraParam);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DelCoins(Dictionary<CurrenciesType, int> costCoins, ConsumeWay way, string extraParam)
        {
            int original = 0;
            CurrenciesType type;
            List<CurrenciesType> syncList = new List<CurrenciesType>();

            if (!CheckCoins(costCoins))
            {
                return false;
            }
            foreach (var kv in costCoins)
            {
                type = kv.Key;
                if (this.currencies.TryGetValue(type, out original))
                {
                    if (original - kv.Value < 0)
                    {
                        currencies[type] = 0;
                    }
                    else
                    {
                        currencies[type] = original - kv.Value;
                    }
                    syncList.Add(type);

                    CheckDelDiamond(type, kv.Value, way);

                    //RecordConsumeLog(way, RewardType.Currencies, (int)type, original, kv.Value, extraParam);
                    //货币消耗埋点
                    BIRecordConsumeCurrency(kv.Value, currencies[type], type, way, extraParam);
                    //BI 货币
                    KomoeEventLogGoldFlow("reduce", "", (int)type, type.ToString(), (int)type, kv.Value, original, currencies[type], (int)way, 0, 0);
                }
            }

            if (syncList.Count > 0)
            {
                SynchronizeCurrienciesChange(syncList);
            }
            return true;
        }

        public bool DelCoins(Dictionary<int, int> costCoins, ConsumeWay way, string extraParam)
        {
            int original = 0;
            CurrenciesType type;
            List<CurrenciesType> syncList = new List<CurrenciesType>();

            if (!CheckCoins(costCoins))
            {
                return false;
            }
            foreach (var kv in costCoins)
            {
                type = (CurrenciesType)kv.Key;
                if (this.currencies.TryGetValue(type, out original))
                {
                    if (original - kv.Value < 0)
                    {
                        currencies[type] = 0;
                    }
                    else
                    {
                        currencies[type] = original - kv.Value;
                    }
                    syncList.Add(type);

                    CheckDelDiamond(type, kv.Value, way);

                    //RecordConsumeLog(way, RewardType.Currencies, (int)type, original, kv.Value, extraParam);
                    //货币消耗埋点
                    BIRecordConsumeCurrency(kv.Value, currencies[type], type, way, extraParam);
                    //BI 货币
                    KomoeEventLogGoldFlow("reduce", "", (int)type, type.ToString(), (int)type, kv.Value, original, currencies[type], (int)way, 0, 0);
                }
            }

            if (syncList.Count > 0)
            {
                SynchronizeCurrienciesChange(syncList);
            }
            return true;
        }

        private void CheckDelDiamond(CurrenciesType currenciesType, int num, ConsumeWay way)
        {
            if (currenciesType == CurrenciesType.diamond)
            {
                ActionManager.RecordActionAndCheck(ActionType.DailyDiamondCostCount, num);

                CheckAddDiamondRebateConsume(currenciesType, num, way);
            }
        }

        /// <summary>
        /// 增加经验
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="way"></param>
        public void AddExp(int exp, ObtainWay way, string extraParam)
        {
            int original = currencies[CurrenciesType.exp];
            var model = CharacterLibrary.GetCharacterLevelModel(Level);
            if (model == null)
            {
                return;
            }

            int curLevelExp = model.Exp;

            //if (Level == 1 && original == 0)
            //{
            //    //通知客户端做经验引导
            //    MSG_ZGC_FIRST_ADD_EXP msg = new MSG_ZGC_FIRST_ADD_EXP();
            //    msg.Exp = exp;
            //    Write(msg);
            //}

            if (original + exp < curLevelExp)
            {
                currencies[CurrenciesType.exp] = original + exp;
                //RecordObtainLog(way, RewardType.Currencies, (int)CurrenciesType.exp, original, exp);
                //货币获取埋点
                BIRecordObtainCurrency(exp, currencies[CurrenciesType.exp], CurrenciesType.exp, way, extraParam);
                //BI 新增物品
                KomoeEventLogGoldFlow("add", "", (int)CurrenciesType.exp, CurrenciesType.exp.ToString(), (int)CurrenciesType.exp, exp, original, currencies[CurrenciesType.exp], (int)way, 0, 0);
                // sync client exp 
                SynchronizeCurrienciesChange(CurrenciesType.exp);
            }
            else
            {
                // 升级条件达成
                var nextLevelModel = CharacterLibrary.GetCharacterLevelModel(Level + 1);
                if (nextLevelModel == null)
                {
                    // 已达最高等级 且经验已存满 什么都没有发生
                    if (original >= curLevelExp)
                    {
                        return;
                    }
                    else
                    {
                        // 当前经验存满 多余舍弃
                        if (original + exp >= curLevelExp)
                        {
                            exp = curLevelExp - original;
                        }
                        currencies[CurrenciesType.exp] = original + exp;
                        //RecordObtainLog(way, RewardType.Currencies, (int)CurrenciesType.exp, original, exp);
                        //货币获取埋点
                        BIRecordObtainCurrency(exp, currencies[CurrenciesType.exp], CurrenciesType.exp, way, extraParam);
                        //BI 新增物品
                        KomoeEventLogGoldFlow("add", "", (int)CurrenciesType.exp, CurrenciesType.exp.ToString(), (int)CurrenciesType.exp, exp, original, currencies[CurrenciesType.exp], (int)way, 0, 0);
                        // sync client exp 
                        SynchronizeCurrienciesChange(CurrenciesType.exp);
                        return;
                    }
                }
                else
                {
                    // 升级
                    int nextLevelExp = nextLevelModel.Exp;
                    Level++;
                    ////升级获得技能
                    //TriggerLevelOutputSkill(Level);
                    int levelUpNeedExp = curLevelExp - original;
                    int leftExp = exp - levelUpNeedExp;
                    if (leftExp >= nextLevelExp)
                    {
                        // 剩余经验足够升下一级
                        currencies[CurrenciesType.exp] = 0;
                        RecordObtainLog(way, RewardType.Currencies, (int)CurrenciesType.exp, 0, exp);
                        AddExp(leftExp, way, extraParam);
                    }
                    else
                    {
                        currencies[CurrenciesType.exp] = leftExp;
                        //RecordObtainLog(way, RewardType.Currencies, (int)CurrenciesType.exp, leftExp, exp);
                        //货币获取埋点
                        BIRecordObtainCurrency(exp, currencies[CurrenciesType.exp], CurrenciesType.exp, way, extraParam);
                        //BI 新增物品
                        KomoeEventLogGoldFlow("add", "", (int)CurrenciesType.exp, CurrenciesType.exp.ToString(), (int)CurrenciesType.exp, exp, original, currencies[CurrenciesType.exp], (int)way, 0, 0);

                        SynchronizeCurrienciesChange(CurrenciesType.exp);

                        SynchronizeCurrienciesChange(CurrenciesType.exp, true);
                    }

                    CheckLevelLimitOpen();

                    //检查开启新的主战阵容
                    CheckUnlockMainBattleQueue();
                }
            }
        }

        public bool CheckCoinUpperLimit(CurrenciesType type)
        {
            return GetCoins(type) >= CurrenciesLibrary.GetMaxNum((int)type);
        }

        public bool CheckLevelUp()
        {
            bool levelUp = false;
            //int current_level = CommonUtility.Calc.GetPCLevel(Packet_Info.jobID, (uint)this.GetExp());
            //if (current_level > Packet_Info.level)
            //{
            //    levelUp = true;
            //    Packet_Info.level = current_level;
            //    //MaxPVPExp = GameConfig.GetMaxPVPExp(current_level);
            //    //BindCost();

            //    // NOTE : 数据更新
            //    BindPacketInfo();
            //    BindStat();
            //    // NOTE : hp恢复
            //    //SetHP(Nature.PRO_MAX_HP);

            //    PKS_ZC_LEVELUP msg_levelup = new PKS_ZC_LEVELUP();
            //    msg_levelup.instance_id = Instance_id;
            //    msg_levelup.info = Packet_Info;
            //    msg_levelup.stat = Packet_Stat;
            //    msg_levelup.currencies = Packet_Currencies;
            //    Write(msg_levelup);

            //    PKS_ZC_LEVELUP_BROADCAST msg_levelup_broadcast = new PKS_ZC_LEVELUP_BROADCAST();
            //    msg_levelup_broadcast.info = new PKS_ZC_CHAR_SIMPLE_INFO();
            //    GetSimpleInfo(msg_levelup_broadcast.info);
            //    BroadCastNearby(msg_levelup_broadcast);

            //    //foreach (var shop in shopManager.ShopList)
            //    //{
            //    //    int shopId = shop.shop_type;
            //    //    ShopManagerModel shopData = GetShopManager(shopId);
            //    if (shopData != null && shopData.ShopRefeshType != (int)ShopRefreshType.Timing)
            //    //    {
            //    //        foreach (var group in shopData.ItemList)
            //    //        {
            //    //            if (group.StartLevel == Packet_Info.level)
            //    //            {
            //    //                Dictionary<int, int> returnList = ShopRefresh(shopId);
            //    //                shop.item_list.Clear();
            //    //                shop.isbuy.Clear();
            //    //                foreach (var item in returnList)
            //    //                {
            //    //                    shop.item_list.Add(item.Key);
            //    //                    shop.isbuy.Add(0);
            //    //                }
            //    //                server.DB.Call(new QueryUpdateShop(Uid, shop), DBIndex);
            //    //                break;
            //    //            }
            //    //        }
            //    //    }
            //    //}
            //}
            return levelUp;
        }



        //方法存在同步问题
        //public void DelCurrencies(Dictionary<int, int> currencies, ConsumeWay way, string extraParam)
        //{
        //    foreach (var coin in currencies)
        //    {
        //        switch (coin.Key)
        //        {
        //            default:
        //                DelCoins((CurrenciesType)coin.Key, coin.Value, way, extraParam);
        //                break;
        //        }
        //    }
        //}

        /// <summary>
        /// 同步单个货币变化
        /// </summary>
        public void SynchronizeCurrienciesChange(CurrenciesType type, bool forceSyncDb = false)
        {
            List<CurrenciesType> currencies = new List<CurrenciesType>();
            currencies.Add(type);
            SynchronizeCurrienciesChange(currencies, forceSyncDb);
        }

        public void SynchronizeCurrienciesChange(Dictionary<int, int> list, bool forceSyncDb = false)
        {
            List<CurrenciesType> currencies = new List<CurrenciesType>();
            foreach (var item in list)
            {
                currencies.Add((CurrenciesType)item.Key);
            }
            SynchronizeCurrienciesChange(currencies, forceSyncDb);
        }
        /// <summary>
        /// 同步多种货币变化
        /// </summary>
        /// <param name="keyList"></param>
        public void SynchronizeCurrienciesChange(List<CurrenciesType> keyList, bool forceSyncDb = false)
        {
            SynchronizeCurrienciesToClient(keyList);
            SynchronizeCurrienciesToDB(keyList, forceSyncDb);
            //SynchronizeExpToRedis(currencies); //frTODO:这里目前只同步exp到redis ，以后也许会同步别的货币类型到redis
        }

        /// <summary>
        /// 同步货币变化到客户端
        /// </summary>
        public void SynchronizeCurrienciesToClient(List<CurrenciesType> keyList)
        {
            MSG_ZGC_SYNC_CURRENCIES msg = UpdateCurrencies(keyList);
            Write(msg);
        }
        public void SyncLevel(int oldLevel)
        {

            server.GameRedis.Call(new OperateUpdateLevel(uid, Level));

            // sync client
            MSG_ZGC_PLAYER_LEVEL notify = new MSG_ZGC_PLAYER_LEVEL();
            notify.Level = Level;
            notify.OldLevel = oldLevel;
            Write(notify);

            //通知Relation
            MSG_ZR_LEVEL_UP notifyRelation = new MSG_ZR_LEVEL_UP();
            notifyRelation.Uid = uid;
            notifyRelation.Level = Level;
            notifyRelation.Chapter = ChapterId;
            notifyRelation.Research = HuntingManager.Research;
            server.SendToRelation(notifyRelation, Uid);
        }



        /// <summary>
        /// 生成货币更新流
        /// </summary>
        /// <param name="keyList">要更新的货币</param>
        /// <returns>货币流</returns>
        public MSG_ZGC_SYNC_CURRENCIES UpdateCurrencies(List<CurrenciesType> keyList)
        {
            MSG_ZGC_SYNC_CURRENCIES msg = new MSG_ZGC_SYNC_CURRENCIES();
            foreach (var key in keyList)
            {
                CURRENCY currency = new CURRENCY();
                currency.CurrencyType = (int)key;
                currency.CurrencyCount = GetCoins(key);
                msg.Currencies.Add(currency);
            }
            return msg;
        }

        /// <summary>
        /// 获取人物所有货币值
        /// </summary>
        /// <returns></returns>
        public MSG_ZGC_SYNC_CURRENCIES GetCurrenciesMsg()
        {
            MSG_ZGC_SYNC_CURRENCIES msg = new MSG_ZGC_SYNC_CURRENCIES();

            foreach (var item in Currencies)
            {
                CURRENCY currency = new CURRENCY();
                currency.CurrencyType = (int)item.Key;
                currency.CurrencyCount = item.Value;
                msg.Currencies.Add(currency);
            }
            return msg;
        }

        /// <summary>
        /// 获取跨zone人物货币信息
        /// </summary>
        /// <returns></returns>
        public MSG_ZMZ_CURRENCIES_INFO GetCurrenciesTransform()
        {
            MSG_ZMZ_CURRENCIES_INFO msg = new MSG_ZMZ_CURRENCIES_INFO();
            msg.Uid = Uid;
            foreach (var item in Currencies)
            {
                ZMZ_CURRENCY currency = new ZMZ_CURRENCY();
                currency.CurrencyType = (int)item.Key;
                currency.CurrencyCount = item.Value;
                msg.Currencies.Add(currency);
            }
            msg.CurrenciesChanged = currenciesChanged;
            return msg;
        }

        /// <summary>
        /// 跨zone加载人物货币信息
        /// </summary>
        /// <param name="currencies"></param>
        public void LoadCurrenciesTransform(RepeatedField<ZMZ_CURRENCY> currencies, bool currenciesChanged)
        {
            foreach (var it in currencies)
            {
                Currencies[(CurrenciesType)it.CurrencyType] = it.CurrencyCount;
            }
            this.currenciesChanged = currenciesChanged;
        }
        ///// <summary>
        ///// 同步Exp变化到Redis
        ///// </summary>
        //public void SynchronizeExpToRedis(List<int> currencyTypes)
        //{
        //    Dictionary<int, int> currenciesList = new Dictionary<int, int>();
        //    foreach (var item in currencyTypes)
        //    {
        //        if (item == (int)CurrenciesType.exp)
        //        {
        //            int coins = GetCoins(item);
        //            currenciesList.Add(item, coins);
        //            server.Redis.Call(new OperateUpdateExp(uid, coins));
        //        }
        //    }
        //}

        /// <summary>
        /// 同步货币变化到DB
        /// </summary>
        public void SynchronizeCurrienciesToDB(List<CurrenciesType> currencyTypes, bool forceSync)
        {
            Dictionary<CurrenciesType, int> currenciesList = new Dictionary<CurrenciesType, int>();
            foreach (var currenciesType in currencyTypes)
            {
                // 延迟同步的货币不立即同步db
                if (!forceSync && DelaySyncDb(currenciesType))
                {
                    currenciesChanged = true;
                    continue;
                }
                else
                {
                    currenciesList.Add(currenciesType, GetCoins(currenciesType));
                }
            }
            //DB
            SynchronizeCurrienciesToDB(currenciesList);
        }

        /// <summary>
        /// 同步货币变化到DB
        /// </summary>
        private void SynchronizeCurrienciesToDB(Dictionary<CurrenciesType, int> currenciesList)
        {
            if (currenciesList.Count > 0)
            {
                string updateSql = CurrenciesLibrary.GetUpdateSql(currenciesList, Uid);
                if (!string.IsNullOrEmpty(updateSql))
                {
                    server.GameDBPool.Call(new QueryUpdateCurrencies(updateSql));
                }
            }
        }
    }
}