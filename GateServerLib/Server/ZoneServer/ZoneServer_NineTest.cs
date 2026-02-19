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
        private void OnResponse_GetNineTestInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_NINETEST_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetNineTestInfo not find client", pcUid);
            }
        }

        private void OnResponse_NineTestClickGrid(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NINETEST_CLICK_GRID>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} NineTestClickGrid not find client", pcUid);
            }
        }

        private void OnResponse_NineTestScoreReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NINETEST_SCORE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} NineTestScoreReward not find client", pcUid);
            }
        }

        private void OnResponse_NineTestReset(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NINETEST_RESET>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} NineTestReset not find client", pcUid);
            }
        }
    }
}
