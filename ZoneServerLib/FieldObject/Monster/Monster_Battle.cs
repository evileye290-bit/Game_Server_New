using CommonUtility;
using EnumerateUtility;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class Monster
    {
        public override float DeadDelay
        { get { return monsterModel.DeadDelay; } }

        // 副本逻辑开始时调用此接口，开始战斗
        public override void StartFighting()
        {
            base.StartFighting();
            InitAI();
            InitHolaEffect();

            CurDungeon.HuntingPeriodBuffEffect(this);

            //检测存货单位
            DispatchAliveCountMessage();
        }

        //public override bool IsEnemy(FieldObject target)
        //{
        //    return IsAttacker != target.IsAttacker;
        //    //switch (target.FieldObjectType)
        //    //{
        //    //    case TYPE.MONSTER:
        //    //        return false;
        //    //    case TYPE.PC:
        //    //    case TYPE.PET:
        //    //    case TYPE.HERO:
        //    //    case TYPE.ROBOT:
        //    //        return true;
        //    //    default:
        //    //        return false;
        //    //}
        //}

        //public override bool IsAlly(FieldObject target)
        //{
        //    return IsAttacker == target.IsAttacker;
        //    //switch (target.FieldObjectType)
        //    //{
        //    //    case TYPE.MONSTER:
        //    //        return true;
        //    //    case TYPE.PC:
        //    //    case TYPE.PET:
        //    //    case TYPE.HERO:
        //    //    case TYPE.ROBOT:
        //    //        return false;
        //    //    default: return true;
        //    //}
        //}

        public override long OnHit(FieldObject caster, DamageType damageType, long damage, ref bool immune, float multipleParam = 1)
        {
            damage = base.OnHit(caster, damageType, damage, ref immune, multipleParam);
            //if (damage > 0 && caster != null && IsEnemy(caster))
            //{
            //    int hateValue = (int)(damage * caster.HateRatio + 0.5f);
            //    hateManager?.AddHate(caster, hateValue);
            //    if (caster.IsPlayer)
            //    {
            //        PlayerChar player = caster as PlayerChar;
            //        foreach (var hero in player.HeroMng.GetHeros())
            //        {
            //            if (caster.IsEnemy(hero.Value))
            //            {
            //                hero.Value.HateManager?.EnsureHasHate(this);//如果主角打怪，确保hero立刻有仇恨
            //            }
            //        }
            //    }
            //    if (caster.IsRobot)
            //    {
            //        Robot robot = caster as Robot;
            //        foreach (var hero in robot.GetHeros())
            //        {
            //            if (caster.IsEnemy(hero.Value))
            //            {
            //                hero.Value.HateManager?.EnsureHasHate(this);
            //            }
            //        }
            //    }
            //}
            return damage;
        }

        public override void OnDead()
        {
            //需要先累计奖励，不然有可能受到死亡消息副本就提前结束 造成奖励无法累计
            DropReward();
            base.OnDead();
            currentMap.RemoveMonster(instanceId);
        }

        public override void OnChanged()
        {
            //不触发死亡，该方法的主要作用是，变身，并需要继承血量

            //副本不存在、未在正常开启状态不接收消息
            DungeonMap dungeonMap = currentMap as DungeonMap;
            if (dungeonMap == null || dungeonMap.State != DungeonState.Started)
            {
                return;
            }

            StopFighttingByReplease();
            dungeonMap.OnFieldObjectDead(this);

            currentMap.RemoveMonster(instanceId);
        }

        //副本结束时，还活着的怪掉落奖励
        public void DropRewardOnBattleEndIfAlive()
        {
            DropReward();

            //掉落后将dropData设置为null，避免重复掉落
            dropData = null;
        }

        public void DropReward()
        {
            if (dropData != null)
            {
                CurDungeon?.OnMonsterDropItems(KillerId, dropData.ID);
            }
        }

        //private void ClearSurroundInfos()
        //{
        //    directEnemys.Clear();
        //    enemysDuePos.Clear();
        //    enemys.Clear();
        //}

        ////instanceId 做key 用于站位
        //private SortedDictionary<int, Hero> directEnemys = new SortedDictionary<int, Hero>();
        ////敌人获得的相对位置
        //private SortedDictionary<int, Vec2> enemysDuePos = new SortedDictionary<int, Vec2>();
        ////循环队列，按逆时针排列
        //private Queue<Hero> enemys = new Queue<Hero>();

        //public SortedDictionary<int, Hero> DirectEnemys
        //{ get { return directEnemys; } }

        //public Queue<Hero> Enemys
        //{ get { return enemys; } }

        //public SortedDictionary<int, Vec2> EnemysDuePos
        //{ get { return enemysDuePos; } }

        //public void RemoveEnemy(Hero hero)
        //{
        //    if (hero == null)
        //    {
        //        return;
        //    }
        //    directEnemys.Remove(hero.InstanceId);
        //    enemysDuePos.Remove(hero.InstanceId);
        //    while (enemys.Contains(hero))
        //    {
        //        Hero temp=enemys.Peek();
        //        if (temp == hero)
        //        {
        //            enemys.Dequeue();
        //        }
        //        else
        //        {
        //            enemys.Enqueue(enemys.Dequeue());
        //        }
        //    }
        //}

        //public void AddEnemyAfter(Hero before,Hero hero,Vec2 pos)// pos用于占坑，否则无法计算
        //{
        //    if (hero == null)
        //    {
        //        return;
        //    }
        //    if (before == null)
        //    {
        //        directEnemys.Add(hero.InstanceId, hero);
        //        enemysDuePos.Add(hero.InstanceId, pos);
        //        enemys.Enqueue(hero);
        //        return;
        //    }
        //    directEnemys.Add(hero.InstanceId, hero);
        //    enemysDuePos.Add(hero.InstanceId, pos);
        //    while (enemys.Contains(before))
        //    {
        //        Hero temp = enemys.Dequeue();
        //        if (temp == before)
        //        {
        //            enemys.Enqueue(temp);
        //            enemys.Enqueue(hero);
        //            break;
        //        }
        //        else
        //        {
        //            enemys.Enqueue(temp);
        //        }
        //    }
        //}


        public bool CheckCollision( Vec2 pos)
        {
            //foreach (var kv in currentMap.HeroList)
            //{
            //    Hero hero = kv.Value;
            //    //if (hero == this || GetOwner() != hero.GetOwner())
            //    //{
            //    //    continue;
            //    //}
            //    //计算是否碰撞
            //    if (Vec2.GetDistance(pos, hero.Position) < MonsterModel.Radius + hero.HeroModel.Radius)
            //    {
            //        Logger.Log.Debug($"{hero.HeroId} radius {hero.HeroModel.Radius} collision with {MonsterModel.Id} radius {MonsterModel.Radius}");
            //        return true;
            //    }
            //}
            foreach (var kv in currentMap.MonsterList)
            {
                Monster monster = kv.Value;
                if (monster == this)
                {
                    continue;
                }
                if (monster.MonsterModel.Radius > MonsterModel.Radius)
                {
                    //计算是否碰撞
                    if (Vec2.GetDistance(pos, monster.Position) < MonsterModel.Radius + monster.MonsterModel.Radius)
                    {
                        Logger.Log.Debug($"{monster.MonsterModel.Id} radius {monster.MonsterModel.Radius} collision with {MonsterModel.Id} radius {MonsterModel.Radius}");
                        return true;
                    }
                }
            }
            return false;
        }

        public Tuple<bool, Vec2> GetNonCollisionPos(FieldObject target, Vec2 pos, float skillDis, float deltaLength = 0.1f)
        {
            //float dis = target.Radius + HeroModel.Radius + skillDis;
            //int count = maxCount;
            int allCount = 0;
            float temp = MonsterModel.Radius;

            temp = target.Radius + MonsterModel.Radius + skillDis - deltaLength;

            Vec2 delta = Position - target.Position;
            delta = delta * temp / (float)delta.GetLength();
            float rad = 0;
            while (rad < 360f)
            {
                List<Vec2> availableVecs = new List<Vec2>();

                rad += 5f;
                allCount++;

                Vec2 tempPos = GetVec2FromTo(delta, rad) + target.Position;
                Vec2 tempPos1 = GetVec2FromTo(delta, -rad) + target.Position;
                if (!CheckCollision(tempPos))
                {
                    Logger.Log.Debug($"monster {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
                    availableVecs.Add(tempPos);
                }
                else if (!CheckCollision(tempPos1))
                {
                    Logger.Log.Debug($"monster {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
                    availableVecs.Add(tempPos1);
                }
                else
                {
                    Logger.Log.Debug($"monster {InstanceId} randomPos {tempPos1} collision for monster {target.InstanceId} with {allCount} allcount");
                    Logger.Log.Debug($"monster {InstanceId} randomPos {tempPos} collision for monster {target.InstanceId} with {allCount} allcount");
                }

                bool got = false;
                Vec2 ans = null;
                if (availableVecs.Count > 0)
                {
                    got = true;
                    ans = availableVecs[0];
                }
                if (got)
                {
                    return Tuple.Create(true, ans);
                }
                //return Tuple.Create(true, tempPos + target.Position);
            }
            Logger.Log.Debug($"monster {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount in default");
            return Tuple.Create(false, Position);
        }

        private Vec2 GetRandomVec2(float length)
        {
            Vec2 vec = new Vec2();
            double dis = RAND.RangeFloat(0, length);
            double angle = RAND.RangeFloat(0, 1f) * Math.PI * 2;
            vec.x = (float)(dis * Math.Sin(angle));
            vec.y = (float)(dis * Math.Cos(angle));
            return vec;
        }

        private Vec2 GetRandomVec2FromTo(float length, float toLength)
        {
            Vec2 vec = new Vec2();
            double dis = RAND.RangeFloat(length, toLength);
            double angle = RAND.RangeFloat(0, 1f) * Math.PI * 2;
            vec.x = (float)(dis * Math.Sin(angle));
            vec.y = (float)(dis * Math.Cos(angle));
            return vec;
        }

        private Vec2 GetVec2FromTo(Vec2 temp, double rad)
        {
            rad = rad * Math.PI / 180;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double x = temp.X * cos - temp.Y * sin;
            double y = temp.X * sin + temp.Y * cos;

            return new Vec2((float)x, (float)y);
        }
    }
}
