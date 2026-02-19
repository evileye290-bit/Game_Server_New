using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using DBUtility;
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

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        //魂师擂台挑战赛
        public CrossChallengeInfoManager CrossChallengeInfoMng { get; set; }

        public void InitCrossChallengeManager()
        {
            CrossChallengeInfoMng = new CrossChallengeInfoManager(this, server);
        }

        /// <summary>
        /// 通知跨服战具体信息
        /// </summary>
        public void SendCrossChallengeManagerMessage()
        {
            if (server.CrossChallengeMng.FirstStartTime > 0)
            {
                SyncCrossChallengeManagerMessage();
            }
            else
            {
                //通知Relation获取
                MSG_ZR_GET_CROSS_CHALLENGE_START req = new MSG_ZR_GET_CROSS_CHALLENGE_START();
                server.SendToRelation(req, uid);
            }
        }

        public void SyncCrossChallengeManagerMessage()
        {
            MSG_ZGC_CROSS_CHALLENGE_MANAGER info = new MSG_ZGC_CROSS_CHALLENGE_MANAGER();
            info.Rank = CrossChallengeInfoMng.Info.Rank;
            info.CrossLevel = CrossChallengeInfoMng.Info.Level;
            info.CrossStar = CrossChallengeInfoMng.Info.Star;
            info.ActiveReward = CrossChallengeInfoMng.Info.ActiveReward;
            info.PreliminaryReward = CrossChallengeInfoMng.Info.PreliminaryReward;
            info.DailyFight = CrossChallengeInfoMng.Info.DailyFight;
            info.SeasonFight = CrossChallengeInfoMng.Info.SeasonFight;
            info.ServerReward = CrossChallengeInfoMng.Info.ServerReward;
            info.StartTime = server.CrossChallengeMng.FirstStartTime;
            info.OpenTeam = server.CrossChallengeMng.TeamId;
            info.BattleTeam = CrossChallengeInfoMng.Info.BattleTeam;

            info.BossStateReward = CrossBossInfoMng.CounterInfo.PassReward;
            info.BossRankReward = CrossBossInfoMng.CounterInfo.Score;

            Write(info);

            GetCrossChallengeGuessingInfo();
        }

        /// <summary>
        /// 领取活跃奖励
        /// </summary>
        /// <param name="heroIds"></param>
        public void GetCrossChallengeActiveReward()
        {
            MSG_ZGC_GET_CROSS_CHALLENGE_ACTIVE_REWARD response = new MSG_ZGC_GET_CROSS_CHALLENGE_ACTIVE_REWARD();

            CrossLevelInfo info = CrossChallengeLibrary.CheckCrossLevel(CrossChallengeInfoMng.Info.Star);
            if (info == null)
            {
                Log.Warn("player {0} GetCrossActiveReward failed: no level info {1}", uid, CrossChallengeInfoMng.Info.Star);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //判断时间
            if (!CrossChallengeLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossChallengeMng.StartTime, server.Now()))
            {
                Log.Warn("player {0} GetCrossActiveReward failed: season preliminary time error", uid);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (CrossChallengeLibrary.ActiveNum > CrossChallengeInfoMng.Info.DailyFight)
            {
                Log.Warn("player {0} GetCrossActiveReward failed: DailyFight is {1}", uid, CrossChallengeInfoMng.Info.DailyFight);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (CrossChallengeInfoMng.Info.ActiveReward != (int)(int)CrossRewardState.None)
            {
                Log.Warn("player {0} GetCrossActiveReward failed: ActiveReward is {1}", uid, CrossChallengeInfoMng.Info.ActiveReward);
                response.Result = (int)ErrorCode.Already;
                Write(response);
                return;
            }

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(info.ActiveReward);
            AddRewards(rewards, ObtainWay.CrossActivityReward);

            //清理旧的配置
            CrossChallengeInfoMng.GetActivityReward();

            //保存DB
            SyncDbUpdateCrossChallengeReward();

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.ActiveReward = CrossChallengeInfoMng.Info.ActiveReward;
            Write(response);
        }

        /// <summary>
        /// 领取海选奖励
        /// </summary>
        /// <param name="heroIds"></param>
        public void GetCrossChallengePreliminaryReward()
        {
            MSG_ZGC_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD response = new MSG_ZGC_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD();

            CrossLevelInfo info = CrossChallengeLibrary.CheckCrossLevel(CrossChallengeInfoMng.Info.Star);
            if (info == null)
            {
                Log.Warn("player {0} GetCrossChallengePreliminaryReward failed: no level info {1}", uid, CrossChallengeInfoMng.Info.Star);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //判断时间
            if (!CrossChallengeLibrary.CheckWeekTime(CrossTimeCheck.Finals, server.CrossChallengeMng.StartTime, server.Now()))
            {
                Log.Warn("player {0} GetCrossChallengePreliminaryReward failed: season preliminary time error", uid);
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (CrossChallengeInfoMng.Info.SeasonFight <= 0)
            {
                Log.Warn("player {0} GetCrossChallengePreliminaryReward failed: SeasonFight is {1}", uid, CrossChallengeInfoMng.Info.SeasonFight);
                response.Result = (int)ErrorCode.NoCrossPreliminary;
                Write(response);
                return;
            }

            if (CrossChallengeInfoMng.Info.PreliminaryReward != (int)CrossRewardState.None)
            {
                Log.Warn("player {0} GetCrossChallengePreliminaryReward failed: PreliminaryReward is {1}", uid, CrossChallengeInfoMng.Info.PreliminaryReward);
                response.Result = (int)ErrorCode.Already;
                Write(response);
                return;
            }

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(info.PreliminaryReward);
            AddRewards(rewards, ObtainWay.CrossPreliminaryReward);

            //清理旧的配置
            CrossChallengeInfoMng.GetPreliminaryReward();

            //保存DB
            SyncDbUpdateCrossChallengeReward();

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.PreliminaryReward = CrossChallengeInfoMng.Info.PreliminaryReward;
            Write(response);
        }

        /// <summary>
        /// 领取全服奖励
        /// </summary>
        /// <param name="heroIds"></param>
        public void GetCrossChallengeServerReward()
        {
            MSG_ZGC_GET_CROSS_CHALLENGE_SERVER_REWARD response = new MSG_ZGC_GET_CROSS_CHALLENGE_SERVER_REWARD();

            if (CrossChallengeInfoMng.Info.ServerReward != (int)CrossRewardState.None)
            {
                Log.Warn("player {0} GetCrossChallengePreliminaryReward failed: PreliminaryReward is {1}", uid, CrossChallengeInfoMng.Info.ServerReward);
                response.Result = (int)ErrorCode.Already;
                Write(response);
                return;
            }

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(CrossChallengeLibrary.ServerReward);
            AddRewards(rewards, ObtainWay.CrossServerReward);

            //清理旧的配置
            CrossChallengeInfoMng.GetServerReward();

            //保存DB
            server.GameDBPool.Call(new QueryUpdateCrossChallengeServerReward(Uid, CrossChallengeInfoMng.Info.ServerReward));

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.ServerReward = CrossChallengeInfoMng.Info.ServerReward;
            Write(response);
        }

        /// <summary>
        /// 获取下注信息
        /// </summary>
        public void GetCrossChallengeGuessingInfo()
        {
            MSG_ZR_GET_CROSS_CHALLENGE_GUESSING_INFO response = new MSG_ZR_GET_CROSS_CHALLENGE_GUESSING_INFO();
            server.SendToRelation(response, uid);
        }

        /// <summary>
        /// 获取下注信息
        /// </summary>
        public void CrossChallengeGuessingChoose(int choose)
        {
            //判断时间是否正确
            if (!CrossChallengeLibrary.CheckWeekTime(CrossTimeCheck.GuessingTime, server.CrossChallengeMng.StartTime, server.Now()))
            {
                MSG_ZGC_CROSS_CHALLENGE_GUESSING_CHOOSE msg = new MSG_ZGC_CROSS_CHALLENGE_GUESSING_CHOOSE();
                msg.Choose = choose;
                Log.Warn("player {0} CrossGuessingChoose failed: time error", uid);
                msg.Result = (int)ErrorCode.CrossChallengeGuessingNotOpen;
                Write(msg);
                return;
            }

            //获取当前时间ID
            CrossBattleTiming endGuessing = CrossChallengeLibrary.GetCurrentGuessingTime(server.CrossChallengeMng.StartTime, server.Now());
            if (endGuessing == CrossBattleTiming.Start)
            {
                MSG_ZGC_CROSS_CHALLENGE_GUESSING_CHOOSE msg = new MSG_ZGC_CROSS_CHALLENGE_GUESSING_CHOOSE();
                msg.Choose = choose;
                Log.Warn("player {0} CrossGuessingChoose failed: not find guessing time", uid);
                msg.Result = (int)ErrorCode.CrossChallengeGuessingNotOpen;
                Write(msg);
                return;
            }

            CrossBattleTiming timing = CrossChallengeLibrary.GetCrossBattleTiming(endGuessing);

            MSG_ZR_CROSS_CHALLENGE_GUESSING_CHOOSE response = new MSG_ZR_CROSS_CHALLENGE_GUESSING_CHOOSE();
            response.Choose = choose;
            response.TimingId = (int)timing;
            server.SendToRelation(response, uid);
        }

        public void CrossChallengeGuessingChoose(int errorCode, int timingId, int choose, bool hasReward)
        {
            MSG_ZGC_CROSS_CHALLENGE_GUESSING_CHOOSE msg = new MSG_ZGC_CROSS_CHALLENGE_GUESSING_CHOOSE();
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
                int num = CrossChallengeLibrary.GuessingOnhookReward / OnhookLibrary.RewardTime;
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
                float rewardCount = coustGold * CrossChallengeLibrary.GuessingOnhookRatio;
                reward = string.Format("{0}:{1}:{2}", rewardItem[0], rewardItem[1], (int)rewardCount);
            }

            if (!hasReward)
            {
                MSG_ZR_CROSS_CHALLENGE_GUESSING_REWARD response = new MSG_ZR_CROSS_CHALLENGE_GUESSING_REWARD();
                response.Choose = choose;
                response.TimingId = timingId;
                response.Reward = reward;
                server.SendToRelation(response, uid);
            }

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void CrossChallengeSwapQueue(int queue1, int queue2)
        {
            MSG_ZGC_CROSS_CHALLENGE_SWAP_QUEUE msg = new MSG_ZGC_CROSS_CHALLENGE_SWAP_QUEUE();

            Dictionary<int, HeroInfo> queueHeroInfos1;
            Dictionary<int, HeroInfo> queueHeroInfos2;
            HeroMng.CrossChallengeQueue.TryGetValue(queue1, out queueHeroInfos1);
            HeroMng.CrossChallengeQueue.TryGetValue(queue2, out queueHeroInfos2);

            Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();

            if (queueHeroInfos1 != null && queueHeroInfos2 != null)
            {
                HeroMng.CrossChallengeQueue[queue1] = queueHeroInfos2;
                HeroMng.CrossChallengeQueue[queue2] = queueHeroInfos1;
                foreach (var kv in queueHeroInfos1.Values.ToList())
                {
                    updateList.Add(kv.Id, kv);
                    kv.CrossChallengeQueueNum = queue2;
                }
                foreach (var kv in queueHeroInfos2.Values.ToList())
                {
                    updateList.Add(kv.Id, kv);
                    kv.CrossChallengeQueueNum = queue1;
                }
            }

            if (updateList.Count > 0)
            {
                List<HeroInfo> list = new List<HeroInfo>();
                foreach (var kv in updateList)
                {
                    SyncDbUpdateHeroItemCrossChallengeQueue(kv.Value);
                    list.Add(kv.Value);
                }
                SyncHeroChangeMessage(list);

                TrackDungeonQueueLog(HeroQueueType.CrossChallenge, updateList);
            }

            msg.Result = (int) ErrorCode.Success;
            Write(msg);
        }

        public void CrossChallengeSwapHero(MSG_GateZ_CROSS_CHALLENGE_SWAP_HERO pks)
        {
            MSG_ZGC_CROSS_CHALLENGE_SWAP_HERO msg = new MSG_ZGC_CROSS_CHALLENGE_SWAP_HERO();

            if (pks.SwapHero.Count != 2)
            {
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            Dictionary<int, HeroInfo> queueHeroInfos1;
            Dictionary<int, HeroInfo> queueHeroInfos2;
            if (!HeroMng.CrossChallengeQueue.TryGetValue(pks.SwapHero[0].Queue, out queueHeroInfos1) ||
                !HeroMng.CrossChallengeQueue.TryGetValue(pks.SwapHero[1].Queue, out queueHeroInfos2))
            {
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }


            HeroInfo heroInfo1 = queueHeroInfos1.Values.FirstOrDefault(x => x.Id == pks.SwapHero[0].HeroId);
            HeroInfo heroInfo2 = queueHeroInfos2.Values.FirstOrDefault(x => x.Id == pks.SwapHero[1].HeroId);
            if (heroInfo1== null || heroInfo2 == null)
            {
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            int pos1 = heroInfo1.CrossChallengePositionNum;
            int pos2 = heroInfo2.CrossChallengePositionNum;
            if (queueHeroInfos1.ContainsKey(pos1) && queueHeroInfos2.ContainsKey(pos2))
            {
                queueHeroInfos1[pos1] = heroInfo2;
                queueHeroInfos2[pos2] = heroInfo1;

                heroInfo1.CrossChallengeQueueNum = pks.SwapHero[1].Queue;
                heroInfo2.CrossChallengeQueueNum = pks.SwapHero[0].Queue;

                heroInfo1.CrossChallengePositionNum = pos2;
                heroInfo2.CrossChallengePositionNum = pos1;

                Dictionary<int, HeroInfo> updateList = new Dictionary<int, HeroInfo>();
                updateList.Add(heroInfo1.Id, heroInfo1);
                updateList.Add(heroInfo2.Id, heroInfo2);

                if (updateList.Count > 0)
                {
                    List<HeroInfo> list = new List<HeroInfo>();
                    foreach (var kv in updateList)
                    {
                        SyncDbUpdateHeroItemCrossChallengeQueue(kv.Value);
                        list.Add(kv.Value);
                    }
                    SyncHeroChangeMessage(list);

                    TrackDungeonQueueLog(HeroQueueType.CrossChallenge, updateList);
                }
            }



            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        /// <summary>
        /// 更新排行榜
        /// </summary>
        /// <param name="rank"></param>
        public void UpdateCrossChallengeSeasonRank(int rank)
        {
            if (rank != CrossChallengeInfoMng.Info.Rank)
            {
                CrossChallengeInfoMng.ChargeRank(rank);
            }
        }

        public void UpdateCrossChallengeTeamId(int teamId)
        {
            CrossChallengeInfoMng.ChargeBattleTeam(teamId);

            SendCrossChallengeManagerMessage();
        }

        /// <summary>
        /// 获取海选对战者
        /// </summary>
        /// <param name="page"></param>
        public void GetCrossChallengePreliminaryChallenger()
        {
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            CrossLevelInfo info = CrossChallengeLibrary.CheckCrossLevel(CrossChallengeInfoMng.Info.Star);
            if (info == null)
            {
                Log.Warn($"player {uid} GetCrossChallengePreliminaryChallenger failed: no level info {CrossChallengeInfoMng.Info.Star}");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //判断时间
            if (!CrossChallengeLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossChallengeMng.StartTime, server.Now()))
            {
                Log.Warn($"player {uid} GetCrossChallengePreliminaryChallenger failed: season preliminary time {server.CrossChallengeMng.StartTime} error");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //Redis 获取列表
            CrossLevelInfo levelInfo = CrossChallengeLibrary.GetCrossLevelInfo(CrossChallengeInfoMng.Info.Level);
            if (levelInfo == null)
            {
                Log.Warn($"player {uid} GetCrossChallengePreliminaryChallenger failed: preliminary level {CrossChallengeInfoMng.Info.Level} error");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (HeroMng.CrossChallengeQueue.Count != CrossChallengeLibrary.CrossQueueCount)
            {
                Log.Warn($"player {uid} GetCrossChallengePreliminaryChallenger failed: queue {HeroMng.CrossQueue.Count} error");
                response.Result = (int)ErrorCode.CrossChallengeQueenNotSet;
                Write(response);
                return;
            }

            if (CrossChallengeInfoMng.LastBattlePlayerInfo != null)
            {
                EnterCrossChallengeMap(CrossChallengeInfoMng.LastBattlePlayerInfo);
            }
            else
            {
                //获取赛季排行榜
                OperateGetRankByScore operate = new OperateGetRankByScore(RankType.CrossChallenge, Uid, server.MainId,
                    CrossChallengeInfoMng.Info.Star, levelInfo.MinNum, levelInfo.CheckRange);
                server.GameRedis.Call(operate, ret =>
                {
                    if ((int)ret == 1)
                    {
                        if (operate.uidRank == null || operate.uidRank.Count == 0)
                        {
                            //使用机器人
                            RobotEnterCrossChallengeMap();
                            return;
                        }
                        else
                        {
                            int index = NewRAND.Next(0, operate.uidRank.Count - 1);
                            if (index < operate.uidRank.Count)
                            {
                                int uid = operate.uidRank[index];
                                //获取玩家信息
                                GetCrossChallengeChallengerInfo(uid);
                            }
                            else
                            {
                                //如果人数不足使用机器人补足匹配 
                                RobotEnterCrossChallengeMap();
                            }
                            return;
                        }
                    }
                    else
                    {
                        Log.Error("LoadRankInfoFromRedis execute OperateGetCrossChallengeRankInfos fail: redis data error!");
                        return;
                    }
                });
            }
        }

        private void RobotEnterCrossChallengeMap()
        { 
            //机器人，直接读表
            CrossRobotInfo robotInfo = RobotLibrary.GetCrossChallengeRobotInfo(CrossChallengeInfoMng.Info.Star);
            if (robotInfo != null)
            {
                PlayerCrossFightInfo rankInfo = GetCrossChallengeRobotInfo(robotInfo);
                if (rankInfo != null)
                {
                    rankInfo.Type = ChallengeIntoType.CrossChallengePreliminary;
                    EnterCrossChallengeMap(rankInfo);
                }
                else
                {
                    Log.WarnLine("player {0} show arena challenger info failed: not find rank info ", Uid);
                    //return;
                }
            }
            else
            {
                Log.WarnLine("player {0} GetCrossArenaRobotInfo failed: not find rank info star {1}", Uid, CrossChallengeInfoMng.Info.Star);
                //return;
            }
        }

        /// <summary>
        /// 进入跨服挑战
        /// </summary>
        public void EnterCrossChallengeMap(PlayerCrossFightInfo fightInfo)
        {
            //到Relation获取一个对手，最后获得对手信息后才开始战斗
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            int dungeonId = CrossChallengeLibrary.MapId;
            response.DungeonId = dungeonId;
            if (fightInfo == null)
            {
                Log.WarnLine("player {0} enter cross battle map failed: not find rank info", Uid);
                response.Result = (int)ErrorCode.NotFindChallengerInfo;
                Write(response);
                return;
            }

            if (CurDungeon?.DungeonResult == DungeonResult.None)
            {
                Log.WarnLine($"player {uid} enter cross battle map failed: request create dungeon {dungeonId} repeat");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            //GetArenaRobotInfo
            if (fightInfo.Type != ChallengeIntoType.CrossChallengeFinals)
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
                dungeonId = 9007;
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

            CrossChallengeInfoMng.LastBattlePlayerInfo = fightInfo;
            CrossChallengeInfoMng.CrossChallengeLastBattleUid = fightInfo.Uid;

            if (dungeon.Model.MapType == MapType.CrossChallenge)
            {
                //记录原来坐在地图、位置
                if (currentMap?.GetMapType() == MapType.Map)
                {
                    CrossChallengeInfoMng.SetOriginalMapInfo(currentMap.MapId, currentMap.Channel, MoveHandler.CurPosition);
                }

                if (CrossChallengeInfoMng.BattleRound == 1)
                {
                    //对手基本信息
                    Write(GetCrossChallengeChallengerMsg(fightInfo));
                }
            }

            //进入当前轮战斗
            CrossChallengeEnterDungeon(dungeon, fightInfo, CrossChallengeInfoMng.BattleRound);
        }

        public void CrossChallengeEnterDungeon(DungeonMap dungeon, PlayerCrossFightInfo fightInfo, int round)
        {
            CrossChallengeDungeonMap CrossChallengeDungeonMap = dungeon as CrossChallengeDungeonMap;
            CrossChallengeDungeonMap.BattleFpsManager?.SetBattleInfo(this, fightInfo);
            CrossChallengeDungeonMap.SetChallengeIntoType(fightInfo, Uid);

            //当前轮次战斗没有上阵英雄则直接失败
            if (!HeroMng.CrossChallengeQueue.ContainsKey(round))
            {
                SendCrossChallengeResult(DungeonResult.Failed);
                return;
            }

            //当前轮次战斗防守方没有上阵英雄则直接胜利
            if (!fightInfo.HeroQueue.ContainsKey(round))
            {
                SendCrossChallengeResult(DungeonResult.Success);
                return;
            }

            long battlePower = HeroMng.GetBattlePower64(HeroQueueType.CrossChallenge);

            //战力压制
            dungeon.SetBattlePowerSuppress(battlePower, fightInfo.GetBattlePower());

            //设置当前战斗轮次
            CrossChallengeDungeonMap.SetBattleRound(round);

            if (battlePower > fightInfo.BattlePower)
            {
                //添加自己
                CrossChallengeDungeonMap.AddAttackerMirror(this);
                //添加对手
                CrossChallengeDungeonMap.AddCrossDefender(fightInfo);
            }
            else
            {
                //添加对手
                CrossChallengeDungeonMap.AddCrossDefender(fightInfo);
                //添加自己
                CrossChallengeDungeonMap.AddAttackerMirror(this);
            }

            if (dungeon.Model.MapType == MapType.CrossChallenge)
            {
                // 成功 进入副本
                RecordEnterMapInfo(CrossChallengeDungeonMap.MapId, CrossChallengeDungeonMap.Channel, CrossChallengeDungeonMap.BeginPosition);
                RecordOriginMapInfo();
                OnMoveMap();
            }
        }

        /// <summary>
        /// 进入下一场战斗
        /// </summary>
        /// <param name="fightInfo"></param>
        /// <param name="dungeonId"></param>
        /// <param name="round"></param>
        public void CrossChallengeGotoNextRound(PlayerCrossFightInfo fightInfo, int dungeonId, int round)
        {
            // 在当前zone创建副本
            DungeonMap dungeon = server.MapManager.CreateDungeon(dungeonId);
            if (dungeon == null)
            {
                Log.Write($"player {Uid} enter cross challenge map request to create dungeon {dungeonId} failed: create dungeon failed");
                return;
            }

            CrossChallengeEnterDungeon(dungeon, fightInfo, round);
        }

        /// <summary>
        /// 决赛信息
        /// </summary>
        /// <param name="page"></param>
        public void ShowCrossChallengeFinalsInfo(int teamId)
        {
            MSG_ZR_SHOW_CROSS_CHALLENGE_BATTLE_FINALS req = new MSG_ZR_SHOW_CROSS_CHALLENGE_BATTLE_FINALS();
            req.TeamId = teamId;
            server.SendToRelation(req, uid);
        }

        public void ShowCrossChallengeChallenger(int showUid, int mainId)
        {
            MSG_ZR_SHOW_CROSS_CHALLENGE_CHALLENGER req = new MSG_ZR_SHOW_CROSS_CHALLENGE_CHALLENGER();
            req.Uid = showUid;
            req.MainId = mainId;
            server.SendToRelation(req, uid);
        }

        public void GetCrossChallengeVideo(int team, int vedio, int index)
        {
            MSG_ZR_GET_CROSS_CHALLENGE_VIDEO req = new MSG_ZR_GET_CROSS_CHALLENGE_VIDEO();
            req.TeamId = team;
            req.VedioId = vedio;
            req.Index = index;
            server.SendToRelation(req, uid);
        }

        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="result">DungeonResult result, ArenaRankInfo rankInfo</param>
        /// <param name="rankInfo"></param>
        public void SendCrossChallengeResult(DungeonResult result)
        {
            //由于跨服挑战是连续战斗，会存在在副本中供进入另外一个副本，当第二次进入副本的时候原来的副本信息会丢失，
            //所以需要重新设置，战斗之前所在的地图信息
            if (CrossChallengeInfoMng.BattleRound >= 3)
            {
                if (CrossChallengeInfoMng.EnterMapInfo == null)
                {
                    MapModel mapModel = MapLibrary.GetMap(CONST.MAIN_MAP_ID);
                    CrossChallengeInfoMng.SetOriginalMapInfo(CONST.MAIN_MAP_ID, CONST.MAIN_MAP_CHANNEL, mapModel.BeginPos);
                }

                //更新挑战次数
                UpdateCounter(CounterType.CrossChallengeCount, -1);

                RecordEnterMapInfo(CrossChallengeInfoMng.EnterMapInfo.MapId, CrossChallengeInfoMng.EnterMapInfo.Channel, CrossChallengeInfoMng.EnterMapInfo.Position);
                OriginMapInfo.SetInfo(CrossChallengeInfoMng.EnterMapInfo.MapId, CrossChallengeInfoMng.EnterMapInfo.Channel, CrossChallengeInfoMng.EnterMapInfo.Position);
            }

            //增加轮次
            CrossChallengeInfoMng.AddBattleRound();

            string reward;
            int oldScore = CrossChallengeInfoMng.Info.Star;
            RewardManager rewards = new RewardManager();

            if (result == DungeonResult.Success)
            {
                CrossChallengeInfoMng.WinCount++;

                 CrossLevelInfo levelInfo = CrossChallengeLibrary.GetCrossLevelInfo(CrossChallengeInfoMng.Info.Level);
                 if (levelInfo == null)
                 {
                     levelInfo = CrossChallengeLibrary.GetCrossLevelInfo(1);
                }

                 //奖励
                 rewards.InitSimpleReward(levelInfo.WinReward);

                 RewardDropItemList dropItemList = RewardDropLibrary.GetRewardDropItems(levelInfo.WinRewardDrop);
                 if (dropItemList != null)
                 {
                     List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(dropItemList, (int)Job);
                    rewards.AddReward(items);
                }
            }
            else
            {
                CrossChallengeInfoMng.LoseCount++;
            }

            //判断时间 超过海选时间不加星星
            if (CrossChallengeLibrary.CheckWeekTime(CrossTimeCheck.Preliminary, server.CrossChallengeMng.StartTime, server.Now()))
            {
                CrossChallengeCheckWin();
            }

            rewards.BreakupRewards();
            AddRewards(rewards, ObtainWay.CrossPreliminaryResult);

            //通知前端奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = GetRewardSyncMsg(rewards);
            rewardMsg.DungeonId = CrossChallengeLibrary.MapId;
            rewardMsg.Result = (int)result;

            CheckCacheRewardMsg(rewardMsg);
        }

        private void CrossChallengeCheckWin()
        {
            //战斗完三场做结算
            if (CrossChallengeInfoMng.BattleRound > CrossChallengeLibrary.BattleRound)
            {
                bool isWin;
                int addStar = CrossChallengeLibrary.AddStar(CrossChallengeInfoMng.WinCount, CrossChallengeInfoMng.LoseCount, out isWin);
                if (isWin)
                {
                    CrossChallengeInfoMng.Info.WinTotal++;
                }

                CrossChallengeInfoMng.AddStar(isWin, addStar);
                CrossChallengeInfoMng.AddFightCount();

                CrossChallengeInfoMng.ResetBattleRound();

                //保存DB
                SyncDbUpdateCrossChallengeResult();
            }
        }

        private void CrossChallengePrimarySetUnFightFail()
        {
            if (CrossChallengeInfoMng.EnterMapInfo != null)
            {
                Log.Info("CrossChallengePrimarySetUnFightFail");
                RecordEnterMapInfo(CrossChallengeInfoMng.EnterMapInfo.MapId, CrossChallengeInfoMng.EnterMapInfo.Channel, CrossChallengeInfoMng.EnterMapInfo.Position);
                OriginMapInfo.SetInfo(CrossChallengeInfoMng.EnterMapInfo.MapId, CrossChallengeInfoMng.EnterMapInfo.Channel, CrossChallengeInfoMng.EnterMapInfo.Position);
            }

            if (CrossChallengeInfoMng.LastBattlePlayerInfo != null)
            {
                Log.Info("CrossChallengePrimarySetUnFightFail kick player");
                CurDungeon?.Stop(DungeonResult.Failed);
                CurDungeon?.Close();

                OnMoveMap();

                //更新挑战次数
                UpdateCounter(CounterType.CrossChallengeCount, -1);
                CrossChallengeInfoMng.LoseCount += (3 - CrossChallengeInfoMng.WinCount - CrossChallengeInfoMng.LoseCount);
                CrossChallengeInfoMng.SetBattleRound(4);
                CrossChallengeCheckWin();
            }
            CrossChallengeInfoMng.ResetBattleRound();
        }

        private void GetCrossChallengeChallengerInfo(int challengerUid)
        {
            //加载玩家信息，先找同服务器玩家
            PlayerChar challenger = server.PCManager.FindPc(challengerUid);
            if (challenger == null)
            {
                challenger = server.PCManager.FindOfflinePc(challengerUid);
                if (challenger == null)
                {
                    //Log.WarnLine("player {0} show player info fail,can not find player {1}.", Uid, showPcUid);
                    //没找到玩家，去relation获取信息
                    GetCrossChallengeChallengerInfoByRelation(challengerUid);
                    return;
                }
            }

            PlayerCrossFightInfo rankInfo = challenger.GetCrossChallengeRobotInfo();
            rankInfo.Type = ChallengeIntoType.CrossChallengePreliminary;
            //进入战斗
            EnterCrossChallengeMap(rankInfo);
        }

        public ZR_BattlePlayerMsg GetBattleChallengePlayerInfoMsg()
        {
            ZR_BattlePlayerMsg response = new ZR_BattlePlayerMsg();
            response.Uid = Uid;
            response.MainId = server.MainId;
            //基本信息
            response.BaseInfo.AddRange(GetCrossChallengeZRHFPlayerMsg());
            long power = 0;
            //伙伴信息
            foreach (var kv in HeroMng.CrossChallengeQueue)
            {
                foreach (var hero in kv.Value)
                {
                    //ZR_Hero_Info zRInfo = GetZRHeroInfo(hero.Value);
                    ZR_Show_HeroInfo zRInfo = GetZrPlayerHeroInfoMsg(hero.Value, HeroQueueType.CrossChallenge);
                    response.Heros.Add(zRInfo);
                    power += zRInfo.Power;
                }
            }

            if (power <= int.MaxValue)
            {
                response.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.BattlePower, (int)power));
            }
            response.BaseInfo.Add(SetBaseInfoItem(HFPlayerInfo.BattlePower64, power));

            response.NatureValues.Add(NatureValues);
            response.NatureRatios.Add(NatureRatios);
            return response;
        }

        private List<ZR_HFPlayerBaseInfoItem> GetCrossChallengeZRHFPlayerMsg()
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
            list.Add(SetBaseInfoItem(HFPlayerInfo.CrossLevel, CrossChallengeInfoMng.Info.Level));
            list.Add(SetBaseInfoItem(HFPlayerInfo.CrossScore, CrossChallengeInfoMng.Info.Star));
            return list;
        }

        public MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO GetCrossChallengeChallengerMsg(PlayerCrossFightInfo fightInfo)
        {
            MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO response = new MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO();
            response.Result = (int)ErrorCode.Success;

            Dictionary<int, Dictionary<int, MSG_ZGC_HERO_INFO>> dic = new Dictionary<int, Dictionary<int, MSG_ZGC_HERO_INFO>>();
            Dictionary<int, MSG_ZGC_HERO_INFO> list;
            foreach (var kv in fightInfo.HeroQueue.OrderBy(x => x.Key))
            {
                foreach (var pos in kv.Value)
                {
                    MSG_ZGC_HERO_INFO info = GetZgcPlayerHeroInfoMsg(pos.Value);
                    info.CrossChallengeQueueNum = kv.Key;
                    info.CrossChallengePositionNum = pos.Key;
                    if (dic.TryGetValue(info.CrossChallengeQueueNum, out list))
                    {
                        list[info.CrossChallengePositionNum] = info;
                    }
                    else
                    {
                        list = new Dictionary<int, MSG_ZGC_HERO_INFO>();
                        list[info.CrossChallengePositionNum] = info;
                        dic.Add(info.CrossChallengeQueueNum, list);
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
                    item.Value.CrossChallengeQueueNum = kv.Key;
                    item.Value.CrossChallengePositionNum = item.Key;
                    info.HeroList.Add(item.Value);
                }
                power += info.BattlePower;
                response.Queue.Add(info);
            }

            //自己信息
            response.Info = new CROSS_CHALLENGER_INFO();
            response.Info.BaseInfo = GetPlayerBaseInfo(fightInfo);
            response.Info.CrossLevel = fightInfo.CrossLevel;
            response.Info.CrossStar = fightInfo.CrossStar;
            if (power < int.MaxValue)
            {
                response.Info.BaseInfo.BattlePower = (int)power;
            }
            response.Info.BaseInfo.BattlePower64 = power;

            return response;
        }



        private void GetCrossChallengeChallengerInfoByRelation(int challengerUid)
        {
            MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO msg = new MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO
            {
                ChallengerUid = challengerUid, GetType = (int) ChallengeIntoType.CrossChallengePreliminary
            };
            server.SendToRelation(msg, Uid);
        }

        private PlayerCrossFightInfo GetCrossChallengeRobotInfo(CrossRobotInfo info)
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

        public PlayerCrossFightInfo GetCrossChallengeRobotInfo()
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
            rankInfo.CrossLevel = CrossChallengeInfoMng.Info.Level;
            rankInfo.CrossStar = CrossChallengeInfoMng.Info.Star;

            rankInfo.NatureValues = NatureValues;
            rankInfo.NatureRatios = NatureRatios;
            //伙伴信息
            foreach (var kv in HeroMng.CrossChallengeQueue)
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
                            robotHero.SoulRings += string.Format("{0}:{1}:{2}:{3}:{4}|", curr.Value.Position, curr.Value.Level, curr.Value.SpecId, curr.Value.Year, curr.Value.Element);
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
                    robotHero.Equipment = string.Join("|", equipmentItems.Select(x=>x.Id));

                    rankInfo.AddHero(robotHero, item.Key, kv.Key);
                }
            }
            return rankInfo;
        }

        public void RefreshCrossChallengeDailyActiveReward()
        {
            CrossChallengeInfoMng.RefreshDaily();
            server.GameDBPool.Call(new QueryUpdateCrossChallengeActiveReward(Uid, CrossChallengeInfoMng.Info.ActiveReward, CrossChallengeInfoMng.Info.DailyFight));
        }

        public void RefreshCrossChallengeRank(bool syncClient)
        {
            CrossChallengeInfoMng.RefreshSeason();
            //SyncDbUpdateCrossChallengeResult();
            if (syncClient)
            {
                SendCrossChallengeManagerMessage();
            }
        }

        /// <summary>
        /// 高手殿堂
        /// </summary>
        /// <param name="page"></param>
        public void ShowCrossChallengeSeasonLeaderInfos()
        {
            //MSG_ZR_SHOW_CROSS_SEASON_LEADER_INFO req = new MSG_ZR_SHOW_CROSS_SEASON_LEADER_INFO();
            //server.SendToRelation(req, uid);
        }


        public void ShowCrossChallengeLeaderInfosMsg(MSG_RZ_SHOW_CROSS_CHALLENGE_LEADER_INFO msg)
        {
            MSG_ZGC_SHOW_CROSS_CHALLENGE_LEADER_INFO response = new MSG_ZGC_SHOW_CROSS_CHALLENGE_LEADER_INFO();
            //玩家
            foreach (var item in msg.List)
            {
                CROSS_CHALLENGE_RANK_INFO rankInfo = GetCrossChallengeRankInfo(item);
                response.List.Add(rankInfo);
            }
            Write(response);
        }

        /// <summary>
        /// 获取挑战者信息
        /// </summary>
        public void ShowCrossChallengeRankInfosMsg(MSG_RZ_SHOW_CROSS_CHALLENGE_RANK_INFO msg)
        {
            MSG_ZGC_CROSS_CHALLENGE_RANK_INFO_LIST response = new MSG_ZGC_CROSS_CHALLENGE_RANK_INFO_LIST();

            CROSS_CHALLENGE_RANK_INFO self = GetSelfCrossChallengeRankInfo(msg.Rank);
            response.OwnerInfo = self;
            response.Page = msg.Page;
            response.TotalCount = msg.TotalCount;
            //玩家
            foreach (var item in msg.List)
            {
                CROSS_CHALLENGE_RANK_INFO rankInfo = GetCrossChallengeRankInfo(item);
                response.List.Add(rankInfo);
            }
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private CROSS_CHALLENGE_RANK_INFO GetSelfCrossChallengeRankInfo(int rank)
        {
            CROSS_CHALLENGE_RANK_INFO self = new CROSS_CHALLENGE_RANK_INFO();
            self.BaseInfo = PlayerInfo.GetPlayerBaseInfo(this);
            self.Rank = rank;
            foreach (var kv in ArenaMng.DefensiveHeros)
            {
                self.Defensive.Add(kv.Key);
            }
            self.CrossLevel = CrossChallengeInfoMng.Info.Level;
            self.CrossStar = CrossChallengeInfoMng.Info.Star;
            return self;
        }

        private CROSS_CHALLENGE_RANK_INFO GetCrossChallengeRankInfo(MSG_RZ_CROSS_CHALLENGE_RANK_INFO item)
        {
            CROSS_CHALLENGE_RANK_INFO challengerInfo = new CROSS_CHALLENGE_RANK_INFO();
            challengerInfo.BaseInfo = GetPlayerBaseInfo(item);
            challengerInfo.Rank = item.Rank;
            challengerInfo.CrossLevel = item.CrossLevel;
            challengerInfo.CrossStar = item.CrossStar;
            challengerInfo.Defensive.AddRange(item.Defensive);
            return challengerInfo;
        }

        public PLAYER_BASE_INFO GetPlayerBaseInfo(MSG_RZ_CROSS_CHALLENGE_RANK_INFO sinfo)
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
            info.MainId = BaseApi.GetMainIdByUid(sinfo.Uid);
            if (sinfo.BattlePower < int.MaxValue)
            {
                info.BattlePower = (int)sinfo.BattlePower;
            }
            info.BattlePower64 = sinfo.BattlePower;
            return info;
        }

        /// <summary>
        /// 保存段位奖励领取
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void SyncDbUpdateCrossChallengeReward()
        {
            server.GameDBPool.Call(new QueryUpdateCrossChallengeReward(Uid, CrossChallengeInfoMng.Info.ActiveReward, CrossChallengeInfoMng.Info.PreliminaryReward));
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void SyncDbUpdateCrossChallengeResult()
        {
            server.GameDBPool.Call(new QueryUpdateCrossChallengeResult(Uid, CrossChallengeInfoMng.Info));
        }

    }
}
