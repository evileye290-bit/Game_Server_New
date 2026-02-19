using CommonUtility;
using EnumerateUtility;
using ScriptFighting;
using ServerModels;
using ServerShared;
using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class FieldObject
    {
        public int GetInstanceId()
        {
            return instanceId;
        }
        public Natures GetNature()
        {
            return nature;
        }
        public void AddBuff(IFightingObject caster, int buffId, int skillLevel)
        {
            if (caster == null)
            {
                AddBuff(null, buffId, skillLevel);
            }
            else
            {
                AddBuff((FieldObject)caster, buffId, skillLevel);
            }
        }

        public void EnhanceSkillEffect(int enhancePolicy, int enhanceSkillLevel)
        {
            SkillEffectEnhancePolicy policy = SkillEffectEnhancePolicyLibrary.GetSkillEnhanceEffectPolicy(enhancePolicy);
            if (policy == null)
            {
                return;
            }
            Skill skill = skillManager.GetSkill(policy.SkillId);
            if (skill == null)
            {
                return;
            }
            SkillEffectModel enhanceModel = SkillEffectLibrary.GetSkillEffectModel(policy.EnhanceEffectId);
            if (enhanceModel == null)
            {
                return;
            }
            foreach (var skillEffect in skill.SkillEffectList)
            {
                if (skillEffect.BasicModel != null && skillEffect.BasicModel.Id == policy.BasicEffectId)
                {
                    skillEffect.SetEnhancePolicy(policy);
                    skillEffect.SetEnhanceModel(enhanceModel);
                    skillEffect.SetEnhanceLevel(enhanceSkillLevel);
                    break;
                }
            }
        }

        public void AddTriggerCreatedBySkill(int triggerId, int skillLevel, IFightingObject caster)
        {
            TriggerCreatedBySkill trigger = new TriggerCreatedBySkill(this, triggerId, skillLevel, caster as FieldObject);
            AddTrigger(trigger);
        }

        public void HeroOwnerAddBuff(IFightingObject caster, int buffId, int skillLevel)
        {
            FieldObject hero = (FieldObject)caster;
            if(hero.FieldObjectType != TYPE.HERO )
            {
                return;
            }
            FieldObject owner = hero.GetOwner();
            if(owner == null)
            {
                return;
            }
            owner.AddBuff(hero, buffId, skillLevel);
        }

        public void HeroOwnerAddTrigger(IFightingObject caster, int triggerId, int skillLevel)
        {
            Hero hero = ((FieldObject)caster) as Hero;
            if (hero == null)
            {
                return;
            }
            FieldObject owner = hero.GetOwner();
            if(owner == null)
            {
                return;
            }
            TriggerCreatedBySkill trigger = new TriggerCreatedBySkill(owner, triggerId, skillLevel, caster as FieldObject);
            trigger.RecordFixedParam(TriggerParamKey.HeroId, hero.HeroId);
            trigger.RecordFixedParam(TriggerParamKey.CreatedBySkillLevel, skillLevel);
            trigger.RecordFixedParam(TriggerParamKey.CreatedBySkillLevelGrowth, SkillLibrary.GetSkillGrowth(skillLevel));

            if (owner.TriggerMng != null)
            {
                owner.TriggerMng.AddTriggersFromOther(caster.GetInstanceId(), trigger);
            }
        }

        public void AddtionalDamageInBuffState(IFightingObject caster, List<BuffType> buffType, long damage)
        {
            if (isDead) return;

            bool inBuffState = false;
            foreach (var kv in buffType)
            {
                if (InBuffState(kv))
                {
                    inBuffState = true;
                    break;
                }
            }

            if (!inBuffState) return;

            bool immune = false;
            damage = OnHit((FieldObject)caster, DamageType.Skill, damage, ref immune);
            MSG_ZGC_DAMAGE damageMsg = GenerateDamageMsg(InstanceId, DamageType.Skill, damage, immune);
            BroadCast(damageMsg);
        }

        public void AddtionalDamageInmark(IFightingObject caster, int skillLevelGrowth, int markId, long damage)
        {
            if (IsDead) return;

            Mark mark = markManager.GetMark(markId);
            if (mark == null) return;

            bool immune = false;
            damage = damage * skillLevelGrowth * mark.CurCount;
            damage = OnHit((FieldObject)caster, DamageType.Skill, damage, ref immune);

            MSG_ZGC_DAMAGE damageMsg = GenerateDamageMsg(InstanceId, DamageType.Skill, damage);
            BroadCast(damageMsg);
        }

        public void AddtionalHalo(IFightingObject caster, int holaId, int skillLevel)
        {
            DungeonMap dungeon = currentMap as DungeonMap;

            if (dungeon == null) return;

            dungeon.AddHola(this, holaId, skillLevel);
        }

        public void CleanAllBuff()
        {
            buffManager.CleanAllBuff();
        }

        public void CleanAllDebuff()
        {
            buffManager.CleanAllDebuff();
        }

        public void FullUpHp()
        {
            SetNatureBaseValue(NatureType.PRO_HP, GetNatureValue(NatureType.PRO_MAX_HP));
        }

        public void ReplaceSkill(int newSkillId, int oldSkillId)
        {
            skillManager.ReplaceSkillByType(newSkillId, oldSkillId);
        }
    }
}
