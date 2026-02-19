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
        private void OnResponse_GetDragonBoatInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRAGON_BOAT_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetDragonBoatInfo not find client", pcUid);
            }
        }

        private void OnResponse_DragonBoatGameStart(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRAGON_BOAT_GAME_START>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} DragonBoatGameStart not find client", pcUid);
            }
        }

        private void OnResponse_DragonBoatGameEnd(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRAGON_BOAT_GAME_END>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} DragonBoatGameEnd not find client", pcUid);
            }
        }

        private void OnResponse_DragonBoatBuyTicket(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRAGON_BOAT_BUY_TICKET>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} DragonBoatBuyTicket not find client", pcUid);
            }
        }

        private void OnResponse_DragonBoatGetFreeTicket(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRAGON_BOAT_FREE_TICKET>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} DragonBoatGetFreeTicket not find client", pcUid);
            }
        }
    }
}
