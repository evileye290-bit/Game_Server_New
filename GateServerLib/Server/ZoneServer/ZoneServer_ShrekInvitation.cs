using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_GetShrekInvitationInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHREK_INVITATION_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ShrekInvitation not find client", pcUid);
            }
        }

        private void OnResponse_GetShrekInvitationReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_SHREK_INVITAION_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetShrekInvitationReward not find client", pcUid);
            }
        }

         private void OnResponse_DiamondRebateInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DIAMOND_REBATE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} DiamondRebateInfo not find client", pcUid);
            }
        }

        private void OnResponse_GetDiamondRebateRewards(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_DIAMOND_REBATE_REWARDS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetDiamondRebateRewards not find client", pcUid);
            }
        }
    }
}
