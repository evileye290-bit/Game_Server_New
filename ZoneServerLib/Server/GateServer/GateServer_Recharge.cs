using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        //public void OnResponse_SaveOrderId(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_SAVE_ORDER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SAVE_ORDER>(stream);
        //    PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
        //    if (player == null)
        //    {
        //        Log.WarnLine("player {0} SaveOrderId not find pc", pks.PcUid);
        //        return;
        //    }
        //    player.SaveRechargeOrderId(pks.OrderId);
        //}

        public void OnResponse_GetOrderId(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_ORDER_ID pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_ORDER_ID>(stream);
            Log.Write("player {0} get orderId {1}", uid, pks.GiftId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} SaveOrderId not find pc", uid);
                return;
            }
            player.GetRechargeHistoryId(pks.GiftId);
        }

        private bool getHistory = false;
        //获取充值记录
        public void OnResponse_ReadRechargeHistory(MemoryStream stream, int uid = 0)
        {
            if (getHistory)
            {
                return;
            }
            else
            {
                getHistory = true;
            }

            MSG_GateZ_GET_RECHARGE_HISTORY pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_RECHARGE_HISTORY>(stream);
            Log.Write("player {0} read rechage history page {1}", pks.PcUid, pks.Page);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.WarnLine("player {0} ReadRechargeHistory not find pc", pks.PcUid);
                getHistory = false;
                return;
            }
            int page = pks.Page;
            if (page >= 0)
            {
                //string tableName = "recharge_history";
                QueryGetRechargeHistory query = new QueryGetRechargeHistory(player.Uid, page - 1, RechargeLibrary.OrderPageCount);
                Api.GameDBPool.Call(query, ret =>
                {
                    MSG_ZGC_RECHARGE_HISTORY msg = new MSG_ZGC_RECHARGE_HISTORY();
                    msg.TotalCount = query.TotalCount;
                    msg.Page = page;

                    foreach (var item in query.historys)
                    {
                        MSG_ZGC_RECHARGE_INFO info = new MSG_ZGC_RECHARGE_INFO();
                        info.OrderId = item.OrderId.ToString();
                        info.State = item.State;
                        info.Time = item.Time;
                        info.Money = item.Money;
                        msg.Infos.Add(info);
                    }

                    player.Write(msg);
                    getHistory = false;
                });
            }
            else
            {
                Log.WarnLine("player {0} ReadRechargeHistory page {1}", pks.PcUid, page);
                getHistory = false;
            }
        }

        ////删除充值记录
        //public void OnResponse_DeleteOrderId(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_DELETE_RECHARGE_HISTORY pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DELETE_RECHARGE_HISTORY>(stream);
        //    PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
        //    if (player == null)
        //    {
        //        Log.WarnLine("player {0} DeleteOrderId not find pc", pks.PcUid);
        //        return;
        //    }
        //    player.DeleteRechargeOrderId(pks.OrderId);
        //}

        public void OnResponse_DebugRecharge(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DEBUG_RECHARGE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DEBUG_RECHARGE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_DebugRecharge not find pc", uid);
                return;
            }
            //player.DebugRecharge(pks.RechargeId);
        }

        public void OnResponse_BuyRechargeGift(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_RECHARGE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_RECHARGE_GIFT>(stream);
            Log.Write("player {0} buy recharge gift id {1} uid {2}", uid, msg.GiftId, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_BuyRechargeGift not find pc", uid);
                return;
            }

            player.BuyRechargeGift(msg.GiftId, msg.Uid);
        }

        public void OnResponse_ReceiveRechargeReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RECEIVE_RECHARGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RECEIVE_RECHARGE_REWARD>(stream);
            Log.Write("player {0} receive recharge gift {1} reward", uid, msg.RechargeItemId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_ReceiveRechargeReward not find pc", uid);
                return;
            }
            player.ReceiveRechargeReward(msg.RechargeItemId);
        }

        public void OnResponse_UseRechargeToken(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_USE_RECHARGE_TOKEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_USE_RECHARGE_TOKEN>(stream);
            Log.Write("player {0} use recharge token, token item {1} giftId {2} giftUid {3}", uid, msg.Uid, msg.GiftItemId, msg.GiftUid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_UseRechargeToken not find pc", uid);
                return;
            }
            player.UseRechargeToken(msg.Uid, msg.GiftItemId, msg.GiftUid);
        }

        public void OnResponse_OpenRechargeGift(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_OPEN_RECHARGE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_OPEN_RECHARGE_GIFT>(stream);
            Log.Write($"player {uid} buy cultivate gift, giftId {msg.GiftId}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_BuyCultivateGift not find pc", uid);
                return;
            }
            player.OpenRechargeGift(msg.GiftId, msg.Uid);
        }

        public void OnResponse_BuyCultivateGift(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_CULTIVATE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_CULTIVATE_GIFT>(stream);
            Log.Write($"player {uid} buy cultivate gift, giftId {msg.GiftId}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_BuyCultivateGift not find pc", uid);
                return;
            }
            player.BuyCultivateGift(msg.GiftId);
        }

        public void OnResponse_ReceiveFreePettyGift(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FREE_PETTY_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FREE_PETTY_GIFT>(stream);
            Log.Write($"player {uid} receive free petty gift, giftId {msg.GiftId}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_ReceiveFreePettyGift not find pc", uid);
                return;
            }
            player.ReceiveFreePettyGift(msg.GiftId);
        }

        public void OnResponse_GetDailyRechargeReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_DAILY_RECHARGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_DAILY_RECHARGE_REWARD>(stream);
            Log.Write($"player {uid} get daily recharge reward {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetDailyRechargeReward not find pc", uid);
                return;
            }
            player.GetDailyRechargeReward(msg.Id);
        }

        public void OnResponse_GetHeroDaysReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_HERO_DAYS_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_HERO_DAYS_REWARD>(stream);
            Log.Write($"player {uid} get hero days reward {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetHeroDaysReward not find pc", uid);
                return;
            }
            player.GetHeroDaysReward(msg.Id);
        }

        public void OnResponse_GetAccumulateRechargeReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_ACCUMULATE_RECHARGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_ACCUMULATE_RECHARGE_REWARD>(stream);
            Log.Write($"player {uid} get accumulate recharge reward {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetAccumulateRechargeReward not find pc", uid);
                return;
            }
            player.GetAccumulateRechargeReward(msg.Id);
        }

        public void OnResponse_GetNewRechargeGiftAccumulateReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_NEW_RECHARGE_GIFT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_NEW_RECHARGE_GIFT_REWARD>(stream);
            Log.Write($"player {uid} get accumulate recharge reward {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetAccumulateRechargeReward not find pc", uid);
                return;
            }
            player.GetNewRechargeGiftAccumulateReward(msg.Id);
        }

        public void OnResponse_GetNewServerPromotionReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_NEWSERVER_PROMOTION_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_NEWSERVER_PROMOTION_REWARD>(stream);
            Log.Write($"player {uid} get new server promotion reward {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetNewServerPromotionReward not find pc", uid);
                return;
            }
            player.GetNewServerPromotionReward(msg.Id);
        }

        public void OnResponse_GetLuckyFlipCardReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_LUCKY_FLIP_CARD_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_LUCKY_FLIP_CARD_REWARD>(stream);
            Log.Write($"player {uid} get lucky flip card reward {msg.Id} , Index :{msg.Index}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetLuckyFlipCardReward not find pc", uid);
                return;
            }
            player.GetLuckyFlipCardReward(msg.Id, msg.Index);
        }

        public void OnResponse_GetLuckyFlipCardCumulateReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD>(stream);
            Log.Write($"player {uid} get lucky flip card cumulate reward {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetLuckyFlipCardCumulateReward not find pc", uid);
                return;
            }
            player.GetLuckyFlipCardCumulateReward(msg.Id);
        }
        public void OnResponse_GetTreasureFlipCardReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_TREASURE_FLIP_CARD_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_TREASURE_FLIP_CARD_REWARD>(stream);
            Log.Write($"player {uid} get lucky flip card reward {msg.Id}, Index:{msg.Index}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetTreasureFlipCardReward not find pc", uid);
                return;
            }
            player.GetTreasureFlipCardReward(msg.Id, msg.Index);
        }

        public void OnResponse_GetTreasureFlipCardCumulateReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD>(stream);
            Log.Write($"player {uid} get lucky flip card cumulate reward {msg.Id}");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_GetTreasureFlipCardCumulateReward not find pc", uid);
                return;
            }
            player.GetTreasureFlipCardCumulateReward(msg.Id);
        }

        public void OnResponse_FlipCardRechargeGift(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FLIP_CARD_RECHARGE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FLIP_CARD_RECHARGE_GIFT>(stream);
            Log.Write("player {0} buy recharge gift id {1} uid {2}", uid, msg.GiftId, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.WarnLine("player {0} OnResponse_BuyRechargeGift not find pc", uid);
                return;
            }

            player.BuyRechargeGift(msg.GiftId, msg.Uid, true);
        }
    }
}
