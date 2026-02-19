using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerModels;

namespace ZoneServerLib
{
    public class ThemeBossDungeon : DungeonMap
    {
        private bool hasBuff = false;
        private double lastDegree;
        private double degree = 0.0;
        private long monsterCurMaxHp;
        private long monsterCurHp;
        private long monsterRealMaxHp = 1;
        private bool killed = false;
        public ThemeBossDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;

            // 加到地图里
            AddHero(hero);

            int pcCount = hero.HeroInfo.ThemeBossQueueNum <= 1 ? AttackerPosIndex : DefenderPosIndex;

            PlayerChar owner = hero.Owner as PlayerChar;

            int pos = hero.HeroInfo.ThemeBossPositionNum;

            Vec2 tempPosition = HeroLibrary.GetHeroPos(pos);

            hero.CollisionPriority = HeroLibrary.GetHeroPosCollisions(pos);

            if (HeroList.Count >= owner.HeroMng.CallHeroCount())
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

        protected override void Start()
        {
            base.Start();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (hasBuff)
            {
                int buffId = ThemeBossLibrary.GetThemeBossBuffByPeriod(player.ThemeBossManager.Period);
                foreach (var hero in HeroList)
                {
                    HeroAddBuff(hero.Value, buffId);
                }
            }
            lastDegree = player.ThemeBossManager.Degree;
            degree = 0.0;
        }     

        public override void Stop(DungeonResult result)
        {
            //记录击杀进度
            RecordKillDegree();
            base.Stop(result);
        }

        protected override void Failed()
        {
            base.Failed();

            PlayerChar player;
            foreach (var kv in PcList)
            {
                player = kv.Value;
                if (!isQuitDungeon)
                {
                    player.UpdateCounter(CounterType.ThemeBossCount, 1);
                    player.AddThemeBossDegree(degree);
                }
                int pointState = 2;
                if (isQuitDungeon)
                {
                    pointState = 3;
                }
                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
                //komoelog
                player.KomoeLogRecordPveFight(9, 1, DungeonModel.Id.ToString(), null, pointState, GetFinishTime());
            }
        }

        protected override void Success()
        {
            DoReward();
            PlayerChar player = null;
            foreach (var kv in PcList)
            {
                try
                {
                    player = kv.Value;
                    RewardManager mng = GetFinalReward(player.Uid);
                    mng.BreakupRewards();
                 
                    player.AddRewards(mng, ObtainWay.ThemeBoss);

                    MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
                    mng.GenerateRewardMsg(rewardMsg.Rewards);
                    rewardMsg.DungeonId = DungeonModel.Id;
                    rewardMsg.Result = (int)DungeonResult;
                    player.Write(rewardMsg);

                    player.UpdateCounter(CounterType.ThemeBossCount, 1);
                    player.AddThemeBossDegree(degree, killed);
                    player.CheckSendThemeBossTitle();

                    //副本类型任务计数
                    PlayerAddTaskNum(player);

                    //增加伙伴经验
                    player.AddHeroExp(DungeonModel.HeroExp);

                    //日志
                    player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                    //komoelog
                    player.KomoeLogRecordPveFight(9, 1, DungeonModel.Id.ToString(), mng.RewardList, 1, GetFinishTime());
                }
                catch (Exception ex)
                {
                    Log.Alert(ex);
                }
            }

            ResetReward();
        }

        public override Monster CreateMonster(int id, Vec2 position, BaseMonsterGen monGenerator, long hp)
        {
            Monster monster = base.CreateMonster(id, position, monGenerator, hp);
            if (monster != null && monster.MonsterModel.IsBoss == 1)
            {
                ResetHpByDegree(lastDegree, monster);
                monsterRealMaxHp = monster.MonsterModel.NatureList[NatureType.PRO_MAX_HP];
            }
            return monster;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            RecordMonsterCurHp();
        }

        public void SetBuffState(bool hasBuff)
        {
            this.hasBuff = hasBuff;
        }

        private void HeroAddBuff(Hero hero, int buffId)
        {
            FieldObject fieldObject = hero as FieldObject;
            if (fieldObject == null)
            {
                return;
            }
            fieldObject.AddBuff(fieldObject, buffId, 1);//
        }

        private void RecordKillDegree()
        {
            double killDegree = (double)(monsterCurMaxHp - monsterCurHp) / monsterRealMaxHp;
            degree += killDegree;
            if (monsterCurHp == 0)
            {
                killed = true;
            }
        }

        private void ResetHpByDegree(double lastDegree, Monster monster)
        {
            long maxHp = monster.GetNatureBaseValue(NatureType.PRO_MAX_HP);
            if (lastDegree == 0.0)
            {
                monsterCurMaxHp = maxHp;
            }
            else
            {
                monsterCurMaxHp = (long)(maxHp * (1 - lastDegree * 0.01));
            }
            monster.SetNatureBaseValue(NatureType.PRO_HP, monsterCurMaxHp);
        }

        private void RecordMonsterCurHp()
        {
            Monster monster = MonsterList.Values.Where(x=>x.MonsterModel.IsBoss == 1).FirstOrDefault();
            if (monster != null && monster.Nature.GetNatureList().Count > 0)
            {
                monsterCurHp = monster.GetNatureValue(NatureType.PRO_HP);
            }
        }

        protected override Vec2 GetPetPostion(Pet pet)
        {
            Vec2 tempPosition;
            //设置位置，一定在aoi前
            PlayerChar owner = pet.Owner as PlayerChar;
            int petIndex = pet.QueueNum <= 1 ? AttackerPosIndex : DefenderPosIndex;

            tempPosition = PetLibrary.PetConfig.GetPetPosition();

            if (PetList.Count >= owner.PetManager.GetCallPetCount())
            {
                OnePlayerPetDone = true;//此时至少有一个玩家连同其pet加载完了
            }

            if (tempPosition != null)
            {
                tempPosition = DungeonModel.GetPosition4Count(petIndex, tempPosition);
            }
            return tempPosition;
        }
    }
}
