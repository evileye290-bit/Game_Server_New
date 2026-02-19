using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        public void OnResponse_TeamTypeList(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TEAM_TYPE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TEAM_TYPE_LIST>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_TEAM_TYPE_LIST teams = new MSG_ZGC_TEAM_TYPE_LIST();
            teams.Result = msg.Result;
            teams.Page = msg.Page;
            teams.OwnTeamId = msg.OwnTeamId;
            teams.ChallengeNum = player.GetDungeonChallengeCount(msg.TeamType);//挑战次数

            //队伍信息
            if (msg.Teams.Count > 0)
            {
                MSG_ZGC_TEAM_INFO team;
                ZGC_TEAM_MEMBER_INFO member;
                foreach (var kv in msg.Teams)
                {
                    team = new MSG_ZGC_TEAM_INFO()
                    {
                        TeamType = kv.TeamType,
                        TeamId = kv.TeamId,
                        CaptainUid = kv.CaptainUid,
                    };

                    foreach (var mem in kv.Members)
                    {
                        member = new ZGC_TEAM_MEMBER_INFO()
                        {
                            Uid = mem.Uid,
                            Name = mem.Name,
                            Sex = mem.Sex,
                            Icon = mem.Sex,
                            IconFrame = mem.IconFrame,
                            Level = mem.Level,
                            Job = mem.Job,
                            Camp = mem.Camp,
                            IsOnline = mem.IsOnline,
                            HeroId = mem.HeroId,
                            BattlePower = mem.BattlePower,
                            GodType = mem.GodType,
                            Research = mem.Research,
                        };
                        team.Members.Add(member);
                    }

                    teams.Teams.Add(team);
                }
            }
            player.Write(teams);
        }

        public void OnResponse_CreateTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CREATE_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CREATE_TEAM>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_CREATE_TEAM notify = new MSG_ZGC_CREATE_TEAM();
            notify.Result = msg.Result;
            if (msg.Result == (int)ErrorCode.Success)
            {
                player.ChangeTeam(msg.Team);
                notify.Team = player.Team.GenerateTeamInfo();
                //komoelog
                player.KomoeLogRecordTeamFlow(1, msg.Team);
            }

            player.Write(notify);
        }

        public void OnResponse_JoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_JOIN_TEAM>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_JOIN_TEAM notify = new MSG_ZGC_JOIN_TEAM();
            notify.Result = msg.Result;
            if (msg.Result == (int)ErrorCode.Success)
            {
                player.ChangeTeam(msg.Team);
                notify.Team = player.Team.GenerateTeamInfo();
                player.KomoeLogRecordTeamFlow(3, msg.Team);
            }

            player.Write(notify);
        }

        public void OnResponse_NewTeamMemberJoin(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NEW_TEAM_MEMBER_JOIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NEW_TEAM_MEMBER_JOIN>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);

            if (player == null || player.Team == null || player.Uid == msg.Member.Uid) return; ;

            var kv = msg.Member;
            var member = new TeamMember()
            {
                Uid = kv.Uid,
                Name = kv.Name,
                Sex = kv.Sex,
                Level = kv.Level,
                Icon = kv.Icon,
                IconFrame = kv.IconFrame,
                Job = kv.Job,
                Camp = (CampType)kv.Camp,
                IsOnline = kv.IsOnline,
                HeroId = kv.HeroId,
                IsRobot = kv.IsRobot,
                BattlePower = kv.BattlePower,
                Chapter = kv.Chapter,
                IsAllowOffline = kv.IsAllowOffline,
                GodType = kv.GodType,
                Research = kv.Research
            };

            player.Team.IsInviteMirror = msg.IsInviteMirror;
            player.Team.AddMember(member, true, msg.Full);
        }

        public void OnResponse_TeamMemberLeave(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TEAM_MEMBER_LEAVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TEAM_MEMBER_LEAVE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }

            if (player.Team != null)
            {
                player.Team.RemoveMember(msg.LeaveUid);
                player.RemoveLeaveMember(msg.LeaveUid);
            }
        }

        public void OnResponse_QuitTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_QUIT_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_QUIT_TEAM>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_QUIT_TEAM response = new MSG_ZGC_QUIT_TEAM();
            response.Result = msg.Result;

            if (response.Result == (int)ErrorCode.Success)
            {
                player.LeaveTeam();
                player.RemoveLeaveMember(msg.Uid);
                //komoelog
                player.KomoeEventLogTeamFlow(6, "0", null);
            }

            player.Write(response);
        }

        public void OnResponse_KickTeamMember(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_KICK_TEAM_MEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_KICK_TEAM_MEMBER>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.Team == null)
            {
                return;
            }

            if (msg.Result == (int)ErrorCode.Success)
            {
                if (player.Uid == msg.KickUid)
                {
                    // 被踢
                    player.LeaveTeam();
                }
                else
                {
                    player.Team.RemoveMember(msg.KickUid);
                }
            }

            //  通知踢人结果
            MSG_ZGC_KICK_TEAM_MEMBER notify = new MSG_ZGC_KICK_TEAM_MEMBER();
            notify.Result = msg.Result;
            notify.KickUid = msg.KickUid;
            player.Write(notify);
        }

        public void OnResponse_TransferCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TRANDSFER_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TRANDSFER_CAPTAIN>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.Team == null)
            {
                return;
            }

            if (msg.Result == (int)ErrorCode.Success)
            {
                player.Team.CaptainUid = msg.NewCapUid;
            }

            MSG_ZGC_TRANSFER_CAPTAIN notify = new MSG_ZGC_TRANSFER_CAPTAIN();
            notify.Result = msg.Result;
            notify.NewCapUid = msg.NewCapUid;
            player.Write(notify);
        }

        public void OnResponse_CaptainChange(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAPTAIN_CHANGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAPTAIN_CHANGE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.Team == null)
            {
                return;
            }

            player.Team.CaptainUid = msg.NewCapUid;
            MSG_ZGC_CAPTAIN_CHANGE notify = new MSG_ZGC_CAPTAIN_CHANGE();
            notify.NewCapUid = msg.NewCapUid;
            player.Write(notify);
        }

        public void OnResponse_TeamMemberOffline(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TEAM_MEMBER_OFFLINE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TEAM_MEMBER_OFFLINE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.Team == null)
            {
                return;
            }

            player.Team.MemberOffline(msg.MemberUid, msg.CapUid);

            MSG_ZGC_TEAM_MEMBER_OFFLINE notify = new MSG_ZGC_TEAM_MEMBER_OFFLINE();
            notify.MemberUid = msg.MemberUid;
            notify.CapUid = msg.CapUid;
            player.Write(notify);
        }

        public void OnResponse_TeamMemberOnline(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TEAM_MEMBER_ONLINE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TEAM_MEMBER_ONLINE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.Team == null)
            {
                return;
            }

            player.Team.MemberOnline(msg.MemberUid, player);
            //player.CheckAndDestoryTeam();

            MSG_ZGC_TEAM_MEMBER_ONLINE notify = new MSG_ZGC_TEAM_MEMBER_ONLINE();
            notify.MemberUid = msg.MemberUid;
            player.Write(notify);
        }

        public void OnResponse_AskJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_ASK_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ASK_JOIN_TEAM>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_ASK_JOIN_TEAM response = new MSG_ZGC_ASK_JOIN_TEAM();

            if (msg.Result == (int)ErrorCode.Success)
            {
                player.ChangeTeam(msg.Team);
                response.Team = player.Team.GenerateTeamInfo();
                player.KomoeLogRecordTeamFlow(2, msg.Team);
            }

            response.Result = msg.Result;
            player.Write(response);
        }

        private void OnResponse_InviteJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_INVITE_JOIN_TEAM>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.CapUid);
            if (player == null)
            {
                return;
            }

            if (msg.Result == (int)ErrorCode.InviteOfflineBrotherSuccess)
            {
                if (player.CheckBrotherExist(msg.InviteUid))
                {
                    MSG_ZR_ANSWER_INVITE_JOIN_TEAM notiy = new MSG_ZR_ANSWER_INVITE_JOIN_TEAM();
                    notiy.Agree = true;
                    notiy.CapUid = msg.CapUid;
                    notiy.InviterUid = msg.InviteUid;
                    notiy.InviterName = msg.InviterName;
                    notiy.IsOfflineBrother = true;
                    notiy.InviteMirror = msg.InviteMirror;
                    player.server.SendToRelation(notiy, uid);
                }
                else
                {
                    msg.Result = (int)ErrorCode.TeamInviterOffline;
                }
            }
            //komoelog
            int operateType = 4;
            if (msg.InviteMirror)
            {
                operateType = 5;
            }
            if (player.Team != null)
            {              
                player.KomoeLogRecordTeamFlow(operateType);
            }

            player.Write(new MSG_ZGC_INVITE_JOIN_TEAM() { Result = msg.Result });
        }

        public void OnResponse_AskInviteJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_ASK_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ASK_INVITE_JOIN_TEAM>(stream);
            PlayerChar inviter = Api.PCManager.FindPc(msg.InviteUid);

            if (inviter == null || inviter.NotStableInMap() || inviter.InDungeon)
            {
                return;
            }

            MSG_ZGC_ASK_INVITE_JOIN_TEAM notify = new MSG_ZGC_ASK_INVITE_JOIN_TEAM();
            notify.InviteUid = msg.InviteUid;
            notify.CapUid = msg.CapUid;
            notify.CapName = msg.CapName;
            notify.CapLevel = msg.CapLevel;
            notify.Research = msg.Research;
            inviter.Write(notify);
        }

        public void OnResponse_AnswerInviteJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_ANSWER_INVITE_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ANSWER_INVITE_JOIN_TEAM>(stream);
            PlayerChar captain = Api.PCManager.FindPc(msg.Uid);
            if (captain == null)
            {
                return;
            }

            MSG_ZGC_ANSWER_INVITE_JOIN_TEAM notify = new MSG_ZGC_ANSWER_INVITE_JOIN_TEAM();
            notify.Result = msg.Result;
            captain.Write(notify);
        }

        public void OnResponse_AskFollowCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_ASK_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ASK_FOLLOW_CAPTAIN>(stream);
            PlayerChar captain = Api.PCManager.FindPc(msg.CapUid);

            if (captain == null || captain.Team == null || captain.NotStableInMap() || captain.Team.CaptainUid != captain.Uid)
            {
                return;
            }

            MSG_ZR_ANSWER_FOLLOW_CAPTAIN response = new MSG_ZR_ANSWER_FOLLOW_CAPTAIN();
            response.MemberUid = msg.MemberUid;

            if (captain.InDungeon)
            {
                response.Result = (int)ErrorCode.CapInDungeon;
                Write(response);

                //if (captain.CurDungeon.DungeonModel.TeamLimit)
                //{
                //    return;
                //}

                //// 副本中 如果是组队副本 允许拉入
                //if (!captain.CurrentMap.Model.IsTeamDungeon())
                //{
                //    return;
                //}
            }

            // 检查通过
            response.MapId = captain.CurrentMap.MapId;
            response.ChannelId = captain.CurrentMap.Channel;
            if (captain.CurrentMap.PVPType == PvpType.None)
            {
                // 非PVP地图 随到队长身边
                response.PosX = captain.Position.X;
                response.PosY = captain.Position.Y;
            }
            else
            {
                // PVP地图 随到地图出生点
                Vec2 beginPos = Api.MapManager.GetBeginPosition(captain.CurrentMap.MapId);
                response.PosX = beginPos.X;
                response.PosY = beginPos.Y;
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void OnResponse_TryAskFollowCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TRY_ASK_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TRY_ASK_FOLLOW_CAPTAIN>(stream);
            PlayerChar captain = Api.PCManager.FindPc(msg.CapUid);

            MSG_ZR_TRY_ANSWER_FOLLOW_CAPTAIN response = new MSG_ZR_TRY_ANSWER_FOLLOW_CAPTAIN();
            response.MemberUid = msg.MemberUid;
            if (captain == null || captain.Team == null || captain.NotStableInMap() || captain.Team.CaptainUid != captain.Uid)
            {
                response.Result = (int)ErrorCode.CaptainNotInTeam;
                Write(response);
                return;
            }

            if (captain.InDungeon)
            {
                response.Result = (int)ErrorCode.CapInDungeon;
                Write(response);
            }

            // 检查通过
            response.MapId = captain.CurrentMap.MapId;
            response.ChannelId = captain.CurrentMap.Channel;
            if (captain.CurrentMap.PVPType == PvpType.None)
            {
                // 非PVP地图 随到队长身边
                response.PosX = captain.Position.X;
                response.PosY = captain.Position.Y;
            }
            else
            {
                // PVP地图 随到地图出生点
                Vec2 beginPos = Api.MapManager.GetBeginPosition(captain.CurrentMap.MapId);
                response.PosX = beginPos.X;
                response.PosY = beginPos.Y;
            }
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void OnResponse_AnswerFollowCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_ANSWER_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ANSWER_FOLLOW_CAPTAIN>(stream);
            PlayerChar member = Api.PCManager.FindPc(msg.MemberUid);
            if (member == null || member.Team == null || member.NotStableInMap() || member.CurrentMap.IsDungeon)
            {
                Log.Write("player {0} will follow captain to map {1} channel {2} failed: in limit condition", msg.MemberUid, msg.MapId, msg.ChannelId);
                return;
            }

            if (msg.Result != (int)ErrorCode.Success)
            {
                MSG_ZGC_FOLLOW_CAPTAIN notify = new MSG_ZGC_FOLLOW_CAPTAIN();
                notify.Result = msg.Result;
                member.Write(notify);
                return;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(msg.MapId);
            if (model != null)
            {
                //非组队副本
                if (model.TeamLimit)
                {
                    MSG_ZGC_FOLLOW_CAPTAIN notify = new MSG_ZGC_FOLLOW_CAPTAIN();
                    notify.Result = (int)ErrorCode.CapMapTeamLimit;
                    notify.MapId = msg.MapId;
                    member.Write(notify);
                    return;
                }

                // 等级不足
                if (member.Level < model.MinLevel)
                {
                    MSG_ZGC_FOLLOW_CAPTAIN notify = new MSG_ZGC_FOLLOW_CAPTAIN();
                    notify.Result = (int)ErrorCode.CapDungeonLevelLimit;
                    notify.MapId = msg.MapId;
                    member.Write(notify);
                    return;
                }
            }
            MSG_ZGC_FOLLOW_CAPTAIN response = new MSG_ZGC_FOLLOW_CAPTAIN();
            response.Result = (int)ErrorCode.Success;

            if (member.CurrentMap.MapId == msg.MapId && member.CurrentMap.Channel == msg.ChannelId)
            {
                response.NeedFly = false;
                member.Write(response);
                member.SetDestination(new Vec2(msg.PosX, msg.PosY));
            }
            else
            {
                response.NeedFly = true;
                member.Write(response);
                member.AskForEnterMap(msg.MapId, msg.ChannelId, new Vec2(msg.PosX, msg.PosY), true, true);
            }
        }

        public void OnResponse_TryAnswerFollowCaptain(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TRY_ANSWER_FOLLOW_CAPTAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TRY_ANSWER_FOLLOW_CAPTAIN>(stream);
            PlayerChar member = Api.PCManager.FindPc(msg.MemberUid);
            if (member == null || member.Team == null || member.NotStableInMap() || member.CurrentMap.IsDungeon)
            {
                Log.Write("player {0} will follow captain to map {1} channel {2} failed: in limit condition", msg.MemberUid, msg.MapId, msg.ChannelId);
                return;
            }

            if (msg.Result != (int)ErrorCode.Success)
            {
                MSG_ZGC_TRY_FOLLOW_CAPTAIN notify = new MSG_ZGC_TRY_FOLLOW_CAPTAIN();
                notify.Result = msg.Result;
                member.Write(notify);
                return;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(msg.MapId);
            if (model != null)
            {
                //非组队副本
                if (model.TeamLimit)
                {
                    MSG_ZGC_TRY_FOLLOW_CAPTAIN notify = new MSG_ZGC_TRY_FOLLOW_CAPTAIN();
                    notify.Result = (int)ErrorCode.CapMapTeamLimit;
                    notify.MapId = msg.MapId;
                    member.Write(notify);
                    return;
                }

                // 等级不足
                if (member.Level < model.MinLevel)
                {
                    MSG_ZGC_TRY_FOLLOW_CAPTAIN notify = new MSG_ZGC_TRY_FOLLOW_CAPTAIN();
                    notify.Result = (int)ErrorCode.CapDungeonLevelLimit;
                    notify.MapId = msg.MapId;
                    member.Write(notify);
                    return;
                }
            }
            MSG_ZGC_TRY_FOLLOW_CAPTAIN response = new MSG_ZGC_TRY_FOLLOW_CAPTAIN();
            response.Result = (int)ErrorCode.Success;

            if (member.CurrentMap.MapId == msg.MapId && member.CurrentMap.Channel == msg.ChannelId)
            {
                response.NeedFly = false;
                member.Write(response);
            }
            else
            {
                response.NeedFly = true;
                member.Write(response);
            }
        }

        public void OnResponse_ChangeTeamType(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHANGE_TEAM_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHANGE_TEAM_TYPE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.Team == null) return;

            MSG_ZGC_CHANGE_TEAM_TYPE response = new MSG_ZGC_CHANGE_TEAM_TYPE();
            response.TeamType = msg.TeamType;
            response.Result = msg.Result;

            if (msg.Result == (int)ErrorCode.Success)
            {
                // 成功发布 通知成员队伍类型变动
                player.Team.Type = msg.TeamType;
                player.Write(response);
            }
            else
            {
                // 失败 通知队长原因
                if (player.Uid == player.Team.CaptainUid)
                {
                    response.LimitUid = msg.LimitUid;
                    player.Write(response);
                }
            }
        }

        public void OnResponse_TeamMemberChangeZone(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TEAM_MEMEBR_CHANGE_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TEAM_MEMEBR_CHANGE_ZONE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.Team == null) return;
            player.Team.MemberChangeZone(msg.MemberUid, msg.SubId, player);
        }

        public void OnResponse_NewTeamDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NEW_TEAM_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NEW_TEAM_DUNGEON>(stream);
            Log.Write("relation notify player {0} to enter team dungeon map {1} channel {2}", msg.Uid, msg.MapId, msg.Channel);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player != null)
            {
                if (player.NotStableInMap())
                {
                    Log.Write("relation notify player {0} to enter team dungeon id {1} uid {2} failed: in limit condition", msg.Uid, msg.MapId, msg.Channel);
                    return;
                }

                // 已经在副本中，则无需拉入
                if (player.InDungeon)
                {
                    return;
                }

                // channel高8位是dungeon所在zone的sub id
                //int dungeonUid = msg.Channel & 0xFFFFFF;
                MapModel model = MapLibrary.GetMap(msg.MapId);
                if (model == null)
                {
                    return;
                }

                if (Api.SubId == msg.Channel >> 24)
                {
                    FieldMap dungeon = Api.MapManager.GetFieldMap(msg.MapId, msg.Channel);
                    if (dungeon != null)
                    {
                        player.RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
                        player.RecordOriginMapInfo();
                        player.OnMoveMap();
                    }
                }
                else
                {
                    // 副本不在当前zone 
                    player.AskForEnterMap(msg.MapId, msg.Channel, model.BeginPos, true);
                }
            }
        }

        public void OnResponse_NewTeamDungeon4Robot(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TRY_CREATE_ROBOT_MEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TRY_CREATE_ROBOT_MEMBER>(stream);
            Log.Write("relation notify player {0} to enter team dungeon map {1} channel {2}", msg.OwnerUid, msg.MapId, msg.Channel);
            PlayerChar player = Api.PCManager.FindPc(msg.OwnerUid);

            //找到副本并拉入,副本肯定在captain所在的服务器

            MapModel model = MapLibrary.GetMap(msg.MapId);
            if (model == null)
            {
                return;
            }

            if (Api.SubId == msg.Channel >> 24)
            {
                FieldMap dungeon = Api.MapManager.GetFieldMap(msg.MapId, msg.Channel);
                if (dungeon != null)
                {
                    if (msg.TeamRobotId == 0 && msg.RobotNatureRatio == 0)
                    {
                        //if (player.CheckTeamMirror(msg.RobotUid))
                        //{
                        //    CallOfflineBrother2Map(player, player.Team.MirrorPlayer, dungeon as TeamDungeonMap,
                        //        player.Team.MirrorPlayer.HuntingManager.Research);
                        //}
                        //else
                        {
                            LoadBattlePlayerInfoWithQuerys((int)ChallengeIntoType.TeamOffline, msg.RobotUid, dungeon, msg.OwnerUid, null, player);
                        }
                    }
                    else
                    {
                        List<HeroInfo> infos = RobotManager.GetTeamHeroList(msg.RobotNatureRatio, msg.TeamRobotId, msg.TeamLevel);
                        HeroInfo temp = infos.First();
                        temp.RobotInfo.Name = msg.Name;
                        temp.RobotInfo.Sex = msg.Sex;
                        List<int> poses = RobotManager.GetTeamHeroPosList(msg.TeamRobotId);

                        Dictionary<int, int> heroPos = new Dictionary<int, int>();
                        foreach (var pos in poses)
                        {
                            heroPos.Add(infos[poses.IndexOf(pos)].Id, pos);
                        }
                        Dictionary<int, int> natureValues = new Dictionary<int, int>();
                        Dictionary<int, int> natureRatios = new Dictionary<int, int>();

                        if (player != null)
                        {
                            natureValues = player.NatureValues;
                            natureRatios = player.NatureRatios;
                        }
                        (dungeon as TeamDungeonMap)?.AddAttackerTeamRobot(infos, msg.RobotUid, natureValues, natureRatios, heroPos);
                    }
                }
            }
            else
            {
                // 副本不在当前zone
                Log.Warn($"try get team dungoenMap {msg.MapId} channel {msg.Channel} for OnResponse_NewTeamDungeon4Robot captainUid {msg.OwnerUid}");
            }
        }


        public void CallOfflineBrother2Map(PlayerChar captain, PlayerChar player, TeamDungeonMap teamDungeon, int research)
        {
            List<HeroInfo> heroInfos = player.HeroMng.GetEquipHeros().Values.ToList();
            player.SetHeroInfoRobotSoulRings(heroInfos);
            Dictionary<int, int> heroPos = new Dictionary<int, int>();
            foreach (var item in player.HeroMng.GetHeroPos())
            {
                heroPos.Add(item.Key, item.Value);
            }

            if (teamDungeon != null)
            {
                captain.Team?.SetMirror(player);
                player.SetCurrentMap(teamDungeon);
                teamDungeon.AddAttackerMirror(player, heroPos);
                (teamDungeon as HuntingTeamDungeonMap)?.SetOfflineBrother(player);
            }
        }


        public void OnResponse_NeedTeamHelp(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NEED_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NEED_TEAM_HELP>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_NEED_TEAM_HELP notify = new MSG_ZGC_NEED_TEAM_HELP();
            notify.Result = msg.Result;
            player.Write(notify);
        }

        public void OnResponse_RequestTeamHelp(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_REQUEST_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_REQUEST_TEAM_HELP>(stream);
            MSG_ZGC_REQUEST_TEAM_HELP notify = new MSG_ZGC_REQUEST_TEAM_HELP()
            {
                Uid = msg.Uid,
                Name = msg.Name,
                Level = msg.Level,
                TeamId = msg.TeamId,
                TeamType = msg.TeamType,
                Research = msg.Research,
            };

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null || player.NotStableInMap() || player.InDungeon || player.CheckBlackExist(msg.Uid))
            {
                return;
            }
            player.Write(notify);
        }

        public void OnResponse_ResponseTeamHelp(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_RESPONSE_TEAM_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_RESPONSE_TEAM_HELP>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_RESPONSE_TEAM_HELP response = new MSG_ZGC_RESPONSE_TEAM_HELP()
            {
                Result = msg.Result,
                DungeonId = msg.DungeonId,
            };
            player.Write(response);
        }

        public void OnResponse_AskPVPChallenge(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_ASK_PVP_CHALLENGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ASK_PVP_CHALLENGE>(stream);
            //Log.Write("relation notify player {0} ask pvp challenge player {1}", msg.sourceUid, msg.destUid);
            //PlayerChar dest = server.PCManager.FindPc(msg.destUid);
            //if (dest == null)
            //{
            //    return;
            //}
            //if (dest.IsTransforming || dest.IsLeavingZone || dest.IsInDungeon || dest.Team != null)
            //{
            //    return;
            //}
            //PKS_ZC_ASK_PVP_CHALLENGE notify = new PKS_ZC_ASK_PVP_CHALLENGE();
            //notify.sourceUid = msg.sourceUid;
            //notify.sourceName = msg.sourceName;
            //notify.sourceLevel = msg.sourceLevel;
            //dest.Write(notify);
        }

        //public void OnResponse_PVPChallenge(MemoryStream stream, int uid = 0)
        //{
        //    MSG_RZ_PVP_CHALLENGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_PVP_CHALLENGE>(stream);
        //    Log.Write("relation notify player {0} anwser player {1} pvp challenge agree {2}", msg.destUid, msg.sourceUid, msg.Agree);
        //    PlayerChar source = server.PCManager.FindPc(msg.sourceUid);
        //    if (source == null || source.IsTransforming == true || source.IsLeavingZone || source.Team != null 
        //        || source.IsInDungeon == true || source.CurrentMap == null)
        //    {
        //        return;
        //    }
        //    if (msg.Agree == true)
        //    {
        //        // 同意 创建副本
        //        int invalidMemberUid = 0;
        //        PKS_ZC_ENTER_DUNGEON.RESULT canCreateResult = source.CanCreateDungeon(GameConfig.ArenaForFunDungeonId, (int)PKS_CZ_ENTER_DUNGEON.DungeonIdType.Dungeon, out invalidMemberUid);
        //        if (canCreateResult != PKS_ZC_ENTER_DUNGEON.Types.RESULT.Success)
        //        {
        //            return;
        //        }

        //        MSG_ZM_PULL_PLAYER pullPlayer = new MSG_ZM_PULL_PLAYER();
        //        pullPlayer.subId = msg.SubId;
        //        pullPlayer.uid = msg.destUid;
        //        pullPlayer.beginPosX = GameConfig.DefenderPosX;
        //        pullPlayer.beginPosY = GameConfig.DefenderPosY;
        //        pullPlayer.teamLimit = false;

        //        if (server.StatAverageFrame > ServerShared.CONST.ACCEPT_DUNGEON_FRAME)
        //        {
        //            Log.Write("current frame is {0}, will create pvp challenge dungeon for player {1}", server.StatAverageFrame, source.Uid);
        //            // 获取副本
        //            FieldMap dungeonItem = null;
        //            dungeonItem = source.SetDungeonFieldMap(GameConfig.ArenaForFunDungeonId);
        //            if (dungeonItem == null)
        //            {
        //                Logger.Log.Warn("player {0} request enter dungeon {1} failed: no such dungeon", source.Uid, GameConfig.ArenaForFunDungeonId);
        //                return;
        //            }
        //            dungeonItem.DungeonType = "ForFun";
        //            // 通知manager 创建副本
        //            MSG_ZM_NEW_MAP notify = new MSG_ZM_NEW_MAP();
        //            notify.MapId = dungeonItem.MapID;
        //            notify.Channel = dungeonItem.Channel;
        //            notify.Type = dungeonItem.DungeonData.Type;
        //            notify.owner = source.Uid;
        //            server.ManagerServer.Write(notify);
        //            //进入副本

        //            int origin_map = source.CurrentMap.MapID;
        //            int origin_channel = source.CurrentMap.Channel;
        //            source.OnLeaveZone();
        //            source.OnEnterDungeon(dungeonItem, origin_map, origin_channel, source.Position.X, source.Position.Y);
        //            pullPlayer.mapId = dungeonItem.MapID;
        //            pullPlayer.channel = dungeonItem.Channel;
        //            server.ManagerServer.Write(pullPlayer);
        //        }
        //        else
        //        {
        //            Log.Write("current frame is {0}, ask manager to  create pvp challenge dungeon for player {1}", server.StatAverageFrame, source.Uid);
        //            // 向manager请求
        //            MSG_ZM_NEED_DUNGEON request = new MSG_ZM_NEED_DUNGEON();
        //            request.CharacterUid = source.Uid;
        //            request.dungeonId = GameConfig.ArenaForFunDungeonId;
        //            request.curMapId = source.CurrentMap.MapID;
        //            request.curChannel = source.CurrentMap.Channel;
        //            request.positionX = source.Position.X;
        //            request.positionY = source.Position.Y;
        //            request.dungeonType = PKS_CZ_ENTER_DUNGEON.DungeonIdType.Dungeon.ToString();
        //            request.fid = source.Fid;
        //            request.pullPlayerList.Add(pullPlayer);
        //            server.ManagerServer.Write(request);
        //            source.IsTransforming = true;
        //        }
        //    }
        //    else
        //    {
        //        PKS_ZC_PVP_CHALLENGE notify = new PKS_ZC_PVP_CHALLENGE();
        //        notify.destName = msg.destName;
        //        notify.agree = msg.Agree;
        //        source.Write(notify);
        //    }
        //}

        public void OnResponse_TeamMemberLevelUp(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_TEAM_MEMBER_LEVELUP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_TEAM_MEMBER_LEVELUP>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null || player.Team == null)
            {
                return;
            }
            TeamMember member = null;
            if (player.Team.MemberList.TryGetValue(msg.MemberUid, out member))
            {
                member.Level = msg.MemberLevel;
                member.Chapter = msg.MemberChapter;
                member.Research = msg.Research;
            }
        }

        public void OnResponse_NotifyTeamContinueHunting(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NOTIFY_TEAM_CONT_HUNTING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NOTIFY_TEAM_CONT_HUNTING>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }
            player.ChangeHuntingState(msg.Contine, msg.Result);
        }

        public void OnResponse_HuntingHelp(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_HUNTING_HELP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_HUNTING_HELP>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }

            player.Write(new MSG_ZGC_HUNTING_HELP() { Result = msg.Result, DungeonId = msg.DungeonId });
        }

        public void OnResponse_HuntingHelpAsk(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_HUNTING_HELP_ASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_HUNTING_HELP_ASK>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.InviteUid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_HUNTING_HELP_ASK request = new MSG_ZGC_HUNTING_HELP_ASK()
            {
                CapUid = msg.CapUid,
                CapName = msg.CapName,
                CapLevel = msg.CapLevel,
                DungeonId = msg.DungeonId,
            };
            player.Write(request);
        }

        public void OnResponse_HuntingHelpAnswerJoin(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_HUNTING_HELP_ANSWER_JOIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_HUNTING_HELP_ANSWER_JOIN>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                return;
            }

            MSG_ZGC_HUNTING_HELP_ANSWER_JOIN request = new MSG_ZGC_HUNTING_HELP_ANSWER_JOIN() { Agree = msg.Agree };
            player.Write(request);
        }
    }
}
