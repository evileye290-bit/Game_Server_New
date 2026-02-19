using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        //魂斗场
        public CrossInfoManager CrossInfoMng { get; set; }

        public void InitCrossBattleManager()
        {
            CrossInfoMng = new CrossInfoManager(this, server);
        }

        /// <summary>
        /// 通知跨服战具体信息
        /// </summary>
        public void SendCrossBattleManagerMessage()
        {
            if (server.CrossBattleMng.FirstStartTime > 0)
            {
                SyncCrossBattleManagerMessage();
            }
            else
            {
                //通知Relation获取
                MSG_ZR_GET_CROSS_BATTLE_START req = new MSG_ZR_GET_CROSS_BATTLE_START();
                server.SendToRelation(req, uid);
            }
        }

        public void SyncCrossBattleManagerMessage()
        {
            MSG_ZGC_CROSS_BATTLE_MANAGER info = new MSG_ZGC_CROSS_BATTLE_MANAGER();
            info.Rank = CrossInfoMng.Info.Rank;
            info.CrossLevel = CrossInfoMng.Info.Level;
            info.CrossStar = CrossInfoMng.Info.Star;
            info.WinStreak = CrossInfoMng.Info.WinStreak;
            info.ActiveReward = CrossInfoMng.Info.ActiveReward;
            info.PreliminaryReward = CrossInfoMng.Info.PreliminaryReward;
            info.DailyFight = CrossInfoMng.Info.DailyFight;
            info.SeasonFight = CrossInfoMng.Info.SeasonFight;
            info.ServerReward = CrossInfoMng.Info.ServerReward;
            info.StartTime = server.CrossBattleMng.FirstStartTime;
            info.OpenTeam = server.CrossBattleMng.TeamId;
            info.BattleTeam = CrossInfoMng.Info.BattleTeam;

            info.BossStateReward = CrossBossInfoMng.CounterInfo.PassReward;
            info.BossRankReward = CrossBossInfoMng.CounterInfo.Score;

            //info.HistoryMaxRank = CrossInfoMng.Info.HistoryMaxRank;
            //info.HistoryMaxStar = CrossInfoMng.Info.HistoryMaxStar;
            //info.FightTotal = CrossInfoMng.Info.FightTotal;
            //info.WinTotal = CrossInfoMng.Info.WinTotal;
            //info.HistoryWinStreak = CrossInfoMng.Info.HistoryWinStreak;
            //info.DefensiveHeros.AddRange(CrossInfoMng.Info.DefensiveHeros);
            Write(info);

            GetCrossGuessingInfo();
        }

        /// <summary>
        /// 领取活跃奖励
        /// </summary>
        /// <param name="heroIds"></param>
        public void GetCrossActiveReward()
        {
            MSG_ZGC_GET_CROSS_BATTLE_ACTIVE_REWARD response = new MSG_ZGC_GET_CROSS_BATTLE_ACTIVE_REWARD();

            CrossLevelInfo info = CrossBattleLibrary.CheckCrossLevel(CrossInfoMng.Info.Star);
            if (info == null)
            {
                Log.Warn("player {0} GetCrossActiveReward failed: no level info {1}", uid, CrossInfoMng.Info.Star);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //判断时间
            if (!CrossBattleLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossBattleMng.StartTime, server.Now()))
            {
                Log.Warn("player {0} GetCrossActiveReward failed: season preliminary time error", uid);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (CrossBattleLibrary.ActiveNum > CrossInfoMng.Info.DailyFight)
            {
                Log.Warn("player {0} GetCrossActiveReward failed: DailyFight is {1}", uid, CrossInfoMng.Info.DailyFight);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (CrossInfoMng.Info.ActiveReward != (int)(int)CrossRewardState.None)
            {
                Log.Warn("player {0} GetCrossActiveReward failed: ActiveReward is {1}", uid, CrossInfoMng.Info.ActiveReward);
                response.Result = (int)ErrorCode.Already;
                Write(response);
                return;
            }

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(info.ActiveReward);
            AddRewards(rewards, ObtainWay.CrossActivityReward);

            //清理旧的配置
            CrossInfoMng.GetActivityReward();

            //保存DB
            SyncDbUpdateCrossBattleReward();

            //komoelog
            KomoeLogRecordPvpFight(2, 4, rewards.RewardList, 1, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Star.ToString(), CrossInfoMng.Info.Star.ToString(), 0, 0);       

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.ActiveReward = CrossInfoMng.Info.ActiveReward;
            Write(response);
        }

        /// <summary>
        /// 领取海选奖励
        /// </summary>
        /// <param name="heroIds"></param>
        public void GetCrossPreliminaryReward()
        {
            MSG_ZGC_GET_CROSS_BATTLE_PRELIMINARY_REWARD response = new MSG_ZGC_GET_CROSS_BATTLE_PRELIMINARY_REWARD();

            CrossLevelInfo info = CrossBattleLibrary.CheckCrossLevel(CrossInfoMng.Info.Star);
            if (info == null)
            {
                Log.Warn("player {0} GetCrossPreliminaryReward failed: no level info {1}", uid, CrossInfoMng.Info.Star);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //判断时间
            if (!CrossBattleLibrary.CheckWeekTime(CrossTimeCheck.Finals, server.CrossBattleMng.StartTime, server.Now()))
            {
                Log.Warn("player {0} GetCrossPreliminaryReward failed: season preliminary time error", uid);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (CrossInfoMng.Info.SeasonFight <= 0)
            {
                Log.Warn("player {0} GetCrossPreliminaryReward failed: SeasonFight is {1}", uid, CrossInfoMng.Info.SeasonFight);
                response.Result = (int)ErrorCode.NoCrossPreliminary;
                Write(response);
                return;
            }

            if (CrossInfoMng.Info.PreliminaryReward != (int)(int)CrossRewardState.None)
            {
                Log.Warn("player {0} GetCrossPreliminaryReward failed: PreliminaryReward is {1}", uid, CrossInfoMng.Info.PreliminaryReward);
                response.Result = (int)ErrorCode.Already;
                Write(response);
                return;
            }

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(info.PreliminaryReward);
            AddRewards(rewards, ObtainWay.CrossPreliminaryReward);

            //清理旧的配置
            CrossInfoMng.GetPreliminaryReward();

            //保存DB
            SyncDbUpdateCrossBattleReward();

            //komoelog
            KomoeLogRecordPvpFight(2, 4, rewards.RewardList, 1, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Star.ToString(), CrossInfoMng.Info.Star.ToString(), 0, 0);

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.PreliminaryReward = CrossInfoMng.Info.PreliminaryReward;
            Write(response);
        }

        /// <summary>
        /// 领取全服奖励
        /// </summary>
        /// <param name="heroIds"></param>
        public void GetCrossServerReward()
        {
            MSG_ZGC_GET_CROSS_BATTLE_SERVER_REWARD response = new MSG_ZGC_GET_CROSS_BATTLE_SERVER_REWARD();


            if (CrossInfoMng.Info.ServerReward != (int)CrossRewardState.None)
            {
                Log.Warn("player {0} GetCrossPreliminaryReward failed: PreliminaryReward is {1}", uid, CrossInfoMng.Info.ServerReward);
                response.Result = (int)ErrorCode.Already;
                Write(response);
                return;
            }

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(CrossBattleLibrary.ServerReward);
            AddRewards(rewards, ObtainWay.CrossServerReward);

            //清理旧的配置
            CrossInfoMng.GetServerReward();

            //保存DB
            server.GameDBPool.Call(new QueryUpdateCrossBattleServerReward(Uid, CrossInfoMng.Info.ServerReward));

            //komoelog
            KomoeLogRecordPvpFight(2, 4, rewards.RewardList, 1, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Star.ToString(), CrossInfoMng.Info.Star.ToString(), 0, 0);

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.ServerReward = CrossInfoMng.Info.ServerReward;
            Write(response);
        }

        /// <summary>
        /// 获取下注信息
        /// </summary>
        /// <param name="heroIds"></param>
        public void GetCrossGuessingInfo()
        {
            MSG_ZR_GET_GUESSING_INFO response = new MSG_ZR_GET_GUESSING_INFO();
            server.SendToRelation(response, uid);
        }

        /// <summary>
        /// 获取下注信息
        /// </summary>
        /// <param name="heroIds"></param>
        public void CrossGuessingChoose(int choose)
        {
            //判断时间是否正确
            if (!CrossBattleLibrary.CheckWeekTime(CrossTimeCheck.GuessingTime, server.CrossBattleMng.StartTime, server.Now()))
            {
                MSG_ZGC_CROSS_GUESSING_CHOOSE msg = new MSG_ZGC_CROSS_GUESSING_CHOOSE();
                msg.Choose = choose;
                Log.Warn("player {0} CrossGuessingChoose failed: time error", uid);
                msg.Result = (int)ErrorCode.NotOpen;
                Write(msg);
                return;
            }
            //获取当前时间ID
            CrossBattleTiming endGuessing = CrossBattleLibrary.GetCurrentGuessingTime(server.CrossBattleMng.StartTime, server.Now());
            if (endGuessing == CrossBattleTiming.Start)
            {
                MSG_ZGC_CROSS_GUESSING_CHOOSE msg = new MSG_ZGC_CROSS_GUESSING_CHOOSE();
                msg.Choose = choose;
                Log.Warn("player {0} CrossGuessingChoose failed: not find guessing time", uid);
                msg.Result = (int)ErrorCode.NotOpen;
                Write(msg);
                return;
            }
            CrossBattleTiming timing = CrossBattleLibrary.GetCrossBattleTiming(endGuessing);

            MSG_ZR_CROSS_GUESSING_CHOOSE response = new MSG_ZR_CROSS_GUESSING_CHOOSE();
            response.Choose = choose;
            response.TimingId = (int)timing;
            server.SendToRelation(response, uid);
        }

        public void CrossGuessingChoose(int errorCode, int timingId, int choose, bool hasReward)
        {
            MSG_ZGC_CROSS_GUESSING_CHOOSE msg = new MSG_ZGC_CROSS_GUESSING_CHOOSE();
            msg.Choose = choose;
            //TODO 判断时间是否正确
            if (errorCode != (int)ErrorCode.Success)
            {
                Log.Warn("player {0} CrossGuessingChoose failed: error code {1}", uid, errorCode);
                msg.Result = (int)ErrorCode.NotOpen;
                Write(msg);
                return;
            }
            string reward = string.Empty;
            if (!hasReward)
            {
                //判断银币消耗
                RewardManager manager = new RewardManager();
                OnhookModel model = OnhookLibrary.GetOnhookModel(OnhookManager.TierId);
                if (model == null)
                {
                    Log.Warn("player {0} CrossGuessingChoose failed: OnhookManager TierId {1}", uid, OnhookManager.TierId);
                    msg.Result = (int)ErrorCode.NotOpen;
                    Write(msg);
                    return;
                }
                int num = CrossBattleLibrary.GuessingOnhookReward / OnhookLibrary.RewardTime;
                List<int> rewardItem = model.Data.GetIntList("GoldCardReward", ":");
                if (rewardItem.Count < 3)
                {
                    Log.Warn("player {0} CrossGuessingChoose failed: OnhookManager reward {1}", uid, model.Data.GetString("GoldCardReward"));
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
                float addRatio = GetTotalOnhookGoldAddRatio();
                int coustGold = (int)(rewardItem[2] * num * (1 + addRatio));
                CurrenciesType costType = (CurrenciesType)rewardItem[0];
                int gold = GetCoins(costType);
                if (gold < coustGold)
                {
                    Log.Warn("player {0} CrossGuessingChoose failed: gold is {1} not {2} ", uid, gold, coustGold);
                    msg.Result = (int)ErrorCode.GoldNotEnough;
                    Write(msg);
                    return;
                }

                DelCoins(costType, coustGold, ConsumeWay.CrossGuessing, OnhookManager.TierId.ToString());
                float rewardCount = coustGold * CrossBattleLibrary.GuessingOnhookRatio;
                reward = string.Format("{0}:{1}:{2}", rewardItem[0], rewardItem[1], (int)rewardCount);
            }

            if (!hasReward)
            {
                MSG_ZR_CROSS_GUESSING_REWARD response = new MSG_ZR_CROSS_GUESSING_REWARD();
                response.Choose = choose;
                response.TimingId = timingId;
                response.Reward = reward;
                server.SendToRelation(response, uid);
            }

            //komoelog
            KomoeLogRecordPvpFight(2, 2, null, 1, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Rank, CrossInfoMng.Info.Star.ToString(), CrossInfoMng.Info.Star.ToString(), 0, 0);

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        /// <summary>
        /// 更新排行榜
        /// </summary>
        /// <param name="rank"></param>
        public void UpdateCrossSeasonRank(int rank)
        {
            if (rank != CrossInfoMng.Info.Rank)
            {
                CrossInfoMng.ChargeRank(rank);            
            }
        }

        public void UpdateCrossBattleTeamId(int teamId)
        {
            CrossInfoMng.ChargeBattleTeam(teamId);

            //SendCrossBattleManagerMessage();
        }

        /// <summary>
        /// 获取海选对战者
        /// </summary>
        /// <param name="page"></param>
        public void GetCrossPreliminaryChallenger()
        {
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            CrossLevelInfo info = CrossBattleLibrary.CheckCrossLevel(CrossInfoMng.Info.Star);
            if (info == null)
            {
                Log.Warn($"player {uid} GetCrossPreliminaryChallenger failed: no level info {CrossInfoMng.Info.Star}");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //判断时间
            if (!CrossBattleLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossBattleMng.StartTime, server.Now()))
            {
                Log.Warn($"player {uid} GetCrossPreliminaryChallenger failed: season preliminary time {server.CrossBattleMng.StartTime} error");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //Redis 获取列表
            CrossLevelInfo levelInfo = CrossBattleLibrary.GetCrossLevelInfo(CrossInfoMng.Info.Level);
            if (levelInfo == null)
            {
                Log.Warn($"player {uid} GetCrossPreliminaryChallenger failed: preliminary level {CrossInfoMng.Info.Level} error");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (HeroMng.CrossQueue.Count == 0)
            {
                Log.Warn($"player {uid} GetCrossPreliminaryChallenger failed: queue {HeroMng.CrossQueue.Count} error");
                response.Result = (int)ErrorCode.NoDefensiveQueew;
                Write(response);
                return;
            }

            int group = CrossBattleLibrary.GetGroupId(server.MainId);
            //获取赛季排行榜
            OperateGetRankByScore operate = new OperateGetRankByScore(RankType.CrossServer, Uid, server.MainId,
                CrossInfoMng.Info.Star, levelInfo.MinNum, levelInfo.CheckRange);
            server.GameRedis.Call(operate, (RedisCallback)(ret =>
            {
                if ((int)ret == 1)
                {
                    if (operate.uidRank == null || operate.uidRank.Count == 0)
                    {
                        //使用机器人
                        RobotEnterCrossBattleMap();
                        return;
                    }
                    else
                    {
                        //int max = Math.Max(levelInfo.MinNum, operate.uidRank.Count - 1);
                        int index = NewRAND.Next(0, operate.uidRank.Count - 1);
                        if (index < operate.uidRank.Count)
                        {
                            int uid = operate.uidRank[index];
                            //PlayerRankBaseInfo rankInfo = GetArenaRankInfo(baseInfo);
                            //获取玩家信息
                            GetCrossChallengerInfo(uid);
                        }
                        else
                        {
                            //如果人数不足使用机器人补足匹配 
                            RobotEnterCrossBattleMap();
                        }
                        return;
                    }
                }
                else
                {
                    Log.Error("LoadRankInfoFromRedis execute OperateGetCrossRankInfos fail: redis data error!");
                    return;
                }
            }));
        }

        private void RobotEnterCrossBattleMap()
        {
            //机器人，直接读表
            CrossRobotInfo robotInfo = RobotLibrary.GetCrossRobotInfo(CrossInfoMng.Info.Star);
            if (robotInfo != null)
            {
                PlayerCrossFightInfo rankInfo = GetCrossRobotInfo(robotInfo);
                if (rankInfo != null)
                {
                    //MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO response = new MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO();
                    //List<int> heros = RobotManager.GetHeroRobotIdList(robotInfo);
                    //List<HeroInfo> infos = RobotManager.GetHeroList(heros);
                    ////信息
                    //foreach (var heroInfo in infos)
                    //{
                    //    Hero hero = NewHero(server, this, heroInfo);
                    //    hero.Init();
                    //    CHALLENGER_HERO_INFO challenger = GetChallengerHeroInfo(hero);
                    //    response.HeroList.Add(challenger);
                    //}

                    //response.Result = (int)ErrorCode.Success;
                    //response.Info = GetCrossRankBaseInfo(rankInfo);
                    //Write(response);
                    rankInfo.Type = ChallengeIntoType.CrossPreliminary;
                    EnterCrossBattleMap(rankInfo);
                }
                else
                {
                    Log.WarnLine("player {0} show arena challenger info failed: not find rank info ", Uid);
                    //return;
                }
            }
            else
            {
                Log.WarnLine("player {0} GetCrossArenaRobotInfo failed: not find rank info star {1}", Uid, CrossInfoMng.Info.Star);
                //return;
            }
        }

        /// <summary>
        /// 进入跨服战
        /// </summary>
        /// <param name="index"></param>
        public void EnterCrossBattleMap(PlayerCrossFightInfo fightInfo)
        {
            //到Relation获取一个对手，最后获得对手信息后才开始战斗
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            int dungeonId = CrossBattleLibrary.MapId;
            response.DungeonId = dungeonId;
            if (fightInfo == null)
            {
                Log.WarnLine("player {0} enter cross battle map failed: not find rank info", Uid);
                response.Result = (int)ErrorCode.NotFindChallengerInfo;
                Write(response);
                return;
            }

            //#if DEBUG
            //            fightInfo.Type = ChallengeIntoType.CrossFinals;
            //#endif

            //GetArenaRobotInfo
            if (fightInfo.Type != ChallengeIntoType.CrossFinals)
            {
                response.Result = (int)CanCreateDungeon(dungeonId);
                if (response.Result != (int)ErrorCode.Success)
                {
                    Log.Write($"player {Uid} request to enter arena {dungeonId} failed: reason {response.Result}");
                    Write(response);
                    return;
                }
            }
            else
            {
                dungeonId = 9004;
            }
            // 在当前zone创建副本
            DungeonMap dungeon = server.MapManager.CreateDungeon(dungeonId);
            if (dungeon == null)
            {
                Log.Write($"player {Uid} enter arena map request to create dungeon {dungeonId} failed: create dungeon failed");
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }

            if (dungeon.Model.MapType == MapType.CrossBattle)
            {
                //对手基本信息
                Write(GetCrossChallengerMsg(fightInfo));
                //更新挑战次数
                UpdateCounter(CounterType.CrossBattleCount, -1);
            }

            CrossBattleDungeonMap crossBattleDungeonMap = dungeon as CrossBattleDungeonMap;
            crossBattleDungeonMap.BattleFpsManager?.SetBattleInfo(this, fightInfo);
            crossBattleDungeonMap.SetChallengeIntoType(fightInfo, Uid);

            long battlePower = HeroMng.GetBattlePower64(HeroQueueType.CrossBattle);

            //战力压制
            dungeon.SetBattlePowerSuppress(battlePower, fightInfo.GetBattlePower());

            if (battlePower > fightInfo.BattlePower)
            {
                //添加自己
                crossBattleDungeonMap.AddAttackerMirror(this);
                //添加对手
                crossBattleDungeonMap.AddCrossDefender(fightInfo);
            }
            else
            {
                //添加对手
                crossBattleDungeonMap.AddCrossDefender(fightInfo);
                //添加自己
                crossBattleDungeonMap.AddAttackerMirror(this);
            }


            if (dungeon.Model.MapType == MapType.CrossBattle)
            {
                // 成功 进入副本
                RecordEnterMapInfo(crossBattleDungeonMap.MapId, crossBattleDungeonMap.Channel, crossBattleDungeonMap.BeginPosition);
                RecordOriginMapInfo();
                OnMoveMap();
            }
            else
            {
                //CrossBattleFinalsDungeonMap crossBattleFinalsDungeonMap = dungeon as CrossBattleFinalsDungeonMap;
                //crossBattleFinalsDungeonMap.StartBattle();
            }
        }


        /// <summary>
        /// 决赛信息
        /// </summary>
        /// <param name="page"></param>
        public void ShowCrossBattleFinalsInfo(int teamId)
        {
            MSG_ZR_SHOW_CROSS_BATTLE_FINALS req = new MSG_ZR_SHOW_CROSS_BATTLE_FINALS();
            req.TeamId = teamId;
            server.SendToRelation(req, uid);
        }

        public void ShowCrossBattleChallenger(int showUid, int mainId)
        {
            MSG_ZR_SHOW_CROSS_BATTLE_CHALLENGER req = new MSG_ZR_SHOW_CROSS_BATTLE_CHALLENGER();
            req.Uid = showUid;
            req.MainId = mainId;
            server.SendToRelation(req, uid);
        }

        public void GetCrossBattleVedio(int team, int vedio)
        {
            MSG_ZR_GET_CROSS_VIDEO req = new MSG_ZR_GET_CROSS_VIDEO();
            req.TeamId = team;
            req.VedioId = vedio;
            server.SendToRelation(req, uid);
        }

        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="result">DungeonResult result, ArenaRankInfo rankInfo</param>
        /// <param name="rankInfo"></param>
        public void SendCrossBattleResult(DungeonResult result)
        {
            //int newResult = NewRAND.Next(1, 3);
            //int newResult = 3;
            int oldScore = CrossInfoMng.Info.Star;
            RewardManager rewards = new RewardManager();
            string reward = string.Empty;
            switch (result)
            {
                case DungeonResult.Success:
                    //判断时间
                    if (CrossBattleLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossBattleMng.StartTime, server.Now()))
                    {
                        //超过海选时间不加星星
                        //CrossInfoMng.AddStar(true, CrossBattleLibrary.WinStar);
                        CrossInfoMng.Info.WinStreak++;
                        CrossInfoMng.Info.WinTotal++;
                        CrossInfoMng.Info.HistoryWinStreak++;
                        int addStar = CrossBattleLibrary.WinStar;
                        if (CrossInfoMng.Info.WinStreak >= CrossBattleLibrary.WinStreakNum)
                        {
                            addStar += CrossBattleLibrary.WinStreakStar;
                        }
                        CrossInfoMng.AddStar(true, addStar);
                        //CrossInfoMng.AddWinStreak();
                        CrossInfoMng.AddFightCount();
                    }
                    //奖励
                    reward = CrossBattleLibrary.ChallengeWinReward;
                    break;
                case DungeonResult.Tie:
                case DungeonResult.Failed:
                default:
                    //判断时间
                    if (CrossBattleLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossBattleMng.StartTime, server.Now()))
                    {
                        //分数增加，排名不变
                        CrossInfoMng.AddStar(false, CrossBattleLibrary.LoseStar);
                        CrossInfoMng.ResetWinStreak();
                        CrossInfoMng.AddFightCount();
                    }
                    //奖励
                    reward = CrossBattleLibrary.ChallengeLoseReward;
                    break;
            }

            //奖励
            rewards.InitSimpleReward(reward);
            AddRewards(rewards, ObtainWay.CrossPreliminaryResult);

            //记录排行榜
            //UpdateCrossRankInfosToRedis(seasonInfo);

            //保存DB
            SyncDbUpdateCrossBattleResult();

            //通知前端奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = GetRewardSyncMsg(rewards);
            rewardMsg.DungeonId = CrossBattleLibrary.MapId;
            rewardMsg.Result = (int)result;

            CheckCacheRewardMsg(rewardMsg);

            int dungeonResult = 2;
            if (result == DungeonResult.Success)
            {
                dungeonResult = 1;
            }
            //komoelog
            KomoeLogRecordPvpFight(2, 1, rewards.RewardList, dungeonResult, CrossInfoMng.Info.Rank, 0, oldScore.ToString(), CrossInfoMng.Info.Star.ToString(), 0, 0);
            ////刷新竞技场信息
            //SendCrossBattleManagerMessage();//relation 返回时会通知


            ////通知Relation 获取排名
            //MSG_ZR_CHALLENGE_WIN_CHANGE_RANK msg = new MSG_ZR_CHALLENGE_WIN_CHANGE_RANK();
            //msg.PcUid = Uid;
            //msg.PcRank = CrossBattleMng.Rank;
            //msg.ChallengerUid = rankInfo.Uid;
            //msg.ChallengerRank = rankInfo.Rank;

            //msg.OldScore = oldScore;
            //msg.Reward = reward;
            //msg.Result = (int)result;
            //msg.HistoryRank = CrossBattleMng.HistoryMaxRank;
            //server.SendToRelation(msg, Uid);

        }

        //public PlayerRankBaseInfo GetArenaRankInfo()
        //{
        //    PlayerRankBaseInfo info = new PlayerRankBaseInfo();
        //    info.Uid = Uid;
        //    info.Name = Name;
        //    info.Level = Level;
        //    info.Sex = Sex;
        //    info.Icon = Icon;
        //    info.IconFrame = 0;
        //    info.ShowDIYIcon = ShowDIYIcon;
        //    info.HeroId = HeroId;
        //    info.GodType = GodType;
        //    //info.BattlePower = UpdateCrossPower();
        //    //info.CrossLevel = CrossInfoMng.Info.Level;
        //    //info.CrossStar = CrossInfoMng.Info.Star;
        //    //info.Defensive.AddRange(CrossInfoMng.Info.DefensiveHeros);
        //    return info;
        //}

        //public PlayerRankBaseInfo GetArenaRankInfo(PlayerBaseInfo baseInfo)
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
        //    info.GodType = baseInfo.HeroId;
        //    info.BattlePower = baseInfo.GodType;
        //    //info.CrossLevel = baseInfo.CrossLevel;
        //    //info.CrossStar = baseInfo.CrossStar;
        //    info.SetDefensive(baseInfo.Defensive);
        //    return info;
        //}

        public static PlayerRankBaseInfo GetArenaRankInfo(CHALLENGER_INFO msg)
        {
            ServerModels.PlayerRankBaseInfo info = new ServerModels.PlayerRankBaseInfo();
            info.Rank = msg.Rank;
            info.IsRobot = msg.IsRobot;
            info.Defensive.AddRange(msg.Defensive);
            info.HeroGod.AddRange(msg.HeroGod);
            info.DefensivePower = msg.DefensivePower;

            info.Uid = msg.BaseInfo.Uid;
            info.Name = msg.BaseInfo.Name;
            info.Sex = msg.BaseInfo.Sex;
            info.Level = msg.BaseInfo.Level;
            info.LadderLevel = msg.BaseInfo.LadderLevel;
            info.BattlePower = msg.BaseInfo.BattlePower;
            info.HeroId = msg.BaseInfo.HeroId;
            info.GodType = msg.BaseInfo.GodType;
            return info;
        }

        private void GetCrossChallengerInfo(int challengerUid)
        {
            //int challengerUid = rankInfo.Uid;
            //int mianId = BaseApi.GetMainIdByUid(challengerUid);
            //if (mianId == server.MainId)
            //{

            //加载玩家信息，先找同服务器玩家
            PlayerChar challenger = server.PCManager.FindPc(challengerUid);
            if (challenger == null)
            {
                challenger = server.PCManager.FindOfflinePc(challengerUid);
                if (challenger == null)
                {
                    //Log.WarnLine("player {0} show player info fail,can not find player {1}.", Uid, showPcUid);
                    //没找到玩家，去relation获取信息
                    GetCrossChallengerInfoByRelation(challengerUid);
                    return;
                }
            }

            ////找到玩家，直接获取信息
            //MSG_ZGC_ARENA_CHALLENGER_HERO_INFO response = challenger.GetChallengerMsg();
            ////缓存信息
            //foreach (var item in response.HeroList)
            //{
            //    RobotHeroInfo robotInfo = GetRobotHeroInfo(item);
            //    rankInfo.HeroInfos.Add(robotInfo);
            //}

            ////发送给Relation 缓存信息
            //MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO addMsg = new MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO();
            //addMsg.PcUid = Uid;
            //addMsg.ChallengerUid = challengerUid;
            //addMsg.Info = response;
            //server.SendToRelation(addMsg, Uid);
            ////获取数据时间
            //rankInfo.UpdateTime = ZoneServerApi.now;

            //MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO syncMsg = challenger.GetCrossChallengerMsg();
            //Write(syncMsg);

            PlayerCrossFightInfo rankInfo = challenger.GetCrossRobotInfo();
            rankInfo.Type = ChallengeIntoType.CrossPreliminary;
            //进入战斗
            EnterCrossBattleMap(rankInfo);
            //}
            //else
            //{
            //    GetCrossChallengerInfoByRelation(rankInfo);
            //}
        }

        public ZR_BattlePlayerMsg GetBattlePlayerInfoMsg()
        {
            ZR_BattlePlayerMsg response = new ZR_BattlePlayerMsg();
            response.Uid = Uid;
            response.MainId = server.MainId;
            //基本信息
            response.BaseInfo.AddRange(GetZRHFPlayerMsg());
            long power = 0;
            //伙伴信息
            foreach (var kv in HeroMng.CrossQueue)
            {
                foreach (var hero in kv.Value)
                {
                    //ZR_Hero_Info zRInfo = GetZRHeroInfo(hero.Value);
                    ZR_Show_HeroInfo zRInfo = GetZrPlayerHeroInfoMsg(hero.Value, HeroQueueType.CrossBattle);
                    response.Heros.Add(zRInfo);
                    power += zRInfo.Power;
                }
            }
            response.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.BattlePower64, power));
            response.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.BattlePower, power.ToIntValue()));

            response.NatureValues.Add(NatureValues);
            response.NatureRatios.Add(NatureRatios);
            return response;
        }

        private List<ZR_HFPlayerBaseInfoItem> GetZRHFPlayerMsg()
        {
            List<ZR_HFPlayerBaseInfoItem> list = new List<ZR_HFPlayerBaseInfoItem>();
            list.Add(SetBaseInfoItem(HFPlayerInfo.Uid, uid));
            list.Add(SetBaseInfoItem(HFPlayerInfo.Name, Name));
            list.Add(SetBaseInfoItem(HFPlayerInfo.Level, Level));
            list.Add(SetBaseInfoItem(HFPlayerInfo.Sex, Sex));
            list.Add(SetBaseInfoItem(HFPlayerInfo.Icon, Icon));
            list.Add(SetBaseInfoItem(HFPlayerInfo.IconFrame, 0));
            list.Add(SetBaseInfoItem(HFPlayerInfo.ShowDIYIcon, ShowDIYIcon));
            list.Add(SetBaseInfoItem(HFPlayerInfo.HeroId, HeroId));
            list.Add(SetBaseInfoItem(HFPlayerInfo.MainId, server.MainId));
            list.Add(SetBaseInfoItem(HFPlayerInfo.GodType, GodType));
            //list.Add(SetBaseInfoItem(HFPlayerInfo.BattlePower, UpdateCrossPower()));
            list.Add(SetBaseInfoItem(HFPlayerInfo.CrossLevel, CrossInfoMng.Info.Level));
            list.Add(SetBaseInfoItem(HFPlayerInfo.CrossScore, CrossInfoMng.Info.Star));
            return list;
        }

        public ZR_BattlePlayerMsg GetBossPlayerInfoMsg()
        {
            ZR_BattlePlayerMsg response = new ZR_BattlePlayerMsg();
            response.Uid = Uid;
            response.MainId = server.MainId;
            //基本信息
            response.BaseInfo.AddRange(GetZRHFPlayerMsg());
            long power = 0;
            //伙伴信息
            foreach (var kv in HeroMng.CrossBossQueue)
            {
                foreach (var hero in kv.Value)
                {
                    ZR_Show_HeroInfo zRInfo = GetZrPlayerHeroInfoMsg(hero.Value, HeroQueueType.CrossBoss);
                    response.Heros.Add(zRInfo);
                    power += zRInfo.Power;
                }
            }
            response.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.BattlePower64, power));
            response.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.BattlePower, power.ToIntValue()));

            response.NatureValues.Add(NatureValues);
            response.NatureRatios.Add(NatureRatios);
            return response;
        }

        //private ZR_Hero_Info GetZRHeroInfo(HeroInfo hero)
        //{
        //    ZR_Hero_Info zRInfo = new ZR_Hero_Info();
        //    zRInfo.Id = hero.Id;
        //    zRInfo.Level = hero.Level;
        //    zRInfo.StepsLevel = hero.StepsLevel;
        //    zRInfo.SoulSkillLevel = hero.SoulSkillLevel;
        //    zRInfo.GodType = hero.GodType;
        //    zRInfo.CrossQueueNum = hero.CrossQueueNum;
        //    zRInfo.CrossPositionNum = hero.CrossPositionNum;
        //    zRInfo.Power = hero.GetBattlePower();
        //    //魂环
        //    zRInfo.SoulRings.AddRange(GetHeroSoulRingMsg(hero.Id));
        //    //魂骨
        //    zRInfo.SoulBones.AddRange(GetHeroSoulBoneMsg(hero.Id));
        //    //属性
        //    zRInfo.Natures = GetNature(hero);
        //    return zRInfo;
        //}

        private ZR_Hero_Nature GetNature(HeroInfo hero)
        {
            ZR_Hero_Nature heroNature = new ZR_Hero_Nature();
            foreach (var item in hero.Nature.GetNatureList())
            {
                ZR_Hero_Nature_Item info = new ZR_Hero_Nature_Item();
                info.NatureType = (int)item.Key;
                info.Value = hero.GetNatureValue(item.Key);
                heroNature.List.Add(info);
            }
            return heroNature;
        }

        //private List<ZR_Hero_SoulBone> GetHeroSoulBoneMsg(int heroId)
        //{
        //    List<ZR_Hero_SoulBone> list = new List<ZR_Hero_SoulBone>();
        //    //魂环
        //    List<SoulBone> soulBoneDic = SoulboneMng.GetEnhancedHeroBones(heroId);
        //    if (soulBoneDic != null)
        //    {
        //        //有魂环
        //        foreach (var soulBone in soulBoneDic)
        //        {
        //            try
        //            {
        //                ZR_Hero_SoulBone soulBoneMsg = new ZR_Hero_SoulBone();
        //                soulBoneMsg.Id = soulBone.TypeId;
        //                list.Add(soulBoneMsg);
        //            }
        //            catch (Exception e)
        //            {
        //                //没找到魂环信息
        //                Log.WarnLine("player {0} GetHeroSoulBoneMsg fail,can not find soulBone {1}, {2}.", Uid, soulBone.Uid, e);
        //            }
        //        }
        //    }
        //    return list;
        //}

        //private List<ZR_Hero_SoulRing> GetHeroSoulRingMsg(int heroId)
        //{
        //    List<ZR_Hero_SoulRing> list = new List<ZR_Hero_SoulRing>();
        //    //魂环
        //    Dictionary<int, SoulRingItem> soulRingDic = SoulRingManager.GetAllEquipedSoulRings(heroId);
        //    if (soulRingDic != null)
        //    {
        //        //有魂环
        //        foreach (var soulRing in soulRingDic)
        //        {
        //            try
        //            {
        //                ZR_Hero_SoulRing soulRingMsg = new ZR_Hero_SoulRing();
        //                soulRingMsg.Pos = soulRing.Key;
        //                soulRingMsg.Level = soulRing.Value.Level;
        //                soulRingMsg.SpecId = soulRing.Value.SpecId;
        //                soulRingMsg.Year = soulRing.Value.Year;
        //                list.Add(soulRingMsg);
        //            }
        //            catch (Exception e)
        //            {
        //                //没找到魂环信息
        //                Log.WarnLine("player {0} GetHeroSoulRingMsg fail,can not find soulBone {1}, {2}.", Uid, soulRing.Value.Uid, e);
        //            }
        //        }
        //    }
        //    return list;
        //}

        public ZR_HFPlayerBaseInfoItem SetBaseInfoItem(HFPlayerInfo key, object value)
        {
            ZR_HFPlayerBaseInfoItem item = new ZR_HFPlayerBaseInfoItem();
            item.Key = (int)key;
            item.Value = value.ToString();
            return item;
        }

        public MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO GetCrossChallengerMsg(PlayerCrossFightInfo fightInfo)
        {

            MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO response = new MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO();
            response.Result = (int)ErrorCode.Success;

            Dictionary<int, Dictionary<int, MSG_ZGC_HERO_INFO>> dic = new Dictionary<int, Dictionary<int, MSG_ZGC_HERO_INFO>>();
            Dictionary<int, MSG_ZGC_HERO_INFO> list;
            foreach (var kv in fightInfo.HeroQueue.OrderBy(x=>x.Key))
            {
                foreach (var pos in kv.Value)
                {
                    MSG_ZGC_HERO_INFO info = GetZgcPlayerHeroInfoMsg(pos.Value);
                    info.CrossQueueNum = kv.Key;
                    info.CrossPositionNum = pos.Key;
                    if (dic.TryGetValue(info.CrossQueueNum, out list))
                    {
                        list[info.CrossPositionNum] = info;
                    }
                    else
                    {
                        list = new Dictionary<int, MSG_ZGC_HERO_INFO>();
                        list[info.CrossPositionNum] = info;
                        dic.Add(info.CrossQueueNum, list);
                    }
                }
            }
            long power = 0;
            foreach (var kv in dic)
            {
                CROSS_BATTLE_HERO_QUEUE info = new CROSS_BATTLE_HERO_QUEUE();
                foreach (var item in kv.Value)
                {
                    info.BattlePower += item.Value.Power;
                    item.Value.CrossQueueNum = kv.Key;
                    item.Value.CrossPositionNum = item.Key;
                    info.HeroList.Add(item.Value);
                }
                power += info.BattlePower;
                response.Queue.Add(info);
            }

            //自己信息
            response.Info = new CROSS_CHALLENGER_INFO();
            response.Info.BaseInfo = GetPlayerBaseInfo(fightInfo);
            response.Info.BaseInfo.BattlePower64 = power;
            response.Info.CrossLevel = fightInfo.CrossLevel;
            response.Info.CrossStar = fightInfo.CrossStar;
            response.Info.BaseInfo.BattlePower = power.ToIntValue();

            return response;
        }



        private void GetCrossChallengerInfoByRelation(int challengerUid)
        {
            //MSG_ZR_GET_ARENA_CHALLENGER msg = new MSG_ZR_GET_ARENA_CHALLENGER();
            //msg.PcUid = Uid;
            ////msg.PcDefensive.Add(HeroId);
            //foreach (var kv in ArenaMng.DefensiveHeros)
            //{
            //    msg.PcDefensive.Add(kv.Key);
            //    msg.PDefPoses.Add(kv.Value);
            //}
            ////msg.PcDefensive.AddRange(ArenaMng.DefensiveHeros);
            ////msg.PDefPoses.AddRange(ArenaMng.GetDefensiveHeroPoses());
            //msg.ChallengerUid = rankInfo.Uid;
            ////msg.ChallengerDefensive.Add(rankInfo.HeroId);
            //msg.ChallengerDefensive.AddRange(rankInfo.Defensive);
            //msg.CDefPoses.AddRange(rankInfo.DefPoses);
            //msg.GetType = (int)ChallengeIntoType.CrossPreliminary;
            //server.SendToRelation(msg, Uid);

            //CrossInfoMng.TempChallengerInfo = rankInfo;

            MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO msg = new MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO();
            msg.ChallengerUid = challengerUid;
            msg.GetType = (int)ChallengeIntoType.CrossPreliminary;
            server.SendToRelation(msg, Uid);
        }

        private PlayerCrossFightInfo GetCrossRobotInfo(CrossRobotInfo info)
        {
            PlayerCrossFightInfo rankInfo = null;
            //机器人，直接读表
            if (info != null)
            {
                rankInfo = new PlayerCrossFightInfo();
                rankInfo.Uid = 0;
                rankInfo.HeroQueue = info.HeroQueue;
                rankInfo.IsRobot = true;
                rankInfo.Name = info.Name;
                rankInfo.Level = info.Level;
                rankInfo.Icon = info.Icon;
                rankInfo.IconFrame = info.IconFrame;
                rankInfo.HeroId = info.HeroId;
                rankInfo.BattlePower = info.BattlePower;
                rankInfo.CrossLevel = info.CrossLevel;
                rankInfo.CrossStar = info.CrossStar;
            }
            else
            {
                Log.WarnLine("player {0} GetCrossArenaRobotInfo failed: not find rank info star {1}", Uid, rankInfo.CrossStar);
                //return;
            }
            return rankInfo;
        }

        public PlayerCrossFightInfo GetCrossRobotInfo()
        {
            PlayerCrossFightInfo rankInfo = new PlayerCrossFightInfo();
            rankInfo.Uid = Uid;
            rankInfo.IsRobot = true;
            rankInfo.Name = Name;
            rankInfo.Level = Level;
            rankInfo.Icon = Icon;
            //rankInfo.IconFrame = IconFrame;
            rankInfo.HeroId = HeroId;
            rankInfo.BattlePower = HeroMng.CalcBattlePower();
            rankInfo.CrossLevel = CrossInfoMng.Info.Level;
            rankInfo.CrossStar = CrossInfoMng.Info.Star;

            rankInfo.NatureValues = NatureValues;
            rankInfo.NatureRatios = NatureRatios;
            //伙伴信息
            foreach (var kv in HeroMng.CrossQueue)
            {
                foreach (var item in kv.Value)
                {
                    RobotHeroInfo robotHero = new RobotHeroInfo();
                    robotHero.HeroId = item.Value.Id;
                    robotHero.Level = item.Value.Level;
                    robotHero.AwakenLevel = item.Value.AwakenLevel;
                    robotHero.StepsLevel = item.Value.StepsLevel;
                    robotHero.SoulSkillLevel = item.Value.SoulSkillLevel;
                    robotHero.EquipIndex = item.Value.EquipIndex;
                    robotHero.GodType = item.Value.GodType;
                    robotHero.BattlePower = item.Value.GetBattlePower();
                    foreach (var nature in item.Value.Nature.GetNatureList())
                    {
                        robotHero.NatureList[nature.Key] = nature.Value.Value;
                    }

                    Dictionary<int, SoulRingItem> soulRing = SoulRingManager.GetAllEquipedSoulRings(item.Value.Id);
                    if (soulRing != null)
                    {
                        foreach (var curr in soulRing)
                        {
                            robotHero.SoulRings += string.Format("{0}:{1}:{2}:{3}:{4}|", curr.Value.Position, curr.Value.Level, curr.Value.SpecId, curr.Value.Year,curr.Value.Element);
                        }
                    }

                    List<SoulBone> soulBoneList = SoulboneMng.GetEnhancedHeroBones(item.Value.Id);
                    if (soulBoneList != null)
                    {
                        List<string> soulBoneStr = soulBoneList.ToList().ConvertAll(x =>
                        {
                            List<int> specList = x.GetSpecList();
                            return specList.Count <= 0 ? x.TypeId.ToString() : x.TypeId.ToString() + ":" + string.Join(":", specList);
                        });
                        robotHero.SoulBones = string.Join("|", soulBoneStr);
                    }

                    HiddenWeaponItem weaponItem = HiddenWeaponManager.GetHeroEquipedHiddenWeapon(item.Value.Id);
                    if (weaponItem != null)
                    {
                        robotHero.HiddenWeapon = $"{weaponItem.Id}:{weaponItem.Info.Star}";
                    }

                    List<EquipmentItem> equipmentItems = EquipmentManager.GetAllEquipedEquipments(item.Value.Id);
                    robotHero.Equipment = string.Join("|", equipmentItems.Select(x => x.Id));

                    rankInfo.AddHero(robotHero, item.Key, kv.Key);
                }
            }
            return rankInfo;
        }

        public void RefreshCrossDailyActiveReward()
        {
            CrossInfoMng.RefreshDaily();
            server.GameDBPool.Call(new QueryUpdateCrossActiveReward(Uid, CrossInfoMng.Info.ActiveReward, CrossInfoMng.Info.DailyFight));
        }

        public void RefreshCrossRank(bool syncClient)
        {
            CrossInfoMng.RefreshSeason();
            //SyncDbUpdateCrossBattleResult();
            if (syncClient)
            {
                SendCrossBattleManagerMessage();
            }
        }



        //private void UpdateCrossRankInfosToRedis(CrossSeasonInfo seasonInfo)
        //{
        //    int group = CrossBattleLibrary.GetCrossGroup(server.MainId);
        //    int newTime = CrossBattleLibrary.GetCrossTimeKey(ZoneServerApi.now);
        //    //server.GameRedis.Call(new OperateUpdateCrossRankInfos(uid, seasonInfo.Id, group, CrossInfoMng.Info.TimeKey, newTime, CrossInfoMng.Info.Star));
        //    CrossInfoMng.Info.TimeKey = newTime;
        //}



        /// <summary>
        /// 高手殿堂
        /// </summary>
        /// <param name="page"></param>
        public void ShowCrossSeasonLeaderInfos()
        {
            MSG_ZR_SHOW_CROSS_SEASON_LEADER_INFO req = new MSG_ZR_SHOW_CROSS_SEASON_LEADER_INFO();
            server.SendToRelation(req, uid);
        }


        public void ShowCrossLeaderInfosMsg(MSG_RZ_SHOW_CROSS_LEADER_INFO msg)
        {
            MSG_ZGC_SHOW_CROSS_LEADER_INFO response = new MSG_ZGC_SHOW_CROSS_LEADER_INFO();
            //玩家
            foreach (var item in msg.List)
            {
                CROSS_BATTLE_RANK_INFO rankInfo = GetCrossRankInfo(item);
                response.List.Add(rankInfo);
            }
            Write(response);
        }

        /// <summary>
        /// 获取挑战者信息
        /// </summary>
        public void ShowCrossRankInfosMsg(MSG_RZ_SHOW_CROSS_RANK_INFO msg)
        {
            MSG_ZGC_CROSS_BATTLE_RANK_INFO_LIST response = new MSG_ZGC_CROSS_BATTLE_RANK_INFO_LIST();

            CROSS_BATTLE_RANK_INFO self = GetSelfCrossRankInfo(msg.Rank);
            response.OwnerInfo = self;
            response.Page = msg.Page;
            response.TotalCount = msg.TotalCount;
            //玩家
            foreach (var item in msg.List)
            {
                CROSS_BATTLE_RANK_INFO rankInfo = GetCrossRankInfo(item);
                response.List.Add(rankInfo);
            }
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private CROSS_BATTLE_RANK_INFO GetSelfCrossRankInfo(int rank)
        {
            CROSS_BATTLE_RANK_INFO self = new CROSS_BATTLE_RANK_INFO();
            self.BaseInfo = PlayerInfo.GetPlayerBaseInfo(this);
            self.Rank = rank;
            foreach (var kv in ArenaMng.DefensiveHeros)
            {
                self.Defensive.Add(kv.Key);
            }
            //self.Defensive.AddRange(ArenaMng.DefensiveHeros);
            self.CrossLevel = CrossInfoMng.Info.Level;
            self.CrossStar = CrossInfoMng.Info.Star;
            return self;
        }

        private CROSS_BATTLE_RANK_INFO GetCrossRankInfo(MSG_RZ_CROS_RANK_INFO item)
        {
            CROSS_BATTLE_RANK_INFO challengerInfo = new CROSS_BATTLE_RANK_INFO();
            challengerInfo.BaseInfo = GetPlayerBaseInfo(item);
            challengerInfo.Rank = item.Rank;
            challengerInfo.CrossLevel = item.CrossLevel;
            challengerInfo.CrossStar = item.CrossStar;
            challengerInfo.Defensive.AddRange(item.Defensive);
            return challengerInfo;
        }

        //public int UpdateCrossPower()
        //{
        //    int power = 0;
        //    foreach (var heroId in CrossInfoMng.Info.DefensiveHeros)
        //    {
        //        //判断是否拥有这个伙伴
        //        HeroInfo hero = HeroMng.GetHeroInfo(heroId);
        //        if (hero != null)
        //        {
        //            power += hero.GetBattlePower();
        //        }
        //    }
        //    HeroInfo mainHero = HeroMng.GetHeroInfo(HeroId);
        //    if (mainHero != null)
        //    {
        //        power += mainHero.GetBattlePower();
        //    }

        //    UpdatePlayerCrossPowerToRedis(power);

        //    return power;
        //}

        ///// <summary>
        ///// 保存反手阵容到Redis
        ///// </summary>
        ///// <param name="defensiveHeros"></param>
        //public void UpdatePlayerCrossDefensiveHerosToRedis(string defensiveHeros)
        //{
        //    server.GameRedis.Call(new OperateUpdateCrossDefensive(Uid, defensiveHeros));
        //}

        /// <summary>
        /// 保存反手阵容战力到Redis
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void UpdatePlayerCrossPowerToRedis(int power)
        {
            server.GameRedis.Call(new OperateUpdateCrossPower(Uid, power));
        }

        /// <summary>
        /// 保存段位奖励领取
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void SyncDbUpdateCrossBattleReward()
        {
            server.GameDBPool.Call(new QueryUpdateCrossBattleReward(Uid, CrossInfoMng.Info.ActiveReward, CrossInfoMng.Info.PreliminaryReward));
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void SyncDbUpdateCrossBattleResult()
        {
            server.GameDBPool.Call(new QueryUpdateCrossBattleResult(Uid, CrossInfoMng.Info));
        }

        /////// <summary>
        /////// 保存反手阵容到DB
        /////// </summary>
        /////// <param name="defensiveHeros"></param>
        ////public void SyncDbUpdateCrossBattleDefensiveHeros(string defensiveHeros)
        ////{
        ////    server.GameDBPool.Call(new QueryUpdateCrossBattleDefensive(Uid, defensiveHeros));
        ////}

        //public CROSS_CHALLENGER_INFO GetCrossRankBaseInfo(PlayerRankBaseInfo challenger)
        //{
        //    CROSS_CHALLENGER_INFO info = new CROSS_CHALLENGER_INFO();
        //    info.IsRobot = challenger.IsRobot;
        //    info.Defensive.AddRange(challenger.Defensive);
        //    //info.CrossLevel = challenger.CrossLevel;
        //    //info.CrossStar = challenger.CrossStar;
        //    info.BaseInfo = GetPlayerBaseInfo(challenger);
        //    if (info.IsRobot)
        //    {
        //        info.BaseInfo.MainId = server.MainId;
        //    }
        //    else
        //    {
        //        info.BaseInfo.MainId = BaseApi.GetMainIdByUid(challenger.Uid);
        //    }
        //    return info;
        //}

        private PLAYER_BASE_INFO GetPlayerBaseInfo(PlayerRankBaseInfo challenger)
        {
            PLAYER_BASE_INFO baseInfo = new PLAYER_BASE_INFO();
            baseInfo.Uid = challenger.Uid;
            baseInfo.Name = challenger.Name;
            baseInfo.Sex = challenger.Sex;
            baseInfo.Level = challenger.Level;
            baseInfo.LadderLevel = challenger.LadderLevel;
            baseInfo.HeroId = challenger.HeroId;
            baseInfo.GodType = challenger.GodType;

            baseInfo.Icon = challenger.Icon;
            baseInfo.IconFrame = challenger.IconFrame;
            baseInfo.ShowDIYIcon = false;
            if (challenger.IsRobot)
            {
                baseInfo.MainId = server.MainId;
            }
            else
            {
                baseInfo.MainId = BaseApi.GetMainIdByUid(challenger.Uid);
            }

            baseInfo.BattlePower64 = challenger.BattlePower;
            if (challenger.BattlePower < int.MaxValue)
            {
                baseInfo.BattlePower = (int)challenger.BattlePower;
            }

            return baseInfo;
        }


        public PLAYER_BASE_INFO GetPlayerBaseInfo(PlayerBaseInfo player)
        {
            PLAYER_BASE_INFO baseInfo = new PLAYER_BASE_INFO();
            baseInfo.Uid = player.Uid;
            baseInfo.Name = player.Name;
            baseInfo.Sex = player.Sex;
            baseInfo.Level = player.Level;
            baseInfo.LadderLevel = player.LadderLevel;
            baseInfo.BattlePower = player.BattlePower;
            baseInfo.HeroId = player.HeroId;
            baseInfo.GodType = player.GodType;

            baseInfo.Icon = player.Icon;
            baseInfo.IconFrame = player.IconFrame;
            baseInfo.ShowDIYIcon = false;
            baseInfo.MainId = BaseApi.GetMainIdByUid(player.Uid);

            return baseInfo;
        }

        public PLAYER_BASE_INFO GetPlayerBaseInfo(MSG_RZ_CROS_RANK_INFO sinfo)
        {
            PLAYER_BASE_INFO info = new PLAYER_BASE_INFO();
            info.Uid = sinfo.Uid;
            info.Name = sinfo.Name;
            info.Icon = sinfo.Icon;
            info.ShowDIYIcon = sinfo.ShowDIYIcon;
            info.IconFrame = sinfo.IconFrame;

            info.Level = sinfo.Level;
            info.Sex = sinfo.Sex;
            info.HeroId = sinfo.HeroId;
            info.GodType = sinfo.GodType;
            //info.LadderLevel = sinfo.LadderLevel;
            info.MainId = BaseApi.GetMainIdByUid(sinfo.Uid);

            if (sinfo.BattlePower < int.MaxValue)
            {
                info.BattlePower = (int)sinfo.BattlePower;
            }
            info.BattlePower64 = sinfo.BattlePower;

            return info;
        }




        ///// <summary>
        ///// 获取我的排名
        ///// </summary>
        //public void GetCrossSeasonRank()
        //{
        //    CrossSeasonInfo info = CrossBattleLibrary.GetCrossSeasonInfoByTime(ZoneServerApi.now);
        //    if (info == null)
        //    {
        //        Log.Warn("player {0} GetCrossPreliminaryChallenger failed: no such season", uid);
        //        return;
        //    }
        //    int group = CrossBattleLibrary.GetCrossGroup(server.MainId);
        //    //获取赛季排行榜
        //    OperateGetCrossRankByScore operate = new OperateGetCrossRankByScore(uid, info.Id, group, CrossInfoMng.Info.TimeKey);
        //    server.GameRedis.Call(operate, ret =>
        //    {
        //        if ((int)ret == 1)
        //        {
        //            if (operate.CrossRank > CrossBattleLibrary.RankMax)
        //            {
        //                CrossInfoMng.ChargeRank(0);
        //            }
        //            else
        //            {
        //                CrossInfoMng.ChargeRank(operate.CrossRank);
        //            }
        //        }
        //        else
        //        {
        //            Log.Warn("player {0} GetCrossSeasonRank execute OperateGetCrossRankByScore fail: redis data error!", uid);
        //            return;
        //        }
        //    });
        //}
        ///// <summary>
        ///// 排行榜
        ///// </summary>
        ///// <param name="page"></param>
        //public void ShowCrossRankInfos(int page)
        //{
        //    MSG_ZR_SHOW_CROSS_RANK_INFO req = new MSG_ZR_SHOW_CROSS_RANK_INFO();
        //    req.Page = page;
        //    server.SendToRelation(req, uid);
        //}

        ///// <summary>
        ///// 保存防守阵容
        ///// </summary>
        ///// <param name="heroIds"></param>
        //public void SaveCrossBattleDefensiveHeros(RepeatedField<int> heroIds)
        //{
        //    MSG_ZGC_SAVE_CROSS_BATTLE_DEFEMSIVE response = new MSG_ZGC_SAVE_CROSS_BATTLE_DEFEMSIVE();

        //    if (heroIds.Count > 3)
        //    {
        //        Log.Warn("player {0} save Cross Battle defensive failed: save hero count is {1}", uid, heroIds.Count);
        //        response.Result = (int)ErrorCode.MaxCount;
        //        Write(response);
        //        return;
        //    }

        //    int power = 0;
        //    if (heroIds.Count > 0)
        //    {
        //        bool isSame = true;
        //        for (int i = 0; i < heroIds.Count; i++)
        //        {
        //            int heroId = heroIds[i];

        //            //判断是否拥有这个伙伴
        //            HeroInfo hero = HeroMng.GetHeroInfo(heroId);
        //            if (hero == null)
        //            {
        //                Log.Warn("player {0} save Cross Battle defensive failed: no such hero {1}", uid, heroId);
        //                response.Result = (int)ErrorCode.NoHeroInfo;
        //                Write(response);
        //                return;
        //            }

        //            //判断是否跟保存的伙伴相同
        //            int saveHeroId = CrossInfoMng.GetDefensiveHeroByIndex(i);
        //            if (heroId != saveHeroId)
        //            {
        //                //有不同，就可以保存修改
        //                isSame = false;
        //            }

        //            //检查是否有重复阵容
        //            for (int j = i + 1; j < heroIds.Count; j++)
        //            {
        //                if (heroId == heroIds[j])
        //                {
        //                    Log.Warn("player {0} save Cross Battle defensive failed: same hero {1}", uid, heroId);
        //                    response.Result = (int)ErrorCode.SaveSameHeroId;
        //                    Write(response);
        //                    return;
        //                }
        //            }
        //            power += hero.GetBattlePower();
        //        }

        //        if (isSame)
        //        {
        //            Log.Write("player {0} save Cross Battle defensive is same", uid);
        //            response.Result = (int)ErrorCode.Success;
        //            Write(response);
        //            return;
        //        }
        //    }

        //    HeroInfo mainHero = HeroMng.GetHeroInfo(HeroId);
        //    if (mainHero == null)
        //    {
        //        Log.Warn("player {0} save Cross Battle defensive failed: no such hero {1}", uid, HeroId);
        //        response.Result = (int)ErrorCode.NoHeroInfo;
        //        Write(response);
        //        return;
        //    }
        //    power += mainHero.GetBattlePower();

        //    //清理旧的配置
        //    CrossInfoMng.ClearDefensiveHero();
        //    //设置新的配置
        //    foreach (var heroId in heroIds)
        //    {
        //        CrossInfoMng.AddDefensiveHero(heroId);
        //    }

        //    //同步
        //    string defensiveHeros = CrossInfoMng.GetDefensiveHeros();

        //    //保存DB
        //    SyncDbUpdateCrossBattleDefensiveHeros(defensiveHeros);

        //    //保存Redis
        //    UpdatePlayerCrossDefensiveHerosToRedis(defensiveHeros);
        //    UpdatePlayerCrossPowerToRedis(power);
        //    response.DefensiveHeros.AddRange(CrossInfoMng.Info.DefensiveHeros);
        //    response.Result = (int)ErrorCode.Success;
        //    Write(response);
        //}


    }
}
