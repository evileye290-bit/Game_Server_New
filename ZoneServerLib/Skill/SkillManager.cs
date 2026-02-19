using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class SkillManager
    {
        class CheatChecker
        {
            public int ErrorCount = 0;
            public int ErrorLog = 0;
            public DateTime LastLogTime = DateTime.MinValue;
        }

        FieldObject owner;
        Skill lastNormalSkill;
        Skill lastBodyAtkSkill;
        CheatChecker cheatChecker;
        private Dictionary<int, int> skillUsedCount = new Dictionary<int, int>();

        private Dictionary<int, Skill> skillList = new Dictionary<int, Skill>();
        public Dictionary<int, Skill> SkillList { get { return skillList; } }

        /// <summary>
        /// 技能增伤害百分比
        /// </summary>
        private Dictionary<int, int> enhanceSkillIdDamageRatio = new Dictionary<int, int>();

        private HashSet<int> mustHitSkillEffect = new HashSet<int>();

        /// <summary>
        /// 有武魂真身
        /// </summary>
        public bool HasBodySkill = false;

        /// <summary>
        /// 有主动技
        /// </summary>
        public bool HasActiveSkill = false;

        public SkillManager(FieldObject owner)
        {
            this.owner = owner;
            cheatChecker = new CheatChecker();
        }

        public void Add(int skillId, int level = 1)
        {
            SkillModel skillModel = SkillLibrary.GetSkillModel(skillId);
            if (skillModel == null)
            {
                Log.Warn("player {0} skill manager bind skill id {1} failed: not in skill lib", owner.Uid, skillId);
                return;
            }
            Skill skill = new Skill(owner, skillModel, level, GetSkillUsedCount(skillId));
            skillList.Add(skillId, skill);
            if (skillModel.Type == SkillType.Body)
            {
                HasBodySkill = true;
            }
            if (skillModel.Type == SkillType.Normal_Skill_1)
            {
                HasActiveSkill = true;
            }
        }

        public bool Check(Skill skill)
        {
            if (skill == null || !skill.Check()) return false;

            if (!skill.SkillModel.CastedByOwner && !CheckTime(skill)) return false;

            switch (skill.SkillModel.Type)
            {
                case SkillType.Normal_Attack_1:
                case SkillType.Normal_Attack_2:
                case SkillType.Normal_Attack_3:
                    if (!CheckNormalAttack(skill))
                    {
                        return false;
                    }
                    break;
                case SkillType.Body_Attack_1:
                case SkillType.Body_Attack_2:
                case SkillType.Body_Attack_3:
                    if (!CheckBodyAttack(skill))
                    {
                        return false;
                    }
                    break;
                case SkillType.UnContralableSkill:
                    {
                        return true;
                    }
                case SkillType.Body:
                case SkillType.Normal_Skill_1:
                    if (!CheckManualSkill(skill))
                    {
                        return false;
                    }
                    break;
                default:
                    if (!CheckCommonSkill(skill))
                    {
                        return false;
                    }
                    break;

            }
            return true;
        }

        private bool CheckTime(Skill skill)
        {
            FsmSkillState skillState = owner.FsmManager.CurFsmState as FsmSkillState;

            if (skillState == null) return true;

            if (CanBreakUpNormalSkill(skillState, skill)) return true;

            // 处于技能状态机下，且技能剩余时间超过
            if (skillState.LeftTime > 0.1f)
                return false;
            return true;
        }

        public void CheckBreakNormalSkill(Skill skill)
        {
            FsmSkillState skillState = owner.FsmManager.CurFsmState as FsmSkillState;

            if (skillState == null) return;

            if (CanBreakUpNormalSkill(skillState, skill))
            {
                skillState.OnEnd(FsmStateType.SKILL);
            }
        }

        /// <summary>
        /// 是否可以打断普攻
        /// </summary>
        /// <param name="skillState"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        public bool CanBreakUpNormalSkill(FsmSkillState skillState, Skill skill)
        {
            if (skillState == null || skill == null || owner == null || skill.SkillModel == null || skillState.Skill == null) return false;

            //技能打断普攻，不做时间检测
            if (skill.SkillModel.CastedByOwner && (owner.FieldObjectType == TYPE.PC || owner.FieldObjectType == TYPE.HERO))
            {
                //if (!skill.IsNormalAttack() && skillState.Skill != null && skillState.Skill.IsNormalAttack())
                //{
                //    //Log.WarnLine($"skill {skill.Id} break up skill {skillState.Skill.Id}");
                //    return true;
                //}
                return skill.SkillModel.Priority > skillState.Skill.SkillModel.Priority;
            }

            return false;
        }

        private bool CheckNormalAttack(Skill skill)
        {
            if (owner.InRealBody)
            {
                return false;
            }
            if (!owner.CanCastNormalAttack())
            {
                return false;
            }
            if (skill.SkillModel.Type == SkillType.Normal_Attack_1)
            {
                return true;
            }
            if (lastNormalSkill == null)
            {
                // Normal_Attack_2, Normal_Attack_3情况，不能越过Normal_Attack_1
                return false;
            }
            if (skill.SkillModel.Type == SkillType.Normal_Attack_2)
            {
                return lastNormalSkill.SkillModel.Type == SkillType.Normal_Attack_1;
            }
            if (skill.SkillModel.Type == SkillType.Normal_Attack_3)
            {
                return lastNormalSkill.SkillModel.Type == SkillType.Normal_Attack_2;
            }
            return false;
        }

        private bool CheckBodyAttack(Skill skill)
        {
            if (!owner.InRealBody)
            {
                return false;
            }
            if (skill.SkillModel.Type == SkillType.Body_Attack_1 || skill.SkillModel.Type == SkillType.Body_Attack_4)
            {
                return true;
            }
            if (lastBodyAtkSkill == null)
            {
                return false;
            }
            if (skill.SkillModel.Type == SkillType.Body_Attack_2)
            {
                return lastBodyAtkSkill.SkillModel.Type == SkillType.Body_Attack_1;
            }
            if (skill.SkillModel.Type == SkillType.Body_Attack_3)
            {
                return lastBodyAtkSkill.SkillModel.Type == SkillType.Body_Attack_2;
            }

            return false;
        }

        private bool CheckManualSkill(Skill skill)
        {
            if (owner.InRealBody)
            {
                return false;
            }
            if (!owner.CanCastManualSkill())
            {
                return false;
            }
            return true;
        }

        //手动释放技能
        private bool CheckCommonSkill(Skill skill)
        {
            if (owner.InRealBody)
            {
                return false;
            }
            if (!owner.CanCastSkill(skill.SkillModel))
            {
                return false;
            }

            return true;
        }

        //public void RecordCheating()
        //{
        //    if ((cheatChecker.LastLogTime - ZoneServerApi.now).TotalSeconds >= 1)
        //    {
        //        cheatChecker.LastLogTime = ZoneServerApi.now;
        //        cheatChecker.ErrorLog = 1;
        //    }
        //    else
        //    {
        //        cheatChecker.ErrorLog++;
        //    }

        //    if (cheatChecker.ErrorLog > 3)
        //    {
        //        if (cheatChecker.ErrorCount > 20)
        //        {
        //            Log.Warn("player {0} use skill cheating!", owner.Uid);
        //            cheatChecker.ErrorCount = 0;
        //        }
        //        else
        //        {
        //            cheatChecker.ErrorCount++;
        //        }
        //    }
        //}

        public Skill GetSkill(int skillId)
        {
            Skill skill = null;
            if (!skillList.TryGetValue(skillId, out skill))
            {
                return null;
            }
            return skill;
        }

        public void StartCasting(Skill skill) //技能状态机开始。就认为释放，需要扣能量
        {
            switch (skill.SkillModel.Type)
            {
                case SkillType.Normal_Attack_1:
                case SkillType.Normal_Attack_2:
                case SkillType.Normal_Attack_3:
                    break;
                case SkillType.Normal_Skill_1:
                case SkillType.Common_Normal_Skill:
                case SkillType.Body:
                    skill.ResetEnergy();
                    break;
                case SkillType.Normal_Skill_2:
                case SkillType.Normal_Skill_3:
                case SkillType.Normal_Skill_4:
                    if (HasActiveSkill && !owner.InBuffState(BuffType.NormalSkill1RefuseAddEnergy))
                    {
                        skillList.Where(kv => { return kv.Value.SkillModel.Type == SkillType.Normal_Skill_1; })
                            .ForEach(kv => kv.Value.AddEnergy(skill.SkillModel.AddEnergy));
                    }
                    skill.ResetEnergy();
                    break;
                // 武魂真身下的攻击
                case SkillType.Body_Attack_1:
                case SkillType.Body_Attack_2:
                case SkillType.Body_Attack_3:
                    break;
                // 队长技
                case SkillType.Captain:
                    skill.ResetEnergy();
                    break;
                case SkillType.UnContralableSkill:
                    //Log.Warn($"{this.owner.FieldObjectType} cast skill {skill.SkillModel.Id}");
                    break;
                default:
                    break;
            }
        }

        public void AfterCasting(Skill skill, bool allMissed)
        {
            switch (skill.SkillModel.Type)
            {
                case SkillType.Normal_Attack_1:
                case SkillType.Normal_Attack_2:
                case SkillType.Normal_Attack_3:
                    // 记录最近使用的普攻
                    lastNormalSkill = skill;
                    // 魂环技能加能量，沉默状态下不加能量
                    if (!owner.InBuffState(BuffType.Silent))
                    {
                        AddNormalSkillEnergy(1, true);
                    }
                    break;

                case SkillType.Normal_Skill_1:
                case SkillType.Normal_Skill_2:
                case SkillType.Normal_Skill_3:
                case SkillType.Normal_Skill_4:
                case SkillType.Common_Normal_Skill:
                    //skill.ResetEnergy();
                    // todo 队长技能量添加

                    //Log.Warn($"{this.owner.FieldObjectType} cast skill {skill.SkillModel.Id}");

                    if (!allMissed)
                    {
                        //// 武魂真身加能量
                        //AddBodyEnergy();
                        //组队副本能量瓶加能量
                        //AddReviveEnergy();
                    }
                    break;
                case SkillType.Body:
                    //skill.ResetEnergy();
                    if (owner.CurDungeon != null)
                    {
                        owner.CurDungeon.CheckEnableMixSKill(owner);
                    }
                    break;

                // 武魂真身下的攻击
                case SkillType.Body_Attack_1:
                    lastBodyAtkSkill = skill;
                    //owner.DisableRealBody();
                    break;
                case SkillType.Body_Attack_2:
                case SkillType.Body_Attack_3:
                    lastBodyAtkSkill = skill;
                    break;
                case SkillType.Body_Attack_4:
                    lastBodyAtkSkill = skill;
                    //owner.DisableRealBody();
                    break;
                // 队长技
                case SkillType.Captain:
                    //skill.ResetEnergy();
                    break;
                case SkillType.UnContralableSkill:
                    //Log.Warn($"{this.owner.FieldObjectType} cast skill {skill.SkillModel.Id}");
                    break;
                default:
                    break;
            }
        }

        public void AddEnergy(int heroId, int value, bool needSync = true)
        { 
            if (owner.InBuffState(BuffType.Silent)) return;

            Skill skill;
            if (!skillList.TryGetValue(heroId, out skill)) return;

            if (value > 0)
            {
                skill.AddEnergy(value, needSync);
            }
            else
            {
                skill.ReduceEnergy(value * -1, needSync);
            }
        }

        public void AddEnergy(SkillType type, int value, bool needSync = true, bool fromOtherSkill = false, bool dispatchEnergyMsg = false)
        {
            if (owner.InBuffState(BuffType.Silent)) return;

            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.Type == type)
                {
                    if (value > 0)
                    {
                        skill.Value.AddEnergy(value, needSync, fromOtherSkill, dispatchEnergyMsg);
                    }
                    else
                    {
                        skill.Value.ReduceEnergy(value * -1, needSync, fromOtherSkill);
                    }
                }
            }
        }

        public int GetEnergy(SkillType type)
        {
            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.Type == type)
                {
                    return skill.Value.Energy;
                }
            }
            return int.MaxValue;
        }

        // 释放普攻 给普通技能加energy
        public void AddNormalSkillEnergy(int value, bool needSync)
        {
            if (owner.InBuffState(BuffType.Silent)) return;

            // 一次性发送 节省多次同步单个技能能量的消息数量
            MSG_ZGC_ADD_NORMAL_SKILL_ENERGY msg = new MSG_ZGC_ADD_NORMAL_SKILL_ENERGY();

            MSG_ZGC_SKILL_ENERGY_INFO info;
            int tempEnergy = 0;
            bool syncMsg = false;
            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.IsNormalSkill())
                {
                    tempEnergy = skill.Value.Energy;
                    if (value > 0)
                    {
                        if (!owner.InBuffState(BuffType.NormalSkill1RefuseAddEnergy) || skill.Value.SkillModel.Type != SkillType.Normal_Skill_1)
                        {
                            skill.Value.AddEnergy(value, false);
                        }
                    }
                    else
                    {
                        skill.Value.ReduceEnergy(value * -1, false);
                    }
                    //if (skill.Value.Id == 201)
                    //{
                    //    Log.Debug("caster {3} skill {2} temp {0} energy {1}", tempEnergy, skill.Value.Energy, skill.Value.Id,owner.InstanceId);
                    //}
                    if (tempEnergy != skill.Value.Energy)
                    {
                        if (needSync)
                        {
                            info = new MSG_ZGC_SKILL_ENERGY_INFO();
                            info.Energy = skill.Value.Energy;
                            info.SkillId = skill.Value.Id;
                            msg.EnergyInfo.Add(info);
                            syncMsg = true;
                        }
                    }
                }
            }

            // 如果需要同步energy
            if (syncMsg)
            {
                PlayerChar player = null;
                if (owner.IsPlayer)
                {
                    player = owner as PlayerChar;
                }
                // 伙伴也同步Player 技能能量信息
                else if (owner.IsHero)
                {
                    Hero hero = owner as Hero;
                    if (hero.OwnerIsRobot)
                    {
                        Robot r = hero.Owner as Robot;
                        if (r.CopyedFromPlayer)
                        {
                            player = r.playerMirror;
                        }
                    }
                    else
                    {
                        player = hero.Owner as PlayerChar;
                    }
                }
                else if (owner.IsRobot)
                {
                    Robot robot = owner as Robot;
                    if (robot != null && robot.CopyedFromPlayer)
                    {
                        player = robot.playerMirror;
                    }
                }
                if (player == null)
                {
                    return;
                }

                msg.AddEnergy = value;
                msg.InstanceId = owner.InstanceId;
                player.Write(msg);
            }
        }

        //public void AddBodyEnergy()
        //{
        //    if (owner.InBuffState(BuffType.Silent)) return;

        //    foreach (var skill in skillList)
        //    {
        //        if (skill.Value.SkillModel.Type == SkillType.Body)
        //        {
        //            skill.Value.AddEnergy(1, true);
        //        }
        //    }
        //}

        //public void AddBodyEnergyPerTime()
        //{
        //    if (owner.InBuffState(BuffType.RefuseRealBodyEnemy)) return;

        //    foreach (var skill in skillList)
        //    {
        //        if (skill.Value.SkillModel.Type == SkillType.Body)
        //        {
        //            skill.Value.AddEnergy(1, true);
        //        }
        //    }
        //}

        public void AddBodyEnergy(int value, bool fromOtherSkill = false, bool dispatchEnergyMsg = false, bool force = false)
        {
            if (owner.InRealBody)
            {
                return;
            }

            //强制调整受到拒绝添加能量buff的影响
            if (!force && owner.InBuffState(BuffType.RefuseRealBodyEnemy))
            {
                return;
            }

            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.Type == SkillType.Body)
                {
                    if (value > 0)
                    {
                        skill.Value.AddEnergy(value, true, fromOtherSkill, dispatchEnergyMsg);
                    }
                    else
                    {
                        skill.Value.ReduceEnergy(value * -1, true, fromOtherSkill);
                    }
                }
            }
        }

        public void AddHitBodyEnergy(long damage, int targetCount)
        {
            if (TeamLibrary.EnergyHitRatio > 0 && targetCount > 0)
            {
                float add = TeamLibrary.EnergyHitRatio * damage / (owner.GetNatureValue(NatureType.PRO_ATK)) / targetCount;
                //Log.Debug($"hero {owner.GetHeroId()} AddHitBodyEnergy with EnergyHitRatio {TeamLibrary.EnergyHitRatio} * damage {damage} / (NatureType.PRO_ATK {owner.GetNatureValue(NatureType.PRO_ATK)}) / targetCount {targetCount} =add {add}");
                AddBodyEnergy((int)add);
            }
        }

        public void AddOnHitBodyEnergy(long damage)
        {
            if (TeamLibrary.EnergyOnHitRatio > 0)
            {
                long maxHp = owner.GetNatureValue(NatureType.PRO_MAX_HP);
                if (maxHp > 0)
                {
                    float add = TeamLibrary.EnergyOnHitRatio * damage / owner.GetNatureValue(NatureType.PRO_MAX_HP);
                    //Log.Debug($"hero {owner.GetHeroId()} AddOnHitBodyEnergy with EnergyOnHitRatio {TeamLibrary.EnergyOnHitRatio} * damage {damage} / NatureType.PRO_MAX_HP {owner.GetNatureValue(NatureType.PRO_MAX_HP)} =add {add}");
                    AddBodyEnergy((int)add);
                }
            }
        }

        public void AddCureBodyEnergy(long cure)
        {
            if (TeamLibrary.EnergyCureRatio > 0)
            {
                long maxHp = owner.GetNatureValue(NatureType.PRO_MAX_HP);
                if (maxHp > 0)
                {
                    float add = TeamLibrary.EnergyCureRatio * cure / owner.GetNatureValue(NatureType.PRO_MAX_HP);
                    //Log.Debug($"hero {owner.GetHeroId()} AddCureBodyEnergy with EnergyCureRatio {TeamLibrary.EnergyCureRatio} * cure {cure} / NatureType.PRO_MAX_HP {owner.GetNatureValue(NatureType.PRO_MAX_HP)} =add {add}");
                    AddBodyEnergy((int)add);
                }
            }
        }

        public void AddReviveEnergy()
        {
            if (owner.CurDungeon == null)
            {
                return;
            }
            if (owner.FieldObjectType == TYPE.PC || owner.FieldObjectType == TYPE.HERO)
            {
                owner.CurDungeon.AddReviveEnergy();
            }
        }

        public void ReduceEnergyLimit(SkillType skillType, int value)
        {
            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.Type == skillType)
                {
                    skill.Value.SetEnergyLimit(skill.Value.GetEnergyLimit() - value);
                }
            }
        }

        public void SetEnergyLimit(SkillType skillType, int energyLimit)
        {
            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.Type == skillType)
                {
                    skill.Value.SetEnergyLimit(energyLimit);
                }
            }
        }

        public void SetEnergyEnough(SkillType skillType)
        {
            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.Type == skillType)
                {
                    skill.Value.SetEnergyEnough();
                }
            }
        }

        // 在战斗开始时，同步前端技能的能量信息
        public MSG_ZGC_SKILL_ENERGY_LIST GetSkillEnergyMsg()
        {
            MSG_ZGC_SKILL_ENERGY_LIST msg = new MSG_ZGC_SKILL_ENERGY_LIST();
            foreach (var skill in skillList)
            {
                if (!skill.Value.SkillModel.IsPassive())
                {
                    msg.List.Add(skill.Value.GenerateEnergyMsg());
                }
            }
            return msg;
        }

        // 对于Hero 返回主角可释放的技能
        public MSG_ZGC_SKILL_ENERGY_LIST GetSkillEnergyForOwner()
        {
            MSG_ZGC_SKILL_ENERGY_LIST msg = new MSG_ZGC_SKILL_ENERGY_LIST();
            Hero hero = owner as Hero;
            if (hero == null) return msg;
            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.CastedByOwner && skill.Value.IsEnergyEnough())
                {
                    msg.List.Add(skill.Value.GenerateEnergyMsg());
                }
            }
            return msg;
        }

        public void FrozenSkill(SkillType type)
        {
            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.Type == type)
                {
                    skill.Value.UpdateFrozenCount(1);
                }
            }
        }

        public void UnFrozenSkill(SkillType type)
        {
            foreach (var skill in skillList)
            {
                if (skill.Value.SkillModel.Type == type)
                {
                    skill.Value.UpdateFrozenCount(-1);
                }
            }
        }

        public void ReplaceSkillByType(int newSkillId, int oldSkillId)
        {
            SkillModel model = SkillLibrary.GetSkillModel(newSkillId);
            if (model == null) return;

            Skill oldSkill = GetSkill(oldSkillId);
            if (oldSkill == null) return;

            Skill newSkill = new Skill(owner, model, oldSkill.Level);

            skillList.Remove(oldSkill.Id);
            skillList.Add(newSkill.Id, newSkill);
        }

        public int GetSkillUsedCount(int skillId)
        {
            int count;
            skillUsedCount.TryGetValue(skillId, out count);
            return count;
        }

        public void AddSkillUsedCount(int skillId)
        {
            int count = GetSkillUsedCount(skillId);
            skillUsedCount[skillId] = ++count;
        }

        public int GetSkillEnhancedDamageRatio(int skillId)
        {
            int ratio;
            enhanceSkillIdDamageRatio.TryGetValue(skillId, out ratio);
            return ratio;
        }

        public void AddSkillEnhanceDamageRatio(int skillId, int addRatio)
        {
            if (!skillList.ContainsKey(skillId)) return;

            int ratio;
            if (enhanceSkillIdDamageRatio.TryGetValue(skillId, out ratio))
            {
                ratio += addRatio;
            }
            else
            {
                ratio = addRatio;
            }
            enhanceSkillIdDamageRatio[skillId] = ratio;
        }

        public void AddSkillEnhanceDamageRatio(SkillType skillType, int addRatio)
        {
            Skill skill = skillList.Values.FirstOrDefault(x => x.SkillModel.Type == skillType);
            if (skill == null) return;

            int ratio;
            if (enhanceSkillIdDamageRatio.TryGetValue(skill.Id, out ratio))
            {
                ratio += addRatio;
            }
            else
            {
                ratio = addRatio;
            }
            enhanceSkillIdDamageRatio[skill.Id] = ratio;
        }

        public void SetMustHitSkillEffect(int skillEffectId)
        {
            if (!mustHitSkillEffect.Contains(skillEffectId))
            {
                mustHitSkillEffect.Add(skillEffectId);
            }
        }

        public bool IsMustHitSkillEffect(int skillEffectId)
        {
            return mustHitSkillEffect.Contains(skillEffectId);
        }
    }
}
