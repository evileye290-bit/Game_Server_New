using System.IO;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;

namespace GateServerLib
{
    partial class ZoneServer
    {
        private void OnResponse_GetXuanBoxInfo(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_XUANBOX_GET_INFO>.Value, stream);
            }
        }

        private void OnResponse_XuanBoxRandom(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_XUANBOX_RANDOM>.Value, stream);
            }
        }

        private void OnResponse_XuanBoxReward(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_XUANBOX_REWARD>.Value, stream);
            }
        }
       
    }
}
