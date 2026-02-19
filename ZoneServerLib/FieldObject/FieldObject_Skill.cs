using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ScriptFighting;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    partial class FieldObject
    {
        protected SkillManager skillManager;
        public SkillManager SkillManager { get { return skillManager; } }

        private List<FieldObject> targetList = new List<FieldObject>();
        private List<FieldObject> doDamageList = new List<FieldObject>();

        public virtual void BindSkills() { }

        public virtual void BindSoulRingSkills() { }

        public virtual void BindSoulBoneSkills() { }

        public void InitSkillManager()
        {
            skillManager = new SkillManager(this);
            BindSkills();
        }

        // 是否可以普攻
        public bool CanCastNormalAttack()
        {
            // 眩晕或缴械状态下 无法普通攻击
            if (InBuffState(BuffType.Dizzy) || InBuffState(BuffType.Disarm))
            {
                return false;
            }
            return true;
        }

        // 是否可以释放主动技能
        public bool CanCastManualSkill()
        {
            // 眩晕或定身状态下 无法普通攻击
            if (InBuffState(BuffType.Dizzy) || InBuffState(BuffType.Silent))
            {
                return false;
            }
            return true;
        }

        public bool CanCastSkill(SkillModel model)
        {
            // 眩晕或定身状态下 无法普通攻击
            if (InBuffState(BuffType.Dizzy))
            {
                return false;
            }

            //是否能够释放领域技能
            if (model.IsDomainSkill && InBuffState(BuffType.RefuseCastSkillWithDomain))
            {
                return false;
            }

            return true;
        }

        public bool CastSkill(int skillId, Vec2 lookDir, Vec2 destPos, int targetId)
        {
            //自动战斗不允许前端释放技能
            if (CurrentMap.Model.IsAutoBattle)return false;

            Skill skill = skillManager.GetSkill(skillId);
            if (skill == null)
            {
                return false;
            }
            if (!skillManager.Check(skill))
            {
                return false;
            }
            if (!CheckSkillParam(skill.SkillModel, destPos, targetId))
            {
                return false;
            }

            skillManager.CheckBreakNormalSkill(skill);

            // 检查通过，进入skill状态机
            skill.InitCastParam(lookDir, destPos, targetId);
            fsmManager.SetNextFsmStateType(FsmStateType.SKILL, true, skill);
            return true;
        }

        public bool CheckSkillParam(SkillModel skillModel, Vec2 destPos, int targetId)
        {
            if (skillModel == null)
            {
                return false;
            }
            FieldObject target = null;

            switch (skillModel.PositionType)
            {
                case SkillPositionType.Self_NoDirection:
                case SkillPositionType.Self_Direction:
                    return true;

                case SkillPositionType.Any:
                    if (destPos == null)
                    {
                        return false;
                    }
                    return (skillModel.CastRange + 1) * (skillModel.CastRange + 1) >= Vec2.GetRangePower(Position, destPos);

                case SkillPositionType.Target:
                    target = currentMap.GetFieldObject(targetId);
                    if (target == null)
                    {
                        // 允许空放
                        return true;
                    }
                    return (skillModel.CastRange + 1f + target.radius + Radius) * (skillModel.CastRange + 1f + target.radius + Radius) >=
                        Vec2.GetRangePower(Position, target.Position);
                case SkillPositionType.Self_NeedEnemy:
                    return CurrMapHaveEnemy();
                default:
                    return false;
            }
        }

        public bool CurrMapHaveEnemy()
        {
            if (!(currentMap is DungeonMap))return false;

            //if (currentMap.GetPlayers().Select(x => IsEnemy(x.Value) && !x.Value.IsDead).FirstOrDefault()) return true;
            if (currentMap.HeroList.Select(x => IsEnemy(x.Value) && !x.Value.IsDead).FirstOrDefault())return true;
            if (currentMap.MonsterList.Select(x => IsEnemy(x.Value) && !x.Value.IsDead && !x.Value.Borning).FirstOrDefault())return true;
            //if (currentMap.RobotList.Select(x => IsEnemy(x.Value) && !x.Value.IsDead).FirstOrDefault()) return true;
            //if (currentMap.PetList.Select(x => IsEnemy(x.Value) && !x.Value.IsDead).FirstOrDefault()) return true;

            return false;
        }

        public int NextCriticalHitCount { get; internal set; }
        public int FleeNextDamageCount { get; private set; }

        public void AddFleeCount(int count)
        {
            FleeNextDamageCount += count;
            if (FleeNextDamageCount > TeamLibrary.FleeCountLimit)
            {
                FleeNextDamageCount = TeamLibrary.FleeCountLimit;
            }
        }

        private bool CheckDodge()
        {
            if (FleeNextDamageCount > 0)
            {
                FleeNextDamageCount--;
                return true;
            }
            return false;
        }


        //20210407 调整：一个技能的某个技能效果命中，后续技能效果需要命中；反之一个技能的某个效果闪避，该技能的其他效果也需要闪避
        //
        public void SkillEffect(Skill skill, int effectedCount = 0, bool afterCasting = true, bool isFirstEffect = true)
        {
            if (!InBattle || skill == null)return;
            
            bool allMissed = true;
            int skillId = skill.Id;
            int targetId = skill.CastTargetId;
            Vec2 lookDir = skill.CastLookDir;
            Vec2 destPos = skill.CastDestPos;
            DamageType damageType = skill.GetDamageType();
            
            HashSet<int> hittedList = new HashSet<int>();//该技能命中了的target
            HashSet<int> dodgedList = new HashSet<int>();//该技能闪避了的target
            
            DispathCastSkillMsg();
            if (skill.IsBodyAttack() && effectedCount == 0)
            {
                DispathCastLastBodyAttackMsg();
            }

            foreach (var skillEffect in skill.SkillEffectList)
            {
                targetList.Clear();
                doDamageList.Clear();

                // 检查是否加强技能效果
                bool enbaleEnhance = CheckEnbaleSkillEffectEnhance(skillEffect);
                skillEffect.EnbaleEnhance(enbaleEnhance);

                float targetRadius;
                SkillEffectModel model = skillEffect.GetModel();
                Vec2 skillPos = FindSkillPos(destPos, targetId, model, out targetRadius);
                if (skillPos == null)
                {
                    continue;
                }

                //可能有技能替换
                Logger.Log.Debug($"skill {skill.Id} effect {model.Id} for caster {InstanceId}");

                DefaultSkillEffectHandler handler = SkillEffectHandlerFactory.GetSkillEffectHandler(model.Name);
                if (handler == null)
                {
                    continue;
                }

                MSG_ZGC_SKILL_EFF skillMsg = new MSG_ZGC_SKILL_EFF();
                skillMsg.CasterId = instanceId;
                skillMsg.SkillId = skillId;
                skillMsg.TargetId = targetId;
                skillMsg.AngleX = lookDir.x;
                skillMsg.AngleY = lookDir.y;
                skillMsg.SkillPosX = skillPos.x;
                skillMsg.SkillPosY = skillPos.y;

                //寻找目标
                FindSkillTargets(targetList, lookDir, skillPos, skill.Level, targetId, model);
                FilterTargets(targetList, model.TargetFilterType);

                //增加技能使用次数
                skillManager.AddSkillUsedCount(skill.Id);

                float multipleParam = 1;
                if (model.MultipleT.Count > effectedCount)
                {
                    multipleParam = model.MultipleT[effectedCount];
                }

                long skillDamage = 0;
                int targetCount = targetList.Count;
                float skillGrowth = SkillLibrary.GetSkillGrowth(skill.Level);
                float skillEnhanceRatio = skillManager.GetSkillEnhancedDamageRatio(skill.Id) * 0.0001f;
                if (this as Pet != null)
                {
                    skillGrowth = PetLibrary.GetPetInbornSkillGrowth(skill.Level);
                }

                //伤害性技能触发增伤判断
                if (model.DoDamage && targetList.Count > 0)
                {
                    //技能范围内目标
                    DispatchTargetsInSkillRange(targetList, skill.Id);

                    //范围内目标被攻击
                    DispatchTargetBeenAttacked(targetList, this);
                }

#if DEBUG
                if (skill.SkillModel.Id == 2114)
                {
                    Logger.Log.Warn($"skill {skill.SkillModel.Id} effect {skillEffect.BasicModel.Id} rang target count {targetList.Count}");
                }
#endif

                foreach (var target in targetList)
                {
                    SkillDamage damage = null;

                    //检查是否闪避，闪避过一次，则后续技能效果都需要闪避
                    bool dodge = dodgedList.Contains(target.instanceId);

                    if (model.DoDamage)
                    {
                        //需要先调用CheckDodge
                        dodge = target.CheckDodge() || dodge;

                        if (dodge == false)
                        {
                            //技能效果必中，或者该技能的其他技能效果已经命中过该目标，则本次技能效果必中
                            bool isMustHit = skillManager.IsMustHitSkillEffect(model.Id) || model.MustHit || hittedList.Contains(target.instanceId);

                            //触发伤害前信息
                            DispatchHeroDoDamageBeforeStartFightMsg(skill.Id, target);

                            if (NextCriticalHitCount > 0)
                            {
                                damage = handler.CalcDamages(this, target, skillEffect, skill.UsedCount, damageType == DamageType.Normal, true, skillGrowth, isMustHit);
                                NextCriticalHitCount--;
                            }
                            else
                            {
                                damage = handler.CalcDamages(this, target, skillEffect, skill.UsedCount, damageType == DamageType.Normal, false, skillGrowth, isMustHit);
                            }

                            //技能伤害加成百分比
                            damage.Damage = (int)(damage.Damage * (1 + skillEnhanceRatio));
                        }
                        else
                        {
                            damage = new SkillDamage();
                        }

                        //是否免疫
                        bool immune = false;
                        long realDamage = 0;
                        long extraDamage = 0;
                        
                        //本次是否闪避
                        dodge = dodge || damage.Dodge;
                        
                        if (dodge)
                        {
                            damage.Reset();
                            damage.Dodge = true;
                            
                            //闪避了的目标
                            dodgedList.Add(target.instanceId);
                            
                            target.DispatchMessage(TriggerMessageType.DodgeSkill, skill);
                        }
                        else
                        {
                            if (damage.Critical)
                            {
                                DispatchMessage(TriggerMessageType.Critical, new CriticalTriMsg(skill.SkillModel, target, damage.Damage));
                                target.DispatchMessage(TriggerMessageType.GetCriticalStrike, null);
                            }
                            
                            // 记录Damage信息
                            realDamage = target.OnHit(this, damageType, damage.Damage, ref immune, multipleParam);
                            extraDamage = target.DoExtraDamage(this);

                            if (realDamage > 0) //添加能量
                            {
                                doDamageList.Add(target);
                                SkillManager.AddHitBodyEnergy(realDamage, targetCount);
                                DispatchAnySkilDoDamageMsg(skill.Id, realDamage, target);
                            }
                            
                            Logger.Log.Debug($"skill {skill.Id} effect {model.Id} for caster {InstanceId} on target {target.instanceId} with damage {realDamage} extraDamage {extraDamage}");
                        }

                        GenerateDamageMsg(target.instanceId, damageType, realDamage, damage.Critical, dodge, immune, extraDamage, skillMsg);

                        skillDamage = Math.Max(skillDamage, realDamage);
                    }

                    // 只有在需要伤害结算且未全都闪避 才算miss
                    if (dodge == false)
                    {
                        allMissed = false;
                        
                        //命中了目标
                        hittedList.Add(target.instanceId);

                        //多段伤害第一次生效，触发trigger
                        if (isFirstEffect)
                        {
                            //技能闪避后续效果不触发
                            Logger.Log.Debug($"skill {skill.Id} effect {model.Id} for caster {InstanceId} on target {target.instanceId} with {model.AfterEventType}");

#if DEBUG
                            if (skill.SkillModel.Id == 2114)
                            {
                                Logger.Log.Warn($"skill {skill.SkillModel.Id} effect {skillEffect.BasicModel.Id} rang target {target.GetHeroId()} is missed {dodge} after event type {model.AfterEventType} param {model.AfterEventParam}");
                            }
#endif
                            if (this as Pet != null)
                            {
                                handler.After(this, target, skillEffect, PetLibrary.GetPetInbornSkillGrowth(skillEffect.BasicLevel), PetLibrary.GetPetInbornSkillGrowth(skillEffect.EnhanceLevel));
                            }
                            else
                            {
                                handler.After(this, target, skillEffect, SkillLibrary.GetSkillGrowth(skillEffect.BasicLevel), SkillLibrary.GetSkillGrowth(skillEffect.EnhanceLevel));
                            }
                        }
                    }

                    if (target.IsDead)
                    {
                        DispatchKillEnemyMsg(instanceId, skill.Id, target.InstanceId, damage?.Critical == true, damageType);
                    }
                }

                if (allMissed)
                {
                    DispatchMessage(TriggerMessageType.SkillMissed, skill.Id);
                }
                else
                {
                    //多段伤害第一次生效，触发trigger
                    if (isFirstEffect)
                    {
                        DispatchOneSkilDoDamageMsg(skillId, skillDamage);
                        DispatchSkillHitTargetMsg(skill.SkillModel, targetList);
                    }
                }

                if (doDamageList.Count > 0)
                {
                    DispatchAnySkilDoDamageTargetListMsg(skill, doDamageList);
                }

                // 同步skillMsg
                BroadCastTargetsArea(skillMsg, targetList);
            }

            if (afterCasting)
            {
                skillManager.AfterCasting(skill, allMissed);
            }
        }

        public Vec2 FindSkillPos(Vec2 destPos, int targetId, SkillEffectModel model, out float radius)
        {
            radius = 0f;
            Vec2 skillPos = null;
            switch (model.StartType)
            {
                case StartType.This:
                    skillPos = Position;
                    break;
                case StartType.Target:
                    FieldObject target = currentMap.GetFieldObject(targetId);
                    if (target != null)
                    {
                        skillPos = target.Position;
                        radius = target.Radius;
                    }
                    break;
                case StartType.Position:
                    // todo 对于由skilleffect.xml确定的位置 则用该位置
                    if (!string.IsNullOrEmpty(model.FixedPos))
                    {
                        skillPos = new Vec2(model.FixedPos);
                    }
                    else
                    {
                        skillPos = destPos;
                    }
                    break;
                default:
                    break;
            }
            return skillPos;
        }

        public void FindSkillTargets(List<FieldObject> targetList, Vec2 lookDir, Vec2 skillPos, int skillLevel,
            int targetId, SkillEffectModel model)
        {
            if (model.SplashType == SplashType.Target)
            {
                // SlashType = Target, 只有StartType = Target才有意义
                if (model.StartType != StartType.Target)
                {
                    return;
                }
                FieldObject target = currentMap.GetFieldObject(targetId);
                if (target == null || target.IsDead)
                {
                    return;
                }
                if (model.TargetType == CommonUtility.TargetType.Enemy && IsEnemy(target))
                {
                    targetList.Add(target);
                }
                if (model.TargetType == CommonUtility.TargetType.Ally && IsAlly(target))
                {
                    targetList.Add(target);
                }
            }
            else
            {
                int withoutId = -1;
                if (!model.ContainTarget && model.StartType == StartType.Target)
                {
                    withoutId = targetId;
                }

                int targetMaxCount = SkillEffectParamCalculator.CalcTargetMaxCount(model.Name, skillLevel, model.TargetMaxCount);
                if (model.TargetType == CommonUtility.TargetType.Enemy)
                {
                    GetEnemyInSplash(this, model.SplashType, skillPos, lookDir, model.Range, model.Width, model.HalfPanTangent,
                        targetList, targetMaxCount, withoutId, model.Alive);
                }
                else if (model.TargetType == CommonUtility.TargetType.Ally)
                {
                    GetAllyInSplash(this, model.SplashType, skillPos, lookDir, model.Range, model.Width, model.HalfPanTangent,
                        targetList, targetMaxCount, withoutId, model.Alive);
                }
            }
        }

        public void GetAllyInSplash(FieldObject caster, SplashType splashType, Vec2 center, Vec2 lookDir, float range, float width, float pan,
            List<FieldObject> targetList, int maxCount, int withoutId = -1, bool alive = true)
        {
            Polygon polygon = null;
            if (splashType == SplashType.Map || currentMap.AoiType == AOIType.All)
            {
                SkillSplashChecker.GetAllyInSplash(caster, currentMap, splashType, center, lookDir, range, width, pan, targetList, maxCount, withoutId, alive, ref polygon);
            }
            else
            {
                SkillSplashChecker.GetAllyInSplash(caster, curRegion, splashType, center, lookDir, range, width, pan, targetList, maxCount, withoutId, alive, ref polygon);
                if (targetList.Count >= maxCount)
                {
                    return;
                }
                for (int i = 0; i < 8; i++)
                {
                    if (curRegion.NeighborList[i] != null)
                    {
                        SkillSplashChecker.GetAllyInSplash(caster, curRegion.NeighborList[i], splashType, center, lookDir, range, width, pan, targetList, maxCount, withoutId, alive, ref polygon);
                        if (targetList.Count >= maxCount)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void GetEnemyInSplash(FieldObject caster, SplashType splashType, Vec2 center, Vec2 lookDir, float range, float width, float pan,
            List<FieldObject> targetList, int maxCount, int withoutId = -1, bool alive = true)
        {
            Polygon polygon = null;
            if (splashType == SplashType.Map || currentMap.AoiType == AOIType.All)
            {
                SkillSplashChecker.GetEnemyInSplash(caster, currentMap, splashType, center, lookDir, range, width, pan, targetList, maxCount, withoutId, alive, ref polygon);
            }
            else
            {
                SkillSplashChecker.GetEnemyInSplash(caster, curRegion, splashType, center, lookDir, range, width, pan, targetList, maxCount, withoutId, alive, ref polygon);
                if (targetList.Count >= maxCount)
                {
                    return;
                }
                for (int i = 0; i < 8; i++)
                {
                    if (curRegion.NeighborList[i] != null)
                    {
                        SkillSplashChecker.GetEnemyInSplash(caster, curRegion.NeighborList[i], splashType, center, lookDir, range, width, pan, targetList, maxCount, withoutId, alive, ref polygon);
                        if (targetList.Count >= maxCount)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void FilterTargets(List<FieldObject> targetList, TargetFilterType filterType)
        {
            if (targetList == null || targetList.Count == 0)
            {
                return;
            }
            switch (filterType)
            {
                case TargetFilterType.MinHp:
                    FilterTarget_MinHp(targetList);
                    break;
                case TargetFilterType.RandomOne:
                    FilterTarget_RandomOne(targetList);
                    break;
                case TargetFilterType.NeareastEnemy:
                    FilterTarget_NeareastEnemy(targetList);
                    break;
                case TargetFilterType.MaxHateRatioEnemy:
                    FilterTarget_MaxHateRatioEnemy(targetList);
                    break;
                case TargetFilterType.MaxHateRatioNeareastEnemy:
                    FilterTarget_MaxHateRatioNeareastEnemy(targetList);
                    break;
                case TargetFilterType.MinHpRatio:
                    FilterTarget_MinHpRatio(targetList);
                    break;
                case TargetFilterType.MaxAtkAlly:
                    FilterTarget_MaxAtkAlly(targetList);
                    break;
                case TargetFilterType.MaxAtkEnemy:
                    FilterTarget_MaxAtkEnemy(targetList);
                    break;
                case TargetFilterType.MaxDefAlly:
                    FilterTarget_MaxDefAlly(targetList);
                    break;
                case TargetFilterType.MaxDefEnemy:
                    FilterTarget_MaxDefEnemy(targetList);
                    break;
                case TargetFilterType.MinMaxHp:
                    FilterTarget_MinMaxHp(targetList);
                    break;
                case TargetFilterType.FarthestEnemy:
                    FilterTarget_FarthestEnemy(targetList);
                    break;
                case TargetFilterType.InDebuff:
                    FilterTarget_InDebuff(targetList);
                    break;
                case TargetFilterType.Default:
                default:
                    break;
            }
            FilterTarget_Born(targetList);
        }

        public void FilterTarget_MaxHateRatioEnemy(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            float curMaxHateRatio = 0f;
            foreach (var item in list)
            {
                if (!item.IsEnemy(this))
                {
                    continue;
                }
                float tempHateRatio = 0f;
                if (target == null)
                {
                    target = item;
                    curMaxHateRatio = target.HateRatio;
                    continue;
                }

                tempHateRatio = item.HateRatio;
                if (tempHateRatio > curMaxHateRatio)
                {
                    target = item;
                    curMaxHateRatio = tempHateRatio;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_MaxHateRatioNeareastEnemy(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            float curMaxHateRatio = 0f;
            float curMinLength = 0f;
            foreach (var item in list)
            {
                if (!item.IsEnemy(this))
                {
                    continue;
                }
                float tempHateRatio = 0f;
                float tempLength = 0f;
                if (target == null)
                {
                    target = item;
                    curMaxHateRatio = target.HateRatio;
                    curMinLength = Vec2.GetDistance(this.Position, target.Position);
                    continue;
                }

                tempHateRatio = item.HateRatio;
                tempLength = Vec2.GetDistance(this.Position, item.Position);
                if (tempHateRatio > curMaxHateRatio || (tempHateRatio == curMaxHateRatio && tempLength < curMinLength))
                {
                    target = item;
                    curMaxHateRatio = tempHateRatio;
                    curMinLength = tempLength;
                }
            }

            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_NeareastEnemy(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            float curMinLength = 0f;
            foreach (var item in list)
            {
                if (!item.IsEnemy(this))
                {
                    continue;
                }
                float tempLength = float.MaxValue;
                if (target == null)
                {
                    target = item;
                    tempLength = Vec2.GetDistance(this.Position, target.Position);
                    curMinLength = tempLength;
                    continue;
                }

                tempLength = Vec2.GetDistance(this.Position, item.Position);
                if (tempLength < curMinLength)
                {
                    target = item;
                    curMinLength = tempLength;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_FarthestEnemy(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            float curMaxLength = 0f;
            foreach (var item in list)
            {
                if (!item.IsEnemy(this))
                {
                    continue;
                }
                float tempLength = float.MaxValue;
                if (target == null)
                {
                    target = item;
                    tempLength = Vec2.GetDistance(this.Position, target.Position);
                    curMaxLength = tempLength;
                    continue;
                }

                tempLength = Vec2.GetDistance(this.Position, item.Position);
                if (tempLength > curMaxLength)
                {
                    target = item;
                    curMaxLength = tempLength;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_MinHp(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            foreach (var item in list)
            {
                if (target == null)
                {
                    target = item;
                    continue;
                }
                if (target.GetHp() > item.GetHp())
                {
                    target = item;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_MinHpRatio(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            foreach (var item in list)
            {
                if (target == null)
                {
                    target = item;
                    continue;
                }
                if (target.GetHp() *1f / target.GetMaxHp() > item.GetHp()*1f / item.GetMaxHp())
                {
                    target = item;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_MinMaxHp(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            foreach (var item in list)
            {
                if (target == null)
                {
                    target = item;
                    continue;
                }
                if (item.GetMaxHp() < target.GetMaxHp())
                {
                    target = item;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_MaxDefEnemy(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            foreach (var item in list)
            {
                if (target == null && IsEnemy(item))
                {
                    target = item;
                    continue;
                }
                if (item.GetNatureValue(NatureType.PRO_DEF) > target.GetNatureValue(NatureType.PRO_DEF) && IsEnemy(item))
                {
                    target = item;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_MaxDefAlly(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            foreach (var item in list)
            {
                if (target == null && !IsEnemy(item))
                {
                    target = item;
                    continue;
                }
                if (item.GetNatureValue(NatureType.PRO_DEF) > target.GetNatureValue(NatureType.PRO_DEF) && !IsEnemy(item))
                {
                    target = item;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_MaxAtkAlly(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            foreach (var item in list)
            {
                if (target == null && !IsEnemy(item))
                {
                    target = item;
                    continue;
                }
                if (item.GetNatureValue(NatureType.PRO_ATK) > target.GetNatureValue(NatureType.PRO_ATK) && !IsEnemy(item))
                {
                    target = item;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_MaxAtkEnemy(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            FieldObject target = null;
            foreach (var item in list)
            {
                if (target == null && IsEnemy(item))
                {
                    target = item;
                    continue;
                }
                if (target.GetNatureValue(NatureType.PRO_ATK) < item.GetNatureValue(NatureType.PRO_ATK) && IsEnemy(item))
                {
                    target = item;
                }
            }
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_RandomOne(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            int index = RAND.Range(0, list.Count - 1);
            FieldObject target = list[index];
            list.Clear();
            list.Add(target);
        }

        public void FilterTarget_Born(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;
            List<FieldObject> newList = new List<FieldObject>();
            foreach (var item in list)
            {
                if (item == null)
                {
                    newList.Add(item);
                }
                else if (item.CurFsmStateType != FsmStateType.MONSTER_BORN)
                {
                    newList.Add(item);
                }
            }
            list.Clear();
            list.AddRange(newList);
        }

        public void FilterTarget_InDebuff(List<FieldObject> list)
        {
            if (list == null || list.Count == 0)return;

            List<FieldObject> newList = new List<FieldObject>();
            foreach (var item in list)
            {
                if (item?.buffManager.HaveDeBuff() == true)
                {
                    newList.Add(item);
                }
            }
            list.Clear();
            list.AddRange(newList);
        }

        private bool CheckEnbaleSkillEffectEnhance(SkillEffect skillEffect)
        {
            if (skillEffect.EnhanceModel != null && skillEffect.EnhancePolicy != null)
            {
                switch (skillEffect.EnhancePolicy.Condition)
                {
                    case SkillEffectEnhanceCondition.None:
                        return true;
                    case SkillEffectEnhanceCondition.HpRateGreater:
                        //return GetHp() >= (int)(GetMaxHp() * skillEffect.EnhancePolicy.EnhanceParam * 0.0001);
                        return GetNatureValue(NatureType.PRO_HP) >= (long)(GetNatureValue(NatureType.PRO_MAX_HP) * (skillEffect.EnhancePolicy.EnhanceParam * 0.0001f));
                    case SkillEffectEnhanceCondition.HpRateLess:
                        return GetNatureValue(NatureType.PRO_HP) < (long)(GetNatureValue(NatureType.PRO_MAX_HP) * (skillEffect.EnhancePolicy.EnhanceParam * 0.0001f));
                    default:
                        return false;
                }
            }
            return false;
        }

        public bool TryGetCastSkillParam(SkillModel skillModel, out FieldObject target, out Vec2 targetPos)
        {
            target = null;
            targetPos = null;
            if (skillModel == null)
            {
                return false;
            }
            switch (skillModel.PositionType)
            {
                case SkillPositionType.Self_NoDirection:
                case SkillPositionType.Self_NeedEnemy:
                    target = this;
                    targetPos = Position;
                    return true;
                case SkillPositionType.Self_Direction:
                    return TryGetCastSkillParamByTarget(skillModel, out target, out targetPos);
                case SkillPositionType.Any:
                    return false;
                case SkillPositionType.Target:
                    return TryGetCastSkillParamByTarget(skillModel, out target, out targetPos);
            }
            return false;
        }

        private bool TryGetCastSkillParamByTarget(SkillModel skillModel, out FieldObject target, out Vec2 targetPos)
        {
            target = null;
            targetPos = null;
            switch (skillModel.PositionFilterType)
            {
                case SkillPositionFilterType.HateEnemy:
                    //if (hateManager == null || hateManager.Target == null || hateManager.Target.IsDead)
                    //{
                    //    if (currentMap is ArenaDungeonMap)
                    //    {
                    //        if ((FieldObjectType == TYPE.HERO || FieldObjectType == TYPE.ROBOT) && TryGetMaxHateRatioTarget(currentMap, out target))
                    //        {
                    //            targetPos = target.Position;
                    //            return true;
                    //        }
                    //    }
                    //    //更新为此处拿最近目标
                    //    if ((FieldObjectType == TYPE.HERO || FieldObjectType == TYPE.ROBOT) && TryGetNeareastEnemyTarget(currentMap, out target))
                    //    {
                    //        targetPos = target.Position;
                    //        return true;
                    //    }
                    //    return false;
                    //}
                    if (TryGetNeareastEnemyTarget(currentMap, out target))
                    {
                        targetPos = target.Position;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    //target = hateManager.Target;
                    //targetPos = hateManager.Target.Position;
                    //return true;
                case SkillPositionFilterType.AllyMainHero:
                    if (FieldObjectType == TYPE.HERO)
                    {
                        target = GetOwner();
                        if (target == null || target.IsDead)
                        {
                            return false;
                        }
                        targetPos = target.Position;
                        return true;
                    }
                    return false;
                case SkillPositionFilterType.FarthestEnemy:
                    if (TryGetFartheastEnemyTarget(currentMap, out target))
                    {
                        targetPos = target.Position;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    Logger.Log.Warn("TryGetCastSkillParamByTarget SkillPositionFilterType {0} not handled", skillModel.PositionFilterType);
                    break;
            }
            return false;
        }

        private bool TryGetNeareastTarget(IFieldObjectContainer container, TYPE type, out FieldObject target)
        {
            target = null;
            targetList.Clear();
            switch (type)
            {
                case TYPE.PC:
                case TYPE.ROBOT:
                    break;
                case TYPE.MONSTER:
                    IEnumerable<Monster> mons = container.GetMonsters().Values;
                    foreach (var item in mons)
                    {
                        if (item.IsDead || item.Borning)
                        {
                            continue;
                        }
                        targetList.Add(item);
                    }
                    break;
                case TYPE.HERO:
                    IEnumerable<Hero> heros = container.GetHeros().Values;
                    foreach (var item in heros)
                    {
                        if (item.IsDead)
                        {
                            continue;
                        }
                        targetList.Add(item);
                    }
                    break;
                default:
                    break;
            }
            FilterTargets(targetList, TargetFilterType.NeareastEnemy);
            if (targetList.Count > 0)
            {
                target = targetList[0];
            }
            if (target == null)
            {
                return false;
            }
            return true;
        }

        private bool TryGetNeareastTarget(IFieldObjectContainer container, out FieldObject target)
        {
            target = null;

            GetEnemyInMap(container);

            FilterTargets(targetList, TargetFilterType.NeareastEnemy);
            if (targetList.Count > 0)
            {
                target = targetList[0];
            }
            if (target == null)
            {
                return false;
            }
            return true;
        }

        private bool TryGetMaxHateRatioTarget(IFieldObjectContainer container, out FieldObject target)
        {
            target = null;

            GetEnemyInMap(container);

            FilterTargets(targetList, TargetFilterType.MaxHateRatioEnemy);
            if (targetList.Count > 0)
            {
                target = targetList[0];
            }
            if (target == null)
            {
                return false;
            }
            return true;
        }

        private bool TryGetMaxHateRatioAndNeareastTarget(IFieldObjectContainer container, out FieldObject target)
        {
            target = null;

            GetEnemyInMap(container);

            FilterTargets(targetList, TargetFilterType.MaxHateRatioNeareastEnemy);
            if (targetList.Count > 0)
            {
                target = targetList[0];
            }
            if (target == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取嘲讽对象
        /// </summary>
        private void GetSneerTarget(out FieldObject target)
        {
            BaseBuff buff = buffManager.GetOneBuffByType(BuffType.Sneer);
            target = buff?.Caster;
        }

        private bool TryGetNeareastEnemyTarget(IFieldObjectContainer container, out FieldObject target)
        {
            GetSneerTarget(out target);

            if (target == null)
            {
                GetEnemyInMap(container);

                FilterTargets(targetList, TargetFilterType.NeareastEnemy);
                if (targetList.Count > 0)
                {
                    target = targetList[0];
                }
            }

            return target != null;
        }

        private bool TryGetFartheastEnemyTarget(IFieldObjectContainer container, out FieldObject target)
        {
            GetSneerTarget(out target);

            if (target == null)
            {
                GetEnemyInMap(container);

                FilterTargets(targetList, TargetFilterType.FarthestEnemy);
                if (targetList.Count > 0)
                {
                    target = targetList[0];
                }
            }

            return target != null;
        }

        private void GetEnemyInMap(IFieldObjectContainer container)
        {
            targetList.Clear();
            if (container == null)return;

            IEnumerable<Monster> mons = container.GetMonsters().Values;
            foreach (var item in mons)
            {
                if (item.IsDead || item.Borning)
                {
                    continue;
                }
                targetList.Add(item);
            }

            IEnumerable<Hero> heros = container.GetHeros().Values;
            foreach (var item in heros)
            {
                if (item.IsDead)
                {
                    continue;
                }
                targetList.Add(item);
            }
        }

        public MSG_ZGC_DAMAGE GenerateDamageMsg(int targetId, DamageType damageType, long damage, bool critical = false, bool dodge = false, bool immune = false, long extraDamage = 0, MSG_ZGC_SKILL_EFF skillMsg = null)
        {
            MSG_ZGC_DAMAGE msg = new MSG_ZGC_DAMAGE()
            {
                TargetId = targetId,
                DamageType = (int) damageType,
                Damage = damage,
                Critical = critical,
                Dodge = dodge,
                Immune = immune,
                ExtraDamage = extraDamage
            };
            
            skillMsg?.DamageList.Add(msg);
            return msg;
        }
    }
}
