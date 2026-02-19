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
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossServerLib
{

    public class CrossBattleManager
    {
        private CrossServerApi server { get; set; }
        //uid, rank
        public int FirstStartTime { get; set; }
        private DateTime lastCheckTime { get; set; }
        private bool IsReset { get; set; }
        private CrossBattleTiming currentTiming { get; set; }
        private int InitResult = (int)CrossBattleTiming.FinalsStart;

        //uid info
        private Dictionary<int, CrossBattleGroupModel> groupList = new Dictionary<int, CrossBattleGroupModel>();
        private Dictionary<int, RedisPlayerInfo> playerBaseInfoList = new Dictionary<int, RedisPlayerInfo>();
        private Dictionary<int, List<CrossHeroInfo>> playerHeroInfoList = new Dictionary<int, List<CrossHeroInfo>>();

        private Dictionary<CrossBattleTiming, CrossBattleTimingFightModel> timingGroupList = new Dictionary<CrossBattleTiming, CrossBattleTimingFightModel>();
        private Dictionary<CrossBattleTiming, CrossGuessingGroupModel> timingGuessingList = new Dictionary<CrossBattleTiming, CrossGuessingGroupModel>();
        public CrossBattleManager(CrossServerApi server)
        {
            this.server = server;

            TaskTimerQuery counterTimer = new CrossBattleTimerQuery(10000);
            Log.Info($"CrossBattleManager call timing task ：after 10000");
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
        }

        //初始化当前时间点
        private void InitTimerManager()
        {
            OperateGetCrossBattleInfo operate = new OperateGetCrossBattleInfo();
            server.CrossRedis.Call(operate, ret =>
            {
                if (operate.IsGetValue)
                {
                    string last = operate.GetInfo(HashField_CrossBattle.lastTiming);
                    if (!string.IsNullOrEmpty(last))
                    {
                        //lastCheckTime = Timestamp.TimeStampToDateTime(int.Parse(last));
                        //currentTiming = CrossBattleLibrary.CheckCurrentTiming(lastCheckTime);
                        currentTiming = (CrossBattleTiming)int.Parse(last);
                        string first = operate.GetInfo(HashField_CrossBattle.FirstStartTime);
                        if (!string.IsNullOrEmpty(first))
                        {
                            FirstStartTime = int.Parse(first);
                            lastCheckTime = CrossBattleLibrary.GetNextTime(CrossBattleTiming.Start, currentTiming, Timestamp.TimeStampToDateTime(FirstStartTime));
                            CrossBattleStart();
                        }
                        else
                        {
                            //初始化从开始时间初始化
                            InitCrossBattleBaseTime();
                        }
                    }
                    else
                    {
                        //初始化从开始时间初始化
                        InitCrossBattleBaseTime();
                    }
                }
                else
                {
                    InitCrossBattleBaseTime();
                }

                RunTimerManager();
            });
        }

        //初始化开始时间
        private void InitCrossBattleBaseTime()
        {
            //lastCheckTime = DateTime.Now;
            //currentTiming = CrossBattleLibrary.CheckCurrentTiming(lastCheckTime);
            //currentTiming = CrossBattleLibrary.CheckCurrentTiming(lastCheckTime);
            currentTiming = CrossBattleTiming.Start;
            lastCheckTime = CrossBattleLibrary.GetBeforeTime(currentTiming, DateTime.Now);
            int lastTime = Timestamp.GetUnixTimeStampSeconds(lastCheckTime);
            server.CrossRedis.Call(new OperateSetCrossBattleInfo(HashField_CrossBattle.lastTiming, (int)currentTiming));
            FirstStartTime = lastTime;
            server.CrossRedis.Call(new OperateSetCrossBattleInfo(HashField_CrossBattle.FirstStartTime, FirstStartTime));
            IsReset = true;
            CrossBattleStart();
        }

        //加载玩家基础信息
        private void InitPlayerBaseInfo(List<int> uidDic)
        {
            OperateGetCrossBattlePlayerBaseInfo operate = new OperateGetCrossBattlePlayerBaseInfo(uidDic);
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
            OperateGetCrossBattleGroupFightInfo operate = new OperateGetCrossBattleGroupFightInfo(groupList);
            server.CrossRedis.Call(operate, ret =>
            {
                //Dictionary<int, List<int>> uidDic = new Dictionary<int, List<int>>();
                List<int> list = new List<int>();
                int maxResult = (int)CrossBattleTiming.FinalsStart;
                foreach (var item in operate.InfoLsit)
                {
                    int groupId = item.GetIntValue(HFCrossBattleGroup.Group);
                    CrossBattleGroupModel group = GetBattleGroup(groupId);
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
                            //if (uidDic.TryGetValue(groupId, out list))
                            //{
                            list.Add(uid);
                            //}
                            //else
                            //{
                            //    list = new List<int>();
                            //    list.Add(uid);
                            //    uidDic.Add(groupId, list);
                            //}
                        }
                        //CrossBattleGroupItem teamItem = group.GetTeam(team);
                        //if (team != 0)
                        //{
                        //    InitBattleResult(item.GetIntValue(HFCrossBattleGroup.Battle1), teamItem, CrossBattleTiming.BattleTime1, uid, index);

                        //    InitBattleResult(item.GetIntValue(HFCrossBattleGroup.Battle2), teamItem, CrossBattleTiming.BattleTime2, uid, index);

                        //    InitBattleResult(item.GetIntValue(HFCrossBattleGroup.Battle3), teamItem, CrossBattleTiming.BattleTime3, uid, index);
                        //}
                        //else
                        //{
                        //    InitBattleResult(item.GetIntValue(HFCrossBattleGroup.Battle4), teamItem, CrossBattleTiming.BattleTime4, uid, index);

                        //    InitBattleResult(item.GetIntValue(HFCrossBattleGroup.Battle5), teamItem, CrossBattleTiming.BattleTime5, uid, index);

                        //    InitBattleResult(item.GetIntValue(HFCrossBattleGroup.Battle6), teamItem, CrossBattleTiming.BattleTime6, uid, index);
                        //}
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
                            CrossBattleGroupItem team = group.Value.GetTeam(teamId);
                            CreateTeamDetachment(checkTiming, 1, groupId, team, teamId);
                        }
                        break;
                    default:
                        break;
                }

            }
        }

        //开始执行
        public void RunTimerManager()
        {
            CrossBattleTiming lstTiming = currentTiming;
            currentTiming = CrossBattleLibrary.CheckNextTiming(currentTiming);
            lastCheckTime = CrossBattleLibrary.GetNextTime(lstTiming, currentTiming, lastCheckTime);

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
            server.CrossRedis.Call(new OperateSetCrossBattleInfo(HashField_CrossBattle.lastTiming, (int)currentTiming));

            if (currentTiming == CrossBattleTiming.Start)
            {
                FirstStartTime = lastTime;
                server.CrossRedis.Call(new OperateSetCrossBattleInfo(HashField_CrossBattle.FirstStartTime, FirstStartTime));
                CrossBattleStart();
            }

            //添加新任务
            RunTimerManager();

            //执行任务
            DoTimingTask(lastTiming);

        }

        //执行事件
        public void DoTimingTask(CrossBattleTiming doTiming)
        {
            server.TrackingLoggerMng.TrackTimerLog(server.MainId, "cross", doTiming.ToString(), server.Now());
            switch (doTiming)
            {
                case CrossBattleTiming.Start:
                    //跨服战开始殿堂更新，包含整个结果
                    //BackupLastFinalsPlayerRankInfo();
                    CrossBattleStart();
                    //清理排行榜
                    ClearCrossBattleRankInfos();
                    break;
                case CrossBattleTiming.FinalsStart:
                    //ClearCrossBattleRankInfos();
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
                MSG_CorssR_CROSS_BATTLE_WIN_FINAL msg = new MSG_CorssR_CROSS_BATTLE_WIN_FINAL();
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
                    List<int> servers = CrossBattleLibrary.GetGroupServers(kv.Key);
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
            CrossBattleTiming checkTiming = CrossBattleLibrary.GetCrossBattleTiming(timing);

            CrossBattleTimingFightModel timingFight = GetCrossBattleTimingFight(checkTiming);
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
                MSG_CorssR_NOTICE_PLAYER_BATTLE_INFO msg = new MSG_CorssR_NOTICE_PLAYER_BATTLE_INFO();
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
        //        CrossBattleGroupItem team = group.Value.GetTeam(teamId);
        //        if (team != null)
        //        {
        //            //StartFight(timing, dic, groupId, team, teamId);
        //            StartFight(timing, groupId, team, teamId);
        //        }
        //    }
        //}

        //private void StartFight(CrossBattleTiming timing, int groupId, CrossBattleGroupItem team, int teamId)
        //{
        //    CrossBattleTiming checkTiming = CrossBattleLibrary.GetCrossBattleTiming(timing);
        //    int timingId = (int)checkTiming;
        //    Dictionary<int, List<CrossBattlePlayer>> fightList = new Dictionary<int, List<CrossBattlePlayer>>();
        //    foreach (var item in team.List)
        //    {
        //        if (item.Value.Uid > 0 && item.Value.Result == timingId)
        //        {
        //            int fightId = CrossBattleLibrary.GetFightId(checkTiming, item.Value.Index);
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
        //        CrossBattlePlayer Player1 = item.Value[0];
        //        CrossBattlePlayer Player2 = item.Value[1];
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
        public void ClearCrossBattleRankInfos()
        {
            //重置跨服服务器时不希望重置排行榜
            //if (!IsReset)
            {
                //获取赛季排行榜, 通知个个服务器，返回前8信息
                MSG_CorssR_CLEAR_BATTLE_RANK msg = new MSG_CorssR_CLEAR_BATTLE_RANK();
                server.RelationManager.Broadcast(msg);
            }
        }

        /// <summary>
        /// 更新跨服战开启时间
        /// </summary>
        public void CrossBattleStart()
        {
            //获取赛季排行榜, 通知个个服务器，返回前8信息
            MSG_CorssR_BATTLE_START msg = new MSG_CorssR_BATTLE_START();
            msg.Time = FirstStartTime;
            server.RelationManager.Broadcast(msg);
        }

        /// <summary>
        /// 清空当前信息
        /// </summary>
        public void ClearLastFinalsPlayerRankInfo()
        {
            OperateClearPlayerBaseInfo operate = new OperateClearPlayerBaseInfo(playerBaseInfoList.Keys.ToList());
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
            MSG_CorssR_GET_BATTLE_RANK msg = new MSG_CorssR_GET_BATTLE_RANK();
            server.RelationManager.Broadcast(msg);
        }

        /// <summary>
        /// 当前决赛初始化
        /// </summary>
        /// <param name="isNew"></param>
        private void InitGroupList(bool isNew)
        {
            foreach (var kv in CrossBattleLibrary.GroupList)
            {
                CrossBattleGroupModel group;
                if (!groupList.TryGetValue(kv.Key, out group))
                {
                    group = new CrossBattleGroupModel();
                    groupList[kv.Key] = group;
                }
                foreach (var team in CrossBattleLibrary.FightGroupList)
                {
                    foreach (var index in CrossBattleLibrary.FightIndexList)
                    {
                        group.Add(0, team.Key, index.Value.Index, InitResult, 0);

                        if (isNew)
                        {
                            server.CrossRedis.Call(new OperateUpdateBattleFightInfo(0, kv.Key, team.Key, index.Key, InitResult, 0));
                        }
                    }
                }
                foreach (var index in CrossBattleLibrary.FightIndexList)
                {
                    group.Add(0, 0, index.Value.Index, InitResult, 0);
                    if (isNew)
                    {
                        server.CrossRedis.Call(new OperateUpdateBattleFightInfo(0, kv.Key, 0, index.Key, InitResult, 0));
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
            server.CrossRedis.Call(new OperateAddPlayerBaseInfo(uid, rInfo));
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
            CrossBattleGroupModel group = GetBattleGroup(groupId);
            if (group != null)
            {
                CrossFightGroup fightGroup = CrossBattleLibrary.GetFightGroup(serverId, rank);
                int index = CrossBattleLibrary.GetFightIndex(rank);
                if (index >= 0)
                {
                    group.Add(uid, fightGroup.Team, index, InitResult, 0);
                }
                server.CrossRedis.Call(new OperateUpdateBattleFightInfo(uid, groupId, fightGroup.Team, index, InitResult, 0));

                //通知玩具teamID
                MSG_CorssR_NOTICE_PLAYER_TEAM_ID msg = new MSG_CorssR_NOTICE_PLAYER_TEAM_ID();
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
            CrossBattleTiming checkTiming = CrossBattleLibrary.GetCrossBattleTiming(timing);
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
            CrossBattleTiming checkTiming = CrossBattleLibrary.GetCrossBattleTiming(timing);
            //分组
            foreach (var group in groupList)
            {
                int groupId = group.Key;
                int teamId = 0; //决赛组
                CrossBattleGroupItem team = group.Value.GetTeam(teamId);
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
        private void CreateTeamDetachment(CrossBattleTiming timing, int addT, int groupId, CrossBattleGroupItem team, int teamId)
        {
            CrossBattleTeamFightModel teamFight = GetCrossBattleGroupFight(timing, groupId, teamId);
            //清理数据
            teamFight.Clear();

            int timingId = (int)timing;
            foreach (var item in team.List)
            {
                if (item.Value.Result + addT >= timingId)
                {
                    int fightId = CrossBattleLibrary.GetFightId(timing, item.Value.Index);
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
            CrossBattleTimingFightModel timingFight = GetCrossBattleTimingFight(timing);
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
            CrossBattleTimingFightModel timingFight = GetCrossBattleTimingFight(timing);
            //分组
            foreach (var group in timingFight.List)
            {
                int groupId = group.Key;
                int teamId = 0; //决赛组
                CrossBattleTeamFightModel team = group.Value.GetGroupFight(teamId);
                //CrossBattleGroupItem team = group.Value.GetTeam(teamId);
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
        //private void StartFight(CrossBattleTiming timing, int groupId, CrossBattleGroupItem team, int teamId)
        //{
        //    CrossBattleTeamFightModel teamFight = GetCrossBattleGroupFight(timing, groupId, teamId);
        //    int timingId = (int)timing;
        //    foreach (var item in team.List)
        //    {
        //        if (item.Value.Result + 1 == timingId)
        //        {
        //            int fightId = CrossBattleLibrary.GetFightId(timing, item.Value.Index);
        //            if (fightId > 0)
        //            {
        //                teamFight.AddPlayer(item.Value, fightId);
        //            }
        //        }
        //    }
        //    PlayerFightStart(timing, groupId, teamId, teamFight);
        //}

        private void PlayerFightStart(CrossBattleTiming timing, int groupId, int teamId, CrossBattleTeamFightModel teamFight)
        {
            foreach (var item in teamFight.List)
            {
                int fightId = item.Key;
                if (item.Value.Count < 2)
                {
                    // 说明参赛人数不足
                    Log.Warn($"cross battle get challenger info find player {timing} mainId {item.Key} relation.");
                    if (item.Value.Count > 0)
                    {
                        SetCrossBattleResult((int)timing, groupId, teamId, fightId, item.Value[0].Index);
                    }
                    continue;
                }
                CrossBattlePlayer Player1 = item.Value[0];
                CrossBattlePlayer Player2 = item.Value[1];
                if (Player1.Uid == 0 && Player2.Uid == 0)
                {
                    SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player1.Index);
                    //RobotAndRobotFightStart(timing, groupId, teamId, fightId, Player1, Player2);
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

        private void RobotAndRobotFightStart(CrossBattleTiming timing, int groupId, int teamId, int fightId, CrossBattlePlayer robot1, CrossBattlePlayer robot2)
        {
            //SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player2.Index);
            CrossFinalsRobotInfo rInfo1 = null;
            CrossFinalsRobotInfo rInfo2 = null;
            if (teamId > 0)
            {
                rInfo1 = RobotLibrary.GetCrossFinalsRobotInfo(teamId, robot1.Index);
                rInfo2 = RobotLibrary.GetCrossFinalsRobotInfo(teamId, robot2.Index);
            }
            else
            {
                rInfo1 = RobotLibrary.GetCrossFinalsRobotInfo(robot1.Index, robot1.OldTeam);
                rInfo2 = RobotLibrary.GetCrossFinalsRobotInfo(robot2.Index, robot2.OldTeam);
            }
            if (rInfo1 == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find robot team {0} index {1} .", teamId, robot2.Index);
                //直接判输
                SetCrossBattleResult((int)timing, groupId, teamId, fightId, robot2.Index);
                return;
            }
            if (rInfo2 == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find robot team {0} index {1} .", teamId, robot1.Index);
                //直接判输
                SetCrossBattleResult((int)timing, groupId, teamId, fightId, robot1.Index);
                return;
            }

            MSG_RCR_RETURN_BATTLE_PLAYER_INFO msg = new MSG_RCR_RETURN_BATTLE_PLAYER_INFO();
            msg.Player1 = GetBattlePlayerMsg(rInfo1, robot1.Index);
            msg.Player2 = GetBattlePlayerMsg(rInfo2, robot2.Index);
            msg.GetType = (int)ChallengeIntoType.CrossFinalsRobot;
            msg.TimingId = (int)timing;
            msg.GroupId = groupId;
            msg.TeamId = teamId;
            msg.FightId = fightId;

            //没有缓存信息，查看玩家是否在线
            FrontendServer relation = server.RelationManager.GetOneServer();
            if (relation != null)
            {
                //通知玩家发送信息回来
                relation.Write(msg);
            }
            else
            {
                //没有找到玩家，直接算输
                Log.Warn($"player 0 send relation msg {msg.GetType()} error : not find mainId 0 relation.");
            }
        }

        private void PlayerAndRobotFightStart(CrossBattleTiming timing, int groupId, int teamId, int fightId, CrossBattlePlayer robot, CrossBattlePlayer player)
        {
            //SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player2.Index);
            CrossFinalsRobotInfo rInfo = null;
            if (teamId > 0)
            {
                rInfo = RobotLibrary.GetCrossFinalsRobotInfo(teamId, robot.Index);
            }
            else
            {
                rInfo = RobotLibrary.GetCrossFinalsRobotInfo(robot.Index, robot.OldTeam);
            }
            if (rInfo == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find robot team {0} index {1} .", teamId, robot.Index);
                //直接判输
                SetCrossBattleResult((int)timing, groupId, teamId, fightId, player.Index);
                return;
            }
            RedisPlayerInfo Player2info = GetRedisPlayerInfo(player.Uid);
            if (Player2info == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player1 rank {0} .", player.Uid);
                //直接判输
                SetCrossBattleResult((int)timing, groupId, teamId, fightId, player.Index);
                return;
            }

            MSG_RCR_RETURN_BATTLE_PLAYER_INFO msg = new MSG_RCR_RETURN_BATTLE_PLAYER_INFO();
            msg.Player1 = GetBattlePlayerMsg(rInfo, robot.Index);
            msg.Player2 = GetBattlePlayerMsg(Player2info, player.Index);
            msg.GetType = (int)ChallengeIntoType.CrossFinalsPlayer2;
            msg.TimingId = (int)timing;
            msg.GroupId = groupId;
            msg.TeamId = teamId;
            msg.FightId = fightId;

            int pcUid = msg.Player2.Uid;
            int mainId = msg.Player2.MainId;
            Log.Write($"player 0 ReturnCrossBattlePlayerInfo player 2 {pcUid} mainId {mainId}");
            //没有缓存信息，查看玩家是否在线
            WriteByPlayer(msg, pcUid);
        }

        private void PlayerAndPlayerFightStart(CrossBattleTiming timing, int groupId, int teamId, int fightId, CrossBattlePlayer Player1, CrossBattlePlayer Player2)
        {
            RedisPlayerInfo Player1info = GetRedisPlayerInfo(Player1.Uid);
            if (Player1info == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player1 rank {0} .", Player1.Uid);
                //直接判输
                SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player1.Index);
                return;
            }
            RedisPlayerInfo Player2info = GetRedisPlayerInfo(Player2.Uid);
            if (Player2info == null)
            {
                //没有找到玩家，直接算输
                Log.Warn("cross battle get challenger info find player1 rank {0} .", Player2.Uid);
                //直接判输
                SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player2.Index);
                return;
            }

            MSG_CorssR_GET_BATTLE_PLAYER msg = new MSG_CorssR_GET_BATTLE_PLAYER();
            msg.Player1 = GetPlayerBaseInfoMsg(Player1info, Player1.Index);
            msg.Player2 = GetPlayerBaseInfoMsg(Player2info, Player2.Index);
            msg.GetType = (int)ChallengeIntoType.CrossFinalsPlayer1;

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
            CrossBattleTiming checkTiming = CrossBattleLibrary.GetCrossBattleTiming(timing);
            CrossBattleTimingFightModel timingFight = GetCrossBattleTimingFight(checkTiming);
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
            CrossBattleTiming checkTiming = CrossBattleLibrary.GetCrossBattleTiming(timing);
            CrossBattleTimingFightModel timingFight = GetCrossBattleTimingFight(checkTiming);
            CrossGuessingGroupModel teamGuessing = GetCrossBattleTimingGuessing(checkTiming);
            //分组
            foreach (var group in timingFight.List)
            {
                int groupId = group.Key;
                CrossGuessingItem guessingItem = teamGuessing.GetGuessingItem(groupId);
                guessingItem.Clear();

                int teamId = 0; //决赛组
                CrossBattleTeamFightModel team = group.Value.GetGroupFight(teamId);
                //CrossBattleGroupItem team = group.Value.GetTeam(teamId);
                if (team != null)
                {
                    SetGuessingItem(guessingItem, team, teamId);
                }

                SendGuessingInfo(checkTiming, groupId, guessingItem);
            }
        }

        private void SendGuessingInfo(CrossBattleTiming checkTiming, int groupId, CrossGuessingItem guessingItem)
        {
            List<int> serverIds = CrossBattleLibrary.GetGroupServers(groupId);
            foreach (var serverId in serverIds)
            {
                CrossGuessingFight fight = guessingItem.GetGuessingInfo(serverId);
                if (fight != null)
                {
                    //说明有竞猜
                    MSG_CorssR_NOTICE_CROSS_GUESSING_INFO msg = new MSG_CorssR_NOTICE_CROSS_GUESSING_INFO();
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

        private void SetGuessingItem(CrossGuessingItem guessingItem, CrossBattleTeamFightModel team, int teamId)
        {
            foreach (var item in team.List)
            {
                int fightId = item.Key;
                if (item.Value.Count < 2)
                {
                    // 说明参赛人数不足
                    continue;
                }
                CrossBattlePlayer Player1 = item.Value[0];
                CrossBattlePlayer Player2 = item.Value[1];
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
            CrossBattleTiming checkTiming = CrossBattleLibrary.GetCrossBattleTiming(timing);
            CrossBattleTimingFightModel timingFight = GetCrossBattleTimingFight(checkTiming);
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
                MSG_CorssR_NOTICE_CROSS_GUESSING_RESULT msg = new MSG_CorssR_NOTICE_CROSS_GUESSING_RESULT();
                msg.TimingId = (int)checkTiming - 1;
                msg.UidList.AddRange(uidList);
                //通知个个服务器
                List<int> serverIds = CrossBattleLibrary.GetGroupServers(group.Key);
                foreach (var serverId in serverIds)
                {
                    //说明有竞猜
                    WriteToRelation(msg, serverId);
                }
            }
        }

        public CrossBattleTimingFightModel GetCrossBattleTimingFight(CrossBattleTiming timing)
        {
            CrossBattleTimingFightModel fight;
            if (!timingGroupList.TryGetValue(timing, out fight))
            {
                fight = new CrossBattleTimingFightModel();
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

        public CrossBattleTeamFightModel GetCrossBattleGroupFight(CrossBattleTiming timing, int groupId, int teamId)
        {
            CrossBattleTimingFightModel fight = GetCrossBattleTimingFight(timing);
            CrossBattleGroupFightModel group = fight.GetGroupFight(groupId);
            CrossBattleTeamFightModel team = group.GetGroupFight(teamId);
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
        public void SetCrossBattleResult(int timingId, int groupId, int teamId, int fightId, int winIndexId)
        {
            CrossBattleGroupModel group = GetBattleGroup(groupId);
            if (group != null)
            {
                CrossBattleGroupItem team = group.GetTeam(teamId);
                if (team != null)
                {
                    //CrossBattlePlayer info = team.GetPlayer(winUid);
                    //if (info != null)
                    //{
                    CrossBattleTiming timing = (CrossBattleTiming)timingId;
                    CrossBattlePlayer result = team.GetPlayerByIndex(winIndexId);
                    //CrossBattlePlayer result = team.GetResult(timing, fightId, winUid);
                    if (result != null)
                    {
                        result.Result = timingId;
                        //保存结果
                        server.CrossRedis.Call(new OperateUpdateBattleFightResult(groupId, teamId, result.Index, result.Result, timing));

                        switch (timing)
                        {
                            case CrossBattleTiming.BattleTime3:
                                {
                                    //小组赛完成
                                    group.Add(result.Uid, 0, teamId, result.Result, result.Index);
                                    //保存结果
                                    server.CrossRedis.Call(new OperateUpdateBattleFightInfo(result.Uid, groupId, 0, teamId, result.Result, result.Index));
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
        public void SetCrossBattleVedio(int timingId, int groupId, int teamId, int fightId, string vedioName)
        {
            CrossBattleGroupModel group = GetBattleGroup(groupId);
            if (group != null)
            {
                CrossBattleGroupItem team = group.GetTeam(teamId);
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
                    server.CrossRedis.Call(new OperateUpdateBattleVedioInfo(groupId, teamId, vedioId, vedioName));
                }
            }
        }

        /// <summary>
        /// 获取录像名
        /// </summary>
        /// <param name="mainId"></param>
        /// <param name="uid"></param>
        /// <param name="teamId"></param>
        /// <param name="vedioId"></param>
        public void GetCrossBattleVedio(int mainId, int uid, int teamId, int vedioId)
        {
            string vedioName = string.Empty;

            int groupId = CrossBattleLibrary.GetGroupId(mainId);
            CrossBattleGroupModel group = GetBattleGroup(groupId);
            if (group != null)
            {
                CrossBattleGroupItem team = group.GetTeam(teamId);
                if (team != null)
                {
                    vedioName = team.GetVideoName(vedioId);
                    if (string.IsNullOrEmpty(vedioName))
                    {
                        OperateGetBattleVedioInfo operate = new OperateGetBattleVedioInfo(groupId, teamId);
                        server.CrossRedis.Call(operate, ret =>
                        {
                            if (operate.VedioList.Count > 0)
                            {
                                foreach (var item in operate.VedioList)
                                {
                                    team.AddVideo(item.Key, item.Value);
                                }
                                vedioName = team.GetVideoName(vedioId);
                            }
                            if (string.IsNullOrEmpty(vedioName))
                            {
                                SendVedioInfoMsg(mainId, uid, teamId, vedioId, "");
                            }
                            else
                            {
                                SendVedioInfoMsg(mainId, uid, teamId, vedioId, vedioName);
                            }
                            return;
                        });
                    }
                    else
                    {
                        SendVedioInfoMsg(mainId, uid, teamId, vedioId, vedioName);
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
        /// <param name="vedioId"></param>
        /// <param name="vedioName"></param>
        private void SendVedioInfoMsg(int mainId, int uid, int teamId, int vedioId, string vedioName)
        {
            MSG_CorssR_GET_CROSS_VIDEO msg = new MSG_CorssR_GET_CROSS_VIDEO();
            msg.VedioId = vedioId;
            msg.TeamId = teamId;
            msg.VideoName = vedioName;
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
            MSG_CorssR_CLEAR_PLAYER_FINAL clearMsg = new MSG_CorssR_CLEAR_PLAYER_FINAL();
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
                    MSG_CorssR_UPDATE_PLAYER_FINAL msg = new MSG_CorssR_UPDATE_PLAYER_FINAL();
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

        public CrossBattleGroupModel GetBattleGroup(int group)
        {
            CrossBattleGroupModel item;
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
                info.Rank = CrossBattleLibrary.GetFightRank(index);
            }
            return info;
        }

        public MSG_CorssR_SHOW_CROSS_BATTLE_FINALS_INFO GetFinalsInfoMsg(int uid, int mainId, int teamId)
        {
            MSG_CorssR_SHOW_CROSS_BATTLE_FINALS_INFO msg = new MSG_CorssR_SHOW_CROSS_BATTLE_FINALS_INFO();
            msg.TeamId = teamId;

            int groupId = CrossBattleLibrary.GetGroupId(mainId);
            CrossBattleGroupModel group = GetBattleGroup(groupId);
            if (group == null)
            {
                Log.WarnLine($"player {uid} GetFinalsInfoMsg error: not find group {groupId} mouel.");
                return null;
            }

            //找到分组信息
            CrossBattleGroupItem team = group.GetTeam(teamId);
            if (team == null)
            {
                Log.WarnLine($"player {uid} GetFinalsInfoMsg error: not find group {groupId} mouel team {teamId}.");
                return null;
            }

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

            //foreach (var fight in team.Fight1)
            //{
            //    foreach (var player in fight.Value.PlayerList)
            //    {
            //        if (player.Result == 1)
            //        {
            //            msg.Fight1.Add(player.Index);
            //            break;
            //        }
            //    }
            //}
            //foreach (var fight in team.Fight2)
            //{
            //    foreach (var player in fight.Value.PlayerList)
            //    {
            //        if (player.Result == 1)
            //        {
            //            msg.Fight2.Add(player.Index);
            //            break;
            //        }
            //    }
            //}
            //foreach (var fight in team.Fight3)
            //{
            //    foreach (var player in fight.Value.PlayerList)
            //    {
            //        if (player.Result == 1)
            //        {
            //            msg.Fight3.Add(player.Index);
            //            break;
            //        }
            //    }
            //}

            return msg;
        }

        public void GetPlayerHeroInfoMsg(int uid, int mainId, int seeUid, int seeMainId)
        {
            if (seeUid > 0 && seeMainId > 0)
            {
                MSG_RCR_CROSS_BATTLE_CHALLENGER msg = new MSG_RCR_CROSS_BATTLE_CHALLENGER();
                msg.Uid = uid;
                msg.MainId = mainId;

                //RedisPlayerInfo playerInfo = GetRedisPlayerInfo(uid);
                //if (playerInfo == null)
                //{
                //    //Log.WarnLine($"player {uid} GetPlayerHeroInfoMsg error: not find {uid} hero info.");
                //    //msg.Result = (int)ErrorCode.NoHeroInfo;
                //    GetBattleHeroInfos(uid, mainId, seeUid, seeMainId);
                //}
                //else
                {
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
            MSG_CorssR_GET_BATTLE_HEROS msg = new MSG_CorssR_GET_BATTLE_HEROS();
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
        /// 备份信息
        /// </summary>
        public void BackupLastFinalsPlayerRankInfo()
        {
            ////清理信息
            //server.CrossRedis.Call(new OperateClearBackupPlayerBaseInfo());
            ////开始备份
            //foreach (var kv in playerBaseInfoList)
            //{
            //    int mainId = kv.Value.GetIntValue(HFPlayerInfo.MainId);
            //    int groupId = CrossBattleLibrary.GetGroupId(mainId);
            //    server.CrossRedis.Call(new OperateBackupPlayerBaseInfo(kv.Key, groupId, kv.Value));
            //}

            //foreach (var kv in groupList)
            //{
            //    int groupId = kv.Key;
            //    foreach (var item in kv.Value.List)
            //    {
            //        int team = item.Key;
            //        foreach (var info in item.Value.List)
            //        {
            //            server.CrossRedis.Call(new OperateBackupBattleFightInfo(info.Value.Uid, groupId, team, info.Value.Index, 0));
            //        }
            //    }
            //}
        }

        //private void SendBattleEmail(CrossBattleTiming timing)
        //{
        //    CrossBattleTiming checkTiming = CrossBattleLibrary.GetCrossBattleTiming(timing);
        //    int timingId = (int)checkTiming;
        //    int emailId = CrossBattleLibrary.GetBattleEmailId(timing);
        //    foreach (var group in groupList)
        //    {
        //        foreach (var team in group.Value.List)
        //        {
        //            foreach (var item in team.Value.List)
        //            {
        //                if (item.Value.Uid > 0 && item.Value.Result + 1 == timingId)
        //                {
        //                    //发送邮件
        //                    MSG_CorssR_SEND_FINALS_REWARD msg = new MSG_CorssR_SEND_FINALS_REWARD();
        //                    msg.Uid = item.Value.Uid;
        //                    msg.EmailId = emailId;
        //                    PlayerWrite(msg, item.Value.Uid);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void LoadFinalsRankInfoFromRedis(int group, List<int> serverList)
        //{
        //    uidRankList.Clear();
        //    groupList.Clear();

        //    OperateGetCrossRankInfosByRank operate = new OperateGetCrossRankInfosByRank(seasonInfo.Id, group, 0, CrossBattleLibrary.FightPlayerCount - 1);
        //    server.GameRedis.Call(operate, (RedisCallback)(ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            if (operate.Characters == null)
        //            {
        //                return;
        //            }
        //            else
        //            {
        //                int i = 0;
        //                foreach (var kv in operate.Characters)
        //                {
        //                    i++;
        //                    PlayerRankBaseInfo item = GetArenaRankInfo(kv.Value, i);
        //                    AddPlayerRankInfo(item, group);
        //                }

        //                GetChallengerInfo(group);

        //                //RankSort();
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            Log.Error("LoadCrossRankInfosByRank execute OperateGetCrossRankInfosByRank fail: redis data error!");
        //            return;
        //        }
        //    }));
        //}

        //private void GetChallengerInfo(int group)
        //{
        //    //FightList.Clear();
        //    //初始化完成，获取一波数据
        //    foreach (var kv in CrossBattleLibrary.CrossFight)
        //    {
        //        PlayerRankBaseInfo Player1 = GetArenaRankInfoByRank(group, kv.Value.Player1);
        //        if (Player1 != null)
        //        {
        //            PlayerRankBaseInfo Player2 = GetArenaRankInfoByRank(group, kv.Value.Player2);
        //            if (Player2 != null)
        //            {
        //                int mianId = BaseApi.GetMainIdByUid(Player2.Uid);
        //                //没有缓存信息，查看玩家是否在线
        //                FrontendServer relation = server.RelationManager.GetSinglePointServer(mianId);
        //                if (relation != null)
        //                {
        //                    //找到玩家说明玩家在线，通知玩家发送信息回来
        //                    MSG_CorssR_GET_CHALLENGER msg = new MSG_CorssR_GET_CHALLENGER();
        //                    msg.PcUid = Player1.Uid;
        //                    msg.ChallengerUid = Player2.Uid;
        //                    msg.ChallengerDefensive.AddRange(Player2.Defensive);
        //                    msg.PcDefensive.AddRange(Player1.Defensive);
        //                    msg.GetType = (int)ChallengeIntoType.CrossFinals;
        //                    relation.Write(msg, Player1.Uid);

        //                    AddArenaRankInfoByRank(group, CrossBattleState.Battle64, Player1.Uid, Player2.Uid);
        //                }
        //                else
        //                {
        //                    //没有找到玩家，直接算输
        //                    Log.Warn("cross battle get challenger info find player {0} mainId {1} relation.", Player2.Uid, mianId);
        //                }
        //            }
        //            else
        //            {
        //                //没有找到玩家，直接算输
        //                Log.Warn("cross battle get challenger info find player2 rank {0} .", kv.Value.Player2);
        //            }
        //        }
        //        else
        //        {
        //            //没有找到玩家，直接算输
        //            Log.Warn("cross battle get challenger info find player1 rank {0} .", kv.Value.Player1);
        //        }
        //    }
        //}

        //public PlayerRankBaseInfo GetArenaRankInfo(PlayerBaseInfo baseInfo, int rank)
        //{
        //    ServerModels.PlayerRankBaseInfo info = new ServerModels.PlayerRankBaseInfo();
        //    info.Uid = baseInfo.Uid;
        //    info.Name = baseInfo.Name;
        //    info.Level = baseInfo.Level;
        //    info.Sex = baseInfo.Sex;
        //    info.Icon = baseInfo.Icon;
        //    info.IconFrame = baseInfo.IconFrame;
        //    info.ShowDIYIcon = baseInfo.ShowDIYIcon;
        //    info.HeroId = baseInfo.HeroId;
        //    info.GodType = baseInfo.GodType;
        //    info.BattlePower = baseInfo.BattlePower;
        //    //info.CrossLevel = baseInfo.CrossLevel;
        //    //info.CrossStar = baseInfo.CrossStar;
        //    info.SetDefensive(baseInfo.Defensive);
        //    info.Rank = rank;
        //    return info;
        //}





        //public void CrossBattleStart(CrossBattleState state)
        //{
        //    foreach (var group in CrossBattleLibrary.GroupList)
        //    {
        //        //CrossBattleGroupModel groupItem = GetBattleGroupItem(group);
        //        //if (groupItem != null)
        //        //{
        //        //    List<PlayerRankBaseInfo> uids = new List<PlayerRankBaseInfo>();
        //        //    获取赛季排行榜
        //        //    Dictionary<int, int> rankBattleList = groupItem.GetLastBattleRankList(state);
        //        //    foreach (var kv in rankBattleList)
        //        //    {
        //        //        PlayerRankBaseInfo Player1 = GetArenaRankInfoByUid(kv.Key);
        //        //        if (Player1 != null)
        //        //        {
        //        //            if (CheckBattleResult(state, Player1))
        //        //            {
        //        //                uids.Add(Player1);
        //        //                continue;
        //        //            }
        //        //        }
        //        //        else
        //        //        {
        //        //            没有找到玩家，直接算输
        //        //            Log.Warn("cross battle get challenger info find player1 rank {0} .", kv.Key);
        //        //        }

        //        //        PlayerRankBaseInfo Player2 = GetArenaRankInfoByUid(kv.Value);
        //        //        if (Player2 != null)
        //        //        {
        //        //            if (CheckBattleResult(state, Player2))
        //        //            {
        //        //                uids.Add(Player2);
        //        //                continue;
        //        //            }
        //        //        }
        //        //        else
        //        //        {
        //        //            没有找到玩家，直接算输
        //        //            Log.Warn("cross battle get challenger info find player1 rank {0} .", kv.Key);
        //        //        }
        //        //    }

        //        //    for (int i = 0; i < uids.Count - 1; i += 2)
        //        //    {
        //        //        PlayerRankBaseInfo Player1 = uids[i];

        //        //        PlayerRankBaseInfo Player2 = uids[i + 1];


        //        //        int mianId = BaseApi.GetMainIdByUid(Player2.Uid);
        //        //        没有缓存信息，查看玩家是否在线
        //        //        FrontendServer relation = server.RelationManager.GetSinglePointServer(mianId);
        //        //        if (relation != null)
        //        //        {
        //        //            找到玩家说明玩家在线，通知玩家发送信息回来
        //        //            MSG_CorssR_GET_CHALLENGER msg = new MSG_CorssR_GET_CHALLENGER();
        //        //            msg.PcUid = Player1.Uid;
        //        //            msg.ChallengerUid = Player2.Uid;
        //        //            msg.ChallengerDefensive.AddRange(Player2.Defensive);
        //        //            msg.PcDefensive.AddRange(Player1.Defensive);
        //        //            msg.GetType = (int)ChallengeIntoType.CrossFinals;
        //        //            relation.Write(msg, Player1.Uid);

        //        //            AddArenaRankInfoByRank(group, state, Player1.Uid, Player2.Uid);
        //        //        }
        //        //        else
        //        //        {
        //        //            没有找到玩家，直接算输
        //        //            Log.Warn("cross battle get challenger info find player {0} mainId {1} relation.", Player2.Uid, mianId);
        //        //        }
        //        //    }
        //        //}
        //    }
        //}

        //private static bool CheckBattleResult(CrossBattleState state, ServerModels.PlayerRankBaseInfo info)
        //{
        //    //switch (state)
        //    //{
        //    //    case CrossBattleState.BattleFinals:
        //    //        if (info.FinalsResult > 11110)
        //    //        {
        //    //            return true;
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle4:
        //    //        if (info.FinalsResult > 1110)
        //    //        {
        //    //            return true;
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle8:
        //    //        if (info.FinalsResult > 110)
        //    //        {
        //    //            return true;
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle16:
        //    //        if (info.FinalsResult > 10)
        //    //        {
        //    //            return true;
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle32:
        //    //        if (info.FinalsResult > 0)
        //    //        {
        //    //            return true;
        //    //        }
        //    //        break;
        //    //    default:
        //    //        break;
        //    //}
        //    return false;
        //}

        //public PlayerRankBaseInfo GetArenaRankInfoByRank(int group, int rank)
        //{
        //    ServerModels.PlayerRankBaseInfo info = null;
        //    CrossBattleGroupModel item = GetBattleGroup(group);
        //    if (item != null)
        //    {
        //        //int uid;
        //        //if (item.RankUidList.TryGetValue(rank, out uid))
        //        //{
        //        //    info = GetArenaRankInfoByUid(uid);
        //        //}
        //    }
        //    return info;
        //}

        //public void AddArenaRankInfoByRank(int group, CrossBattleState state, int uid1, int uid2)
        //{
        //    CrossBattleGroupModel item = GetBattleGroup(group);
        //    if (item != null)
        //    {
        //        SetBattleUid(state, uid1, uid2, item);
        //    }
        //    else
        //    {
        //        item = new CrossBattleGroupModel();
        //        SetBattleUid(state, uid1, uid2, item);
        //        groupList.Add(group, item);
        //    }
        //}

        //private static void SetBattleUid(CrossBattleState state, int uid1, int uid2, CrossBattleGroupModel dic)
        //{
        //    //switch (state)
        //    //{
        //    //    case CrossBattleState.BattleFinals:
        //    //        if (!dic.Rank2List.ContainsKey(uid1))
        //    //        {
        //    //            dic.Rank2List.Add(uid1, uid2);
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle4:
        //    //        if (!dic.Rank4List.ContainsKey(uid1))
        //    //        {
        //    //            dic.Rank4List.Add(uid1, uid2);
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle8:
        //    //        if (!dic.Rank8List.ContainsKey(uid1))
        //    //        {
        //    //            dic.Rank8List.Add(uid1, uid2);
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle16:
        //    //        if (!dic.Rank16List.ContainsKey(uid1))
        //    //        {
        //    //            dic.Rank16List.Add(uid1, uid2);
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle32:
        //    //        if (!dic.Rank32List.ContainsKey(uid1))
        //    //        {
        //    //            dic.Rank32List.Add(uid1, uid2);
        //    //        }
        //    //        break;
        //    //    case CrossBattleState.Battle64:
        //    //        if (!dic.Rank64List.ContainsKey(uid1))
        //    //        {
        //    //            dic.Rank64List.Add(uid1, uid2);
        //    //        }
        //    //        break;
        //    //    default:
        //    //        break;
        //    //}
        //}
        //private void StartFight(CrossBattleTiming timing, Dictionary<int, List<int>> dic, int groupId, CrossBattleGroupItem team, int teamId)
        //{
        //    //划分战斗
        //    Dictionary<int, CrossBattleFightInfo> fightList = team.CheckFiggtTeam(dic, timing);

        //    //准备通知开始战斗
        //    foreach (var item in fightList)
        //    {
        //        int fightId = item.Key;
        //        if (item.Value.PlayerList.Count < 2)
        //        {
        //            // 说明参赛人数不足
        //            Log.Warn($"cross battle get challenger info find player {timing} mainId {item.Key} relation.");
        //            if (item.Value.PlayerList.Count < 2)
        //            {
        //                SetCrossBattleResult((int)timing, groupId, teamId, fightId, item.Value.PlayerList[0].Uid);
        //            }
        //            else
        //            {
        //                SetCrossBattleResult((int)timing, groupId, teamId, fightId, 0);
        //            }
        //            continue;
        //        }
        //        CrossBattlePlayer Player1 = item.Value.PlayerList[0];
        //        CrossBattlePlayer Player2 = item.Value.PlayerList[1];
        //        if (Player1.Uid == 0 && Player2.Uid == 0)
        //        {
        //            SetCrossBattleResult((int)timing, groupId, teamId, fightId, item.Value.PlayerList[0].Uid);
        //        }
        //        else if (Player1.Uid == 0 && Player2.Uid != 0)
        //        {
        //            SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player2.Uid);
        //        }
        //        else if (Player1.Uid != 0 && Player2.Uid == 0)
        //        {
        //            SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player1.Uid);
        //        }
        //        else
        //        {
        //            RedisPlayerInfo Player1info = GetRedisPlayerInfo(Player1.Uid);
        //            if (Player1info == null)
        //            {
        //                //没有找到玩家，直接算输
        //                Log.Warn("cross battle get challenger info find player1 rank {0} .", Player1.Uid);
        //                //直接判输
        //                SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player1.Uid);
        //                continue;
        //            }
        //            RedisPlayerInfo Player2info = GetRedisPlayerInfo(Player2.Uid);
        //            if (Player2info == null)
        //            {
        //                //没有找到玩家，直接算输
        //                Log.Warn("cross battle get challenger info find player1 rank {0} .", Player2.Uid);
        //                //直接判输
        //                SetCrossBattleResult((int)timing, groupId, teamId, fightId, Player2.Uid);
        //                continue;
        //            }

        //            MSG_CorssR_GET_BATTLE_PLAYER msg = new MSG_CorssR_GET_BATTLE_PLAYER();
        //            msg.Player1 = GetPlayerBaseInfoMsg(Player1info, Player1.Index);
        //            msg.Player2 = GetPlayerBaseInfoMsg(Player2info, Player2.Index);
        //            msg.GetType = (int)ChallengeIntoType.CrossFinals;

        //            msg.TimingId = (int)timing;
        //            msg.GroupId = groupId;
        //            msg.TeamId = teamId;
        //            msg.FightId = fightId;
        //            //没有缓存信息，查看玩家是否在线
        //            FrontendServer relation = server.RelationManager.GetSinglePointServer(msg.Player1.MainId);
        //            if (relation != null)
        //            {
        //                //通知玩家发送信息回来
        //                relation.Write(msg, Player1.Uid);
        //            }
        //            else
        //            {
        //                //没有找到玩家，直接算输
        //                Log.Warn("cross battle get challenger info find player {0} mainId {1} relation.", Player2.Uid, msg.Player1.MainId);
        //            }
        //        }
        //    }
        //}
        //private static void InitBattleResult(int result, CrossBattleGroupItem teamItem, CrossBattleTiming timing, int uid, int index)
        //{
        //    if (teamItem != null)
        //    {
        //        int fightId = CrossBattleLibrary.GetFightId(timing, index);
        //        if (fightId > 0)
        //        {
        //            teamItem.InitResult(timing, fightId, uid, index, result);
        //        }
        //    }
        //}

        /// <summary>
        /// 聊天喇叭信息
        /// </summary>
        public void SendChatTrumpetInfo(int mainId, int uid, int itemId, string words, RC_SPEAKER_INFO pcInfo)
        {
            int groupId = CrossBattleLibrary.GetGroupId(mainId);
            if (groupId > 0 && uid > 0)
            {
                MSG_CrossR_CHAT_TRUMPET msg = new MSG_CrossR_CHAT_TRUMPET();
                msg.MainId = mainId;
                msg.ItemId = itemId;
                msg.Words = words;
                msg.PcInfo = GetCRSpeakerInfo(pcInfo);
                List<int> servers = CrossBattleLibrary.GetGroupServers(groupId);
                foreach (var serverId in servers)
                {
                    WriteToRelation(msg, serverId);
                    //FrontendServer relation = server.RelationManager.GetSinglePointServer(serverId);
                    //if (relation != null)
                    //{
                    //    relation.Write(msg, uid);
                    //}
                    //else
                    //{
                    //    Log.Warn($"cross send chat trumpet info to relation failed: not find mainId {serverId} relation.");
                    //}
                }
            }
        }

        private CR_SPEAKER_INFO GetCRSpeakerInfo(RC_SPEAKER_INFO msg)
        {
            CR_SPEAKER_INFO pcInfo = new CR_SPEAKER_INFO();
            pcInfo.Uid = msg.Uid;
            pcInfo.Name = msg.Name;
            pcInfo.Camp = msg.Camp;
            pcInfo.Level = msg.Level;
            pcInfo.FaceIcon = msg.FaceIcon;
            pcInfo.ShowFaceJpg = msg.ShowFaceJpg;
            pcInfo.FaceFrame = msg.FaceFrame;
            pcInfo.Sex = msg.Sex;
            pcInfo.Title = msg.Title;
            pcInfo.TeamId = msg.TeamId;
            pcInfo.HeroId = msg.HeroId;
            pcInfo.GodType = msg.GodType;
            pcInfo.ChatFrameId = msg.ChatFrameId;
            pcInfo.ArenaLevel = msg.ArenaLevel;
            return pcInfo;
        }
    }
}
