using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_OnhookInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ONHOOK_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnhookInfo not find client", pcUid);
            }
        }

        private void OnResponse_OnhookGetReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ONHOOK_GET_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnhookGetReward not find client", pcUid);
            }
        }
    }
}
