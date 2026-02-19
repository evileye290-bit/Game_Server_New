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
        private void OnResponse_ShreklandInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHREKLAND_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get shrekland info not find client", pcUid);
            }
        }

        private void OnResponse_ShreklandUseRoulette(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHREKLAND_USE_ROULETTE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shrekland use roulette not find client", pcUid);
            }
        }

        private void OnResponse_ShreklandRefreshRewards(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHREKLAND_REFRESH_REWARDS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shrekland refresh rewards not find client", pcUid);
            }
        }

        private void OnResponse_ShreklandGetScoreReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHREKLAND_GET_SCORE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shrekland get score reward not find client", pcUid);
            }
        }
    }
}
