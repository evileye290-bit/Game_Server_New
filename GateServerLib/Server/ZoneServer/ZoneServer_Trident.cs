using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_TridentInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TRIDENT_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TridentInfo not find client", pcUid);
            }
        }

        private void OnResponse_TridentReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TRIDENT_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TridentReward not find client", pcUid);
            }
        }

        private void OnResponse_TridentUseShovel(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TRIDENT_USE_SHOVEL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TridentUseShovel not find client", pcUid);
            }
        }

    }
}
