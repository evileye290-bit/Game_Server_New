using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_RechargeSaveOrder(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SAVE_ORDER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SAVE_ORDER>(stream);
            MSG_GateZ_SAVE_ORDER requestMsg = new MSG_GateZ_SAVE_ORDER();
            requestMsg.PcUid = Uid;
            requestMsg.OrderId = msg.OrderId;
            WriteToZone(requestMsg);
        }

        public void OnResponse_GetOrderId(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_ORDER_ID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_ORDER_ID>(stream);
            MSG_GateZ_GET_ORDER_ID requestMsg = new MSG_GateZ_GET_ORDER_ID();
            requestMsg.GiftId = msg.GiftId;
            WriteToZone(requestMsg);
        }

        public void OnResponse_ReadRechargeHistory(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_RECHARGE_HISTORY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_RECHARGE_HISTORY>(stream);
            MSG_GateZ_GET_RECHARGE_HISTORY requestMsg = new MSG_GateZ_GET_RECHARGE_HISTORY();
            requestMsg.PcUid = Uid;
            requestMsg.Page = msg.Page;
            WriteToZone(requestMsg);
        }

        public void OnResponse_DeleteOrderId(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DELETE_RECHARGE_HISTORY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DELETE_RECHARGE_HISTORY>(stream);
            MSG_GateZ_DELETE_RECHARGE_HISTORY requestMsg = new MSG_GateZ_DELETE_RECHARGE_HISTORY();
            requestMsg.PcUid = Uid;
            requestMsg.OrderId = msg.OrderId;
            WriteToZone(requestMsg);
        }

        public void OnResponse_DebugRecharge(MemoryStream stream)
        {
#if DEBUG
            if (curZone == null) return;
            MSG_CG_DEBUG_RECHARGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DEBUG_RECHARGE>(stream);
            MSG_GateZ_DEBUG_RECHARGE requestMsg = new MSG_GateZ_DEBUG_RECHARGE();
            requestMsg.RechargeId = msg.RechargeId;
            WriteToZone(requestMsg);
#endif
        }

        public void OnResponse_BuyRechargeGift(MemoryStream stream)
        {
//#if DEBUG
            if (curZone == null) return;
            MSG_CG_BUY_RECHARGE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_RECHARGE_GIFT>(stream);
            MSG_GateZ_BUY_RECHARGE_GIFT requestMsg = new MSG_GateZ_BUY_RECHARGE_GIFT();
            requestMsg.GiftId = msg.GiftId;
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(requestMsg);
//#endif
        }

        public void OnResponse_ReceiveRechargeReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RECEIVE_RECHARGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RECEIVE_RECHARGE_REWARD>(stream);
            MSG_GateZ_RECEIVE_RECHARGE_REWARD request = new MSG_GateZ_RECEIVE_RECHARGE_REWARD();
            request.RechargeItemId = msg.RechargeItemId;
            WriteToZone(request);
        }

        public void OnResponse_UseRechargeToken(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_USE_RECHARGE_TOKEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_USE_RECHARGE_TOKEN>(stream);
            MSG_GateZ_USE_RECHARGE_TOKEN request = new MSG_GateZ_USE_RECHARGE_TOKEN();
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            request.GiftItemId = msg.GiftItemId;
            request.GiftUid = ExtendClass.GetUInt64(msg.GiftUidHigh, msg.GiftUidLow);
            WriteToZone(request);
        }

        public void OnResponse_OpenRechargeGift(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_OPEN_RECHARGE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_OPEN_RECHARGE_GIFT>(stream);
            MSG_GateZ_OPEN_RECHARGE_GIFT requestMsg = new MSG_GateZ_OPEN_RECHARGE_GIFT();
            requestMsg.GiftId = msg.GiftId;
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(requestMsg);
        }

        public void OnResponse_BuyCultivateGift(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_CULTIVATE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_CULTIVATE_GIFT>(stream);
            MSG_GateZ_BUY_CULTIVATE_GIFT request = new MSG_GateZ_BUY_CULTIVATE_GIFT();
            //request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            request.GiftId = msg.GiftId;
            WriteToZone(request);
        }

        public void OnResponse_ReceiveFreePettyGift(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FREE_PETTY_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FREE_PETTY_GIFT>(stream);
            MSG_GateZ_FREE_PETTY_GIFT request = new MSG_GateZ_FREE_PETTY_GIFT();
            request.GiftId = msg.GiftId;
            WriteToZone(request);
        }

        public void OnResponse_GetDailyRechargeReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_DAILY_RECHARGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_DAILY_RECHARGE_REWARD>(stream);
            MSG_GateZ_GET_DAILY_RECHARGE_REWARD request = new MSG_GateZ_GET_DAILY_RECHARGE_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_GetHeroDaysReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_HERO_DAYS_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_HERO_DAYS_REWARD>(stream);
            MSG_GateZ_GET_HERO_DAYS_REWARD request = new MSG_GateZ_GET_HERO_DAYS_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_GetAccumulateRechargeReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_ACCUMULATE_RECHARGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_ACCUMULATE_RECHARGE_REWARD>(stream);
            MSG_GateZ_GET_ACCUMULATE_RECHARGE_REWARD request = new MSG_GateZ_GET_ACCUMULATE_RECHARGE_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_GetNewRechargeGiftAccumulateReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_NEW_RECHARGE_GIFT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_NEW_RECHARGE_GIFT_REWARD>(stream);
            MSG_GateZ_GET_NEW_RECHARGE_GIFT_REWARD request = new MSG_GateZ_GET_NEW_RECHARGE_GIFT_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_GetNewServerPromotionReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_NEWSERVER_PROMOTION_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_NEWSERVER_PROMOTION_REWARD>(stream);
            MSG_GateZ_GET_NEWSERVER_PROMOTION_REWARD request = new MSG_GateZ_GET_NEWSERVER_PROMOTION_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_GetLuckyFlipCardReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_LUCKY_FLIP_CARD_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_LUCKY_FLIP_CARD_REWARD>(stream);
            MSG_GateZ_GET_LUCKY_FLIP_CARD_REWARD request = new MSG_GateZ_GET_LUCKY_FLIP_CARD_REWARD();
            request.Id = msg.Id;
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_GetLuckyFlipCardCumulateReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD>(stream);
            MSG_GateZ_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD request = new MSG_GateZ_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_GetTreasureFlipCardReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_TREASURE_FLIP_CARD_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_TREASURE_FLIP_CARD_REWARD>(stream);
            MSG_GateZ_GET_TREASURE_FLIP_CARD_REWARD request = new MSG_GateZ_GET_TREASURE_FLIP_CARD_REWARD();
            request.Id = msg.Id;
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_GetTreasureFlipCardCumulateReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD>(stream);
            MSG_GateZ_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD request = new MSG_GateZ_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_FlipCardRechargeGift(MemoryStream stream)
        {
            //#if DEBUG
            if (curZone == null) return;
            MSG_CG_FLIP_CARD_RECHARGE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FLIP_CARD_RECHARGE_GIFT>(stream);
            MSG_GateZ_FLIP_CARD_RECHARGE_GIFT requestMsg = new MSG_GateZ_FLIP_CARD_RECHARGE_GIFT();
            requestMsg.GiftId = msg.GiftId;
            requestMsg.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            WriteToZone(requestMsg);
            //#endif
        }
    }
}
