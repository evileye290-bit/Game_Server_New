using System.Collections.Generic;
using CommonUtility;
using DataProperty;
using Message.Manager.Protocol.MZ;
using System;
using System.Linq;
using Logger;
using EpPathFinding;
using System.IO;
using Message.Relation.Protocol.RZ;
using DBUtility;
using EnumerateUtility;
using ServerModels;
using ServerShared;
using Message.Zone.Protocol.ZM;
using ServerShared.Map;
using System.Collections.ObjectModel;
using ServerModels.Monster;

namespace ZoneServerLib
{
    public partial class FieldMap : BaseMap
    {

        public Dictionary<int, BaseMonsterGen> monsterGenList = new Dictionary<int, BaseMonsterGen>();

        private Dictionary<int, Monster> monsterList = new Dictionary<int, Monster>();
        public Dictionary<int, Monster> MonsterList
        { get { return monsterList; } }

        private List<int> monsterRemoveList = new List<int>();
        protected List<Monster> monsterAddList = new List<Monster>();

        private float monsterGrowth = 1.0f;
        public float MonsterGrowth { get { return monsterGrowth; } }

        public virtual Monster CreateMonster(int id, Vec2 position, BaseMonsterGen monGenerator, long hp)
        {
            //Log.Info("map {0} create monster {1} name {2}", mapID, monsterData.ID, monsterData.Name);
            MonsterModel monsterModel = MonsterLibrary.GetMonsterModel(id);
            if(monsterModel == null)
            {
                Log.Warn($"create monster {id} failed: no such model");
                return null;
            }
            Monster monster = new Monster(server);
            monster.Init(TokenId, this, monsterModel, monGenerator);
            monster.SetGenPos(position);

            if (hp > 0)
            {
                monster.SetNatureBaseValue(NatureType.PRO_HP, hp);
            }

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
            return monster;
        }

        public virtual Hero CreateMonsterHero(int id, Vec2 position, BaseMonsterGen monGenerator, long hp)
        {
            //Log.Info("map {0} create monster {1} name {2}", mapID, monsterData.ID, monsterData.Name);
            MonsterHeroModel monsterModel = MonsterLibrary.GetMonsterHeroModel(id);
            if (monsterModel == null)
            {
                Log.Warn($"create monster hero {id} failed: no such model");
                return null;
            }
            HeroInfo info = InitFromRobotInfo(monsterModel);

            Robot robot = GetMonsterRobot(info);
            Hero hero = robot.NewHero(server, robot, info);

            hero.InitMonsterHero(monsterModel, monGenerator);
            hero.GenAngle = monGenerator.Model.GenAngle;
            hero.CollisionPriority = monsterModel.PosCollision;
            hero.MonsterHeroSoulRingCount = monsterModel.SoulRingCount;
            hero.MonsterHeroSkillLevel = monsterModel.SkillLevel;
            hero.MonsterHeroYears = monsterModel.SoulRingYears;
            hero.SetPosition(position);

            if (hp > 0)
            {
                hero.SetNatureBaseValue(NatureType.PRO_HP, hp);
            }

            // 加到地图里
            AddHero(hero);

            hero.InitBaseBattleInfo();
            hero.AddToAoi();
            hero.BroadCastHp();
            hero.StartFighting();
            return hero;
        }

        public Robot GetMonsterRobot(HeroInfo info)
        {
            Robot robot = robotList.Values.FirstOrDefault(x => x.GetOwnerUid() < 0);
            if (robot == null)
            {
                robot = new Robot(server, -1);
                //robot.InitNatureExt(natureValues, natureRatios);//怪物不需要初始化这个额外属性
                robot.InitRobot(info.Id, info);
                robot.EnterMap(this);
                AddRobot(robot);
                robot.SetOwnerUid(-1);
                //robot.SetHeroPoses(heroPos);
                //robot.SetHeroInfos(infos);
            }

            return robot;
        }

        private void RemoveMonster()
        {
            if (monsterRemoveList.Count > 0)
            {
                foreach (var instance_id in monsterRemoveList)
                {
                    try
                    {
                        RemoveObjectSimpleInfo(instance_id);
                        Monster monster = GetMonster(instance_id);
                        if (monster != null)
                        {
                            monster.RemoveFromAoi();
                            monsterList.Remove(instance_id);
                            MonsterGenCheck();
                        }
                        MonsterGenCheck();
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                monsterRemoveList.Clear();
            }
        }

        public Monster GetMonster(int instanceId)
        {
            Monster monster;
            monsterList.TryGetValue(instanceId, out monster);
            return monster;
        }

        public void RemoveMonster(int instanceId)
        {
            monsterRemoveList.Add(instanceId);
        }

        public void UpdateMonster(float dt)
        {
            if (!NeedCheck()) return;

            //必须放在最开始，否则会导致后续波次怪全部生成
            foreach (var monster in monsterAddList)
            {
                AddMonster(monster);
            }
            monsterAddList.Clear();

            foreach (var regen in monsterGenList)
            {
                try
                {
                    regen.Value.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }

            foreach (var monster in monsterList)
            { 
                try
                {
                    monster.Value.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        public void AddMonster(Monster monster)
        {
            monsterList.Add(monster.InstanceId, monster);
            AddObjectSimpleInfo(monster.InstanceId, TYPE.MONSTER);
        }

        private void InitMonsterGens()
        {
            if (!NeedCheck()) return;

            List<MonsterGenModel> modelList = MonsterGenLibrary.GetModelsByMap(MapId);
            if (modelList == null) return;
            foreach (var model in modelList)
            {
                if (model.GenType == MonsterGenType.GodHeroCount) continue;

                BaseMonsterGen monsterGen = MonsterGenFactory.CreateMonsterGen(this, model);
                monsterGenList.Add(monsterGen.Id, monsterGen);
            }
        }

        public void IniMonsterGensByGodHeroCount(int count)
        {
            if (!NeedCheck()) return;

            List<MonsterGenModel> modelList = MonsterGenLibrary.GetModelsByMap(MapId);
            if (modelList == null) return;

            int needGodHeroCount = 0;
            foreach (var model in modelList)
            {
                if (model.GenType != MonsterGenType.GodHeroCount) continue;

                if (!int.TryParse(model.GenParam, out needGodHeroCount)) continue;

                if (count != needGodHeroCount) continue;

                //只构造与其成神hero数量相等的gen
                BaseMonsterGen monsterGen = MonsterGenFactory.CreateMonsterGen(this, model);
                monsterGenList.Add(monsterGen.Id, monsterGen);
            }
        }

        public int GetAliveMonsterCountByGenId(int genId)
        {
            //包含已经添加到列表中的和待添加到列表中的
            int count = monsterList.Values.Where(x => x.Generator.Id == genId && !x.IsDead).Count() + 
                monsterAddList.Where(x => x.Generator.Id == genId).Count();
            return count;
        }

        public BaseMonsterGen GetMonsterGen(int genId)
        {
            BaseMonsterGen gen = null;
            monsterGenList.TryGetValue(genId, out gen);
            return gen;
        }

        public void MonsterGenCheck()
        {
            foreach(var gen in monsterGenList)
            {
                if (gen.Value.GenType == MonsterGenType.UntilMonsterLess)
                {
                    gen.Value.CheckGen();
                }
            }
        }

        public void ReleaseMonsterResource()
        {
            //解除 monsterGen中map引用
            foreach (var id in monsterGenList.Keys.ToList())
            {
                monsterGenList[id].Dispose();
            }
            monsterGenList.Clear();

            // 移除并释放monster资源
            foreach (var monster in monsterAddList)
            {
                monster.Dispose();
            }
            monsterAddList.Clear();

            foreach (var kv in monsterList)
            {
                kv.Value.Dispose();
            }
            monsterList.Clear();
        }

        public void SetMonsterGrowth(float growth)
        {
            monsterGrowth = growth;
        }
    }
}