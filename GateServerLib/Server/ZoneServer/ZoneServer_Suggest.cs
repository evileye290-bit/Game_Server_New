using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {

        private void OnResponse_Suggest(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SUGGEST>.Value, stream);
            }
        }

    }
}
