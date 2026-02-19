using System;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;
using CommonUtility;

namespace GateServerLib
{
    public partial class Client
    {

        private void OnResponse_IslandHighInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_ISLAND_HIGH_INFO request = new MSG_GateZ_ISLAND_HIGH_INFO();
            WriteToZone(request);
        }

        private void OnResponse_IslandHighRock(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_HIGH_ROCK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_HIGH_ROCK>(stream);
            MSG_GateZ_ISLAND_HIGH_ROCK request = new MSG_GateZ_ISLAND_HIGH_ROCK() {Type = msg.Type, Num = msg.Num};
            WriteToZone(request);
        }

        private void OnResponse_IslandHighReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_HIGH_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_HIGH_REWARD>(stream);
            MSG_GateZ_ISLAND_HIGH_REWARD request = new MSG_GateZ_ISLAND_HIGH_REWARD() { Type = msg.Type, RewardId = msg.RewardId};
            WriteToZone(request);
        }

        private void OnResponse_IslandHighBuyItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_HIGH_BUY_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_HIGH_BUY_ITEM>(stream);
            MSG_GateZ_ISLAND_HIGH_BUY_ITEM request = new MSG_GateZ_ISLAND_HIGH_BUY_ITEM() {ItemId = msg.ItemId};
            WriteToZone(request);
        }

    }
}
