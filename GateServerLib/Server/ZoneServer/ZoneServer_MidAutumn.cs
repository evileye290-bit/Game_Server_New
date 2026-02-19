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
        private void OnResponse_GetMidAutumnInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_MIDAUTUMN_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetMidAutumnInfo not find client", pcUid);
            }
        }

        private void OnResponse_DrawMidAutumnReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRAW_MIDAUTUMN_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} DrawMidAutumnReward not find client", pcUid);
            }
        }

        private void OnResponse_GetMidAutumnScoreReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_MIDAUTUMN_SCORE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetMidAutumnScoreReward not find client", pcUid);
            }
        }
    }
}
