using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_TridentReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TRIDENT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TRIDENT_REWARD>(stream);
            Log.Write("player {0} request TridentReward type {1}", uid, msg.Type);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.TridentReward(msg.Type, msg.RewardId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("TridentReward fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("TridentReward fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_TridentUseShovel(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TRIDENT_USE_SHOVEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TRIDENT_USE_SHOVEL>(stream);
            Log.Write("player {0} request TridentUseShovel GiftId {1}", uid, msg.GiftId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.TridentUseShovel(msg.GiftId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("TridentUseShovel fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("TridentUseShovel fail, can not find player {0} .", uid);
                }
            }
        }
    }
}
