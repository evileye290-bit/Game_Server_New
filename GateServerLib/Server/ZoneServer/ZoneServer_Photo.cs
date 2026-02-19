using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using Message.Zone.Protocol.ZGate;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_UploadPhoto(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPLOAD_PHOTO>.Value, stream);
            }
        }
        private void OnResponse_RemovePhoto(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REMOVE_PHOTO>.Value, stream);
            }
        }
        private void OnResponse_PhotoList(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PHOTO_LIST>.Value, stream);
            }
        }

        public void OnResponse_PopRank(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_POP_RANK>.Value, stream);
            }
        }
    }
}
