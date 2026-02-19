using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_ContributionInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CONTRIBUTION_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ContributionInfo not find client", pcUid);
            }
        }

        private void OnResponse_GetContributionReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CONTRIBUTION_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetContributionReward not find client", pcUid);
            }
        }
    }
}
