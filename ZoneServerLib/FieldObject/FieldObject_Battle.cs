using CommonUtility;
using DataProperty;
using System;
using System.Collections.Generic;
using System.IO;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using System.Linq;
using System.ComponentModel;
using ServerShared;
using ServerModels;

namespace ZoneServerLib
{
    public partial class FieldObject 
    {
        protected bool InBattle
        { get; set; }
        public bool IsReviving
        { get; set; }

        public bool InRealBody
        { get; private set; }

        public virtual float DeadDelay
        { get { return 3.0f; }}

        public bool IsAttacker = false;

        public bool IsAlly(FieldObject target)
        { return IsAttacker == target.IsAttacker; }

        public bool IsEnemy(FieldObject target)
        { return IsAttacker != target.IsAttacker; }

        protected bool isDead = false;

        public bool InDungeon
        { get { return (currentMap != null && currentMap.IsDungeon); } }

        public int KillerId { get; private set; }

        public virtual void StartFighting()
        {
            InBattle = true;
            DisableRealBody();
        }

        public virtual void StopFighting()
        {
            InBattle = false;
            targetList.Clear();
            FsmManager?.SetNextFsmStateType(FsmStateType.IDLE, true);
        }

        public virtual void RealReleaseSkill(int skillId)
        {
            SkillEngine.AddSkill(skillId, null);
        }

        public virtual void ClearBasicBattleState()
        {
            if (TriggerMng != null)
            {
                TriggerMng.ClearSelfTriggers();
                TriggerMng.StopTriggersFromOther();
            }
            if (messageDispatcher != null)
            {
                messageDispatcher.Stop();
            }

            //需要先清除buff 然后再重置nature，防止某类buff结束广播血量，导致前端显示角色死亡
            if (buffManager != null)
            {
                buffManager.Stop();
            }
            skillManager = new SkillManager(this);
            ResetNature();
            DisableRealBody();            
            hateManager = null;
            markManager = null;
        }

        // 被动技能起效
        public void PassiveSkillEffect()
        {
            // 由于skill effect可能对skillList做修改 所以需要先提出所有skill再做处理
            List<Skill> skills = new List<Skill>();
            foreach(var item in skillManager.SkillList)
            {
                skills.Add(item.Value);
            }
            foreach(var skill in skills)
            {
                if(!skill.SkillModel.IsPassive())
                {
                    continue;
                }
                FieldObject target = null;
                Vec2 targetPos = null;
                if(!TryGetCastSkillParam(skill.SkillModel, out target, out targetPos))
                {
                    continue;
                }
                skill.InitCastParam(targetPos - Position, targetPos, target == null ? 0 : target.InstanceId);
                SkillEffect(skill);
            }
        }

        // 根据caster状态，调整damage数值
        private long AdjustDamageByCaster(FieldObject caster, long damage)
        {
            if (caster == null) return damage;

            //if (caster.GetNatureValue(NatureType.PRO_BURN_MORE) > 0 && InBuffState(BuffType.Burn))
            //{
            //    damage = (long)(damage * (1 + caster.GetNatureValue(NatureType.PRO_BURN_MORE) * 0.0001));
            //}

            //if (caster.GetNatureValue(NatureType.PRO_POISON_MORE) > 0 && (InBuffState(BuffType.Poison) || InBuffState(BuffType.PoisonDGB)))
            //{
            //    damage = (long)(damage * (1 + caster.GetNatureValue(NatureType.PRO_POISON_MORE) * 0.0001));
            //}

            if (caster.GetNatureValue(NatureType.PRO_DAM_TO_CTR) > 0 && BeControlled())
            {
                damage = (long)(damage * (1 + caster.GetNatureValue(NatureType.PRO_DAM_TO_CTR) * 0.0001f));
            }

            //职业伤害增益
            long jobDamageRatio = 0;
            Hero hero = this as Hero;
            JobType jobType = hero == null ? JobType.None : hero.GetJobType();

            switch (jobType)
            {
                case JobType.SingleAttack:
                    jobDamageRatio = caster.GetNatureValue(NatureType.PRO_DO_JOB_DAMAGE_SINGLEATTACK);
                    break;
                case JobType.Tank:
                    jobDamageRatio = caster.GetNatureValue(NatureType.PRO_DO_JOB_DAMAGE_TANK);
                    break;
                case JobType.Support:
                    jobDamageRatio = caster.GetNatureValue(NatureType.PRO_DO_JOB_DAMAGE_SUPPORT);
                    break;
                case JobType.Control:
                    jobDamageRatio = caster.GetNatureValue(NatureType.PRO_DO_JOB_DAMAGE_CONTROL);
                    break;
                case JobType.GroupAttack:
                    jobDamageRatio = caster.GetNatureValue(NatureType.PRO_DO_JOB_DAMAGE_GROUPATTACK);
                    break;
            }
            if (jobDamageRatio > 0)
            { 
                damage = (long)(damage * (1 + jobDamageRatio * 0.0001f));
            }

            long enhanceDamage = caster.GetNatureValue(NatureType.PRO_DAM_ENHANCE_RATIO_ONCE);
            damage = (long)(damage * (1 + (caster.GetNatureValue(NatureType.PRO_DAM) + enhanceDamage) * 0.0001f));

            if (enhanceDamage > 0)
            {
                caster.SetNatureBaseValue(NatureType.PRO_DAM_ENHANCE_RATIO_ONCE, 0);
            }

            return damage;
        }

        // 根据伤害类型，调整damage数值
        private long AdjustDamageByType(FieldObject caster, DamageType damageType, long damage, ref bool immune)
        {
            long finalDamage = damage;
            switch (damageType)
            {
                case DamageType.Bleed:
                    if (caster.InBuffState(BuffType.BleedMore))
                    {
                        finalDamage += caster.GetBuffTotal_M(BuffType.BleedMore);
                    }
                    finalDamage = (long)(finalDamage * (1 + GetNatureValue(NatureType.PRO_BLEED_ADD_MORE) * 0.0001f));
                    finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_BLEED_MORE) * 0.0001f));
                    break;
                case DamageType.Burn:
                    if (InBuffState(BuffType.Burn))
                    {
                        finalDamage = (long)(finalDamage * (1 + GetNatureValue(NatureType.PRO_BURN_ADD_MORE) * 0.0001f));
                        finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_BURN_MORE) * 0.0001f));
                    }
                    break;
                case DamageType.Poison:
                    if (InBuffState(BuffType.Poison) || InBuffState(BuffType.PoisonDGB))
                    {
                        long ratio = GetNatureValue(NatureType.PRO_POISON_ADD_MORE) - GetNatureValue(NatureType.PRO_POISON_REDUCE);
                        finalDamage = (long)(finalDamage * (1 + ratio * 0.0001f));
                        finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_POISON_MORE) * 0.0001f));
                    }
                    break;
                case DamageType.Normal:
                    {
                        if (InBuffState(BuffType.SkillAttackDamage))
                        {
                            finalDamage = 0;
                            immune = true;
                            return 0;
                        }
                        
                        //caster 单次增伤固定值
                        finalDamage += caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_VALUE_ONCE);
                        caster.SetNatureBaseValue(NatureType.PRO_ADD_DAMAGE_VALUE_ONCE,0);
                        //caster 单次增伤百分比
                        finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE) * 0.0001f));
                        caster.SetNatureBaseValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE,0);
                        caster.SetNatureAddedValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE,0);

                        //caster 增伤固定值
                        finalDamage += caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_VALUE);
                        // caster 对damage 增强
                        finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_ADAM) * 0.0001f)) + caster.GetNatureValue(NatureType.PRO_NORMAL_ATK);
                        // 自身 对damage 衰减
                        finalDamage = (long)(finalDamage * (1 - GetNatureValue(NatureType.PRO_DEF_ADAM) * 0.0001f));

                        //衰减最终伤害至少1
                        if (finalDamage <= 0) finalDamage = 1;
                    }
                    break;
                case DamageType.Skill:
                    {
                        //只受到普攻伤害
                        if (InBuffState(BuffType.NormalAttackDamage))
                        {
                            finalDamage = 0;
                            immune = true;
                            return 0;
                        }

                        //闪避技能
                        long fleeSkillRate = GetNatureValue(NatureType.PRO_FLEE_SKL);
                        if (fleeSkillRate > 0 && RAND.Range(1, 10000) < fleeSkillRate)
                        {
                            finalDamage = 0;
                            immune = true;
                            return 0;
                        }

                        //免疫一次技能伤害
                        if (InBuffState(BuffType.IgnoreSkillDamageAndReboundOnce))
                        {
                            BaseBuff buff = buffManager.GetOneBuffByType(BuffType.IgnoreSkillDamageAndReboundOnce);
                            if (buff != null)
                            {
                                buff.OnEnd();
                                caster.DoSpecDamage(this, DamageType.Thorns, finalDamage);

                                finalDamage = 0;
                                immune = true;
                                return 0;
                            }
                        }

                        //caster 单次增伤固定值
                        finalDamage += caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_VALUE_ONCE);
                        caster.SetNatureBaseValue(NatureType.PRO_ADD_DAMAGE_VALUE_ONCE,0);
                        //caster 单次增伤百分比
                        finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE) * 0.0001f));
                        caster.SetNatureBaseValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE,0);
                        caster.SetNatureAddedValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE,0);
                        //caster 增伤固定值
                        finalDamage += caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_VALUE);
                        // caster 对damage 增强
                        finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_SDAM) * 0.0001f));
                        // 自身 对damage 衰减
                        finalDamage = (long)(finalDamage * (1 - GetNatureValue(NatureType.PRO_DEF_SDAM) * 0.0001f));

                        //衰减最终伤害至少1
                        if (finalDamage <= 0) finalDamage = 1;
                    }
                    break;
                case DamageType.Body:
                    {
                        //只受到普攻伤害
                        if (InBuffState(BuffType.NormalAttackDamage) || InBuffState(BuffType.SkillAttackDamage))
                        {
                            finalDamage = 0;
                            immune = true;
                            return 0;
                        }
                        
                        //caster 单次增伤固定值
                        finalDamage += caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_VALUE_ONCE);
                        caster.SetNatureBaseValue(NatureType.PRO_ADD_DAMAGE_VALUE_ONCE,0);
                        //caster 单次增伤百分比
                        finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE) * 0.0001f));
                        caster.SetNatureBaseValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE,0);
                        caster.SetNatureAddedValue(NatureType.PRO_ADD_DAMAGE_RATIO_ONCE,0);
                        //caster 增伤固定值
                        finalDamage += caster.GetNatureValue(NatureType.PRO_ADD_DAMAGE_VALUE);

                        finalDamage = (long)(finalDamage * (1 + caster.GetNatureValue(NatureType.PRO_BDAM) * 0.0001f));
                    }
                    break;
                default:
                    break;
            }
            return finalDamage;
        }

        // 根据自身状态，调整
        private long AdjustDamageBySelf(long damage, ref bool immune)
        {
            if (damage <= 0)
            {
                immune = true;
                return 0;
            }

            long finalDamage = damage;
            // 无敌状态 无伤害
            if (InBuffState(BuffType.Invincible))
            {
                immune = true;
                return 0;
            }

            // 减伤百分比属性
            long reduceDamageRatio = GetNatureValue(NatureType.PRO_RDC_DMG_RATIO);
            if (reduceDamageRatio > 0)
            {
                if(reduceDamageRatio > 8000)
                {
                    reduceDamageRatio = 8000;
                }
                finalDamage = (long)(finalDamage * ((10000 - reduceDamageRatio) * 0.0001f));
            }

            //减伤固定值属性
            long reduceDamage = GetNatureValue(NatureType.PRO_RDC_DMG);
            if (reduceDamage > 0)
            {
                finalDamage = finalDamage - reduceDamage;
            }

            //减伤最多减到1
            if (finalDamage <= 0) finalDamage = 1;

            // 易伤百分比属性
            long addDamageRatio = GetNatureValue(NatureType.PRO_ADD_DMG_RATIO);
            if(addDamageRatio > 0)
            {
                finalDamage = (long)(finalDamage * ((10000 + addDamageRatio) * 0.0001f));
            }
            //单次易伤
            long addOnceDamage = GetNatureValue(NatureType.PRO_ADD_DMG_ONCE);
            if (addOnceDamage > 0)
            {
                finalDamage = (long)(finalDamage * ((10000 + addOnceDamage) * 0.0001f));
                SetNatureBaseValue(NatureType.PRO_ADD_DMG_ONCE, 0);
            }          
            //易伤固定值属性
            long addDamage = GetNatureValue(NatureType.PRO_ADD_DMG);
            if (addDamage > 0)
            {
                finalDamage = finalDamage + addDamage;
            }
            //单次固定值易伤
            long fixedDamOnce = GetNatureValue(NatureType.PRO_FIXED_DAM_ONCE);
            if (fixedDamOnce > 0)
            {
                finalDamage = finalDamage + fixedDamOnce;
                SetNatureBaseValue(NatureType.PRO_FIXED_DAM_ONCE, 0);
            }
            //被控受伤提升
            if (GetNatureValue(NatureType.PRO_DAM_IN_CTR) > 0 && BeControlled())
            {
                finalDamage = (long)(finalDamage * (1 + GetNatureValue(NatureType.PRO_DAM_IN_CTR) * 0.0001f));
            }

            //免疫**点以下的伤害
            if (InBuffState(BuffType.IgnoreLessDamage))
            {
                BaseBuff buff = buffManager.GetOneBuffByType(BuffType.IgnoreLessDamage);
                if (buff != null && buff.M >= finalDamage)
                {
                    immune = true;
                    finalDamage = 0;
                }
            }

            if (InBuffState(BuffType.IgnoreDamage))
            {
                BaseBuff buff = buffManager.GetOneBuffByType(BuffType.IgnoreDamage);
                if (buff!=null && buff.ProbabilityHappened())
                {
                    immune = true;
                    finalDamage = 0;
                }
            }

            // 致命一击免疫
            if (InBuffState(BuffType.EscapeFromDeath) && finalDamage > GetHp())
            {
                bool happened = false;
                List<BaseBuff> buffList = buffManager.GetBuffsByType(BuffType.EscapeFromDeath);
                foreach (var buff in buffList)
                {
                    if(buff.ProbabilityHappened())
                    {
                        immune = true;
                        happened = true;
                        finalDamage = 0;
                        break;
                    }
                }
                if(happened)
                {
                    buffManager.RemoveBuffsByType(BuffType.EscapeFromDeath);
                }
            }
            return finalDamage;
        }

        public long AdjustDamage(FieldObject caster, DamageType damageType, long damage, ref bool immune)
        {
            long finalDamage = damage;
            finalDamage = AdjustDamageByCaster(caster, damage);
            finalDamage = AdjustDamageByType(caster, damageType, finalDamage, ref immune);
            finalDamage = AdjustDamageBySelf(finalDamage, ref immune);
            return finalDamage;
        }



        // 基类OnHit只做伤害处理，其他逻辑如死亡应在对应子类做处理
        public virtual long OnHit(FieldObject caster, DamageType damageType, long damage, ref bool immune, float multipleParam = 1)
        {
            if (IsDead || !InBattle)
            {
                return 0;
            }

            damage = AdjustDamage(caster, damageType, damage, ref immune);
            if (damage <= 0)
            {
                // 什么都没有发生
                return 0;
            }
            //通过伤害系数分摊伤害
            damage = (long)(damage * multipleParam);
            if (damage <= 0)
            {
                //给一个最小伤害
                damage = 1;
            }
            // 护盾
            if (InShield())
            {
                long shieldHp = GetNatureValue(NatureType.PRO_SHIELD_HP);
                if (shieldHp > damage)
                {
                    DispatchShieldDamageMsg(caster, damage);
                    UpdateShieldHp(damage * -1);
                }
                else
                {
                    // 打破护盾
                    DispatchShieldDamageMsg(caster, shieldHp);
                    UpdateShieldHp(shieldHp * -1);
                    DispatchShieldBreakUpMsg();

                    UpdateHp(damageType, (damage - shieldHp) * -1, caster);
                }
            }
            else
            {
                UpdateHp(damageType, damage * -1, caster);
            }

            // 吸血
            DoVampireLogic(caster, damageType, damage);
            
            // 溅射
            if (damageType == DamageType.Normal && caster.InBuffState(BuffType.Splash))
            {
                caster.BuffManager.DoSpecLogic(BuffType.Splash, new DamageTriMsg(caster, damageType, damage, this));
            }
            // 反伤 
            if ((damageType == DamageType.Normal || damageType == DamageType.Skill || damageType == DamageType.Body)
                && InBuffState(BuffType.DamageRebound) && !IsDead && caster != null)
            {
                List<BaseBuff> reboundBuffList = buffManager.GetBuffsByType(BuffType.DamageRebound);
                foreach(var buff in reboundBuffList)
                {
                    if (buff.IsEnd || caster.IsDead)
                    {
                        continue;
                    }
                    caster.DoSpecDamage(this, DamageType.Thorns, (long)(damage * (buff.C * 0.0001f)));
                }
            }

            if (IsDead)
            {
                SetKiller(caster.instanceId);
            }

            Logger.Log.Debug($"caster {caster.FieldObjectType} {caster.instanceId} {caster.Position} hit {FieldObjectType} {instanceId} {Position} with damageType {damageType} damage {damage}");
            return damage;
        }

        private void DoVampireLogic(FieldObject caster, DamageType damageType, long  damage)
        {
            if(caster == null || damage <= 0) return;

            switch (damageType)
            {
                case DamageType.Normal:
                case DamageType.Skill:
                case DamageType.Body:
                case DamageType.Extra:
                    if (caster.InBuffState(BuffType.Vampire))
                    {
                        caster.BuffManager.DoSpecLogic(BuffType.Vampire, damage);
                    }
                    if (caster.InBuffState(BuffType.VampireOnHpLess))
                    {
                        caster.BuffManager.DoSpecLogic(BuffType.VampireOnHpLess, damage);
                    }
                    break;
            }
        }

        public virtual void OnDead()
        {
            //副本不存在、未在正常开启状态不接收消息
            DungeonMap dungeonMap = currentMap as DungeonMap;
            if (dungeonMap == null || dungeonMap.State != DungeonState.Started)
            {
                return;
            }

            //field object trigger
            DispatchMessage(TriggerMessageType.Dead, this);

            //map trigger
            dungeonMap.DispatchFieldObjectDeadMsg(this);

            StopFighting();//清理之后messageDispatcher会停止，所以message一定要在之前处理
            dungeonMap.OnFieldObjectDead(this);

            //移除领域
            dungeonMap.RemoveDomain(this);

            Logger.Log.Debug($"{FieldObjectType} instance {instanceId} dead");
        }

        /// <summary>
        /// 变身
        /// </summary>
        public virtual void OnChanged()
        {
        }

        //开始复活操作
        public virtual void Revive()
        {
        }

        //复活后事件
        public virtual void OnRevived()
        {
            IsReviving = false;
            isDead = false;
        }

        public virtual bool CanBeRevived()
        {
            return IsReviving;
        }

        private void SetKiller(int instanceId)
        {
            KillerId = instanceId;
        }

        public bool InShield()
        {
            return GetNatureValue(NatureType.PRO_SHIELD_HP) > 0;
        }

        public void DoSpecDamage(FieldObject caster, DamageType damageType, long damage, object param = null)
        {
            bool imm = false;
            damage = OnHit(caster, damageType, damage, ref imm);
            if (damage <= 0) return;

            if (IsDead)
            {
                caster.DispatchKillEnemyMsg(KillerId, 0, instanceId, false, damageType, param);
            }

            MSG_ZGC_DAMAGE damageMsg = GenerateDamageMsg(InstanceId, damageType, damage);
            BroadCast(damageMsg);
        }

        /// <summary>
        /// 只有治疗buff才走该方法，其他其他方法走血量回复
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="cure"></param>
        public void DoCure(FieldObject caster, Int64 cure, bool dispatchCureMsg = false)
        {
            if (cure <= 0) return;
            if (dispatchCureMsg)
            {
                DispatchCastCureBuffMsg(caster);
            }
            // 治疗效果计算
            if (cure > 0)
            {
                cure = (Int64)(cure * (1 + GetNatureAddedValue(NatureType.PRO_CURE) * 0.0001f));

                long enhanceRatio = 0;

                //计算治疗增强百分比
                {
                    enhanceRatio += GetNatureValue(NatureType.PRO_BECURED_ENHANCE);
                    enhanceRatio += caster.GetNatureAddedValue(NatureType.PRO_CURE_ENHANCE);

                    if (caster.GetNatureAddedValue(NatureType.PRO_CURE_ENHANCE_ONCE) > 0)
                    {
                        enhanceRatio += caster.GetNatureAddedValue(NatureType.PRO_CURE_ENHANCE_ONCE);
                        caster.SetNatureAddedValue(NatureType.PRO_CURE_ENHANCE_ONCE, 0);
                    }
                }

                //治疗半分比加成后的值
                cure = (Int64)(cure * (1 + enhanceRatio * 0.0001f));

                if (GetNatureAddedValue(NatureType.PRP_CURE_ENHANCE_FIXED) != 0)
                {
                    cure += GetNatureAddedValue(NatureType.PRP_CURE_ENHANCE_FIXED);
                }

                // PRO_CURE可能为负 如治疗效果下降buff
                if (cure <= 0)
                {
                    return;
                }
            }
            UpdateHp(DamageType.Cure, cure, caster);

            if (dispatchCureMsg)
            {
                DispatchCuredMsg();
            }

            caster?.SkillManager?.AddCureBodyEnergy(cure);
            MSG_ZGC_DAMAGE damageMsg = GenerateDamageMsg(InstanceId, DamageType.Cure, cure * -1);
            BroadCast(damageMsg);
        }

        /// <summary>
        /// 用于治疗时候额外加血等，不需要触发当收到治疗时候的一系列triger
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="hp"></param>
        public void AddHp(FieldObject caster, Int64 hp)
        { 
            if (hp <= 0) return;

            UpdateHp(DamageType.Cure, hp, caster);

            MSG_ZGC_DAMAGE damageMsg = GenerateDamageMsg(InstanceId, DamageType.Cure, hp * -1);
            BroadCast(damageMsg);
        }

        public void EnableRealBody()
        {
            if (InRealBody) return;
            InRealBody = true;
            BroadcastSimpleInfo();
        }

        public void DisableRealBody()
        {
            if (!InRealBody) return;
            InRealBody = false;
            ResetRadius();

            // 清除待释放的真身攻击
            if(skillEngine != null)
            {
                skillEngine.RemoveBodyAttacks();
            }
            // 停止当前的技能
            FsmSkillState skillFSM = FsmManager.CurFsmState as FsmSkillState;
            if (skillFSM != null)
            {
                skillFSM.LeftTime = 0;
            }
            BroadcastSimpleInfo();
        }

        public DUNGEON_FIELDOBJECT GetDungeonFieldObjectMsg()
        {
            DUNGEON_FIELDOBJECT msg = new DUNGEON_FIELDOBJECT();
            msg.InstanceId = instanceId;
            msg.ShieldMaxHp = GetNatureValue(NatureType.PRO_SHIELD_MAX_HP);
            msg.ShieldHp = GetNatureValue(NatureType.PRO_SHIELD_HP);
            msg.RealBodyInfo = new MSG_ZGC_REALBODY_TIME();
            if (InRealBody && buffManager != null)
            {
                List<BaseBuff> buffList = buffManager.GetBuffsByType(BuffType.DisableRealBody);
                if (buffList.Count > 0)
                {
                    BaseBuff realBodyBuff = buffList[0];
                    msg.RealBodyInfo.DuringTime = realBodyBuff.LeftTime;
                    msg.RealBodyInfo.OriginShapeTime = realBodyBuff.KeepTime - realBodyBuff.C;
                    msg.RealBodyInfo.InstanceId = instanceId;
                }
            }
            
            if (buffManager != null)
            {
                List<int> buffList = buffManager.GetBuffIds();
                foreach (var buffType in buffList)
                {
                    msg.BuffList.Add((int)buffType);
                }
            }
            return msg;
        }

        public void BroadCastRevived()
        {
            MSG_ZGC_FIELDOBJECT_REVIVE msg = new MSG_ZGC_FIELDOBJECT_REVIVE();
            msg.InstanceId = instanceId;
            BroadCast(msg);
        }

        public long DoExtraDamage(FieldObject caster)
        {
            //额外伤害
            long extraDamage = GetNatureValue(NatureType.PRO_EXTRA_DAMAGE_ONCE);
            if (extraDamage > 0)
            {
                SetNatureBaseValue(NatureType.PRO_EXTRA_DAMAGE_ONCE, 0);
            }

            extraDamage += GetNatureValue(NatureType.PRO_EXTRA_DAMAGE);

            bool imm = false;
            extraDamage = OnHit(caster, DamageType.Extra, extraDamage, ref imm);

            return extraDamage;
        }

        protected virtual void BroadCastHiddenWeaponInfo()
        {
        }

    }
}