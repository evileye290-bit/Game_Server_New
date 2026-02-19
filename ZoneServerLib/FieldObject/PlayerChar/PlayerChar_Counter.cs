using DataProperty;
using ServerShared;
using System;
using System.Collections.Generic;
using CommonUtility;
using System.Linq;
using EnumerateUtility;
using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using DBUtility.Sql;
using EnumerateUtility.Timing;
using Message.Zone.Protocol.ZM;
using Google.Protobuf.Collections;
using ServerModels;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //计数器
        private DateTime lastSyncCounterTime = ZoneServerApi.now;
        private Dictionary<CounterType, Counter> counterList = new Dictionary<CounterType, Counter>();
        private Dictionary<CounterType, DateTime> recoryNextTimes = new Dictionary<CounterType, DateTime>();
        private Dictionary<CounterType, bool> updateCounterList = new Dictionary<CounterType, bool>();
        public void BindCounterList(Dictionary<CounterType, Counter> counterList, Dictionary<CounterType, int> timeList)
        {
            this.counterList = counterList;

            bool saveDb = false;
            foreach (var type in CounterLibrary.CountParamList)
            {
                Counter counter = GetCounter(type);
                if (counter != null)
                {
                    int time;
                    if (timeList.TryGetValue(type, out time))
                    {
                        //说明是特殊刷新
                        if (CaculateCounterOfflineAddNum(counter, time))
                        {
                            saveDb = true;
                        }
                        //else
                        //{
                        //    ClearCounterRecoryTime(counter);
                        //}
                    }
                    else
                    {
                        CounterModel model = CounterLibrary.GetCounterModel(type);
                        //特殊刷新计数器
                        if (model != null && model.MaxCount > counter.Count)
                        {
                            //开启计时
                            if (CounterStartRefresh(counter, model))
                            {
                                saveDb = true;
                            }
                        }
                    }
                }
            }

            if (saveDb)
            {
                //保存DB
                SyncCounterTimeDB();
            } 
        }

        Dictionary<CounterType, DateTime> counterTempLList = new Dictionary<CounterType, DateTime>();
        public void UpdateCounterAddNum()
        {
            foreach (var kv in recoryNextTimes)
            {
                if (kv.Value <= ZoneServerApi.now)
                {
                    counterTempLList.Add(kv.Key, kv.Value);
                }
            }

            if (counterTempLList.Count > 0)
            {
                foreach (var kv in counterTempLList)
                {
                    Counter counter = GetCounter(kv.Key);
                    if (counter != null)
                    {
                        CounterModel model = CounterLibrary.GetCounterModel(counter.Type);
                        int revertTime = model.GetParamIntValue(CounterParamType.TIME);
                        int revertNum = model.GetParamIntValue(CounterParamType.NUM);

                        AddCounterByTimeRecory(counter, model, revertNum);
                        //if (counter.Count >= model.MaxCount)
                        //{
                        //    //达到最大上限移除时间
                        //    ClearCounterRecoryTime(counter);
                        //}
                        //else
                        //{
                        //设置刷新时间
                        SetCounterRecoryTime(counter, ZoneServerApi.now, revertTime);
                        //}

                        //保存DB
                        SyncCounterTimeDB();
                        //通知客户端
                        SyncChangeCounterMsg(counter);
                    }
                }
                counterTempLList.Clear();
            }
        }

        private bool CaculateCounterOfflineAddNum(Counter counter, int recoryLastTime)
        {
            CounterModel model = CounterLibrary.GetCounterModel(counter.Type);
            if (model == null)
            {
                Log.Warn("player {0} CaculateOfflineNeedAddNum counter type {1} error: not find", Uid, counter.Type);
                return false;
            }

            if (!model.CheckParamKey(CounterParamType.TIME) || !model.CheckParamKey(CounterParamType.NUM))
            {
                Log.Warn("player {0} CaculateOfflineNeedAddNum counter type {1} error: not find time or num", Uid, counter.Type);
                return false;
            }

            if (model.MaxCount <= counter.Count)
            {
                return false;
            }

            int revertTime = model.GetParamIntValue(CounterParamType.TIME);
            int revertNum = model.GetParamIntValue(CounterParamType.NUM);

            if (recoryLastTime > 0)
            {
                DateTime lastTime = Timestamp.TimeStampToDateTime(recoryLastTime);
                TimeSpan passTime = ZoneServerApi.now - lastTime;

                if (passTime.TotalSeconds / revertTime * revertNum > model.MaxCount)
                {
                    //数量达到最大上限
                    AddCounterByTimeRecory(counter, model, model.MaxCount);
                    ////达到最大上限移除时间
                    //ClearCounterRecoryTime(counter);
                    //设置刷新时间
                    SetCounterRecoryTime(counter, ZoneServerApi.now, revertTime);
                }
                else
                {
                    //数量未达到最大上限
                    if (passTime.TotalSeconds > revertTime)
                    {
                        int recoryCount = (int)(passTime.TotalSeconds / revertTime) * revertNum;//可以回复的次数
                        double restTime = passTime.TotalSeconds % revertTime;

                        AddCounterByTimeRecory(counter, model, recoryCount);
                        //if (counter.Count >= model.MaxCount)
                        //{
                        //    //达到最大上限移除时间
                        //    ClearCounterRecoryTime(counter);
                        //}
                        //else
                        {
                            //设置刷新时间
                            SetCounterRecoryTime(counter, ZoneServerApi.now.AddSeconds(restTime * -1), revertTime);
                        }
                    }
                    else
                    {
                        //离线时间不到回复一次的
                        SetCounterRecoryTime(counter, lastTime, revertTime);
                    }
                }
            }
            else
            {
                SetCounterRecoryTime(counter, ZoneServerApi.now, revertTime);
            }
            return true;
        }

        private bool CounterStartRefresh(Counter counter, CounterModel model)
        {
            if (!model.CheckParamKey(CounterParamType.TIME))
            {
                //Log.Warn("player {0} CounterStartRefresh counter type {1} error: not find time or num", Uid, counter.Type);
                return false; 
            }

            //if (model.MaxCount <= counter.Count)
            //{
            //    return false;
            //}

            int revertTime = model.GetParamIntValue(CounterParamType.TIME);
            SetCounterRecoryTime(counter, ZoneServerApi.now, revertTime);

            ////保存DB
            //SyncCounterTimeDB();
            return true;
        }

        private void SetCounterRecoryTime(Counter counter, DateTime time, int revertTime)
        {
            counter.RecoryLastTime = Timestamp.GetUnixTimeStampSeconds(time);
            recoryNextTimes[counter.Type] = time.AddSeconds(revertTime);
        }

        //private void ClearCounterRecoryTime(Counter counter)
        //{
        //    counter.RecoryLastTime = 0;
        //    recoryNextTimes.Remove(counter.Type);
        //}

        public DateTime GetRecoryTime(CounterType counterType)
        {
            DateTime time = server.Now();
            recoryNextTimes.TryGetValue(counterType, out time);
            return time;
        }

        private void AddCounterByTimeRecory(Counter counter, CounterModel model, int num)
        {
            if (counter.Count >= model.MaxCount)
            {
                return;
            }
            else if (counter.Count + num > model.MaxCount)
            {
                num = model.MaxCount - counter.Count;
            }

            counter = UpdateCounter(counter.Type, num, false);
        }

        private void SyncCounterTimeDB()
        {
            string timeString = string.Empty;
            foreach (var kv in recoryNextTimes)
            {
                Counter counter = GetCounter(kv.Key);
                if (counter != null)
                {
                    timeString += string.Format("{0}:{1}|", (int)counter.Type, counter.RecoryLastTime);
                }
            }
            server.GameDBPool.Call(new QueryUpdateCounterTime(Uid, timeString));
        }

        public Counter UpdateCounter(CounterType type, int count, bool needSync = true)
        {
            if (type != CounterType.None)
            {
                Counter counter = GetCounter(type);
                if (counter != null)
                {
                    counter.Count += count;
                }
                else
                {
                    counter = InitCounter(type, count);
                }
                //counter.Changed = true;
                updateCounterList[type] = true;

                if (needSync)
                {
                    SyncChangeCounterMsg(counter, count);
                }
               
                return counter;
            }
            else
            {
                Log.Warn("player {0} update counter type {1} count {2} error: {3}", Uid, type, count, new Exception());
                return null;
            }
        }

        private void SyncChangeCounterMsg(Counter counter, int count)
        {
            if (CounterLibrary.CountParamList.Contains(counter.Type))
            {
                CounterModel model = CounterLibrary.GetCounterModel(counter.Type);
                //特殊刷新计数器
                if (model != null)
                {
                    if (count < 0)
                    {
                        //说明是扣除次数
                        if (!recoryNextTimes.ContainsKey(counter.Type))
                        {
                            //开启计时
                            if (CounterStartRefresh(counter, model))
                            {
                                SyncCounterTimeDB();
                            }
                        }
                        else
                        {
                            //count < 0 然后用当前次数加扣除次数 最大比较，
                            if (counter.Count - count >= model.MaxCount)
                            {
                                //从新计算
                                if (CounterStartRefresh(counter, model))
                                {
                                    SyncCounterTimeDB();
                                }
                            }
                        }
                    }
                    //if (!recoryNextTimes.ContainsKey(type))
                    //{
                    //    //开启计时
                    //    if (CounterStartRefresh(counter, model))
                    //    {
                    //        SyncCounterTimeDB();
                    //    }
                    //}
                    //else
                    //{
                    //    int revertTime = model.GetParamIntValue(CounterParamType.TIME);
                    //    //设置刷新时间
                    //    SetCounterRecoryTime(counter, ZoneServerApi.now, revertTime);
                    //}
                }
                //else
                //{
                //    ClearCounterRecoryTime(counter);
                //    SyncCounterTimeDB();
                //}
            }


            SyncChangeCounterMsg(counter);
        }

        public void SetCounter(CounterType type, int count, bool needSync = true)
        {
            Counter counter = GetCounter(type);
            if (counter != null)
            {
                counter.Count = count;
            }
            else
            {
                counter = InitCounter(type, count);
            }
            //counter.Changed = true;
            updateCounterList[type] = true;

            if (needSync)
            {
                SyncChangeCounterMsg(counter, count);
            }
        }

        public Counter InitCounter(CounterType type, int count, int lastTime)
        {
            Counter counter = InitCounter(type, count);
            if (lastTime > 0)
            {
                CounterModel model = CounterLibrary.GetCounterModel(counter.Type);
                int revertTime = model.GetParamIntValue(CounterParamType.TIME);
                DateTime recoryLastTime = Timestamp.TimeStampToDateTime(lastTime);
                SetCounterRecoryTime(counter, recoryLastTime, revertTime);
            }
            return counter;
        }

        public Counter InitCounter(CounterType type, int count)
        {
            Counter counter = new Counter(type, count);
            counterList.Add(type, counter);
            return counter;
        }

        public int GetCounterValue(CounterType type)
        {
            Counter counter = GetCounter(type);
            if (counter != null)
            {
                return counter.Count;
            }
            return 0;
        }

        public Counter GetCounter(CounterType type)
        {
            Counter counter = null;
            counterList.TryGetValue(type, out counter);
            return counter;
        }


        /// <summary>
        /// 添加多组计数
        /// </summary>
        /// <param name="currencies"></param>
        /// <param name="way"></param>
        /// <param name="extraParam"></param>
        public void AddCounters(RewardManager rewards, RewardResult resulet, ObtainWay way, string extraParam = "")
        {
            Dictionary<int, int> counters = rewards.GetRewardList(RewardType.Counter);
            if (counters != null)
            {
                foreach (var coin in counters)
                {
                    CounterType type = (CounterType)coin.Key;

                    Counter counter = UpdateCounter(type, coin.Value, false);
                    if (counter != null)
                    {
                        resulet.Counters.Add((int)type, counter);
                    }
                }

                if (resulet.Counters.Count > 0)
                {
                    SyncChangeCounterMsg(resulet.Counters);
                }
            }
        }

        /// <summary>
        /// 同步计数器变化到DB
        /// </summary>
        public void SyncChangedCounterToDB(List<Counter> counters)
        {
            if (counters.Count > 0)
            {
                //CounterSql counterSql = new CounterSql(uid);
                //string counterTableName = "game_counter";
                string updateSql = CounterLibrary.GetUpdateSql(counters, Uid);
                if (!string.IsNullOrEmpty(updateSql))
                {
                    QueryUpdateCounters queryCounter = new QueryUpdateCounters(updateSql);
                    server.GameDBPool.Call(queryCounter);
                }
                //foreach (var item in counters)
                //{
                //    item.Changed = false;
                //}
            }
        }

        public void SyncDbDelayCounters(bool force)
        {
            //if ((ZoneServerApi.now - lastSyncCurrenciesTime).TotalSeconds >= ServerShared.CONST.SYNC_COUNTER_TIME || force)
            if (updateCounterList.Count > 0)
            {
                List<Counter> counters = new List<Counter>();
                //foreach (var item in counterList)
                //{
                //    if (item.Value.Changed)
                //    {
                //        counters.Add(item.Value);
                //    }
                //}
                foreach (var type in updateCounterList)
                {
                    Counter counter = GetCounter(type.Key);
                    if (counter != null)
                    {
                        counters.Add(counter);
                    }
                }
                SyncChangedCounterToDB(counters);
                //lastSyncCounterTime = ZoneServerApi.now;
                updateCounterList.Clear();
            }
        }

        public bool CheckCounter(CounterType type)
        {
            bool gotMax = false;
            Counter counter = GetCounter(type);
            if (counter != null)
            {
                Data counterData = DataListManager.inst.GetData("Counter", (int)type);
                if (counterData != null && counter.Count >= counterData.GetInt("MaxCount"))
                {
                    gotMax = true;
                }
            }
            else
            {
                gotMax = true;
            }
            return gotMax;
        }

        public void CheckCounterRefresh(TimingType refresh_type)
        {
            bool isRefresh = false;
            List<CounterType> list = CounterLibrary.GetRefreshCounter(refresh_type);
            if (list != null)
            {
                Counter counter = null;
                foreach (var type in list)
                {
                    counter = GetCounter(type);
                    if (counter != null)
                    {
                        if (counter.Count != 0)
                        {
                            counter.Count = 0;
                            //counter.Changed = true;
                            updateCounterList[type] = true;
                            isRefresh = true;
                        }
                    }
                }
            }

            if (isRefresh)
            {
                Write(GetCounterMsg());
            }
        }

        public void CheckCounterRefresh1(TimingType refresh_type)
        {
            bool isRefresh = false;
            List<CounterType> list = CounterLibrary.GetRefreshCounter(refresh_type);
            if (list != null)
            {
                Counter counter = null;
                foreach (var type in list)
                {
                    counter = GetCounter(type);
                    if (counter != null)
                    {
                        int count = CounterLibrary.GetMaxCount(type);
                        if (counter.Count < count)
                        {
                            counter.Count = count;
                            //counter.Changed = true;
                            updateCounterList[type] = true;
                            isRefresh = true;
                        }
                    }
                }
            }

            if (isRefresh)
            {
                Write(GetCounterMsg());
            }
        }

        private int GetCounterRecoryNextTime(CounterType type)
        {
            int time = 0;
            DateTime nextTime;
            if (recoryNextTimes.TryGetValue(type, out nextTime))
            {
                time = Timestamp.GetUnixTimeStampSeconds(nextTime);
            }
            return time;
        }

        public MSG_ZMZ_COUNTER_INFO GetCounterTransform()
        {
            MSG_ZMZ_COUNTER_INFO info = new MSG_ZMZ_COUNTER_INFO();
            info.Uid = Uid;
            foreach (var item in counterList)
            {
                ZMZ_COUNTER counter = new ZMZ_COUNTER();
                counter.CounterType = (int)item.Key;
                counter.CounterCount = item.Value.Count;
                counter.LastTime = item.Value.RecoryLastTime;
                info.Counters.Add(counter);
            }
            return info;
        }

        public MSG_ZGC_COUNTER_INFO GetCounterMsg()
        {
            MSG_ZGC_COUNTER_INFO info = new MSG_ZGC_COUNTER_INFO();
            foreach (var item in counterList)
            {
                bool needSync = CounterLibrary.CheckNeedSync(item.Key);
                if (needSync)
                {
                    ZGC_COUNTER counter = new ZGC_COUNTER();
                    counter.CounterType = (int)item.Key;
                    counter.CounterCount = item.Value.Count;
                    counter.NextTime = GetCounterRecoryNextTime(item.Key);
                    info.Counters.Add(counter);
                }
            }
            return info;
        }

        public void SyncChangeCounterMsg(CounterType type)
        {
            Counter counter = GetCounter(type);
            if (counter != null)
            {
                SyncChangeCounterMsg(counter);
            }
        }

        public void SyncChangeCounterMsg(Counter counter)
        {
            bool needSync = CounterLibrary.CheckNeedSync(counter.Type);
            if (needSync)
            {
                MSG_ZGC_COUNTER_INFO info = new MSG_ZGC_COUNTER_INFO();
                ZGC_COUNTER item = new ZGC_COUNTER();
                item.CounterType = (int)counter.Type;
                item.CounterCount = counter.Count;
                item.NextTime = GetCounterRecoryNextTime(counter.Type);
                info.Counters.Add(item);
                Write(info);
            }
        }

        public void SyncChangeCounterMsg(Dictionary<int, Counter> counters)
        {
            if (counters.Count > 0)
            {
                MSG_ZGC_COUNTER_INFO info = new MSG_ZGC_COUNTER_INFO();
                foreach (var counter in counters)
                {
                    bool needSync = CounterLibrary.CheckNeedSync(counter.Value.Type);
                    if (needSync)
                    {
                        ZGC_COUNTER item = new ZGC_COUNTER();
                        item.CounterType = (int)counter.Value.Type;
                        item.CounterCount = counter.Value.Count;
                        item.NextTime = GetCounterRecoryNextTime(counter.Value.Type);
                        info.Counters.Add(item);
                    }
                }
                Write(info);
            }
        }

        public void LoadCounterTransform(RepeatedField<ZMZ_COUNTER> list)
        {
            foreach (var item in list)
            {
                InitCounter((CounterType)item.CounterType, item.CounterCount, item.LastTime);
            }
        }

        public void BuyCounterCount(int counterType, int count)
        {
            MSG_ZGC_COUNTER_BUY_COUNT response = new MSG_ZGC_COUNTER_BUY_COUNT();
            response.CounterType = counterType;

            Data buyData = DataListManager.inst.GetData("Counter", (int)counterType);
            if (buyData == null)
            {
                Log.Warn($"player {Uid} buy counter {counterType} count failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //最低一次
            if (count <= 0)
                count = 1;

            CounterType cbType = (CounterType)counterType;

            int buyedCount = GetCounterValue(cbType);
            if (buyedCount + count > buyData.GetInt("MaxCount"))
            {
                Log.Warn($"player {Uid} buy counter {counterType} count failed: over maxCount");
                response.Result = (int)ErrorCode.MaxBuyCount;
                Write(response);
                return;
            }

            string costStr = buyData.GetString("Price");
            if (string.IsNullOrEmpty(costStr))
            {
                Log.Warn($"player {Uid} buy counter {counterType} count failed: not find price");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int costCoin = 0;
            for (int i = 1; i <= count;  i++)
            {
                costCoin += CounterLibrary.GetBuyCountCost(costStr, buyedCount + i);
            }

            if (!CheckCoins(CurrenciesType.diamond, costCoin))
            {
                Log.Warn($"player {Uid} buy counter {counterType} count failed: coins not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {costCoin}");
                response.Result = (int)ErrorCode.DiamondNotEnough;
                Write(response);
                return;
            }

            ConsumeWay way = ConsumeWay.BuyCount;

            switch (cbType)
            {
                case CounterType.IntegralBossBuy:
                    way = ConsumeWay.IntegralBossBuy;
                    break;
                case CounterType.HuntingBuy:
                    //狩猎魂兽特殊处理
                    UpdateCounter(CounterType.HuntingCount, count);
                    way = ConsumeWay.Hunting;
                    break;
                case CounterType.BenefitsExpBuy:
                    //特殊处理
                    UpdateCounter(CounterType.BenefitsExp, count);
                    way = ConsumeWay.BenefitsExp;
                    break;
                case CounterType.BenefitsGoldBuy:
                    //特殊处理
                    UpdateCounter(CounterType.BenefitsGold, count);
                    way = ConsumeWay.BenefitsGold;
                    break;
                case CounterType.BenefitsSoulBreathBuy:
                    //特殊处理
                    UpdateCounter(CounterType.BenefitsSoulBreath, count);
                    way = ConsumeWay.BenefitsSoulBreath;
                    break;
                case CounterType.BenefitsSoulPowerBuy:
                    //特殊处理
                    UpdateCounter(CounterType.BenefitsSoulPower, count);
                    way = ConsumeWay.BenefitsSoulPower;
                    break;
                case CounterType.CampBuildBuyDiceCount:
                    CampBuild.UpdateCampBuildDiceCount(count);
                    way = ConsumeWay.CampBuildBuyDiceCount;
                    break;
                case CounterType.ActionBuyCount:
                    UpdateActionCount(count);
                    break;
                case CounterType.BuyCampBattleNattureCount:
                    UpdateBattleFortNature(count+ buyedCount);
                    way = ConsumeWay.CampFortAddNature;
                    break;
                case CounterType.SecretAreaSweepCountBuy:
                    way = ConsumeWay.SecretAreaSweepCountBuy;
                    break;
                case CounterType.CrossBattleBuyCount:
                    UpdateCounter(CounterType.CrossBattleCount, count);
                    way = ConsumeWay.CrossBattleBuyCount;
                    break;
                case CounterType.ThemeBossBuyCount:
                    way = ConsumeWay.ThemeBossBuyCount;
                    break;
                case CounterType.CrossBossActionBuyCount:
                    //UpdateCounter(CounterType.CrossBossActionCount, count);
                    way = ConsumeWay.CrossBossBuyActionCount;
                    break;
                case CounterType.CrossChallengeBuyCount:
                    UpdateCounter(CounterType.CrossChallengeCount, count);
                    way = ConsumeWay.CrossChallengeBuyCount;
                    break;
                case CounterType.SpaceTimeTowerBuyCount:
                {
                    UpdateCounter(CounterType.SpaceTimeTowerCount, count);
                    way = ConsumeWay.SpaceTimeTowerBuy;
                    break;
                }
                default:
                    break;
            }

            DelCoins(CurrenciesType.diamond, costCoin, way, count.ToString());

            UpdateCounter(cbType, count);

            //komoelog
            KomoeLogRecordBuyCounterByType(cbType, CurrenciesType.diamond, costCoin);        

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }



        //获取副本剩余挑战次数
        public int GetDungeonChallengeRestCount(MapType type)
        {
            MapCounterModel model = CounterLibrary.GetCounterType(type);
            return GetCounterRestCount(model.Counter, model.CounterBuy);
        }

        public int GetCounterRestCount(CounterType counter, CounterType buyCounter)
        {
            int count = GetCounterValue(counter);
            int buyedCount = GetCounterValue(buyCounter);
            int maxCount = CounterLibrary.GetMaxCount(counter);
            return Math.Max(0, maxCount + buyedCount - count);
        }

        public void UpdateCounter(MapType type, int count)
        {
            MapCounterModel model = CounterLibrary.GetCounterType(type);
            UpdateCounter(model.Counter, count);
        }

        /// <summary>
        /// 获取非计数器维护的次数
        /// </summary>
        public void GetSpecialCount()
        {
            MSG_ZGC_GET_SPECIAL_COUNT response = new MSG_ZGC_GET_SPECIAL_COUNT();
            //委派事件剩余次数
            //response.DelegationCount = DelegationMng.GetRestDelegateCount(GetCounterValue(CounterType.DelegateCount), GetCounterValue(CounterType.DelegateBuyCount));
            Write(response);
        }

        private void UpdateActionCount(int count)
        {
            var num = CampBattleLibrary.GetBuyOneActionCount();
            UpdateCounter(CounterType.ActionCount, count* num);
        }

        private void KomoeLogRecordBuyCounterByType(CounterType cbType, CurrenciesType coinType, int costNum)
        {
            long totalPower;
            List<Dictionary<string, object>> heroPosPower = null;
            switch (cbType)
            {
                case CounterType.CrossBattleBuyCount:
                    heroPosPower = ParseMultiQueueHeroPosPowerList(MapType.CrossBattle, HeroMng.CrossQueue, out totalPower);
                    break;
                case CounterType.CrossBossActionBuyCount:
                    heroPosPower = ParseMultiQueueHeroPosPowerList(MapType.CrossBoss, HeroMng.CrossBossQueue, out totalPower);
                    break;
                case CounterType.ThemeBossBuyCount:
                    heroPosPower = ParseMultiQueueHeroPosPowerList(MapType.ThemeBoss, HeroMng.ThemeBossQueue, out totalPower);
                    break;
                default:
                    heroPosPower = ParseMainHeroPosPowerList(HeroMng.GetHeroPos(), out totalPower);
                    break;
            }
            List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, (int)coinType, costNum);
            switch (cbType)
            {
                case CounterType.ChallengeCountBuy:
                    KomoeEventLogPvpFight(1, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.ChallengeCount, CounterType.ChallengeCountBuy), HeroMng.CalcBattlePower(), 0, 1, ArenaMng.Rank, ArenaMng.Rank, ArenaMng.Level.ToString(), ArenaMng.Level.ToString(), 0, 0, null);
                    break;
                case CounterType.CrossBattleBuyCount:
                    KomoeEventLogPvpFight(2, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.CrossBattleCount, CounterType.CrossBattleBuyCount), HeroMng.CalcBattlePower(), 0, 1, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Star.ToString(), CrossInfoMng.Info.Star.ToString(), 0, 0, null);
                    break;
                case CounterType.SecretAreaSweepCountBuy:
                    KomoeEventLogPveFight(0, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.SecretAreaSweepCount, CounterType.SecretAreaSweepCountBuy), HeroMng.CalcBattlePower(), 0, 0, 1, 0, GetTeamDetail(), consume, null);
                    break;
                case CounterType.IntegralBossBuy:
                    KomoeEventLogPveFight(1, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.IntegralBoss, CounterType.IntegralBossBuy), HeroMng.CalcBattlePower(), 0, 0, 1, 0, GetTeamDetail(), consume, null);
                    break;
                case CounterType.HuntingBuy:
                    KomoeEventLogPveFight(2, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.HuntingCount, CounterType.HuntingBuy), HeroMng.CalcBattlePower(), 0, 0, 1, 0, GetTeamDetail(), consume, null);
                    break;
                case CounterType.BenefitsSoulBreathBuy:
                    KomoeEventLogPveFight(3, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.BenefitsSoulBreath, CounterType.BenefitsSoulBreathBuy), HeroMng.CalcBattlePower(), 0, 0, 1, 0, GetTeamDetail(), consume, null);
                    break;
                case CounterType.BenefitsSoulPowerBuy:
                    KomoeEventLogPveFight(4, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.BenefitsSoulPower, CounterType.BenefitsSoulPowerBuy), HeroMng.CalcBattlePower(), 0, 0, 1, 0, GetTeamDetail(), consume, null);
                    break;
                case CounterType.CrossBossActionBuyCount:
                    KomoeEventLogPveFight(6, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.CrossBossActionCount, CounterType.CrossBossActionBuyCount), HeroMng.CalcBattlePower(), 0, 0, 1, 0, GetTeamDetail(), consume, null);
                    break;
                case CounterType.OnhookBuyCount:
                    KomoeEventLogPveFight(7, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.OnhookCount, CounterType.OnhookBuyCount), HeroMng.CalcBattlePower(), 0, 0, 1, 0, GetTeamDetail(), consume, null);
                    break;
                case CounterType.ThemeBossBuyCount:
                    KomoeEventLogPveFight(9, "", "", 3, heroPosPower, GetCounterRestCount(CounterType.ThemeBossCount, CounterType.ThemeBossBuyCount), HeroMng.CalcBattlePower(), 0, 0, 1, 0, GetTeamDetail(), consume, null);
                    break;
                case CounterType.CampBuildBuyDiceCount:
                    KomoeEventLogCampBuild(((int)Camp).ToString(), Camp.ToString(), 0, 3, 0, CampBuild.StepCount, CampBuild.StepCount, 0, CampBuild.FlagCount, CampBuild.FlagCount, CampBuild.DoubleLeftCount, consume, null);
                    break;
                case CounterType.BuyCampBattleNattureCount:
                    KomoeEventLogCampBattle(((int)Camp).ToString(), Camp.ToString(), 0, 1, HeroMng.CalcBattlePower(), 0, 0, "", "", "", 0, consume);
                    break;
                default:
                    break;
            }
        }
    }
}
