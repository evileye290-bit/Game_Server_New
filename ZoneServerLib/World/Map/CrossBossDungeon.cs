using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerShared;
using System;
using System.Linq;
using ServerModels;

namespace ZoneServerLib
{
    public class CrossBossDungeon : TeamDungeonMap
    {
        private long damageHp;
        private long monsterHp;

        public CrossBossDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }
        
        public void AddAttackerMirror(PlayerChar player)
        {
            AddMirrorRobot(true,player);
        }

        public void SetMonsterHp(long hp)
        {
            monsterHp = hp;
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
            // if (hpRatio == 0)
            // {
            //     //手动触发怪物死亡消息，避免副本无法结算问题
            //     //map trigger
            //     monster.SetCurrMap(this);
            //     monster.SetInstanceId(TokenId);
            //     monster.SetMonsterGenerator(monGenerator);
            //     DispatchFieldObjectDeadMsg(monster);
            //
            //     OnFieldObjectDead(monster);
            //     return null;
            // }
            
            monster.Init(TokenId, this, monsterModel, monGenerator);
            monster.SetGenPos(position);

            monster.SetNatureBaseValue(NatureType.PRO_HP, monsterHp);
            
            return monster;
        }

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;
            PlayerChar owner = hero.Owner as PlayerChar;

            // 加到地图里
            AddHero(hero);

            //玩家的两只队伍占位置1，2，镜像的队伍占位置 3，4
            int pcCount = AttackerPosIndex + hero.HeroInfo.ThemeBossQueueNum;
            int pos = hero.HeroInfo.ThemeBossPositionNum;
            
            //todo 需要使用跨服BOss的队伍信息
            Log.Warn("todo need use cross boss hero queue, please check");
            
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

        public override void Stop(DungeonResult result)
        {
            base.Stop(result);

            Monster monster = MonsterList.Values.First();
            long hp = monster.GetNatureValue(NatureType.PRO_HP);

            damageHp = monsterHp - hp;
        }

        protected override void Success()
        {
            DoReward();
            
            foreach (var kv in PcList)
            {
                try
                {
                    PlayerChar player = kv.Value;
                    RewardManager mng = GetFinalReward(player.Uid);
                    mng.BreakupRewards();
                 
                    player.AddRewards(mng, ObtainWay.ThemePassLevelReward);

                    MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
                    mng.GenerateRewardMsg(rewardMsg.Rewards);
                    rewardMsg.DungeonId = DungeonModel.Id;
                    rewardMsg.Result = (int)DungeonResult;
                    player.Write(rewardMsg);

                    // player.UpdateCounter(CounterType.ThemeBossCount, 1);
                    // player.AddThemeBossDegree(degree, killed);
                    // player.CheckSendThemeBossTitle();

                    //副本类型任务计数
                    PlayerAddTaskNum(player);

                    //增加伙伴经验
                    player.AddHeroExp(DungeonModel.HeroExp);

                    //日志
                    player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
                }
                catch (Exception ex)
                {
                    Log.Alert(ex);
                }
            }

            ResetReward();
        }
        
        protected override void Failed()
        {
            base.Failed();

            foreach (var kv in PcList)
            {
                PlayerChar player = kv.Value;
                if (!isQuitDungeon)
                {
                    // player.UpdateCounter(CounterType.ThemeBossCount, 1);
                }
                
                int pointState = 2;
                if (isQuitDungeon)
                {
                    pointState = 3;
                }
                
                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
            }
        }
        
    }
}
