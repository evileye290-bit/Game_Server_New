using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_IntegralBossInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INTERGRAL_BOSS_INFO>.Value, stream);
            }
        }

        public void OnResponse_IntegralBossState(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INTERGRAL_BOSS_STATE>.Value, stream);
            }
        }

        public void OnResponse_IntegralBossKillInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INTERGRAL_BOSS_KILLINFO>.Value, stream);
            }
        }
    }
}
