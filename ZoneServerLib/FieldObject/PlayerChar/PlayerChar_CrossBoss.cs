using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public CrossBossManager CrossBossInfoMng { get; set; }

        public void InitCrossBossManager()
        {
            CrossBossInfoMng = new CrossBossManager(this);
        }

        public void GetCrossBossInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_CROSS_BOSS_INFO msg = new MSG_ZR_GET_CROSS_BOSS_INFO();
            server.SendToRelation(msg, Uid);
        }


        public void GetCrossBossPassReward(int dungeonId)
        {
            MSG_ZGC_GET_CROSS_BOSS_PASS_REWARD response = new MSG_ZGC_GET_CROSS_BOSS_PASS_REWARD();

            if (CrossBossInfoMng.CounterInfo.PassReward != dungeonId) //(int)CrossRewardState.None)
            {
                Log.Warn("player {0} GetCrossBossPassReward failed: PassReward is {1}  not {2}",
                    uid, CrossBossInfoMng.CounterInfo.PassReward, dungeonId);
                response.Result = (int)ErrorCode.Already;
                Write(response);
                return;
            }

            CrossBossDungeonModel model = CrossBossLibrary.GetDungeonModel(dungeonId);
            if (model == null)
            {
                Log.Warn("player {0} GetCrossBossPassReward failed: no dungeonId info {1}", uid, dungeonId);//
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            ////检查TODO
            //CrossBossPassReward info = CrossBossLibrary.GetCrossBossPassReward(1);//
            //if (info == null)
            //{
            //    Log.Warn("player {0} GetCrossBossPassReward failed: no level info {1}", uid, 1);//
            //    response.Result = (int)ErrorCode.NotOpen;
            //    Write(response);
            //    return;
            //}

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(model.Reward);
            AddRewards(rewards, ObtainWay.CrossBossPassReward);

            //清理旧的配置
            CrossBossInfoMng.SetPassRewardState(0);

            //保存DB
            SyncDbUpdateCrossBossReward();

            //komoelog
            KomoeLogRecordPveFight(6, 4, dungeonId.ToString(), rewards.RewardList, 1);

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.RewardState = CrossBossInfoMng.CounterInfo.PassReward;
            Write(response);
        }


        public void GetCrossBossRankReward(int dungeonId)
        {
            MSG_ZGC_GET_CROSS_BOSS_RANK_REWARD response = new MSG_ZGC_GET_CROSS_BOSS_RANK_REWARD();

            if (CrossBossInfoMng.CounterInfo.Score != dungeonId) //(int)CrossRewardState.None)
            {
                Log.Warn("player {0} GetCrossBossRankReward failed: PassReward is {1}  not {2}",
                    uid, CrossBossInfoMng.CounterInfo.Score, dungeonId);
                response.Result = (int)ErrorCode.Already;
                Write(response);
                return;
            }

            CrossBossDungeonModel model = CrossBossLibrary.GetDungeonModel(dungeonId);
            if (model == null)
            {
                Log.Warn("player {0} GetCrossBossRankReward failed: no dungeonId info {1}", uid, dungeonId);//
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            //领取奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(model.FristRankReward);
            AddRewards(rewards, ObtainWay.CrossBossRankReward);

            //清理旧的配置
            CrossBossInfoMng.SetScoreState(0);

            //保存DB
            SyncDbUpdateCrossBossReward();

            //komoelog
            KomoeLogRecordPveFight(6, 4, dungeonId.ToString(), rewards.RewardList, 1);

            rewards.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.Score = CrossBossInfoMng.CounterInfo.Score;
            Write(response);
        }

        /// <summary>
        /// 保存段位奖励领取
        /// </summary>
        /// <param name="defensiveHeros"></param>
        private void SyncDbUpdateCrossBossReward()
        {
            server.GameDBPool.Call(new QueryUpdateCrossBossReward(Uid,
                CrossBossInfoMng.CounterInfo.PassReward, CrossBossInfoMng.CounterInfo.Score));
        }

        /// <summary>
        /// 挑战BOSS
        /// </summary>
        public void StartChallengeCrossBoss()
        {
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();

            int count = GetDungeonChallengeRestCount(MapType.CrossBoss);
            if (count <= 0)
            {
                Log.Warn($"player {uid} StartChallengeCrossBoss failed: count {count} error");
                response.Result = (int)ErrorCode.ChallengeCountNotEnough;
                Write(response);
                return;
            }
            if (HeroMng.CrossBossQueue.Count == 0)
            {
                Log.Warn($"player {uid} StartChallengeCrossBoss failed: queue {HeroMng.CrossBossQueue.Count} error");
                response.Result = (int)ErrorCode.NoDefensiveQueew;
                Write(response);
                return;
            }

            MSG_ZR_ENTER_CROSS_BOSS_MAP msg = new MSG_ZR_ENTER_CROSS_BOSS_MAP();
            server.SendToRelation(msg, Uid);
        }

        /// <summary>
        /// 获取挑战者信息
        /// </summary>
        /// <param name="page"></param>
        public void GetCrossBossChallenger()
        {
            MSG_ZR_CROSS_BOSS_CHALLENGER msg = new MSG_ZR_CROSS_BOSS_CHALLENGER();
            server.SendToRelation(msg, Uid);
        }
        /// <summary>
        /// 挑战守关
        /// </summary>
        /// <param name="page"></param>
        public void ChallengeCrossBossDefense()
        {
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();

            int count = GetDungeonChallengeRestCount(MapType.CrossBoss);
            if (count <= 0)
            {
                Log.Warn($"player {uid} ChallengeCrossBossDefense failed: count {count} error");
                response.Result = (int)ErrorCode.ChallengeCountNotEnough;
                Write(response);
                return;
            }
            if (HeroMng.CrossBossQueue.Count == 0)
            {
                Log.Warn($"player {uid} ChallengeCrossBossDefense failed: queue {HeroMng.CrossBossQueue.Count} error");
                response.Result = (int)ErrorCode.NoDefensiveQueew;
                Write(response);
                return;
            }

            MSG_ZR_CHALLENGE_CROSS_BOSS_MAP msg = new MSG_ZR_CHALLENGE_CROSS_BOSS_MAP();
            server.SendToRelation(msg, Uid);
        }

        /// <summary>
        /// 进入跨服BOSS
        /// </summary>
        public void EnterCrossBossMap(PlayerCrossFightInfo fightInfo)
        {
            int dungeonId = fightInfo.DungeonId;

            //到Relation获取一个对手，最后获得对手信息后才开始战斗
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = dungeonId;

            response.Result = (int)CanCreateDungeon(dungeonId);
            if (response.Result != (int)ErrorCode.Success)
            {
                Log.Write($"player {Uid} request to enter arena {dungeonId} failed: reason {response.Result}");
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

            CrossBossDungeon dungeonMap = dungeon as CrossBossDungeon;
            dungeonMap.BattleFpsManager?.SetBattleInfo(this, fightInfo);
            //dungeonMap.SetChallengeIntoType(fightInfo, Uid);
            
            //添加自己
            dungeonMap.AddAttackerMirror(this);
            //添加伙伴
            dungeonMap.AddCrossBossPartner(uid, fightInfo);

            // 成功 进入副本
            RecordEnterMapInfo(dungeonMap.MapId, dungeonMap.Channel, dungeonMap.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();
        }

        public void EnterCrossBossDefenseMap(PlayerCrossFightInfo fightInfo)
        {
            int dungeonId = CrossBossLibrary.DefenseMap;

            //到Relation获取一个对手，最后获得对手信息后才开始战斗
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = dungeonId;
            if (fightInfo == null)
            {
                Log.WarnLine("player {0} enter cross boss defense map failed: not find rank info", Uid);
                response.Result = (int)ErrorCode.NotFindChallengerInfo;
                Write(response);
                return;
            }

            response.Result = (int)CanCreateDungeon(dungeonId);
            if (response.Result != (int)ErrorCode.Success)
            {
                Log.Write($"player {Uid} request to enter boss defense {dungeonId} failed: reason {response.Result}");
                Write(response);
                return;
            }


            // 在当前zone创建副本
            DungeonMap dungeon = server.MapManager.CreateDungeon(dungeonId);
            if (dungeon == null)
            {
                Log.Write($"player {Uid} enter boss defense map request to create dungeon {dungeonId} failed: create dungeon failed");
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }

            //对手基本信息
            Write(GetCrossChallengerMsg(fightInfo));

            CrossBossDefenseMap dungeonMap = dungeon as CrossBossDefenseMap;
            dungeonMap.BattleFpsManager?.SetBattleInfo(this, fightInfo);
            dungeonMap.SetChallengeIntoType(fightInfo, Uid);
            
            long battlePower = HeroMng.GetBattlePower64(HeroQueueType.CrossBoss);

            //战力压制
            dungeon.SetBattlePowerSuppress( battlePower, fightInfo.GetBattlePower());

            if (battlePower > fightInfo.BattlePower)
            {
                //添加自己
                dungeonMap.AddAttackerMirror(this);
                //添加对手
                dungeonMap.AddCrossDefender(fightInfo);
            }
            else
            {
                //添加对手
                dungeonMap.AddCrossDefender(fightInfo);
                 //添加自己
                dungeonMap.AddAttackerMirror(this);
            }

            // 成功 进入副本
            RecordEnterMapInfo(dungeonMap.MapId, dungeonMap.Channel, dungeonMap.BeginPosition);
            RecordOriginMapInfo();
            OnMoveMap();
        }

        public void SyncCrosBossHeroQueuMsg(ChallengeIntoType type)
        {
            //获取到玩家1 信息，返回到Relation
            MSG_ZRZ_RETURN_BOSS_PLAYER_INFO addMsg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
            addMsg.Player1 = GetBossPlayerInfoMsg();
            addMsg.GetType = (int)type;
            server.SendToRelation(addMsg, Uid);
        }

        public void UpdateCrosBossScoreMsg(int dungeonId, ulong scoreHp, int defenserUid)
        {
            MSG_ZR_CHANGE_CROSS_BOSS_SCORE msg = new MSG_ZR_CHANGE_CROSS_BOSS_SCORE();
            msg.SiteId = dungeonId;
            msg.ScoreHp = scoreHp;
            msg.DefenseUid = defenserUid;
            server.SendToRelation(msg, Uid);

            MSG_ZRZ_RETURN_BOSS_PLAYER_INFO addMsg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
            addMsg.Player1 = GetBossPlayerInfoMsg();
            server.SendToRelation(addMsg, Uid);
        }

        public void SendCrossBossDefenseResult(DungeonResult result, PlayerCrossFightInfo fightInfo)
        {
           

            switch (result)
            {
                case DungeonResult.Success:
                    MSG_ZRZ_RETURN_BOSS_PLAYER_INFO addMsg = new MSG_ZRZ_RETURN_BOSS_PLAYER_INFO();
                    addMsg.Player1 = GetBossPlayerInfoMsg();
                    addMsg.GetType = (int)ChallengeIntoType.CrossBossSiteDefenseReturn;
                    addMsg.DungeonId = fightInfo.DungeonId;
                    server.SendToRelation(addMsg, Uid);

                    //更新挑战次数
                    UpdateCounter(CounterType.CrossBossActionCount, 1);
                    break;
                default:
                    break;
            }
        }
    }
}
