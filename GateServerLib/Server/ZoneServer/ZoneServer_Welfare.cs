using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
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
        private void OnResponse_WelfareTriggerState(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_WELFARE_TRIGGER_STATE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} Welfare trigger state not find client", pcUid);
            }
        }

        private void OnResponse_WelfareTriggerChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_WELFARE_TRIGGER_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} Welfare trigger change not find client", pcUid);
            }
        }
    }
}
