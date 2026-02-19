using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_SecretAreaInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SECRET_AREA_INFO>.Value, stream);
            }
        }

        public void OnResponse_SecretAreaSweep(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SECRET_AREA_SWEEP>.Value, stream);
            }
        }

        public void OnResponse_SecretAreaRankInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SECRET_AREA_RANK_LIST>.Value, stream);
            }
        }

        public void OnResponse_SecretAreaContinueFight(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SECRET_AREA_CONT_FIGHT>.Value, stream);
            }
        }
    }
}
