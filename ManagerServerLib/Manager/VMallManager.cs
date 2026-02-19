//using DBUtility;
//using EnumerateUtility;
//using Logger;
//using Message.Manager.Protocol.MZ;
//using ServerModels;
//using ServerModels.Recharge;
//using ServerShared;
//using System;
//using System.Collections.Generic;

//namespace ManagerServerLib
//{
//    public class VMallManager
//    {
//        private ManagerServerApi server { get; set; }
//        private readonly long tempMainId;
//        private int CurMaxOrderId;

//        public VMallManager(ManagerServerApi server)
//        {
//            this.server = server;
//            tempMainId = server.MainId * CONST.RechargeOrderTempNum;
//        }

//        private void Init()
//        {
//            //加载微商城充值发货历史信息
//            QueryGetMaxVMallRechargeOrderId query = new QueryGetMaxVMallRechargeOrderId();
//            server.GameDBPool.Call(query, ret =>
//            {
//                if (query.OrderId > tempMainId)
//                {
//                    CurMaxOrderId = GetHistoryId(query.OrderId);
//                }
//                Log.Write("Init CurMaxOrderId id : " + CurMaxOrderId);
//            });

//            //TODO:BOIL 未完成订单信息

//        }

//        public long GetOrderId(int historyId)
//        {
//            return historyId + tempMainId;
//        }

//        public int GetHistoryId(long orderId)
//        {
//            return (int)(orderId - tempMainId);
//        }

//        //public void UpdateRechargeManager(int historyId, string orderInfo, DateTime time, float amount, RechargeWay way)
//        //{
//        //    long orderId = GetOrderId(historyId);
//        //    RepairOrderInfo info = GetRepairOrderInfo(orderId);
//        //    //判断当前是否有这个订单在处理
//        //    if (info != null)
//        //    {
//        //        int pcUid = info.Uid;
//        //        int giftId = info.Gift;
//        //        UpdateRechargeManagerNew(historyId, orderId, pcUid, giftId, orderInfo, time, amount, way);
//        //    }
//        //    else
//        //    {
//        //        //未找到订单
//        //        Log.Error($"RechargeManager not find info id {historyId} order {orderInfo} time {time} money {amount} way {way}");
//        //    }
//        //}
//        //public void UpdateRechargeManager(int historyId, string orderInfo, DateTime time, float amount, RechargeWay way)
//        //{
//        //    long orderId = GetOrderId(historyId);
//        //    //判断当前是否有这个订单在处理
//        //    if (!playerRechargeList.ContainsKey(orderId))
//        //    {
//        //        playerRechargeList.Add(orderId, true);

//        //        QueryGetOrderInfo queryGetOrderInfo = new QueryGetOrderInfo(orderId);
//        //        server.GameDBPool.Call(queryGetOrderInfo, ret =>
//        //        {
//        //            try
//        //            {
//        //                if (queryGetOrderInfo.FindOrder)
//        //                {
//        //                    //找到订单
//        //                    if (queryGetOrderInfo.MakeState == 0 && queryGetOrderInfo.PcUid > 0)
//        //                    {
//        //                        int pcUid = queryGetOrderInfo.PcUid;
//        //                        int giftId = queryGetOrderInfo.GiftId;
//        //                        UpdateRechargeManagerNew(orderId, pcUid, giftId, orderInfo, time, amount, way);
//        //                    }
//        //                    else
//        //                    {
//        //                        //未找到订单
//        //                        Log.Warn($"RechargeManager has make id {orderId} order {orderInfo} time {time} money {amount} way {way}");
//        //                    }
//        //                }
//        //                else
//        //                {
//        //                    //未找到订单
//        //                    Log.Error($"RechargeManager not find id {orderId} order {orderInfo} time {time} money {amount} way {way}");
//        //                }
//        //            }
//        //            catch (Exception e)
//        //            {
//        //                Log.Error($"RechargeManager not find id {orderId} order {orderInfo} time {time} money {amount} way {way}: {e.ToString()}");
//        //            }
//        //            finally
//        //            {
//        //                playerRechargeList.Remove(orderId);
//        //            }
//        //        });
//        //    }
//        //}
//        //public void UpdateRechargeManagerNew(int historyId, long orderId, int pcUid, int giftId, string orderInfo, DateTime time, float amount, RechargeWay way)
//        //{
//        //    Log.Write($"player {pcUid} recharge {orderId} rechargeId {giftId} order {orderInfo} time {time} way {way}", pcUid, giftId, orderInfo, time, way);
//        //    //比对信息
//        //    RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(giftId);
//        //    if (recharge == null)
//        //    {
//        //        Log.Error($"player {pcUid} recharge {orderId} rechargeId {giftId} order {orderInfo} get error, not find recharge item in xml");
//        //        return;
//        //    }
//        //    RechargePriceModel price = RechargeLibrary.GetRechargePrice(recharge.RechargeId);
//        //    if (price == null)
//        //    {
//        //        Log.Error($"player {pcUid} recharge {orderId} rechargeId {giftId} order {orderInfo} get error, not find recharge price {recharge.RechargeId} in xml");
//        //        return;
//        //    }
//        //    RechargePriceModel discountPrice = RechargeLibrary.GetRechargePrice(recharge.DiscountRechargeId);
//        //    float discountAmount = 0;
//        //    if (discountPrice != null)
//        //    {
//        //        discountAmount = discountPrice.Money;
//        //    }
//        //    if (amount <= 0)
//        //    {
//        //        Log.Error($"player {pcUid} recharge {orderId} rechargeId {giftId} order {orderInfo} get error with amount {price.Money} discountAmount {discountAmount} and realAmount {amount}");
//        //        return;
//        //    }
//        //    //修改订单状态
//        //    server.GameDBPool.Call(new QueryMakeOrderInfo(orderId, orderInfo, time, amount, (int)way));
//        //    RepairOderList.Remove(historyId);


//        //    //通知Zone发奖
//        //    MSG_MZ_UPDATE_RECHARGE msg = new MSG_MZ_UPDATE_RECHARGE();
//        //    msg.OrderId = orderId;
//        //    msg.Uid = pcUid;
//        //    msg.RechargeId = giftId;
//        //    msg.Money = amount;
//        //    msg.Way = (int)way;
//        //    msg.OrderInfo = orderInfo;
//        //    server.ZoneServerManager.Broadcast(msg);
//        //}
//    }
//}
