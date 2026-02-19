using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_ContributionInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CONTRIBUTION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CONTRIBUTION_INFO>(stream);
            Log.Write("player {0} get contribution info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ContributionInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            

            player.GetContributionInfo();
        }

        public void OnResponse_GetContributionReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CONTRIBUTION_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CONTRIBUTION_REWARD>(stream);
            Log.Write("player {0} request GetContributionReward", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetContributionReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetContributionReward();
        }
    }
}
