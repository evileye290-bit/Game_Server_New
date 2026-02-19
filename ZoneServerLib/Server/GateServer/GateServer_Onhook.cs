using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetOnhookInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ONHOOK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ONHOOK_INFO>(stream);
            Log.Write("player {0} get onhook info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetOnhookInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetOnhookInfo();
        }

        public void OnResponse_GetOnhookReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ONHOOK_GET_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ONHOOK_GET_REWARD>(stream);
            Log.Write("player {0} request get onhook reward {1}", uid, msg.RewardType);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetOnhookReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetOnhookReward(msg.RewardType);
        }
    }
}
