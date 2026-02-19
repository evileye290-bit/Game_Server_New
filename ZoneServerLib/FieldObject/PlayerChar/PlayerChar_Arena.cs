using CommonUtility;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerModels.Arena;
using ServerShared;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        //魂斗场
        public ArenaManager ArenaMng { get; set; }

        public void InitArenaManager()
        {
            ArenaMng = new ArenaManager(this, server);
        }

        public string DailyRankReward = string.Empty;

        /// <summary>
        /// 进入竞技场
        /// </summary>
        /// <param name="index"></param>
        public void EnterArenaMap(int index)
        {
            PlayerRankBaseInfo rankInfo = ArenaMng.GetArenaRankInfoByIndex(index);
            if (rankInfo != null)
            {
                if (rankInfo.HeroInfos.Count == 0)
                {
                    ShowChallengerInfo(index);
                }
                else
                {
                    EnterArenaMap(rankInfo);
                }
            }
        }

        public void EnterVersusMapByUid(int challengerUid)
        {
            PlayerChar challenger = server.PCManager.FindPc(challengerUid);
            if (challenger == null)
            {
                challenger = server.PCManager.FindOfflinePc(challengerUid);
                if (challenger == null)
                {
                    server.RelationServer.LoadBattlePlayerInfoWithQuerys((int)ChallengeIntoType.Versus, challengerUid, uid, uid);
                    return;
                }
            }

            PlayerRankBaseInfo rankInfo = challenger.GetChallengerRankBaseInfo();

            List<HeroInfo> heroInfos = challenger.HeroMng.GetEquipHeros().Values.ToList();
            challenger.SetHeroInfoRobotSoulRings(heroInfos);
            PetInfo petInfo = challenger.PetManager.GetPetInfo(challenger.PetManager.OnFightPet);
            EnterVersusMap(rankInfo, heroInfos, petInfo);
        }

        private void EnterArenaMap(PlayerRankBaseInfo rankInfo)
        {
            int dungeonId = ArenaLibrary.MapId;
            //GetArenaRobotInfo
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = dungeonId;
            response.Result = (int)CanCreateDungeon(dungeonId);
            if (response.Result != (int)ErrorCode.Success)
            {
                Log.Write($"player {Uid} request to enter arena {dungeonId} failed: reason {response.Result}");
                Write(response);
                return;
            }

            if (rankInfo == null)
            {
                Log.WarnLine("player {0} enter arena map failed: not find rank info index {1}", Uid, rankInfo.Index);
                response.Result = (int)ErrorCode.NotFindChallengerInfo;
                Write(response);
                return;
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
            ArenaDungeonMap arenaDungeonMap = dungeon as ArenaDungeonMap;

            Log.Write("player {0} enter arena map Challenge index {1} uid {2} rank {3}", Uid, rankInfo.Index, rankInfo.Uid, rankInfo.Rank);

            dungeon.BattleFpsManager?.SetBattleInfo(this, rankInfo);

            int battlePower = HeroMng.GetBattlePower();

            //战力压制
            dungeon.SetBattlePowerSuppress( battlePower, rankInfo.BattlePower);
            if (battlePower > rankInfo.BattlePower)
            {
                //添加自己
                arenaDungeonMap.AddAttackerMirror(this);
                //添加对手
                arenaDungeonMap.AddArenaDefender(rankInfo);
            }
            else
            {
                //添加对手
                arenaDungeonMap.AddArenaDefender(rankInfo);
                //添加自己
                arenaDungeonMap.AddAttackerMirror(this);
            }

            // 成功 进入副本
            RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();

            if (ArenaMng.DefensiveHeros.Count == 0)
            {
                //设置一套初始阵容
                foreach (var item in HeroMng.GetAllHeroPos())
                {
                    ArenaMng.AddDefensiveHero(item.Item1, item.Item2);
                }
                int power = ArenaMng.GetDefensiveBattlePower();
                SecdArenaDefensiveInfoToRelation(power);
                //同步
                string defensiveHeros = ArenaMng.GetDefensiveHeros();
                //保存DB
                SyncDbUpdateDefensiveHeros(defensiveHeros);
                //保存Redis
                UpdatePlayerDefensiveHerosToRedis(defensiveHeros);
                UpdatePlayerDefensivePowerToRedis(power);
            }

            if (dungeon.GetMapType() == MapType.Arena)
            {
                AddRunawayActivityNumForType(RunawayAction.Aren);
            }
        }

        public void AddAttackerAndDefender(PlayerChar player, PlayerRankBaseInfo defBaseInfo)
        {
        }

        public void EnterVersusMap(PlayerRankBaseInfo rankInfo, List<HeroInfo> heroInfos, PetInfo petInfo = null)
        {
            int dungeonId = ArenaLibrary.VersusMapId;
            //GetArenaRobotInfo
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = dungeonId;
            response.Result = (int)CanCreateDungeon(dungeonId);
            if (response.Result != (int)ErrorCode.Success)
            {
                Log.Write($"player {Uid} request to enter versus {dungeonId} failed: reason {response.Result}");
                Write(response);
                return;
            }

            if (rankInfo == null)
            {
                Log.WarnLine("player {0} enter versus map failed: not find rank info index {1}", Uid, rankInfo.Index);
                response.Result = (int)ErrorCode.NotFindChallengerInfo;
                Write(response);
                return;
            }

            // 在当前zone创建副本
            DungeonMap dungeon = server.MapManager.CreateDungeon(dungeonId);
            if (dungeon == null)
            {
                Log.Write($"player {Uid} enter versus map request to create dungeon {dungeonId} failed: create dungeon failed");
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }
            Log.Write("player {0} enter versus map Challenge index {1} uid {2} rank {3}", Uid, rankInfo.Index, rankInfo.Uid, rankInfo.Rank);
            VersusDungeonMap versusDungeon = (dungeon as VersusDungeonMap);

            //战力压制
            dungeon.SetBattlePowerSuppress(HeroMng.GetBattlePower(HeroMng.GetHeroPos()), rankInfo.BattlePower);

            //添加自己
            versusDungeon.AddAttackerMirror(this);

            //添加对手
            versusDungeon.AddDefenderRobot(rankInfo, heroInfos, petInfo);


            // 成功 进入副本
            RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();
        }

        /// <summary>
        /// 竞技场结算
        /// </summary>
        /// <param name="result"></param>
        /// <param name="rankInfo"></param>
        public void SendChallengeResult(DungeonResult result, ServerModels.PlayerRankBaseInfo rankInfo)
        {
            //if (CheckCounter(CounterType.ChallengeCount))
            //{
            //    Log.Warn("player {0} SendChallengeResult error cout is max", Uid);
            //    //通知前端奖励
            //    MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD(); ;
            //    rewardMsg.DungeonId = ArenaLibrary.MapId;
            //    rewardMsg.Result = (int)DungeonResult.HelpCountUseUp;
            //    Write(rewardMsg);
            //    return;
            //}
            int oldScore = ArenaMng.Score;
            int oldLevel = ArenaMng.Level;
            RewardManager rewards = new RewardManager();
            string reward = string.Empty;
            switch (result)
            {
                case DungeonResult.Success:
                    ArenaMng.AddScore(ArenaLibrary.WinScore);
                    ArenaMng.AddWinStreak();
                    //奖励
                    reward = ArenaLibrary.ChallengeWinReward;                
                    break;
                case DungeonResult.Failed:
                case DungeonResult.Tie:
                default:
                    //分数增加，排名不变
                    ArenaMng.AddScore(ArenaLibrary.LoseScore);
                    ArenaMng.ResetWinStreak();
                    //奖励
                    reward = ArenaLibrary.ChallengeLoseReward;
                    break;
            }
            //奖励
            rewards.InitSimpleReward(reward);
            AddRewards(rewards, ObtainWay.ChallengeResult);
            //更新挑战次数
            UpdateCounter(CounterType.ChallengeCount, 1);
            //更新挑战时间
            ArenaMng.SetFightTime(ZoneServerApi.now);
            //保存DB
            SyncDbUpdateFightResult();


            //通知Relation 获取排名
            MSG_ZR_CHALLENGE_WIN_CHANGE_RANK msg = new MSG_ZR_CHALLENGE_WIN_CHANGE_RANK();
            msg.PcUid = Uid;
            msg.PcRank = ArenaMng.Rank;
            msg.ChallengerUid = rankInfo.Uid;
            msg.ChallengerRank = rankInfo.Rank;

            msg.OldScore = oldScore;
            msg.Reward = reward;
            msg.Result = (int)result;
            msg.HistoryRank = ArenaMng.HistoryMaxRank;
            server.SendToRelation(msg, Uid);

            int dungeonResult = 2;
            if (result == DungeonResult.Success)
            {
                dungeonResult = 1;
            }
            //komoelog
            KomoeLogRecordPvpFight(1, 1, rewards.RewardList, dungeonResult, ArenaMng.Rank, rankInfo.Rank, oldLevel.ToString(), ArenaMng.Level.ToString(), rankInfo.Uid, rankInfo.BattlePower);            
        }

        /// <summary>
        /// 切磋结算
        /// </summary>
        /// <param name="result"></param>
        public void SendChallengeResult(DungeonResult result)
        {
            //通知前端奖励
            MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
            rewardMsg.DungeonId = ArenaLibrary.MapId;
            rewardMsg.Result = (int)result;
            Write(rewardMsg);
        }

        /// <summary>
        /// 显示挑战者信息
        /// </summary>
        /// <param name="index"></param>
        public void ShowChallengerInfo(int index)
        {
            MSG_ZGC_ARENA_CHALLENGER_HERO_INFO response = new MSG_ZGC_ARENA_CHALLENGER_HERO_INFO();

            ServerModels.PlayerRankBaseInfo rankInfo = ArenaMng.GetArenaRankInfoByIndex(index);
            if (rankInfo == null)
            {
                Log.WarnLine("player {0} show arena challenger info failed: not find rank info index {1}", Uid, index);
                response.Result = (int)ErrorCode.NotFindChallengerInfo;
                Write(response);
                return;
            }

            if (rankInfo.HeroInfos.Count == 0)
            {
                rankInfo.HeroInfos = new List<RobotHeroInfo>();
                //需要同步数据
                if (rankInfo.IsRobot)
                {
                    //机器人，直接读表
                    ArenaRobotInfo info = RobotLibrary.GetArenaRobotInfo(rankInfo.Rank);
                    if (info != null)
                    {
                        List<int> heros = RobotManager.GetHeroRobotIdList(info);
                        List<HeroInfo> infos = RobotManager.GetHeroList(heros);

                        //信息
                        for (int i = 0; i < infos.Count; i++)
                        {
                            // Hero heroInfo = NewHero(server, this, infos[i]);
                            // heroInfo.Init();

                            CHALLENGER_HERO_INFO challenger = GetRobotChallengerHeroInfo(infos[i]);
                            challenger.EquipIndex = i + 2;
                            response.HeroList.Add(challenger);
                        }
                        response.Result = (int)ErrorCode.Success;
                        response.Info = GetArenaRankBaseInfo(rankInfo);
                        Write(response);

                        //缓存信息
                        foreach (var item in response.HeroList)
                        {
                            RobotHeroInfo robotInfo = GetRobotHeroInfo(item);
                            rankInfo.HeroInfos.Add(robotInfo);
                        }
                    }
                    else
                    {
                        Log.WarnLine("player {0} show arena challenger info failed: not find rank info index {1}", Uid, index);
                        response.Result = (int)ErrorCode.NotFindRobotInfo;
                        Write(response);
                        return;
                    }
                }
                else
                {
                    //获取玩家信息
                    GetChallengerInfo(rankInfo);
                }
            }
            else
            {
                if (rankInfo.UpdateTime != null && (ZoneServerApi.now - rankInfo.UpdateTime).TotalSeconds > ArenaLibrary.InfoRefreshTime)
                {
                    if (!rankInfo.IsRobot)
                    {
                        //获取玩家信息
                        GetChallengerInfo(rankInfo);
                    }
                    else
                    {
                        //找到玩家，直接获取信息
                        response.Result = (int)ErrorCode.Success;
                        response.Info = GetArenaRankBaseInfo(rankInfo);
                        response.HeroList.AddRange(GetArenaChallengerInfo(rankInfo.HeroInfos));
                        response.Pet = GetArenaChallengerPetInfo(rankInfo.PetInfo);
                        Write(response);
                    }
                }
                else
                {
                    //找到玩家，直接获取信息
                    response.Result = (int)ErrorCode.Success;
                    response.Info = GetArenaRankBaseInfo(rankInfo);
                    response.HeroList.AddRange(GetArenaChallengerInfo(rankInfo.HeroInfos));
                    response.Pet = GetArenaChallengerPetInfo(rankInfo.PetInfo);
                    Write(response);
                }
            }
        }

        /// <summary>
        /// 领取段位奖励
        /// </summary>
        /// <param name="heroIds"></param>
        public void GetRankReward(int level)
        {
            MSG_ZGC_GET_RANK_LEVEL_REWARD response = new MSG_ZGC_GET_RANK_LEVEL_REWARD();

            if (level > ArenaMng.Level)
            {
                Log.Warn("player {0} get rank level reward failed: no such level {1}", uid, level);
                response.Result = (int)ErrorCode.RankLevelNotEnough;
                Write(response);
                return;
            }

            if (ArenaMng.LevelReward.Contains(level))
            {
                Log.Warn("player {0} get rank level reward failed: has get level {1} reward", uid, level);
                response.Result = (int)ErrorCode.AlreadyGetRankLevelReward;
                Write(response);
                return;
            }

            RankLevelInfo info = ArenaLibrary.GetRankLevelInfo(level);
            if (info == null)
            {
                Log.Warn("player {0} get rank level reward failed: not find level info {1}", uid, level);
                response.Result = (int)ErrorCode.RankLevelNotEnough;
                Write(response);
                return;
            }

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(info.Rewards);
            AddRewards(rewards, ObtainWay.RankLevelReward);

            //清理旧的配置
            ArenaMng.AddLevelReward(level);

            //同步
            string levelReward = ArenaMng.GetLevelRewards();

            //保存DB
            SyncDbUpdateLevelReward(levelReward);

            //komoelog
            KomoeLogRecordPvpFight(1, 4, rewards.RewardList, 1, ArenaMng.Rank, ArenaMng.Rank, ArenaMng.Level.ToString(), ArenaMng.Level.ToString(), 0, 0);

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.LevelReward.AddRange(ArenaMng.LevelReward);
            Write(response);
        }

        /// <summary>
        /// 充值挑战时间
        /// </summary>
        /// <param name="heroId"></param>
        public void ResetArenaFightTime()
        {
            //MSG_ZGC_RESET_ARENA_FIGHT_TIME response = new MSG_ZGC_RESET_ARENA_FIGHT_TIME();

            ////判断是否CD中
            //TimeSpan timeSpan = ArenaMng.FightTime - ZoneServerApi.now;
            //if (timeSpan.TotalSeconds > ArenaLibrary.FightCD)
            //{
            //    //CD中不需要花费钻石
            //    Log.Write("player {0} rest fight time need no cost: time is {1} now is {2}", uid, ArenaMng.FightTime, ZoneServerApi.now);
            //}
            //else
            //{
            //    //判断钻石是否足够
            //    if (GetCoins(ArenaLibrary.ResetCostType) < ArenaLibrary.ResetCostNum)
            //    {
            //        Log.Warn("player {0} rest fight time failed: {1} is {2} error", uid, ArenaLibrary.ResetCostType, GetCoins(ArenaLibrary.ResetCostType));
            //        response.Result = (int)ErrorCode.NoCoin;
            //        Write(response);
            //        return;
            //    }

            //    //重置后时间
            //    DateTime time = ArenaMng.FightTime.AddSeconds(-ArenaLibrary.FightCD);
            //    //重置时间
            //    ArenaMng.SetFightTime(time);

            //    //扣除花费
            //    DelCoins(ArenaLibrary.ResetCostType, ArenaLibrary.ResetCostNum, ConsumeWay.ResetArenaFightTime);

            //    //同步
            //    SyncDbUpdateFightTime(time);
            //}

            //response.FightTime = Timestamp.GetUnixTimeStampSeconds(ArenaMng.FightTime);
            //response.Result = (int)ErrorCode.Success;
            //Write(response);
        }

        /// <summary>
        /// 保存防守阵容
        /// </summary>
        /// <param name="heroIds"></param>
        public void SaveDefensiveHeros(RepeatedField<int> heroIds, RepeatedField<int> poses)
        {
            MSG_ZGC_SAVE_DEFEMSIVE response = new MSG_ZGC_SAVE_DEFEMSIVE();

            if (heroIds.Count > HeroLibrary.HeroPosCount || heroIds.Count < 1)
            {
                Log.Warn("player {0} save defensive failed: save heroInfo count is {1}", uid, heroIds.Count);
                response.Result = (int)ErrorCode.MaxCount;
                Write(response);
                return;
            }
            if (heroIds.Count != poses.Count)
            {
                Log.Warn("player {0} save defensive failed: save heroInfo count is {1}, save pose count is {2}", uid, heroIds.Count, poses.Count);
                response.Result = (int)ErrorCode.MaxCount;
                Write(response);
                return;
            }
            //int power = 0;
            if (heroIds.Count > 0)
            {
                List<int> defensives = new List<int>();
                bool isSame = false;
                for (int i = 0; i < heroIds.Count; i++)
                {
                    int heroId = heroIds[i];

                    //判断是否拥有这个伙伴
                    HeroInfo hero = HeroMng.GetHeroInfo(heroId);
                    if (hero == null)
                    {
                        Log.Warn("player {0} save defensive failed: no such heroInfo {1}", uid, heroId);
                        response.Result = (int)ErrorCode.NoHeroInfo;
                        Write(response);
                        return;
                    }

                    ////判断是否跟保存的伙伴相同
                    //int saveHeroId = ArenaMng.GetDefensiveHeroByIndex(i);
                    //if (defensives.Contains(heroId))
                    //{
                    //有不同，就可以保存修改
                    //    isSame = true;
                    //}

                    //检查是否有重复阵容
                    for (int j = i + 1; j < heroIds.Count; j++)
                    {
                        if (heroId == heroIds[j])
                        {
                            Log.Warn("player {0} save defensive failed: same heroInfo {1}", uid, heroId);
                            response.Result = (int)ErrorCode.SaveSameHeroId;
                            Write(response);
                            return;
                        }
                    }
                    //power += heroInfo.GetBattlePower();
                }

                if (isSame)
                {
                    Log.Write("player {0} save defensive is same", uid);
                    response.Result = (int)ErrorCode.Success;
                    Write(response);
                    return;
                }
            }

            //HeroInfo mainHero = HeroMng.GetHeroInfo(HeroId);
            //if (mainHero == null)
            //{
            //    Log.Warn("player {0} save defensive failed: no such heroInfo {1}", uid, HeroId);
            //    response.Result = (int)ErrorCode.NoHeroInfo;
            //    Write(response);
            //    return;
            //}
            //power += mainHero.GetBattlePower();

            //清理旧的配置
            ArenaMng.ClearDefensiveHero();

            for (int i = 0; i < heroIds.Count; i++)
            {
                ArenaMng.AddDefensiveHero(heroIds[i], poses[i]);
            }
            int power = ArenaMng.GetDefensiveBattlePower();
            SecdArenaDefensiveInfoToRelation(power);
            //同步
            string defensiveHeros = ArenaMng.GetDefensiveHeros();
            //保存DB
            SyncDbUpdateDefensiveHeros(defensiveHeros);
            //保存Redis
            UpdatePlayerDefensiveHerosToRedis(defensiveHeros);
            UpdatePlayerDefensivePowerToRedis(power);

            foreach (var kv in ArenaMng.DefensiveHeros)
            {
                response.DefensiveHeros.Add(kv.Key);
                response.HeroPoses.Add(kv.Value);
            }
            //response.DefensiveHeros.AddRange(ArenaMng.DefensiveHeros);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private void SecdArenaDefensiveInfoToRelation(int power)
        {
            //通知Relation
            MSG_ZR_UPDATE_AREMA_DEFEMDER msg = new MSG_ZR_UPDATE_AREMA_DEFEMDER();
            msg.PcUid = uid;
            msg.Power = power;
            //设置新的配置
            foreach (var kv in ArenaMng.DefensiveHeros)
            {
                msg.Defensive.Add(kv.Key);
                msg.DefPoses.Add(kv.Value);
            }
            server.SendToRelation(msg, Uid);
        }

        /// <summary>
        /// 换一换
        /// </summary>
        public void ChangeChallengers()
        {
            if (ArenaMng.CanChangeChallengers())
            {
                GetArenaChallengers();

                ArenaMng.SetChangeChallengerTime();
            }
            else
            {
                //CD中
                Log.Warn("player {0} change challengers failed: last time is {1}", uid, ArenaMng.ChangeChallengerTime);
            }
        }

        public void GetArenaChallengers()
        {
            //可以刷新  //发送给Relation 缓存信息
            MSG_ZR_GET_ARENA_CHALLENGERS msg = new MSG_ZR_GET_ARENA_CHALLENGERS();
            server.SendToRelation(msg, Uid);
        }

        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="page"></param>
        public void ShowArenaRankInfos(int page)
        {
            MSG_ZR_SHOW_ARENA_RANK_INFO req = new MSG_ZR_SHOW_ARENA_RANK_INFO();
            req.Page = page;
            server.SendToRelation(req, uid);
        }

        /// <summary>
        /// 获取挑战者信息
        /// </summary>
        public void ShowArenaRankInfos(int rank, int page, int totalCount, List<RedisValue> uids, Dictionary<int, ServerModels.PlayerRankBaseInfo> list)
        {
            if (uids.Count > 0)
            {
                //到redis中获取信息
                OperateGetBaseInfoByIds operate = new OperateGetBaseInfoByIds(uids);
                server.GameRedis.Call(operate, ret1 =>
                {
                    ARENA_RANK_INFO self = GetSelfArenaRankInfo(rank);
                    MSG_ZGC_ARENA_RANK_INFO_LIST response = new MSG_ZGC_ARENA_RANK_INFO_LIST();
                    response.OwnerInfo = self;
                    response.Page = page;
                    response.TotalCount = totalCount;

                    if ((int)ret1 == 1)
                    {
                        if (operate.Characters == null)
                        {
                            response.Result = (int)ErrorCode.CharNotExist;
                            Write(response);
                            return;
                        }
                        if (operate.Characters.Count > 0)
                        {

                            foreach (var challenger in list)
                            {
                                //玩家
                                foreach (var item in operate.Characters)
                                {
                                    if (item.Uid == challenger.Value.Uid)
                                    {
                                        ARENA_RANK_INFO rankInfo = GetArenaRankInfo(challenger.Value.Rank, item);
                                        response.List.Add(rankInfo);
                                        break;
                                    }
                                }
                            }
                            response.Result = (int)ErrorCode.Success;
                            Write(response);
                        }
                        else
                        {
                            response.Result = (int)ErrorCode.CharNotExist;
                            Write(response);
                            return;
                        }
                        return;
                    }
                    else
                    {
                        //没找到对应id的信息
                        //Log.Error("player {0} search an not exist playerId {1} :redis date error", Uid, playerId);
                        response.Result = (int)ErrorCode.CharNotExist;
                        Write(response);
                        return;
                    }
                });
            }
            else
            {
                ARENA_RANK_INFO self = GetSelfArenaRankInfo(rank);
                MSG_ZGC_ARENA_RANK_INFO_LIST response = new MSG_ZGC_ARENA_RANK_INFO_LIST();
                response.Page = page;
                response.TotalCount = totalCount;
                response.OwnerInfo = self;
                response.Result = (int)ErrorCode.Success;
                Write(response);
            }
        }

        /// <summary>
        /// 获取挑战者信息
        /// </summary>
        public void GetChallengerInfos(int rank, List<RedisValue> uids, Dictionary<int, ServerModels.PlayerRankBaseInfo> list)
        {
            ArenaMng.SetArenaRankInfoList(list);

            if (uids.Count > 0)
            {
                //到redis中获取信息
                OperateGetBaseInfoByIds operate = new OperateGetBaseInfoByIds(uids);
                server.GameRedis.Call(operate, ret1 =>
                {
                    MSG_ZGC_GET_ARENA_CHALLENGERS response = new MSG_ZGC_GET_ARENA_CHALLENGERS();
                    if ((int)ret1 == 1)
                    {
                        if (operate.Characters == null)
                        {
                            response.Result = (int)ErrorCode.CharNotExist;
                            Write(response);
                            return;
                        }
                        if (operate.Characters.Count > 0)
                        {

                            foreach (var challenger in ArenaMng.ChallengerInfolist)
                            {
                                if (!challenger.Value.IsRobot)
                                {
                                    //玩家
                                    foreach (var item in operate.Characters)
                                    {
                                        if (item.Uid == challenger.Value.Uid)
                                        {
                                            CHALLENGER_INFO challengerInfo = GetChallengerInfo(challenger.Key, challenger.Value.Rank, item);
                                            SetArenaRankBaseInfo(challenger.Value, challengerInfo);
                                            response.List.Add(challengerInfo);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    CHALLENGER_INFO challengerInfo = GetRobotChallengerInfo(challenger.Key, challenger.Value.Rank);
                                    SetArenaRankBaseInfo(challenger.Value, challengerInfo);
                                    response.List.Add(challengerInfo);
                                }
                            }
                            response.Rank = rank;
                            response.Result = (int)ErrorCode.Success;
                            Write(response);
                        }
                        else
                        {
                            response.Result = (int)ErrorCode.CharNotExist;
                            Write(response);
                            return;
                        }
                        return;
                    }
                    else
                    {
                        //没找到对应id的信息
                        //Log.Error("player {0} search an not exist playerId {1} :redis date error", Uid, playerId);
                        response.Result = (int)ErrorCode.CharNotExist;
                        Write(response);
                        return;
                    }
                });
            }
            else
            {

                MSG_ZGC_GET_ARENA_CHALLENGERS response = new MSG_ZGC_GET_ARENA_CHALLENGERS();
                //全是机器人，直接获取机器人信息发送
                foreach (var challenger in ArenaMng.ChallengerInfolist)
                {
                    if (challenger.Value.IsRobot)
                    {
                        CHALLENGER_INFO challengerInfo = GetRobotChallengerInfo(challenger.Key, challenger.Value.Rank);
                        SetArenaRankBaseInfo(challenger.Value, challengerInfo);
                        response.List.Add(challengerInfo);
                    }
                }
                response.Result = (int)ErrorCode.Success;
                Write(response);
            }

        }

        public void UpdateDefensivePower()
        {
            int power = ArenaMng.GetDefensiveBattlePower();
            //foreach (var heroId in ArenaMng.DefensiveHeros)
            //{
            //    //判断是否拥有这个伙伴
            //    HeroInfo heroInfo = HeroMng.GetHeroInfo(heroId.Key);
            //    if (heroInfo != null)
            //    {
            //        power += heroInfo.GetBattlePower();
            //    }
            //}
            //HeroInfo mainHero = HeroMng.GetHeroInfo(HeroId);
            //if (mainHero != null)
            //{
            //    power += mainHero.GetBattlePower();
            //}

            UpdatePlayerDefensivePowerToRedis(power);
        }

        /// <summary>
        /// 保存反手阵容到Redis
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void UpdatePlayerDefensiveHerosToRedis(string defensiveHeros)
        {
            server.GameRedis.Call(new OperateUpdateArenaDefensive(Uid, defensiveHeros));
        }

        /// <summary>
        /// 保存反手阵容战力到Redis
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void UpdatePlayerDefensivePowerToRedis(int power)
        {
            server.GameRedis.Call(new OperateUpdateDefensivePower(Uid, power));
        }

        /// <summary>
        /// 保存段位奖励领取
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void SyncDbUpdateLevelReward(string levelReward)
        {
            server.GameDBPool.Call(new QueryUpdateLevelReward(Uid, levelReward));
        }
        /// <summary>
        /// 保存反手阵容到DB
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void SyncDbUpdateDefensiveHeros(string defensiveHeros)
        {
            server.GameDBPool.Call(new QueryUpdateArenaDefensive(Uid, defensiveHeros));
        }
        /// <summary>
        /// 保存挑战时间
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void SyncDbUpdateFightTime(DateTime time)
        {
            string timeString = time.ToString(CONST.DATETIME_TO_STRING);
            server.GameDBPool.Call(new QueryUpdateArenaFightTime(Uid, timeString));
        }
        /// <summary>
        /// 保存挑战结果
        /// </summary>
        /// <param name="defensiveHeros"></param>
        public void SyncDbUpdateFightResult()
        {
            string timeString = ArenaMng.FightTime.ToString(CONST.DATETIME_TO_STRING);
            server.GameDBPool.Call(new QueryUpdateArenaFightResult(Uid, ArenaMng.Level, ArenaMng.Score, ArenaMng.HistoryMaxScore,
                ArenaMng.FightTotal, ArenaMng.WinTotal, ArenaMng.WinStreak, ArenaMng.HistoryWinStreak, timeString));
        }

        public void GetArenaDailyReward()
        {
            if (!string.IsNullOrEmpty(DailyRankReward))
            {
                string[] rewardIds = StringSplit.GetArray("|", DailyRankReward);
                if (rewardIds.Length > 0)
                {
                    MSG_ZR_ARENA_DAILY_REWARD msg = new MSG_ZR_ARENA_DAILY_REWARD();
                    foreach (var item in rewardIds)
                    {
                        msg.Ids.Add(int.Parse(item));
                    }
                    server.SendToRelation(msg, Uid);
                }
                DailyRankReward = string.Empty;
            }
        }


        public MSG_ZGC_CHALLENGE_RESULT GetChallengeResultInfo(int oldRank, int oldScore)
        {
            MSG_ZGC_CHALLENGE_RESULT arenaInfo = new MSG_ZGC_CHALLENGE_RESULT();
            arenaInfo.PcUid = Uid;
            arenaInfo.OldRank = oldRank;
            arenaInfo.NewRank = ArenaMng.Rank;
            arenaInfo.OldScore = oldScore;
            arenaInfo.NewScore = ArenaMng.Score;
            arenaInfo.HistoryScore = ArenaMng.HistoryMaxScore;
            arenaInfo.HistoryRank = ArenaMng.HistoryMaxRank;
            arenaInfo.WinStreak = ArenaMng.WinStreak;
            return arenaInfo;
        }

        public ARENA_RANK_INFO GetSelfArenaRankInfo(int rank)
        {
            ARENA_RANK_INFO self = new ARENA_RANK_INFO();
            self.BaseInfo = PlayerInfo.GetPlayerBaseInfo(this);
            self.Rank = rank;
            foreach (var kv in ArenaMng.DefensiveHeros)
            {
                self.Defensive.Add(kv.Key);
            }
            //self.Defensive.AddRange(ArenaMng.DefensiveHeros);
            return self;
        }

        private ARENA_RANK_INFO GetArenaRankInfo(int rank, PlayerBaseInfo item)
        {
            ARENA_RANK_INFO challengerInfo = new ARENA_RANK_INFO();
            challengerInfo.BaseInfo = PlayerInfo.GetPlayerBaseInfo(item);
            challengerInfo.Rank = rank;
            if (!string.IsNullOrEmpty(item.Defensive))
            {
                string[] defensive = StringSplit.GetArray("|", item.Defensive);
                for (int i = 0; i < defensive.Length; i++)
                {
                    string[] hero = StringSplit.GetArray(":", defensive[i]);
                    challengerInfo.Defensive.Add(int.Parse(hero[0]));
                    if (hero.Length > 1)
                    {
                        challengerInfo.DefPoses.Add(int.Parse(hero[1]));
                    }
                    else
                    {
                        challengerInfo.DefPoses.Add(i + 1);
                    }
                }
                //foreach (var heroInfo in defensive)
                //{
                //    //challengerInfo.Defensive.Add(int.Parse(heroInfo));
                //    challengerInfo.Defensive.Add(int.Parse(heroInfo.Split(':')[0]));
                //    challengerInfo.DefPoses.Add(int.Parse(heroInfo.Split(':')[1]));
                //}
            }
            return challengerInfo;
        }

        private CHALLENGER_INFO GetChallengerInfo(int index, int rank, PlayerBaseInfo item)
        {
            CHALLENGER_INFO challengerInfo = new CHALLENGER_INFO();
            challengerInfo.Index = index;
            challengerInfo.Rank = rank;
            challengerInfo.BaseInfo = PlayerInfo.GetPlayerBaseInfo(item);
            challengerInfo.DefensivePower = item.DefensivePower;

            Dictionary<int, int> heroGodList = item.HeroGod.ToDictionary('|', ':');

            if (!string.IsNullOrEmpty(item.Defensive))
            {
                string[] defensive = StringSplit.GetArray("|", item.Defensive);
                for (int i = 0; i < defensive.Length; i++)
                {
                    string[] hero = StringSplit.GetArray(":", defensive[i]);
                    int heroId = int.Parse(hero[0]);
                    challengerInfo.Defensive.Add(heroId);
                    if (hero.Length > 1)
                    {
                        challengerInfo.DefPoses.Add(int.Parse(hero[1]));
                    }
                    else
                    {
                        challengerInfo.DefPoses.Add(i + 1);
                    }
                    challengerInfo.HeroGod.Add(heroGodList.ContainsKey(heroId) ? heroGodList[heroId] : 0);
                }
            }

            return challengerInfo;
        }

        private CHALLENGER_INFO GetRobotChallengerInfo(int index, int rank)
        {
            ArenaRobotInfo robotInfo = RobotLibrary.GetArenaRobotInfo(rank);
            if (robotInfo != null)
            {
                //获取机器人信息发送
                CHALLENGER_INFO challengerInfo = new CHALLENGER_INFO();
                challengerInfo.Index = index;
                challengerInfo.Rank = rank;
                challengerInfo.BaseInfo = PlayerInfo.GetPlayerBaseInfo(robotInfo, server.MainId);
                challengerInfo.DefensivePower = robotInfo.BattlePower;

                List<int> heros = RobotManager.GetDefensiveHeroList(robotInfo);
                List<int> poses = RobotManager.GetHeroRobotIdPosesList(robotInfo);
                foreach (var item in heros)
                {
                    challengerInfo.Defensive.Add(item);
                    challengerInfo.DefPoses.Add(poses[heros.IndexOf(item)]);
                    challengerInfo.HeroGod.Add(0);
                }
                return challengerInfo;
            }
            else
            {
                Log.Warn("player {0} get arena challenger failed: not find rank {1} robot info ", uid, rank);
                return null;

            }
        }

        private void GetChallengerInfo(ServerModels.PlayerRankBaseInfo rankInfo)
        {
            int challengerUid = rankInfo.Uid;
            //加载玩家信息，先找同服务器玩家
            PlayerChar challenger = server.PCManager.FindPc(challengerUid);
            if (challenger == null)
            {
                challenger = server.PCManager.FindOfflinePc(challengerUid);
                if (challenger == null)
                {
                    //Log.WarnLine("player {0} show player info fail,can not find player {1}.", Uid, showPcUid);
                    //没找到玩家，去relation获取信息
                    MSG_ZR_GET_ARENA_CHALLENGER msg = new MSG_ZR_GET_ARENA_CHALLENGER();
                    msg.PcUid = Uid;
                    //msg.PcDefensive.Add(HeroId);
                    foreach (var kv in ArenaMng.DefensiveHeros)
                    {
                        msg.PcDefensive.Add(kv.Key);
                    }
                    //msg.PcDefensive.AddRange(ArenaMng.DefensiveHeros);

                    msg.ChallengerUid = challengerUid;
                    //msg.ChallengerDefensive.Add(rankInfo.HeroId);
                    msg.ChallengerDefensive.AddRange(rankInfo.Defensive);
                    msg.GetType = (int)ChallengeIntoType.Arena;
                    server.SendToRelation(msg, Uid);
                    return;
                }
            }

            //找到玩家，直接获取信息
            MSG_ZGC_ARENA_CHALLENGER_HERO_INFO response = challenger.GetChallengerMsg();
            response.Info = GetArenaRankBaseInfo(rankInfo);
            Write(response);

            //缓存信息
            rankInfo.HeroInfos.Clear();
            foreach (var item in response.HeroList)
            {
                RobotHeroInfo robotInfo = GetRobotHeroInfo(item);
                rankInfo.HeroInfos.Add(robotInfo);
            }
            rankInfo.PetInfo = GetRobotPetInfo(response.Pet);

            rankInfo.NatureValues = new Dictionary<int, int>(response.NatureValues);
            rankInfo.NatureRatios = new Dictionary<int, int>(response.NatureRatios); 

            //发送给Relation 缓存信息
            MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO addMsg = new MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO();
            addMsg.Info = response;
            addMsg.PcUid = Uid;
            addMsg.ChallengerUid = challengerUid;
            server.SendToRelation(addMsg, Uid);

            //获取数据时间
            rankInfo.UpdateTime = ZoneServerApi.now;
        }

        public List<CHALLENGER_HERO_INFO> GetArenaChallengerInfo(List<RobotHeroInfo> list)
        {
            List<CHALLENGER_HERO_INFO> heroList = new List<CHALLENGER_HERO_INFO>();

            foreach (var item in list)
            {
                CHALLENGER_HERO_INFO info = new CHALLENGER_HERO_INFO();
                info.Id = item.HeroId;
                info.Level = item.Level;
                info.AwakenLevel = item.AwakenLevel;
                info.StepsLevel = item.StepsLevel;
                info.GodType = item.GodType;
                info.SoulSkillLevel = item.SoulSkillLevel;

                info.HeroNature = new Hero_Nature();
                foreach (var nature in item.NatureList)
                {
                    switch (nature.Key)
                    {
                        case NatureType.PRO_MAX_HP:
                            info.HeroNature.MaxHp = nature.Value.ToInt64TypeMsg();
                            break;
                        case NatureType.PRO_ATK:
                            info.HeroNature.Atk = nature.Value.ToInt64TypeMsg();
                            break;
                        case NatureType.PRO_DEF:
                            info.HeroNature.Def = nature.Value.ToInt64TypeMsg();
                            break;
                        case NatureType.PRO_HIT:
                            info.HeroNature.Hit = nature.Value.ToInt64TypeMsg();
                            break;
                        case NatureType.PRO_FLEE:
                            info.HeroNature.Flee = nature.Value.ToInt64TypeMsg();
                            break;
                        case NatureType.PRO_CRI:
                            info.HeroNature.Cri = nature.Value.ToInt64TypeMsg();
                            break;
                        case NatureType.PRO_RES:
                            info.HeroNature.Res = nature.Value.ToInt64TypeMsg();
                            break;
                        case NatureType.PRO_IMP:
                            info.HeroNature.Imp = nature.Value.ToInt64TypeMsg();
                            break;
                        case NatureType.PRO_ARM:
                            info.HeroNature.Arm = nature.Value.ToInt64TypeMsg();
                            break;
                        default:
                            break;
                    }
                }

                string[] soulRingInfo = item.SoulRings.Split('|');
                if (soulRingInfo.Count() > 0)
                {
                    foreach (string temp in soulRingInfo)
                    {
                        string[] temps = temp.Split(':');
                        if (temps.Count() != 4)
                        {
                            continue;
                        }
                        CHALLENGER_HERO_SOULRING soulRingMsg = new CHALLENGER_HERO_SOULRING();
                        soulRingMsg.Pos = int.Parse(temps[0]);
                        soulRingMsg.Level = int.Parse(temps[1]);
                        soulRingMsg.SpecId = int.Parse(temps[2]);
                        soulRingMsg.Year = int.Parse(temps[3]);
                        if (temps.Length == 5)
                        {
                            soulRingMsg.Element = int.Parse(temps[4]);
                        }
                        info.SoulRings.Add(soulRingMsg);
                    }
                }

                //info.SoulBones.AddRange(item.SoulBones.ToList('|'));

                string[] soulBoneInfo = StringSplit.GetArray("|", item.SoulBones);
                //魂骨
                foreach (var soulBone in soulBoneInfo)
                {
                    List<int> soulBoneAttr = soulBone.ToList(':');
                    if (soulBoneAttr.Count < 1) continue;

                    HERO_SOULBONE soulBoneMsg = new HERO_SOULBONE();
                    soulBoneMsg.Id = soulBoneAttr[0];
                    soulBoneAttr.RemoveAt(0);
                    soulBoneMsg.SpecIds.AddRange(soulBoneAttr);

                    info.SoulBones.Add(soulBoneMsg);
                }

                //暗器
                List<int> weaponInfo = item.HiddenWeapon.ToList(':');
                //魂骨
                if (weaponInfo.Count == 2)
                {
                    HERO_HIDDENWEAPON weaponMsg = new HERO_HIDDENWEAPON() { Id = weaponInfo[0], Star = weaponInfo[1] };
                    info.HiddenWeapon = weaponMsg;
                }

                //装备(套装)
                info.Equipments.Add(item.Equipment.ToList('|'));

                heroList.Add(info);
            }
            return heroList;
        }

        public static RobotHeroInfo GetRobotHeroInfo(CHALLENGER_HERO_INFO item)
        {
            RobotHeroInfo robotInfo = new RobotHeroInfo();
            robotInfo.HeroId = item.Id;
            robotInfo.Level = item.Level;
            robotInfo.AwakenLevel = item.AwakenLevel;
            robotInfo.StepsLevel = item.StepsLevel;
            robotInfo.EquipIndex = item.EquipIndex;
            robotInfo.GodType = item.GodType;
            robotInfo.SoulSkillLevel = item.SoulSkillLevel;
            robotInfo.NatureList[NatureType.PRO_MAX_HP] = item.HeroNature.MaxHp.GetInt64();
            robotInfo.NatureList[NatureType.PRO_ATK] = item.HeroNature.Atk.GetInt64();
            robotInfo.NatureList[NatureType.PRO_DEF] = item.HeroNature.Def.GetInt64();
            robotInfo.NatureList[NatureType.PRO_HIT] = item.HeroNature.Hit.GetInt64();
            robotInfo.NatureList[NatureType.PRO_FLEE] = item.HeroNature.Flee.GetInt64();
            robotInfo.NatureList[NatureType.PRO_CRI] = item.HeroNature.Cri.GetInt64();
            robotInfo.NatureList[NatureType.PRO_RES] = item.HeroNature.Res.GetInt64();
            robotInfo.NatureList[NatureType.PRO_IMP] = item.HeroNature.Imp.GetInt64();
            robotInfo.NatureList[NatureType.PRO_ARM] = item.HeroNature.Arm.GetInt64();

            robotInfo.SoulRings = "";
            foreach (var soulRing in item.SoulRings)
            {
                robotInfo.SoulRings += $"{soulRing.Pos}:{soulRing.Level}:{soulRing.SpecId}:{soulRing.Year}:{soulRing.Element}|";
            }

            if (item.SoulBones.Count > 0)
            {
                List<string> soulBoneStr = item.SoulBones.ToList().ConvertAll(x => x.SpecIds.Count <= 0 ? x.Id.ToString() : x.Id.ToString() + ":" + string.Join(":", x.SpecIds));
                robotInfo.SoulBones = string.Join("|", soulBoneStr);
            }

            if (item.HiddenWeapon != null)
            {
                robotInfo.HiddenWeapon = $"{item.HiddenWeapon.Id}:{item.HiddenWeapon.Star}";
            }

            robotInfo.Equipment = string.Join("|", item.Equipments);

            return robotInfo;
        }

        public MSG_ZGC_ARENA_CHALLENGER_HERO_INFO GetChallengerMsg()
        {
            MSG_ZGC_ARENA_CHALLENGER_HERO_INFO response = new MSG_ZGC_ARENA_CHALLENGER_HERO_INFO();
            response.Result = (int)ErrorCode.Success;
            //伙伴信息
            foreach (var kv in ArenaMng.DefensiveHeros)
            {
                HeroInfo info = HeroMng.GetHeroInfo(kv.Key);
                if (info != null)
                {
                    //伙伴信息
                    CHALLENGER_HERO_INFO challenger = GetChallengerHeroInfo(info);
                    challenger.EquipIndex = kv.Value;
                    response.HeroList.Add(challenger);
                    ////总战力
                    //response.PlayerInfo.Power += info.GetBattlePower();
                }
                else
                {
                    //没找到伙伴信息
                    Log.WarnLine("player {0} get challenger heroInfo  info fail,can not find heroInfo {1}.", Uid, kv.Value);
                }
            }
            response.NatureValues.Add(NatureValues);
            response.NatureRatios.Add(NatureRatios);

            //宠物信息
            PetInfo petInfo = PetManager.GetDungeonQueuePet(DungeonQueueType.Arena, 1);
            if (petInfo != null)
            {
                response.Pet = GetChallengerPetInfo(petInfo);
            }

            return response;
        }

        private CHALLENGER_HERO_INFO GetChallengerHeroInfo(HeroInfo heroInfo)
        {
            CHALLENGER_HERO_INFO challenger = GetChallengerHeroMessage(heroInfo);
            //魂环
            Dictionary<int, SoulRingItem> soulRingDic = SoulRingManager.GetAllEquipedSoulRings(heroInfo.Id);
            if (soulRingDic != null)
            {
                //有魂环
                foreach (var soulRing in soulRingDic)
                {
                    CHALLENGER_HERO_SOULRING soulRingMsg = new CHALLENGER_HERO_SOULRING();
                    soulRingMsg.Pos = soulRing.Key;
                    soulRingMsg.Level = soulRing.Value.Level;
                    soulRingMsg.SpecId = soulRing.Value.SpecId;
                    soulRingMsg.Year = soulRing.Value.Year;
                    soulRingMsg.Element = soulRing.Value.Element;
                    challenger.SoulRings.Add(soulRingMsg);
                }
            }

            List<SoulBone> soulBones = SoulboneMng.GetEnhancedHeroBones(heroInfo.Id);
            if (soulBones != null && soulBones.Count > 0)
            {
                soulBones.ForEach(x =>
                {
                    HERO_SOULBONE soulBoneMsg = new HERO_SOULBONE();
                    soulBoneMsg.Id = x.TypeId;
                    soulBoneMsg.SpecIds.AddRange(x.GetSpecList());
                    challenger.SoulBones.Add(soulBoneMsg);
                });
            }

            var weaponItem = HiddenWeaponManager.GetHeroEquipedHiddenWeapon(heroInfo.Id);
            if (weaponItem != null)
            {
                challenger.HiddenWeapon = new HERO_HIDDENWEAPON() { Id = weaponItem.Id, Star = weaponItem.Info.Star };
            }
            return challenger;
        }

        private CHALLENGER_HERO_INFO GetRobotChallengerHeroInfo(HeroInfo heroInfo)
        {
            CHALLENGER_HERO_INFO challenger = GetChallengerHeroMessage(heroInfo);

            if (heroInfo == null || heroInfo.RobotInfo == null) return challenger;

            //魂环
            string[] soulRingInfo = heroInfo.RobotInfo.SoulRings.Split('|');
            //有魂环
            foreach (var soulRing in soulRingInfo)
            {
                try
                {
                    string[] info = soulRing.Split(':');
                    int pos = int.Parse(info[0]);
                    int level = int.Parse(info[1]);
                    int spec = int.Parse(info[2]);
                    int year = int.Parse(info[3]);
                    int element = int.Parse(info[4]);

                    CHALLENGER_HERO_SOULRING soulRingMsg = new CHALLENGER_HERO_SOULRING();
                    soulRingMsg.Pos = pos;
                    soulRingMsg.Level = level;
                    soulRingMsg.SpecId = spec;
                    soulRingMsg.Year = year;
                    soulRingMsg.Element = element;

                    challenger.SoulRings.Add(soulRingMsg);
                }
                catch (Exception e)
                {
                    //没找到魂环信息
                    Log.WarnLine("player {0} get challenger heroInfo info fail,can not find SoulRings {1}, {2}.", Uid, heroInfo.RobotInfo.SoulRings, e);
                }
            }

            string[] soulBoneInfo = StringSplit.GetArray("|", heroInfo.RobotInfo.SoulBones);
            //魂骨
            foreach (var soulBone in soulBoneInfo)
            {
                List<int> soulBoneAttr = soulBone.ToList(':');
                if (soulBoneAttr.Count < 1) continue;

                HERO_SOULBONE soulBoneMsg = new HERO_SOULBONE();
                soulBoneMsg.Id = soulBoneAttr[0];
                soulBoneAttr.RemoveAt(0);
                soulBoneMsg.SpecIds.AddRange(soulBoneAttr);

                challenger.SoulBones.Add(soulBoneMsg);
            }

            //暗器
            List<int> weaponInfo = heroInfo.RobotInfo.HiddenWeapon.ToList(':');
            //魂骨
            if (weaponInfo.Count == 2)
            {
                HERO_HIDDENWEAPON weaponMsg = new HERO_HIDDENWEAPON() { Id = weaponInfo[0], Star = weaponInfo[1] };
                challenger.HiddenWeapon = weaponMsg;
            }

            //装备(套装)
            challenger.Equipments.Add(heroInfo.RobotInfo.Equipment.ToList('|'));

            return challenger;
        }

        public CHALLENGER_HERO_INFO GetChallengerHeroMessage(HeroInfo heroInfo)
        {
            CHALLENGER_HERO_INFO info = new CHALLENGER_HERO_INFO();
            info.Id = heroInfo.Id;
            info.Level = heroInfo.Level;
            info.AwakenLevel = heroInfo.AwakenLevel;
            info.StepsLevel = heroInfo.StepsLevel;
            info.HeroNature = GetNatureMsg(heroInfo.Nature);
            info.GodType = heroInfo.GodType;
            info.SoulSkillLevel = heroInfo.SoulSkillLevel;
            return info;
        }

        private void SetArenaRankBaseInfo(ServerModels.PlayerRankBaseInfo challenger, CHALLENGER_INFO challengerInfo)
        {
            challenger.Name = challengerInfo.BaseInfo.Name;
            challenger.Sex = challengerInfo.BaseInfo.Sex;
            challenger.Level = challengerInfo.BaseInfo.Level;
            challenger.LadderLevel = challengerInfo.BaseInfo.LadderLevel;
            challenger.HeroId = challengerInfo.BaseInfo.HeroId;
            challenger.GodType = challengerInfo.BaseInfo.GodType;
            challenger.BattlePower = challengerInfo.DefensivePower;
            challenger.DefensivePower = challengerInfo.DefensivePower;
            challenger.Index = challenger.Index;
            challenger.Defensive.AddRange(challengerInfo.Defensive);
            challenger.DefPoses.AddRange(challengerInfo.DefPoses);
            challenger.HeroGod.AddRange(challengerInfo.HeroGod);
        }

        public CHALLENGER_INFO GetArenaRankBaseInfo(ServerModels.PlayerRankBaseInfo challenger)
        {
            CHALLENGER_INFO info = new CHALLENGER_INFO();
            info.Rank = challenger.Rank;
            info.IsRobot = challenger.IsRobot;
            info.Defensive.AddRange(challenger.Defensive);
            info.HeroGod.AddRange(challenger.HeroGod);
            info.DefensivePower = challenger.DefensivePower;

            info.BaseInfo = new PLAYER_BASE_INFO();
            info.BaseInfo.Uid = challenger.Uid;
            info.BaseInfo.Name = challenger.Name;
            info.BaseInfo.Sex = challenger.Sex;
            info.BaseInfo.Level = challenger.Level;
            info.BaseInfo.LadderLevel = challenger.LadderLevel;
            info.BaseInfo.HeroId = challenger.HeroId;
            info.BaseInfo.GodType = challenger.GodType;
            info.BaseInfo.Icon = challenger.Icon;
            info.BaseInfo.ShowDIYIcon = challenger.ShowDIYIcon;
            info.BaseInfo.IconFrame = challenger.IconFrame;
            info.BaseInfo.BattlePower64 = challenger.BattlePower;
            if (challenger.BattlePower < int.MaxValue)
            {
                info.BaseInfo.BattlePower = (int)challenger.BattlePower;
            }

            return info;
        }

        public CHALLENGER_INFO GetArenaRankBaseInfo()
        {
            CHALLENGER_INFO info = new CHALLENGER_INFO();
            info.Rank = ArenaMng.Rank;
            info.IsRobot = false;
            info.Defensive.AddRange(ArenaMng.DefensiveHeros.Keys.ToList());
            info.DefensivePower = ArenaMng.GetDefensiveBattlePower();

            info.BaseInfo = new PLAYER_BASE_INFO();
            info.BaseInfo.Uid = Uid;
            info.BaseInfo.Name = Name;
            info.BaseInfo.Sex = Sex;
            info.BaseInfo.Level = Level;
            info.BaseInfo.LadderLevel = ArenaMng.Level;
            info.BaseInfo.BattlePower = HeroMng.CalcBattlePower();
            info.BaseInfo.HeroId = HeroId;
            info.BaseInfo.GodType = GodType;
            info.BaseInfo.Icon = Icon;
            info.BaseInfo.ShowDIYIcon = ShowDIYIcon;
            //info.BaseInfo.IconFrame = icon;
            return info;
        }


        public void SenArenaManagerMessage()
        {
            MSG_ZGC_ARENA_MANAGER info = new MSG_ZGC_ARENA_MANAGER();
            info.Rank = ArenaMng.Rank;
            info.HistoryMaxRank = ArenaMng.HistoryMaxRank;
            info.Level = ArenaMng.Level;
            info.Score = ArenaMng.Score;
            info.HistoryMaxScore = ArenaMng.HistoryMaxScore;
            info.FightTotal = ArenaMng.FightTotal;
            info.WinTotal = ArenaMng.WinTotal;
            info.WinStreak = ArenaMng.WinStreak;
            info.FightTime = Timestamp.GetUnixTimeStampSeconds(ArenaMng.FightTime);
            foreach (var kv in ArenaMng.DefensiveHeros)
            {
                info.DefensiveHeros.Add(kv.Key);
                info.DefPoses.Add(kv.Value);
            }
            info.LevelReward.AddRange(ArenaMng.LevelReward);
            info.HistoryWinStreak = ArenaMng.HistoryWinStreak;
            Write(info);
        }

        public MSG_ZMZ_ARENA_MANAGER GetArenaTransformMsg()
        {
            MSG_ZMZ_ARENA_MANAGER msg = new MSG_ZMZ_ARENA_MANAGER();
            msg.Rank = ArenaMng.Rank;
            msg.Level = ArenaMng.Level;
            msg.Score = ArenaMng.Score;
            msg.HistoryMaxScore = ArenaMng.HistoryMaxScore;
            msg.FightTotal = ArenaMng.FightTotal;
            msg.WinTotal = ArenaMng.WinTotal;
            msg.WinStreak = ArenaMng.WinStreak;
            string timeString = ArenaMng.FightTime.ToString(CONST.DATETIME_TO_STRING);
            msg.FightTime = timeString;
            msg.DefensiveHeros = ArenaMng.GetDefensiveHeros();
            msg.LevelReward = ArenaMng.GetLevelRewards();
            msg.HistoryMaxRank = ArenaMng.HistoryMaxRank;
            msg.HistoryWinStreak = ArenaMng.HistoryWinStreak;

            foreach (var item in ArenaMng.ChallengerInfolist)
            {
                ZMZ_CHALLENGER_INFO info = GetArenaChallengerTransforMsg(item.Value);
                msg.List.Add(info);
            }

            return msg;
        }

        private ZMZ_CHALLENGER_INFO GetArenaChallengerTransforMsg(ServerModels.PlayerRankBaseInfo challenger)
        {
            ZMZ_CHALLENGER_INFO info = new ZMZ_CHALLENGER_INFO();
            info.Rank = challenger.Rank;
            info.IsRobot = challenger.IsRobot;
            info.Defensive.AddRange(challenger.Defensive);
            info.DefensivePower = challenger.DefensivePower;
            info.DefensivePoses.AddRange(challenger.DefPoses);
            info.Index = challenger.Index;

            info.BaseInfo = new ZMZ_PLAYER_BASE_INFO();
            info.BaseInfo.Uid = challenger.Uid;
            info.BaseInfo.Name = challenger.Name;
            info.BaseInfo.Sex = challenger.Sex;
            info.BaseInfo.Level = challenger.Level;
            info.BaseInfo.LadderLevel = challenger.LadderLevel;
            info.BaseInfo.BattlePower = challenger.BattlePower;
            info.BaseInfo.HeroId = challenger.HeroId;
            info.BaseInfo.Uid = challenger.Uid;
            info.BaseInfo.GodType = challenger.GodType;

            return info;
        }

        public void LoadArenaFromTransform(MSG_ZMZ_ARENA_MANAGER msg)
        {

            ArenaManagerInfo info = new ArenaManagerInfo();
            info.Rank = msg.Rank;
            info.Level = msg.Level;
            info.Score = msg.Score;
            info.FightTotal = msg.FightTotal;
            info.WinTotal = msg.WinTotal;
            info.WinStreak = msg.WinStreak;
            info.FightTime = msg.FightTime;
            info.DefensiveHeros = msg.DefensiveHeros;
            info.LevelReward = msg.LevelReward;
            info.HistoryMaxScore = msg.HistoryMaxScore;
            info.HistoryMaxRank = msg.HistoryMaxRank;
            info.HistoryWinStreak = msg.HistoryWinStreak;

            ArenaMng.Init(info);
            foreach (var item in msg.List)
            {
                PlayerRankBaseInfo challenger = LoadArenaChallengerTransforMsg(item);
                ArenaMng.AddArenaRankInfoList(challenger);
            }
        }

        private PlayerRankBaseInfo LoadArenaChallengerTransforMsg(ZMZ_CHALLENGER_INFO msg)
        {
            PlayerRankBaseInfo info = new PlayerRankBaseInfo();
            info.Rank = msg.Rank;
            info.IsRobot = msg.IsRobot;
            info.Defensive.AddRange(msg.Defensive);
            info.DefensivePower = msg.DefensivePower;
            info.DefPoses.AddRange(msg.DefensivePoses);
            info.Index = msg.Index;

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

        public void SetHeroInfoRobotSoulRings(List<HeroInfo> heroInfos)
        {
            foreach (var heroInfo in heroInfos)
            {
                heroInfo.RobotInfo = new RobotHeroInfo();

                Dictionary<int, SoulRingItem> soulRing = SoulRingManager.GetAllEquipedSoulRings(heroInfo.Id);
                if (soulRing != null)
                {
                    foreach (var curr in soulRing)
                    {
                        heroInfo.RobotInfo.SoulRings += string.Format("{0}:{1}:{2}:{3}:{4}|", curr.Value.Position, curr.Value.Level, curr.Value.SpecId, curr.Value.Year, curr.Value.Element);
                    }
                }

                List<SoulBone> soulBoneList = SoulboneMng.GetEnhancedHeroBones(heroInfo.Id);
                if (soulBoneList != null)
                {
                    List<string> soulBoneStr = soulBoneList.ToList().ConvertAll(x =>
                    {
                        List<int> specList = x.GetSpecList();
                        return specList.Count <= 0 ? x.TypeId.ToString() : x.TypeId.ToString() + ":" + string.Join(":", specList);
                    });
                    heroInfo.RobotInfo.SoulBones = string.Join("|", soulBoneStr);
                }

                HiddenWeaponItem weaponItem = HiddenWeaponManager.GetHeroEquipedHiddenWeapon(heroInfo.Id);
                if (weaponItem != null)
                {
                    heroInfo.RobotInfo.HiddenWeapon = $"{weaponItem.Id}:{weaponItem.Info.Star}";
                }

                List<EquipmentItem> equipmentItems = EquipmentManager.GetAllEquipedEquipments(heroInfo.Id);
                heroInfo.RobotInfo.Equipment = string.Join("|", equipmentItems.Select(x => x.Id));
            }
        }

        public PlayerRankBaseInfo GetChallengerRankBaseInfo()
        {
            PlayerRankBaseInfo info = new PlayerRankBaseInfo();
            info.Rank = ArenaMng.Rank;
            info.IsRobot = false;
            List<Tuple<int, int, Vec2>> defs = HeroMng.GetAllHeroPos();
            foreach (var item in defs)
            {
                info.Defensive.Add(item.Item1);
                info.DefPoses.Add(item.Item2);
                info.HeroGod.Add(HeroGodManager.GetHeroGodType(item.Item1));
            }
            info.DefensivePower = HeroMng.CalcBattlePower();

            info.Uid = Uid;
            info.Name = Name;
            info.Sex = Sex;
            info.Level = Level;
            info.LadderLevel = ArenaMng.Level;
            info.BattlePower = info.DefensivePower;
            info.HeroId = HeroId;
            info.GodType = GodType;

            info.NatureValues = NatureValues;
            info.NatureRatios = NatureRatios;

            return info;
        }

        private void CheckBroadCastArenaFirstLogin()
        {
            if (ArenaMng.Rank == 1)
            {
                BroadCastArenaFirstLogin();
            }
        }

        public CHALLENGER_PET_INFO GetChallengerPetInfo(PetInfo petInfo)
        {
            CHALLENGER_PET_INFO msg = new CHALLENGER_PET_INFO();
            msg.Id = petInfo.PetId;
            msg.Level = petInfo.Level;
            msg.Aptitude = petInfo.Aptitude;
            msg.BreakLevel = petInfo.BreakLevel;
            msg.Shape = petInfo.Shape;

            msg.Nature = GeneratePetNatureMsg(petInfo.Nature);
            return msg;
        }

        public CHALLENGER_PET_INFO GetArenaChallengerPetInfo(RobotPetInfo petInfo)
        {
            if (petInfo == null) return null;
            CHALLENGER_PET_INFO msg = new CHALLENGER_PET_INFO();
            msg.Id = petInfo.Id;
            msg.Level = petInfo.Level;
            msg.Aptitude = petInfo.Aptitude;
            msg.BreakLevel = petInfo.BreakLevel;
            msg.Shape = petInfo.Shape;

            msg.Nature = GetPetNatureMsg(petInfo.NatureList);
            return msg;
        }

        public static RobotPetInfo GetRobotPetInfo(CHALLENGER_PET_INFO petMsg)
        {
            if (petMsg == null)
            {
                return null;
            }
            RobotPetInfo robotInfo = new RobotPetInfo();
            robotInfo.Id = petMsg.Id;
            robotInfo.Level = petMsg.Level;        
            robotInfo.Aptitude = petMsg.Aptitude;
            robotInfo.BreakLevel = petMsg.BreakLevel;
            robotInfo.Shape = petMsg.Shape;
            robotInfo.NatureList[NatureType.PRO_MAX_HP] = petMsg.Nature.MaxHp.GetInt64();
            robotInfo.NatureList[NatureType.PRO_ATK] = petMsg.Nature.Atk.GetInt64();
            robotInfo.NatureList[NatureType.PRO_DEF] = petMsg.Nature.Def.GetInt64();
            robotInfo.NatureList[NatureType.PRO_HIT] = petMsg.Nature.Hit.GetInt64();
            robotInfo.NatureList[NatureType.PRO_FLEE] = petMsg.Nature.Flee.GetInt64();
            robotInfo.NatureList[NatureType.PRO_CRI] = petMsg.Nature.Cri.GetInt64();
            robotInfo.NatureList[NatureType.PRO_RES] = petMsg.Nature.Res.GetInt64();
            robotInfo.NatureList[NatureType.PRO_IMP] = petMsg.Nature.Imp.GetInt64();
            robotInfo.NatureList[NatureType.PRO_ARM] = petMsg.Nature.Arm.GetInt64();
            return robotInfo;
        }

        private void SyncArenaDefensivePetToRelation(int power, PetInfo petInfo)
        {
            //通知Relation
            MSG_ZR_UPDATE_ARENA_DEFENSIVE_PET msg = new MSG_ZR_UPDATE_ARENA_DEFENSIVE_PET();
            msg.PcUid = uid;
            msg.Power = power;
            msg.PetId = petInfo.PetId;
            server.SendToRelation(msg, Uid);
        }

        public void UpdatePlayerDefensivePetToRedis(int petId)
        {
            server.GameRedis.Call(new OperateUpdateArenaDefensivePet(Uid, petId));
        }
    }
}
