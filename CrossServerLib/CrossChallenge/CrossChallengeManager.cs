using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using EnumerateUtility;
using Google.Protobuf;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RC;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace CrossServerLib
{

    public class CrossChallengeManager
    {
        private CrossServerApi server { get; set; }
        //uid, rank
        public int FirstStartTime { get; set; }
        private DateTime lastCheckTime { get; set; }
        private bool IsReset { get; set; }
        private CrossBattleTiming currentTiming { get; set; }
        private int InitResult = (int)CrossBattleTiming.FinalsStart;

        //uid info
        private Dictionary<int, CrossChallengeGroupModel> groupList = new Dictionary<int, CrossChallengeGroupModel>();
        private Dictionary<int, RedisPlayerInfo> playerBaseInfoList = new Dictionary<int, RedisPlayerInfo>();
        private Dictionary<int, List<CrossHeroInfo>> playerHeroInfoList = new Dictionary<int, List<CrossHeroInfo>>();

        private Dictionary<CrossBattleTiming, CrossChallengeTimingFightModel> timingGroupList = new Dictionary<CrossBattleTiming, CrossChallengeTimingFightModel>();
        private Dictionary<CrossBattleTiming, CrossGuessingGroupModel> timingGuessingList = new Dictionary<CrossBattleTiming, CrossGuessingGroupModel>();

        public CrossChallengeManager(CrossServerApi server)
        {
            this.server = server;

            TaskTimerQuery counterTimer = new CrossBattleTimerQuery(10000);
            Log.Info($"CrossChallengeManager call timing task ：after 10000");
            server.TaskTimerMng.Call(counterTimer, (ret) =>
            {
                //延迟10秒初始化
                Init();
            });
        }

        //初始化
        public void Init()
        {
            groupList.Clear();
            playerBaseInfoList.Clear();
            playerHeroInfoList.Clear();
            timingGroupList.Clear();
            timingGuessingList.Clear();

            //初始化列表
            InitGroupList(false);
            //加载分组数据
            InitGroupInfos();

            InitBattleInfo();
        }

        //初始化当前时间点
        private void InitTimerManager()
        {
            OperateGetCrossChallengeInfo operate = new OperateGetCrossChallengeInfo();
            server.CrossRedis.Call(operate, ret =>
            {
                if (operate.IsGetValue)
                {
                    string last = operate.GetInfo(HashField_CrossBattle.lastTiming);
                    if (!string.IsNullOrEmpty(last))
                    {
                        currentTiming = (CrossBattleTiming)int.Parse(last);
                        string first = operate.GetInfo(HashField_CrossBattle.FirstStartTime);
                        if (!string.IsNullOrEmpty(first))
                        {
                            FirstStartTime = int.Parse(first);
                            lastCheckTime = CrossChallengeLibrary.GetNextTime(CrossBattleTiming.Start, currentTiming, Timestamp.TimeStampToDateTime(FirstStartTime));
                            CrossChallengeStart();
                        }
                        else
                        {
                            //初始化从开始时间初始化
                            InitCrossChallengeBaseTime();
                        }
                    }
                    else
                    {
                        //初始化从开始时间初始化
                        InitCrossChallengeBaseTime();
                    }
                }
                else
                {
                    InitCrossChallengeBaseTime();
                }

                RunTimerManager();
            });
        }

        //初始化开始时间
        private void InitCrossChallengeBaseTime()
        {
            currentTiming = CrossBattleTiming.Start;
            lastCheckTime = CrossChallengeLibrary.GetBeforeTime(currentTiming, DateTime.Now);
            int lastTime = Timestamp.GetUnixTimeStampSeconds(lastCheckTime);
            server.CrossRedis.Call(new OperateSetCrossChallengeInfo(HashField_CrossBattle.lastTiming, (int)currentTiming));
            FirstStartTime = lastTime;
            server.CrossRedis.Call(new OperateSetCrossChallengeInfo(HashField_CrossBattle.FirstStartTime, FirstStartTime));
            IsReset = true;
            CrossChallengeStart();
        }

        //加载玩家基础信息
        private void InitPlayerBaseInfo(List<int> uidDic)
        {
            OperateGetCrossChallengePlayerBaseInfo operate = new OperateGetCrossChallengePlayerBaseInfo(uidDic);
            server.CrossRedis.Call(operate, ret =>
            {
                if (operate.Players.Count > 0)
                {
                    playerBaseInfoList = operate.Players;
                }

                //加载时间计时器
                InitTimerManager();
                return;
            });
        }

        //加载决赛信息
        private void InitGroupInfos()
        {
            OperateGetCrossChallengeGroupFightInfo operate = new OperateGetCrossChallengeGroupFightInfo(groupList);
            server.CrossRedis.Call(operate, ret =>
            {
                List<int> list = new List<int>();
                int maxResult = (int)CrossBattleTiming.FinalsStart;
                foreach (var item in operate.InfoLsit)
                {
                    int groupId = item.GetIntValue(HFCrossBattleGroup.Group);
                    CrossChallengeGroupModel group = GetBattleGroup(groupId);
                    if (group != null)
                    {
                        int team = item.GetIntValue(HFCrossBattleGroup.Team);
                        int uid = item.GetIntValue(HFCrossBattleGroup.Uid);
                        int index = item.GetIntValue(HFCrossBattleGroup.Index);
                        int result = item.GetIntValue(HFCrossBattleGroup.Result);
                        int oldTeam = item.GetIntValue(HFCrossBattleGroup.OldTeam);
                        group.Add(uid, team, index, result, oldTeam);

                        if (maxResult < result)
                        {
                            maxResult = result;
                        }
                        if (uid > 0)
                        {
                            list.Add(uid);
                        }
                    }
                }
                if (maxResult < (int)CrossBattleTiming.BattleTime6)
                {
                    //初始化分组
                    InitDivideIntoGroups(maxResult);
                }


                if (list.Count > 0)
                {
                    //加载玩家基本数据
                    InitPlayerBaseInfo(list);
                }
                else
                {
                    //加载时间计时器
                    InitTimerManager();
                }
                return;
            });
        }
        //初始化分组
        private void InitDivideIntoGroups(int maxResult)
        {
            CrossBattleTiming checkTiming = (CrossBattleTiming)(maxResult + 1);
            //分组
            foreach (var group in groupList)
            {
                int groupId = group.Key;
                switch (checkTiming)
                {
                    case CrossBattleTiming.BattleTime1:
                    case CrossBattleTiming.BattleTime2:
                    case CrossBattleTiming.BattleTime3:
                        {
                            foreach (var team in group.Value.List)
                            {
                                int teamId = team.Key;
                                if (teamId > 0)
                                {
                                    CreateTeamDetachment(checkTiming, 1, groupId, team.Value, teamId);
                                }
                            }
                        }
                        break;
                    case CrossBattleTiming.BattleTime4:
                    case CrossBattleTiming.BattleTime5:
                    case CrossBattleTiming.BattleTime6:
                        {
                            int teamId = 0; //决赛组
                            CrossChallengeGroupItem team = group.Value.GetTeam(teamId);
                            CreateTeamDetachment(checkTiming, 1, groupId, team, teamId);
                        }
                        break;
                    default:
                        break;
                }

            }
        }

        public void InitBattleInfo()
        {
            OperateGetCrossChallengeGroupBattleInfo operate = new OperateGetCrossChallengeGroupBattleInfo(groupList);
            server.CrossRedis.Call(operate, ret =>
            {
                foreach (var battleInfo in operate.BattleInfos)
                {
                    CrossChallengeGroupModel group = GetBattleGroup(battleInfo.GroupId);
                    if (group == null) continue;

                    CrossChallengeGroupItem team = group.GetTeam(battleInfo.TeamIdId);
                    if (team == null) continue;

                    foreach (var item in battleInfo.BattleInfo)
                    {
                        team.AddBattleInfo(item.Key, item.Value);
                    }
                }
            });
        }

        //开始执行
        public void RunTimerManager()
        {
            CrossBattleTiming lstTiming = currentTiming;
            currentTiming = CrossChallengeLibrary.CheckNextTiming(currentTiming);
            lastCheckTime = CrossChallengeLibrary.GetNextTime(lstTiming, currentTiming, lastCheckTime);

            double interval = (lastCheckTime - DateTime.Now).TotalMilliseconds;
            if (interval < 0)
            {
                switch (currentTiming)
                {
                    case CrossBattleTiming.ShowTime1:
                    case CrossBattleTiming.ShowTime2:
                    case CrossBattleTiming.ShowTime3:
                    case CrossBattleTiming.ShowTime4:
                    case CrossBattleTiming.ShowTime5:
                    case CrossBattleTiming.ShowTime6:
                        interval = 300000;
                        break;
                    default:
                        interval = 60000;
                        break;
                }
            }
            TaskTimerQuery counterTimer = new CrossBattleTimerQuery(interval);
            Log.Info($"InitTimerManager call task {currentTiming}：{lastCheckTime} after {interval}");
            server.TaskTimerMng.Call(counterTimer, (ret) =>
            {
                TimingRefreshByPlayers();
            });
        }

        //时间点触发
        private void TimingRefreshByPlayers()
        {
            //执行成功，保存时间
            CrossBattleTiming lastTiming = currentTiming;
            int lastTime = Timestamp.GetUnixTimeStampSeconds(lastCheckTime);
            server.CrossRedis.Call(new OperateSetCrossChallengeInfo(HashField_CrossBattle.lastTiming, (int)currentTiming));

            if (currentTiming == CrossBattleTiming.Start)
            {
                FirstStartTime = lastTime;
                server.CrossRedis.Call(new OperateSetCrossChallengeInfo(HashField_CrossBattle.FirstStartTime, FirstStartTime));
                CrossChallengeStart();
            }

            //添加新任务
            RunTimerManager();

            //执行任务
            DoTimingTask(lastTiming);

        }

        //执行事件
        public void DoTimingTask(CrossBattleTiming doTiming)
        {
            server.TrackingLoggerMng.TrackTimerLog(server.MainId, "crosschallenge", doTiming.ToString(), server.Now());
            switch (doTiming)
            {
                case CrossBattleTiming.Start:
                    //跨服战开始殿堂更新，包含整个结果
                    //BackupLastFinalsPlayerRankInfo();
                    CrossChallengeStart();
                    //清理排行榜
                    ClearCrossChallengeRankInfos();
                    break;
                case CrossBattleTiming.FinalsStart:
                    //ClearCrossChallengeRankInfos();
                    //清空上次对阵
                    ClearLastFinalsPlayerRankInfo();
                    //决赛开始锁定排行榜 筛选每个服务器前8
                    LoadFinalsPlayerRankInfo();
                    break;
                case CrossBattleTiming.GuessingTime:
                    //分组对战
                    DivideIntoGroups(doTiming);
                    //开启抽奖
                    GuessingStart(doTiming);
                    break;

                case CrossBattleTiming.BattleTime1:
                    //8个战区同时开始第一场战斗，选出前32
                    BattleStart(doTiming);
                    break;
                case CrossBattleTiming.ShowTime1:
                    //分组对战
                    DivideIntoGroups(doTiming);
                    //通知选手
                    NoticePlayerBattleInfo(doTiming, 0);
                    //发放竞猜奖励
                    SendGuessingResult(doTiming);
                    //开启抽奖
                    GuessingStart(doTiming);
                    break;

                case CrossBattleTiming.BattleTime2:
                    //8个战区同时开始第一场战斗，选出前32
                    BattleStart(doTiming);
                    break;
                case CrossBattleTiming.ShowTime2:
                    //分组对战
                    DivideIntoGroups(doTiming);
                    //通知选手
                    NoticePlayerBattleInfo(doTiming, 0);
                    //发放竞猜奖励
                    SendGuessingResult(doTiming);
                    //开启抽奖
                    GuessingStart(doTiming);
                    break;

                case CrossBattleTiming.BattleTime3:
                    //8个战区同时开始第一场战斗，选出前32
                    BattleStart(doTiming);
                    break;
                case CrossBattleTiming.ShowTime3:
                    //分组对战
                    FinalsDivideIntoGroups(doTiming);
                    //通知选手
                    NoticePlayerBattleInfo(doTiming, 0);
                    //发放竞猜奖励
                    SendGuessingResult(doTiming);
                    //竞猜
                    FinalGuessingStart(doTiming);
                    break;

                case CrossBattleTiming.BattleTime4:
                    //8个战区同时开始第一场战斗，选出前32
                    BattlFinalsStart(doTiming);
                    break;
                case CrossBattleTiming.ShowTime4:
                    //分组对战
                    FinalsDivideIntoGroups(doTiming);
                    //通知选手
                    NoticePlayerBattleInfo(doTiming, 0);
                    //发放竞猜奖励
                    SendGuessingResult(doTiming);
                    //竞猜
                    FinalGuessingStart(doTiming);
                    break;

                case CrossBattleTiming.BattleTime5:
                    //8个战区同时开始第一场战斗，选出前32
                    BattlFinalsStart(doTiming);
                    break;
                case CrossBattleTiming.ShowTime5:
                    //分组对战
                    FinalsDivideIntoGroups(doTiming);
                    //通知选手
                    NoticePlayerBattleInfo(doTiming, 0);
                    //发放竞猜奖励
                    SendGuessingResult(doTiming);
                    //竞猜
                    FinalGuessingStart(doTiming);
                    break;


                case CrossBattleTiming.BattleTime6:
                    //8个战区同时开始第一场战斗，选出前32
                    BattlFinalsStart(doTiming);
                    break;
                case CrossBattleTiming.ShowTime6:
                    //所有服都通知
                    NoticePlayerFirstInfo();
                    break;

                case CrossBattleTiming.PrepareTime1:
                case CrossBattleTiming.PrepareTime2:
                case CrossBattleTiming.PrepareTime3:
                case CrossBattleTiming.PrepareTime4:
                case CrossBattleTiming.PrepareTime5:
                case CrossBattleTiming.PrepareTime6:
                    //通知选手
                    NoticePlayerBattleInfo(doTiming, 1);
                    break;
                case CrossBattleTiming.FinalsReward:
                    SendBattleFinalsReward();
                    break;
                case CrossBattleTiming.End:
                default:
                    break;
            }
        }

        /// <summary>
        /// 第一公告
        /// </summary>
        public void NoticePlayerFirstInfo()
        {
            int timingId = (int)CrossBattleTiming.BattleTime6;

            Dictionary<int, List<int>> serverUidList = new Dictionary<int, List<int>>();
            foreach (var kv in groupList)
            {
                MSG_CorssR_CROSS_CHALLENGE_WIN_FINAL msg = new MSG_CorssR_CROSS_CHALLENGE_WIN_FINAL();
                int uid = 0;
                int mainId = 0;
                string name = string.Empty;
                bool findFirst = false;
                foreach (var team in kv.Value.List)
                {
                    foreach (var index in team.Value.List)
                    {
                        if (index.Value.Uid > 0 && index.Value.Result == timingId)
                        {
                            RedisPlayerInfo info = GetRedisPlayerInfo(index.Value.Uid);
                            if (info != null)
                            {
                                uid = index.Value.Uid;
                                mainId = info.GetIntValue(HFPlayerInfo.MainId);
                                name = info.GetStringValue(HFPlayerInfo.Name);
                                findFirst = true;
                                break;
                            }
                        }
                    }
                    if (findFirst)
                    {
                        break;
                    }
                }
                if (uid > 0)
                {
                    msg.Uid = uid;
                    msg.MainId = mainId;
                    msg.Name = name;
                    List<int> servers = CrossChallengeLibrary.GetGroupServers(kv.Key);
                    foreach (var serverId in servers)
                    {
                        FrontendServer relation = server.RelationManager.GetSinglePointServer(serverId);
                        if (relation != null)
                        {
                            relation.Write(msg);
                        }
                        else
                        {
                            //没有找到玩家，直接算输
                            Log.Warn($"cross battle NoticePlayerFirstInfo not find mainId {serverId} relation.");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 发送邮件和公告
        /// </summary>
        /// <param name="timing"></param>
        /// <param name="addTiming"></param>
        public void NoticePlayerBattleInfo(CrossBattleTiming timing, int addTiming)
        {
            CrossBattleTiming checkTiming = CrossChallengeLibrary.GetCrossBattleTiming(timing);

            CrossChallengeTimingFightModel timingFight = GetCrossBattleTimingFight(checkTiming);
            Dictionary<int, List<int>> serverUidList = new Dictionary<int, List<int>>();
            foreach (var kv in timingFight.List)
            {
                foreach (var team in kv.Value.List)
                {
                    foreach (var index in team.Value.List)
                    {
                        foreach (var item in index.Value)
                        {
                            int uid = item.Uid;
                            RedisPlayerInfo info = GetRedisPlayerInfo(uid);
                            if (info != null)
                            {
                                int mainId = info.GetIntValue(HFPlayerInfo.MainId);
                                AddServerUidList(serverUidList, uid, mainId);
                            }
                        }
                    }
                }
            }

            int timingId = (int)timing;
            foreach (var serverId in serverUidList)
            {
                MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_BATTLE_INFO msg = new MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_BATTLE_INFO();
                msg.TimingId = timingId;
                foreach (var uid in serverId.Value)
                {
                    msg.List.Add(uid);
                }
                //通知
                if (!WriteToRelation(msg, serverId.Key))
                {
                    //没有找到玩家，直接算输
                    Log.Warn($"cross battle NoticePlayerBattleInfo not find mainId {serverId.Key} relation.");
                }
            }
        }


        ///// <summary>
        ///// 小组赛开始
        ///// </summary>
        ///// <param name="timing"></param>
        //public void BattleStart(CrossBattleTiming timing)
        //{
        //    //分组
        //    foreach (var group in groupList)
        //    {
        //        int groupId = group.Key;
        //        foreach (var team in group.Value.List)
        //        {
        //            int teamId = team.Key;
        //            if (teamId > 0)
        //            {
        //                //StartFight(timing, dic, groupId, team.Value, teamId);
        //                StartFight(timing, groupId, team.Value, teamId);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 前8决赛开始
        ///// </summary>
        ///// <param name="timing"></param>
        //public void BattlFinalsStart(CrossBattleTiming timing)
        //{
        //    //分组
        //    foreach (var group in groupList)
        //    {
        //        int groupId = group.Key;
        //        int teamId = 0; //决赛组
        //        CrossChallengeGroupItem team = group.Value.GetTeam(teamId);
        //        if (team != null)
        //        {
        //            //StartFight(timing, dic, groupId, team, teamId);
        //            StartFight(timing, groupId, team, teamId);
        //        }
        //    }
        //}

        //private void StartFight(CrossBattleTiming timing, int groupId, CrossChallengeGroupItem team, int teamId)
        //{
        //    CrossBattleTiming checkTiming = CrossChallengeLibrary.GetCrossBattleTiming(timing);
        //    int timingId = (int)checkTiming;
        //    Dictionary<int, List<CrossChallengePlayer>> fightList = new Dictionary<int, List<CrossChallengePlayer>>();
        //    foreach (var item in team.List)
        //    {
        //        if (item.Value.Uid > 0 && item.Value.Result == timingId)
        //        {
        //            int fightId = CrossChallengeLibrary.GetFightId(checkTiming, item.Value.Index);
        //            if (fightId > 0)
        //            {
        //                AddCheckList(fightList, item.Value, fightId);
        //            }
        //        }
        //    }
        //    foreach (var item in fightList)
        //    {
        //        int fightId = item.Key;
        //        if (item.Value.Count < 2)
        //        {
        //            // 说明参赛人数不足
        //            Log.Warn($"cross battle get challenger info find player {timing} mainId {item.Key} relation.");
        //            continue;
        //        }
        //        CrossChallengePlayer Player1 = item.Value[0];
        //        CrossChallengePlayer Player2 = item.Value[1];
        //        RedisPlayerInfo Player1info = GetRedisPlayerInfo(Player1.Uid);
        //        RedisPlayerInfo Player2info = GetRedisPlayerInfo(Player2.Uid);
        //        if (Player1info == null || Player2info == null)
        //        {
        //            //没有找到玩家，直接算输
        //            Log.Warn($"cross battle get challenger info find player {timing} mainId {item.Key} relation.");
        //            continue;
        //        }

        //        MSG_CorssR_GET_BATTLE_PLAYER msg = new MSG_CorssR_GET_BATTLE_PLAYER();
        //        msg.Player1 = GetPlayerBaseInfoMsg(Player1info, Player1.Index);
        //        msg.Player2 = GetPlayerBaseInfoMsg(Player2info, Player2.Index);
        //        msg.GetType = (int)ChallengeIntoType.CrossFinalsPlayer1;

        //        msg.TimingId = (int)timing;
        //        msg.GroupId = groupId;
        //        msg.TeamId = teamId;
        //        msg.FightId = fightId;
        //        //没有缓存信息，查看玩家是否在线
        //        FrontendServer relation = server.RelationManager.GetSinglePointServer(msg.Player1.MainId);
        //        if (relation != null)
        //        {
        //            //通知玩家发送信息回来
        //            relation.Write(msg, Player1.Uid);
        //        }
        //        else
        //        {
        //            //没有找到玩家，直接算输
        //            Log.Warn("cross battle get challenger info find player {0} mainId {1} relation.", Player2.Uid, msg.Player1.MainId);
        //        }
        //    }
        //}


        private void AddServerUidList(Dictionary<int, List<int>> serverUidList, int uid, int mainId)
        {
            List<int> list;
            if (serverUidList.TryGetValue(mainId, out list))
            {
                list.Add(uid);
            }
            else
            {
                list = new List<int>();
                list.Add(uid);
                serverUidList.Add(mainId, list);
            }
        }


        /// <summary>
        /// 重置排行榜
        /// </summary>
        public void ClearCrossChallengeRankInfos()
        {
            //重置跨服服务器时不希望重置排行榜
            //if (!IsReset)
            {
                //获取赛季排行榜, 通知个个服务器，返回前8信息
                MSG_CorssR_CROSS_CHALLENGE_CLEAR_BATTLE_RANK msg = new MSG_CorssR_CROSS_CHALLENGE_CLEAR_BATTLE_RANK();
                server.RelationManager.Broadcast(msg);
            }
        }

        /// <summary>
        /// 更新跨服战开启时间
        /// </summary>
        public void CrossChallengeStart()
        {
            //获取赛季排行榜, 通知个个服务器，返回前8信息
            MSG_CorssR_CROSS_CHALLENGE_BATTLE_START msg = new MSG_CorssR_CROSS_CHALLENGE_BATTLE_START();
            msg.Time = FirstStartTime;
            server.RelationManager.Broadcast(msg);
        }

        /// <summary>
        /// 清空当前信息
        /// </summary>
        public void ClearLastFinalsPlayerRankInfo()
        {
            var operate = new OperateClearCrossChallengePlayerBaseInfo(playerBaseInfoList.Keys.ToList());
            foreach (var kv in groupList)
            {
                int groupId = kv.Key;
                foreach (var item in kv.Value.List)
                {
                    int team = item.Key;
                    foreach (var info in item.Value.List)
                    {
                        int index = info.Key;

                        operate.Add(groupId, team, index);
                    }
                }
            }

            server.CrossRedis.Call(operate);
        }

        /// <summary>
        /// 通知所有Relation 获取信息
        /// </summary>
        public void LoadFinalsPlayerRankInfo()
        {
            //添加战斗位置信息
            groupList.Clear();
            playerBaseInfoList.Clear();

            //初始化列表
            InitGroupList(true);

            //获取赛季排行榜, 通知个个服务器，返回前8信息
            MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_RANK msg = new MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_RANK();
            server.RelationManager.Broadcast(msg);
        }

        /// <summary>
        /// 当前决赛初始化
        /// </summary>
        /// <param name="isNew"></param>
        private void InitGroupList(bool isNew)
        {
            foreach (var kv in CrossChallengeLibrary.GroupList)
            {
                CrossChallengeGroupModel group;
                if (!groupList.TryGetValue(kv.Key, out group))
                {
                    group = new CrossChallengeGroupModel();
                    groupList[kv.Key] = group;
                }
                foreach (var team in CrossChallengeLibrary.FightGroupList)
                {
                    foreach (var index in CrossChallengeLibrary.FightIndexList)
                    {
                        group.Add(0, team.Key, index.Value.Index, InitResult, 0);

                        if (isNew)
                        {
                            server.CrossRedis.Call(new OperateUpdateCrossChallengeBattleFightInfo(0, kv.Key, team.Key, index.Key, InitResult, 0));
                        }
                    }
                }
                foreach (var index in CrossChallengeLibrary.FightIndexList)
                {
                    group.Add(0, 0, index.Value.Index, InitResult, 0);
                    if (isNew)
                    {
                        server.CrossRedis.Call(new OperateUpdateCrossChallengeBattleFightInfo(0, kv.Key, 0, index.Key, InitResult, 0));
                    }
                }

            }
        }

        /// <summary>
        /// 新增玩家基本信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="rInfo"></param>
        public void AddPlayerBaseInfo(int uid, int groupId, RedisPlayerInfo rInfo)
        {
            playerBaseInfoList[uid] = rInfo;
            server.CrossRedis.Call(new OperateCrossChallengeAddPlayerBaseInfo(uid, rInfo));
        }

        /// <summary>
        /// 添加对战信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="groupId"></param>
        /// <param name="serverId"></param>
        /// <param name="mainId"></param>
        /// <param name="rank"></param>
        public void AddBattleGroupInfo(int uid, int groupId, int serverId, int mainId, int rank)
        {
            CrossChallengeGroupModel group = GetBattleGroup(groupId);
            if (group != null)
            {
                CrossFightGroup fightGroup = CrossChallengeLibrary.GetFightGroup(serverId, rank);
                int index = CrossChallengeLibrary.GetFightIndex(rank);
                if (index >= 0)
                {
                    group.Add(uid, fightGroup.Team, index, InitResult, 0);
                }
                server.CrossRedis.Call(new OperateUpdateCrossChallengeBattleFightInfo(uid, groupId, fightGroup.Team, index, InitResult, 0));

                //通知玩具teamID
                MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID msg = new MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID();
                msg.TeanId = fightGroup.Team;
                msg.Uid = uid;
                WriteByPlayer(msg, uid, mainId);
            }
            else
            {
                Log.WarnLine($"player {uid} add battle group info not find group {groupId} from {mainId}");
            }
        }

        /// <summary>
        /// 对战分组
        /// </summary>
        /// <param name="timing"></param>
        public void DivideIntoGroups(CrossBattleTiming timing)
        {
            CrossBattleTiming checkTiming = CrossChallengeLibrary.GetCrossBattleTiming(timing);
            //分组
            foreach (var group in groupList)
            {
                int groupId = group.Key;
                foreach (var team in group.Value.List)
                {
                    int teamId = team.Key;
                    if (teamId > 0)
                    {
                        CreateTeamDetachment(checkTiming, 1, groupId, team.Value, teamId);
                    }
                }
            }
        }
        /// <summary>
        /// 决赛对战分组
        /// </summary>
        /// <param name="timing"></param>
        public void FinalsDivideIntoGroups(CrossBattleTiming timing)
        {
            CrossBattleTiming checkTiming = CrossChallengeLibrary.GetCrossBattleTiming(timing);
            //分组
            foreach (var group in groupList)
            {
                int groupId = group.Key;
                int teamId = 0; //决赛组
                CrossChallengeGroupItem team = group.Value.GetTeam(teamId);
                CreateTeamDetachment(checkTiming, 1, groupId, team, teamId);
            }
        }
        /// <summary>
        /// 小组分队
        /// </summary>
        /// <param name="timing"></param>
        /// <param name="groupId"></param>
        /// <param name="team"></param>
        /// <param name="teamId"></param>
        private void CreateTeamDetachment(CrossBattleTiming timing, int addT, int groupId, CrossChallengeGroupItem team, int teamId)
        {
            CrossChallengeTeamFightModel teamFight = GetCrossChallengeGroupFight(timing, groupId, teamId);
            //清理数据
            teamFight.Clear();

            int timingId = (int)timing;
            foreach (var item in team.List)
            {
                if (item.Value.Result + addT >= timingId)
                {
                    int fightId = CrossChallengeLibrary.GetFightId(timing, item.Value.Index);
                    if (fightId > 0)
                    {
                        teamFight.AddPlayer(item.Value, fightId);
                    }
                }
            }
        }

        /// <summary>
        /// 小组赛开始
        /// </summary>
        /// <param name="timing"></param>
        public void BattleStart(CrossBattleTiming timing)
        {
            CrossChallengeTimingFightModel timingFight = GetCrossBattleTimingFight(timing);
            //分组
            foreach (var group in timingFight.List)
            {
                int groupId = group.Key;
                foreach (var team in group.Value.List)
                {
                    int teamId = team.Key;
                    if (teamId > 0)
                    {
                        //StartFight(timing, dic, groupId, team.Value, teamId);
                        //StartFight(timing, groupId, team.Value, teamId);
                        PlayerFightStart(timing, groupId, teamId, team.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 前8决赛开始
        /// </summary>
        /// <param name="timing"></param>
        public void BattlFinalsStart(CrossBattleTiming timing)
        {
            CrossChallengeTimingFightModel timingFight = GetCrossBattleTimingFight(timing);
            //分组
            foreach (var group in timingFight.List)
            {
                int groupId = group.Key;
                int teamId = 0; //决赛组
                CrossChallengeTeamFightModel team = group.Value.GetGroupFight(teamId);
                //CrossChallengeGroupItem team = group.Value.GetTeam(teamId);
                if (team != null)
                {
                    //StartFight(timing, dic, groupId, team, teamId);
                    //StartFight(timing, groupId, team, teamId);
                    PlayerFightStart(timing, groupId, teamId, team);
                }
            }
        }

        ///// <summary>
        ///// 开始战斗
        ///// </summary>
        ///// <param name="timing"></param>
        ///// <param name="groupId"></param>
        ///// <param name="team"></param>
        ///// <param name="teamId"></param>
        //private void StartFight(CrossBattleTiming timing, int groupId, CrossChallengeGroupItem team, int teamId)
        //{
        //    CrossChallengeTeamFightModel teamFight = GetCrossChallengeGroupFight(timing, groupId, teamId);
        //    int timingId = (int)timing;
        //    foreach (var item in team.List)
        //    {
        //        if (item.Value.Result + 1 == timingId)
        //        {
        //            int fightId = CrossChallengeLibrary.GetFightId(timing, item.Value.Index);
        //            if (fightId > 0)
        //            {
        //                teamFight.AddPlayer(item.Value, fightId);
        //            }
        //        }
        //    }
        //    PlayerFightStart(timing, groupId, teamId, teamFight);
        //}

        private void PlayerFightStart(CrossBattleTiming timing, int groupId, int teamId, CrossChallengeTeamFightModel teamFight)
        {
            foreach (var item in teamFight.List)
            {
                int fightId = item.Key;
                if (item.Value.Count < 2)
                {
                    // 说明参赛人数不足
                    Log.Warn($"cross challenge get challenger info find player {timing} mainId {item.Key} relation.");
                    if (item.Value.Count > 0)
                    {
                        SetCrossChallengeResult((int)timing, groupId, teamId, fightId, item.Value[0].Index);
                    }
                    continue;
                }
                CrossChallengePlayer Player1 = item.Value[0];
                CrossChallengePlayer Player2 = item.Value[1];
                if (Player1.Uid == 0 && Player2.Uid == 0)
                {
                    SetCrossChallengeResult((int)timing, groupId, teamId, fightId, Player1.Index);
                }
                else if (Player1.Uid == 0)
                {
                    PlayerAndRobotFightStart(timing, groupId, teamId, fightId, Player1, Player2);

                }
                else if (Player2.Uid == 0)
                {
                    PlayerAndRobotFightStart(timing, groupId, teamId, fightId, Player2, Player1);
                }
                else
                {
                    PlayerAndPlayerFightStart(timing, groupId, teamId, fightId, Player1, Player2);
                }
            }
        }

        //private void RobotAndRobotFightStart(CrossBattleTiming timing, int groupId, int teamId, int fightId, CrossChallengePlayer robot1, CrossChallengePlayer robot2)
        //{
        //    //SetCrossChallengeResult((int)timing, groupId, teamId, fightId, Player2.Index);
        //    CrossFinalsRobotInfo rInfo1 = null;
        //    CrossFinalsRobotInfo rInfo2 = null;
        //    if (teamId > 0)
        //    {
        //        rInfo1 = RobotLibrary.GetCrossChallengeFinalsRobotInfo(teamId, robot1.Index);
        //        rInfo2 = RobotLibrary.GetCrossChallengeFinalsRobotInfo(teamId, robot2.Index);
        //    }
        //    else
        //    {
        //        rInfo1 = RobotLibrary.GetCrossChallengeFinalsRobotInfo(robot1.Index, robot1.OldTeam);
        //        rInfo2 = RobotLibrary.GetCrossChallengeFinalsRobotInfo(robot2.Index, robot2.OldTeam);
        //    }
        //    if (rInfo1 == null)
        //    {
        //        //没有找到玩家，直接算输
        //        Log.Warn("cross battle get challenger info find robot team {0} index {1} .", teamId, robot2.Index);
        //        //直接判输
        //        SetCrossChallengeResult((int)timing, groupId, teamId, fightId, robot2.Index, "");
        //        return;
        //    }
        //    if (rInfo2 == null)
        //    {
        //        //没有找到玩家，直接算输
        //        Log.Warn("cross battle get challenger info find robot team {0} index {1} .", teamId, robot1.Index);
        //        //直接判输
        //        SetCrossChallengeResult((int)timing, groupId, teamId, fightId, robot1.Index, "0_0_0");
        //        return;
        //    }

        //    MSG_RCR_RETURN_CROSS_CHALLENGE_PLAYER_INFO msg = new MSG_RCR_RETURN_CROSS_CHALLENGE_PLAYER_INFO();
        //    msg.Player1 = GetBattlePlayerMsg(rInfo1, robot1.Index);
        //    msg.Player2 = GetBattlePlayerMsg(rInfo2, robot2.Index);
        //    msg.GetType = (int)ChallengeIntoType.CrossChallengeFinalsRobot;
        //    msg.TimingId = (int)timing;
        //    msg.GroupId = groupId;
        //    msg.TeamId = teamId;
        //    msg.FightId = fightId;

        //    //没有缓存信息，查看玩家是否在线
        //    FrontendServer relation = server.RelationManager.GetOneServer();
        //    if (relation != null)
        //    {
        //        //通知玩家发送信息回来
        //        relation.Write(msg);
        //    }
        //    else
        //    {
        //        //没有找到玩家，直接算输
        //        Log.Warn($"player 0 send relation msg {msg.GetType()} error : not find mainId 0 relation.");
        //    }
        //}

        private void PlayerAndRobotFightStart(CrossBattleTiming timing, int groupId, int teamId, int fightId, CrossChallengePlayer robot, CrossChallengePlayer player)
        {
            //SetCrossChallengeResult((int)timing, groupId, teamId, fightId, Player2.Index);
            CrossFinalsRobotInfo rInfo = null;
            if (teamId > 0)
            {
                rInfo = RobotLibrary.GetCrossChallengeFinalsRobotInfo(teamId, robot.Index);
            }
            else
            {
                rInfo = RobotLibrary.GetCrossChallengeFinalsRobotInfo(robot.Index, robot.OldTeam);
            }
            if (rInfo == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find robot team {0} index {1} .", teamId, robot.Index);
                //直接判输
                SetCrossChallengeResult((int)timing, groupId, teamId, fightId, player.Index);
                return;
            }
            RedisPlayerInfo Player2info = GetRedisPlayerInfo(player.Uid);
            if (Player2info == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player1 rank {0} .", player.Uid);
                //直接判输
                SetCrossChallengeResult((int)timing, groupId, teamId, fightId, player.Index);
                return;
            }

            MSG_RCR_RETURN_CROSS_CHALLENGE_PLAYER_INFO msg = new MSG_RCR_RETURN_CROSS_CHALLENGE_PLAYER_INFO();
            msg.Player1 = GetBattlePlayerMsg(rInfo, robot.Index);
            msg.Player2 = GetBattlePlayerMsg(Player2info, player.Index);
            msg.GetType = (int)ChallengeIntoType.CrossChallengeFinalsPlayer2;
            msg.TimingId = (int)timing;
            msg.GroupId = groupId;
            msg.TeamId = teamId;
            msg.FightId = fightId;

            int pcUid = msg.Player2.Uid;
            int mainId = msg.Player2.MainId;
            Log.Write($"player 0 ReturnCrossChallengePlayerInfo player 2 {pcUid} mainId {mainId}");
            //没有缓存信息，查看玩家是否在线
            WriteByPlayer(msg, pcUid);
        }

        private void PlayerAndPlayerFightStart(CrossBattleTiming timing, int groupId, int teamId, int fightId, CrossChallengePlayer Player1, CrossChallengePlayer Player2)
        {
            RedisPlayerInfo Player1info = GetRedisPlayerInfo(Player1.Uid);
            if (Player1info == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player1 rank {0} .", Player1.Uid);
                //直接判输
                SetCrossChallengeResult((int)timing, groupId, teamId, fightId, Player1.Index);
                return;
            }
            RedisPlayerInfo Player2info = GetRedisPlayerInfo(Player2.Uid);
            if (Player2info == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player1 rank {0} .", Player2.Uid);
                //直接判输
                SetCrossChallengeResult((int)timing, groupId, teamId, fightId, Player2.Index);
                return;
            }

            MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_PLAYER msg = new MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_PLAYER();
            msg.Player1 = GetPlayerBaseInfoMsg(Player1info, Player1.Index);
            msg.Player2 = GetPlayerBaseInfoMsg(Player2info, Player2.Index);
            msg.GetType = (int)ChallengeIntoType.CrossChallengeFinalsPlayer1;

            msg.TimingId = (int)timing;
            msg.GroupId = groupId;
            msg.TeamId = teamId;
            msg.FightId = fightId;
            //没有缓存信息，查看玩家是否在线
            FrontendServer relation = server.RelationManager.GetSinglePointServer(msg.Player1.MainId);
            if (relation != null)
            {
                //通知玩家发送信息回来
                relation.Write(msg, Player1.Uid);
            }
            else
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player {0} mainId {1} relation.", Player2.Uid, msg.Player1.MainId);
            }
        }

        public void GuessingStart(CrossBattleTiming timing)
        {
            CrossBattleTiming checkTiming = CrossChallengeLibrary.GetCrossBattleTiming(timing);
            CrossChallengeTimingFightModel timingFight = GetCrossBattleTimingFight(checkTiming);
            CrossGuessingGroupModel teamGuessing = GetCrossBattleTimingGuessing(checkTiming);
            //分组
            foreach (var group in timingFight.List)
            {
                int groupId = group.Key;

                CrossGuessingItem guessingItem = teamGuessing.GetGuessingItem(groupId);
                guessingItem.Clear();

                foreach (var team in group.Value.List)
                {
                    int teamId = team.Key;
                    if (teamId > 0)
                    {
                        SetGuessingItem(guessingItem, team.Value, teamId);
                    }
                }

                //通知个个服务器
                SendGuessingInfo(checkTiming, groupId, guessingItem);
            }
        }

        public void FinalGuessingStart(CrossBattleTiming timing)
        {
            CrossBattleTiming checkTiming = CrossChallengeLibrary.GetCrossBattleTiming(timing);
            CrossChallengeTimingFightModel timingFight = GetCrossBattleTimingFight(checkTiming);
            CrossGuessingGroupModel teamGuessing = GetCrossBattleTimingGuessing(checkTiming);
            //分组
            foreach (var group in timingFight.List)
            {
                int groupId = group.Key;
                CrossGuessingItem guessingItem = teamGuessing.GetGuessingItem(groupId);
                guessingItem.Clear();

                int teamId = 0; //决赛组
                CrossChallengeTeamFightModel team = group.Value.GetGroupFight(teamId);
                //CrossChallengeGroupItem team = group.Value.GetTeam(teamId);
                if (team != null)
                {
                    SetGuessingItem(guessingItem, team, teamId);
                }

                SendGuessingInfo(checkTiming, groupId, guessingItem);
            }
        }

        private void SendGuessingInfo(CrossBattleTiming checkTiming, int groupId, CrossGuessingItem guessingItem)
        {
            List<int> serverIds = CrossChallengeLibrary.GetGroupServers(groupId);
            foreach (var serverId in serverIds)
            {
                CrossGuessingFight fight = guessingItem.GetGuessingInfo(serverId);
                if (fight != null)
                {
                    //说明有竞猜
                    MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_INFO msg = new MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_INFO();
                    msg.TimingId = (int)checkTiming;
                    msg.Uid1 = fight.Player1.Uid;
                    msg.Uid2 = fight.Player2.Uid;
                    msg.TeanId = fight.TeamId;
                    WriteToRelation(msg, serverId);
                }
                else
                {
                    //没有竞猜
                    Log.Warn($"cross sever guessing start not find server {serverId} fight info");
                }
                //WriteToRelation();
            }
        }

        private void SetGuessingItem(CrossGuessingItem guessingItem, CrossChallengeTeamFightModel team, int teamId)
        {
            foreach (var item in team.List)
            {
                int fightId = item.Key;
                if (item.Value.Count < 2)
                {
                    // 说明参赛人数不足
                    continue;
                }
                CrossChallengePlayer Player1 = item.Value[0];
                CrossChallengePlayer Player2 = item.Value[1];
                if (Player1.Uid == 0 || Player2.Uid == 0)
                {
                    // 说明参赛人数不足
                    continue;
                }
                RedisPlayerInfo Player1info = GetRedisPlayerInfo(Player1.Uid);
                RedisPlayerInfo Player2info = GetRedisPlayerInfo(Player2.Uid);
                if (Player1info == null || Player2info == null)
                {
                    //说明参赛人数不足
                    continue;
                }

                CrossGuessingPlayer guessingPlayer1 = GetCrossGuessingPlayer(Player1info, Player1.Index);
                CrossGuessingPlayer guessingPlayer2 = GetCrossGuessingPlayer(Player2info, Player2.Index);
                guessingItem.Add(guessingPlayer1, guessingPlayer2, teamId);
            }
        }

        public void SendGuessingResult(CrossBattleTiming timing)
        {
            CrossBattleTiming checkTiming = CrossChallengeLibrary.GetCrossBattleTiming(timing);
            CrossChallengeTimingFightModel timingFight = GetCrossBattleTimingFight(checkTiming);
            //分组
            foreach (var group in timingFight.List)
            {
                List<int> uidList = new List<int>();

                foreach (var team in group.Value.List)
                {
                    foreach (var index in team.Value.List)
                    {
                        foreach (var item in index.Value)
                        {
                            int uid = item.Uid;
                            RedisPlayerInfo info = GetRedisPlayerInfo(uid);
                            if (info != null)
                            {
                                int mainId = info.GetIntValue(HFPlayerInfo.MainId);
                                uidList.Add(uid);
                            }
                        }
                    }
                }
                MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_RESULT msg = new MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_RESULT();
                msg.TimingId = (int)checkTiming - 1;
                msg.UidList.AddRange(uidList);
                //通知个个服务器
                List<int> serverIds = CrossChallengeLibrary.GetGroupServers(group.Key);
                foreach (var serverId in serverIds)
                {
                    //说明有竞猜
                    WriteToRelation(msg, serverId);
                }
            }
        }

        public CrossChallengeTimingFightModel GetCrossBattleTimingFight(CrossBattleTiming timing)
        {
            CrossChallengeTimingFightModel fight;
            if (!timingGroupList.TryGetValue(timing, out fight))
            {
                fight = new CrossChallengeTimingFightModel();
                fight.Timing = timing;
                timingGroupList.Add(timing, fight);
            }
            return fight;
        }

        public CrossGuessingGroupModel GetCrossBattleTimingGuessing(CrossBattleTiming timing)
        {
            CrossGuessingGroupModel fight;
            if (!timingGuessingList.TryGetValue(timing, out fight))
            {
                fight = new CrossGuessingGroupModel();
                timingGuessingList.Add(timing, fight);
            }
            return fight;
        }

        public CrossChallengeTeamFightModel GetCrossChallengeGroupFight(CrossBattleTiming timing, int groupId, int teamId)
        {
            CrossChallengeTimingFightModel fight = GetCrossBattleTimingFight(timing);
            CrossChallengeGroupFightModel group = fight.GetGroupFight(groupId);
            CrossChallengeTeamFightModel team = group.GetGroupFight(teamId);
            return team;
        }

        /// <summary>
        /// 保存结果
        /// </summary>
        /// <param name="timingId"></param>
        /// <param name="groupId"></param>
        /// <param name="teamId"></param>
        /// <param name="fightId"></param>
        /// <param name="winIndexId"></param>
        public void SetCrossChallengeResult(int timingId, int groupId, int teamId, int fightId, int winIndexId)
        {
            CrossChallengeGroupModel group = GetBattleGroup(groupId);
            if (group != null)
            {
                CrossChallengeGroupItem team = group.GetTeam(teamId);
                if (team != null)
                {
                    CrossBattleTiming timing = (CrossBattleTiming)timingId;
                    CrossChallengePlayer result = team.GetPlayerByIndex(winIndexId);
                    if (result != null)
                    {
                        result.Result = timingId;
                        //保存结果
                        server.CrossRedis.Call(new OperateUpdateCrossChallengeBattleFightResult(groupId, teamId, result.Index, result.Result, timing));

                        switch (timing)
                        {
                            case CrossBattleTiming.BattleTime3:
                                {
                                    //小组赛完成
                                    group.Add(result.Uid, 0, teamId, result.Result, result.Index);
                                    //保存结果
                                    server.CrossRedis.Call(new OperateUpdateCrossChallengeBattleFightInfo(result.Uid, groupId, 0, teamId, result.Result, result.Index));
                                }
                                break;
                            default:
                                break;
                        }
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// 保存录像
        /// </summary>
        /// <param name="timingId"></param>
        /// <param name="groupId"></param>
        /// <param name="teamId"></param>
        /// <param name="fightId"></param>
        /// <param name="vedioName"></param>
        public void SetCrossChallengeVideo(int timingId, int groupId, int teamId, int fightId, string vedioName, string battleInfo)
        {
            CrossChallengeGroupModel group = GetBattleGroup(groupId);
            if (group != null)
            {
                CrossChallengeGroupItem team = group.GetTeam(teamId);
                if (team != null)
                {
                    int vedioId = 1;
                    CrossBattleTiming timing = (CrossBattleTiming)timingId;
                    switch (timing)
                    {
                        case CrossBattleTiming.BattleTime1:
                        case CrossBattleTiming.BattleTime4:
                            vedioId = fightId;
                            break;
                        case CrossBattleTiming.BattleTime2:
                        case CrossBattleTiming.BattleTime5:
                            vedioId = fightId + 4;
                            break;
                        case CrossBattleTiming.BattleTime3:
                        case CrossBattleTiming.BattleTime6:
                            vedioId = fightId + 6;
                            break;
                        default:
                            break;
                    }
                    team.AddVideo(vedioId, vedioName);
                    team.AddBattleInfo(vedioId, battleInfo);
                    server.CrossRedis.Call(new OperateUpdateCrossChallengeVedioInfo(groupId, teamId, vedioId, vedioName));
                    server.CrossRedis.Call(new OperateUpdateCrossChallengeBattleInfo(groupId, teamId, vedioId, battleInfo));
                }
            }
        }

        /// <summary>
        /// 获取录像名
        /// </summary>
        /// <param name="mainId"></param>
        /// <param name="uid"></param>
        /// <param name="teamId"></param>
        /// <param name="videoId"></param>
        public void GetCrossChallengeVideo(int mainId, int uid, int teamId, int videoId, int index)
        {
            string videoName = string.Empty;

            int groupId = CrossChallengeLibrary.GetGroupId(mainId);
            CrossChallengeGroupModel group = GetBattleGroup(groupId);
            if (group != null)
            {
                CrossChallengeGroupItem team = group.GetTeam(teamId);
                if (team != null)
                {
                    videoName = team.GetVideoName(videoId);
                    if (string.IsNullOrEmpty(videoName))
                    {
                        OperateGetCrossChallengeVedioInfo operate = new OperateGetCrossChallengeVedioInfo(groupId, teamId);
                        server.CrossRedis.Call(operate, ret =>
                        {
                            if (operate.VedioList.Count > 0)
                            {
                                foreach (var item in operate.VedioList)
                                {
                                    team.AddVideo(item.Key, item.Value);
                                }
                                videoName = team.GetVideoName(videoId);
                            }

                            if (string.IsNullOrEmpty(videoName))
                            {
                                SendVideoInfoMsg(mainId, uid, teamId, videoId, "", index);
                            }
                            else
                            {
                                SendVideoInfoMsg(mainId, uid, teamId, videoId, videoName, index);
                            }
                            return;
                        });
                    }
                    else
                    {
                        SendVideoInfoMsg(mainId, uid, teamId, videoId, videoName, index);
                    }
                }
            }
        }

        /// <summary>
        /// 发送录像名
        /// </summary>
        /// <param name="mainId"></param>
        /// <param name="uid"></param>
        /// <param name="teamId"></param>
        /// <param name="videoId"></param>
        /// <param name="videoName"></param>
        private void SendVideoInfoMsg(int mainId, int uid, int teamId, int videoId, string videoName, int index)
        {
            MSG_CorssR_CROSS_CHALLENGE_GET_CROSS_VIDEO msg = new MSG_CorssR_CROSS_CHALLENGE_GET_CROSS_VIDEO();
            msg.VedioId = videoId;
            msg.TeamId = teamId;
            msg.Index = index;
            msg.VideoName = videoName;
            if (!string.IsNullOrEmpty(videoName))
            {
                string[] videoStrings = videoName.Split('|');
                if (videoStrings.Length > index)
                {
                    msg.VideoName = videoStrings[index];
                }
            }

            //没有缓存信息，查看玩家是否在线
            FrontendServer relation = server.RelationManager.GetSinglePointServer(mainId);
            if (relation != null)
            {
                //通知玩家发送信息回来
                relation.Write(msg, uid);
            }
            else
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get vedio info find player {0} mainId {1} relation.", uid, mainId);
            }
        }

        /// <summary>
        /// 更新阵容信息
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="heros"></param>
        public void UpdateHeroInfo(int uid, List<CrossHeroInfo> heros)
        {
            playerHeroInfoList[uid] = heros;
        }

        /// <summary>
        /// 发送决赛奖励
        /// </summary>
        public void SendBattleFinalsReward()
        {
            Dictionary<int, Dictionary<int, int>> uidRankList = new Dictionary<int, Dictionary<int, int>>();

            //清理排名
            MSG_CorssR_CROSS_CHALLENGE_CLEAR_PLAYER_FINAL clearMsg = new MSG_CorssR_CROSS_CHALLENGE_CLEAR_PLAYER_FINAL();
            server.RelationManager.Broadcast(clearMsg);

            foreach (var kv in groupList)
            {
                int groupId = kv.Key;
                foreach (var item in kv.Value.List)
                {
                    int teamId = item.Key;
                    foreach (var index in item.Value.List)
                    {
                        if (index.Value.Uid > 0)
                        {
                            int uid = index.Value.Uid;
                            CrossBattleTiming timing = (CrossBattleTiming)index.Value.Result;

                            RedisPlayerInfo info = GetRedisPlayerInfo(uid);
                            if (info != null)
                            {
                                int mainId = info.GetIntValue(HFPlayerInfo.MainId);
                                int rank = GetBattleTimingRank(timing);

                                AddRewardRank(uidRankList, uid, mainId, rank);
                            }
                        }
                    }
                }
            }

            foreach (var serverId in uidRankList)
            {
                //通知各个服务器
                FrontendServer relation = server.RelationManager.GetSinglePointServer(serverId.Key);
                if (relation != null)
                {
                    MSG_CorssR_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL msg = new MSG_CorssR_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL();
                    foreach (var item in serverId.Value)
                    {
                        int uid = item.Key;
                        int rank = item.Value;
                        msg.List.Add(uid, rank);
                    }
                    relation.Write(msg);
                }
                else
                {
                    //没有找到玩家，直接算输
                    Log.Warn($"cross battle send reward not find mainId {serverId.Key} relation.");
                }
            }
        }

        public RedisPlayerInfo GetRedisPlayerInfo(int uid)
        {
            RedisPlayerInfo info;
            playerBaseInfoList.TryGetValue(uid, out info);
            return info;
        }

        public CorssR_BattlePlayerMsg GetPlayerBaseInfoMsg(RedisPlayerInfo baseInfo, int index)
        {
            CorssR_BattlePlayerMsg info = new CorssR_BattlePlayerMsg();
            info.Index = index;
            if (baseInfo != null)
            {
                info.Uid = baseInfo.GetIntValue(HFPlayerInfo.Uid);
                info.MainId = baseInfo.GetIntValue(HFPlayerInfo.MainId);
                CorssR_HFPlayerBaseInfoItem item;
                foreach (var kv in baseInfo.DataList)
                {
                    item = new CorssR_HFPlayerBaseInfoItem();
                    item.Key = (int)kv.Key;
                    item.Value = kv.Value.ToString();
                    info.BaseInfo.Add(item);
                }
            }
            return info;
        }

        public RC_BattlePlayerMsg GetBattlePlayerMsg(RedisPlayerInfo baseInfo, int index)
        {
            RC_BattlePlayerMsg info = new RC_BattlePlayerMsg();
            info.Index = index;
            if (baseInfo != null)
            {
                info.Uid = baseInfo.GetIntValue(HFPlayerInfo.Uid);
                info.MainId = baseInfo.GetIntValue(HFPlayerInfo.MainId);
                RC_HFPlayerBaseInfoItem item;
                foreach (var kv in baseInfo.DataList)
                {
                    item = new RC_HFPlayerBaseInfoItem();
                    item.Key = (int)kv.Key;
                    item.Value = kv.Value.ToString();
                    info.BaseInfo.Add(item);
                }
            }
            return info;
        }

        public RC_BattlePlayerMsg GetBattlePlayerMsg(CrossFinalsRobotInfo rInfo, int index)
        {
            RC_BattlePlayerMsg info = new RC_BattlePlayerMsg();
            info.Index = index;
            if (rInfo != null)
            {
                //info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.Uid, uid));
                info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.Name, rInfo.Name));
                info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.Level, rInfo.Level));
                //info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.Sex, Sex));
                info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.Icon, rInfo.Icon));
                info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.IconFrame, rInfo.IconFrame));
                //info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.ShowDIYIcon, ShowDIYIcon));
                info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.HeroId, rInfo.HeroId));
                //info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.GodType, rInfo.GodType));
                //response.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.BattlePower, UpdateCrossPower()));
                info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.CrossLevel, rInfo.CrossLevel));
                info.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.CrossScore, rInfo.CrossStar));

                //伙伴信息
                foreach (var kv in rInfo.HeroQueue)
                {
                    foreach (var hero in kv.Value)
                    {
                        RC_Hero_Info zRInfo = new RC_Hero_Info();
                        zRInfo.Id = hero.Value.HeroId;
                        zRInfo.Level = hero.Value.Level;
                        zRInfo.StepsLevel = hero.Value.StepsLevel;
                        zRInfo.SoulSkillLevel = hero.Value.SoulSkillLevel;
                        zRInfo.GodType = hero.Value.GodType;
                        zRInfo.CrossQueueNum = kv.Key;
                        zRInfo.CrossPositionNum = hero.Key;
                        zRInfo.Power = hero.Value.BattlePower;

                        string[] soulRingInfo = StringSplit.GetArray("|", hero.Value.SoulRings);
                        //魂环
                        foreach (var soulRing in soulRingInfo)
                        {
                            //有魂环
                            try
                            {
                                string[] tempInfo = StringSplit.GetArray(":", soulRing);

                                RC_Hero_SoulRing soulRingMsg = new RC_Hero_SoulRing();
                                soulRingMsg.Pos = int.Parse(tempInfo[0]);
                                soulRingMsg.Level = int.Parse(tempInfo[1]);
                                soulRingMsg.SpecId = int.Parse(tempInfo[2]);
                                soulRingMsg.Year = int.Parse(tempInfo[3]);
                                if (tempInfo.Length == 5)
                                {
                                    soulRingMsg.Element = int.Parse(tempInfo[4]);
                                }

                                zRInfo.SoulRings.Add(soulRingMsg);
                            }
                            catch (Exception e)
                            {
                                //没找到魂环信息
                                Log.WarnLine("get robot hero info fail,can not find SoulRings {0}, {1}.", hero.Value.SoulRings, e);
                            }
                        }
                        string[] soulBoneInfo = StringSplit.GetArray("|", hero.Value.SoulBones);
                        //魂骨
                        foreach (var soulBone in soulBoneInfo)
                        {
                            try
                            {
                                List<int> soulBoneAttr = soulBone.ToList(':');
                                if (soulBoneAttr.Count < 1) continue;

                                RC_Hero_SoulBone soulBoneMsg = new RC_Hero_SoulBone();
                                soulBoneMsg.Id = soulBoneAttr[0];
                                soulBoneAttr.RemoveAt(0);
                                soulBoneMsg.SpecIds.AddRange(soulBoneAttr);

                                zRInfo.SoulBones.Add(soulBoneMsg);
                            }
                            catch (Exception e)
                            {
                                //没找到魂环信息
                                Log.WarnLine("get robot hero info fail,can not find SoulBones {0}, {1}.", hero.Value.SoulBones, e);
                            }
                        }

                        //暗器
                        List<int> weaponInfo = hero.Value.HiddenWeapon.ToList(':');
                        //魂骨
                        if (weaponInfo.Count == 2)
                        {
                            try
                            {
                                zRInfo.HiddenWeapon = new RC_Hero_HiddenWeapon() { Id = weaponInfo[0], Star = weaponInfo[1] };
                            }
                            catch (Exception e)
                            {
                                //没找到魂环信息
                                Log.WarnLine("get robot hero info fail,can not find hidden weapon {0}, {1}.", hero.Value.HiddenWeapon, e);
                            }
                        }

                        zRInfo.Equipments.Add(hero.Value.Equipment.ToList('|'));

                        //属性
                        zRInfo.Natures = GetNature(hero.Value.NatureList);

                        info.Heros.Add(zRInfo);
                    }
                }
            }

            return info;
        }

        private RC_Hero_Nature GetNature(Dictionary<NatureType, long> nature)
        {
            RC_Hero_Nature heroNature = new RC_Hero_Nature();
            foreach (var item in nature)
            {
                RC_Hero_Nature_Item info = new RC_Hero_Nature_Item();
                info.NatureType = (int)item.Key;
                info.Value = item.Value;
                heroNature.List.Add(info);
            }
            return heroNature;
        }

        public RC_HFPlayerBaseInfoItem SetBaseInfoItem(HFPlayerInfo key, object value)
        {
            RC_HFPlayerBaseInfoItem item = new RC_HFPlayerBaseInfoItem();
            item.Key = (int)key;
            item.Value = value.ToString();
            return item;
        }

        public CrossChallengeGroupModel GetBattleGroup(int group)
        {
            CrossChallengeGroupModel item;
            groupList.TryGetValue(group, out item);
            return item;
        }

        public CrossGuessingPlayer GetCrossGuessingPlayer(RedisPlayerInfo baseInfo, int index)
        {
            CrossGuessingPlayer info = new CrossGuessingPlayer();
            if (baseInfo != null)
            {
                info.Uid = baseInfo.GetIntValue(HFPlayerInfo.Uid);
                info.MainId = baseInfo.GetIntValue(HFPlayerInfo.MainId);
                info.Rank = CrossChallengeLibrary.GetFightRank(index);
            }
            return info;
        }

        public MSG_CorssR_SHOW_CROSS_CHALLENGE_FINALS_INFO GetFinalsInfoMsg(int uid, int mainId, int teamId)
        {
            MSG_CorssR_SHOW_CROSS_CHALLENGE_FINALS_INFO msg = new MSG_CorssR_SHOW_CROSS_CHALLENGE_FINALS_INFO();
            msg.TeamId = teamId;

            int groupId = CrossChallengeLibrary.GetGroupId(mainId);
            CrossChallengeGroupModel group = GetBattleGroup(groupId);
            if (group == null)
            {
                Log.WarnLine($"player {uid} GetFinalsInfoMsg error: not find group {groupId} mouel.");
                return null;
            }

            //找到分组信息
            CrossChallengeGroupItem team = group.GetTeam(teamId);
            if (team == null)
            {
                Log.WarnLine($"player {uid} GetFinalsInfoMsg error: not find group {groupId} mouel team {teamId}.");
                return null;
            }

            List<int> defaultWin = new List<int>() {1, 1, 1};
            foreach (var player in team.List)
            {
                RedisPlayerInfo playerInfo = GetRedisPlayerInfo(player.Value.Uid);
                CorssR_BattlePlayerMsg info = GetPlayerBaseInfoMsg(playerInfo, player.Value.Index);
                info.OldTeam = player.Value.OldTeam;
                msg.List.Add(info);

                if (teamId > 0)
                {
                    if (player.Value.Result >= (int)CrossBattleTiming.BattleTime1)
                    {
                        msg.Fight1.Add(player.Value.Index);
                    }
                    if (player.Value.Result >= (int)CrossBattleTiming.BattleTime2)
                    {
                        msg.Fight2.Add(player.Value.Index);
                    }
                    if (player.Value.Result >= (int)CrossBattleTiming.BattleTime3)
                    {
                        msg.Fight3.Add(player.Value.Index);
                    }
                }
                else
                {
                    if (player.Value.Result >= (int)CrossBattleTiming.BattleTime4)
                    {
                        msg.Fight1.Add(player.Value.Index);
                    }
                    if (player.Value.Result >= (int)CrossBattleTiming.BattleTime5)
                    {
                        msg.Fight2.Add(player.Value.Index);
                    }
                    if (player.Value.Result >= (int)CrossBattleTiming.BattleTime6)
                    {
                        msg.Fight3.Add(player.Value.Index);
                    }
                }
            }

            for (int i = 1; i <= 7; i++)
            {
                string info = string.Empty;
                MSG_CorssR_CROSS_CHALLENGE_WIN_INFO winInfo = new MSG_CorssR_CROSS_CHALLENGE_WIN_INFO() { BattleId = i };
                if (team.BattleInfoList.TryGetValue(i, out info))
                {
                    winInfo.BattleInfo.Add(info.ToList('_'));
                }
                else
                {
                    winInfo.BattleInfo.Add(defaultWin);
                }
                msg.BattleInfoList.Add(winInfo);
            }

            return msg;
        }

        public void GetPlayerHeroInfoMsg(int uid, int mainId, int seeUid, int seeMainId)
        {
            if (seeUid > 0 && seeMainId > 0)
            {
                MSG_RCR_CROSS_CHALLENGE_CHALLENGER msg = new MSG_RCR_CROSS_CHALLENGE_CHALLENGER();
                msg.Uid = uid;
                msg.MainId = mainId;

                List<CrossHeroInfo> heroList = GetCrossHeroInfo(uid);
                if (heroList == null)
                {
                    //通知Relation 获取信息
                    GetBattleHeroInfos(uid, mainId, seeUid, seeMainId);
                    return;
                }
                else
                {
                    foreach (var hero in heroList)
                    {
                        msg.Heros.Add(GetPlayerHeroInfoMsg(hero));
                    }
                    msg.Result = (int)ErrorCode.Success;
                }

                FrontendServer relation = server.RelationManager.GetSinglePointServer(seeMainId);
                if (relation != null)
                {
                    //通知玩家发送信息回来
                    relation.Write(msg, seeUid);
                }
                else
                {
                    //没有找到玩家，直接算输
                    Log.Warn("cross battle GetPlayerHeroInfoMsg find player {0} mainId {1} relation.", seeUid, seeMainId);
                }
            }
        }

        //通知 relation 获取 hero info
        private void GetBattleHeroInfos(int uid, int mainId, int seeUid, int seeMainId)
        {
            //foreach (var kv in playerBaseInfoList)
            //{
            //通知zone，获取hero 信息
            MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_HEROS msg = new MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_HEROS();
            msg.Uid = uid;
            msg.MainId = mainId;
            msg.SeeUid = seeUid;
            msg.SeeMainId = seeMainId;
            //没有缓存信息，查看玩家是否在线
            FrontendServer relation = server.RelationManager.GetSinglePointServer(mainId);
            if (relation != null)
            {
                //通知玩家发送信息回来
                relation.Write(msg, msg.Uid);
            }
            else
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player {0} mainId {1} relation.", uid, mainId);
            }
            //}
        }

        public RCR_Show_HeroInfo GetPlayerHeroInfoMsg(CrossHeroInfo baseInfo)
        {
            RCR_Show_HeroInfo info = new RCR_Show_HeroInfo();
            info.Id = baseInfo.Id;
            info.Level = baseInfo.Level;
            info.StepsLevel = baseInfo.StepsLevel;
            info.TitleLevel = baseInfo.TitleLevel;
            info.SoulSkillLevel = baseInfo.SoulSkillLevel;
            info.GodType = baseInfo.GodType;
            info.QueueNum = baseInfo.QueueNum;
            info.PositionNum = baseInfo.PositionNum;
            info.ComboPower = baseInfo.ComboPower;
            info.Power = baseInfo.Power;

            foreach (var item in baseInfo.SoulRings)
            {
                info.SoulRings.Add(GetShowSoulRingMsg(item));
            }

            foreach (var item in baseInfo.SoulBones)
            {
                info.SoulBones.Add(GetShowSoulBoneMsg(item));
            }

            foreach (var item in baseInfo.Equipment)
            {
                info.Equipments.Add(GetShowEquipmentMsg(item));
            }

            info.HiddenWeapon = new RCR_Show_HiddenWeapon()
            {
                Id = baseInfo.HiddenWeapon.Id, 
                Level = baseInfo.HiddenWeapon.Level,
                Star = baseInfo.HiddenWeapon.Star,
                WashList = { baseInfo.HiddenWeapon.WashList }
            };

            return info;
        }

        private RCR_Show_SoulRing GetShowSoulRingMsg(CrossHeroSoulRing rInfo)
        {
            RCR_Show_SoulRing BaseInfo = new RCR_Show_SoulRing();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Year = rInfo.Year;
            BaseInfo.Pos = rInfo.Pos;
            BaseInfo.Element = rInfo.Element;
            return BaseInfo;
        }
        private RCR_Show_SoulBone GetShowSoulBoneMsg(CrossHeroSoulBone rInfo)
        {
            RCR_Show_SoulBone BaseInfo = new RCR_Show_SoulBone();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Prefix = rInfo.Prefix;

            BaseInfo.EquipedHeroId = rInfo.EquipedHeroId;
            BaseInfo.PartType = rInfo.PartType;
            BaseInfo.AnimalType = rInfo.AnimalType;
            BaseInfo.Quality = rInfo.Quality;
            BaseInfo.Prefix = rInfo.Prefix;
            BaseInfo.MainNatureType = rInfo.MainNatureType;
            BaseInfo.MainNatureValue = rInfo.MainNatureValue;
            BaseInfo.AdditionType1 = rInfo.AdditionType1;
            BaseInfo.AdditionType2 = rInfo.AdditionType2;
            BaseInfo.AdditionValue1 = rInfo.AdditionValue1;
            BaseInfo.AdditionValue2 = rInfo.AdditionValue2;
            BaseInfo.AdditionType3 = rInfo.AdditionType3;
            BaseInfo.AdditionValue3 = rInfo.AdditionValue3;
            BaseInfo.SpecIds.AddRange(rInfo.SpecIds);
            BaseInfo.Score = rInfo.Score;

            return BaseInfo;
        }
        private RCR_Show_Equipment GetShowEquipmentMsg(CrossHeroEquipment rInfo)
        {
            RCR_Show_Equipment BaseInfo = new RCR_Show_Equipment();
            BaseInfo.Id = rInfo.Id;
            BaseInfo.Level = rInfo.Level;

            BaseInfo.EquipedHeroId = rInfo.EquipedHeroId;
            BaseInfo.PartType = rInfo.PartType;
            BaseInfo.Score = rInfo.Score;

            BaseInfo.Slot = GetShowEquipmentSlotMsg(rInfo);

            return BaseInfo;
        }

        private RCR_Show_Equipment_Slot GetShowEquipmentSlotMsg(CrossHeroEquipment rInfo)
        {
            RCR_Show_Equipment_Slot BaseInfo = new RCR_Show_Equipment_Slot();
            BaseInfo.JewelTypeId = rInfo.JewelTypeId;

            foreach (var item in rInfo.Injections)
            {
                RCR_Show_Equipment_Injection info = new RCR_Show_Equipment_Injection();
                info.NatureType = item.NatureType;
                info.NatureValue = item.NatureValue;
                info.InjectionSlot = item.InjectionSlot;
                BaseInfo.Injections.Add(info);
            }
            return BaseInfo;
        }

        public List<CrossHeroInfo> GetCrossHeroInfo(int uid)
        {
            List<CrossHeroInfo> item;
            playerHeroInfoList.TryGetValue(uid, out item);
            return item;
        }

        private void AddRewardRank(Dictionary<int, Dictionary<int, int>> uidRankList, int uid, int mainId, int rank)
        {
            Dictionary<int, int> dic;
            if (uidRankList.TryGetValue(mainId, out dic))
            {
                dic[uid] = rank;
            }
            else
            {
                dic = new Dictionary<int, int>();
                dic[uid] = rank;
                uidRankList.Add(mainId, dic);
            }
        }

        private static int GetBattleTimingRank(CrossBattleTiming timing)
        {
            int rank = 0;
            switch (timing)
            {
                case CrossBattleTiming.BattleTime1:
                    rank = 32;
                    break;
                case CrossBattleTiming.BattleTime2:
                    rank = 16;
                    break;
                case CrossBattleTiming.BattleTime3:
                    rank = 8;
                    break;
                case CrossBattleTiming.BattleTime4:
                    rank = 4;
                    break;
                case CrossBattleTiming.BattleTime5:
                    rank = 2;
                    break;
                case CrossBattleTiming.BattleTime6:
                    rank = 1;
                    break;
                case CrossBattleTiming.End:
                case CrossBattleTiming.Start:
                case CrossBattleTiming.FinalsStart:
                default:
                    rank = 64;
                    break;
            }

            return rank;
        }

        private bool WriteToRelation<T>(T msg, int mainId) where T : Google.Protobuf.IMessage
        {
            FrontendServer relation = server.RelationManager.GetSinglePointServer(mainId);
            if (relation != null)
            {
                relation.Write(msg);
                return true;
            }
            else
            {
                //没有找到玩家，直接算输
                return false;
            }
        }

        private void WriteByPlayer<T>(T msg, int uid = 0) where T : Google.Protobuf.IMessage
        {
            RedisPlayerInfo info = GetRedisPlayerInfo(uid);
            if (info != null)
            {
                int mainId = info.GetIntValue(HFPlayerInfo.MainId);
                WriteByPlayer(msg, uid, mainId);
            }
            else
            {
                Log.Warn($"player {uid} send relation msg {msg.GetType()} error : not find uid info.");
            }
        }

        private void WriteByPlayer<T>(T msg, int uid, int mainId) where T : IMessage
        {
            //没有缓存信息，查看玩家是否在线
            FrontendServer relation = server.RelationManager.GetSinglePointServer(mainId);
            if (relation != null)
            {
                //通知玩家发送信息回来
                relation.Write(msg, uid);
            }
            else
            {
                //没有找到玩家，直接算输
                Log.Warn($"player {uid} send relation msg {msg.GetType()} error : not find mainId {mainId} relation.");
            }
        }

        /// <summary>
        /// 聊天喇叭信息
        /// </summary>
        //public void SendChatTrumpetInfo(int mainId, int uid, int itemId, string words, RC_SPEAKER_INFO pcInfo)
        //{
        //    int groupId = CrossChallengeLibrary.GetGroupId(mainId);
        //    if (groupId > 0 && uid > 0)
        //    {
        //        MSG_CrossR_CHAT_TRUMPET msg = new MSG_CrossR_CHAT_TRUMPET();
        //        msg.MainId = mainId;
        //        msg.ItemId = itemId;
        //        msg.Words = words;
        //        msg.PcInfo = GetCRSpeakerInfo(pcInfo);
        //        List<int> servers = CrossChallengeLibrary.GetGroupServers(groupId);
        //        foreach (var serverId in servers)
        //        {
        //            WriteToRelation(msg, serverId);
        //        }
        //    }
        //}

        //private CR_SPEAKER_INFO GetCRSpeakerInfo(RC_SPEAKER_INFO msg)
        //{
        //    CR_SPEAKER_INFO pcInfo = new CR_SPEAKER_INFO();
        //    pcInfo.Uid = msg.Uid;
        //    pcInfo.Name = msg.Name;
        //    pcInfo.Camp = msg.Camp;
        //    pcInfo.Level = msg.Level;
        //    pcInfo.FaceIcon = msg.FaceIcon;
        //    pcInfo.ShowFaceJpg = msg.ShowFaceJpg;
        //    pcInfo.FaceFrame = msg.FaceFrame;
        //    pcInfo.Sex = msg.Sex;
        //    pcInfo.Title = msg.Title;
        //    pcInfo.TeamId = msg.TeamId;
        //    pcInfo.HeroId = msg.HeroId;
        //    pcInfo.GodType = msg.GodType;
        //    pcInfo.ChatFrameId = msg.ChatFrameId;
        //    pcInfo.ArenaLevel = msg.ArenaLevel;
        //    return pcInfo;
        //}
    }
}
