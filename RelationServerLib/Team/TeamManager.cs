using CommonUtility;
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
    public class TeamManager
    {
        private int teamIndex = 0;
        private Queue<int> pooledTeamId = new Queue<int>();

        private DateTime lastCheckTime = DateTime.MinValue;
        private ZoneServerManager server;

        //玩家对应的队伍信息
        private Dictionary<int, int> uid2TeamId = new Dictionary<int, int>();

        // key teamId, value Team
        private Dictionary<int, Team> teamList = new Dictionary<int, Team>();

        // key teamType, value <key teamId>
        private Dictionary<int, List<int>> teamTypeList = new Dictionary<int, List<int>>();

        private List<int> waittingAddRobortTeam = new List<int>();//等待添加机器人
        private Dictionary<int, TeamHelpInfo> teamHelpList = new Dictionary<int, TeamHelpInfo>();//等待二次发送

        // <uid,List<teampId>>
        private ListMap<int, int> brotherTeam = new ListMap<int, int>();

        public TeamManager(ZoneServerManager server)
        {
            this.server = server;
        }

        public void Update()
        {
            if ((BaseApi.now - lastCheckTime).TotalSeconds < 2)
            {
                return;
            }

            CheckNeedTeamHelp();
            CheckNeedAddRobort();
            lastCheckTime = BaseApi.now;
        }

        public Team CreateTeam(TeamMember captain, int teamType)
        {
            Team newTeam = new Team(this, GetTeamId(), captain.Uid, teamType);
            if (!newTeam.BindData())
            {
                return null;
            }
            newTeam.AddMember(captain, false);
            teamList.Add(newTeam.TeamId, newTeam);

            if (teamType > 0)
            {
                AddToTypeList(newTeam);
            }
            return newTeam;
        }

        private int GetTeamId()
        {
            if (pooledTeamId.Count > 0) return pooledTeamId.Dequeue();
            if (teamIndex >= int.MaxValue) teamIndex = 0;
            return ++teamIndex;
        }

        public Team GetTeam(int team_id)
        {
            Team team = null;
            teamList.TryGetValue(team_id, out team);
            return team;
        }

        public void RemoveTeam(int teamId)
        {
            Team team;
            if (!teamList.TryGetValue(teamId, out team))
            {
                return;
            }

            teamList.Remove(team.TeamId);
            RemoveFromTypeList(team.Type, team.TeamId);
            NotifyRemoveTeam(team);

            if (pooledTeamId.Count < 100)
            {
                pooledTeamId.Enqueue(teamId);
            }
        }

        public Team GetPcJoinedTeam(int uid, bool online = false)
        {
            int teamId = 0;
            if (uid2TeamId.TryGetValue(uid, out teamId))
            {
                Team team = null;
                if (teamList.TryGetValue(teamId, out team))
                {
                    if (team.IsInviteMirror && online) return null;

                    return team;
                }
            }

            return null;
        }

        public void JoinTeam(int uid, int teamId)
        {
            this.uid2TeamId[uid] = teamId;
        }

        public void LeaveTeam(int uid)
        {
            this.uid2TeamId.Remove(uid);
        }

        public void SetTeamType(Team team, int oldTeamType)
        {
            RemoveFromTypeList(oldTeamType, team.TeamId);
            AddToTypeList(team);
            NotifyTeamTypeChange(team);
        }

        public void GetTeamTypeList(int teamType, int page, MSG_RZ_TEAM_TYPE_LIST response)
        {
#if DEBUG
            //if (!teamTypeList.ContainsKey(teamType) || teamTypeList[teamType].Count < 100)
            //{
            //    BuildFalseData(teamType);
            //}
#endif

            List<int> list;
            if (!teamTypeList.TryGetValue(teamType, out list))
            {
                return;
            }

            if (list.Count == 0)
            {
                return;
            }

            int lowIndex = Math.Max(0, page * 30);
            int highIndex = Math.Min(list.Count, (page + 1) * 30);

            Team team;
            for (int i = lowIndex; i < highIndex; ++i)
            {
                team = this.GetTeam(list[i]);

                //进入战斗了
                if (team == null || team.InDungeon)
                {
                    continue;
                }

                MSG_RZ_TEAM_INFO teamInfo = team.GenerateTeamInfo();
                response.Teams.Add(teamInfo);
            }
        }

        public void NeedTamHelp(TeamHelpInfo info, bool repeate = false,int repeatedTimes=1, Client client = null)
        {
            Team team = GetTeam(info.TeamId);
            if (team == null) return;

            Client friend = null;
            List<Client> helpList = new List<Client>();
            int minLevel = repeate ? info.HelpSenderInfo.Level - TeamLibrary.RepeatLevelRange : info.HelpSenderInfo.Level - TeamLibrary.HelpLevelRange;
            int maxLevel = repeate ? info.HelpSenderInfo.Level + TeamLibrary.RepeatLevelRange : info.HelpSenderInfo.Level + TeamLibrary.HelpLevelRange;

            LimitData limitData = LimitLibrary.GetLimitData(LimitType.HuntingTeam);
            if (limitData != null)
            {
                minLevel = Math.Max(minLevel, limitData.Level);
            }

            Client cap = server.GetClient(team.CaptainUid);
            int minResearch = 0;
            if (TeamLibrary.ResearchRanges.Count >= repeatedTimes)
            {
                minResearch = cap.Research - (int)TeamLibrary.ResearchRanges[repeatedTimes-1];
            }
            else
            {
                minResearch = cap.Research - (int)TeamLibrary.ResearchRanges.Last();
            }
            if (minResearch < 1)
            {
                minResearch = 1;
            }

            DungeonModel model = DungeonLibrary.GetDungeon(team.Type);
            if (model != null && minLevel < model.MinLevel)
            {
                minLevel = model.MinLevel;
            }

            //征兆单个队友
            if (info.HelpSenderInfo.FriendUid > 0)
            {
                friend = this.server.GetClient(info.HelpSenderInfo.FriendUid);
                if (friend == null)
                {
                    MSG_RZ_INVITE_JOIN_TEAM response = new MSG_RZ_INVITE_JOIN_TEAM()
                    {
                        CapUid = info.HelpSenderInfo.Uid,
                        InviteUid = info.HelpSenderInfo.FriendUid,
                        Result = (int)ErrorCode.InviteOfflineBrotherSuccess
                    };

                    client?.Write(response);
                    return;
                }

                bool added = false;
                switch ((MapType)model.Type)
                {
                    case MapType.Hunting:
                    case MapType.HuntingDeficute:
                    case MapType.HuntingTeamDevil:
                    case MapType.HuntingActivityTeam:
                        HuntingModel huntingModel = HuntingLibrary.GetByMapId(model.Id);
                        if (huntingModel == null) return;

                        if (friend.Research > 0 && friend.Research >= huntingModel.ResearchLimit)
                        {
                            if (CheckTeamFull(friend))
                            {
                                client?.Write(new MSG_RZ_NEED_TEAM_HELP() { Result = (int)ErrorCode.AnswerJoinMemberInOtherTeam });
                                return;
                            }
                            added = true;
                            helpList.Add(friend);
                        }
                        else
                        {
                            client?.Write(new MSG_RZ_NEED_TEAM_HELP() { Result = (int)ErrorCode.FriendResearchNotEnough });
                            return;
                        }
                        break;
                    default:
                        if (friend.Level >= minLevel && friend.Level <= maxLevel && friend.ChapterId >= model.ChapterLimit)
                        {
                            if (CheckTeamFull(friend))
                            {
                                client?.Write(new MSG_RZ_NEED_TEAM_HELP() { Result = (int)ErrorCode.AnswerJoinMemberInOtherTeam });
                                return;
                            }
                            added = true;
                            helpList.Add(friend);
                        }
                        else
                        {
                            client?.Write(new MSG_RZ_NEED_TEAM_HELP() { Result = (int)ErrorCode.FriendNotOpenDungeon });
                            return;
                        }
                        break;
                }
                if (added)
                {
                    client.CurZone.Write(new MSG_RZ_NEED_TEAM_HELP { Result = (int)ErrorCode.Success }, client.Uid);
                }
            }
            else
            {
                foreach (var id in info.HelpSenderInfo.Friends)
                {
                    if (helpList.Count >= TeamLibrary.HelpCount) break;

                    friend = this.server.GetClient(id);
                    if (friend == null || !friend.IsOnline || team.MemberList.ContainsKey(id))
                    {
                        continue;
                    }

                    if (CheckHelpLimit(model, cap, friend, minLevel, maxLevel, minResearch))
                    {
                        helpList.Add(friend);
                    }
                }

                SupplementHelpList(helpList, team, info, cap, model, minLevel, maxLevel, minResearch);
            }

            MSG_RZ_REQUEST_TEAM_HELP notify = new MSG_RZ_REQUEST_TEAM_HELP()
            {
                TeamId = team.TeamId,
                TeamType = team.Type,
                Name = info.HelpSenderInfo.Name,
                Level = info.HelpSenderInfo.Level,
                Camp = info.HelpSenderInfo.Camp,
                Uid = info.HelpSenderInfo.Uid,
                Research = cap.Research,
            };

            team.SetSendNeedHelpTime(BaseApi.now);
            helpList.ForEach(x => x.CurZone.Write(notify, x.Uid));
        }

        private bool CheckTeamFull(Client client)
        {
            return client.Team?.CheckFull() == true;
        }

        private void SupplementHelpList(List<Client> helpList, Team team, TeamHelpInfo info, Client cap, DungeonModel model, int minLevel, int maxLevel, int minResearch)
        {
            int needNum = TeamLibrary.HelpCount - helpList.Count;
            if (needNum <= 0) return;

            int i = 0;
            foreach (var player in this.server.ClientList)
            {
                if (i > needNum) break;

                if (player.Value.IsInDungeon()) continue;
                if (team.MemberList.ContainsKey(player.Value.Uid)) continue;
                if (info.HelpSenderInfo.Friends.Contains(player.Value.Uid)) continue;

                if (CheckHelpLimit(model, cap, player.Value, minLevel, maxLevel, minResearch))
                {
                    helpList.Add(player.Value);
                }
            }
        }

        private bool CheckFriendHelpLimit(DungeonModel model, Client cap, Client player, int minLevel, int maxLevel, int minResearch)
        {
            switch ((MapType)model.Type)
            {
                case MapType.Hunting:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.HuntingActivityTeam:
                    if (player.Research > 0 && player.Research >= minResearch && player.Research <= cap.Research && player.Level >= minLevel && player.Level <= maxLevel)
                    {
                        return true;
                    }
                    break;
                default:
                    if (player.Level >= minLevel && player.Level <= maxLevel && player.ChapterId >= model.ChapterLimit)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        private bool CheckHelpLimit(DungeonModel model, Client cap, Client player, int minLevel, int maxLevel, int minResearch)
        {
            if (CheckTeamFull(player)) return false;

            switch ((MapType)model.Type)
            {
                case MapType.Hunting:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.HuntingActivityTeam:
                    if (player.Research > 0 && player.Research >= minResearch && player.Research <= cap.Research && player.Level >= minLevel && player.Level <= maxLevel)
                    {
                        return true;
                    }
                    break;
                default:
                    if (player.Level >= minLevel && player.Level <= maxLevel && player.ChapterId >= model.ChapterLimit)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        public void AddToHelpList(TeamHelpInfo info)
        {
            this.teamHelpList[info.TeamId] = info;
        }

        public void RemoveHelpInfo(List<int> helpList)
        {
            helpList.ForEach(id => this.teamHelpList.Remove(id));
        }

        private void CheckNeedTeamHelp()
        {
            Team team = null;
            List<int> removeList = new List<int>();
            foreach (var kv in teamHelpList)
            {
                team = GetTeam(kv.Key);
                if (team == null || team.InDungeon || team.CheckFull())
                {
                    removeList.Add(kv.Key);
                    continue;
                }

                if ((BaseApi.now - kv.Value.SendTimes.Last()).TotalSeconds > TeamLibrary.RepeatSendTime)
                {
                    //removeList.Add(kv.Key);
                    waittingAddRobortTeam.Add(kv.Key);
                    kv.Value.SendTimes.Add(BaseApi.now);
                    if (kv.Value.SendTimes.Count > TeamLibrary.ResearchRanges.Count)
                    {
                        removeList.Add(kv.Key);
                    }
                    NeedTamHelp(kv.Value, true,kv.Value.SendTimes.Count);
                }

                //if ((BaseApi.now - kv.Value.FirstSendTime).TotalSeconds > TeamLibrary.RepeatSendTime)
                //{
                //    removeList.Add(kv.Key);
                //    waittingAddRobortTeam.Add(kv.Key);
                //    NeedTamHelp(kv.Value, true);
                //}
            }
            if (removeList.Count > 0)
            {
                removeList.ForEach(id => this.teamHelpList.Remove(id));
            }
        }

        private void CheckNeedAddRobort()
        {
            Team team = null;
            List<int> removeList = new List<int>();
            foreach (var id in waittingAddRobortTeam)
            {
                team = GetTeam(id);
                if (team == null || team.InDungeon || team.CheckEnough())
                {
                    removeList.Add(id);
                    continue;
                }

                if ((BaseApi.now - team.LastSendNeedHelpTime).TotalSeconds > TeamLibrary.RobortTime)
                {
                    AddRobort(team);
                    removeList.Add(id);
                    continue;
                }
            }
            if (removeList.Count > 0)
            {
                removeList.ForEach(id => this.waittingAddRobortTeam.Remove(id));
            }
        }

        private void AddRobort(Team team)
        {
            //Log.Debug($"team {team.TeamId} add robort");
            //team.AddRobort();
            //todo 挑选一个合适的robot放入
            team.ChooseRobotForCaptain();
        }

        private void AddToTypeList(Team team)
        {
            List<int> list;
            if (teamTypeList.TryGetValue(team.Type, out list))
            {
                list.Add(team.TeamId);
            }
            else
            {
                list = new List<int>();
                list.Add(team.TeamId);
                teamTypeList.Add(team.Type, list);
            }
        }

        private void RemoveFromTypeList(int teamType, int teamId)
        {
            List<int> list;
            if (teamTypeList.TryGetValue(teamType, out list))
            {
                list.Remove(teamId);
            }
        }

        //通知成员队伍解散
        private void NotifyRemoveTeam(Team team)
        {
            foreach (var member in team.MemberList.Values.ToList())
            {
                LeaveTeam(member.Uid);

                //成员推出队伍
                if (member.CheckOnline()&&member.Client!=null)
                {
                    member.Client.Team = null;
                    MSG_RZ_QUIT_TEAM notify = new MSG_RZ_QUIT_TEAM();
                    notify.Uid = member.Uid;
                    notify.Result = (int)ErrorCode.Success;

                    member.Client.CurZone.Write(notify);
                }
            }
        }

#if DEBUG
        //private void BuildFalseData(int teamType)
        //{
        //    List<int> list;
        //    if (!teamTypeList.TryGetValue(teamType, out list))
        //    {
        //        list = new List<int>();
        //        teamTypeList.Add(teamType, list);
        //    }
        //    int index = list.Count;
        //    int needCount = 100 - index;
        //    Team team;
        //    while (index++ < 100)
        //    {
        //        TeamMemeberInfo memberInfo = new TeamMemeberInfo()
        //        {
        //            CampId = 1,
        //            Icon = 1,
        //            IconFrame = 801,
        //            Job = 1,
        //            Level = 2,
        //            Name = "T" + index,
        //            Sex = 1,
        //            ShowDIYIcon = false,
        //            Uid = index,
        //            HeroId = 1,
        //        };

        //        TeamMember member = new TeamMember(null, memberInfo);
        //        team = CreateTeam(member, teamType);
        //    }
        //}

#endif

        //通知队伍类型变动
        private void NotifyTeamTypeChange(Team team)
        {
            foreach (var kv in team.MemberList)
            {
                if (kv.Value.CheckOnline())
                {
                    MSG_RZ_CHANGE_TEAM_TYPE response = new MSG_RZ_CHANGE_TEAM_TYPE();
                    response.Result = (int)ErrorCode.Success;
                    response.Uid = kv.Key;
                    response.TeamType = team.Type;
                    kv.Value.Client?.CurZone.Write(response);
                }
            }
        }
    }
}
