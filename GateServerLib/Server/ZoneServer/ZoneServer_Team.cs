using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_TeamTypeList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TEAM_TYPE_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_TeamTypeList answer not find client", pcUid);
            }
        }

        private void OnResponse_CreateTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CREATE_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_CreateTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_JoinTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_JOIN_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_JoinTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_NewMemberJoinTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEW_TEAM_MEMBER_JOIN>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_NewMemberJoinTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_LeaveTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TEAM_MEMBER_LEAVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_LeaveTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_AskJoinTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ASK_JOIN_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_AskJoinTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_QuitTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_QUIT_TEAM>.Value, stream);
                Log.Write("client {0} Sync TitleChange", client.Uid);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_QuitTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_KickTeamMember(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_KICK_TEAM_MEMBER>.Value, stream);
                Log.Write("client {0} Sync TitleChange", client.Uid);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_KickTeamMember answer not find client", pcUid);
            }
        }

        private void OnResponse_TransferCaptain(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TRANSFER_CAPTAIN>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_TransferCaptain answer not find client", pcUid);
            }
        }

        private void OnResponse_CaptainChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAPTAIN_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_CaptainChange answer not find client", pcUid);
            }
        }

        private void OnResponse_TeamMemberOffline(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TEAM_MEMBER_OFFLINE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_TeamMemberOffline answer not find client", pcUid);
            }
        }

        private void OnResponse_TeamMemberOnline(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TEAM_MEMBER_ONLINE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_TeamMemberOnline answer not find client", pcUid);
            }
        }

        private void OnResponse_InviteJoinTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INVITE_JOIN_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_InviteJoinTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_AskInviteJoinTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ASK_INVITE_JOIN_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_AskInviteJoinTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_AnswerInviteJoinTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ANSWER_INVITE_JOIN_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_AnswerInviteJoinTeam answer not find client", pcUid);
            }
        }

        private void OnResponse_ChangeTeamType(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHANGE_TEAM_TYPE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_ChangeTeamType answer not find client", pcUid);
            }
        }

        private void OnResponse_ReliveTeamMember(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TEAM_RELIVE_TEAMMEMBER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_ReliveTeamMember answer not find client", pcUid);
            }
        }

        private void OnResponse_NeedTeamHelp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEED_TEAM_HELP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_TeamNeedHelp answer not find client", pcUid);
            }
        }

        private void OnResponse_RequestTeamHelp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REQUEST_TEAM_HELP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_RequestTeamHelp answer not find client", pcUid);
            }
        }

        private void OnResponse_ResponseTeamHelp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RESPONSE_TEAM_HELP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_ResponseTeamHelp answer not find client", pcUid);
            }
        }

        private void OnResponse_ResponseFlowCaptain(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FOLLOW_CAPTAIN>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_ResponseFlowCaptain answer not find client", pcUid);
            }
        }
        //MSG_ZGC_TRY_FOLLOW_CAPTAIN
        private void OnResponse_ResponseTryFlowCaptain(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TRY_FOLLOW_CAPTAIN>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_ResponseTryFlowCaptain answer not find client", pcUid);
            }
        }

        private void OnResponse_ReliveHero(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RELIVE_HERO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_ReliveHero answer not find client", pcUid);
            }
        }

        private void OnResponse_InviteFriendJoinTeam(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INVITE_FRIEND_JOIN_TEAM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_InviteFriendJoinTeam answer not find client", pcUid);
            }
        }
        
        private void OnResponse_QuitTeamInDungeon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_QUIT_TEAM_INDUNGEON>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_QuitTeamInDungeon answer not find client", pcUid);
            }
        }
    }
}
