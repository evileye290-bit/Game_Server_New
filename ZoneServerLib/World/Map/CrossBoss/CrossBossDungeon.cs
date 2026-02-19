using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using EnumerateUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class CrossBossDungeon : TeamDungeonMap
    {
        private ulong damageHp;
        private PlayerCrossFightInfo crossFightInfo;
        private bool isKillBoss = false;
        private int playerUid { get; set; }
        public CrossBossDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            IsSpeedUpDungeon = true;
        }

        public override void OnPlayerLeave(PlayerChar player, bool reEnter = false)
        {
            //掉线不扣次数
            if (!player.IsOnline())
            {
                isQuitDungeon = true;
            }
            base.OnPlayerLeave(player, reEnter);
        }

        public void AddAttackerMirror(PlayerChar player)
        {
            player.IsAttacker = true;

            foreach (var index in player.HeroMng.CrossBossQueue.Keys.OrderBy(x=>x))
            {
                var queue = player.HeroMng.CrossBossQueue[index];
                Dictionary<int, int> poses = new Dictionary<int, int>();
                List<HeroInfo> heros = new List<HeroInfo>();

                foreach (var pos in queue)
                {
                    int posId = pos.Key;
                    HeroInfo heroInfo = pos.Value;
                    poses.Add(heroInfo.Id, posId);
                    heros.Add(heroInfo);
                }

                if (player.Uid > 0)
                {
                    Robot robot = Robot.CopyFromPlayer(server, player);
                    robot.IsAttacker = true;
                    robot.EnterMap(this);
                    robot.SetOwnerUid(player.Uid);
                    base.AddRobot(robot);

                    robot.SetHeroPoses(poses);
                    robot.SetHeroInfos(heros);
                    robot.CopyHeros2CrossMap(player);
                }
                else
                {
                    AddRobotAndHeros(true, heros, player.Uid, player.NatureValues, player.NatureRatios, poses);
                }
            }
        }
        
        public void AddCrossBossPartner(int uid, PlayerCrossFightInfo info)
        {
            playerUid = uid;
            crossFightInfo = info;
            Log.Write($"player {uid} CrossBossDungeon init hp {info.Hp} max hp {info.MaxHp}");

            foreach (var index in info.HeroQueue.Keys.OrderBy(x=>x))
            {
                var queue = info.HeroQueue[index];
                List<HeroInfo> infos = new List<HeroInfo>();
                Dictionary<int, int> heroPoses = new Dictionary<int, int>();
                foreach (var kv in queue)
                {
                    heroPoses.Add(kv.Value.HeroId, kv.Key);

                    HeroInfo heroInfo = RobotManager.InitFromRobotInfo(kv.Value);
                    heroInfo.CrossBossQueueNum = index;
                    heroInfo.CrossBossPositionNum = kv.Key;
                    infos.Add(heroInfo);
                }

                HeroInfo temp = infos.First();
                temp.RobotInfo.Name = info.Name;
                temp.RobotInfo.Sex = info.Sex;
                AddRobotAndHeros(true, infos, info.Uid == uid ? info.Uid * -1 : info.Uid, info.NatureValues, info.NatureRatios, heroPoses);
            }
        }

        public override Vec2 CalcBeginPos(int i, FieldObject field)
        {
            int index = AttackerPosIndex > DungeonModel.PlayerPos.Count ? 1 : AttackerPosIndex;
            if (field is PlayerChar)
            {
                index = 1;
            }

            return DungeonModel.PlayerPos[index];
        }

        public override Monster CreateMonster(int id, Vec2 position, BaseMonsterGen monGenerator, long hp)
        {
            MonsterModel monsterModel = MonsterLibrary.GetMonsterModel(id);
            if (monsterModel == null)
            {
                Log.Warn($"create monster {id} failed: no such model");
                return null;
            }
            
            Monster monster = new Monster(server);
            monster.Init(TokenId, this, monsterModel, monGenerator);
            monster.SetGenPos(position);

            monster.SetNatureBaseValue(NatureType.PRO_MAX_HP, (long)crossFightInfo.MaxHp);
            monster.SetNatureBaseValue(NatureType.PRO_HP, (long)crossFightInfo.Hp);
            
            monster.AddToAoi();
            monsterAddList.Add(monster);
            if(IsDungeon)
            {
                DungeonMap dungeon = monster.CurrentMap as DungeonMap;
                if(dungeon != null && dungeon.State > DungeonState.Open)
                {
                    monster.StartFighting();
                }
            }

            if (crossFightInfo.AddBuff)
            {
                CrossBossDungeonModel model = CrossBossLibrary.GetDungeonModel(DungeonModel.Id);
                if (model?.BuffId > 0)
                {
                    monster.AddBuff(monster, model.BuffId, 1);
                }
            }

            return monster;
        }

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;
            PlayerChar owner = hero.Owner as PlayerChar;

            // 加到地图里
            AddHero(hero);

            //玩家的两只队伍占位置1，2，镜像的队伍占位置 3，4
            int pcCount = AttackerPosIndex;
            int pos = hero.HeroInfo.CrossBossPositionNum;

            Vec2 tempPosition = HeroLibrary.GetHeroPos(pos);
            hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(pos);

            if (HeroList.Count >= owner?.HeroMng.CallHeroCount())
            {
                OnePlayerDone = true;//此时至少有一个玩家连同其hero加载完了
            }

            if (tempPosition != null)
            {
                tempPosition = DungeonModel.GetPosition4Count(pcCount, tempPosition);
            }

            hero.SetPosition(tempPosition);
            hero.InitBaseBattleInfo();

            if (add2Aoi)
            {
                hero.AddToAoi();
                hero.BroadCastHp();
            }
        }

        protected override void SaveHeroAndMonsterInfo()
        {
            if (MonsterList.Count > 0)
            {
                Monster monster = MonsterList.Values.First();
                ulong currentHp = (ulong)monster.GetNatureValue(NatureType.PRO_HP);
                if (crossFightInfo.Hp < currentHp)
                {
                    damageHp = 0;
                    Log.ErrorLine($"player {playerUid} CrossBossDungeon hp errpr: init hp {crossFightInfo.Hp} and currentHp {currentHp}");
                }
                else
                {
                    damageHp = crossFightInfo.Hp - currentHp;
                }
            }
            else
            {
                if(isKillBoss && crossFightInfo.Hp > 0)
                {
                    damageHp = crossFightInfo.Hp;
                }
                else
                {
                    damageHp = 0;
                    Log.ErrorLine($"player {playerUid} CrossBossDungeon hp errpr: init hp {crossFightInfo.Hp} and isKillBoss {isKillBoss}");
                }
            }
        }

        public override void Stop(DungeonResult result)
        {
            SetSpeedUp(false);
            isKillBoss = result == DungeonResult.Success;
            base.Stop(DungeonResult.Success);
        }

        protected override void Success()
        {
            DoReward();

            PlayerChar player = PcList.Values.FirstOrDefault();
            if(player == null) return;
            
            NotifySpeedUpEnd(player);

            try
            {
                if (!isQuitDungeon && damageHp > 0)
                {
                    SendClientReward(player);

                    player.UpdateCounter(CounterType.CrossBossActionCount, 1);
                    player.UpdateCrosBossScoreMsg(crossFightInfo.DungeonId, damageHp, crossFightInfo.Uid);

                    //副本类型任务计数
                    PlayerAddTaskNum(player);

                    //增加伙伴经验
                    player.AddHeroExp(DungeonModel.HeroExp);

                    //日志
                    player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                    //komoelog
                    player.KomoeLogRecordPveFight(6, 1, DungeonModel.Id.ToString(), null, 1, GetFinishTime());
                }
                else
                {
                    Log.ErrorLine($"player {player.Uid} CrossBossDungeon not send score: isQuitDungeon {isQuitDungeon} and damageHp {damageHp}");

                    MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
                    rewardMsg.DungeonId = DungeonModel.Id;
                    rewardMsg.Result = (int)DungeonResult.Failed;
                    rewardMsg.CrossBossDamage = damageHp.ToString();
                    player.Write(rewardMsg);
                }
            }
            catch (Exception ex)
            {
                Log.Alert(ex);
            }
        }

        private void SendClientReward(PlayerChar player)
        {
            if(isQuitDungeon) return;

            RewardManager mng = GetFinalReward(player.Uid);
            float growthFacto = 0;
            int addGoldNum = 0;
            int maxGold = 0;
            CrossBossDungeonModel model = CrossBossLibrary.GetDungeonModel(DungeonModel.Id);
            if (model != null)
            {
                growthFacto = model.GoldCoinGrowthFactor;
                addGoldNum = model.GoldCoin;
                maxGold = model.MaxGold;
            }
            int num = (int) (damageHp * growthFacto) + addGoldNum;
            if (maxGold > 0)
            {
                num = Math.Min(maxGold, num);
            }
            mng.AddReward(new ItemBasicInfo((int)RewardType.Currencies, (int)CurrenciesType.gold,num));
            mng.BreakupRewards();

            player.AddRewards(mng, ObtainWay.CrossBossDungeonReward, DungeonModel.Id.ToString());

            MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
            mng.GenerateRewardMsg(rewardMsg.Rewards);
            rewardMsg.DungeonId = DungeonModel.Id;
            rewardMsg.Result = (int)DungeonResult.Success;
            rewardMsg.CrossBossDamage = damageHp.ToString();
            
            int pcAddScore = (int)((damageHp * CrossBossLibrary.ScoreParamA) + CrossBossLibrary.ScoreParamB);
            rewardMsg.CrossBossScore = pcAddScore;
            
            player.CheckCacheRewardMsg(rewardMsg);
            
            ResetReward();
        }
    }
}
