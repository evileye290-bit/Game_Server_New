using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        //副本
        private bool reEnterDungeon = false;
        //现在只有攻击方是玩家。防御方都是机器人（包括玩家镜像）所以这里默认IsAttacker = true也不去设置了

        public bool SetReEnterDungeon(bool reEnter)
        {
            bool last = reEnterDungeon;
            reEnterDungeon = reEnter;
            return last;
        }

        public bool GetReEnterDungeon()
        {
            return reEnterDungeon;
        }

        public ErrorCode CanCreateDungeon(int dungeonId)
        {
            // 根据不同副本，进行一系列检查
            if (NotStableInMap())
            {
                return ErrorCode.IsTransforming;
            }
            if (currentMap != null && currentMap.IsDungeon && currentMap.Model.MapType!=MapType.CrossChallenge)
            {
                return ErrorCode.InDungeon;
            }
            MapModel mapModel = MapLibrary.GetMap(dungeonId);

            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(dungeonId);
            if (mapModel == null || dungeonModel == null)
            {
                return ErrorCode.NotExist;
            }

            //等级
            if (!dungeonModel.CheckLevelLimit(Level))
            {
                return ErrorCode.LevelLimit;
            }

            //职业
            if (CheckJobPermission(dungeonModel))
            {
                return ErrorCode.JobNotPermission;
            }

            switch (mapModel.MapType)
            {
                case MapType.CommonSingleDungeon:
                    {
                        if (Team != null)
                        {
                            return ErrorCode.InTeam;
                        }
                        //允许Gm手动创建副本
                        if (IsGm != 1)
                        {
                            if (!TaskMng.CheckDungeonTask(dungeonId))
                            {
                                //任务中没有通关该副本任务
                                Log.WarnLine($"player {Uid} create dungeon id {dungeonId} not exist in task");
                                return ErrorCode.Fail;
                            }
                        }
                    }
                    break;
                case MapType.NoCheckSingleDungeon:
                    {
                        if (Team != null)
                        {
                            return ErrorCode.InTeam;
                        }
                        //允许Gm手动创建副本
                        if (IsGm != 1)
                        {
                            if (!TaskMng.CheckOneDungeonTask(dungeonId))
                            {
                                //任务中没有通关该副本任务
                                Log.WarnLine($"player {Uid} create dungeon id {dungeonId} not exist in task");
                                return ErrorCode.Fail;
                            }
                        }
                    }
                    break;
                case MapType.TeamDungeon:
                    {
                        ErrorCode teamResult = CanCreateTeamDungeon(dungeonId);
                        if (teamResult != ErrorCode.Success)
                        {
                            return teamResult;
                        }
                    }
                    break;
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulPower:
                case MapType.SoulBreath:
                    {
                        if (!CheckLimitOpen(mapModel.MapType))
                        {
                            return ErrorCode.DungeonNotOpen;
                        }

                        if (Team != null)
                        {
                            return ErrorCode.InTeam;
                        }

                        //int restChallenteCount = GetDungeonChallengeRestCount(mapModel.MapType);
                        MapCounterModel model = CounterLibrary.GetCounterType(mapModel.MapType);
                        int restChallenteCount = GetCounterValue(model.Counter);
                        if (restChallenteCount <= 0)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                case MapType.Hunting:
                    {
                        if (Team != null) return ErrorCode.InTeam;

                        if(!HuntingCheckResearch(dungeonId)) return ErrorCode.HuntingResearchNotEnough;

                        if (HuntingManager.CheckHuntingActivityUnlocked(dungeonId)) return ErrorCode.HuntingActivityUnlocked;
                    }
                    break;
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                    {
                        if (!HuntingCheckResearch(dungeonId)) return ErrorCode.HuntingResearchNotEnough;

                        return CanCreateTeamDungeon(dungeonId);
                    }
                case MapType.HuntingActivitySingle:
                    if (HuntingManager.CheckHuntingActivityUnlocked(dungeonId)) return ErrorCode.HuntingActivityUnlocked;
                    break;
                case MapType.HuntingActivityTeam:
                    if (HuntingManager.CheckHuntingActivityUnlocked(dungeonId)) return ErrorCode.HuntingActivityUnlocked;
                    return CanCreateTeamDungeon(dungeonId);
                case MapType.IntegralBoss:
                    {
                        ErrorCode teamResult = CanCreateTeamDungeon(dungeonId);
                        if (teamResult != ErrorCode.Success)
                        {
                            return teamResult;
                        }

                        if (!server.IntegralBossManager.IsOpenning)
                        {
                            return ErrorCode.DungeonNotOpen;
                        }
                    }
                    break;
                case MapType.Arena:
                    {
                        if (Team != null)
                        {
                            return ErrorCode.InTeam;
                        }
                        int restChallenteCount = GetDungeonChallengeRestCount(mapModel.MapType);
                        if (restChallenteCount <= 0)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                case MapType.Versus:
                    {
                        if (Team != null)
                        {
                            return ErrorCode.InTeam;
                        }
                    }
                    break;
                case MapType.CrossBattle:
                    {
                        CrossLevelInfo info = CrossBattleLibrary.CheckCrossLevel(CrossInfoMng.Info.Star);
                        if (info == null)
                        {
                            return ErrorCode.DungeonNotOpen;
                        }
                        //判断时间
                        if (!CrossBattleLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossBattleMng.StartTime, server.Now()))
                        {
                            return ErrorCode.DungeonNotOpen;
                        }
                        MapCounterModel model = CounterLibrary.GetCounterType(mapModel.MapType);
                        int restChallenteCount = GetCounterValue(model.Counter);
                        if (restChallenteCount <= 0)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                case MapType.CrossBoss:
                    {
                        int restChallenteCount = GetDungeonChallengeRestCount(mapModel.MapType);
                        if (restChallenteCount <= 0)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                case MapType.CrossBossSite:
                    {
                        //int restChallenteCount = GetDungeonChallengeRestCount(mapModel.MapType);
                        //if (restChallenteCount <= 0)
                        //{
                        //    return ErrorCode.ChallengeCountNotEnough;
                        //}
                        return ErrorCode.Success;
                    }
                    break;
                case MapType.SecretArea:
                    return SecretAreaManager.CheckCreateDungeon(dungeonId);
                case MapType.Chapter:
                    //return ChapterManager.CheckCreateDungeon(dungeonModel);
                case MapType.GodPath:
                    return CheckCreateDungeon(dungeonModel);
                case MapType.GodPathAcrossOcean:
                    {
                        if (((int)dungeonModel.Difficulty) - AcroessOceanDiff > 1)
                        {
                            return ErrorCode.NeedPassLowDiffcuteDungeon;
                        }

                        if (GetDungeonChallengeRestCount(mapModel.MapType) <= 0)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                case MapType.Tower:
                    return ErrorCode.Success;
                case MapType.CampGatherEncounterEnemy:
                case MapType.CampGatherEncounterMonster:
                    if (!CheckCampGatherHasDungeon(dungeonId))
                    {
                        return ErrorCode.BadDungeonId;
                    }
                    CampBattleStep step = GetCampBattleStep();
                    CampBattleExpendModel expend = GetCampBattleExpend(step);
                    if (expend != null)
                    {
                        //行动力检测
                        if (GetCounterValue(CounterType.ActionCount) < expend.CollectionPoint.Item1)
                        {
                            //采集副本
                            if (ExpendedAction != 0)
                            {
                                ExpendedAction = 0;
                            }
                            else
                            {
                                return ErrorCode.ActionCountNotEnough;
                            }
                        }
                    }
                    break;
                case MapType.CampBattleNeutral:           
                case MapType.CampBattle:
                case MapType.CampDefense:
                    return ErrorCode.Fail;
                case MapType.PushFigure:
                    if (Team != null) return ErrorCode.InTeam;
                    return ChechCreatePushFigureDungeon(dungeonId);
                case MapType.VideoPlay:
                case MapType.CrossChallengeVideoPlay:
                    return ErrorCode.Success;
                case MapType.ThemeBoss:
                    {
                        if (Team != null)
                        {
                            return ErrorCode.InTeam;
                        }
                        int restChallenteCount = GetDungeonChallengeRestCount(mapModel.MapType);
                        if (restChallenteCount <= 0)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                case MapType.CarnivalBoss:
                    {
                        if (Team != null)
                        {
                            return ErrorCode.InTeam;
                        }
                        int restChallenteCount = GetDungeonChallengeRestCount(mapModel.MapType);
                        if (restChallenteCount <= 0)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                case MapType.IslandChallenge:
                    return ErrorCode.Success;
                case MapType.CrossChallenge:
                    {
                        CrossLevelInfo info = CrossChallengeLibrary.CheckCrossLevel(CrossInfoMng.Info.Star);
                        if (info == null)
                        {
                            return ErrorCode.DungeonNotOpen;
                        }
                        //判断时间
                        if (!CrossChallengeLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossChallengeMng.StartTime, server.Now()))
                        {
                            return ErrorCode.DungeonNotOpen;
                        }
                        MapCounterModel model = CounterLibrary.GetCounterType(mapModel.MapType);
                        int restChallenteCount = GetCounterValue(model.Counter);
                        if (restChallenteCount <= 0)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                case MapType.HuntingIntrude:
                    {
                        if (Team != null)
                        {
                            return ErrorCode.InTeam;
                        }
                        break;
                    }
                case MapType.SpaceTimeTower:
                    {
                        if (!SpaceTimeTowerMng.IsOpening())
                        {
                            return ErrorCode.DungeonNotOpen;
                        }
                        if (SpaceTimeTowerMng.FailCount >= SpaceTimeTowerLibrary.ChallengeMaxCount)
                        {
                            return ErrorCode.ChallengeCountNotEnough;
                        }
                    }
                    break;
                default:
                    return ErrorCode.DungeonNotOpen;
            }

            return ErrorCode.Success;
        }

        private ErrorCode CanCreateTeamDungeon(int dungeonId)
        {
            DungeonModel model = DungeonLibrary.GetDungeon(dungeonId);
            if (!IsCaptain())
            {
                return ErrorCode.NotTeamCaptain;
            }

            if (!model.CheckMemberCountLimit(Team.MemberCount))
            {
                return ErrorCode.DungeonMemberCountLimit;
            }

            //CheckAndDestoryTeam();
            if (Team == null)
            {
                return ErrorCode.NotInTeam;
            }

            foreach (var member in Team.MemberList)
            {
                if (!member.Value.IsOnline &&!member.Value.IsAllowOffline)
                {
                    return ErrorCode.TeamMemberOfflineOfDungeon;
                }

                if (!model.CheckLevelLimit(member.Value.Level))
                {
                    return ErrorCode.TeamMemberLevelLimit;
                }

                if (!member.Value.IsRobot && !member.Value.IsAllowOffline && model.ChapterLimit > member.Value.Chapter)
                { 
                    Log.Warn($"Team ErrorCode.TeamMemberChapterLimit, info is robot {member.Value.IsRobot} allowOffline {member.Value.IsAllowOffline} model chapter limit {model.ChapterLimit} memeber chapter {member.Value.Chapter}");
                    return ErrorCode.TeamMemberChapterLimit;
                }

                if (member.Value.IsAllowOffline)
                {
                    if (!CheckBrotherExist(member.Value.Uid)) return ErrorCode.NotFriendBrotherListFull;
                }

                PlayerChar player = server.PCManager.FindPc(member.Key);
                if (player == null)
                {
                    continue;
                }
                else
                {
                    //镜像不用检查队友
                    if (Team.CaptainUid != member.Key && Team.IsInviteMirror)
                    {
                        continue;
                    }

                    if (player.InDungeon || !player.IsMapLoadingDone || player.LoadingDoneCreateDungeonWaiting > ZoneServerApi.now)
                    {
                        return ErrorCode.TeamMemberInDungeon;
                    }

                    if (player.NotStableInMap())
                    {
                        return ErrorCode.IsTransforming;
                    }
                }
             
            }

            // todo 有阵营要求的副本 需要检查队员阵营

            //检查副本是否强制要求伙伴数量
            //DungeonModel model = DungeonLibrary.GetDungeon(dungeonId);
            if (model.TeamMemberLimitMin == 1 && Team.MemberCount == 1)
            {
                if (HeroMng.GetAllHeroPosHeroId().Count < 4)
                {
                    return ErrorCode.HeroNumMinLimit;
                }
            }
            return ErrorCode.Success;
        }

        //副本职业限定
        public bool CheckJobPermission(DungeonModel model)
        {
            string permission = model.Data.GetString("Permission");
            if (string.IsNullOrEmpty(permission))
            {
                //没有限制
                return false;
            }

            Dictionary<int, int> job2Count = StringSplit.GetKVPairs(permission);
            if (job2Count.Count == 0)
            {
                return false;
            }

            if (HeroMng.CheckJobPermission(job2Count))
            {
                return true;
            }

            return false;
        }


        public void RecordEnterMapInfo(int destMapId, int destChannel, Vec2 destPos)
        {
            EnterMapInfo.SetInfo(destMapId, destChannel, destPos);
        }

        public void RecordOriginMapInfo()
        {
            OriginMapInfo.SetInfo(currentMap.MapId, currentMap.Channel, Position);
        }

        public void RecordLastMapInfo(int lastMapId, int lastChannel, Vec2 lastPos)
        {
            LastMapInfo.SetInfo(lastMapId, lastChannel, lastPos);
        }


        /// <summary>
        /// 废弃
        /// </summary>
        /// <param name="dungeonId"></param>
        public DungeonMap CreateDungeon(int dungeonId)
        {
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = dungeonId;
            response.Result = (int)ErrorCode.Success; //CanCreateDungeon(dungeonId);
            if (response.Result != (int)ErrorCode.Success)
            {
                Log.Write($"player {Uid} request to create dungeon {dungeonId} failed: reason {response.Result}");
                Write(response);
                return null;
            }

            DungeonMap dungeon = null;
            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(dungeonId);
            bool needCheck = dungeonModel == null? true : dungeonModel.NeedCheck();

            // 根据状态 决定在当前zone创建副本还是请求manager做均衡负载
            if (server.Fps.GetFrame() >= GameConfig.ACCEPT_DUNGEON_FRAME || !needCheck)
            {
                dungeon = server.MapManager.CreateDungeon(dungeonId, HeroMng.GetGoldHeroCount());
                if (dungeon == null)
                {
                    Log.Write($"player {Uid} request to create dungeon {dungeonId} failed: create dungeon failed");
                    response.Result = (int)ErrorCode.CreateDungeonFailed;
                    Write(response);
                    return null;
                }

                if (dungeon.Model.IsTeamDungeon())
                {
                    if (Team != null)
                    {
                        TeamDungeonMap teamDungeonMap = dungeon as TeamDungeonMap;
                        teamDungeonMap.InitTeamDungeonMap(Team.MemberCount - Team.RobotCount, Team.TeamId);
                        //向relation通知队员进入副本
                        teamDungeonMap.NotifyTeamMembersEnter(Uid);
                    }
                }
                // 成功 进入副本
                RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
                RecordOriginMapInfo();
                OnMoveMap();
            }
            else
            {
                MapModel model = MapLibrary.GetMap(dungeonId);
                if (model != null)
                {
                    // 当前负载较高，需要向manager请求均衡负载
                    MSG_ZM_NEED_DUNGEON request = new MSG_ZM_NEED_DUNGEON();
                    request.Uid = Uid;
                    request.DestDungeonId = dungeonId;
                    request.OriginMapId = CurrentMapId;
                    request.OriginChannel = CurrentChannel;
                    request.OriginPosX = Position.X;
                    request.OriginPosY = Position.Y;

                    if (model.IsTeamDungeon())
                    {
                        if (Team != null)
                        {
                            request.TheoryMemberCount = Team.MemberCount - Team.RobotCount;
                            request.TeamId = Team.TeamId;
                        }
                    }

                    server.ManagerServer.Write(request);
                    SetIsTransforming(true);
                }
            }
            return dungeon;
        }

        public void ManagerCreateDungeon(int dungeonId, bool huntingHelp = false)
        {
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = dungeonId;
            response.Result = (int)CanCreateDungeon(dungeonId);
            if (response.Result != (int)ErrorCode.Success)
            {
                Log.Write($"player {Uid} request to create dungeon {dungeonId} failed: reason {response.Result}");
                Write(response);
                return;
            }

            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(dungeonId);
            if (huntingHelp)
            {
                //猎杀魂兽求援
                if (!dungeonModel.IsHuntingHelpDungeon())
                {
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                if (CheckCounter(CounterType.AskTopRankHelp))
                {
                    response.Result = (int)ErrorCode.AskTopRankCountNotEnough;
                    Write(response);
                    return;
                }
            }

            //bool needCheck = dungeonModel == null ? true : dungeonModel.NeedCheck();
            // 根据状态 决定在当前zone创建副本还是请求manager做均衡负载
            //当前zone禁止进入 到，manager做负载均衡
            if (ZoneTransformManager.Instance.IsForbided(server.SubId) || server.Fps.GetFrame() < GameConfig.ACCEPT_DUNGEON_FRAME)
            {
                MapModel model = MapLibrary.GetMap(dungeonId);
                if (model != null)
                {
                    // 当前负载较高，需要向manager请求均衡负载
                    MSG_ZM_NEED_DUNGEON request = new MSG_ZM_NEED_DUNGEON();
                    request.Uid = Uid;
                    request.DestDungeonId = dungeonId;
                    request.OriginMapId = CurrentMapId;
                    request.OriginChannel = CurrentChannel;
                    request.OriginPosX = Position.X;
                    request.OriginPosY = Position.Y;

                    if (model.IsTeamDungeon())
                    {
                        if (Team != null)
                        {
                            request.TheoryMemberCount = Team.MemberCount - Team.RobotCount;
                            request.TeamId = Team.TeamId;
                        }
                    }

                    server.ManagerServer.Write(request);
                    SetIsTransforming(true);
                }
            }
            else
            {
                DungeonMap dungeon = server.MapManager.CreateDungeon(dungeonId, HeroMng.GetGoldHeroCount(), Uid); //等manager消息进行下一步
                if (dungeon == null)
                {
                    Log.Write($"player {Uid} request to create dungeon {dungeonId} failed: create dungeon failed");
                    response.Result = (int)ErrorCode.CreateDungeonFailed;
                    Write(response);
                    return;
                }

                if (dungeon.Model.IsTeamDungeon())
                {
                    if (Team != null)
                    {
                        TeamDungeonMap teamDungeonMap = dungeon as TeamDungeonMap;
                        teamDungeonMap.InitTeamDungeonMap(Team.MemberCount - Team.RobotCount, Team.TeamId);
                        teamDungeonMap.SetIsHelpState(huntingHelp, uid);
                    }
                }
                IsAttacker = true;
                // 成功 进入副本
                RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
                RecordOriginMapInfo();
                OnMoveMap();
            }
        }

        //public void AfterManagerCreateDungeon(int dungeonId,int channel)
        //{
        //    DungeonModel dungeonModel = DungeonLibrary.GetDungeon(dungeonId);

        //    DungeonMap dungeon = server.MapManager.GetFieldMap(dungeonId, channel) as DungeonMap;
        //    // 根据状态 决定在当前zone创建副本还是请求manager做均衡负载
        //    if (dungeon.Model.IsTeamDungeon())
        //    {
        //        if (Team != null)
        //        {
        //            TeamDungeonMap teamDungeonMap = dungeon as TeamDungeonMap;
        //            teamDungeonMap.InitTeamDungeonMap(Team.MemberCount-Team.RobotCount, Team.TeamId);
        //            //向relation通知队员进入副本
        //            teamDungeonMap.NotifyTeamMembersEnter(Uid);
        //        }
        //    }
        //}

        public void CreateRobotRankDungeon(int dungeonId,int rank)
        {
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = dungeonId;
            response.Result = (int)ErrorCode.Success;// (int)CanCreateDungeon(dungeonId);
            if (response.Result != (int)ErrorCode.Success)
            {
                Log.Write($"player {Uid} request to create dungeon {dungeonId} failed: reason {response.Result}");
                Write(response);
                return;
            }

            // 在当前zone创建副本
            DungeonMap dungeon = server.MapManager.CreateDungeon(dungeonId);
            if (dungeon == null)
            {
                Log.Write($"player {Uid} request to create dungeon {dungeonId} failed: create dungeon failed");
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }

            //dungeon.AddMirrorRobot(this);
            //player.PullTeamMemberIntoTeamDungeon();
            ArenaRobotInfo info = RobotLibrary.GetArenaRobotInfo(rank);
            List<int> heros = RobotManager.GetHeroRobotIdList(info);// todo 需要把主角的sex name等信息提供到RobotHeroInfo中
            dungeon.AddDefenderHeros(heros);
            // 成功 进入副本
            RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();
        }

        public void CreateRobotDungeon(int dungeonId)
        {
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = dungeonId;
            response.Result = (int)ErrorCode.Success;// (int)CanCreateDungeon(dungeonId);
            if (response.Result != (int)ErrorCode.Success)
            {
                Log.Write($"player {Uid} request to create dungeon {dungeonId} failed: reason {response.Result}");
                Write(response);
                return;
            }

            // 在当前zone创建副本
            DungeonMap dungeon = server.MapManager.CreateDungeon(dungeonId);
            if (dungeon == null)
            {
                Log.Write($"player {Uid} request to create dungeon {dungeonId} failed: create dungeon failed");
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }

            //if (dungeon.Model.IsTeamDungeon())
            //{
            //    //if (Team != null)
            //    //{
            //    //    TeamDungeonMap teamDungeonMap = dungeon as TeamDungeonMap;
            //    //    teamDungeonMap.InitTeamDungeonMap(Team.MemberCount, Team.TeamId);
            //    //    // 将robot 拉入战场
            //    //    teamDungeonMap.AddMirrorRobot(this);
            //    //}
            //    dungeon.AddMirrorRobot(this);
            //}

            dungeon.AddMirrorRobot(true,this);
            //player.PullTeamMemberIntoTeamDungeon();
            // 成功 进入副本
            RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();
        }

        public void StopDungeon()
        {
            if (CurDungeon == null)
            {
                Log.Write($"player {Uid} request to stop dungeon battle failed, cur map is null");
                return;
            }

            CurDungeon.OnStopBattle(this);
        }

        public void SetDungeonResult(int result)
        {
            if (CurDungeon == null)
            {
                Log.Write($"player {Uid} request to set dungeon result failed, cur map is null");
                return;
            }

            //不需要后端演算的副本
            if (!CurDungeon.DungeonModel.NeedCheck())
            {
                CurDungeon.Stop((DungeonResult)result);
            }
        }

        public void LeaveDungeon()
        {
            if (CurDungeon == null)
            {
                Log.Write($"player {Uid} request to leave dungeon failed, cur map is null");
                return;
            }

            // todo 其他判断，如组队副本不允许再有其他成员情况下离开等
            // 成功后 返回之前的地图 离开
            if (server.PCManager.FindOfflinePc(uid) == null)
            {
                //用于从剧情副本回来，跳到某个NPC位置
                if (CurDungeon.DungeonModel.DesPos != null && CurDungeon.DungeonResult == DungeonResult.Success)
                {
                    OriginMapInfo.SetPosition(CurDungeon.DungeonModel.DesPos);
                }

                MapModel mapModel = MapLibrary.GetMap(OriginMapInfo.MapId);
                if (mapModel != null)
                {
                    if (mapModel.MapType != MapType.Map)
                    {
                        Log.Warn("player {0} LeaveDungeon enter map {1} error, Channel {2} MapType is {3}", Uid, OriginMapInfo.MapId, OriginMapInfo.Channel, mapModel.MapType);
                        mapModel = MapLibrary.GetMap(CONST.MAIN_MAP_ID);
                        OriginMapInfo.SetInfo(CONST.MAIN_MAP_ID, CONST.MAIN_MAP_CHANNEL, mapModel.BeginPos);
                    }
                }
                else
                {
                    Log.Warn("player {0} LeaveDungeon enter map {1} error, Channel {2} not find model", Uid, OriginMapInfo.MapId, OriginMapInfo.Channel);
                    mapModel = MapLibrary.GetMap(CONST.MAIN_MAP_ID);
                    OriginMapInfo.SetInfo(CONST.MAIN_MAP_ID, CONST.MAIN_MAP_CHANNEL, mapModel.BeginPos);
                }

                AskForEnterMap(OriginMapInfo.MapId, OriginMapInfo.Channel, OriginMapInfo.Position);
            }
            else
            {
                LeaveWorld();
            }
        }

        public void ReliveTeamMember(int instanceId)
        {
            if (CurTeamDungeon == null)
            {
                Log.Write($"player {Uid} relive team member failed, CurTeamDungeon is null");
                return;
            }

            PlayerChar player = CurTeamDungeon.GetPlayer(instanceId);
            if (player == null)
            {
                Log.Write($"player {Uid} relive team member failed, not find member instance Id {instanceId}");
            }

            CurTeamDungeon.RevivePlayer(player);
        }

        public int GetDungeonChallengeCount(int dungeonId)
        {
            var model = MapLibrary.GetMap(dungeonId);
            if (model == null)
            {
                return 0;
            }
            switch (model.MapType)
            {
                case MapType.Hunting:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.HuntingActivitySingle:
                case MapType.HuntingActivityTeam:
                    MapCounterModel mapCounter = CounterLibrary.GetCounterType(model.MapType);
                    return GetCounterValue(mapCounter.Counter);
                default:
                    return GetDungeonChallengeRestCount(model.MapType);
            }
        }

        public bool CheckHelpRewardCount()
        {
            return CheckCounter(CounterType.TeamHelpCount);
        }

        /// <summary>
        /// 帮杀奖励
        /// </summary>
        public RewardManager GetDungeonHelpReward()
        {
            RewardManager manager = new RewardManager();
            HelpRewardModel hrModel = HelpRewardLibrary.GetHelpReward(Level);
            if (hrModel == null) return manager;

            manager.InitSimpleReward(hrModel.Data.GetString("reward"));
            return manager;
        }

        /// <summary>
        /// 帮杀奖励用完结算通知
        /// </summary>
        public void NotifyDungeonHelpUeslessRewardMsg(int dungeonId)
        {
            //帮杀次数用完
            MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
            rewardMsg.DungeonId = dungeonId;
            rewardMsg.Result = (int)DungeonResult.HelpCountUseUp;
            Write(rewardMsg);
        }

        public MSG_ZGC_DUNGEON_REWARD GetRewardSyncMsg(RewardManager mng)
        {
            MSG_ZGC_DUNGEON_REWARD msg = new MSG_ZGC_DUNGEON_REWARD();
            mng.GenerateRewardMsg(msg.Rewards);
            return msg;
        }

        public void NotifyRelationEnterDungeon()
        {
            MSG_ZR_PLAYER_ENTER_DUNGEON msg = new MSG_ZR_PLAYER_ENTER_DUNGEON();
            server.SendToRelation(msg, uid);
        }

        public void NotifyRelationLeaveDungeon()
        {
            MSG_ZR_PLAYER_LEAVE_DUNGEON msg = new MSG_ZR_PLAYER_LEAVE_DUNGEON();
            server.SendToRelation(msg, uid);
        }

        public void Request_BattleDungeonData()
        {
            if (currentMap is DungeonMap)
            {
                MSG_ZGC_DUNGEON_BATTLE_DATA msg = currentMap.BattleDataManager.GenerateBattleDataMsg(uid);
                Write(msg);
            }
        }


        public MSG_ZGC_EQUIP_HERO_INFO GenerateEquipedHeroInfo(HeroInfo heroInfo)
        {
            return new MSG_ZGC_EQUIP_HERO_INFO
            {
                HeroId = heroInfo.Id,
                HeroNature = GetEquipHeroNature(heroInfo),
                SoulSkillLevel = heroInfo.SoulSkillLevel,
                GodType = heroInfo.GodType,
                //GroValue = heroInfo.GetGroVal()
            };
        }

        private Equip_Hero_Nature GetEquipHeroNature(HeroInfo heroInfo)
        {
            Equip_Hero_Nature heroNature = new Equip_Hero_Nature();
            //foreach (var item in NatureLibrary.Basic9Nature)
            //{
            //    Hero_Nature_Item info = GetNatureItemMsg(item.Key, heroInfo.Nature);
            //    heroNature.List.Add(info);
            //}
            //foreach (var item in NatureLibrary.BasicSpeedNature)
            //{
            //    Hero_Nature_Item info = GetNatureItemMsg(item.Key, heroInfo.Nature);
            //    heroNature.List.Add(info);
            //}

            //{
            heroNature.MaxHp = heroInfo.GetNatureValue(NatureType.PRO_MAX_HP);
            heroNature.Atk = heroInfo.GetNatureValue(NatureType.PRO_ATK);
            heroNature.Def = heroInfo.GetNatureValue(NatureType.PRO_DEF);
            heroNature.Hit = heroInfo.GetNatureValue(NatureType.PRO_HIT);
            heroNature.Flee = heroInfo.GetNatureValue(NatureType.PRO_FLEE);
            heroNature.Cri = heroInfo.GetNatureValue(NatureType.PRO_CRI);
            heroNature.Res = heroInfo.GetNatureValue(NatureType.PRO_RES);
            heroNature.Imp = heroInfo.GetNatureValue(NatureType.PRO_IMP);
            heroNature.Arm = heroInfo.GetNatureValue(NatureType.PRO_ARM);

            heroNature.Spd = heroInfo.GetNatureValue(NatureType.PRO_SPD);
            heroNature.SpdInBattle = heroInfo.GetNatureValue(NatureType.PRO_RUN_IN_BATTLE);
            heroNature.SpdOutBattle = heroInfo.GetNatureValue(NatureType.PRO_RUN_OUT_BATTLE);
            //};

            return heroNature;
        }

        public bool CanSkipBattle(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.PushFigure:
                    if (!CheckAllMonthCard())
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }
    }
}
