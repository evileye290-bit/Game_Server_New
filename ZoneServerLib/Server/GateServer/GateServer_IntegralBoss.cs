using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_IntegralInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_INTERGRAL_BOSS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_INTERGRAL_BOSS_INFO>(stream);
            Log.Write("player {0} request IntegralInfo", msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.RequestIntegralBossInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("IntegralInfo fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("IntegralInfo fail, can not find player {0} .", msg.Uid);
                }
            }
        }

        public void OnResponse_IntegralKillInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_INTERGRAL_BOSS_KILLINFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_INTERGRAL_BOSS_KILLINFO>(stream);
            Log.Write("player {0} request Integral Kill Info", msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                player.RequestIntegralBossKillInfo(msg.DungeonId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player != null)
                {
                    Log.WarnLine("Integral kill Info fail, player {0} is offline.", msg.Uid);
                }
                else
                {
                    Log.WarnLine("Integral kill Info fail, can not find player {0} .", msg.Uid);
                }
            }
        }
    }
}
