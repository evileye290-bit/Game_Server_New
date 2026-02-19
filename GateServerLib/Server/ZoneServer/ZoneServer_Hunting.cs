using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_HuntingInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_INFO>.Value, stream);
            }
        }

        public void OnResponse_BattleStartTime(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BATTLE_START_TIME>.Value, stream);
            }
        }

        public void OnResponse_HuntingDropSoulRing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_DROP_SOULRING>.Value, stream);
            }
        }

        public void OnResponse_HuntingChallangeCount(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_CHALLENBGE_COUNT>.Value, stream);
            }
        }

        public void OnResponse_HuntingSweep(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_SWEEP>.Value, stream);
            }
        }

        public void OnResponse_ContinueHunting(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CONTINUE_HUNTING>.Value, stream);
            }
        }

        public void OnResponse_MemberLeaveMap(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_MEMBER_LEAVE_MAP>.Value, stream);
            }
        }

        public void OnResponse_NotifyCaptainMemberLeave(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NOTIFY_CAPTAIN_MEMBERLEAVE>.Value, stream);
            }
        }

        public void OnResponse_HuntingActivityUnlock(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_ACTICITY_UNLOCK>.Value, stream);
            }
        }

        public void OnResponse_HuntingActivitySweep(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_ACTICITY_SWEEP>.Value, stream);
            }
        }

        public void OnResponse_HuntingHelp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_HELP>.Value, stream);
            }
        }

        public void OnResponse_HuntingHelpAsk(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_HELP_ASK>.Value, stream);
            }
        }

        public void OnResponse_HuntingHelpAnswerJoin(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_HELP_ANSWER_JOIN>.Value, stream);
            }
        }

        public void OnResponse_HuntingintrudeInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_INTRUDE_INFO>.Value, stream);
            }
        }

        public void OnResponse_HuntingintrudeChallenge(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_INTRUDE_CHALLENGE>.Value, stream);
            }
        }

        public void OnResponse_HuntingIntrudeUpdateHeroPos(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HUNTING_INTRUDE_HERO_POS>.Value, stream);
            }
        }
    }
}
