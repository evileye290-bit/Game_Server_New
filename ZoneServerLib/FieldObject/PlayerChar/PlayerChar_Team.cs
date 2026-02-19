using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar : FieldObject
    {
        //组队
        private DateTime lastTeamHelpTime = DateTime.MinValue;
        public Team Team { get; private set; }
        private List<int> willLeaveList = new List<int>(); 
        public List<int> WillLeaveList { get { return willLeaveList; } }

        public int CaptainUid { get; private set; }
        public bool IsCaptain()
        {
            return Team != null && Team.CaptainUid == uid;
        }

        public bool CheckTeamMirror(int uid)
        {
            if (Team == null || !Team.IsInviteMirror) return false;

            if (Team.MirrorPlayer == null || Team.MirrorPlayer.uid != uid)
            {
                Team.MirrorPlayer = null;
                return false;
            }

            return true;
        }

        public void ChangeTeam(MSG_RZ_TEAM_INFO teamInfo)
        {
            if (teamInfo != null)
            {
                this.LeaveTeam();
            }


            TeamMember member;
            Team temp = new Team(teamInfo.TeamId, teamInfo.TeamType, teamInfo.CaptainUid, this);
            foreach (var kv in teamInfo.Members)
            {
                member = new TeamMember()
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
                    BattlePower = kv.BattlePower,
                    Chapter = kv.Chapter,
                    IsRobot = kv.IsRobot,
                    IsAllowOffline = kv.IsAllowOffline,
                    GodType = kv.GodType,
                    Research = kv.Research
                };

                temp.IsInviteMirror = teamInfo.InviteMirror;

                //创建队伍不需要新加入的成员不需要同步
                temp.AddMember(member, false);
            }

            this.JoinTeam(temp);
        }

        public void LeaveTeam()
        {
            if (this.Team != null)
            {
                this.Team = null;
            }
        }

        private void JoinTeam(Team team)
        {
            this.Team = team;
        }

        public void CheckAndDestoryTeam()
        {
            if (Team?.NeedDestroy == true)
            {
                Team.SetNeedDestory(false);
                
                //离线好友已上线，自动解除队伍
                Write(new MSG_ZGC_QUIT_TEAM() { Result = (int)ErrorCode.OfflineBrotherOnline });
                server.SendToRelation(new MSG_ZR_QUIT_TEAM() {  Uid = uid, TeamId = Team.TeamId});
            }
        }

        public void RequestTeamTypeList(int teamType, int page)
        {
            if (teamType == 0)
            {
                return;
            }
            MSG_ZR_TEAM_TYPE_LIST request = new MSG_ZR_TEAM_TYPE_LIST();
            request.TeamType = teamType;
            request.Page = page;
            request.Uid = uid;
            server.SendToRelation(request);
        }

        // 自己创建队伍
        public void RequestCreateTeam(MSG_GateZ_CREATE_TEAM msg)
        {
            MSG_ZGC_CREATE_TEAM response = new MSG_ZGC_CREATE_TEAM();
            if (Team != null)
            {
                Log.Warn($"player {Uid} request create team {msg.TeamType} failed: already in team");
                response.Result = (int)ErrorCode.InTeam;
                Write(response);
                return;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(msg.TeamType);
            if (model == null || model.TeamLimit)
            {
                Log.Warn($"player {Uid} request create team {msg.TeamType} failed: map team limit");
                response.Result = (int)ErrorCode.MapTeamLimit;
                Write(response);
                return;
            }

            if (Level < model.MinLevel)
            {
                Log.Warn($"player {Uid} request create team {msg.TeamType} failed: level {Level} limit");
                response.Result = (int)ErrorCode.LevelLimit;
                Write(response);
                return;
            }

            // 检查通过 向Relation请求
            MSG_ZR_CREATE_TEAM request = new MSG_ZR_CREATE_TEAM();
            request.Uid = uid;
            request.TeamType = msg.TeamType;
            request.Teamlevel = GetMaxEquipHeroLevel();
            server.SendToRelation(request,uid);
        }

        // 加入队伍
        public void RequestJoinTeam(int teamId)
        {
            if (NotStableInMap() || currentMap.IsDungeon)
            {
                Log.Warn($"player {Uid} request join team {teamId} failed: in dungeon");
                MSG_ZGC_JOIN_TEAM response = new MSG_ZGC_JOIN_TEAM();
                response.Result = (int)ErrorCode.InDungeon;
                Write(response);
                return;
            }

            MSG_ZR_JOIN_TEAM request = new MSG_ZR_JOIN_TEAM();
            request.Uid = uid;
            request.TeamId = teamId;
            server.SendToRelation(request);
        }

        // 退出队伍
        public void RequestQuitTeam()
        {
            if (Team == null)
            {
                Log.Warn("player {0} RequestQuitTeam not find team", uid);
                MSG_ZGC_QUIT_TEAM response = new MSG_ZGC_QUIT_TEAM();
                response.Result = (int)ErrorCode.Success;
                Write(response);
                return;
            }

            if (NotStableInMap() || currentMap.IsDungeon)
            {
                Log.Warn("player {0} RequestQuitTeam failed: in dungeon", uid);
                MSG_ZGC_QUIT_TEAM response = new MSG_ZGC_QUIT_TEAM();
                response.Result = (int)ErrorCode.InDungeon;
                Write(response);
                return;
            }

            MSG_ZR_QUIT_TEAM request = new MSG_ZR_QUIT_TEAM();
            request.Uid = uid;
            request.TeamId = Team.TeamId;
            server.SendToRelation(request);
        }

        // 踢出队员
        public void RequestKickTeam(int kickUid)
        {
            if (Team == null)
            {
                //MSG_ZGC_KICK_TEAM_MEMBER response = new MSG_ZGC_KICK_TEAM_MEMBER();
                //response.Result = (int)ErrorCode.NotInTeam;
                //Write(response);
                Log.Warn("player {0} RequestKickTeam not find team", uid);
                MSG_ZGC_QUIT_TEAM response = new MSG_ZGC_QUIT_TEAM();
                response.Result = (int)ErrorCode.Success;
                Write(response);
                return;
            }
            if (Team.CaptainUid != uid || kickUid == uid)
            {
                Log.Warn("player {0} request kick team member {1} failed: not team captain", uid, kickUid);
                MSG_ZGC_KICK_TEAM_MEMBER response = new MSG_ZGC_KICK_TEAM_MEMBER();
                response.Result = (int)ErrorCode.NotTeamCaptain;
                Write(response);
                return;
            }

            if (!Team.MemberList.ContainsKey(kickUid))
            {
                Log.Warn("player {0} request kick team member {1} failed: kick member not in team", uid, kickUid);
                MSG_ZGC_KICK_TEAM_MEMBER response = new MSG_ZGC_KICK_TEAM_MEMBER();
                response.Result = (int)ErrorCode.KickMemberNotInTeam;
                Write(response);
                return;
            }

            MSG_ZR_KICK_TEAM_MEMBER request = new MSG_ZR_KICK_TEAM_MEMBER();
            request.Uid = uid;
            request.TeamId = Team.TeamId;
            request.KickUid = kickUid;
            server.SendToRelation(request);
        }

        public void RequestTransferCaptain(int newCapUid)
        {
            ErrorCode errorCode = CheckTransferCaptain(newCapUid);
            if (errorCode != ErrorCode.Success)
            {
                Log.Warn("player {0} request transfer captain to {1} failed: errorCode {2}", uid, newCapUid, (int)errorCode);
                MSG_ZGC_TRANSFER_CAPTAIN response = new MSG_ZGC_TRANSFER_CAPTAIN();
                response.Result = (int)errorCode;
                Write(response);
                return;
            }

            // 检查通过
            MSG_ZR_TRANDSFER_CAPTAIN request = new MSG_ZR_TRANDSFER_CAPTAIN();
            request.Uid = Uid;
            request.MemberUid = newCapUid;
            server.SendToRelation(request);
        }

        private ErrorCode CheckTransferCaptain(int newCapUid)
        {
            if (Uid == newCapUid)
            {
                return ErrorCode.Fail;
            }
            if (Team == null)
            {
                return ErrorCode.NotInTeam;
            }
            if (Team.CaptainUid != Uid)
            {
                return ErrorCode.NotTeamCaptain;
            }
            if (InDungeon)
            {
                return ErrorCode.TeamInDungeonChangeCap;
            }
            if (!Team.MemberList.ContainsKey(newCapUid))
            {
                return ErrorCode.MemberNotInTeam;
            }
            return ErrorCode.Success;
        }

        public void RequestAskJoinTeam(int askUid)
        {
            PlayerChar beenAskedPlayer = null;
            ErrorCode errorCode = CheckAskJoinTeam(askUid, out beenAskedPlayer);
            if (errorCode != ErrorCode.Success)
            {
                Log.Warn("player {0} request ask {1} join team failed: errorCode {2}", uid, askUid, (int)errorCode);
                MSG_ZGC_ASK_JOIN_TEAM response = new MSG_ZGC_ASK_JOIN_TEAM();
                response.Result = (int)errorCode;
                Write(response);
                return;
            }

            if (beenAskedPlayer == null)
            {
                OperateCheckBlackList operationGetBlackList = new OperateCheckBlackList(askUid, uid);
                server.GameRedis.Call(operationGetBlackList, ret =>
                {
                    if ((int)ret == 1)
                    {
                        if (operationGetBlackList.Exist)
                        {
                            Log.Warn("player {0} request ask {1} join team failed: in target black", uid, askUid);
                            MSG_ZGC_ASK_JOIN_TEAM response = new MSG_ZGC_ASK_JOIN_TEAM();
                            response.Result = (int)ErrorCode.InTargetBlack;
                            Write(response);
                            return;
                        }
                        else
                        {
                            SendAskJoinTeamRequest2Relation(askUid);
                        }
                    }
                });
            }
            else
            {
                SendAskJoinTeamRequest2Relation(askUid);
            }
        }

        private ErrorCode CheckAskJoinTeam(int askUid, out PlayerChar targetPlayer)
        {
            targetPlayer = server.PCManager.FindPc(askUid);
            if (NotStableInMap() || currentMap.IsDungeon)
            {
                return ErrorCode.InDungeon;
            }
            if (targetPlayer != null)
            {
                //被申请的玩家不再队伍中
                if (targetPlayer.Team == null)
                {
                    return ErrorCode.NotInTeam;
                }

                if (targetPlayer.NotStableInMap() || targetPlayer.CurrentMap.IsDungeon)
                {
                    return ErrorCode.InDungeon;
                }

                if (targetPlayer.CheckBlackExist(uid))
                {
                    return ErrorCode.InTargetBlack;
                }
            }
            return ErrorCode.Success;
        }

        private void SendAskJoinTeamRequest2Relation(int askUid)
        {
            MSG_ZR_ASK_JOIN_TEAM request = new MSG_ZR_ASK_JOIN_TEAM();
            request.Uid = uid;
            request.AskUid = askUid;
            server.SendToRelation(request);
        }

        // 邀请加入队伍
        public void InviteJoinTeam(int memberUid, bool inviteMirror)
        {
            PlayerChar targetPlayer;
            ErrorCode errorCode = CheckInviteJoinTeam(memberUid, inviteMirror, out targetPlayer);
            if (errorCode != ErrorCode.Success)
            {
                Log.Warn("player {0} invite {1} join team failed: errorCode {2}", uid, memberUid, (int)errorCode);
                MSG_ZGC_INVITE_JOIN_TEAM response = new MSG_ZGC_INVITE_JOIN_TEAM();
                response.Result = (int)errorCode;
                Write(response);
                return;
            }

            if (targetPlayer == null)
            {
                OperateCheckBlackList operationCheckInBlack = new OperateCheckBlackList(memberUid, uid);
                server.GameRedis.Call(operationCheckInBlack, ret =>
                {
                    if ((int)ret == 1)
                    {
                        if (operationCheckInBlack.Exist)
                        {
                            Log.Warn("player {0} invite {1} join team failed: in target black", uid, memberUid);
                            MSG_ZGC_INVITE_JOIN_TEAM response = new MSG_ZGC_INVITE_JOIN_TEAM();
                            response.Result = (int)ErrorCode.InTargetBlack;
                            Write(response);
                            return;
                        }
                        else
                        {
                            SendInviteJoinTeamRequest2Relation(memberUid, inviteMirror);
                        }
                    }
                });
            }
            else
            {
                SendInviteJoinTeamRequest2Relation(memberUid, inviteMirror);
            }
        }

        private void SendInviteJoinTeamRequest2Relation(int inviteUid, bool inviteMirror, bool isBrother =false)
        {
            server.SendToRelation(new MSG_ZR_INVITE_JOIN_TEAM() {CapUid = uid, InviteUid = inviteUid, InviteMirror = inviteMirror});
        }

        private ErrorCode CheckInviteJoinTeam(int memberId, bool inviteMirror, out PlayerChar tergetPlayer)
        {
            tergetPlayer = null;
            if (NotStableInMap() || currentMap.IsDungeon)
            {
                return ErrorCode.InDungeon;
            }

            if (Team != null)
            {
                if (!Team.IsFreeTeam)
                {
                    DungeonModel model = DungeonLibrary.GetDungeon(Team.Type);
                    if (model == null || model.TeamLimit)
                    {
                        return ErrorCode.MapTeamLimit;
                    }

                    if (Level < model.MinLevel)
                    {
                        return ErrorCode.LevelLimit;
                    }
                }

                if (Team.CaptainUid != Uid)
                {
                    return ErrorCode.NotTeamCaptain;
                }

                if (Team.MemberCount >= TeamLibrary.TeamMemberCountLimit)
                {
                    return ErrorCode.TeamFull;
                }
            }

            if (!inviteMirror)
            {
                tergetPlayer = server.PCManager.FindPc(memberId);
                if (tergetPlayer != null)
                {
                    if (tergetPlayer.NotStableInMap() || tergetPlayer.CurrentMap.IsDungeon)
                    {
                        return ErrorCode.InDungeon;
                    }

                    if (tergetPlayer.CheckBlackExist(uid))
                    {
                        return ErrorCode.InTargetBlack;
                    }
                }
            }

          
            return ErrorCode.Success;
        }

        public void FlowCaptain()
        {
            ErrorCode code = CheckTeamFlowCaptain();
            if (code != ErrorCode.Success)
            {
                Log.Warn("player {0} follow captain failed: errorCode {1}", uid, (int)code);
                MSG_ZGC_FOLLOW_CAPTAIN notify = new MSG_ZGC_FOLLOW_CAPTAIN();
                notify.Result = (int)code;
                Write(notify);
                return;
            }

            PlayerChar captain = server.PCManager.FindPc(Team.CaptainUid);
            if (captain == null)
            {
                // captain不在当前zone 通过Relation转发请求
                MSG_ZR_ASK_FOLLOW_CAPTAIN request = new MSG_ZR_ASK_FOLLOW_CAPTAIN();
                request.MemberUid = Uid;
                request.CapUid = Team.CaptainUid;
                server.SendToRelation(request);
                return;
            }

            // 与captain在同一个zone
            if (captain.NotStableInMap())
            {
                Log.Warn("player {0} request follow captain {1} failed: captain in limit condition", Uid, Team.CaptainUid);
                return;
            }

            MSG_ZGC_FOLLOW_CAPTAIN response = new MSG_ZGC_FOLLOW_CAPTAIN();
            response.MapId = captain.CurrentMap.MapId;
            response.Channel = captain.CurrentMap.Channel;
            response.Result = (int)ErrorCode.Success;

            if (captain.InDungeon)
            {
                Log.Warn("player {0} follow captain failed: captain in dungeon", uid);
                response.Result = (int)ErrorCode.CapInDungeon;
                Write(response);
                return;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(Team.Type);
            if (model != null)
            {
                if (model.TeamLimit)
                {
                    Log.Warn("player {0} follow captain failed: captain map team limit", uid);
                    response.Result = (int)ErrorCode.CapMapTeamLimit;
                    Write(response);
                    return;
                }

                if (Level < model.MinLevel)
                {
                    Log.Warn("player {0} follow captain failed: captain dungeon level limit", uid);
                    response.Result = (int)ErrorCode.CapDungeonLevelLimit;
                    Write(response);
                    return;
                }
            }

            //相同地图中 走到队长身边
            if (captain.CurrentMap.MapId == CurrentMap.MapId && captain.CurrentMap.Channel == CurrentMap.Channel)
            {
                response.NeedFly = false;
                Write(response);

                SetDestination(captain.Position);
                FsmManager.SetNextFsmStateType(FsmStateType.RUN);
            }
            else
            {
                //不同地图

                response.NeedFly = true;
                Write(response);

                RecordEnterMapInfo(captain.currentMap.MapId, captain.currentMap.Channel, captain.Position);
                EnterMapInfo.SetNeedAnim();
                RecordOriginMapInfo();
                OnMoveMap();
            }
        }

        public void TryFlowCaptain()
        {
            ErrorCode code = CheckTeamFlowCaptain();
            if (code != ErrorCode.Success)
            {
                Log.Warn("player {0} try follow captain failed: errorCode {1}", uid, (int)code);
                MSG_ZGC_TRY_FOLLOW_CAPTAIN notify = new MSG_ZGC_TRY_FOLLOW_CAPTAIN();
                notify.Result = (int)code;
                Write(notify);
                return;
            }

            PlayerChar captain = server.PCManager.FindPc(Team.CaptainUid);
            if (captain == null)
            {
                // captain不在当前zone 通过Relation转发请求
                MSG_ZR_TRY_ASK_FOLLOW_CAPTAIN request = new MSG_ZR_TRY_ASK_FOLLOW_CAPTAIN();
                request.MemberUid = Uid;
                request.CapUid = Team.CaptainUid;
                server.SendToRelation(request);
                return;
            }

            // 与captain在同一个zone
            if (captain.NotStableInMap())
            {
                Log.Warn("player {0} request follow captain {1} failed: captain in limit condition", Uid, Team.CaptainUid);
                return;
            }

            MSG_ZGC_TRY_FOLLOW_CAPTAIN response = new MSG_ZGC_TRY_FOLLOW_CAPTAIN();
            response.MapId = captain.CurrentMap.MapId;
            response.Channel = captain.CurrentMap.Channel;

            if (captain.InDungeon)
            {
                Log.Warn("player {0} try follow captain failed: captain in dungeon", uid);
                response.Result = (int)ErrorCode.CapInDungeon;
                Write(response);
                return;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(Team.Type);
            if (model != null)
            {
                if (model.TeamLimit)
                {
                    Log.Warn("player {0} try follow captain failed: captain map team limit", uid);
                    response.Result = (int)ErrorCode.CapMapTeamLimit;
                    Write(response);
                    return;
                }

                if (Level < model.MinLevel)
                {
                    Log.Warn("player {0} try follow captain failed: captain dungeon level limit", uid);
                    response.Result = (int)ErrorCode.CapDungeonLevelLimit;
                    Write(response);
                    return;
                }
            }

            //相同地图中 走到队长身边
            if (captain.CurrentMap.MapId == CurrentMap.MapId && captain.CurrentMap.Channel == CurrentMap.Channel)
            {
                response.Result = (int)ErrorCode.Success;
                response.NeedFly = false;
                Write(response);
            }
            else
            {
                response.Result = (int)ErrorCode.Success;
                response.NeedFly = true;
                Write(response);
            }
        }

        public void RequestChangeTeamType(int teamType)
        {
            ErrorCode code = CheckTeamAuth();
            if (code != ErrorCode.Success)
            {
                Log.Warn("player {0} request change teamType to {1} failed: errorCode {2}", uid, teamType, (int)code);
                MSG_ZGC_CHANGE_TEAM_TYPE response = new MSG_ZGC_CHANGE_TEAM_TYPE();
                response.TeamType = teamType;
                response.Result = (int)code;
                Write(response);
                return;
            }

            if (Team.Type == teamType)
            {
                Log.Warn("player {0} request change teamType to {1} failed: same type", uid, teamType);
                MSG_ZGC_CHANGE_TEAM_TYPE response = new MSG_ZGC_CHANGE_TEAM_TYPE();
                response.TeamType = teamType;
                response.Result = (int)ErrorCode.SameTeamType;
                Write(response);
                return;
            }

            MSG_ZR_CHANGE_TEAM_TYPE msg = new MSG_ZR_CHANGE_TEAM_TYPE();
            msg.CapUid = uid;
            msg.TeamType = teamType;
            server.SendToRelation(msg);
        }

        public void RequestReliveTeamMember(int memberUid)
        {
            MSG_ZGC_TEAM_RELIVE_TEAMMEMBER response = new MSG_ZGC_TEAM_RELIVE_TEAMMEMBER();
            response.ReliveUid = memberUid;

            if (memberUid == uid)
            {
                Log.Warn("player {0} request relive team member {1} failed: can not relive self", uid, memberUid);
                response.Result = (int)ErrorCode.CanNotReviveSelf;
                Write(response);
                return;
            }

            if (Team == null)
            {
                Log.Warn("player {0} request relive team member {1} failed: player not in team", uid, memberUid);
                response.Result = (int)ErrorCode.NoTeam;
                Write(response);
                return;
            }

            if (!Team.MemberList.ContainsKey(memberUid))
            {
                Log.Warn("player {0} request relive team member {1} failed: member not in team", uid, memberUid);
                response.Result = (int)ErrorCode.MemberNotInTeam;
                Write(response);
                return;
            }

            if (CurTeamDungeon == null)
            {
                Log.Warn("player {0} request relive team member {1} failed:not in dungeon", uid, memberUid);
                response.Result = (int)ErrorCode.NotInDungeon;
                Write(response);
                return;
            }

            if (CurTeamDungeon.EnergyCanCount <= 0)
            {
                Log.Warn("player {0} request relive team member {1} failed:relive drug not enough", uid, memberUid);
                response.Result = (int)ErrorCode.ReliveDrugNotEnough;
                Write(response);
                return;
            }

            //if ((CurTeamDungeon.LastReviveTime - BaseApi.now).TotalSeconds < TeamLibrary.ReviveTime)
            //{
            //    response.Result = (int)ErrorCode.ReliveTimeCD;
            //    Write(response);
            //    return;
            //}

            PlayerChar relivePlayer = server.PCManager.FindPc(memberUid);
            if (relivePlayer == null)
            {
                Log.Warn("player {0} request relive team member {1} failed: target offline", uid, memberUid);
                response.Result = (int)ErrorCode.TargetOffline;
                Write(response);
                return;
            }

            if (!relivePlayer.IsDead)
            {
                Log.Warn("player {0} request relive team member {1} failed: target not dead", uid, memberUid);
                response.Result = (int)ErrorCode.RelivePlayerNotDead;
                Write(response);
                return;
            }

            ReliveTeamMember(relivePlayer.InstanceId);
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void RequestReliveHero(int instanceId)
        {
            MSG_ZGC_RELIVE_HERO response = new MSG_ZGC_RELIVE_HERO();
            response.InstanceId = instanceId;

            if (CurDungeon == null)
            {
                Log.Warn($"player {Uid} relive hero failed, CurDungeon is null");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //if (CurDungeon.Model.IsTeamDungeon())
            //{
            //    Log.Write($"player {Uid} relive hero failed, CurDungeon is team dungeon");
            //    response.Result = (int)ErrorCode.Fail;
            //    Write(response);
            //    return;
            //}

            if (CurDungeon.EnergyCanCount <= 0)
            {
                Log.Warn($"player {Uid} relive hero failed, relive drug not enough");
                response.Result = (int)ErrorCode.ReliveDrugNotEnough;
                Write(response);
                return;
            }

            Hero hero = CurDungeon.GetHero(instanceId);
            if (hero == null)
            {
                Log.Warn($"player {Uid} relive hero, not find hero instance Id {instanceId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!hero.IsDead)
            {
                Log.Warn($"player {Uid} relive hero failed: hero instance Id {instanceId} not dead");
                response.Result = (int)ErrorCode.ReviveHeroNotDead;
                Write(response);
                return;
            }

            //if ((CurDungeon.LastReviveTime - BaseApi.now).TotalSeconds < TeamLibrary.GetReviveCD(CurDungeon.ReviveCount))
            //{
            //    response.Result = (int)ErrorCode.ReliveTimeCD;
            //    Write(response);
            //    return;
            //}

            response.Result = (int)ErrorCode.Success;
            Write(response);
            CurDungeon.ReviveHero(hero);
        }

        public void RequestTeamHelp(int friendUid)
        {
            ErrorCode code = CheckTeamAuth();
            if (code != ErrorCode.Success)
            {
                Log.Warn("player {0} request {1} team help failed: errorCode {2}", uid, friendUid, (int)code);
                MSG_ZGC_NEED_TEAM_HELP response = new MSG_ZGC_NEED_TEAM_HELP();
                response.Result = (int)code;
                Write(response);
                return;
            }

            if (friendUid <= 0)
            {
                if ((BaseApi.now - lastTeamHelpTime).TotalSeconds < TeamLibrary.HelpCDTime)
                {
                    Log.Warn("player {0} request {1} team help failed: help in CD", uid, friendUid);
                    MSG_ZGC_NEED_TEAM_HELP response = new MSG_ZGC_NEED_TEAM_HELP();
                    response.Result = (int)ErrorCode.SendTeamHelpTooQuick;
                    Write(response);
                    return;
                }
            }

            lastTeamHelpTime = BaseApi.now;

            MSG_ZR_NEED_TEAM_HELP msg = new MSG_ZR_NEED_TEAM_HELP()
            {
                Uid = uid,
                Name = Name,
                Level = Level,
                Camp = (int)Camp,
            };
            msg.FriendUid = friendUid;
            msg.Friends.AddRange(friendList.Keys);
            server.SendToRelation(msg);
        }

        public void ResponseTeamHelp(bool result, int teamId)
        {
            if (!result)
            {
                return;
            }

            if (NotStableInMap() || currentMap.IsDungeon)
            {
                Log.Warn("player {0} response team {1} help failed: in dungeon", uid, teamId);
                MSG_ZGC_RESPONSE_TEAM_HELP response = new MSG_ZGC_RESPONSE_TEAM_HELP()
                {
                    DungeonId = teamId,
                    Result = (int)ErrorCode.InDungeon
                };
                Write(response);
                return;
            }

            MSG_ZR_RESPONSE_TEAM_HELP request = new MSG_ZR_RESPONSE_TEAM_HELP();
            request.Uid = uid;
            request.TeamId = teamId;
            server.SendToRelation(request);
        }

        public void InviteFriendJoinTeam(int playerUid)
        {
            if (Team == null)
            {
                Log.Warn("player {0} invite friend {1} join team failed: player not in team", uid, playerUid);
                MSG_ZGC_INVITE_FRIEND_JOIN_TEAM msg = new MSG_ZGC_INVITE_FRIEND_JOIN_TEAM();
                msg.Result = (int)ErrorCode.NotInTeam;
                Write(msg);
                return;
            }

            PlayerChar friend = server.PCManager.FindPc(playerUid);
            if (friend != null)
            {
                if (friend.NotStableInMap())
                {
                    Log.Warn("player {0} invite friend {1} join team failed: unknown", uid, playerUid);
                    MSG_ZGC_INVITE_FRIEND_JOIN_TEAM msg = new MSG_ZGC_INVITE_FRIEND_JOIN_TEAM();
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }

                if (friend.InDungeon)
                {
                    Log.Warn("player {0} invite friend {1} join team failed: friend in dungeon", uid, playerUid);
                    MSG_ZGC_INVITE_FRIEND_JOIN_TEAM msg = new MSG_ZGC_INVITE_FRIEND_JOIN_TEAM();
                    msg.Result = (int)ErrorCode.InDungeon;
                    Write(msg);
                    return;
                }

                if (friend.CheckBlackExist(playerUid))
                {
                    Log.Warn("player {0} invite friend {1} join team failed: in friend black", uid, playerUid);
                    MSG_ZGC_INVITE_FRIEND_JOIN_TEAM msg = new MSG_ZGC_INVITE_FRIEND_JOIN_TEAM();
                    msg.Result = (int)ErrorCode.InBlack;
                    Write(msg);
                    return;
                }

                MSG_ZGC_REQUEST_TEAM_HELP notify = new MSG_ZGC_REQUEST_TEAM_HELP()
                {
                    Name = Name,
                    Level = Level,
                    TeamId = Team.TeamId,
                    TeamType = Team.TeamId,
                    Uid = uid,
                };
                friend.Write(notify);
            }
            else
            {
                var notify = new MSG_ZR_INVITE_FRIEND_JOIN_TEAM();
                notify.Friend = playerUid;
                server.SendToRelation(notify, Uid);
            }
        }

        private ErrorCode CheckTeamAuth()
        {
            if (Team == null)
            {
                return ErrorCode.NotInTeam;
            }
            if (Team.CaptainUid != uid)
            {
                return ErrorCode.NotTeamCaptain;
            }
            return ErrorCode.Success;
        }

        private ErrorCode CheckTeamFlowCaptain()
        {
            if (NotStableInMap() || currentMap.IsDungeon)
            {
                return ErrorCode.InDungeon;
            }
            if (Team == null)
            {
                return ErrorCode.NotInTeam;
            }
            if (uid == Team.CaptainUid)
            {
                return ErrorCode.CanNotFlowSelf;
            }
            return ErrorCode.Success;
        }

        //副本里退队
        public void RequestQuitTeamInDungeon()
        {
            MSG_ZGC_QUIT_TEAM_INDUNGEON response = new MSG_ZGC_QUIT_TEAM_INDUNGEON();
            if (Team == null)
            {
                Log.Warn("player {0} RequestQuitTeamInDungeon not find team", uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (NotStableInMap())
            {
                Log.Warn("player {0} RequestQuitTeamInDungeon failed : unknown", uid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //记录将要退队状态
            RecordMemberWillLeave(Uid);

            ContinueHunting(false);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }
        
        private void RecordMemberWillLeave(int leaveUid)
        {
            Add2WillLeaveList(leaveUid);
            foreach (var member in Team.MemberPlayerList)
            {
                member.Value.Add2WillLeaveList(leaveUid);
            }
        }

        public void MemberRealQuitTeam()
        {          
            if (Team == null || WillLeaveList.Count == 0)
            {
                return;
            }
            if (WillLeaveList.Contains(Uid))
            {
                RequestQuitTeam();
            }
            //连续狩猎终止通知前端
            //if (HuntingManager.ContinueHunting)
            //{
            //    ChangeHuntingState(false);
            //}
        }

        public void NotifyTeamMemberLeaveMap()
        {
            MSG_ZGC_MEMBER_LEAVE_MAP notify = new MSG_ZGC_MEMBER_LEAVE_MAP();
            if (WillLeaveList.Count > 0)
            {
                notify.Continue = false;
            }
            else
            {
                notify.Continue = true;
            }
            Write(notify);
        }

        public void NotifyMemberLeaveMap()
        {
            MSG_ZGC_NOTIFY_CAPTAIN_MEMBERLEAVE notify = new MSG_ZGC_NOTIFY_CAPTAIN_MEMBERLEAVE();
            Write(notify);
        }

        public void RecordCaptinUid(int captainUid)
        {
            CaptainUid = captainUid;
        }    

        public void RemoveLeaveMember(int leaveUid)
        {
            if (WillLeaveList.Contains(leaveUid))
            {
                willLeaveList.Remove(leaveUid);
            }
        }

        private void Add2WillLeaveList(int leaveUid)
        {
            willLeaveList.Add(leaveUid);
        }

        public void KomoeLogRecordTeamFlow(int operateType, MSG_RZ_TEAM_INFO teamInfo)
        {
            List<Dictionary<string, object>> teamDetail = new List<Dictionary<string, object>>();
            Dictionary<string, object> unitDetail;
            foreach (var member in teamInfo.Members)
            {
                unitDetail = new Dictionary<string, object>();
                unitDetail.Add("uid", member.Uid);
                unitDetail.Add("level", member.Level);
                teamDetail.Add(unitDetail);
            }
            KomoeEventLogTeamFlow(operateType, teamInfo.TeamId.ToString(), teamDetail);
        }

        public void KomoeLogRecordTeamFlow(int operateType)
        {
            List<Dictionary<string, object>> teamDetail = new List<Dictionary<string, object>>();
            Dictionary<string, object> unitDetail;
            foreach (var kv in Team.MemberList)
            {
                unitDetail = new Dictionary<string, object>();
                unitDetail.Add("uid", kv.Value.Uid);
                unitDetail.Add("level", kv.Value.Level);
                teamDetail.Add(unitDetail);
            }
            KomoeEventLogTeamFlow(operateType, Team.TeamId.ToString(), teamDetail);
        }

        public List<Dictionary<string, object>> GetTeamDetail()
        {
            if (Team == null)
            {
                return null;
            }
            List<Dictionary<string, object>> teamDetail = new List<Dictionary<string, object>>();
            Dictionary<string, object> unitDetail;           
            foreach (var kv in Team.MemberList)
            {
                unitDetail = new Dictionary<string, object>();
                unitDetail.Add("uid", kv.Value.Uid);
                unitDetail.Add("level", kv.Value.Level);
                teamDetail.Add(unitDetail);
            }
            return teamDetail;
        }
    }
}
