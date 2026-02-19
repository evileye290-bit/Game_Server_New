using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_IntegralBossInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_INTERGRAL_BOSS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_INTERGRAL_BOSS_INFO>(stream);
            MSG_GateZ_INTERGRAL_BOSS_INFO request = new MSG_GateZ_INTERGRAL_BOSS_INFO();
            request.Uid = Uid;
            WriteToZone(request);
        }

        public void OnResponse_IntegralBossKillInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_INTERGRAL_BOSS_KILLINFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_INTERGRAL_BOSS_KILLINFO>(stream);
            MSG_GateZ_INTERGRAL_BOSS_KILLINFO request = new MSG_GateZ_INTERGRAL_BOSS_KILLINFO();
            request.Uid = Uid;
            request.DungeonId = msg.DungeonId;
            WriteToZone(request);
        }

    }
}
