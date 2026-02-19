using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_TransferEnterMap(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TRANSFER_ENTER_MAP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} transfer enter map not find client", pcUid);
            }
        }

        private void OnResponse_AutoPathFinding(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_AUTOPATHFINDING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} auto path finding not find client", pcUid);
            }
        }
    }
}
