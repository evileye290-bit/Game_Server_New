using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DBUtility;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_TeamTypeList(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_TEAM_TYPE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_TEAM_TYPE_LIST>(stream);

            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null)
            {
                Log.Warn("player {0} create team failed: no such client", msg.Uid);
                return;
            }

            MSG_RZ_TEAM_TYPE_LIST response = new MSG_RZ_TEAM_TYPE_LIST();
            response.TeamType = msg.TeamType;
            if (client.Team != null)
            {
                response.OwnTeamId = client.Team.TeamId;
            }

            response.Result = (int)ErrorCode.Success;
            ZoneManager.TeamManager.GetTeamTypeList(msg.TeamType, msg.Page, response);
            Write(response, msg.Uid);
        }

        public void OnResponse_CreateTeam(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CREATE_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CREATE_TEAM>(stream);
            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null)
            {
                Log.Warn("player {0} create team failed: no such client", msg.Uid);
                return;
            }

            MSG_RZ_CREATE_TEAM response = new MSG_RZ_CREATE_TEAM();
            response.Uid = msg.Uid;
            if (client.Team != null)
            {
                response.Result = (int)ErrorCode.InTeam;
                Write(response);
                return;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(msg.TeamType);
            if (model == null || model.TeamLimit)
            {
                response.Result = (int)ErrorCode.MapTeamLimit;
                Write(response);
                return;
            }

            if (model.ChapterLimit > client.ChapterId)
            {
                response.Result = (int)ErrorCode.ChapterTaskNotFinish;
                Write(response);
                return;
            }

            RedisPlayerInfo playerInfo = Api.RPlayerInfoMng.GetPlayerInfo(msg.Uid);
            if (playerInfo != null)
            {
                TeamMember member = new TeamMember(client, playerInfo);

                member.IsOnline = client.IsOnline;

                if (client.Level < model.MinLevel)
                {
                    response.Result = (int)ErrorCode.LevelLimit;
                    Write(response);
                    return;
                }

                member.HeroMaxLevel = msg.Teamlevel;

                Team team = ZoneManager.TeamManager.CreateTeam(member, msg.TeamType);
                if (team == null)
                {
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
                client.Team = team;

                response.Result = (int)ErrorCode.Success;
                response.Team = team.GenerateTeamInfo();
                client.CurZone.Write(response);
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);

                Log.Warn("player {0} OnResponse_CreateTeam failed: redis error", msg.Uid);
                return;
            }
        }

        public void OnResponse_JoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_JOIN_TEAM>(stream);
            MSG_RZ_JOIN_TEAM response = new MSG_RZ_JOIN_TEAM();
            response.Uid = msg.Uid;

            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null)
            {
                Log.Warn("player {0} OnResponse_JoinTeam failed: no such client", msg.Uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            Team team = ZoneManager.TeamManager.GetTeam(msg.TeamId);
            if (team == null || team.InDungeon)
            {
                response.Result = (int)ErrorCode.NoTeam;
                Write(response);
                return;
            }

            if (team.CheckFull())
            {
                Log.Warn("player {0} OnResponse_JoinTeam failed: team full", msg.Uid);
                response.Result = (int)ErrorCode.TeamFull;
                Write(response);
                return;
            }

            if (client.Team != null)
            {
                client.Team.RemoveMember(msg.Uid);
            }

            RedisPlayerInfo playerInfo = Api.RPlayerInfoMng.GetPlayerInfo(msg.Uid);
            if (playerInfo != null)
            {
                TeamMember member = new TeamMember(client, playerInfo);
                team.AddMember(member);

                client.Team = team;

                response.Team = team.GenerateTeamInfo();
                response.Result = (int)ErrorCode.Success;
                Write(response);
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);

                Log.Warn("player {0} OnResponse_JoinTeam failed: redis error", msg.Uid);
                return;
            }
        }

        public void OnResponse_QuitTeam(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_QUIT_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_QUIT_TEAM>(stream);
            MSG_RZ_QUIT_TEAM response = new MSG_RZ_QUIT_TEAM();
            response.Uid = msg.Uid;

            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null)
            {
                Log.Warn("player {0} quit team failed: no such client", msg.Uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            Team team = ZoneManager.TeamManager.GetTeam(msg.TeamId);
            if (team == null)
            {
                //response.Result = (int)ErrorCode.NoTeam;
                response.Result = (int)ErrorCode.Success;
                Write(response);
                return;
            }

            if (team.InDungeon)
            {
                response.Result = (int)ErrorCode.InDungeon;
                Write(response);
                return;
            }

            team.RemoveMember(msg.Uid);
            client.Team = null;           

            // 通知自己
            MSG_RZ_QUIT_TEAM notify = new MSG_RZ_QUIT_TEAM();
            notify.Result = (int)ErrorCode.Success;
            notify.Uid = msg.Uid;
            client.CurZone.Write(notify);
        }

        public void OnResponse_KickTeamMember(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_KICK_TEAM_MEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_KICK_TEAM_MEMBER>(stream);
            MSG_RZ_KICK_TEAM_MEMBER response = new MSG_RZ_KICK_TEAM_MEMBER();
            response.Uid = msg.Uid;
            response.KickUid = msg.KickUid;

            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null)
            {
                Log.Write("player {0} request kick team {1} member {2} failed: no such client", msg.Uid, msg.TeamId, msg.KickUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            Team team = ZoneManager.TeamManager.GetTeam(msg.TeamId);
            if (team == null)
            {
                //response.Result = (int)ErrorCode.NoTeam;
                response.Result = (int)ErrorCode.Success;
                Write(response);
                return;
            }

            if (team.InDungeon)
            {
                response.Result = (int)ErrorCode.InDungeon;
                Write(response);
                return;
            }

            if (team.CaptainUid != msg.Uid || msg.Uid == msg.KickUid)
            {
                response.Result = (int)ErrorCode.NotTeamCaptain;
                Write(response);
                return;
            }

            if (!team.MemberList.ContainsKey(msg.KickUid))
            {
                response.Result = (int)ErrorCode.KickMemberNotInTeam;
                Write(response);
                return;
            }

            // 验证通过 踢人
            Client member = ZoneManager.GetClient(msg.KickUid);
            if (member != null)
            {
                //通知退出队伍
                MSG_RZ_QUIT_TEAM notify = new MSG_RZ_QUIT_TEAM();
                notify.Uid = msg.KickUid;
                notify.Result = (int)ErrorCode.Success;
                member.CurZone.Write(notify);
            }

            team.RemoveMember(msg.KickUid);
        }

        public void OnResponse_TransferCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_TRANDSFER_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_TRANDSFER_CAPTAIN>(stream);
            MSG_RZ_TRANDSFER_CAPTAIN response = new MSG_RZ_TRANDSFER_CAPTAIN();
            response.Uid = msg.Uid;

            Client oldCaptain = ZoneManager.GetClient(msg.Uid);
            if (oldCaptain == null)
            {
                Log.Write("player {0} OnResponse_TransferCaptain failed: no such client", msg.Uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            Team team = oldCaptain.Team;

            if (team == null)
            {
                response.Result = (int)ErrorCode.NotInTeam;
                Write(response);
                return;
            }

            if (team.CaptainUid != oldCaptain.Uid)
            {
                response.Result = (int)ErrorCode.NotTeamCaptain;
                Write(response);
                return;
            }

            if (!team.MemberList.ContainsKey(msg.MemberUid))
            {
                response.Result = (int)ErrorCode.MemberNotInTeam;
                Write(response);
                return;
            }

            Client newCaptain = ZoneManager.GetClient(msg.MemberUid);
            if (newCaptain == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (newCaptain.IsOnline == false || newCaptain.CurZone == null)
            {
                response.Result = (int)ErrorCode.TeamMemberOffline;
                Write(response);
                return;
            }

            if (newCaptain.Team.TeamId != team.TeamId)
            {
                Log.Warn("player {0} transfer team {1} to player {2} in team {3} failed: not in same team!", msg.Uid, oldCaptain.Team.TeamId, msg.MemberUid, newCaptain.Team.TeamId);
                return;
            }

            response.NewCapUid = newCaptain.Uid;
            response.Result = (int)ErrorCode.Success;
            Write(response);

            team.ChangeCaptain(newCaptain.Uid);
        }

        public void OnResponse_AskJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ASK_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ASK_JOIN_TEAM>(stream);
            Client apply = ZoneManager.GetClient(msg.Uid);
            if (apply == null)
            {
                Log.Warn($"player {msg.Uid} OnResponse_AskJoinTeam failed: client no exist");
                return;
            }

            MSG_RZ_ASK_JOIN_TEAM response = new MSG_RZ_ASK_JOIN_TEAM();
            response.Uid = msg.Uid;

            Client beenAskedPlayer = ZoneManager.GetClient(msg.AskUid);
            if (beenAskedPlayer == null)
            {
                response.Result = (int)ErrorCode.TargetOffline;
                Write(response);
                return;
            }

            if (beenAskedPlayer.Team == null)
            {
                response.Result = (int)ErrorCode.NotInTeam;
                Write(response);
                return;
            }

            Team team = beenAskedPlayer.Team;
            if (team.InDungeon)
            {
                response.Result = (int)ErrorCode.InDungeon;
                Write(response);
                return;
            }

            if (team.MemberList.ContainsKey(msg.Uid))
            {
                response.Result = (int)ErrorCode.InTeam;
                Write(response);
                return;
            }


            if (apply.Team != null)
            {
                apply.Team.RemoveMember(msg.Uid);
            }

            if (team.CheckFull())
            {
                response.Result = (int)ErrorCode.TeamFull;
                Write(response);
                return;
            }

            RedisPlayerInfo playerInfo = Api.RPlayerInfoMng.GetPlayerInfo(msg.Uid);
            if (playerInfo != null)
            {
                if (!team.IsFreeTeam)
                {
                    DungeonModel model = DungeonLibrary.GetDungeon(team.Type);
                    if (model == null || model.TeamLimit)
                    {
                        response.Result = (int)ErrorCode.MapTeamLimit;
                        Write(response);
                        return;
                    }

                    if (beenAskedPlayer.Level < model.MinLevel)
                    {
                        response.Result = (int)ErrorCode.LevelLimit;
                        Write(response);
                        return;
                    }

                    if (model.ChapterLimit > beenAskedPlayer.ChapterId)
                    {
                        response.Result = (int)ErrorCode.ChapterTaskNotFinish;
                        Write(response);
                        return;
                    }
                }

                TeamMember member = new TeamMember(apply, playerInfo);
                team.AddMember(member);
                apply.Team = team;

                response.Result = (int)ErrorCode.Success;
                response.Team = team.GenerateTeamInfo();
                Write(response);
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);

                Log.Warn("player {0} OnResponse_AskJoinTeam failed: redis error", msg.Uid);
                return;
            }
        }

        public void OnResponse_InviteJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_INVITE_JOIN_TEAM>(stream);
            Client captain = ZoneManager.GetClient(msg.CapUid);
            Client inviter = ZoneManager.GetClient(msg.InviteUid);

            MSG_RZ_INVITE_JOIN_TEAM response = new MSG_RZ_INVITE_JOIN_TEAM();
            response.InviteUid = msg.InviteUid;
            response.CapUid = msg.CapUid;
            response.InviteMirror = msg.InviteMirror;

            if (captain == null)
            {
                Log.Warn($"player {msg.CapUid} or {msg.InviteUid} OnResponse_AskJoinTeam failed: client no exist");
                return;
            }

            if (msg.InviteMirror || inviter == null || inviter.IsOnline == false || inviter.CurZone == null)
            {
                response.Result = (int)ErrorCode.InviteOfflineBrotherSuccess;
                response.InviteMirror = true;
                Write(response);
                return;
            }

            Team team = captain.Team;

            if (team != null)
            {
                if (team.InDungeon)
                {
                    response.Result = (int)ErrorCode.InDungeon;
                    Write(response);
                    return;
                }

                //被邀请者在队伍中
                if (team.MemberList.ContainsKey(msg.InviteUid))
                {
                    response.Result = (int)ErrorCode.InTeam;
                    Write(response);
                    return;
                }

                if (!team.IsFreeTeam)
                {
                    DungeonModel model = DungeonLibrary.GetDungeon(team.Type);
                    if (model == null || model.TeamLimit)
                    {
                        response.Result = (int)ErrorCode.MapTeamLimit;
                        Write(response);
                        return;
                    }

                    if (model.ChapterLimit > captain.ChapterId)
                    {
                        response.Result = (int)ErrorCode.ChapterTaskNotFinish;
                        Write(response);
                        return;
                    }

                    if (model.ChapterLimit > inviter.ChapterId)
                    {
                        response.Result = (int)ErrorCode.InviterChapterTaskNotFinish;
                        Write(response);
                        return;
                    }
                }

                if (team.CaptainUid != captain.Uid)
                {
                    response.Result = (int)ErrorCode.NotTeamCaptain;
                    Write(response);
                    return;
                }

                if (team.CheckFull())
                {
                    response.Result = (int)ErrorCode.TeamFull;
                    Write(response);
                    return;
                }
            }

            if (inviter.IsInDungeon())
            {
                response.Result = (int)ErrorCode.InDungeon;
                Write(response);
                return;
            }

            if (inviter.Team != null)
            {
                response.Result = (int)ErrorCode.InTeam;
                Write(response);
                return;
            }

            DateTime inviteTime;
            if (captain.InviteTeamList.TryGetValue(inviter.Uid, out inviteTime) && ((Api.Now() - inviteTime).TotalSeconds < 30))
            {
                response.Result = (int)ErrorCode.TeamAlreadyInvite;
                Write(response);
                return;
            }

            // 验证通过 发出邀请
            captain.InviteTeamList[inviter.Uid] = Api.Now();

            RedisPlayerInfo playerInfo = Api.RPlayerInfoMng.GetPlayerInfo(captain.Uid);
            if (playerInfo != null)
            {
                response.Result = (int)ErrorCode.Success;
                Write(response);

                MSG_RZ_ASK_INVITE_JOIN_TEAM request = new MSG_RZ_ASK_INVITE_JOIN_TEAM();
                request.CapUid = captain.Uid;
                request.CapLevel = captain.Level;
                request.CapName = playerInfo.GetStringValue(HFPlayerInfo.Name);
                request.InviteUid = inviter.Uid;
                request.Research = captain.Research;
                inviter.CurZone.Write(request);
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);

                Log.Warn("player {0} OnResponse_InviteJoinTeam failed: redis error", uid);
            }
        }

        public void OnResponse_AnswerInviterJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ANSWER_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ANSWER_INVITE_JOIN_TEAM>(stream);
            Client captain = ZoneManager.GetClient(msg.CapUid);

            if (captain == null || captain.IsInDungeon())
            {
                return;
            }

            if (!msg.Agree)
            {
                MSG_RZ_INVITE_JOIN_TEAM notify = new MSG_RZ_INVITE_JOIN_TEAM();
                notify.CapUid = captain.Uid;
                notify.InviterName = msg.InviterName;
                notify.Agree = msg.Agree;
                captain.CurZone.Write(notify);
                return;
            }

            int inviterUid = msg.InviterUid;

            //邀请镜像的话不需要知道镜像的是否在线
            Client inviter = msg.IsOfflineBrother ? null : ZoneManager.GetClient(inviterUid);

            if (!msg.IsOfflineBrother)
            {
                if (inviter == null || inviter.CurZone == null)
                {
                    return;
                }

                if (!captain.InviteTeamList.ContainsKey(inviterUid))
                {
                    return;
                }
                captain.InviteTeamList.Remove(inviterUid);
            }

            MSG_RZ_ANSWER_INVITE_JOIN_TEAM response = new MSG_RZ_ANSWER_INVITE_JOIN_TEAM();
            response.Uid = msg.InviterUid;

            if (!captain.IsOnline)
            {
                if (msg.IsOfflineBrother)
                {
                    return;
                }
                response.Result = (int)ErrorCode.TargetOffline;
                inviter?.CurZone.Write(response);
                return;
            }

            Team team = captain.Team;
            if (team == null)
            {
                RedisPlayerInfo playerCap = Api.RPlayerInfoMng.GetPlayerInfo(captain.Uid);
                RedisPlayerInfo playerInv = Api.RPlayerInfoMng.GetPlayerInfo(inviterUid);
                if (playerCap == null || playerInv == null)
                {
                    Log.Warn("player {0} AnswerInviterJoinTeam failed: redis not find info {1}", inviterUid, captain.Uid);
                    return;
                }

                TeamMember memberCap = new TeamMember(captain, playerCap);
                TeamMember memberInv = new TeamMember(inviter, playerInv);
                memberInv.IsAllowOffline = msg.IsOfflineBrother;

                team = ZoneManager.TeamManager.CreateTeam(memberCap, 0);
                if (team == null)
                {
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                team.IsInviteMirror = msg.InviteMirror;
                team.AddMember(memberInv, false);

                captain.Team = team;
                captain.NotifyTeamInfo();

                if (!msg.IsOfflineBrother && inviter != null)
                {
                    inviter.Team = team;
                    inviter.NotifyTeamInfo();
                }
            }
            else
            {
                if (captain.Team.InDungeon)
                {
                    if (msg.IsOfflineBrother)
                    {
                        return;
                    }
                    response.Result = (int)ErrorCode.NoTeam;
                    inviter?.CurZone.Write(response);
                    return;
                }

                //被邀请者在队伍中
                if (team.MemberList.ContainsKey(inviterUid))
                {
                    response.Result = (int)ErrorCode.InTeam;
                    Write(response);
                    return;
                }

                if (captain.Team.CheckFull())
                {
                    if (msg.IsOfflineBrother)
                    {
                        return;
                    }

                    response.Result = (int)ErrorCode.TeamFull;
                    inviter?.CurZone.Write(response);
                    return;
                }

                RedisPlayerInfo playerInfo = Api.RPlayerInfoMng.GetPlayerInfo(inviterUid);
                if (playerInfo == null)
                {
                    Log.Warn("player {0} AnswerInviterJoinTeam failed: redis error", inviterUid);
                    return;
                }

                TeamMember memberInv = new TeamMember(inviter, playerInfo);
                memberInv.IsAllowOffline = msg.IsOfflineBrother;
                memberInv.Research = playerInfo.GetIntValue(HFPlayerInfo.Research);
                captain.Team.IsInviteMirror = msg.InviteMirror;

                captain.Team.AddMember(memberInv);

                if (!msg.IsOfflineBrother && inviter != null)
                {
                    inviter.Team = team;
                    inviter.NotifyTeamInfo();
                }
            }
        }

        //转发到队长所在的zone
        public void OnResponse_AskFollowCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ASK_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ASK_FOLLOW_CAPTAIN>(stream);
            Client captain = ZoneManager.GetClient(msg.CapUid);
            if (captain == null || captain.Team == null || captain.Team.CaptainUid != msg.CapUid || captain.CurZone == null || captain.IsOnline == false)
            {
                return;
            }

            MSG_RZ_ASK_FOLLOW_CAPTAIN request = new MSG_RZ_ASK_FOLLOW_CAPTAIN();
            request.MemberUid = msg.MemberUid;
            request.CapUid = msg.CapUid;
            captain.CurZone.Write(request);
        }

        public void OnResponse_TryAskFollowCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_TRY_ASK_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_TRY_ASK_FOLLOW_CAPTAIN>(stream);
            Client captain = ZoneManager.GetClient(msg.CapUid);
            if (captain == null || captain.Team == null || captain.Team.CaptainUid != msg.CapUid || captain.CurZone == null || captain.IsOnline == false)
            {
                return;
            }

            MSG_RZ_TRY_ASK_FOLLOW_CAPTAIN request = new MSG_RZ_TRY_ASK_FOLLOW_CAPTAIN();
            request.MemberUid = msg.MemberUid;
            request.CapUid = msg.CapUid;
            captain.CurZone.Write(request);
        }

        //来自队长所在zone的回应
        public void OnResponse_AnswerFollowCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ANSWER_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ANSWER_FOLLOW_CAPTAIN>(stream);
            Log.Debug("player {0} will follow captain to map {0} channel {1}", msg.MemberUid, msg.MapId, msg.ChannelId);
            Client member = Api.ZoneManager.GetClient(msg.MemberUid);
            if (member == null || member.Team == null || member.Team.CaptainUid == member.Uid || member.CurZone == null || member.IsOnline == false)
            {
                return;
            }
            MSG_RZ_ANSWER_FOLLOW_CAPTAIN notify = new MSG_RZ_ANSWER_FOLLOW_CAPTAIN();
            notify.MemberUid = msg.MemberUid;
            notify.MapId = msg.MapId;
            notify.ChannelId = msg.ChannelId;
            notify.PosX = msg.PosX;
            notify.PosY = msg.PosY;
            notify.Result = msg.Result;
            member.CurZone.Write(notify);
        }

        public void OnResponse_TRYAnswerFollowCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_TRY_ANSWER_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_TRY_ANSWER_FOLLOW_CAPTAIN>(stream);
            Log.Debug("player {0} will follow captain to map {0} channel {1}", msg.MemberUid, msg.MapId, msg.ChannelId);
            Client member = Api.ZoneManager.GetClient(msg.MemberUid);
            if (member == null || member.Team == null || member.Team.CaptainUid == member.Uid || member.CurZone == null || member.IsOnline == false)
            {
                return;
            }
            MSG_RZ_TRY_ANSWER_FOLLOW_CAPTAIN notify = new MSG_RZ_TRY_ANSWER_FOLLOW_CAPTAIN();
            notify.MemberUid = msg.MemberUid;
            notify.MapId = msg.MapId;
            notify.ChannelId = msg.ChannelId;
            notify.PosX = msg.PosX;
            notify.PosY = msg.PosY;
            notify.Result = msg.Result;
            member.CurZone.Write(notify);
        }

        public void OnResponse_ChangeTeamType(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHANGE_TEAM_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHANGE_TEAM_TYPE>(stream);
            Client captain = ZoneManager.GetClient(msg.CapUid);
            if (captain == null) return;

            MSG_RZ_CHANGE_TEAM_TYPE response = new MSG_RZ_CHANGE_TEAM_TYPE();
            response.Uid = msg.CapUid;

            if (captain.Team == null)
            {
                response.Result = (int)ErrorCode.NoTeam;
                captain.CurZone.Write(response);
                return;
            }


            if (captain.Team.Type == msg.TeamType)
            {
                Log.Warn("player {0} change same team type {1}", msg.CapUid, msg.TeamType);
                response.Result = (int)ErrorCode.SameTeamType;
                Write(response);
                return;
            }

            if (captain.Team.CaptainUid != captain.Uid)
            {
                response.Result = (int)ErrorCode.NotTeamCaptain;
                Write(response);
                return;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(msg.TeamType);
            if (model == null)
            {
                response.Result = (int)ErrorCode.NoTeamType;
                Write(response);
                return;
            }

            //if (!model.CheckMemberCountLimit(captain.Team.MemberCount))
            //{
            //    response.Result = (int)ErrorCode.DungeonMemberCountLimit;
            //    Write(response);
            //    return;
            //}

            foreach (var member in captain.Team.MemberList)
            {
                if (member.Value.IsRobot || member.Value.IsAllowOffline) continue;

                Client memClient = ZoneManager.GetClient(member.Value.Uid);

                if (memClient != null)
                {
                    if (memClient?.Level < model.MinLevel)
                    {
                        response.Result = (int)ErrorCode.TeamMemberLevelLimit;
                        response.Uid = msg.CapUid;
                        response.LimitUid = member.Key;
                        Write(response);
                        return;
                    }

                    if (memClient?.ChapterId < model.ChapterLimit)
                    {
                        response.Result = (int)ErrorCode.TeamMemberChapterTaskNotFinish;
                        Write(response);
                        return;
                    }
                }
            }

            captain.Team.SetTeamType(msg.TeamType, model);
        }

        public void OnResponse_NewTeamDunegon(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_NEW_TEAM_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_NEW_TEAM_DUNGEON>(stream);
            Client owner = ZoneManager.GetClient(msg.OwnerUid);
            if (owner == null)
            {
                Log.Warn("player {0} create team dungeon id {1} uid {2} failed: can not find client", msg.OwnerUid, msg.MapId, msg.Channel);
                return;
            }

            if (owner.Team == null)
            {
                Log.Warn("player {0} create team dungeon id {1} uid {2} failed: team is null", msg.OwnerUid, msg.MapId, msg.Channel);
                return;
            }

            owner.Team.InDungeon = true;

            //通知队伍成员进入副本
            owner.Team.NotifyCreateTeamDungeon(msg.MapId, msg.Channel, msg.MainId, msg.SubId);
        }

        public void OnResponse_TeamQuitDunegon(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_TEAM_QUIT_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_TEAM_QUIT_DUNGEON>(stream);
            Team team = ZoneManager.TeamManager.GetTeam(msg.TeamId);
            if (team != null)
            {
                team.InDungeon = false;
            }
        }

        public void OnResponse_NeedTeamHelp(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_NEED_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_NEED_TEAM_HELP>(stream);
            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null) return;

            MSG_RZ_NEED_TEAM_HELP response = new MSG_RZ_NEED_TEAM_HELP();

            var team = client.Team;
            if (team == null || team.Type == 0 || team.CaptainUid != client.Uid)
            {
                response.Result = (int)ErrorCode.Fail;
                client.CurZone.Write(response, msg.Uid);
                return;
            }

            var info = new TeamHelpInfo() { TeamId = team.TeamId, FirstSendTime = BaseApi.now, SendTimes = new List<DateTime>() { BaseApi.now }, HelpSenderInfo = msg };
            ZoneManager.TeamManager.NeedTamHelp(info, false, 1, client);

            //单人邀请不需要自动添加机器人
            if (msg.FriendUid <= 0)
            {
                response.Result = (int)ErrorCode.Success;
                client.CurZone.Write(response, msg.Uid);

                ZoneManager.TeamManager.AddToHelpList(info);
            }
        }

        public void OnResponse_ResponseTeamHelp(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_RESPONSE_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_RESPONSE_TEAM_HELP>(stream);
            MSG_RZ_RESPONSE_TEAM_HELP response = new MSG_RZ_RESPONSE_TEAM_HELP
            {
                Uid = msg.Uid,
            };

            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null)
            {
                Log.Warn("player {0} OnResponse_ResponseTeamHelp failed: no such client", msg.Uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (client.Team != null)
            {
                client.Team.RemoveMember(msg.Uid);
            }

            Team team = ZoneManager.TeamManager.GetTeam(msg.TeamId);
            if (team == null || team.InDungeon)
            {
                response.Result = (int)ErrorCode.NoTeam;
                Write(response);
                return;
            }

            response.DungeonId = team.Type;

            if (team.CheckFull())
            {
                Log.Warn("player {0} OnResponse_ResponseTeamHelp failed: team full", msg.Uid);
                response.Result = (int)ErrorCode.TeamFull;
                Write(response);
                return;
            }

            RedisPlayerInfo playerInfo = Api.RPlayerInfoMng.GetPlayerInfo(msg.Uid);
            if (playerInfo != null)
            {
                TeamMember member = new TeamMember(client, playerInfo);
                team.AddMember(member);

                client.Team = team;

                //通知创建队伍信息
                client.NotifyTeamInfo();

                response.Result = (int)ErrorCode.Success;
                Write(response);
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);

                Log.Warn("player {0} OnResponse_ResponseTeamHelp failed: redis error", msg.Uid);
                return;
            }
        }

        public void OnResponse_QuitTeamRobot(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_QUIT_TEAM_ROBOT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_QUIT_TEAM_ROBOT>(stream);
            Log.Write("player {0} request quit team robot {1}", msg.Uid, msg.TeamID);

            Team team = ZoneManager.TeamManager.GetTeam(msg.TeamID);
            if (team == null)
            {
                return;
            }
        }

        public void OnResponse_EnterDungeon(MemoryStream stream, int uid = 0)
        {
            Client client = ZoneManager.GetClient(uid);
            client?.SetInDungeon(true);
        }

        public void OnResponse_LeaveDungeon(MemoryStream stream, int uid = 0)
        {
            Client client = ZoneManager.GetClient(uid);
            client?.SetInDungeon(false);
        }

        public void OnResponse_InviteFriendJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_INVITE_FRIEND_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_INVITE_FRIEND_JOIN_TEAM>(stream);

            MSG_RZ_INVITE_JOIN_TEAM response = new MSG_RZ_INVITE_JOIN_TEAM();
            response.CapUid = uid;

            Client captain = ZoneManager.GetClient(uid);

            if (captain == null)
            {
                Log.Warn($"player {uid}  OnResponse_InviteFriendJoinTeam failed: client no exist");
                return;
            }

            Team team = captain.Team;

            if (team != null)
            {
                if (team.InDungeon)
                {
                    response.Result = (int)ErrorCode.InDungeon;
                    Write(response);
                    return;
                }

                if (!team.IsFreeTeam)
                {
                    DungeonModel model = DungeonLibrary.GetDungeon(team.Type);
                    if (model == null || model.TeamLimit)
                    {
                        response.Result = (int)ErrorCode.MapTeamLimit;
                        Write(response);
                        return;
                    }

                    if (model.ChapterLimit > captain.ChapterId)
                    {
                        response.Result = (int)ErrorCode.ChapterTaskNotFinish;
                        Write(response);
                        return;
                    }
                }

                if (team.CaptainUid != captain.Uid)
                {
                    response.Result = (int)ErrorCode.NotTeamCaptain;
                    Write(response);
                    return;
                }

                if (team.CheckFull())
                {
                    response.Result = (int)ErrorCode.TeamFull;
                    Write(response);
                    return;
                }
            }

            Client frinedClient = ZoneManager.GetClient(msg.Friend);
            if (frinedClient?.IsOnline == true)
            {
                if (frinedClient.IsInDungeon())
                {
                    response.Result = (int)ErrorCode.InDungeon;
                    Write(response);
                    return;
                }

                if (frinedClient.Team != null)
                {
                    response.Result = (int)ErrorCode.InTeam;
                    Write(response);
                    return;
                }

                if (captain.Team != null)
                {
                    DungeonModel model = DungeonLibrary.GetDungeon(captain.Team.Type);
                    if (model == null || model.TeamLimit == true)
                    {
                        response.Result = (int)ErrorCode.MapTeamLimit;
                        Write(response);
                        return;
                    }

                    if (!model.CheckLevelLimit(frinedClient.Level))
                    {
                        response.Result = (int)ErrorCode.LevelLimit;
                        Write(response);
                        return;
                    }

                    if (model.ChapterLimit > frinedClient.ChapterId || !model.CheckLevelLimit(frinedClient.Level))
                    {
                        response.Result = (int)ErrorCode.ChapterTaskNotFinish;
                        Write(response);
                        return;
                    }
                }

                RedisPlayerInfo playerInfo = Api.RPlayerInfoMng.GetPlayerInfo(uid);
                if (playerInfo == null)
                {
                    OperateGetPlayerInfo operatePlayerInfo = new OperateGetPlayerInfo(uid);
                    Api.GameRedis.Call(operatePlayerInfo, ret =>
                    {
                        if ((int)ret == 1)
                        {
                            MSG_RZ_REQUEST_TEAM_HELP notify = new MSG_RZ_REQUEST_TEAM_HELP()
                            {
                                TeamId = team.TeamId,
                                TeamType = team.Type,
                                Name = operatePlayerInfo.Info.GetStringValue(HFPlayerInfo.Name),
                                Level = captain.Level,
                                Camp = operatePlayerInfo.Info.GetIntValue(HFPlayerInfo.CampId),
                                Uid = captain.Uid,
                                Research = captain.Research,
                            };
                            frinedClient.CurZone?.Write(notify);
                        }
                    });
                }
                else
                {
                    MSG_RZ_REQUEST_TEAM_HELP notify = new MSG_RZ_REQUEST_TEAM_HELP()
                    {
                        TeamId = team.TeamId,
                        TeamType = team.Type,
                        Name = playerInfo.GetStringValue(HFPlayerInfo.Name),
                        Level = captain.Level,
                        Camp = playerInfo.GetIntValue(HFPlayerInfo.CampId),
                        Uid = captain.Uid,
                        Research = captain.Research,
                    };
                    frinedClient.CurZone?.Write(notify);
                }

                //满足邀请条件发出邀请
                //MSG_RZ_ASK_INVITE_JOIN_TEAM request = new MSG_RZ_ASK_INVITE_JOIN_TEAM();
                //request.CapUid = captain.Uid;
                //request.CapLevel = captain.Level;
                //request.CapName = playerInfo.GetStringValue(HFPlayerInfo.Name);
                //request.InviteUid = frinedClient.Uid;
                //frinedClient.CurZone?.Write(request);
            }
        }

        public void OnResponse_TransferDone(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_TRANSFORM_DONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_TRANSFORM_DONE>(stream);
            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null || client.Team == null) return;

            MSG_RZ_JOIN_TEAM response = new MSG_RZ_JOIN_TEAM();
            response.Team = client.Team.GenerateTeamInfo();
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void OnResponse_NotifyTeamContinueHunting(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_NOTIFY_TEAM_CONT_HUNTING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_NOTIFY_TEAM_CONT_HUNTING>(stream);
            MSG_RZ_NOTIFY_TEAM_CONT_HUNTING response = new MSG_RZ_NOTIFY_TEAM_CONT_HUNTING();

            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null)
            {
                Log.Warn("player {0} notify team continue hunting failed: no such client", uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            response.Uid = msg.Uid;
            response.Contine = msg.Continue;

            Team team = ZoneManager.TeamManager.GetTeam(msg.TeamId);
            if (team == null)
            {
                //response.Result = (int)ErrorCode.NoTeam;
                response.Result = (int)ErrorCode.Success;
                Write(response);
                return;
            }

            //if (!team.CheckEnough())
            //{
            //    response.Result = (int)ErrorCode.HeroNumMinLimit;
            //    Write(response);
            //    return;
            //}

            team.NotifyMemberContinueHunting(msg.Continue);
        }

        public void OnResponse_HuntingHelpAsk(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_HUNTING_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_HUNTING_HELP>(stream);
            MSG_RZ_HUNTING_HELP response = new MSG_RZ_HUNTING_HELP() { Uid = uid, DungeonId = msg.DungeonId };

            Client captain = ZoneManager.GetClient(uid);
            if (captain == null)
            {
                Log.Warn("player {0} notify team continue hunting failed: no such client", uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (captain.Team == null)
            {
                response.Result = (int)ErrorCode.NotInTeam;
                Write(response);
                return;
            }


            Team team = captain.Team;
            TeamMember member = team.MemberList.Values.Where(x => x.Uid != uid).FirstOrDefault();
            if (member == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            Client inviter = ZoneManager.GetClient(member.Uid);
            if (inviter == null || !inviter.IsOnline)
            {
                response.Result = (int)ErrorCode.TeamMemberOfflineOfDungeon;
                Write(response);
                return;
            }

            if (team.CaptainUid != captain.Uid)
            {
                response.Result = (int)ErrorCode.NotTeamCaptain;
                Write(response);
                return;
            }

            if (team.InDungeon)
            {
                response.Result = (int)ErrorCode.InDungeon;
                Write(response);
                return;
            }

            if (!team.IsFreeTeam)
            {
                DungeonModel model = DungeonLibrary.GetDungeon(team.Type);
                if (model == null || model.TeamLimit)
                {
                    response.Result = (int)ErrorCode.MapTeamLimit;
                    Write(response);
                    return;
                }

                if (model.ChapterLimit > captain.ChapterId)
                {
                    response.Result = (int)ErrorCode.ChapterTaskNotFinish;
                    Write(response);
                    return;
                }

                if (model.ChapterLimit > inviter.ChapterId)
                {
                    response.Result = (int)ErrorCode.InviterChapterTaskNotFinish;
                    Write(response);
                    return;
                }
            }

            if (inviter.IsInDungeon())
            {
                response.Result = (int)ErrorCode.InDungeon;
                Write(response);
                return;
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);

            MSG_RZ_HUNTING_HELP_ASK request = new MSG_RZ_HUNTING_HELP_ASK()
            {
                CapUid = uid,
                CapName = msg.CapName,
                CapLevel = msg.CapLevel,
                DungeonId = msg.DungeonId,
                InviteUid = inviter.Uid
            };
            inviter.Write(request);
        }

        public void OnResponse_HuntingHelpAnswer(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_HUNTING_HELP_ANSWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_HUNTING_HELP_ANSWER>(stream);

            Client captain = ZoneManager.GetClient(msg.CapUid);
            if (captain == null || !captain.IsOnline)
            {
                return;
            }

            if (captain.Team == null)
            {
                return;
            }

            Team team = captain.Team;
            TeamMember member = team.GetTeamMember(uid);
            if (member == null) return;

            MSG_RZ_HUNTING_HELP_ANSWER_JOIN response = new MSG_RZ_HUNTING_HELP_ANSWER_JOIN() { Agree = msg.Agree, Uid = captain.Uid };
            captain.Write(response);
        }
    }
}
