using System.IO;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;

namespace GateServerLib
{
    partial class ZoneServer
    {
        private void OnResponse_WishLanternInfo(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_WISH_LANTERN_INFO>.Value, stream);
            }
        }

        private void OnResponse_WishLanternSelect(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_WISH_LANTERN_SELECT_REWARD>.Value, stream);
            }
        }

        private void OnResponse_WishLanternLight(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_WISH_LANTERN_LIGHT>.Value, stream);
            }
        }

        private void OnResponse_WishLanternReset(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_WISH_LANTERN_RESET>.Value, stream);
            }
        }
    }
}
