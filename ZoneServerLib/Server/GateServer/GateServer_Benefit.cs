using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    partial class GateServer
    {
        public void OnResponse_BenefitInfo(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request BenefitInfo", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.BenefitInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("BenefitInfo fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("BenefitInfo fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_BenefitSweep(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BENEFIT_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BENEFIT_SWEEP>(stream);
            Log.Write("player {0} request benefit sweep {1}", uid, msg.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.BenefitSweep(msg.Id);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("BenefitSweep fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("BenefitSweep fail, can not find player {0} .", uid);
                }
            }
        }
    }
}
