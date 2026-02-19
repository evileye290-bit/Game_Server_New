using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Manager.Protocol.MZ;
using ServerModels;
using ServerModels.Recharge;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ManagerServerLib
{
    public class RechargeManager
    {
        //private Dictionary<long, bool> playerRechargeList = new Dictionary<long, bool>();

        public Dictionary<long, RepairOrderInfo> RepairOderList = new Dictionary<long, RepairOrderInfo>();

        public int RechargeHistoryId { get; set; }
        private long tempMainId { get; set; }
        private ManagerServerApi server { get; set; }
        public RechargeManager(ManagerServerApi server)
        {
            this.server = server;

            tempMainId = server.MainId * CONST.RechargeOrderTempNum;

            InitHistoryId();
        }

        private void InitHistoryId()
        {
            QueryGetMaxRechargeHistoryId query = new QueryGetMaxRechargeHistoryId(server.MainId);
            server.GameDBPool.Call(query, ret =>
            {
                if (query.HistoryId > tempMainId)
                {
                    RechargeHistoryId = GetHistoryId(query.HistoryId);
                }
                Log.Write("InitRechargeHistoryId max id : " + RechargeHistoryId);
            });

            //QueryGetMaxVWallHistoryId vQuery = new QueryGetMaxVWallHistoryId();
            //server.GameDBPool.Call(vQuery, ret =>
            //{
            //    if (vQuery.HistoryId > tempMainId)
            //    {
            //        vwallHistoryId = GetHistoryId(vQuery.HistoryId);
            //    }
            //    Log.Write("InitRechargeHistoryId max id : " + vwallHistoryId);
            //});

            QueryGetMakeRechargeList makeQuery = new QueryGetMakeRechargeList();
            server.GameDBPool.Call(makeQuery, ret =>
            {
                foreach (var item in makeQuery.historys)
                {
                    RepairOderList.Add(item.Key, item.Value);
                }
                Log.Write("InitRechargeHistory count: " + RepairOderList.Count);
            });
        }

        public int GetNewHistoryId()
        {
            return ++RechargeHistoryId;
        }
        //public int GetNewVWallHistoryId()
        //{
        //    return ++vwallHistoryId;
        //}
        public void SaveHistoryId(int uid, int giftId, int historyId, int payMode)
        {
            RepairOrderInfo info = new RepairOrderInfo();
            info.Uid = uid;
            info.Gift = giftId;
            //info.HistoryId = historyId;
            long orderId = GetOrderId(historyId);
            RepairOderList[orderId] = info;

            server.GameDBPool.Call(new QueryInsterNewOrderId(orderId, uid, giftId, server.Now(), payMode));
        }

        public RepairOrderInfo GetRepairOrderInfo(long orderId)
        {
            RepairOrderInfo info;
            RepairOderList.TryGetValue(orderId, out info);
            return info;
        }

        public long GetOrderId(int historyId)
        {
            return historyId + tempMainId;
        }

        public int GetHistoryId(long orderId)
        {
            string id = orderId.ToString().Substring(5);
            return int.Parse(id);
        }

        public void UpdateRechargeManager(int historyId, string orderInfo, DateTime time, float amount, string payCurrency, RechargeWay way, string isSandbox, string payMode)
        {
            long orderId = GetOrderId(historyId);
            RepairOrderInfo info = GetRepairOrderInfo(orderId);
            //判断当前是否有这个订单在处理
            if (info != null)
            {
                int pcUid = info.Uid;
                int giftId = info.Gift;
                UpdateRechargeManagerNew(historyId, orderId, pcUid, giftId, orderInfo, time, amount, payCurrency, way, isSandbox, payMode);
            }
            else
            {
                //未找到订单
                Log.Error($"RechargeManager not find info id {historyId} order {orderInfo} time {time} money {amount} way {way}");
            }
        }
        //public void UpdateRechargeManager(int historyId, string orderInfo, DateTime time, float amount, RechargeWay way)
        //{
        //    long orderId = GetOrderId(historyId);
        //    //判断当前是否有这个订单在处理
        //    if (!playerRechargeList.ContainsKey(orderId))
        //    {
        //        playerRechargeList.Add(orderId, true);

        //        QueryGetOrderInfo queryGetOrderInfo = new QueryGetOrderInfo(orderId);
        //        server.GameDBPool.Call(queryGetOrderInfo, ret =>
        //        {
        //            try
        //            {
        //                if (queryGetOrderInfo.FindOrder)
        //                {
        //                    //找到订单
        //                    if (queryGetOrderInfo.MakeState == 0 && queryGetOrderInfo.PcUid > 0)
        //                    {
        //                        int pcUid = queryGetOrderInfo.PcUid;
        //                        int giftId = queryGetOrderInfo.GiftId;
        //                        UpdateRechargeManagerNew(orderId, pcUid, giftId, orderInfo, time, amount, way);
        //                    }
        //                    else
        //                    {
        //                        //未找到订单
        //                        Log.Warn($"RechargeManager has make id {orderId} order {orderInfo} time {time} money {amount} way {way}");
        //                    }
        //                }
        //                else
        //                {
        //                    //未找到订单
        //                    Log.Error($"RechargeManager not find id {orderId} order {orderInfo} time {time} money {amount} way {way}");
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Log.Error($"RechargeManager not find id {orderId} order {orderInfo} time {time} money {amount} way {way}: {e.ToString()}");
        //            }
        //            finally
        //            {
        //                playerRechargeList.Remove(orderId);
        //            }
        //        });
        //    }
        //}
        public void UpdateRechargeManagerNew(int historyId, long orderId, int pcUid, int giftId, string orderInfo, DateTime time, float amount, string payCurrency, RechargeWay way, string isSandbox, string payMode)
        {
            Log.Info($"player {pcUid} recharge {orderId} rechargeId {giftId} order {orderInfo} time {time} way {way} amount {amount} payCurrency {payCurrency} isSandbox {isSandbox} payMode {payMode}");
            //比对信息
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItemOrSdkItem(giftId);
            if (recharge == null)
            {
                Log.Error($"player {pcUid} recharge {orderId} rechargeId {giftId} order {orderInfo} get error, not find recharge item in xml");
                return;
            }
            RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
            if (price == null)
            {
                Log.Error($"player {pcUid} recharge {orderId} rechargeId {giftId} order {orderInfo} get error, not find recharge price {recharge.RechargeId} in xml");
                return;
            }
            RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(recharge.DiscountRechargeId);
            float discountAmount = 0;
            if (discountPrice != null)
            {
                discountAmount = discountPrice.Money;
            }
            //if (amount <= 0)
            //{
            //    Log.Error($"player {pcUid} recharge {orderId} rechargeId {giftId} order {orderInfo} get error with amount {price.Money} discountAmount {discountAmount} and realAmount {amount}");
            //    return;
            //}
            //修改订单状态
            server.GameDBPool.Call(new QueryMakeOrderInfo(orderId, orderInfo, time, amount, (int)way, int.Parse(payMode)));
            RepairOderList.Remove(orderId);


            //通知Zone发奖
            MSG_MZ_UPDATE_RECHARGE msg = new MSG_MZ_UPDATE_RECHARGE();
            msg.OrderId = orderId;
            msg.Uid = pcUid;
            msg.RechargeId = giftId;
            msg.Money = amount;
            msg.PayCurrency = payCurrency;
            msg.Way = (int)way;
            msg.OrderInfo = orderInfo;
            msg.Num = 1;
            msg.IsSandbox = isSandbox;
            msg.PayMode = payMode;
            server.ZoneServerManager.Broadcast(msg);
        }
    }
}
