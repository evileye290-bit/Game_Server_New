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
        private void OnResponse_GetCanoeInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CANOE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetCanoeInfo not find client", pcUid);
            }
        }

        private void OnResponse_CanoeGameStart(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CANOE_GAME_START>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} CanoeGameStart not find client", pcUid);
            }
        }

        private void OnResponse_CanoeGameEnd(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CANOE_GAME_END>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} CanoeGameEnd not find client", pcUid);
            }
        }

        private void OnResponse_CanoeGetReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CANOE_GET_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} CanoeGetReward not find client", pcUid);
            }
        }
    }
}
