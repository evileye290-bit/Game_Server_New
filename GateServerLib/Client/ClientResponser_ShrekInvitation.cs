using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_GetShrekInvitationReward(MemoryStream stream)
        {
            MSG_CG_GET_SHREK_INVITAION_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_SHREK_INVITAION_REWARD>(stream);
            MSG_GateZ_GET_SHREK_INVITAION_REWARD request = new MSG_GateZ_GET_SHREK_INVITAION_REWARD();
            request.Id = msg.Id;
            request.Type = msg.Type;
            WriteToZone(request);
        }

        private void OnResponse_GetDiamondRebateRewards(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_DIAMOND_REBATE_REWARDS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_DIAMOND_REBATE_REWARDS>(stream);
            WriteToZone(new MSG_GateZ_GET_DIAMOND_REBATE_REWARDS() { Id = msg.Id });
        }
    }
}
