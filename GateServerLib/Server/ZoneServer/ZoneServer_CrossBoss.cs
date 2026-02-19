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
        private void OnResponse_GetCrossBossInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_BOSS_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetCrossBossInfo not find client", pcUid);
            }
        }

        private void OnResponse_UpdateCrossBossQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_CROSS_BOSS_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update cross boss queue not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossBossPassReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_BOSS_PASS_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross boss pass reward not find client", pcUid);
            }
        }
        private void OnResponse_GetCrossBossRankReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_BOSS_RANK_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross boss pass reward not find client", pcUid);
            }
        }


        private void OnResponse_GetCrossBossDefenserInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_BOSS_CHALLENGER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross boss defenser not find client", pcUid);
            }
        }
    }
}
