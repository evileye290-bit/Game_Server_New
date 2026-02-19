using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationServerLib
{
    public class Team
    {
        //保证按照添加删除的顺序来排列
        private List<TeamMember> members = new List<TeamMember>();
        private List<Robot> robotList = new List<Robot>();//机器人
        private DateTime lastSendNeedHelpTime = DateTime.MaxValue;
        private Dictionary<int, TeamMember> memberList = new Dictionary<int, TeamMember>(); // key uid
        public Dictionary<int, TeamMember> MemberList
        { get { return memberList; } }
        public DateTime LastSendNeedHelpTime
        { get { return lastSendNeedHelpTime; } }


        public TeamManager TeamManager { get; private set; }

        public int Type { get; private set; }
        public int TeamId { get; set; }
        public int CaptainUid { get; set; }
        public bool InDungeon { get; set; }//战斗状态
        public int MemberCount { get { return memberList.Count; } }
        public bool IsFreeTeam { get { return Type == 0; } }
        public bool IsInviteMirror { get; set; }
        public DungeonModel Model { get; private set; }

        public Team(TeamManager manager, int teamId, int captainUid, int teamType)
        {
            TeamManager = manager;
            TeamId = teamId;
            CaptainUid = captainUid;
            Type = teamType;
        }

        public void SetTeamType(int type, DungeonModel model)
        {
            int oldType = this.Type;
            this.Type = type;
            this.Model = model;

            TeamManager.SetTeamType(this, oldType);
        }

        public void SetClient(int uid, Client client)
        {
            TeamMember member;
            if (this.memberList.TryGetValue(uid, out member))
            {
                member.SetClient(client);
            }
        }

        public TeamMember GetTeamMember(int uid)
        {
            TeamMember member;
            this.memberList.TryGetValue(uid, out member);
            return member;
        }

        public void SetSendNeedHelpTime(DateTime time)
        {
            this.lastSendNeedHelpTime = time;
        }

        public bool BindData()
        {
            if (!IsFreeTeam)
            {
                var model = DungeonLibrary.GetDungeon(Type);
                if (model == null)
                {
                    Log.Warn($"player {CaptainUid} create team fail : have not this type of  DungeonModel error type {Type}");
                    return false;
                }
                this.Model = model;
            }

            return true;
        }

        #region Help

        public void AddRobot(Robot robot)
        {
            memberList.Add(robot.Uid, robot);
        }

        public void ChooseRobotForCaptain()
        {
            TeamMember cap = memberList[CaptainUid];
            TeamBattleRobotInfo robotInfo = RobotLibrary.ChooseTeamRobot(cap.HeroMaxLevel);
            if (robotInfo != null)
            {
                int uid = int.MaxValue - cap.Uid;
                while (true)
                {
                    if (!memberList.ContainsKey(uid))
                    {
                        break;
                    }
                    uid++;
                }
                Robot robot = new Robot(uid, cap, robotInfo);
                robot.Research = cap.Client == null ? cap.Research : cap.Client.Research;
                AddRobotMember(robot);
            }
        }

        public void AddRobotMember(Robot member, bool notify = true)
        {
            if (memberList.ContainsKey(member.Uid))
            {
                return;
            }

            members.Add(member);
            memberList.Add(member.Uid, member);

            TeamManager.JoinTeam(member.Uid, TeamId);

            if (notify)
            {
                NotifyNewMemberJoin(member);
            }
        }

        public void RemoveRobots()
        {
            List<int> robots = new List<int>();
            memberList.ForEach(kv =>
            {
                if (kv.Value is Robot)
                {
                    robots.Add(kv.Value.Uid);
                }
            });
            if (robots.Count > 0)
            {
                RemoveMember(robots[0]);
            }


        }

        #endregion

        //队员是否达到下限
        public bool CheckEnough()
        {
            return IsFreeTeam || MemberCount >= Model.TeamMemberLimitMin;
        }

        public bool CheckFull()
        {
            if (IsFreeTeam)
            {
                return MemberCount >= TeamLibrary.TeamMemberCountLimit;
            }
            else
            {
                return MemberCount >= Model.TeamMemberLimitMax;
            }
        }

        public void AddMember(TeamMember member, bool notify = true)
        {
            if (memberList.ContainsKey(member.Uid))
            {
                return;
            }

            members.Add(member);
            memberList.Add(member.Uid, member);

            this.TeamManager.JoinTeam(member.Uid, TeamId);

            //如果有机器人则移除一个机器人
            RemoveRobots();

            if (notify)
            {
                NotifyNewMemberJoin(member);
            }
        }

        /// <summary>
        /// 移除队员并通知，同时检测队长转让
        /// </summary>
        public void RemoveMember(int uid)
        {
            TeamMember member;
            if (memberList.TryGetValue(uid, out member))
            {
                //设置玩家退出队伍
                if (member.Client != null)
                {
                    member.Client.Team = null;
                }
                this.TeamManager.LeaveTeam(uid);
                members.Remove(member);
                memberList.Remove(uid);

                NotifyMemberLeave(uid);              
            }
            bool hasPerson = false;
            foreach (var kv in memberList)
            {
                if (kv.Value.CheckOnline() && kv.Value.Client != null)
                {
                    hasPerson = true;
                }
            }

            if (MemberCount == 0 || !hasPerson)
            {
                this.TeamManager.RemoveTeam(this.TeamId);
                return;
            }

            if (uid == CaptainUid)
            {
                // 队长退出 则从剩余队员中选取新队长
                ChooseNewCaptain();
            }
        }

        public void MemberOnline(int uid)
        {
            //通知队伍其他成员
            foreach (var kc in MemberList)
            {
                if (kc.Key == uid)
                {
                    continue;
                }

                if (kc.Value.CheckOnline())
                {
                    MSG_RZ_TEAM_MEMBER_ONLINE notify = new MSG_RZ_TEAM_MEMBER_ONLINE();
                    notify.Uid = kc.Key;
                    notify.MemberUid = uid;
                    kc.Value.Client?.CurZone.Write(notify);
                }
            }
        }

        public void MemberOffline(int uid)
        {
            //通知成员下线
            int onlineMemberCount = 0;
            foreach (var kv in MemberList)
            {
                if (kv.Key == uid)
                {
                    continue;
                }

                if (kv.Value.CheckOnline())
                {
                    onlineMemberCount++;

                    // 通知成员下线 
                    MSG_RZ_TEAM_MEMBER_OFFLINE notify = new MSG_RZ_TEAM_MEMBER_OFFLINE();
                    notify.Uid = kv.Key;
                    notify.MemberUid = uid;
                    notify.CapUid = CaptainUid;
                    kv.Value.Client?.CurZone.Write(notify);
                }
            }

            bool hasPerson = false;
            foreach (var kv in memberList)
            {
                if (kv.Value.CheckOnline() && kv.Value.Client != null)
                {
                    hasPerson = true;
                }
            }

            if (MemberCount == 0 || !hasPerson || onlineMemberCount == 0)
            {
                this.TeamManager.RemoveTeam(this.TeamId);
                return;
            }

            //if (CaptainUid == uid)
            //{
            //    ChooseNewCaptain();
            //}
        }

        public void ChooseNewCaptain()
        {
            TeamMember captain = null;
            foreach (var kv in memberList)
            {
                if (kv.Value.CheckOnline() && !(kv.Value is Robot))
                {
                    captain = kv.Value;
                }
            }

            if (captain != null)
            {
                ChangeCaptain(captain.Uid);
            }
            else
            {
                //队伍中没人了，或者全部离线了 删除队伍
                this.TeamManager.RemoveTeam(this.TeamId);
            }
        }

        public void MemberChangeZone(int uid, int subId)
        {
            // 通知其他成员 该成员跨zone 重置同zone member缓存
            foreach (var kv in memberList)
            {
                if (kv.Key == uid)
                {
                    continue;
                }

                if (kv.Value.Client?.CurZone != null)
                {
                    MSG_RZ_TEAM_MEMEBR_CHANGE_ZONE notify = new MSG_RZ_TEAM_MEMEBR_CHANGE_ZONE();
                    notify.Uid = kv.Key;
                    notify.MemberUid = uid;
                    notify.SubId = subId;
                    kv.Value.Client.CurZone.Write(notify);
                }
            }
        }

        public void ChangeCaptain(int uid)
        {
            int oldUid = this.CaptainUid;
            this.CaptainUid = uid;
            NotifyCaptainChange(oldUid);
        }

        public bool IsInBrotherTeam(int uid)
        {
            return memberList.Values.Where(x => x.Uid == uid && x.IsAllowOffline).FirstOrDefault() != null;
        }


        public MSG_RZ_TEAM_INFO GenerateTeamInfo()
        {
            var teamInfo = new MSG_RZ_TEAM_INFO();
            teamInfo.TeamId = TeamId;
            teamInfo.TeamType = Type;
            teamInfo.CaptainUid = CaptainUid;
            teamInfo.InviteMirror = IsInviteMirror;

            members.ForEach(mem => teamInfo.Members.Add(mem.GenerateMemberInfo()));
            if (robotList.Count > 0)
            {
                robotList.ForEach(mem => teamInfo.Members.Add(mem.GenerateMemberInfo()));
            }

            return teamInfo;
        }

        //队长变动通知队员
        public void NotifyCaptainChange(int oldUid)
        {
            foreach (var kv in memberList)
            {
                if (kv.Value.CheckOnline())
                {
                    MSG_RZ_CAPTAIN_CHANGE notify = new MSG_RZ_CAPTAIN_CHANGE();
                    notify.Uid = kv.Key;
                    notify.NewCapUid = CaptainUid;
                    kv.Value.Client?.CurZone.Write(notify);
                }
            }
        }

        public void NotifyMemberLeave(int uid)
        {
            foreach (var kv in memberList)
            {
                if (kv.Value.Uid == uid)
                {
                    continue;
                }

                if (kv.Value.CheckOnline())
                {
                    MSG_RZ_TEAM_MEMBER_LEAVE notify = new MSG_RZ_TEAM_MEMBER_LEAVE();
                    notify.Uid = kv.Key;
                    notify.LeaveUid = uid;
                    kv.Value.Client?.CurZone.Write(notify);
                }
            }
        }

        //通知其他成员有新成员加入
        private void NotifyNewMemberJoin(TeamMember member)
        {
            //只有自己一个人不需要通知
            if (memberList.Count == 1)
            {
                return;
            }

            bool full = CheckFull();
            var msg = member.GenerateMemberInfo();

            // 应根据组队类型 是否需要同步zone 决定是否通知新队员其他成员信息
            foreach (var kv in memberList)
            {
                if (kv.Value.Uid == member.Uid)
                {
                    continue;
                }

                if (kv.Value.CheckOnline())
                {
                    MSG_RZ_NEW_TEAM_MEMBER_JOIN notify = new MSG_RZ_NEW_TEAM_MEMBER_JOIN();
                    notify.Uid = kv.Key;
                    notify.Member = msg;
                    notify.Full = full;
                    notify.IsInviteMirror = IsInviteMirror;
                    kv.Value.Client?.CurZone.Write(notify);
                }
            }
        }

        public void NotifyCreateTeamDungeon(int mapId, int channel, int mainId, int subId)
        {
            foreach (var kv in MemberList)
            {
                if (kv.Key != CaptainUid && kv.Value.CheckOnline() && kv.Value.Client != null)
                {
                    MSG_RZ_NEW_TEAM_DUNGEON notify = new MSG_RZ_NEW_TEAM_DUNGEON();
                    notify.Uid = kv.Key;
                    notify.OwnerUid = kv.Key;
                    notify.MapId = mapId;
                    notify.Channel = channel;
                    kv.Value.Client.CurZone.Write(notify);
                }
                else 
                {
                    if (kv.Value is Robot || kv.Value.IsAllowOffline)
                    {
                        Robot robot = kv.Value as Robot;

                        MSG_RZ_TRY_CREATE_ROBOT_MEMBER msg = new MSG_RZ_TRY_CREATE_ROBOT_MEMBER();
                        //机器人uid 所需ratio 所用robotid
                        msg.TeamId = TeamId;
                        msg.OwnerUid = CaptainUid;
                        msg.MapId = mapId;
                        msg.Channel = channel;
                        msg.RobotUid = kv.Key;
                        msg.RobotNatureRatio = robot == null ? 0 : robot.GetRatio();
                        msg.TeamRobotId = robot == null ? 0 : robot.GetRobotId();
                        msg.TeamLevel = kv.Value.HeroMaxLevel;
                        msg.Name = kv.Value.Name;
                        msg.Sex = kv.Value.Sex;
                        msg.GodType = kv.Value.GodType;
                        TeamMember cap = MemberList[CaptainUid];

                        FrontendServer server = cap?.Client.CurZone.ServerManager.GetServer(mainId, subId);
                        server?.Write(msg);
                    }

                    //cap.Client.CurZone.Write(msg);
                }
            }
        }

        public void NotifyMemberLevelUp(Client client)
        {
            foreach (var kv in memberList)
            {
                if (kv.Value.Uid == client.Uid) continue;

                if (kv.Value.CheckOnline())
                {
                    MSG_RZ_TEAM_MEMBER_LEVELUP notify = new MSG_RZ_TEAM_MEMBER_LEVELUP
                    {
                        Uid = kv.Key,
                        MemberLevel = client.Level,
                        MemberChapter = client.ChapterId,
                        Research = client.Research,
                    };
                    kv.Value.Client?.CurZone.Write(notify);
                }
            }
        }

        //通知队员连续狩猎
        public void NotifyMemberContinueHunting(bool isContinue)
        {
            foreach (var kv in memberList)
            {
                if (kv.Value.CheckOnline())
                {
                    MSG_RZ_NOTIFY_TEAM_CONT_HUNTING notify = new MSG_RZ_NOTIFY_TEAM_CONT_HUNTING();
                    notify.Uid = kv.Key;
                    notify.Result = (int)ErrorCode.Success;
                    notify.Contine = isContinue;
                    kv.Value.Client?.CurZone.Write(notify);
                }
            }
        }        
    }
}
