using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_IslandHighInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_HIGH_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandHighInfo not find client", pcUid);
            }
        }

        private void OnResponse_IslandHighRock(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_HIGH_ROCK>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GodPathHightRock not find client", pcUid);
            }
        }

        private void OnResponse_IslandHighReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_HIGH_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandHighReward not find client", pcUid);
            }
        }

        private void OnResponse_IslandHighBuyItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_HIGH_BUY_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandHighBuyItem not find client", pcUid);
            }
        }
        
    }
}
