using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_HeroGodList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_GOD_INFO_LIST>.Value, stream);
            }
        }

        private void OnResponse_HeroGodInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_GOD_INFO>.Value, stream);
            }
        }

        private void OnResponse_HeroGodUnlock(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_GOD_UNLOCK>.Value, stream);
            }
        }

        private void OnResponse_HeroGodEquip(MemoryStream stream, int pcUid = 0)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_GOD_EQUIP>.Value, stream);
            }
        }

    }
}
