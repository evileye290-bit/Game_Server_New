using System.IO;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;

namespace GateServerLib
{
    partial class ZoneServer
    {
        private void OnResponse_GetRouletteInfo(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ROULETTE_GET_INFO>.Value, stream);
            }
        }

        private void OnResponse_RouletteRandom(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ROULETTE_RANDOM>.Value, stream);
            }
        }

        private void OnResponse_RouletteReward(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ROULETTE_REWARD>.Value, stream);
            }
        }

        private void OnResponse_RouletteRefresh(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ROULETTE_REFRESH>.Value, stream);
            }
        }

        private void OnResponse_RouletteBuyItem(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ROULETTE_BUY_ITEM>.Value, stream);
            }
        }
    }
}
