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
        private void OnResponse_GetGardenInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GARDEN_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetGardenInfo not find client", pcUid);
            }
        }

        private void OnResponse_PlantedSeed(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GARDEN_PLANTED_SEED>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} PlantedSeed not find client", pcUid);
            }
        }

        private void OnResponse_GetGardenReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GARDEN_REAWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetGardenReward not find client", pcUid);
            }
        }

        private void OnResponse_BuySeed(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GARDEN_BUY_SEED>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BuySeed not find client", pcUid);
            }
        }

        private void OnResponse_GardenShopExchange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GARDEN_SHOP_EXCHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GardenShopExchange not find client", pcUid);
            }
        }
    }
}
