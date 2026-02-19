using CommonUtility;
using EnumerateUtility;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class SkillSplashChecker
    {
        public static bool CheckTargetValid(FieldObject target, int withoutId, bool alive)
        {
            if (alive && target.IsDead)
            {
                return false;
            }
            if (!alive && !target.IsDead)
            {
                return false;
            }
            if (target.InstanceId == withoutId)
            {
                return false;
            }
            return true;
        }

        public static bool CheckTargetInCircle(FieldObject caster, FieldObject target, Vec2 center, float range, int withoutId, bool alive)
        {
            if (!CheckTargetValid(target, withoutId, alive))
            {
                return false;
            }
            float casterRadius = caster == null ? 0f : caster.Radius;
            float targetRadius = target == null ? 0f : target.Radius;
            return Vec2.GetRangePower(center, target.Position) < (casterRadius + range + targetRadius) * (casterRadius + range + targetRadius);
        }

        public static bool CheckTargetInPan(FieldObject caster, FieldObject target, Vec2 center, Vec2 lookDir, float range, float pan, int withoutId, bool alive)
        {
            if (!CheckTargetInCircle(caster, target, center, range, withoutId, alive))
            {
                return false;
            }
            //return Vec2.InPan(lookDir, target.Position - caster.Position, pan);
            return Vec2.InSector(lookDir, target.Position - caster.Position, pan);
        }

        public static bool CheckTargetInSquare(FieldObject caster, FieldObject target, Vec2 center, Vec2 lookDir, float range, float width, int withoutId, bool alive, ref Polygon polygon)
        {
            if (!CheckTargetInCircle(caster, target, center, range, withoutId, alive))
            {
                return false;
            }
            //如果polygon为null，先计算一个
            if (polygon == null)
            {
                Vec2 leftTop, rightTop, rightBottom, leftBottom;
                Vec2 look = lookDir.Clone();
                look.Normalize();
                Vec2 rectCenter = center + look * range/2;
                leftBottom = center + (look * width/2).GetCounterclockwiseOrthogonalVec2s();
                rightBottom = center * 2 - leftBottom;
                leftTop = rectCenter * 2 - rightBottom;
                rightTop = rectCenter * 2 - leftBottom;
                polygon = new Rect(leftTop, rightTop, rightBottom, leftBottom);
            }

            float casterRadius = caster == null ? 0f : caster.Radius;
            float targetRadius = target == null ? 0f : target.Radius;
            Vec2 tempVec = (target.Position - caster.Position);
            Vec2 tempVecClone = tempVec.Clone();
            tempVec.Normalize();
            Vec2 minusDis = tempVec * (casterRadius + targetRadius);
            if (tempVecClone.magnitudePower > minusDis.magnitudePower)
            {
                return polygon.InPolygon(target.Position - minusDis);
            }
            else
            {
                return polygon.InPolygon(target.Position);
            }

            //double radian = Vec2.GetRadian(Vec2.up, lookDir);
            //double sin = Math.Sin(radian);
            //double cos = Math.Cos(radian);
            //Vec2 local = new Vec2();
            //Vec2.OperatorMinus(target.Position, center, local);
            //local.Rotate(sin, cos);
            //return local.IsIntersectSqure(width * 0.5f, range, target.Radius);
        }


        public static bool CheckTargetInSplash(FieldObject caster, FieldObject target, SplashType splashType, Vec2 center, Vec2 lookDir, float range,
            float pan, float width, int withoutId, bool alive, ref Polygon polygon)
        {
            switch (splashType)
            {
                case SplashType.Map:
                    return CheckTargetValid(target, withoutId, alive);
                case SplashType.Circle:
                    return CheckTargetInCircle(caster, target, center, range, withoutId, alive);
                case SplashType.Pan:
                    return CheckTargetInPan(caster, target, center, lookDir, range, pan, withoutId, alive);
                case SplashType.Square:
                    return CheckTargetInSquare(caster, target, center, lookDir, range, width, withoutId, alive, ref polygon);
                default:
                    break;
            }
            return false;
        }

        public static void GetAllyInSplash(FieldObject caster, IFieldObjectContainer container, SplashType splashType, Vec2 center, Vec2 lookDir,
            float range, float width, float pan, List<FieldObject> targetList, int maxCount, int withoutId, bool alive, ref Polygon polygon)
        {
            FieldObject target = null;
            //Polygon polygon = null;
            switch (caster.FieldObjectType)
            {
                case TYPE.MONSTER:
                    IReadOnlyDictionary<int, Monster> monsterList = container.GetMonsters();
                    foreach (var monster in monsterList)
                    {
                        target = monster.Value;

                        if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                        {
                            targetList.Add(target);
                            if (targetList.Count >= maxCount)
                            {
                                return;
                            }
                        }
                    }
                    break;
                case TYPE.PC:
                case TYPE.PET:
                case TYPE.HERO:
                case TYPE.ROBOT:
                    //自动战斗不考虑playerchar，已经是robot
                    if (!caster.CurrentMap.Model.IsAutoBattle)
                    {
                        //IReadOnlyDictionary<int, PlayerChar> playerList = container.GetPlayers();
                        //foreach (var player in playerList)
                        //{
                        //    target = player.Value;
                        //    if (!caster.IsAlly(target))
                        //    {
                        //        continue;
                        //    }
                        //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                        //    {
                        //        targetList.Add(target);
                        //        if (targetList.Count >= maxCount)
                        //        {
                        //            return;
                        //        }
                        //    }
                        //}
                    }
                    //IReadOnlyDictionary<int, Robot> robotList = container.GetRobots();
                    //foreach (var robot in robotList)
                    //{
                    //    target = robot.Value;
                    //    if (!caster.IsAlly(target))
                    //    {
                    //        continue;
                    //    }
                    //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                    //    {
                    //        targetList.Add(target);
                    //        if (targetList.Count >= maxCount)
                    //        {
                    //            return;
                    //        }
                    //    }
                    //}
                    IReadOnlyDictionary<int, Hero> heroList = container.GetHeros();
                    foreach (var hero in heroList)
                    {
                        target = hero.Value;
                        if (!caster.IsAlly(target) || target.IsDead)
                        {
                            continue;
                        }
                        if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                        {
                            targetList.Add(target);
                            if (targetList.Count >= maxCount)
                            {
                                return;
                            }
                        }
                    }
                    //IReadOnlyDictionary<int, Pet> petList = container.GetPets();
                    //foreach (var pet in petList)
                    //{
                    //    target = pet.Value;
                    //    if (!caster.IsAlly(target))
                    //    {
                    //        continue;
                    //    }
                    //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                    //    {
                    //        targetList.Add(target);
                    //        if (targetList.Count >= maxCount)
                    //        {
                    //            return;
                    //        }
                    //    }
                    //}
                    break;
            }
        }

        public static void GetEnemyInSplash(FieldObject caster, IFieldObjectContainer container, SplashType splashType, Vec2 center, Vec2 lookDir,
            float range, float width, float pan, List<FieldObject> targetList, int maxCount, int withoutId, bool alive, ref Polygon polygon)
        {
            FieldObject target = null;
            //Polygon polygon = null;
            switch (caster.FieldObjectType)
            {
                case TYPE.MONSTER:
                    // Monster 与player及hero均为enemy 不需要检查是否为enemy
                    //IReadOnlyDictionary<int, PlayerChar> playerList = container.GetPlayers();
                    //foreach (var player in playerList)
                    //{
                    //    target = player.Value;
                    //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                    //    {
                    //        targetList.Add(target);
                    //        if (targetList.Count >= maxCount)
                    //        {
                    //            return;
                    //        }
                    //    }
                    //}
                    IReadOnlyDictionary<int, Hero> heroList = container.GetHeros();
                    foreach (var hero in heroList)
                    {
                        target = hero.Value;
                        if (target.IsDead) continue;

                        if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                        {
                            targetList.Add(target);
                            if (targetList.Count >= maxCount)
                            {
                                return;
                            }
                        }
                    }
                    //IReadOnlyDictionary<int, Pet> petList = container.GetPets();
                    //foreach (var pet in petList)
                    //{
                    //    target = pet.Value;
                    //    if (target.IsDead) continue;

                    //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                    //    {
                    //        targetList.Add(target);
                    //        if (targetList.Count >= maxCount)
                    //        {
                    //            return;
                    //        }
                    //    }
                    //}
                    //IReadOnlyDictionary<int, Robot> robotList = container.GetRobots();
                    //foreach (var robot in robotList)
                    //{
                    //    target = robot.Value;
                    //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                    //    {
                    //        targetList.Add(target);
                    //        if (targetList.Count >= maxCount)
                    //        {
                    //            return;
                    //        }
                    //    }
                    //}
                    break;
                case TYPE.PC:
                case TYPE.PET:
                case TYPE.HERO:
                case TYPE.ROBOT:
                    if (caster.CurrentMap.PVPType != PvpType.None)
                    {
                        //自动战斗不考虑playerchar
                        if (!caster.CurrentMap.Model.IsAutoBattle)
                        {
                            //playerList = container.GetPlayers();
                            //foreach (var player in playerList)
                            //{
                            //    target = player.Value;
                            //    if (target == null || !caster.IsEnemy(target))
                            //    {
                            //        continue;
                            //    }
                            //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                            //    {
                            //        targetList.Add(target);
                            //        if (targetList.Count >= maxCount)
                            //        {
                            //            return;
                            //        }
                            //    }
                            //}
                        }
                        //robotList = container.GetRobots();
                        //foreach (var robot in robotList)
                        //{
                        //    target = robot.Value;
                        //    if (target == null || !caster.IsEnemy(target))
                        //    {
                        //        continue;
                        //    }
                        //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                        //    {
                        //        targetList.Add(target);
                        //        if (targetList.Count >= maxCount)
                        //        {
                        //            return;
                        //        }
                        //    }
                        //}
                        //IReadOnlyDictionary<int, Pet> petList = container.GetPets();
                        //foreach (var pet in petList)
                        //{
                        //    target = pet.Value;
                        //    if (target == null || !caster.IsEnemy(target) || target.IsDead)
                        //    {
                        //        continue;
                        //    }
                        //    if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                        //    {
                        //        targetList.Add(target);
                        //        if (targetList.Count >= maxCount)
                        //        {
                        //            return;
                        //        }
                        //    }
                        //}
                        heroList = container.GetHeros();
                        foreach (var hero in heroList)
                        {
                            target = hero.Value;
                            if (target == null || !caster.IsEnemy(target))
                            {
                                continue;
                            }
                            if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                            {
                                targetList.Add(target);
                                if (targetList.Count >= maxCount)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    IReadOnlyDictionary<int, Monster> monsterList = container.GetMonsters();
                    foreach (var monster in monsterList)
                    {
                        // hero player与monster为enemy 无需检查IsEnemy
                        
                        target = monster.Value;
                        if (monster.Value.Borning)
                        {
                            continue;
                        }
                        if (CheckTargetInSplash(caster, target, splashType, center, lookDir, range, pan, width, withoutId, alive, ref polygon))
                        {
                            targetList.Add(target);
                            if (targetList.Count >= maxCount)
                            {
                                return;
                            }
                        }
                    }
                    break;
            }
        }


        public static void GetAllyInMap(FieldObject caster, IFieldObjectContainer map, List<FieldObject> targetList)
        {
            if (map == null) return;

            FieldObject target = null;
            switch (caster.FieldObjectType)
            {
                case TYPE.MONSTER:
                    IReadOnlyDictionary<int, Monster> monsterList = map.GetMonsters();
                    foreach (var hero in monsterList)
                    {
                        target = hero.Value;
                        if (!caster.IsAlly(target) || target.IsDead)
                        {
                            continue;
                        }
                        targetList.Add(target);
                    }
                    break;
                case TYPE.PC:
                case TYPE.PET:
                case TYPE.HERO:
                case TYPE.ROBOT:
                    IReadOnlyDictionary<int, Hero> heroList = map.GetHeros();
                    foreach (var hero in heroList)
                    {
                        target = hero.Value;
                        if (!caster.IsAlly(target) || target.IsDead)
                        {
                            continue;
                        }
                        targetList.Add(target);
                    }
                    //IReadOnlyDictionary<int, Pet> petList = map.GetPets();
                    //foreach (var pet in petList)
                    //{
                    //    target = pet.Value;
                    //    if (!caster.IsAlly(target))
                    //    {
                    //        continue;
                    //    }
                    //    targetList.Add(target);
                    //}
                    break;
            }
        }

        public static void GetEnemyInMap(FieldObject caster, IFieldObjectContainer map, List<FieldObject> targetList)
        {
            if (map == null) return;

            FieldObject target = null;
            switch (caster.FieldObjectType)
            {
                case TYPE.MONSTER:
                    {
                        IReadOnlyDictionary<int, Monster> monsterList = map.GetMonsters();
                        foreach (var hero in monsterList)
                        {
                            target = hero.Value;
                            if (!caster.IsEnemy(target) || target.IsDead)
                            {
                                continue;
                            }
                            targetList.Add(target);
                        }
                    }
                    break;
                case TYPE.PC:
                case TYPE.PET:
                case TYPE.HERO:
                case TYPE.ROBOT:
                    {
                        if (caster.CurrentMap.PVPType != PvpType.None)
                        {
                            //IReadOnlyDictionary<int, Pet> petList = map.GetPets();
                            //foreach (var pet in petList)
                            //{
                            //    target = pet.Value;
                            //    if (target == null || !caster.IsEnemy(target) || target.IsDead)
                            //    {
                            //        continue;
                            //    }
                            //    targetList.Add(target);
                            //}
                            IReadOnlyDictionary<int, Hero> heroList = map.GetHeros();
                            foreach (var hero in heroList)
                            {
                                target = hero.Value;
                                if (target == null || !caster.IsEnemy(target))
                                {
                                    continue;
                                }
                                targetList.Add(target);
                            }
                        }
                        IReadOnlyDictionary<int, Monster> monsterList = map.GetMonsters();
                        foreach (var monster in monsterList)
                        {
                            // hero player与monster为enemy 无需检查IsEnemy

                            target = monster.Value;
                            if (monster.Value.Borning)
                            {
                                continue;
                            }
                            targetList.Add(target);
                        }
                    }
                    break;
            }
        }
    }
}
