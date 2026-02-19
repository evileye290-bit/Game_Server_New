using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZR;
using ServerModels;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_TeamTypeList(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_TEAM_TYPE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TEAM_TYPE_LIST>(stream);
            Log.Write("player {0} request team type {1} page {2}", Uid, msg.TeamType, msg.Page);

            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_TeamTypeList fail, not find online player {0}.", Uid);
                return;
            }

            player.RequestTeamTypeList(msg.TeamType, msg.Page);
        }

        public void OnResponse_CreateTeam(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_CREATE_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CREATE_TEAM>(stream);
            Log.Write("player {0} request to create type {1} team", Uid, msg.TeamType);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_CreateTeam fail, not find online player {0}.", Uid);
                return;
            }
            player.RequestCreateTeam(msg);
        }

        public void OnResponse_JoinTeam(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_JOIN_TEAM>(stream);
            Log.Write("player {0} request to join team {1}", Uid, msg.TeamId);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_JoinTeam fail, not find online player {0}.", Uid);
                return;
            }

            player.RequestJoinTeam(msg.TeamId);
        }

        public void OnResponse_QuitTeam(MemoryStream stream, int Uid = 0)
        {
            PlayerChar player = Api.PCManager.FindPc(Uid);
            Log.Write("player {0} request quit team", Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_QuitTeam fail, not find online player {0}.", Uid);
                return;
            }

            player.RequestQuitTeam();
        }

        public void OnResponse_KickTeamMember(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_KICK_TEAM_MEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_KICK_TEAM_MEMBER>(stream);
            Log.Write("player {0} request kick team member {1}", Uid, msg.KickUid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_KickTeamMember, not find online player {0}.", Uid);
                return;
            }

            player.RequestKickTeam(msg.KickUid);
        }

        public void OnResponse_TramsforCaptain(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_TRANSFER_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TRANSFER_CAPTAIN>(stream);
            Log.Write("player {0} request transfer captain to {1}", Uid, msg.NewCapUid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("useitem fail, not find online player {0}.", Uid);
                return;
            }

            player.RequestTransferCaptain(msg.NewCapUid);
        }

        public void OnResponse_AskJoinTeam(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_ASK_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ASK_JOIN_TEAM>(stream);
            Log.Write("player {0} request ask {1} join team", Uid, msg.AskUid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_AskJoinTeam fail, not find online player {0}.", Uid);
                return;
            }

            player.RequestAskJoinTeam(msg.AskUid);
        }

        public void OnResponse_InviteJoinTeam(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_INVITE_JOIN_TEAM>(stream);
            Log.Write("player {0} request invite {1} join team", Uid, msg.InviteUid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_InviteCreateTeam fail, not find online player {0}.", Uid);
                return;
            }

            player.InviteJoinTeam(msg.InviteUid, msg.InviteMirror);
        }

        public void OnResponse_AnswerInviteJoinTeam(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_ANSWER_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ANSWER_INVITE_JOIN_TEAM>(stream);
            Log.Write("player {0} request answer {1} invite join team: agree {2}", Uid, msg.CapUid, msg.Agree.ToString());
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_AnswerInviteJoinTeam, not find online player {0}.", Uid);
                return;
            }

            MSG_ZR_ANSWER_INVITE_JOIN_TEAM notify = new MSG_ZR_ANSWER_INVITE_JOIN_TEAM();
            notify.CapUid = msg.CapUid;
            notify.InviterUid = player.Uid;
            notify.InviterName = player.Name;
            notify.Agree = msg.Agree;
            Api.SendToRelation(notify);
        }

        public void OnResponse_TryAskFollowCaptain(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_TRY_ASK_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TRY_ASK_FOLLOW_CAPTAIN>(stream);
            Log.Write("player {0} request try ask follow captain", Uid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_TryAskFollowCaptain, not find online player {0}.", Uid);
                return;
            }

            player.TryFlowCaptain();
        }

        public void OnResponse_AskFollowCaptain(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_ASK_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ASK_FOLLOW_CAPTAIN>(stream);
            Log.Write("player {0} request ask follow captain", Uid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_AskFollowCaptain, not find online player {0}.", Uid);
                return;
            }

            player.FlowCaptain();
        }

        public void OnResponse_ChangeTeamType(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_CHANGE_TEAM_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHANGE_TEAM_TYPE>(stream);
            Log.Write("player {0} request change team type to {1}", Uid, msg.TeamType);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_ChangeTeamType, not find online player {0}.", Uid);
                return;
            }

            player.RequestChangeTeamType(msg.TeamType);
        }

        public void OnResponse_ReliveTeamMember(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_TEAM_RELIVE_TEAMMEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TEAM_RELIVE_TEAMMEMBER>(stream);
            Log.Write("player {0} request relive team member {1}", Uid, msg.ReliveUid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_ReliveTeamMember, not find online player {0}.", Uid);
                return;
            }

            player.RequestReliveTeamMember(msg.ReliveUid);
        }

        public void OnResponse_ReliveHero(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_RELIVE_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RELIVE_HERO>(stream);
            Log.Write("player {0} request relive hero instance {1}", Uid, msg.InstanceId);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_ReliveHero, not find online player {0}.", Uid);
                return;
            }

            player.RequestReliveHero(msg.InstanceId);
        }

        public void OnResponse_NeedTeamHelp(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_NEED_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_NEED_TEAM_HELP>(stream);
            Log.Write("player {0} request need friend {1} team help", Uid, msg.FriendUid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_NeedTeamHelp fail, not find online player {0}.", Uid);
                return;
            }

            player.RequestTeamHelp(msg.FriendUid);
        }

        public void OnResponse_ResponseTeamHelp(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_RESPONSE_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RESPONSE_TEAM_HELP>(stream);
            Log.Write("player {0} request response team help, teamId {1} result {2}", Uid, msg.TeamId, msg.Result.ToString());
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_NeedTeamHelp fail, not find online player {0}.", Uid);
                return;
            }

            player.ResponseTeamHelp(msg.Result, msg.TeamId);
        }

        public void OnResponse_InviteFriendJoinTeam(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_INVITE_FRIEND_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_INVITE_FRIEND_JOIN_TEAM>(stream);
            Log.Write("player {0} request invite friend {1} join team", Uid, msg.InviteUid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_InviteFriendJoinTeam fail, not find online player {0}.", Uid);
                return;
            }
       
            player.InviteFriendJoinTeam(msg.InviteUid);
        }

        public void OnResponse_QuitTeamInDungeon(MemoryStream stream, int Uid = 0)
        {
            MSG_GateZ_QUIT_TEAM_INDUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_QUIT_TEAM_INDUNGEON>(stream);
            Log.Write("player {0} request quit team in dungeon", Uid);
            PlayerChar player = Api.PCManager.FindPc(Uid);
            if (player == null)
            {
                Log.WarnLine("OnResponse_QuitTeamInDungeon fail, not find online player {0}.", Uid);
                return;
            }
            player.RequestQuitTeamInDungeon();
        }
    }
}
