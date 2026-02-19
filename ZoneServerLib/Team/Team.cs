using System.Collections.Generic;
using Logger;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class TeamMember
    {
        public int Uid { get; set; }
        public string Name { get; set; }
        public int Sex { get; set; }
        public int Level { get; set; }
        public int Chapter { get; set; }
        public int Icon { get; set; }
        public int IconFrame { get; set; }
        public int Job { get; set; }
        public bool IsOnline { get; set; }//在线状态需要及时刷新
        public CampType Camp { get; set; }
        public int Fid { get; set; }
        public int HeroId { get; set; }
        public int BattlePower { get; set; }
        public bool IsRobot { get; set; }
        public bool IsAllowOffline { get; set; }
        public int GodType { get; set; }
        public int Research { get; set; }

        public ZGC_TEAM_MEMBER_INFO GenerateMemberInfo()
        {
            ZGC_TEAM_MEMBER_INFO info = new ZGC_TEAM_MEMBER_INFO()
            {
                Uid = this.Uid,
                Name = this.Name,
                Sex = this.Sex,
                Level = this.Level,
                Icon = this.Icon,
                IconFrame = this.IconFrame,
                Job = this.Job,
                Camp = (int)this.Camp,
                IsOnline = this.IsOnline,
                HeroId = this.HeroId,
                BattlePower = this.BattlePower,
                IsRobot = this.IsRobot || IsAllowOffline,
                GodType = this.GodType,
                Research = Research,
            };
            return info;
        }
    }

    public class Team
    {
        //组的金兰，当目标金兰上线，该队伍需要被解散
        public bool NeedDestroy { get; private set; }
        public PlayerChar Owner { get; set; }
        public int Type { get; set; }
        public int TeamId { get; set; }
        public int CaptainUid { get; set; }
        public bool IsInviteMirror { get; set; }
        public int OwnerUid { get { return Owner.Uid; } }

        public PlayerChar MirrorPlayer { get; set; }

        public Dictionary<int, TeamMember> MemberList = new Dictionary<int, TeamMember>();

        public int RobotCount { get; private set; }
        public int MemberCount { get { return MemberList.Count; } }

        public Dictionary<int, PlayerChar> MemberPlayerList = new Dictionary<int, PlayerChar>();
      
        public Team(int teamId, int type, int captainUid, PlayerChar owner)
        {
            Owner = owner;
            TeamId = teamId;
            Type = type;
            CaptainUid = captainUid;
        }

        public bool IsFreeTeam { get { return Type == 0; } }

        public void SetMirror(PlayerChar player)
        {
            MirrorPlayer = player;
        }

        public void AddMember(TeamMember member, bool notifyMember, bool full = false)
        {
            if (MemberList.ContainsKey(member.Uid))
            {
                Log.Warn("player {0} already in team {1}", member.Uid, TeamId);
                return;
            }

            MemberList.Add(member.Uid, member);
            if (member.IsRobot || member.IsAllowOffline)
            {
                RobotCount++;
            }

            if (notifyMember)
            {
                NotifyMemberJoin(member, full);
            }

            PlayerChar memberPlayer = Owner.server.PCManager.FindPc(member.Uid);
            if (memberPlayer != null)
            {
                AddMemberPlayer(memberPlayer);
            }
        }

        public void RemoveMember(int uid)
        {
            if (MemberList.ContainsKey(uid) && (MemberList[uid].IsRobot || MemberList[uid].IsAllowOffline))
            {
                RobotCount--;
            }
            MemberList.Remove(uid);
            
            MemberPlayerList.Remove(uid);

            NotifyMemberLeave(uid);
        }

        public MSG_ZGC_TEAM_INFO GenerateTeamInfo()
        {
            var teamInfo = new MSG_ZGC_TEAM_INFO();
            teamInfo.TeamId = TeamId;
            teamInfo.TeamType = Type;
            teamInfo.CaptainUid = CaptainUid;

            ZGC_TEAM_MEMBER_INFO memberInfo;
            foreach (var member in MemberList)
            {
                memberInfo = member.Value.GenerateMemberInfo();
                if (memberInfo == null)
                {
                    continue;
                }
                teamInfo.Members.Add(memberInfo);
            }

            return teamInfo;
        }

        public void NotifyMemberJoin(TeamMember member, bool full = false)
        {
            if (member.Uid == OwnerUid) return;
            MSG_ZGC_NEW_TEAM_MEMBER_JOIN notify = new MSG_ZGC_NEW_TEAM_MEMBER_JOIN();
            notify.Full = full;
            notify.Member = member.GenerateMemberInfo();
            Owner.Write(notify);
        }

        //通知客户端玩家离队
        public void NotifyMemberLeave(int leaveUid)
        {
            if (leaveUid == OwnerUid) return;
            MSG_ZGC_TEAM_MEMBER_LEAVE notify = new MSG_ZGC_TEAM_MEMBER_LEAVE();
            notify.Uid = leaveUid;
            Owner.Write(notify);
        }

        public void MemberOnline(int memberUid, PlayerChar owner)
        {
            TeamMember member;
            if (MemberList.TryGetValue(memberUid, out member))
            {
                member.IsOnline = true;

                if (member.IsAllowOffline)
                {
                    SetNeedDestory(true);
                }
            }

            PlayerChar memberPlayer = owner.server.PCManager.FindPc(memberUid);
            if (memberPlayer != null)
            {
                AddMemberPlayer(memberPlayer);
            }
        }

        public void MemberOffline(int memberUid, int captain_uid)
        {
            TeamMember member;
            if (MemberList.TryGetValue(memberUid, out member))
            {
                member.IsOnline = false;

                if (member.IsAllowOffline)
                {
                    SetNeedDestory(true);
                }
            }

            MemberPlayerList.Remove(memberUid);
            CaptainUid = captain_uid;
        }

        public void SetNeedDestory(bool need)
        { 
            NeedDestroy = need;
        }

        public void MemberChangeZone(int memberUid, int subId, PlayerChar owner)
        {
            if (owner.server.SubId != subId)
            {
                // 与member 新zone不等
                MemberPlayerList.Remove(memberUid);
            }
            else
            {
                PlayerChar memberPlayer = owner.server.PCManager.FindPc(memberUid);
                if (memberPlayer != null)
                {
                    AddMemberPlayer(memberPlayer);
               }
            }
        }

        private void AddMemberPlayer(PlayerChar player)
        {
            if (player == null) return;
            if (player.Uid == OwnerUid)
            {
                return;
            }
            
            if (!MemberPlayerList.ContainsKey(player.Uid))
            {
                MemberPlayerList.Add(player.Uid, player);
            }

        }     
    }

}
