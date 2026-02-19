using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_OnhookInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ONHOOK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ONHOOK_INFO>(stream);
            MSG_GateZ_ONHOOK_INFO request = new MSG_GateZ_ONHOOK_INFO();

            WriteToZone(request);
        }

        public void OnResponse_OnhookGetReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ONHOOK_GET_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ONHOOK_GET_REWARD>(stream);
            MSG_GateZ_ONHOOK_GET_REWARD request = new MSG_GateZ_ONHOOK_GET_REWARD()
            {
                RewardType = msg.RewardType,
            };

            WriteToZone(request);
        }
    }
}
