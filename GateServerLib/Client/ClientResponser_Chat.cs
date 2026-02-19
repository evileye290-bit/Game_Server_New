using EnumerateUtility;
using EnumerateUtility.Chat;
using Google.Protobuf.Collections;
using Logger;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateCM;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        //聊天
        private void OnResponse_Chat(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHAT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHAT>(stream);
            MSG_GateZ_CHAT request = new MSG_GateZ_CHAT();
            request.PcUid = Uid;
            request.ChatChannel = msg.ChatChannel;
            request.Content = msg.Content;
            request.EmojiId = msg.EmojiId;
            request.Param = msg.Param;
            WriteToZone(request);
        }

        //喇叭
        private void OnResponse_ChatTrumpet(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_USE_CHAT_TRUMPET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_USE_CHAT_TRUMPET>(stream);
            MSG_GateZ_USE_CHAT_TRUMPET request = new MSG_GateZ_USE_CHAT_TRUMPET();
            request.PcUid = Uid;
            request.Words = msg.Words;
            request.Id = msg.Id;
            request.UseItem = msg.UseItem;
            WriteToZone(request);
        }

        //检查聊天限制 暂不需
        private void OnResponse_CheckChatLimit(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHECK_CHATLIMIT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHECK_CHATLIMIT>(stream);
            MSG_GateZ_CHECK_CHATLIMIT request = new MSG_GateZ_CHECK_CHATLIMIT();
            request.ChatChannel = msg.ChatChannel;
            request.PcUid = msg.PcUid;
            WriteToZone(request);                       
        }

        //附近表情
        private void OnResponse_NearbyEmoji(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_NEARBY_EMOJI msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_NEARBY_EMOJI>(stream);
            MSG_GateZ_NEARBY_EMOJI request = new MSG_GateZ_NEARBY_EMOJI();
            request.PcUid = Uid;
            request.EmojiId = msg.EmojiId;
            WriteToZone(request);
        }

        //举报
        private void OnResponse_TipOff(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TIP_OFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TIP_OFF>(stream);           
            MSG_GateZ_TIP_OFF request = new MSG_GateZ_TIP_OFF();
            request.SourceUid = Uid;
            request.DestUid = msg.DestUid;
            request.DestName = msg.DestName;
            request.Type = msg.Type;
            request.Content.AddRange(msg.Content);
            request.Detail = msg.Detail;
            WriteToZone(request);
        }

        //购买气泡框
        private void OnResponse_ActivityChatBubble(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ACTIVITY_CHAT_BUBBLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ACTIVITY_CHAT_BUBBLE>(stream);
            MSG_GateZ_ACTIVITY_CHAT_BUBBLE request = new MSG_GateZ_ACTIVITY_CHAT_BUBBLE();
            request.PcUid = Uid;
            request.BubbleId = msg.BubbleId;
            WriteToZone(request);
        }

        //购买喇叭
        private void OnResponse_BuyChatTrumpet(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_TRUMPET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_TRUMPET>(stream);
            MSG_GateZ_BUY_TRUMPET request = new MSG_GateZ_BUY_TRUMPET();
            request.PcUid = Uid;
            request.ItemId = msg.ItemId;
            request.Num = msg.Num;
            WriteToZone(request);
        }

        //消红点
        private void OnResponse_ClearBubbleRedPoint(MemoryStream stream)
        {
            MSG_CG_CLEAR_BUBBLE_REDPOINT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CLEAR_BUBBLE_REDPOINT>(stream);
            MSG_GateZ_CLEAR_BUBBLE_REDPOINT request = new MSG_GateZ_CLEAR_BUBBLE_REDPOINT();
            request.ItemId = msg.ItemId;
            WriteToZone(request);
        }
    }
}
