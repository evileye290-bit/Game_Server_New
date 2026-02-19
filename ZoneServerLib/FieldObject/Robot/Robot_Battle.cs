using CommonUtility;
using EnumerateUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class Robot
    {
        public override float DeadDelay { get { return HeroModel.DeadDelay; } }

        public override void StartFighting()
        {
            if (CurDungeon == null) return;
            //if (CurDungeon.CheckPlayerStartedFighting(uid))
            //{
            //    return;
            //}
            CurDungeon.RecordPlayerStartedFighting(uid);

            base.StartFighting();
            InitFSM();
            //InitBattleState();

            RobotHerosStartFighting();
            InitHolaEffect();

            //检测存货单位
            DispatchAliveCountMessage();
        }

        private Dictionary<string, SkillDelayInfo> skillsToRelease = new Dictionary<string, SkillDelayInfo>();
        private int skillsToReleaseCount = 0;
        private float skillDelayTime = 0.2f;

        private void InitBattleState()
        {
            //InitAI 初始化ai相关 直接
            skillManager = new SkillManager(this);
            buffManager = new BuffManager(this);
            skillEngine = new SkillEngine(this);
            markManager = new MarkManager(this);
            InitTrigger();
            BindTriggers();
            BindSkills();
            if (!CopyedFromPlayer)
            {
                BindSoulRingSkills();
            }
            else
            {
                CopySoulRingSkill(playerMirror);
            }

            //仇恨manager
            hateManager = new HeroHateManager(this, HeroModel.HateRange, HeroModel.HateRefreshTime); //由于不会被溅射仇恨，所以可能不知道该打谁，从而不战斗

            PassiveSkillEffect();
        }

        public override void BindSkills()
        {
            if (heroInfo == null)
            {
                return;
            }

            int skillLevel = heroInfo.SoulSkillLevel / 10 + 1;

            // 默认自带技能，如普攻 武魂真身等
            HeroModel heroModel = HeroLibrary.GetHeroModel(HeroId);
            if (heroModel == null)
            {
                return;
            }

            foreach (var skillId in heroModel.Skills)
            {
                SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
                if (skillModel == null)
                {
                    continue;
                }

                // 魂环技单独处理
                if (skillModel.SoulRingPos > 0)
                {
                    continue;
                }

                // 非魂环技能 技能等级为1
                SkillManager.Add(skillId, skillLevel);
            }
        }

        public override void BindSoulRingSkills()
        {
            if (heroInfo == null)
            {
                return;
            }

            int skillLevel = heroInfo.SoulSkillLevel / 10 + 1;

            // 默认自带技能，如普攻 武魂真身等
            HeroModel heroModel = HeroLibrary.GetHeroModel(HeroId);
            if (heroModel == null)
            {
                return;
            }
            List<BattleSoulRingInfo> soulRingSpecList = new List<BattleSoulRingInfo>();
            foreach (var skillId in heroModel.Skills)
            {
                SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
                if (skillModel == null)
                {
                    continue;
                }

                SkillManager.Add(skillId, skillLevel);

                // 魂环技， 通过魂环等级确定技能等级
                if (HeroSoulRings.ContainsKey(HeroId) && HeroSoulRings[HeroId].ContainsKey(skillModel.SoulRingPos))
                {
                    BattleSoulRingInfo soulRingInfo = HeroSoulRings[HeroId][skillModel.SoulRingPos];
                    soulRingSpecList.Add(soulRingInfo);
                }
            }
            SoulRingSpecUtil.DoEffect(soulRingSpecList, this);

            PrintNatures(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>", "<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", 0);
        }

        public override void BindSoulBoneSkills()
        {
            List<BattleSoulBoneInfo> soulBoneSpecList = new List<BattleSoulBoneInfo>();
            if (HeroSoulBones.TryGetValue(HeroId, out soulBoneSpecList))
            {
                List<SoulBoneSpecModel> specs = new List<SoulBoneSpecModel>();
                foreach (var boneInfo in soulBoneSpecList)
                {
                    List<SoulBoneSpecModel> soulBoneSpecs = SoulBoneLibrary.GetSpecModel4ItemId(boneInfo.Id, boneInfo.SpecList);
                    if (soulBoneSpecs != null && soulBoneSpecs.Count > 0)
                    {
                        specs.AddRange(soulBoneSpecs);
                    }
                }
                SoulBoneSpecUtil.DoEffect(specs, this);
            }
        }

        // todo pvp相关待修正
        //public override bool IsEnemy(FieldObject target)
        //{
        //switch (target.FieldObjectType)
        //{
        //    case TYPE.MONSTER:
        //        return true;
        //    case TYPE.ROBOT:
        //        if (currentMap.PVPType == PvpType.Person && target != this)
        //        {
        //            return true;
        //        }
        //        return false;
        //    case TYPE.PC:
        //        if (CurrentMap.Model.IsAutoBattle)
        //        {
        //            //自动战斗 pc 只是作为消息转发不参与战斗
        //            return false;
        //        }
        //        else
        //        {
        //            if (currentMap.PVPType == PvpType.Person && target != this)
        //            {
        //                return true;
        //            }
        //        }
        //        return false;
        //    case TYPE.HERO:
        //        Hero hero = (Hero)target;
        //        if (currentMap.PVPType == PvpType.Person && hero.Owner != this)
        //        {
        //            return true;
        //        }
        //        return false;

        //    default:
        //        return false;
        //}
        //}

        //// todo pvp相关待修正
        //public override bool IsAlly(FieldObject target)
        //{
        //    return IsAttacker == target.IsAttacker;
        //    //switch (target.FieldObjectType)
        //    //{
        //    //    case TYPE.MONSTER:
        //    //        return false;
        //    //    case TYPE.ROBOT:
        //    //        if (target == this)
        //    //            return true;
        //    //        return false;
        //    //    case TYPE.PC:
        //    //        if (CurrentMap.Model.IsAutoBattle)
        //    //        {
        //    //            return false;
        //    //        }
        //    //        else
        //    //        {
        //    //            if (currentMap.PVPType == PvpType.Person && target != this)
        //    //            {
        //    //                return false;
        //    //            }
        //    //        }
        //    //        return true;
        //    //    case TYPE.HERO:
        //    //        Hero hero = (Hero)target;
        //    //        if (currentMap.PVPType == PvpType.Person && hero.Owner != this)
        //    //        {
        //    //            return false;
        //    //        }
        //    //        return true;
        //    //    default:
        //    //        return true;
        //    //}

        //}

        public void CastSkill(Skill skill)
        {
            if (skill.Energy <= skill.GetEnergyLimit())
            {
                ReleaseSkill(InstanceId, skill.Id);
            }
        }

        public void ReleaseSkill(int instanceId, int skillId)
        {
            string key = instanceId + "_" + skillId + skillsToReleaseCount++;
            skillsToRelease.Add(key, new SkillDelayInfo()
            {
                Key = key,
                Delay = 0f,
                InstanceId = instanceId,
                SkillId = skillId
            });
        }

        public void UpdateSkillRelease(float deltaTime)
        {
            List<string> delaySkillRemove = new List<string>();
            foreach (var kv in skillsToRelease)
            {
                if (kv.Value.Delay >= skillDelayTime)
                {
                    RealRelease(kv.Value.InstanceId, kv.Value.SkillId);
                    delaySkillRemove.Add(kv.Value.Key);
                }
                else
                {
                    kv.Value.Delay += deltaTime;
                }
            }
            foreach (var item in delaySkillRemove)
            {
                skillsToRelease.Remove(item);
            }
        }

        private void RealRelease(int instanceId, int skillId)
        {
            if (instanceId == InstanceId)
            {
                RealReleaseSkill(skillId);
            }
            else
            {
                (currentMap as DungeonMap)?
                    .HeroList.Where(kv => kv.Key == instanceId)
                    .ForEach(kv => kv.Value.RealReleaseSkill(skillId));
            }
        }

        public override void OnDead()
        {
            base.OnDead();
        }

        //开始复活操作
        public override void Revive()
        {
            if (!(currentMap as DungeonMap)?.CanRevive ?? false)
            {
                return;
            }
            IsReviving = true;
            FsmManager.SetNextFsmStateType(FsmStateType.IDLE);
        }

        //复活后事件
        public override void OnRevived()
        {
            base.OnRevived();
            InitBattleState();
        }

        public override long OnHit(FieldObject caster, DamageType damageType, long damage, ref bool immune, float multipleParam = 1)
        {
            damage = base.OnHit(caster, damageType, damage, ref immune, multipleParam);
            if (damage > 0 && caster != null && IsEnemy(caster))
            {
                //int hateValue = (int)(damage * caster.HateRatio + 0.5f);
                //int teamMateHate = hateValue / 3;
                ////调用队友的hatemanager
                //if (!(CurrentMap is ArenaDungeonMap))
                //{
                //    foreach (var hero in CurrentMap.HeroList)
                //    {
                //        if (hero.Value.Owner == this)
                //        {
                //            hero.Value.HateManager?.AddHate(caster, teamMateHate);
                //        }
                //    }
                //}

                //HateManager?.AddHate(caster, hateValue);

                SkillManager.AddOnHitBodyEnergy(damage);
            }
            return damage;
        }

        public override void StopFighting()
        {
            base.StopFighting();
            ClearBasicBattleState();
        }

        public bool InBattleField()
        {
            if (InBattle)
            {
                return true;
            }
            return false;
        }


        //注意搜索目标的时候要对所有对hero带判断的地方添加robot类型的判断
        public bool FindTarget()
        {
            List<FieldObject> tempTargetList = new List<FieldObject>();
            FieldObject target = null;
            Vec2 targetPos = null;

            Skill skill;
            if (SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
            {
                // 有准备好的技能，且该技能满足释放条件
                return true;
            }
            return false;
        }

        public void ClearAllBattleState()
        {
            // 机器人不需要清理回图相关

            // 属性重置
            InBattle = false;
            ClearBasicBattleState();

            messageDispatcher = null;
            triggerManager = null;

            FsmManager.SetNextFsmStateType(FsmStateType.IDLE);

        }

        //public bool CheckCollisionWithPriority(FieldObject target, Vec2 pos)
        //{
        //    foreach (var kv in currentMap.HeroList)
        //    {
        //        Hero hero = kv.Value;
        //        //根据优先级可被计算的目标
        //        if (hero.HeroModel.CollisionPriority > HeroModel.CollisionPriority
        //            || hero.HeroModel.CollisionPriority == HeroModel.CollisionPriority && hero.InstanceId < InstanceId)
        //        {
        //            //计算是否碰撞
        //            if (Vec2.GetDistance(pos, hero.Position) < HeroModel.Radius + hero.HeroModel.Radius)
        //            {
        //                Logger.Log.Debug($"{hero.HeroId} radius {hero.HeroModel.Radius} collision with {HeroId} radius {HeroModel.Radius}");
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        public bool CheckCollision(FieldObject target, Vec2 pos)
        {
            foreach (var kv in currentMap.HeroList)
            {
                Hero hero = kv.Value;
                //计算是否碰撞
                if (Vec2.GetDistance(pos, hero.Position) < HeroModel.Radius + hero.Radius)
                {
                    Logger.Log.Debug($"{hero.HeroId} radius {hero.Radius} collision with {HeroId} radius {HeroModel.Radius}");
                    return true;
                }
            }
            return false;
        }

        public Tuple<bool, Vec2> GetNonCollisionPos(FieldObject target, Vec2 pos, float skillDis, int maxCount = 4, float deltaLength = 0.1f)
        {
            float dis = target.Radius + HeroModel.Radius + skillDis;
            int count = maxCount;
            int allCount = 0;
            float temp = target.Radius + HeroModel.Radius + skillDis - deltaLength;
            for (int i = 0; i < 50; i++)
            {
                allCount++;
                Vec2 tempPos = GetRandomVec2FromTo(temp, dis);
                if (!CheckCollision(target, tempPos + target.Position))
                {
                    Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount");
                    return Tuple.Create(true, tempPos + target.Position);
                }
                else
                {
                    Logger.Log.Debug($"hero {InstanceId} randomPos {tempPos} collision for monster {target.InstanceId} with {allCount} allcount");
                }
            }
            Logger.Log.Debug($"hero {InstanceId} get non collision pos for monster {target.InstanceId} with {allCount} allcount in default");
            return Tuple.Create(false, pos);
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
    }
}
