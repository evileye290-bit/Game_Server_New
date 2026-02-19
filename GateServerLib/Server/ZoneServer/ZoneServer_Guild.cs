using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_CreateGuild(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CREATE_GUILD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} create guild not find client", pcUid);
            }
        }

        
    }
}
