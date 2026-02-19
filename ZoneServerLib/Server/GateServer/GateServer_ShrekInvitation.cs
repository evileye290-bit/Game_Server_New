using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_GetShrekInvitationReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_SHREK_INVITAION_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_SHREK_INVITAION_REWARD>(stream);
            Log.Write("player {0} GetShrekInvitationReward ", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetShrekInvitationReward(msg.Id, msg.Type);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("GetShrekInvitationReward fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("GetShrekInvitationReward, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_GetDiamondRebateRewards(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_DIAMOND_REBATE_REWARDS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_DIAMOND_REBATE_REWARDS>(stream);
            Log.Write("player {0} GetDiamondRebateRewards ", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetDiamondRebateRewards(msg.Id);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("GetDiamondRebateRewards fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("GetDiamondRebateRewards, can not find player {0} .", uid);
                }
            }
        }
    }
}
