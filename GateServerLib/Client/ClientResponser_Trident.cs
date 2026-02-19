using System;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;
using CommonUtility;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_TridentReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TRIDENT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TRIDENT_REWARD>(stream);
            MSG_GateZ_TRIDENT_REWARD request = new MSG_GateZ_TRIDENT_REWARD() { Type = msg.Type, RewardId = msg.RewardId};
            WriteToZone(request);
        }

        private void OnResponse_TridentUseShovel(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TRIDENT_USE_SHOVEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TRIDENT_USE_SHOVEL>(stream);
            MSG_GateZ_TRIDENT_USE_SHOVEL request = new MSG_GateZ_TRIDENT_USE_SHOVEL() { GiftId = msg.GiftId};
            WriteToZone(request);
        }

    }
}
