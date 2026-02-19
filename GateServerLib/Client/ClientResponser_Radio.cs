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

        public void OnResponse_GetRedioAllAnchorRank(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RADIO_GET_ALL_ANCHOR_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RADIO_GET_ALL_ANCHOR_RANK>(stream);
            MSG_GateZ_RADIO_GET_ALL_ANCHOR_RANK requestMsg = new MSG_GateZ_RADIO_GET_ALL_ANCHOR_RANK();
            requestMsg.PcUid = Uid;
            WriteToZone(requestMsg);
        }

        public void OnResponse_GetRedioAnchorContributionRank(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RADIO_GET_ANCHOR_CONTRIBUTION_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RADIO_GET_ANCHOR_CONTRIBUTION_RANK>(stream);
            MSG_GateZ_RADIO_GET_ANCHOR_CONTRIBUTION_RANK requestMsg = new MSG_GateZ_RADIO_GET_ANCHOR_CONTRIBUTION_RANK();
            requestMsg.PcUid = Uid;
            requestMsg.AnchorId = msg.AnchorId;
            WriteToZone(requestMsg);
        }

        public void OnResponse_GetRedioAllContributionRank(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RADIO_GET_ALL_CONTRIBUTION_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RADIO_GET_ALL_CONTRIBUTION_RANK>(stream);
            MSG_GateZ_RADIO_GET_ALL_CONTRIBUTION_RANK requestMsg = new MSG_GateZ_RADIO_GET_ALL_CONTRIBUTION_RANK();
            requestMsg.PcUid = Uid;
            WriteToZone(requestMsg);
        }

        public void OnResponse_GetRedioContributionReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RADIO_GET_CONTRIBUTION_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RADIO_GET_CONTRIBUTION_REWARD>(stream);
            MSG_GateZ_RADIO_GET_CONTRIBUTION_REWARD requestMsg = new MSG_GateZ_RADIO_GET_CONTRIBUTION_REWARD();
            requestMsg.PcUid = Uid;
            requestMsg.RewardId = msg.RewardId;
            WriteToZone(requestMsg);
        }

        public void OnResponse_RedioGiveGift(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RADIO_GIVE_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RADIO_GIVE_GIFT>(stream);
            MSG_GateZ_RADIO_GIVE_GIFT requestMsg = new MSG_GateZ_RADIO_GIVE_GIFT();
            requestMsg.PcUid = Uid;
            requestMsg.ItemId = msg.ItemId;
            requestMsg.Num = msg.Num;
            requestMsg.AnchorId = msg.AnchorId;
            WriteToZone(requestMsg);
        }

        public void OnResponse_RedioEnter(MemoryStream stream)
        {
            if (curZone == null) return;
            //MSG_CG_RADIO_ENTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RADIO_ENTER>(stream);
            RadioOpen = true;
        }

        public void OnResponse_RedioLeave(MemoryStream stream)
        {
            if (curZone == null) return;
            //MSG_CG_RADIO_LEAVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RADIO_LEAVE>(stream);
            RadioOpen = false;
        }
    }
}
