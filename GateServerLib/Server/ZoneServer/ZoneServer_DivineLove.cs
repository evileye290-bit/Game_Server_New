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
        private void OnResponse_GetDivineLoveInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DIVINE_LOVE_VALUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetDivineLoveInfo not find client", pcUid);
            }
        }

        private void OnResponse_GetDivineLoveReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_DIVINE_LOVE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetDivineLoveReward not find client", pcUid);
            }
        }

        private void OnResponse_GetDivineLoveCumulateReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_DIVINE_LOVE_CUMULATE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetDivineLoveCumulateReward not find client", pcUid);
            }
        }

        private void OnResponse_BuyDivineLoveItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_DIVINE_LOVE_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BuyDivineLoveItem not find client", pcUid);
            }
        }

        private void OnResponse_GetDivineLoveInfoList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DIVINE_LOVE_INFO_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetDivineLoveInfoList not find client", pcUid);
            }
        }

        private void OnResponse_OpenDivineLoveRound(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_OPEN_DIVINE_LOVE_ROUND>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OpenDivineLoveRound not find client", pcUid);
            }
        }

        private void OnResponse_CloseDivineLoveRound(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CLOSE_DIVINE_LOVE_ROUND>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} CloseDivineLoveRound not find client", pcUid);
            }
        }
    }
}
