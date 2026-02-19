using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_TeamTypeList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TEAM_TYPE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TEAM_TYPE_LIST>(stream);
            MSG_GateZ_TEAM_TYPE_LIST request = new MSG_GateZ_TEAM_TYPE_LIST();
            request.TeamType = msg.TeamType;
            request.Page = msg.Page;
            WriteToZone(request);
        }

        public void OnResponse_CreateTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CREATE_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CREATE_TEAM>(stream);
            MSG_GateZ_CREATE_TEAM request = new MSG_GateZ_CREATE_TEAM();
            request.TeamType = msg.TeamType;
            WriteToZone(request);
        }

        public void OnResponse_JoinTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_JOIN_TEAM>(stream);
            MSG_GateZ_JOIN_TEAM request = new MSG_GateZ_JOIN_TEAM();
            request.TeamId = msg.TeamId;
            WriteToZone(request);
        }

        public void OnResponse_QuitTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_QUIT_TEAM request = new MSG_GateZ_QUIT_TEAM();
            WriteToZone(request);
        }

        public void OnResponse_KickTeamMember(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_KICK_TEAM_MEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_KICK_TEAM_MEMBER>(stream);
            MSG_GateZ_KICK_TEAM_MEMBER request = new MSG_GateZ_KICK_TEAM_MEMBER();
            request.KickUid = msg.KickUid;
            WriteToZone(request);
        }

        public void OnResponse_TransferCaptain(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TRANDSFER_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TRANDSFER_CAPTAIN>(stream);
            MSG_GateZ_TRANSFER_CAPTAIN request = new MSG_GateZ_TRANSFER_CAPTAIN();
            request.NewCapUid = msg.NewCapUid;
            WriteToZone(request);
        }

        public void OnResponse_AskJoinTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ASK_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ASK_JOIN_TEAM>(stream);
            MSG_GateZ_ASK_JOIN_TEAM request = new MSG_GateZ_ASK_JOIN_TEAM();
            request.AskUid = msg.AskUid;
            WriteToZone(request);
        }

        public void OnResponse_InviteJoinTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_INVITE_JOIN_TEAM>(stream);
            MSG_GateZ_INVITE_JOIN_TEAM request = new MSG_GateZ_INVITE_JOIN_TEAM();
            request.InviteUid = msg.InviteUid;
            request.InviteMirror = msg.InviteMirror;
            WriteToZone(request);
        }

        public void OnResponse_AnswerInviteJoinTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ANSWER_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ANSWER_INVITE_JOIN_TEAM>(stream);
            MSG_GateZ_ANSWER_INVITE_JOIN_TEAM request = new MSG_GateZ_ANSWER_INVITE_JOIN_TEAM();
            request.Agree = msg.Agree;
            request.CapUid = msg.CapUid;
            WriteToZone(request);
        }

        public void OnResponse_AskFollowCaptain(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_ASK_FOLLOW_CAPTAIN request = new MSG_GateZ_ASK_FOLLOW_CAPTAIN();
            WriteToZone(request);
        }

        public void OnResponse_TryAskFollowCaptain(MemoryStream stream)
        {
            MSG_GateZ_TRY_ASK_FOLLOW_CAPTAIN request = new MSG_GateZ_TRY_ASK_FOLLOW_CAPTAIN();
            WriteToZone(request);
        }

        public void OnResponse_ChangeTeamType(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHANGE_TEAM_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHANGE_TEAM_TYPE>(stream);
            MSG_GateZ_CHANGE_TEAM_TYPE request = new MSG_GateZ_CHANGE_TEAM_TYPE();
            request.TeamType = msg.TeamType;
            WriteToZone(request);
        }

        public void OnResponse_ReliveTeamMember(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TEAM_RELIVE_TEAMMEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TEAM_RELIVE_TEAMMEMBER>(stream);
            MSG_GateZ_TEAM_RELIVE_TEAMMEMBER request = new MSG_GateZ_TEAM_RELIVE_TEAMMEMBER();
            request.ReliveUid = msg.ReliveUid;
            WriteToZone(request);
        }

        public void OnResponse_NeedTeamHelp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_NEED_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_NEED_TEAM_HELP>(stream);
            WriteToZone(new MSG_GateZ_NEED_TEAM_HELP() { FriendUid = msg.FriendUid });
        }

        public void OnResponse_ResponseTeamHelp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RESPONSE_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RESPONSE_TEAM_HELP>(stream);
            MSG_GateZ_RESPONSE_TEAM_HELP request = new MSG_GateZ_RESPONSE_TEAM_HELP()
            {
                Result = msg.Result,
                TeamId = msg.TeamId,
            };
            WriteToZone(request);
        }

        public void OnResponse_ReliveHero(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RELIVE_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RELIVE_HERO>(stream);
            MSG_GateZ_RELIVE_HERO request = new MSG_GateZ_RELIVE_HERO();
            request.InstanceId = msg.InstanceId;
            WriteToZone(request);
        }

        public void OnResponse_InviteFriendJoinTeam(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_INVITE_FRIEND_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_INVITE_FRIEND_JOIN_TEAM>(stream);
            MSG_GateZ_INVITE_FRIEND_JOIN_TEAM request = new MSG_GateZ_INVITE_FRIEND_JOIN_TEAM() { InviteUid = msg.InviteUid};
            WriteToZone(request);
        }

        public void OnResponse_QuitTeamInDungeon(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_QUIT_TEAM_INDUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_QUIT_TEAM_INDUNGEON>(stream);
            MSG_GateZ_QUIT_TEAM_INDUNGEON request = new MSG_GateZ_QUIT_TEAM_INDUNGEON();
            WriteToZone(request);     
        }
    }
}
