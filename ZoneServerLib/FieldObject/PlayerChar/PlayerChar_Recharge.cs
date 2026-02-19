using CommonUtility;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerModels.Recharge;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using ZoneServerLib.Recharge;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        private RechargeManager RechargeMng = new RechargeManager();

        private Dictionary<long, int> TempOrderList = new Dictionary<long, int>();
        public RechargeManager RechargeManager => RechargeMng;

        public List<RechargeHistoryItem> Historys = new List<RechargeHistoryItem>();

        public void BindFirstOrderInfo(RechargeHistoryItem firstOrder)
        {
            RechargeMng.BindFirstOrderInfo(firstOrder);
        }

        public void BindRechargeManager(int first, int total, int current, int daily, float price, float money, List<RechargeHistoryItem> historys, int accumulateOnceMaxMoney, int lastRechargeTime, int payCount)
        {
            RechargeMng.First = first;
            RechargeMng.AccumulateTotal = total;
            RechargeMng.AccumulateCurrent = current;
            RechargeMng.AccumulateDaily = daily;
            RechargeMng.AccumulatePrice = price;
            RechargeMng.AccumulateMoney = money;
            RechargeMng.AccumulateOnceMaxMoney = accumulateOnceMaxMoney;
            RechargeMng.LastCommonRechargeTime = lastRechargeTime;
            RechargeMng.PayCount = payCount;

            Historys = historys;
        }

        public void BindOperationalActivity(int MonthCardTime, int SeasonCardTime, int WeekCardStart, int WeekCardEnd, int MonthCardState, 
            int SuperMonthCardTime, int SuperMonthCardState, int SeasonCardState, string accumulateRechargeRewards, int newRechargeGiftScore, string newRechargeGiftRewards, int GrowthFund = 0)
        {
            RechargeMng.MonthCardTime = MonthCardTime;
            RechargeMng.MonthCardState = MonthCardState;
            RechargeMng.SuperMonthCardTime = SuperMonthCardTime;
            RechargeMng.SuperMonthCardState = SuperMonthCardState;
            RechargeMng.SeasonCardTime = SeasonCardTime;
            RechargeMng.SeasonCardState = SeasonCardState;
            RechargeMng.WeekCardStart = WeekCardStart;
            RechargeMng.WeekCardEnd = WeekCardEnd;
            RechargeMng.GrowthFund = GrowthFund;
            RechargeMng.AccumulateRechargeRewards = accumulateRechargeRewards;
            RechargeMng.NewRechargeGiftScore = newRechargeGiftScore;
            RechargeMng.NewRechargeGiftRewards = newRechargeGiftRewards;
        }

        public void SaveRechargeOrderId(string orderId)
        {
            //if (!string.IsNullOrEmpty(orderId))
            //{
            //    //保存修改
            //    //string tableName = "recharge_history";
            //    server.GameDBPool.Call(new QueryUpdateOrderId(Uid, CommonConst.RECHARGE_CREATE, orderId, ZoneServerApi.now));
            //}
            //else
            //{
            //    Log.WarnLine("player {0} SaveOrderId order id is null", Uid);
            //}
        }

        public void GetRechargeHistoryId(int giftId)
        {
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItemOrSdkItem(giftId);
            if (recharge == null)
            {
                //没有找到产品ID
                Log.Warn($"player {Uid} GetRechargeHistoryId productId {giftId} error: not find {giftId} item model");
                return;
            }
            RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
            if (price == null)
            {
                //没有找到价格
                Log.Warn($"player {Uid} GetRechargeHistoryId productId {giftId} error: not find {recharge.RechargeId} price model");
                return;
            }
            if (RechargeLibrary.CheckIsRechargeReward(recharge.GiftType, recharge.Id))
            {
                Log.Warn($"player {Uid} GetRechargeHistoryId productId {giftId} error: item is recharge reward can not buy");
                return;
            }
            if (!GiftManager.CheckGiftItemHaveBuyCount(recharge))
            {
                Log.Warn($"player {Uid} GetRechargeHistoryId productId {giftId} error: buy count not enough");
                return;
            }
            if (!CheckCanBuyPassCard(recharge))
            {
                Log.Warn($"player {Uid} GetRechargeHistoryId productId {giftId} error:  passcard already bought this period");
                return;
            }

            if (price.Price == 0)
            {
                switch (recharge.GiftType)
                {
                    case RechargeGiftType.LuckyFlipCard:
                        GetLuckyFlipCardRewardForFree(recharge);
                        break;
                    case RechargeGiftType.TreasureFlipCard:
                        GetTreasureFlipCardRewardForFree(recharge);
                        break;
                    case RechargeGiftType.IslandHighGift:
                        GetIslandHighGiftRewardForFree(recharge);
                        break;
                    case RechargeGiftType.NewRechargeGift:
                        {
                            RechargeGiftModel activityModel;
                            if (RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NewRechargeGift, ZoneServerApi.now, out activityModel))
                            {
                                //增加积分
                                RechargeMng.NewRechargeGiftScore += RechargeLibrary.GetNewRechargGifteScore(recharge.Id);
                                RechargeRewardEnd();
                                BIRecordPointGameLog(RechargeLibrary.GetNewRechargGifteScore(recharge.Id), RechargeMng.NewRechargeGiftScore, "new_recharge_gift", activityModel.SubType);
                            }
                            ReceiveFreeGiftReward(recharge);
                        }
                        break;
                    default:
                        ReceiveFreeGiftReward(recharge);
                        break;
                }
                return;
            }
  
            MSG_ZM_GET_RECHARGE_ID msg = new MSG_ZM_GET_RECHARGE_ID();
            msg.GiftId = giftId;
            server.ManagerServer.Write(msg, Uid);
        }

        public void SendRechargeHistoryId(int orderId, int giftId)
        {
            MSG_ZGC_GET_ORDER_ID msg = new MSG_ZGC_GET_ORDER_ID();
            msg.OrderId = orderId;
            msg.GiftId = giftId;
            Write(msg);
        }

        public void ReceiveFreeGiftReward(RechargeItemModel recharge)
        {
            MSG_ZGC_RECHARGE_GIFT response = new MSG_ZGC_RECHARGE_GIFT();
            bool result = CheckCanReceiveCommonGiftReward(recharge);
            if (result)
            {
                GiftItem item;
                SyncUpdateGiftItemInfo(recharge, out item);

                response.Result = (int)ErrorCode.Success;
                response.GiftItemId = item.Id;
                response.BuyCount = item.CurBuyCount;
                response.RewardRatio = item.DiamondRatio;

                string reward = recharge.Reward;
                if (!string.IsNullOrEmpty(recharge.ExtraReward))
                {
                    reward = string.Format("{0}|{1}|", reward, recharge.ExtraReward);
                }
                //发奖
                RewardManager manager = GetSimpleReward(reward, ObtainWay.Recharge);
                manager.GenerateRewardItemInfo(response.Rewards);
                Write(response);
            }
        }

        public void DeleteRechargeOrderId(long orderId)
        {
            if (orderId > 0)
            {
                //保存修改
                //string tableName = "recharge_history";
                server.GameDBPool.Call(new QueryDeleteRechargeHistory(Uid, orderId));
            }
            else
            {
                Log.WarnLine("player {0} DeleteRechargeOrderId order id is null", Uid);
            }
        }

        public void CheckNotReceivedRecharge()
        {
            if (Historys.Count > 0)
            {
                //RechargeHistoryItem item = Historys[0];
                //Historys.RemoveAt(0);
                //return item;
                foreach (var item in Historys)
                {
                    GetRechargeRewardNew(item.OrderId, item.OrderInfo, item.ProductId, (RechargeWay)item.Way, item.Money, item.PayCurrency, item.Time, item.Num, 0, item.PayMode);
                }

                Historys.Clear();
            }
        }

        //public RechargeHistoryItem CheckRechargeRewards()
        //{
        //    if (Historys.Count > 0)
        //    {
        //        RechargeHistoryItem item = Historys[0];
        //        Historys.RemoveAt(0);
        //        return item;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public bool CheckAllMonthCard()
        {
            if (CheckMonthCardState() ||
                CheckSuperMonthCardState())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CheckMonthCardState()
        {
            if (RechargeMng.MonthCardTime > Timestamp.GetUnixTimeStampSeconds(server.Now()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CheckSuperMonthCardState()
        {
            if (RechargeMng.SuperMonthCardTime > Timestamp.GetUnixTimeStampSeconds(server.Now()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public void GetRechargeReward(int rechargeType, string orderId, float amount, string time, string reward = "")
        //{
        //    //查看充值状态
        //    //string tableName = "recharge_history";
        //    server.GameDBPool.Call(new QueryGetOrderId(Uid, CommonConst.RECHARGE_GET, orderId), ret =>
        //    {
        //        int result = (int)ret;
        //        switch (result)
        //        {
        //            case -2:
        //                //出现异常
        //                Log.ErrorLine("player {0} GetRechargeReward RechargeType {1} QueryGetOrderId {2} error", Uid, rechargeType, orderId);
        //                return;
        //            case 1:
        //                //已经领取过
        //                Log.WarnLine("player {0} GetRechargeReward RechargeType {1} QueryGetOrderId {2} has get reward", Uid, rechargeType, orderId);
        //                return;
        //            case -1:
        //                //没有找到订单
        //                Log.WarnLine("player {0} GetRechargeReward RechargeType {1} QueryGetOrderId {2} not find make state", Uid, rechargeType, orderId);
        //                return;
        //            default:
        //                break;
        //        }

        //        //增加保险
        //        if (TempOrderList.TryGetValue(orderId, out result))
        //        {
        //            if (result == 1)
        //            {
        //                //已经处理过订单
        //                Log.WarnLine("player {0} GetRechargeReward RechargeType {1} TempOrderList {2} has get reward", Uid, rechargeType, orderId);
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            //计算奖励
        //            UpdateRechargeFromHistoryManager(rechargeType, orderId, amount, time, reward);
        //        }
        //    });
        //}

        //public void UpdateRechargeFromHistoryManager(int rechargeId, string orderId, float amount, string timeString, string reward)
        //{
        //    //最后获得的钻石
        //    int totalDiamond = 0;
        //    //充值时间用来判断当时是否有活动
        //    DateTime time = DateTime.Parse(timeString);
        //    //判断是否是内部
        //    bool needUpdateDb = CheckOrderIdUpdateDb(orderId);

        //    RechargeRewardModel recharge = RechargeLibrary.GetNormalRechargeModel(rechargeId);
        //    if (recharge != null)
        //    {
        //        float money = recharge.Money;

        //        //是否是首充
        //        CheckFirstReward();

        //        //充值项次数
        //        RechargeItem rechargeItem = RechargeMng.GetRechargeItem(rechargeId);
        //        rechargeItem.Totle += 1;
        //        rechargeItem.Current += 1;


        //        //检查充值奖励
        //        if (needUpdateDb && amount != money)
        //        {
        //            //如果不是测试账单，钱数和配置不符，不添加钻石
        //            Log.Warn("player {0} recharge type {1} money {2} real money {3}", Uid, rechargeId, money, amount);
        //        }
        //        else
        //        {
        //            totalDiamond = int.Parse(recharge.Reward);
        //        }
        //        //充值统计
        //        RechargeMng.AccumulateTotal += totalDiamond;
        //        RechargeMng.AccumulateCurrent += totalDiamond;

        //        //如果账单到账时间不是今天，那么今天的累加值不增加
        //        if (time >= ZoneServerApi.now.Date)
        //        {
        //            rechargeItem.Daily += 1;
        //            RechargeMng.AccumulateDaily += totalDiamond;
        //        }

        //        //修改状态
        //        TempOrderList.Add(orderId, 1);
        //        //string historyTableName = "recharge_history";
        //        server.GameDBPool.Call(new QueryUpdateOrderId(Uid, CommonConst.RECHARGE_GET, orderId, ZoneServerApi.now));

        //        //累计奖励
        //        reward = string.Format("{0}|{1}|{2}:{3}|", reward, recharge.Reward, (int)CurrenciesType.diamond, totalDiamond);
        //    }

        //    //实际处理
        //    UpdateRechargeManager(uid, MainId, rechargeId, orderId, time, amount);

        //    //检查是否还有没获得奖励的订单
        //    RechargeHistoryItem item = CheckRechargeRewards();
        //    if (item != null)
        //    {
        //        //继续处理订单
        //        GetRechargeReward(item.ProductId, item.OrderId, item.Money, item.Time, reward);
        //    }
        //    else
        //    {
        //        //结算奖励
        //        RechargeRewardEnd(reward);
        //    }
        //}

        ////public void UpdateRechargeManager(int rechargeType, string orderId, float amount, string timeString, string reward)
        ////{
        //////最后获得的钻石
        ////int totalDiamond = 0;
        //////充值时间用来判断当时是否有活动
        ////DateTime time = DateTime.Parse(timeString);
        //////判断是否是内部
        ////bool needUpdateDb = CheckOrderIdUpdateDb(orderId);

        ////RechargeRewardModel recharge = RechargeLibrary.GetRecharge(rechargeType);
        ////if (recharge != null)
        ////{
        ////    float money = recharge.Money;
        ////    int normalDiamond = recharge.Diamond;
        ////    int addDiamond = recharge.DiamondExt;
        ////    int dinamndExt = recharge.limitExtDiamond;
        ////    int limitNum = recharge.limit;
        ////    int maxBuy = recharge.MaxBuy;

        ////    //是否是首充
        ////    CheckFirstReward();

        ////    //充值项次数
        ////    RechargeItem rechargeItem = RechargeMng.GetRechargeItem(rechargeType);
        ////    rechargeItem.Totle += 1;
        ////    rechargeItem.Current += 1;


        ////    //检查充值奖励
        ////    if (needUpdateDb && amount != money)
        ////    {
        ////        //如果不是测试账单，钱数和配置不符，不添加钻石
        ////        Log.Warn("player {0} recharge type {1} money {2} real money {3}", Uid, rechargeType, money, amount);
        ////    }
        ////    else
        ////    {
        ////        //判断最大购买次数
        ////        if (maxBuy > 0 && rechargeItem.Current > maxBuy)
        ////        {
        ////            //如果有最大购买次数，已经购买的次数超过了配置值，只给普通钻石数
        ////            Log.Warn("player {0} recharge type {1} max buy {2} real buy {3} ", Uid, rechargeType, maxBuy, rechargeItem.Current);
        ////            totalDiamond = normalDiamond;
        ////        }
        ////        else
        ////        {
        ////            totalDiamond = normalDiamond;
        ////            //判断特殊奖励次数
        ////            if (limitNum > 0 && rechargeItem.Current <= limitNum)
        ////            {
        ////                //特殊奖励
        ////                totalDiamond = totalDiamond + dinamndExt;
        ////            }
        ////            else
        ////            {
        ////                //正常奖励
        ////                totalDiamond = totalDiamond + addDiamond;
        ////            }
        ////        }
        ////    }
        ////    //充值统计
        ////    RechargeMng.AccumulateTotal += totalDiamond;
        ////    RechargeMng.AccumulateCurrent += totalDiamond;


        ////    //如果账单到账时间不是今天，那么今天的累加值不增加
        ////    if (time >= ZoneServerApi.now.Date)
        ////    {
        ////        rechargeItem.Daily += 1;
        ////        RechargeMng.AccumulateDaily += totalDiamond;
        ////    }

        ////    //修改状态
        ////    TempOrderList.Add(orderId, 1);
        ////    //string historyTableName = "recharge_history";
        ////    server.GameDBPool.Call(new QueryUpdateOrderId(Uid, CommonConst.RECHARGE_GET, orderId, ZoneServerApi.now));

        ////    //累计奖励
        ////    reward = string.Format("{0}|{1}|{2}:{3}|", reward, recharge.Reward, (int)CurrenciesType.diamond, totalDiamond);
        ////}

        //////检查是否还有没获得奖励的订单
        ////RechargeHistoryItem item = CheckRechargeRewards();
        ////if (item != null)
        ////{
        ////    //继续处理订单
        ////    GetRechargeReward(item.ProductId, item.OrderId, item.Money, item.Time, reward);
        ////}
        ////else
        ////{
        ////    //结算奖励
        ////    RechargeRewardEnd(reward);
        ////}
        ////}

        //public void UpdateRechargeManager(int pcUid, int mainId, int rechargeId, string orderId, DateTime time, float amount)
        //{
        //    //比对信息
        //    RechargeRewardModel recharge = RechargeLibrary.GetNormalRechargeModel(rechargeId);
        //    if (recharge.Money != (int)amount)
        //    {
        //        Log.Error($"player {pcUid} recharge mainId {mainId} rechargeId {rechargeId} order {orderId} get error with amount for {recharge.Money} and realAmount {amount}");
        //        return;
        //    }
        //    //发奖

        //    List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

        //    QueryRecharge rechargeQuery = new QueryRecharge(pcUid, recharge.Money);
        //    querys.Add(rechargeQuery);


        //    //判断是否是特殊卡 
        //    //根据rechargeType确定月卡等信息
        //    //如果特殊就更新db的位置
        //    RechargeTypeInfo priceInfo = RechargeLibrary.GetRechargeTypeInfo(recharge.RechargeType);
        //    if (priceInfo != null)
        //    {
        //        switch (recharge.RechargeType)
        //        {
        //            case RechargeType.MonthCard:
        //                {
        //                    int seconds = (int)(server.Now().Date.AddDays(priceInfo.Param) - server.Now()).TotalSeconds;
        //                    QueryUpdateMonthCard tempQuery = new QueryUpdateMonthCard(pcUid, seconds);
        //                    querys.Add(tempQuery);
        //                }
        //                break;
        //            case RechargeType.WeekCard:
        //                int seconds3 = (int)(server.Now().Date.AddDays(priceInfo.Param) - server.Now()).TotalSeconds;
        //                QueryUpdateWeekCard weekQuery = new QueryUpdateWeekCard(pcUid, seconds3);
        //                querys.Add(weekQuery);
        //                break;
        //            case RechargeType.SeasonCard:
        //                int seconds2 = (int)(server.Now().Date.AddDays(priceInfo.Param) - server.Now()).TotalSeconds;
        //                QueryUpdateSeasonCard seasonQuery = new QueryUpdateSeasonCard(pcUid, seconds2);
        //                querys.Add(seasonQuery);
        //                break;
        //            case RechargeType.GrowthFund:
        //                QueryUpdateGrowthFund growthQuery = new QueryUpdateGrowthFund(pcUid);
        //                querys.Add(growthQuery);
        //                break;
        //            case RechargeType.GrowthFundEx:
        //                QueryUpdateGrowthFundEx growthExQuery = new QueryUpdateGrowthFundEx(pcUid);
        //                querys.Add(growthExQuery);
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    server.GameDBPool.Call(new DBQueryTransaction(querys, true), ret =>
        //    {
        //        UpdateRechargeRewardManager(recharge.RechargeId, orderId, recharge.Money, time.ToString());
        //    });
        //    server.GameDBPool.Call(new QueryUpdateOrderId(Uid, CommonConst.RECHARGE_GET, orderId, ZoneServerApi.now));

        //}

        //public void UpdateRechargeRewardManager(int rechargeId, string orderId, float amount, string timeString)
        //{
        //    string reward = (int)CurrenciesType.diamond + ":" + (int)RewardType.Currencies + ":" + amount;

        //    RechargeItemModel rechargeItemModel = RechargeLibrary.GetRechargeItem(rechargeId);
        //    if (rechargeItemModel == null) return;

        //    BIRecordRechargeLog((int)amount, orderId, rechargeId.ToString(), rechargeItemModel.GiftType.ToString(), ((CommonGiftType)rechargeItemModel.SubType).ToString());

        //    //根据rechargeType确定月卡等信息
        //    RechargeRewardModel recharge = RechargeLibrary.GetNormalRechargeModel(rechargeId);
        //    RechargeTypeInfo priceInfo = RechargeLibrary.GetRechargeTypeInfo(recharge.RechargeType);
        //    if (priceInfo != null)
        //    {
        //        switch (recharge.RechargeType)
        //        {
        //            case RechargeType.MonthCard:
        //                {
        //                    int seconds = (int)(server.Now().Date.AddDays(priceInfo.Param) - server.Now()).TotalSeconds;
        //                    //RechargeMng.MonthCardTime =Timestamp.GetUnixTimeStampSeconds(server.Now())+ seconds;//db在manager更新过了,微小的差异不会影响
        //                    if (Timestamp.TimeStampToDateTime(RechargeMng.MonthCardTime) < server.Now())
        //                    {
        //                        seconds += Timestamp.GetUnixTimeStampSeconds(server.Now());
        //                    }
        //                    else
        //                    {
        //                        seconds += RechargeMng.MonthCardTime;
        //                    }
        //                    RechargeMng.MonthCardTime = seconds;
        //                }
        //                break;
        //            case RechargeType.WeekCard:
        //                {
        //                    int seconds3 = (int)(server.Now().Date.AddDays(priceInfo.Param) - server.Now()).TotalSeconds;
        //                    if (Timestamp.TimeStampToDateTime(RechargeMng.WeekCardEnd) < server.Now())
        //                    {
        //                        RechargeMng.WeekCardStart = Timestamp.GetUnixTimeStampSeconds(server.Now());
        //                        RechargeMng.WeekCardEnd = Timestamp.GetUnixTimeStampSeconds(server.Now()) + seconds3;
        //                    }
        //                    else
        //                    {
        //                        RechargeMng.WeekCardEnd += seconds3;
        //                    }
        //                }
        //                break;
        //            case RechargeType.SeasonCard:
        //                {
        //                    int seconds2 = (int)(server.Now().Date.AddDays(priceInfo.Param) - server.Now()).TotalSeconds;
        //                    if (Timestamp.TimeStampToDateTime(RechargeMng.SeasonCardTime) < server.Now())
        //                    {
        //                        seconds2 += Timestamp.GetUnixTimeStampSeconds(server.Now());
        //                    }
        //                    else
        //                    {
        //                        seconds2 += RechargeMng.MonthCardTime;
        //                    }
        //                    RechargeMng.SeasonCardTime = seconds2;
        //                }
        //                break;
        //            case RechargeType.GrowthFund:
        //                RechargeMng.GrowthFund |= 1;
        //                break;
        //            case RechargeType.GrowthFundEx:
        //                RechargeMng.GrowthFund |= 2;
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    //通知前端
        //    RechargeRewardEnd(reward);
        //    //SendRechargeManger(RechargeMng.GetProductsValue(), RechargeMng.GetRewardsValue());
        //}

        //public void DebugRecharge(int id)
        //{
        //    //模拟充值到manager
        //    MSG_ZM_DEBUG_RECHARGE msg = new MSG_ZM_DEBUG_RECHARGE();
        //    msg.RechargeId = id;
        //    msg.Uid = uid;
        //    server.ManagerServer.Write(msg);
        //    //Log.Debug($"player {uid} debug recharge with id {id}");
        //}

        private void RechargeRewardEnd()
        {
            //修改充值数据
            //string tableName = "recharge";
            server.GameDBPool.Call(new QueryUpdateRechargeProducts(Uid, RechargeMng.AccumulateTotal, RechargeMng.AccumulateCurrent, RechargeMng.AccumulateDaily, RechargeMng.AccumulateOnceMaxMoney, RechargeMng.AccumulatePrice, RechargeMng.AccumulateMoney, RechargeMng.PayCount, RechargeMng.NewRechargeGiftScore));

            //通知客户端
            SendRechargeManger();
        }

        private void SendRechargeManger()
        {
            //通知客户端
            MSG_ZGC_RECHARGE_MANAGER msg = new MSG_ZGC_RECHARGE_MANAGER();
            msg.First = RechargeMng.First;
            msg.AccumulateTotal = RechargeMng.AccumulateTotal;
            msg.AccumulateCurrent = RechargeMng.AccumulateCurrent;
            msg.AccumulateDaily = RechargeMng.AccumulateDaily;
            msg.MonthCardTime = RechargeMng.MonthCardTime;
            msg.MonthCardState = RechargeMng.MonthCardState;
            msg.SuperMonthCardTime = RechargeMng.SuperMonthCardTime;
            msg.SuperMonthCardState = RechargeMng.SuperMonthCardState;
            msg.SeasonCardTime = RechargeMng.SeasonCardTime;
            //msg.SeasonCardState = RechargeMng.SeasonCardState;
            msg.WeekCardEnd = RechargeMng.WeekCardEnd;
            msg.WeekCardStart = RechargeMng.WeekCardStart;
            msg.GrowthFund = RechargeMng.GrowthFund;
            //msg.Products = product;
            //msg.Rewards = rewards;
            msg.AccumulateRechargeRewards.AddRange(RechargeMng.GetAccumulateRechargeRewards());
            msg.AccumulatePrice = RechargeMng.AccumulatePrice;
            msg.NewRechargeGiftScore = RechargeMng.NewRechargeGiftScore;
            msg.NewRechargeGiftRewards.AddRange(RechargeMng.GetNewRechargeGiftRewards());
            Write(msg);
        }

        private bool CheckFirstReward()
        {
            if (RechargeMng.First == -1 && RechargeManager.AccumulateTotal >= RechargeLibrary.FirstRechargeAccumulate)
            {
                RechargeMng.First = 0;
                //单独修改数据库
                //string tableName = "recharge";
                server.GameDBPool.Call(new QueryUpdateFirstReward(Uid, 0));
                return true;
            }
            return false;
        }

        public bool CheckIsBigRPlayer()
        {
            if (RechargeMng.AccumulateTotal > 10000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool CheckOrderIdUpdateDb(string orderId)
        {
            if (orderId.StartsWith(CONST.VIRTUAL_RECHARGE_PREFIX))
            {
                // 如果是内部充值订单，不记录到BI中
                return false;
            }
            else
            {
                return true;
            }
        }

        //----------------------------新旧充值分割线--------------------------------------

        /// <summary>
        /// 购买充值礼包
        /// </summary>
        /// <param name="id">giftItemId</param>
        public void BuyRechargeGift(int id, ulong giftUid, bool isFlipCard = false)
        {
            //模拟充值到manager
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItemOrSdkItem(id);
            if (recharge == null)
            {
                Log.WarnLine($"player {Uid} BuyRechargeGift productId {id} error: not find gift in xml");
                return;
            }
            if (!GiftManager.CheckGiftItemHaveBuyCount(recharge))
            {
                Log.ErrorLine($"player {Uid} BuyRechargeGift productId {id} error: buy count not enough");
                return;
            }
            RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
            if (price != null && price.Price == 0)
            {
                switch (recharge.GiftType)
                {
                    case RechargeGiftType.LuckyFlipCard:
                        GetLuckyFlipCardRewardForFree(recharge);
                        break;
                    case RechargeGiftType.TreasureFlipCard:
                        GetTreasureFlipCardRewardForFree(recharge);
                        break;
                    case RechargeGiftType.IslandHighGift:
                        GetIslandHighGiftRewardForFree(recharge);
                        break;
                    case RechargeGiftType.NewRechargeGift:
                        {
                            RechargeGiftModel activityModel;
                            if (RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NewRechargeGift, ZoneServerApi.now, out activityModel))
                            {
                                //增加积分
                                RechargeMng.NewRechargeGiftScore += RechargeLibrary.GetNewRechargGifteScore(recharge.Id);
                                RechargeRewardEnd();
                                BIRecordPointGameLog(RechargeLibrary.GetNewRechargGifteScore(recharge.Id), RechargeMng.NewRechargeGiftScore, "new_recharge_gift", activityModel.SubType);
                            }
                            ReceiveFreeGiftReward(recharge);
                        }
                        break;
                    default:
                        ReceiveFreeGiftReward(recharge);
                        break;
                }
            }
            else if (recharge.GiftType == RechargeGiftType.TreasureFlipCard && isFlipCard)
            {
                GetTreasureFlipCardRewardForFlipCard(recharge);
            }
            else
            {
#if DEBUG
                MSG_ZM_BUY_RECHARGE_GIFT msg = new MSG_ZM_BUY_RECHARGE_GIFT();
                msg.Uid = Uid;
                msg.GiftId = id;
                msg.GiftUid = giftUid;
                RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(recharge.DiscountRechargeId);
                bool discount = GiftManager.CheckHasDiscountPrice(recharge, discountPrice);
                msg.Discount = discount;
                server.ManagerServer.Write(msg, Uid);
#endif
            }
            Log.Debug($"player {Uid} buy recharge gift with id {id} uid {giftUid}");
        }

        public void OpenRechargeGift(int id, ulong giftUid)
        {
            //模拟充值到manager
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(id);
            if (recharge == null)
            {
                recharge = RechargeLibrary.GetRechargeItem(id, true);
            }

            if (recharge == null)
            {
                Log.WarnLine($"player {Uid} OpenRechargeGift productId {id} error: not find gift in xml");
                return;
            }
            if (!GiftManager.CheckHaveThisTypeGift(recharge))
            {
                Log.ErrorLine($"player {Uid} OpenRechargeGift productId {id} error: buy count not enough");
                return;
            }

            GiftItem giftItem = GiftManager.GetGiftItem(recharge);
            if (giftItem == null)
            {
                Log.ErrorLine($"player {Uid} OpenRechargeGift productId {id} error: not find giftItem");
                return;
            }
            TimingGiftInfo info = ActionManager.GetTimingGiftInfo(giftItem.Uid);
            if (info == null)
            {
                Log.ErrorLine($"player {Uid} OpenRechargeGift productId {id} error: not find giftItem uid {giftItem.Uid}");
                return;
            }
            RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
            if (price == null)
            {
                Log.WarnLine($"player {Uid} OpenRechargeGift productId {id} error: not find gift price");
                return;
            }

            BIRecordLimitPackLog(price.Price, "CNY", 2, 0, info.ActionId, id, giftUid);
            //KomoeEventLogGiftPush((int)recharge.GiftType, giftItem.Id, recharge.GiftType.ToString(), price.Price, info.ActionId, RewardManager.GetRewardDic(recharge.Reward));
        }
        //public void UpdateRechargeManagerNew(int pcUid, int mainId, int rechargeId, ulong rechargeUid, string orderId, DateTime time, float amount, bool hasDiscount = false)
        //{
        //    //比对信息
        //    RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(rechargeId);
        //    RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);


        //}

        //public void UpdateRechargeRewardManagerNew(int rechargeId, string orderId, float amount, string rewards, string extraRewards, string timeString, bool first, bool firstDiamond, RechargeGiftType giftType, string subType)
        //{
        //    //string reward = (int)CurrenciesType.diamond + ":" + (int)RewardType.Currencies + ":" + amount;

        //    BIRecordRechargeLog((int)amount, orderId, rechargeId.ToString(), giftType.ToString(), subType);

        //    //根据rechargeType确定月卡等信息
        //    RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(rechargeId);
        //    RechargeTypeInfo rechargeType = RechargeLibrary.GetRechargeTypeInfo(recharge.RechargeType);
        //    if (rechargeType != null)
        //    {
        //        switch ((RechargeType)recharge.RechargeType)
        //        {
        //            case RechargeType.MonthCard:
        //                {
        //                    int seconds = (int)(server.Now().Date.AddDays(rechargeType.Param) - server.Now()).TotalSeconds;
        //                    //RechargeMng.MonthCardTime =Timestamp.GetUnixTimeStampSeconds(server.Now())+ seconds;//db在manager更新过了,微小的差异不会影响
        //                    if (Timestamp.TimeStampToDateTime(RechargeMng.MonthCardTime) < server.Now())
        //                    {
        //                        seconds += Timestamp.GetUnixTimeStampSeconds(server.Now());
        //                    }
        //                    else
        //                    {
        //                        seconds += RechargeMng.MonthCardTime;
        //                    }
        //                    RechargeMng.MonthCardTime = seconds;
        //                }
        //                break;
        //            case RechargeType.SuperMonthCard:
        //                {
        //                    int seconds2 = (int)(server.Now().Date.AddDays(rechargeType.Param) - server.Now()).TotalSeconds;
        //                    if (Timestamp.TimeStampToDateTime(RechargeMng.SuperMonthCardTime) < server.Now())
        //                    {
        //                        seconds2 += Timestamp.GetUnixTimeStampSeconds(server.Now());
        //                    }
        //                    else
        //                    {
        //                        seconds2 += RechargeMng.SuperMonthCardTime;
        //                    }
        //                    RechargeMng.SuperMonthCardTime = seconds2;
        //                }
        //                break;
        //            case RechargeType.WeekCard:
        //                {
        //                    int seconds3 = (int)(server.Now().Date.AddDays(rechargeType.Param) - server.Now()).TotalSeconds;
        //                    if (Timestamp.TimeStampToDateTime(RechargeMng.WeekCardEnd) < server.Now())
        //                    {
        //                        RechargeMng.WeekCardStart = Timestamp.GetUnixTimeStampSeconds(server.Now());
        //                        RechargeMng.WeekCardEnd = Timestamp.GetUnixTimeStampSeconds(server.Now()) + seconds3;
        //                    }
        //                    else
        //                    {
        //                        RechargeMng.WeekCardEnd += seconds3;
        //                    }
        //                }
        //                break;
        //            case RechargeType.SeasonCard:
        //                {
        //                    int seconds4 = (int)(server.Now().Date.AddDays(rechargeType.Param) - server.Now()).TotalSeconds;
        //                    if (Timestamp.TimeStampToDateTime(RechargeMng.SeasonCardTime) < server.Now())
        //                    {
        //                        seconds4 += Timestamp.GetUnixTimeStampSeconds(server.Now());
        //                    }
        //                    else
        //                    {
        //                        seconds4 += RechargeMng.MonthCardTime;
        //                    }
        //                    RechargeMng.SeasonCardTime = seconds4;
        //                }
        //                break;
        //            case RechargeType.GrowthFund:
        //                RechargeMng.GrowthFund |= 1;
        //                break;
        //            case RechargeType.GrowthFundEx:
        //                RechargeMng.GrowthFund |= 2;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    //通知前端
        //    //结算奖励
        //    RechargeRewardEnd(rewards, extraRewards, firstDiamond);

        //    //SendRechargeManger(RechargeMng.GetProductsValue(), RechargeMng.GetRewardsValue());
        //}

        public void GetRechargeRewardNew(long orderId, string orderInfo, int productId, RechargeWay way, float amount, string payCurrency, string time, int num, int isSandbox, int payMode)
        {
            //if (TempOrderList.ContainsKey(orderId))
            //{
            //    //正在处理过订单
            //    Log.WarnLine($"player {Uid} GetRechargeReward productId {productId} OrderId {orderId} error: is getting");
            //    return;
            //}
            //TempOrderList.Add(orderId, 1);
            //查看充值状态
            //QueryGetOrderInfo queryGetOrderInfo = new QueryGetOrderInfo(orderId);
            //server.GameDBPool.Call(queryGetOrderInfo, ret =>
            //{
            //if (!queryGetOrderInfo.FindOrder)
            //{
            //    //没有找到订单号
            //    Log.ErrorLine($"player {Uid} GetRechargeReward productId {productId} OrderId {orderId} error: not find order");
            //    return;
            //}

            //if (queryGetOrderInfo.GetState != 0)
            //{
            //    //已经领取
            //    Log.ErrorLine($"player {Uid} GetRechargeReward productId {productId} OrderId {orderId} error: has get reward");
            //    return;
            //}

                try
                {
                    //计算奖励
                    UpdateRechargeFromHistoryManagerNew(orderId, orderInfo, productId, way, amount, payCurrency, time, num, isSandbox, payMode);
                }
                catch (Exception e)
                {
                    Log.WarnLine($"player {Uid} GetRechargeReward productId {productId} OrderId {orderId} error: {e.ToString()}");
                	DeleteRechargeOrderId(orderId);
                }
                finally
                {
                    TempOrderList.Remove(orderId);
                }
            //});
        }

        //充值
        public void UpdateRechargeFromHistoryManagerNew(long orderId, string orderInfo, int giftItemId, RechargeWay way, float amount, string payCurrency, string timeString, int buyNum, int isSandbox, int payMode)
        {
            Log.Info($"player {Uid} GetRechargeReward productId {giftItemId} OrderId {orderId} info {orderInfo} way {way} amount {amount} payCurrency {payCurrency}timeString {timeString} buyNum {buyNum} isSandbox {isSandbox} payMode {payMode}");

            RechargeItemModel recharge = RechargeLibrary.GetRechargeItemOrSdkItem(giftItemId);
            if (recharge == null)
            {
                //没有找到产品ID
                Log.ErrorLine($"player {Uid} GetRechargeReward productId {giftItemId} OrderId {orderId} error: not find {giftItemId} item model");
                DeleteRechargeOrderId(orderId);
                return;
            }
            RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
            if (price == null)
            {
                //没有找到价格
                Log.ErrorLine($"player {Uid} GetRechargeReward productId {giftItemId} OrderId {orderId} error: not find {recharge.RechargeId} price model");
                DeleteRechargeOrderId(orderId);
                return;
            }
            if (RechargeLibrary.CheckIsRechargeReward(recharge.GiftType, recharge.Id))
            {
                Log.ErrorLine($"player {Uid} GetRechargeReward productId {giftItemId} OrderId {orderId} error: item is recharge reward can not buy");
                DeleteRechargeOrderId(orderId);
                return;
            }
            if (!GiftManager.CheckGiftItemHaveBuyCount(recharge))
            {
                Log.ErrorLine($"player {Uid} GetRechargeReward productId {giftItemId} OrderId {orderId} error: buy count not enough");
                DeleteRechargeOrderId(orderId);
                return;
            }
            if (!CheckCanBuyPassCard(recharge))
            {
                Log.ErrorLine($"player {Uid} GetRechargeReward productId {giftItemId} OrderId {orderId} error: passcard already bought this period");
                DeleteRechargeOrderId(orderId);
                return;
            }
            int num = 1;
            if (way == RechargeWay.VMall && buyNum > 1)
            {
                num = buyNum;
            }
            //最后获得的钻石
            int totalDiamond = 0;
            //充值时间用来判断当时是否有活动
            DateTime time = DateTime.Parse(timeString);
            //花费
            float money;
            float payMoneyPrice;
            //是否打折
            RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(recharge.DiscountRechargeId);
            bool hasDiscount = CheckHasDiscountPrice(recharge, discountPrice);
            //判断是否有折扣价且是首次充值该项
            if (hasDiscount)
            {
                money = discountPrice.Money;
                totalDiamond = discountPrice.Diamond;
                payMoneyPrice = discountPrice.Price;
            }
            else
            {
                money = price.Money;
                totalDiamond = price.Diamond;
                payMoneyPrice = price.Price;
            }

            //检查充值奖励
            if (!CheckRechargeTest(way))
            {
                //if (amount != money)
                //{
                //    //如果不是测试账单，钱数和配置不符，不添加钻石
                //    Log.ErrorLine($"player {Uid} GetRechargeReward productId {giftItemId} OrderId {orderId} error: money {money} real money {amount}");
                //    DeleteRechargeOrderId(orderId);
                //    return;
                //}
            }

            //充值统计
            bool ignoreAccumulate = false;
            if (recharge.GiftType == RechargeGiftType.VWall && recharge.SubType == (int)VMallItemType.RechargeToken)
            {
                ignoreAccumulate = true;
            }
            if (!ignoreAccumulate)
            {
                RechargeMng.AccumulateTotal += totalDiamond * num;
                RechargeMng.AccumulateCurrent += totalDiamond * num;
                RechargeMng.AccumulatePrice += payMoneyPrice * num;
                RechargeMng.AccumulateMoney += money * num;
                RechargeMng.PayCount += num;
            }

            //活动期充值统计
            if (RechargeLibrary.CheckInSpecialRechargeActivityTime(RechargeGiftType.CarnivalRecharge, ZoneServerApi.now) && !ignoreAccumulate)
            {
                AddCarnivalAccumulatePrice(payMoneyPrice * num);
            }

            AddWelfareTriggerItem(WelfareConditionType.Recharge, (int)RechargeMng.AccumulatePrice);

            //充值项次数
            //RechargeItem rechargeItem = RechargeMng.GetRechargeItem(giftItemId);
            //rechargeItem.Totle += num;
            //rechargeItem.Current += num;

            //如果账单到账时间不是今天，那么今天的累加值不增加
            if (time >= ZoneServerApi.now.Date)
            {
                //rechargeItem.Daily += num;
                if (!ignoreAccumulate)
                {
                    RechargeMng.AccumulateDaily += totalDiamond * num;
                }
            }

            //实际处理

            //特殊充值项状态处理
            DoSpeciaRechargeIteml(recharge.GiftType, recharge.SubType);
            string reward = string.Empty;

            if (recharge.GiftType == RechargeGiftType.VWall)
            {
                if (num > 1 && !string.IsNullOrEmpty(recharge.Reward))
                {
                    List<ItemBasicInfo> allRewards = RewardDropLibrary.GetSimpleRewards(recharge.Reward, num);
                    if (allRewards.Count > 0)
                    {
                        foreach (var item in allRewards)
                        {
                            reward += "|" + item.ToString();
                        }
                        reward = reward.Substring(1);
                    }
                }
                else
                {
                    reward = recharge.Reward;
                }

                //发送邮件
                SendPersonEmail(recharge.EmailId, reward: reward);
                //server.GameDBPool.Call(new QueryUpdateVMallOrder(orderId, server.Now()));
            }
            else
            {
                //发放奖励
                int rewardRatio = GetRechargeItemRatio(recharge);
                reward = recharge.Reward;

                Log.Write("player {0} recharge end get reward {1} ratio {2}", Uid, reward, rewardRatio);
                //获得奖励
                RewardManager rewards = new RewardManager();
                rewards.InitSimpleReward(reward, false);
                rewards *= rewardRatio;
                rewards.BreakupRewards();
                AddRewards(rewards, ObtainWay.Recharge, giftItemId.ToString());
                if (rewardRatio > 1)
                {
                    reward = GetFinalRechargeRewards(rewards);
                }
            }


            //免费领不算入首充
            if (price.Price != 0 && way != RechargeWay.ActivateItem)
            {
                //是否是首充
                if (CheckFirstReward())
                {
                    //首充公告
                    BroadCastFirstRecharge();

                    //首充发称号卡
                    TitleMng.UpdateTitleConditionCount(TitleObtainCondition.FirstRecharge);

                    BIRecordActivityLog(ActivityAction.FirstRecharge, giftItemId);
                    //首充
                    server.BILoggerMng.FirstRechargeTaLog(Uid, ChannelId, server.MainId, payMoneyPrice);
                }
            }
         

#if SUNJIA
            //测试充值不记录BI
            BIRecordRechargeLog(amount, orderId, orderInfo, giftItemId.ToString(), recharge.GiftType.ToString(), recharge.SubType.ToString());

#endif
            //充值埋点
            if (!CheckRechargeTest(way))
            {
                //代币,激活道具不记录日志
                if (way == RechargeWay.Token)
                {
                    BIRecordTokenConsumeLog(payMoneyPrice * num, orderId, orderInfo, giftItemId.ToString(), recharge.GiftType.ToString(), recharge.SubType.ToString());
                }
                else if (way == RechargeWay.VMall)
                {
                    BIRecordRechargeLog(payMoneyPrice * num, orderId, orderInfo, giftItemId.ToString(), recharge.GiftType.ToString(), recharge.SubType.ToString(), "2");
                }
                else //if (way != RechargeWay.ActivateItem)
                {
                    //测试充值不记录BI
                    BIRecordRechargeLog(payMoneyPrice * num, orderId, orderInfo, giftItemId.ToString(), recharge.GiftType.ToString(), recharge.SubType.ToString());
                }
                //BI:recharge
                KomoeEventLogRechargeFlow(orderId, money, amount, payCurrency, giftItemId, (int)recharge.GiftType, recharge.GiftType.ToString(), 
                    num, RechargeMng.AccumulateMoney, RechargeMng.AccumulatePrice, isSandbox);


         
            }
            if (payMoneyPrice > 0)
            {
                AddSpecialActivityNumForType(SpecialAction.AnyMoney);
                AddSpecialActivityNumForType(SpecialAction.FixedMoney, payMoneyPrice * num);
            }
            //
            GiftItem giftItem = null;
            switch (recharge.GiftType)
            {
                case RechargeGiftType.PettyMoney:
                    UpdatePettyMoneyGift(recharge);
                    break;
                case RechargeGiftType.DailyRecharge:
                    UpdateDailyRecharge(recharge, reward);
                    break;
                case RechargeGiftType.NewServerPromotion:
                    UpdateNewServerPromotion(recharge, reward);
                    break;
                case RechargeGiftType.LuckyFlipCard:
                    UpdateLuckyFlipCardInfo(recharge, reward);
                    break;
                case RechargeGiftType.IslandHighGift:
                    UpdateIslandHighGiftInfo(recharge, reward);
                    break;
                case RechargeGiftType.Trident:
                    UpdateTridentInfo(recharge, reward);
                    break;
                case RechargeGiftType.DragonBoat:
                    UpdateDragonBoatBuyInfo(recharge, reward);
                    break;
                case RechargeGiftType.VWall:
                    break;
                case RechargeGiftType.TreasureFlipCard:
                    UpdateTreasureFlipCardInfo(recharge, reward);
                    break;
                case RechargeGiftType.DirectPurchase:
                    UpdateDirectPurchaseInfo(recharge, reward);
                    break;
                default:
                    SyncUpdateGiftItemInfo(recharge, out giftItem);
                    UpdateSpecialGiftInfo(recharge, giftItem);

                    if (giftItem != null)
                    {
                        SendRechargeGiftInfo(giftItem, reward);
                    }
                    break;
            }
            //保存充值状态，通知前端
            RechargeRewardEnd();

            MSG_ZM_UPDATE_RECHARGE updateMsg = new MSG_ZM_UPDATE_RECHARGE();
            updateMsg.OrderId = orderId;
            updateMsg.Uid = Uid;
            updateMsg.RechargeId = giftItemId;
            updateMsg.Money = amount;
            updateMsg.Way = (int)way;
            updateMsg.OrderInfo = orderInfo;
            updateMsg.Num = num;
            server.ManagerServer.Write(updateMsg);

            //检查是否还有没获得奖励的订单
            //CheckNotReceivedRecharge();

            //if (way != RechargeWay.VMall)
            //{
            //    //修改状态
            //    server.GameDBPool.Call(new QueryChangeRechargeGetRewardState(orderId, ZoneServerApi.now));

            //    //检查是否还有没获得奖励的订单
            //    CheckNotReceivedRecharge();
            //}

            //触发限时礼包
            if (giftItem != null)
            {
                bool isCommonGift = ActionManager.OnBuyedTimeGift(giftItem);
                ActionManager.BIRecordLimitTimePackLog(recharge, giftItem, way);

                if (money > RechargeMng.AccumulateOnceMaxMoney)
                {
                    SetRecentAccumulateOnceMaxMoney((int)money, isCommonGift);
                }
            }

            //不同渠道支付方式处理
            if (payMode > 0)
            {
                WebPayModeTypeActionLogic(payMode, recharge, payMoneyPrice);
            }

            //检查更新firstOrderInfo
            if (RechargeMng.FirstOrderInfo.OrderId == 0)
            {
                QueryLoadFirstOrderInfo query = new QueryLoadFirstOrderInfo(Uid);
                server.GameDBPool.Call(query, ret=> 
                {
                    if (query.Info != null)
                    {
                        RechargeMng.BindFirstOrderInfo(query.Info);
                    }
                });
            }
            //漂流探宝
            AddDriftExploreTaskNum(TaskType.TotalRecharge, payMoneyPrice * num, false);
        }

        private bool CheckRechargeTest(RechargeWay way)
        {
            switch (way)
            {
                case RechargeWay.Zone:
                case RechargeWay.Global:
                    return true;
                default:
                    return false;
            }
        }

        private void DoSpeciaRechargeIteml(RechargeGiftType type, int subType)
        {
            //根据礼包类型确定月卡等信息         
            switch (type)
            {
                case RechargeGiftType.MonthCard:
                    {
                        if ((MonthCardType)subType == MonthCardType.Normal)
                        {
                            int seconds = (int)(server.Now().Date.AddDays(RechargeLibrary.MonthCardDays) - server.Now()).TotalSeconds;
                            if (Timestamp.TimeStampToDateTime(RechargeMng.MonthCardTime) < DateTime.Now)
                            {
                                seconds += Timestamp.GetUnixTimeStampSeconds(DateTime.Now);
                            }
                            else
                            {
                                seconds += RechargeMng.MonthCardTime;
                            }
                            RechargeMng.MonthCardTime = seconds;
                            server.GameDBPool.Call(new QueryUpdateMonthCard(Uid, RechargeMng.MonthCardTime));
                        }
                        else if ((MonthCardType)subType == MonthCardType.Super)
                        {
                            int seconds = (int)(server.Now().Date.AddDays(RechargeLibrary.MonthCardDays) - server.Now()).TotalSeconds;
                            if (Timestamp.TimeStampToDateTime(RechargeMng.SuperMonthCardTime) < DateTime.Now)
                            {
                                seconds += Timestamp.GetUnixTimeStampSeconds(DateTime.Now);
                            }
                            else
                            {
                                seconds += RechargeMng.SuperMonthCardTime;
                            }
                            RechargeMng.SuperMonthCardTime = seconds;
                            server.GameDBPool.Call(new QueryUpdateSuperMonthCard(Uid, RechargeMng.SuperMonthCardTime));
                        }
                    }
                    break;
                case RechargeGiftType.GrowthFund:
                    {
                        switch ((GrowthFundType)subType)
                        {
                            case GrowthFundType.Normal:
                                {
                                    RechargeMng.GrowthFund |= 1;
                                    server.GameDBPool.Call(new QueryUpdateGrowthFund(Uid, RechargeMng.GrowthFund));
                                    //成长基金
                                    AddActivityNumForType(ActivityAction.GrowthFund, ChapterId - 1);
                                }
                                break;
                            case GrowthFundType.Super:
                                {
                                    RechargeMng.GrowthFund |= 2;
                                    server.GameDBPool.Call(new QueryUpdateGrowthFund(Uid, RechargeMng.GrowthFund));
                                    AddActivityNumForType(ActivityAction.GrowthFundEx, ChapterId - 1);

                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                //case RechargeType.WeekCard:
                //    {
                //        int seconds = (int)(server.Now().Date.AddDays(rechargeType.Param) - server.Now()).TotalSeconds;
                //        if (Timestamp.TimeStampToDateTime(RechargeMng.WeekCardEnd) < server.Now())
                //        {
                //            RechargeMng.WeekCardStart = Timestamp.GetUnixTimeStampSeconds(server.Now());
                //            RechargeMng.WeekCardEnd = Timestamp.GetUnixTimeStampSeconds(server.Now()) + seconds;
                //        }
                //        else
                //        {
                //            RechargeMng.WeekCardEnd += seconds;
                //        }
                //        server.GameDBPool.Call(new QueryUpdateWeekCard(Uid, RechargeMng.WeekCardStart, RechargeMng.WeekCardEnd));
                //    }
                //    break;
                //case RechargeType.SeasonCard:
                //    {
                //        int seconds = (int)(server.Now().Date.AddDays(rechargeType.Param) - server.Now()).TotalSeconds;
                //        if (Timestamp.TimeStampToDateTime(RechargeMng.SeasonCardTime) < server.Now())
                //        {
                //            seconds += Timestamp.GetUnixTimeStampSeconds(server.Now());
                //        }
                //        else
                //        {
                //            seconds += RechargeMng.MonthCardTime;
                //        }
                //        RechargeMng.SeasonCardTime = seconds;
                //        server.GameDBPool.Call(new QueryUpdateSeasonCard(Uid, RechargeMng.SeasonCardTime));
                //    }
                //    break;

                default:
                    break;
            }
        }

        //public void GetRechargeRewardNew(int rechargeType, ulong rechargeUid, string orderId, float amount, string time, string reward = "")
        //{
        //    //查看充值状态
        //    //string tableName = "recharge_history";
        //    server.GameDBPool.Call(new QueryGetOrderId(Uid, CommonConst.RECHARGE_GET, orderId), ret =>
        //    {
        //        int result = (int)ret;
        //        switch (result)
        //        {
        //            case -2:
        //                //出现异常
        //                Log.ErrorLine("player {0} GetRechargeReward RechargeType {1} QueryGetOrderId {2} error", Uid, rechargeType, orderId);
        //                return;
        //            case 1:
        //                //已经领取过
        //                Log.WarnLine("player {0} GetRechargeReward RechargeType {1} QueryGetOrderId {2} has get reward", Uid, rechargeType, orderId);
        //                return;
        //            case -1:
        //                //没有找到订单
        //                Log.WarnLine("player {0} GetRechargeReward RechargeType {1} QueryGetOrderId {2} not find make state", Uid, rechargeType, orderId);
        //                return;
        //            default:
        //                break;
        //        }

        //        //增加保险
        //        if (TempOrderList.TryGetValue(orderId, out result))
        //        {
        //            if (result == 1)
        //            {
        //                //已经处理过订单
        //                Log.WarnLine("player {0} GetRechargeReward RechargeType {1} TempOrderList {2} has get reward", Uid, rechargeType, orderId);
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            //计算奖励
        //            UpdateRechargeFromHistoryManagerNew(rechargeType, rechargeUid, orderId, amount, time, reward);
        //        }
        //    });
        //}

        //public void UpdateRechargeFromHistoryManagerNew(int rechargeId, ulong rechargeUid, string orderId, float amount, string timeString, string reward)
        //{
        //    //最后获得的钻石
        //    int totalDiamond = 0;
        //    //充值时间用来判断当时是否有活动
        //    DateTime time = DateTime.Parse(timeString);
        //    //判断是否是内部
        //    bool needUpdateDb = CheckOrderIdUpdateDb(orderId);

        //    bool firstRecharge = false;
        //    bool hasDiscount = false;
        //    RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(rechargeId);
        //    if (recharge != null)
        //    {
        //        RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
        //        float money;
        //        //判断是否有折扣价且是首次充值该项
        //        hasDiscount = CheckHasDiscountPrice(rechargeId);
        //        if (hasDiscount)
        //        {
        //            money = price.DiscountMoney;
        //        }
        //        else
        //        {
        //            money = price.Money;
        //        }
        //        //免费领不算入首充
        //        if (price.Price != 0)
        //        {
        //            //是否是首充
        //            firstRecharge = CheckFirstReward();
        //        }

        //        //充值项次数
        //        RechargeItem rechargeItem = RechargeMng.GetRechargeItem(rechargeId);
        //        rechargeItem.Totle += 1;
        //        rechargeItem.Current += 1;

        //        //检查充值奖励
        //        if (needUpdateDb && amount != money)
        //        {
        //            //如果不是测试账单，钱数和配置不符，不添加钻石
        //            Log.Warn("player {0} recharge type {1} money {2} real money {3}", Uid, rechargeId, money, amount);
        //        }
        //        else
        //        {
        //            if (hasDiscount)
        //            {
        //                totalDiamond = price.DiscountDiamond;
        //            }
        //            else
        //            {
        //                totalDiamond = price.Diamond;
        //            }
        //        }
        //        //充值统计
        //        RechargeMng.AccumulateTotal += totalDiamond;
        //        RechargeMng.AccumulateCurrent += totalDiamond;
        //        if (money > RechargeMng.AccumulateOnceMaxMoney)
        //        {
        //            RechargeMng.AccumulateOnceMaxMoney = (int)money;
        //        }

        //        //如果账单到账时间不是今天，那么今天的累加值不增加
        //        if (time >= ZoneServerApi.now.Date)
        //        {
        //            rechargeItem.Daily += 1;
        //            RechargeMng.AccumulateDaily += totalDiamond;
        //        }

        //        //修改状态
        //        TempOrderList.Add(orderId, 1);
        //        //string historyTableName = "recharge_history";
        //        server.GameDBPool.Call(new QueryUpdateOrderId(Uid, CommonConst.RECHARGE_GET, orderId, ZoneServerApi.now));

        //        //累计奖励
        //        //reward = string.Format("{0}|{1}|{2}:{3}|", reward, recharge.Reward, (int)CurrenciesType.diamond, totalDiamond);
        //        reward = string.Format("{0}|{1}|", reward, recharge.Reward);
        //        //首充翻倍用于前端显示
        //        if (CheckIsFirstRechargeThisItem(rechargeId) && !string.IsNullOrEmpty(recharge.ExtraReward))
        //        {
        //            reward = string.Format("{0}|{1}|", reward, recharge.ExtraReward);
        //        }
        //    }

        //    //实际处理
        //    GiftItem giftItem;
        //    UpdateRechargeManagerNew(uid, MainId, rechargeId, rechargeUid, orderId, time, amount, out giftItem, firstRecharge, hasDiscount);

        //    //检查是否还有没获得奖励的订单
        //    RechargeHistoryItem item = CheckRechargeRewards();
        //    if (item != null)
        //    {
        //        //继续处理订单
        //        GetRechargeRewardNew(item.ProductId, rechargeUid, item.OrderId, item.Money, item.Time, reward);
        //    }
        //    else
        //    {
        //        if (giftItem != null)
        //        {
        //            SendRechargeGiftInfo(giftItem, reward);
        //        }
        //    }
        //}

        private void SendRechargeGiftInfo(GiftItem giftItem, string reward)
        {
            MSG_ZGC_RECHARGE_GIFT response = new MSG_ZGC_RECHARGE_GIFT();
            response.GiftItemId = giftItem.Id;
            if (giftItem.Uid != 0)//判断是否是限时礼包
            {
                response.BuyCount = giftItem.BuyCount;
                //用于前端限时礼包的显示
                MSG_ZGC_LIMIT_TIME_GIFTS notify = GiftManager.GenerateOpenedGiftListMsg();
                Write(notify);
            }
            else
            {
                response.BuyCount = giftItem.CurBuyCount;
            }
            response.RewardRatio = giftItem.DiamondRatio;
            response.Discount = giftItem.GetDiscount();
            response.Result = (int)ErrorCode.Success;
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(reward);
            rewards.GenerateRewardItemInfo(response.Rewards);
            Write(response);
        }

        //领取充值奖励
        public void ReceiveRechargeReward(int rechargeItemId)
        {
            MSG_ZGC_RECEIVE_RECHARGE_REWARD response = new MSG_ZGC_RECEIVE_RECHARGE_REWARD();

            RechargeItemModel model = RechargeLibrary.GetRechargeItem(rechargeItemId);
            if (model == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Warn("player {0} receive recharge reward not find recharge gift item {1} in xml", Uid, rechargeItemId);
                return;
            }
            if (!RechargeLibrary.CheckIsRechargeReward(model.GiftType, model.Id))
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Warn("player {0} receive recharge reward failed: itemId {1} is not recharge reward", Uid, rechargeItemId);
                return;
            }
            switch (model.GiftType)
            {
                case RechargeGiftType.FirstRecharge:
                    bool firstResult = UpdateFirstRewardInfo(model, response);
                    if (!firstResult)
                    {
                        return;
                    }
                    break;
                case RechargeGiftType.MonthCard:
                    bool monthResult = UpdateMonthCardInfo(model, response);
                    if (!monthResult)
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }

            GiftItem item;
            SyncUpdateGiftItemInfo(model, out item);

            //发奖
            response.RechargeItemId = model.Id;
            response.FirstDegree = RechargeMng.First;
            response.Result = (int)ErrorCode.Success;
            RewardManager manager = GetSimpleReward(model.Reward, ObtainWay.GetFirstRecharge);
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);

            //通知客户端
            SendRechargeManger();
        }

        private bool CheckCanReceiveFirstReward(RechargeItemModel rechargeItem)
        {
            RechargeGiftType giftType = RechargeLibrary.GetRechargeGiftType(rechargeItem.Id);
            if (RechargeMng.First < 0)
            {
                return false;
            }
            if (giftType != RechargeGiftType.FirstRecharge)
            {
                return false;
            }
            if (RechargeMng.AccumulateTotal < RechargeLibrary.FirstRechargeAccumulate)
            {
                return false;
            }
            int days = (server.Now().Date - server.OpenServerDate).Days + 1;
            //if (days >= RechargeMng.First && RechargeMng.First <= 2)
            //{
            //    return true;
            //}
            int subType = RechargeLibrary.GetRechargeGiftSubType(rechargeItem.Id);
            switch (days)
            {
                case 1:
                    if (subType > (int)FirstRechargeType.FirstDay)
                    {
                        return false;
                    }
                    break;
                case 2:
                    if (subType > (int)FirstRechargeType.SecondDay)
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
            if (!GiftManager.CheckGiftItemHaveBuyCount(rechargeItem))
            {
                return false;
            }
            return true;
        }

        private void UpdateFirstRewardDegree()
        {
            RechargeMng.First++;
        }

        private void SyncDbUpdateFirstRewardDegree()
        {
            server.GameDBPool.Call(new QueryUpdateFirstReward(Uid, RechargeMng.First));
        }

        private bool UpdateFirstRewardInfo(RechargeItemModel model, MSG_ZGC_RECEIVE_RECHARGE_REWARD response)
        {
            if (!CheckCanReceiveFirstReward(model))
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Warn("player {0} can not receive first recharge reward", Uid);
                return false;
            }
            UpdateFirstRewardDegree();
            //同步库
            SyncDbUpdateFirstRewardDegree();
            return true;
        }

        private bool UpdateMonthCardInfo(RechargeItemModel model, MSG_ZGC_RECEIVE_RECHARGE_REWARD response)
        {
            if (!CheckCanReceiveMonthCardReward(model))
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Warn("player {0} can not receive month card reward", Uid);
                return false;
            }
            UpdateMonthCardState(model.SubType);

            SyncDbUpdateMonthCardState(model.SubType);
            return true;
        }

        private bool CheckCanReceiveMonthCardReward(RechargeItemModel rechargeItem)
        {
            //if (RechargeMng.First < 0)
            //{
            //    return false;
            //}
            if (rechargeItem.GiftType != RechargeGiftType.MonthCard)
            {
                return false;
            }
            //超出时效或者今日已领
            switch ((MonthCardType)rechargeItem.SubType)
            {
                case MonthCardType.Normal:
                    if (ZoneServerApi.now > Timestamp.TimeStampToDateTime(RechargeMng.MonthCardTime) || RechargeMng.MonthCardState == 1)
                    {
                        return false;
                    }
                    break;
                case MonthCardType.Super:
                    if (ZoneServerApi.now > Timestamp.TimeStampToDateTime(RechargeMng.SuperMonthCardTime) || RechargeMng.SuperMonthCardState == 1)
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        private void UpdateMonthCardState(int subType)
        {
            RechargeMng.UpdateMonthCardState(subType);
        }

        private void SyncDbUpdateMonthCardState(int subType)
        {
            switch ((MonthCardType)subType)
            {
                case MonthCardType.Normal:
                    server.GameDBPool.Call(new QueryUpdateMonthCardState(Uid, RechargeMng.MonthCardState));
                    break;
                case MonthCardType.Super:
                    server.GameDBPool.Call(new QueryUpdateSuperMonthCardState(Uid, RechargeMng.SuperMonthCardState));
                    break;
                default:
                    break;
            }
        }

        private bool CheckCanReceiveCommonGiftReward(RechargeItemModel rechargeItem)
        {
            switch (rechargeItem.GiftType)
            {
                case RechargeGiftType.Common:
                    return GiftManager.CheckGiftItemHaveBuyCount(rechargeItem);
                case RechargeGiftType.NewRechargeGift:
                    {
                        //检查是否活动开启
                        RechargeGiftModel activityModel;
                        if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NewRechargeGift, ZoneServerApi.now, out activityModel))
                        {
                            Log.Warn($"player {Uid} CheckCanReceiveCommonGiftReward failed: not open");
                            return false;
                        }
                        int period = activityModel.SubType;
                        if (rechargeItem.SubType != period)
                        {
                            Log.Warn($"player {Uid} CheckCanReceiveCommonGiftReward failed: rewardId {rechargeItem.Id} not cur period {period}");
                            return false;
                        }
                    }

                    return GiftManager.CheckGiftItemHaveBuyCount(rechargeItem);
                default:
                    return false;
            }
        }

        private void SyncUpdateGiftItemInfo(RechargeItemModel recharge, out GiftItem item, ulong rechargeUid = 0)
        {
            item = null;
            if (GiftManager.CheckHaveThisTypeGift(recharge))
            {
                //更新礼包信息
                item = UpdateGiftItem(recharge);
                if (item == null)
                {
                    return;
                }
                if (recharge.GiftType == RechargeGiftType.LimitTime)
                {
                    string rewards = string.Format("{0}|{1}", recharge.Reward, recharge.ExtraReward);
                    server.GameDBPool.Call(new QueryUpdateLimitTimeGift(item.Uid, item.BuyCount, rewards));
                }
                else
                {
                    server.GameDBPool.Call(new QueryUpdateGiftItemBuyCount(Uid, recharge.GiftType, GiftManager.BuildGiftItemIdString(recharge.GiftType), GiftManager.BuildGiftBuyCountString(recharge.GiftType), GiftManager.BuildGiftCurBuyCountString(recharge.GiftType), GiftManager.BuildGiftDoubleFlagString(recharge.GiftType), GiftManager.BuildGiftDiscountString(recharge.GiftType), GiftManager.BuildDiamondRatioString(recharge.GiftType)));
                }
            }
            else
            {
                //更新礼包信息
                item = UpdateGiftItem(recharge);
                if (item == null)
                {
                    return;
                }
                server.GameDBPool.Call(new QueryInsertGiftItem(Uid, recharge.GiftType, GiftManager.BuildGiftItemIdString(recharge.GiftType), GiftManager.BuildGiftBuyCountString(recharge.GiftType), GiftManager.BuildGiftCurBuyCountString(recharge.GiftType), GiftManager.BuildGiftDoubleFlagString(recharge.GiftType), GiftManager.BuildGiftDiscountString(recharge.GiftType), GiftManager.BuildDiamondRatioString(recharge.GiftType)));
            }
        }

        /// <summary>
        /// 使用代币
        /// </summary>
        public void UseRechargeToken(ulong uid, int giftItemId, ulong giftUid)
        {
            MSG_ZGC_USE_RECHARGE_TOKEN response = new MSG_ZGC_USE_RECHARGE_TOKEN();

            BaseItem item = BagManager.GetItem(uid);
            if (item == null)
            {
                Log.Warn("player {0} use item recharge token {1} faield: not find item", Uid, uid);
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }
            //验有没有这个充值项
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItemOrSdkItem(giftItemId);
            if (recharge == null)
            {
                Log.Warn("player {0} use item recharge token faield: not find recharge item {1}", Uid, giftItemId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //检查是否是充值奖励
            if (RechargeLibrary.CheckIsRechargeReward(recharge.GiftType, recharge.Id))
            {
                Log.Warn("player {0} use item recharge token faield: item {1} is recharge reward can not buy", Uid, giftItemId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //检查是否可以购买
            if (!GiftManager.CheckGiftItemHaveBuyCount(recharge))
            {
                Log.Warn("player {0} use item recharge token faield: item {1} buy count not enough", Uid, giftItemId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (!CheckCanBuyPassCard(recharge))
            {
                Log.Warn("player {0} use item recharge token faield: passcard {1} already bought this period", Uid, giftItemId);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //比对价格
            RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
            RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(recharge.DiscountRechargeId);
            bool hasDiscount = CheckHasDiscountPrice(recharge, discountPrice);
            if (hasDiscount)
            {
                if (discountPrice.Price != RechargeLibrary.GetRechargeTokenPrice(item.Id))
                {
                    Log.Warn("player {0} use item recharge token {1} faield: discount price error {2}", Uid, item.Id, discountPrice.Price);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            }
            else if (price.Price != RechargeLibrary.GetRechargeTokenPrice(item.Id))
            {
                Log.Warn("player {0} use item recharge token {1} faield: price error {2}", Uid, item.Id, price.Price);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            response.Result = (int)UseRechargeToken(item, 1);

            if (response.Result == (int)ErrorCode.Success)
            {
                DoRechargeToken(giftItemId, hasDiscount);
            }
            Write(response);
        }

        public void DoRechargeToken(int giftId, bool hasDiscount)
        {
            MSG_ZM_RECHARGE_TOKEN msg = new MSG_ZM_RECHARGE_TOKEN();
            msg.GiftId = giftId;
            msg.HasDicount = hasDiscount;
            server.ManagerServer.Write(msg, Uid);
        }

        private void SyncDbInsertOrder(GiftItem item, float money)
        {
            string orderId = "rechargeToken";
            string uuid = Guid.NewGuid().ToString("N");
            orderId = orderId + uuid;
            server.GameDBPool.Call(new QueryMakeOrderId(Uid, orderId, server.Now(), item.Id, money, (int)RechargeWay.Token));
        }

        private ErrorCode UseRechargeToken(BaseItem item, int num)
        {
            ErrorCode errorCode = ErrorCode.Fail;

            if (!CheckItemInfo(item, num, ref errorCode))
            {
                return errorCode;
            }
            else
            {
                if (item.MainType != MainType.Consumable)
                {
                    Log.Warn($"player {Uid} use recharge token failed: mainType is not consumble");
                    return errorCode;
                }
                ItemModel model = BagLibrary.GetItemModel(item.Id);
                if (model == null)
                {
                    Log.Warn($"player {Uid} use recharge token have not model ItemModel item id {item.Id}");
                    return ErrorCode.Fail;
                }
                if (Level < model.LevelLimit)
                {
                    Log.Warn($"player {Uid} use recharge token id {item.Id} levellimit ");
                    return ErrorCode.UseItemLevelLimt;
                }
                NormalItem normalItem = item as NormalItem;
                if ((ConsumableType)normalItem.SubType != ConsumableType.RechargeToken)
                {
                    Log.Warn($"player {Uid} use item is not recharge token, item id {item.Id}");
                    return ErrorCode.Fail;
                }
                Log.Write("player {0} Use Id:{1} Num:{2}", Uid, item.Id, num);

                BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, num, ConsumeWay.ItemUse);

                if (baseItem != null)
                {
                    SyncClientItemInfo(item);
                    //使用消耗品
                    AddTaskNumForType(TaskType.UseConsumable, 1, true, normalItem.SubType);
                }
            }
            return ErrorCode.Success;
        }

        /// <summary>
        /// 刷新月卡领取状态
        /// </summary>
        private void RefreshMonthCardReceiveState()
        {
            RechargeMng.RefreshMonthCardReceiveState();
            SyncDbUpdateMonthCardReceiveState();
            //通知客户端
            SendRechargeManger();
        }

        private void SyncDbUpdateMonthCardReceiveState()
        {
            server.GameDBPool.Call(new QueryUpdateRefreshMonthCardState(Uid, RechargeMng.MonthCardState, RechargeMng.SuperMonthCardState));
        }

        private bool CheckHasDiscountPrice(RechargeItemModel recharge, RechargePriceModel discountPrice)
        {
            return GiftManager.CheckHasDiscountPrice(recharge, discountPrice);
        }

        public float GetTotalOnhookGoldAddRatio()
        {
            float addRatio = GetContributionOnhookRatio();
            if (ZoneServerApi.now <= Timestamp.TimeStampToDateTime(RechargeMng.SuperMonthCardTime))
            {
                addRatio += RechargeLibrary.OnhookGoldUp;
            }
            return addRatio;
        }

        public void MonthCardUpOnhookRewards(RewardManager manager)
        {
            bool monthCard = true;
            float addRatio = GetContributionOnhookRatio();

            if (ZoneServerApi.now > Timestamp.TimeStampToDateTime(RechargeMng.SuperMonthCardTime))
            {
                monthCard = false;
            }

            int rewardType = (int)RewardType.Currencies;
            int god = (int)CurrenciesType.gold;
            int soulPower = (int)CurrenciesType.soulPower;

            foreach (var item in manager.AllRewards)
            {
                if (item.RewardType == rewardType)
                {
                    if (item.Id == god && monthCard)
                    {
                        item.Num += (int)(item.Num * (addRatio + RechargeLibrary.OnhookGoldUp));
                        continue;
                    }
                    else if (item.Id == soulPower && monthCard)
                    {
                        item.Num += (int)(item.Num * (addRatio + RechargeLibrary.OnhookSoulPowerUp));
                        continue;
                    }
                }

                item.Num += (int)(item.Num * addRatio);
            }

            //Dictionary<int, int> currenciesList;
            //if (manager.RewardList.TryGetValue(RewardType.Currencies, out currenciesList))
            //{
            //    int goldReward = currenciesList[(int)CurrenciesType.gold];
            //    int soulPowerRewrd = currenciesList[(int)CurrenciesType.soulPower];
            //    foreach (var item in manager.AllRewards)
            //    {
            //        if ((RewardType)item.RewardType != RewardType.Currencies)
            //        {
            //            continue;
            //        }
            //        if ((CurrenciesType)item.Id == CurrenciesType.gold)
            //        {
            //            item.Num += (int)(goldReward * RechargeLibrary.OnhookGoldUp);
            //            manager.AddBreakupRewardSpecial(item);
            //        }
            //        else if ((CurrenciesType)item.Id == CurrenciesType.soulPower)
            //        {
            //            manager.AddBreakupRewardSpecial(item);
            //        }
            //    }
            //}
        }

        public void UpdateSpecialGiftInfo(RechargeItemModel recharge, GiftItem giftItem)
        {
            if (giftItem == null)
            {
                return;
            }

            switch (recharge.GiftType)
            {
                case RechargeGiftType.PassCard:
                    //if (recharge.GiftType == RechargeGiftType.PassCard)
                    {
                        MSG_ZGC_PASSCARD_RECHARGE_RESULT response = PassCardMng.BuyPasscard();
                        Write(response);
                    }
                    break;
                case RechargeGiftType.ThemePass:
                    //if (recharge.GiftType == RechargeGiftType.ThemePass)
                    {
                        MSG_ZGC_BUY_THEMEPASS_RESULT response = ThemePassMamager.BuyThemePass(recharge.SubType);
                        Write(response);
                    }
                    break;
                case RechargeGiftType.NewRechargeGift:
                    {
                        RechargeGiftModel activityModel;
                        if (RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NewRechargeGift, ZoneServerApi.now, out activityModel))
                        {
                            //增加积分
                            RechargeMng.NewRechargeGiftScore += RechargeLibrary.GetNewRechargGifteScore(recharge.Id);
                            //SendRechargeManger();
                            BIRecordPointGameLog(RechargeLibrary.GetNewRechargGifteScore(recharge.Id), RechargeMng.NewRechargeGiftScore, "new_recharge_gift", activityModel.SubType);
                        }
                    }
                    break;
                case RechargeGiftType.DaysRecharge:
                    daysRechargeManager.UpdateRechargeInfo(recharge);
                    break;
                default:
                    break;
            }
        }

        public bool CheckCanBuyPassCard(RechargeItemModel model)
        {
            if (model.GiftType == RechargeGiftType.PassCard)
            {
                if (PassCardMng.CheckHaveBoughtThisPeriod())
                {
                    MSG_ZGC_PASSCARD_RECHARGE_RESULT res = new MSG_ZGC_PASSCARD_RECHARGE_RESULT();
                    res.BoughtPasscard = false;
                    res.ErrorCode = (int)ErrorCode.Already;
                    Log.Warn("player {0} buy passcard already bought period {1}", Uid, PassCardMng.CurPeriod);
                    Write(res);
                    return false;
                }
            }
            else if (model.GiftType == RechargeGiftType.ThemePass)
            {
                if (!ThemePassMamager.CheckCanBuyThemePass(model.SubType))
                {
                    MSG_ZGC_BUY_THEMEPASS_RESULT response = new MSG_ZGC_BUY_THEMEPASS_RESULT();
                    response.ThemeType = model.SubType;
                    response.Bought = false;
                    response.Result = (int)ErrorCode.Already;
                    Log.Warn("player {0} buy theme pass already bought themeType {1}", Uid, model.SubType);
                    Write(response);
                    return false;
                }
            }
            return true;
        }

        internal void SetRecentAccumulateOnceMaxMoney(int money, bool isNormal = false)
        {
            RechargeManager.AccumulateOnceMaxMoney = money;
            server.GameDBPool.Call(new QueryUpdateRechargeAccumulateOnceMaxMoney(Uid, RechargeMng.First));

            if (isNormal)
            {
                SetLastCommonRechargeTime(server.Now());
            }
        }

        internal void SetLastCommonRechargeTime(DateTime time)
        {
            RechargeManager.LastCommonRechargeTime = Timestamp.GetUnixTimeStampSeconds(time);
            SyncDBLastCommonRechargeTime(time);
        }

        private void SyncDBLastCommonRechargeTime(DateTime time)
        {
            server.GameDBPool.Call(new QueryUpdateLastCommonRechargeTime(Uid, RechargeMng.First));
        }

        public ErrorCode CheckCanActivateMonthCard(int itemId)
        {
            int rechargeItemId = RechargeLibrary.GetMonthCardRechargeItem(itemId);
            if (rechargeItemId == 0)
            {
                return ErrorCode.Fail;
            }
            //验有没有这个充值项
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(rechargeItemId);
            if (recharge == null)
            {
                Log.Warn("player {0} use item activate month card faield: not find recharge item {1}", Uid, rechargeItemId);
                return ErrorCode.Fail;
            }
            //检查是否可以购买
            if (!CheckCanActivateMonthCard(recharge))
            {
                Log.Warn("player {0} use item activate month card faield: month card {1} already activated", Uid, recharge.Id);
                return ErrorCode.MonthCardOpened;
            }

            if (!GiftManager.CheckGiftItemHaveBuyCount(recharge))
            {
                Log.Warn("player {0} use item activate month card faield: item {1} buy count not enough", Uid, recharge.Id);
                return ErrorCode.MonthCardOpened;
            }
            return ErrorCode.Success;
        }

        public void ActivateMonthCard(int itemId)
        {
            int rechargeItemId = RechargeLibrary.GetMonthCardRechargeItem(itemId);
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(rechargeItemId);
            RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(recharge.DiscountRechargeId);
            bool hasDiscount = CheckHasDiscountPrice(recharge, discountPrice);

            MSG_ZM_ACTIVATE_ITEM msg = new MSG_ZM_ACTIVATE_ITEM();
            msg.GiftId = recharge.Id;
            msg.HasDicount = hasDiscount;
            server.ManagerServer.Write(msg, Uid);
        }

        private bool CheckCanActivateMonthCard(RechargeItemModel rechargeItem)
        {
            if (rechargeItem.GiftType != RechargeGiftType.MonthCard)
            {
                return false;
            }
            switch ((MonthCardType)rechargeItem.SubType)
            {
                case MonthCardType.Normal:
                    return !CheckMonthCardState();
                case MonthCardType.Super:
                    return !CheckSuperMonthCardState();
                default:
                    break;
            }
            return false;
        }

        //领取累积充值奖励
        public void GetAccumulateRechargeReward(int rewardId)
        {
            MSG_ZGC_GET_ACCUMULATE_RECHARGE_REWARD response = new MSG_ZGC_GET_ACCUMULATE_RECHARGE_REWARD();
            response.Id = rewardId;

            AccumulateRechargeModel accumulateRecharge = RechargeLibrary.GetAccumulateRechargeById(rewardId);
            if (accumulateRecharge == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get accumulate recharge reward {rewardId} failed: not find int xml");
                Write(response);
                return;
            }
            if (RechargeMng.AccumulatePrice < accumulateRecharge.AccumulatePrice)
            {
                response.Result = (int)ErrorCode.NotReach;
                Log.Warn($"player {Uid} get accumulate recharge reward {rewardId} failed: accumulate price {RechargeMng.AccumulatePrice} not enough");
                Write(response);
                return;
            }
            //开服第几天
            int day = (int)(server.Now().Date - server.OpenServerDate).TotalDays;
            if (day < RechargeLibrary.AccumulateRechargeOpenDay)
            {
                response.Result = (int)ErrorCode.NotReachTime;
                Log.Warn($"player {Uid} get accumulate recharge reward {rewardId} failed: activity not open on day {day} yet");
                Write(response);
                return;
            }
            string[] gotRewards = StringSplit.GetArray("|", RechargeMng.AccumulateRechargeRewards);
            if (gotRewards.Contains(rewardId.ToString()))
            {
                response.Result = (int)ErrorCode.AlreadyReceived;
                Log.Warn($"player {Uid} get accumulate recharge reward {rewardId} failed: already got");
                Write(response);
                return;
            }
            RechargeMng.AccumulateRechargeRewards += rewardId + "|";
            SyncDbUpdateAccumulateRechargeGotRewards();

            //发奖
            response.Result = (int)ErrorCode.Success;
            RewardManager manager = GetSimpleReward(accumulateRecharge.Rewards, ObtainWay.GetAccumulateRechargeReward);
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);
        }


        //领取新直购充值奖励
        public void GetNewRechargeGiftAccumulateReward(int rewardId)
        {
            MSG_ZGC_GET_NEW_RECHARGE_GIFT_REWARD response = new MSG_ZGC_GET_NEW_RECHARGE_GIFT_REWARD();
            response.Id = rewardId;

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NewRechargeGift, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get nine test score reward failed: not open");
                Write(response);
                return;
            }
            int period = activityModel.SubType;

            ScoreRewardModel accumulateRecharge = RechargeLibrary.GetNewRechargGifteItem(rewardId);
            if (accumulateRecharge == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get new recharge gift accumulate reward {rewardId} failed: not find int xml");
                Write(response);
                return;
            }

            if (accumulateRecharge.Period != period)
            {
                Log.Warn($"player {Uid} get new recharge gift accumulate reward failed: rewardId {rewardId} not cur period {period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (RechargeMng.NewRechargeGiftScore < accumulateRecharge.Score)
            {
                response.Result = (int)ErrorCode.NotReach;
                Log.Warn($"player {Uid} get new recharge gift accumulate reward {rewardId} failed: accumulate price {RechargeMng.AccumulatePrice} not enough");
                Write(response);
                return;
            }
  
            string[] gotRewards = StringSplit.GetArray("|", RechargeMng.NewRechargeGiftRewards);
            if (gotRewards.Contains(rewardId.ToString()))
            {
                response.Result = (int)ErrorCode.AlreadyReceived;
                Log.Warn($"player {Uid} get new recharge gift accumulate reward {rewardId} failed: already got");
                Write(response);
                return;
            }

            RechargeMng.NewRechargeGiftRewards += rewardId + "|";
            SyncDbUpdateNewRechargeGiftRewards();

            //发奖
            response.Result = (int)ErrorCode.Success;
            RewardManager manager = GetSimpleReward(accumulateRecharge.Reward, ObtainWay.NewRechargeGiftRewards);
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);
        }

        private void SyncDbUpdateAccumulateRechargeGotRewards()
        {
            server.GameDBPool.Call(new QueryUpdateAccumulateRechargeGotRewards(Uid, RechargeMng.AccumulateRechargeRewards));
        }

        //重置钻石礼包双倍
        public void ResetDiamondGiftDoubleFlag()
        {
            GiftManager.ResetDiamondGiftDoubleFlag();
        }

        private void SyncDbUpdateNewRechargeGiftRewards()
        {
            server.GameDBPool.Call(new QueryUpdateNewRechargeGiftRewards(Uid, RechargeMng.NewRechargeGiftRewards));
        }

        //改变钻石礼包倍率
        public bool ChangeDiamondGiftRatio(NormalItem item)
        {
            DiamondRatioCardInfo cardInfo = GiftLibrary.GetDiamondRatioChangeCardInfo(item.Id);
            if (cardInfo == null) return false;
            RechargeItemModel rechargeItem = RechargeLibrary.GetRechargeItem(cardInfo.RechargeId);
            if (rechargeItem == null || rechargeItem.GiftType != RechargeGiftType.Common || rechargeItem.SubType != (int)CommonGiftType.Diamond)return false;
            
            if (GiftManager.ChangeDiamondGiftRatio(rechargeItem, cardInfo.Ratio))
            {
                MSG_ZGC_RESET_DOUBLE_FLAG notify = new MSG_ZGC_RESET_DOUBLE_FLAG();
                notify.Items.Add(new ZGC_DOUBLE_RECHARGE_ITEM() { GiftItemId = rechargeItem.Id, RewardRatio = cardInfo.Ratio });
                notify.Result = (int)ErrorCode.Success;
                Write(notify);
                return true;
            }
            return false;
        }

        public string GetFinalRechargeRewards(RewardManager rewardManager)
        {
            string rewards = "";
            foreach (ItemBasicInfo reward in rewardManager.AllRewards)
            {
                rewards += $"{reward.Id}:{reward.RewardType}:{reward.Num}|";
            }

            return rewards;
        }

        public void LoadFirstOrderInfo(ZMZ_FIRST_ORDER msg)
        {
            RechargeHistoryItem info = new RechargeHistoryItem()
            {
                OrderId = msg.OrderId,
                ProductId = msg.ProductId,
                Money = msg.Money,
                Time = msg.CreateTime,
                OrderInfo = msg.OrderInfo
            };
            RechargeMng.BindFirstOrderInfo(info);
        }

        public ZMZ_FIRST_ORDER GenerateFirstOrderInfo()
        {
            ZMZ_FIRST_ORDER msg = new ZMZ_FIRST_ORDER()
            {
                OrderId = RechargeMng.FirstOrderInfo.OrderId,
                ProductId = RechargeMng.FirstOrderInfo.ProductId,
                Money = RechargeMng.FirstOrderInfo.Money,
                CreateTime = RechargeMng.FirstOrderInfo.Time,
                OrderInfo = RechargeMng.FirstOrderInfo.OrderInfo
            };
            return msg;
        }

        /// <summary>
        /// 不同渠道支付方式处理
        /// </summary>
        /// <param name="payMode">支付方式</param>
        public void WebPayModeTypeActionLogic(int payMode, RechargeItemModel rechargeItem, float price)
        {
            switch (payMode)
            {
                //雷蛇               
                case 107:
                    UpdateWebPayRebateRechargeInfo(rechargeItem.Reward, price);
                    break;
                default:
                    break;
            }
        }
    }
}
