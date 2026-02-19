using CommonUtility;
using EnumerateUtility;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class FieldObject
    {
        public void DispatchSkillEffMsg(int skillId)
        {
            if (SubcribedMessage(TriggerMessageType.SkillEffect))
            {
                DispatchMessage(TriggerMessageType.SkillEffect, skillId);
            }
        }

        public void DispatchSkillStartMsg(SkillModel skillModel, int targetId)
        {
            if (SubcribedMessage(TriggerMessageType.SkillStart))
            {
                DispatchMessage(TriggerMessageType.SkillStart, skillModel.Id);
            }

            if (SubcribedMessage(TriggerMessageType.SkillTypeStart))
            {
                DispatchMessage(TriggerMessageType.SkillTypeStart, (int)skillModel.Type);
            }

            if(skillModel.IsNormalSkill())
            {
                if (SubcribedMessage(TriggerMessageType.NormalSkillStart))
                { 
                    DispatchMessage(TriggerMessageType.NormalSkillStart, skillModel.Id);
                }

                DispatchAllySkillTypeStartMessage(skillModel);
            }

            if (skillModel.IsNormalAttack() && SubcribedMessage(TriggerMessageType.NormalAtkStart))
            {
                DispatchMessage(TriggerMessageType.NormalAtkStart, targetId);
            }
        }     

        public void DispatchKillEnemyMsg(int killerInstanceId, int skillId, int deadInstanceId, bool critical, DamageType damageType, object param = null)
        {
            if (SubcribedMessage(TriggerMessageType.KillEnemy))
            {
                DispatchMessage(TriggerMessageType.KillEnemy, new KillEnemyTriMsg(killerInstanceId, skillId, deadInstanceId, critical, damageType, param));
            }
        }

        public void DispatchSkillHitTargetMsg(SkillModel skillModel, List<FieldObject> targetList)
        {
            // 对skill effect target做额外逻辑处理
            if(targetList.Count == 0)
            {
                return;
            }
            if (SubcribedMessage(TriggerMessageType.AnySkillHit))
            {
                DispatchMessage(TriggerMessageType.AnySkillHit, new SkillHitMsg(skillModel, targetList));
            }
            if (SubcribedMessage(TriggerMessageType.SkillTypeHit))
            {
                DispatchMessage(TriggerMessageType.SkillTypeHit, new SkillHitMsg(skillModel, targetList));
            }
            if(skillModel.IsNormalSkill() && SubcribedMessage(TriggerMessageType.NormalSkillHit))
            {
                DispatchMessage(TriggerMessageType.NormalSkillHit, new SkillHitMsg(skillModel, targetList));
            }
            if (skillModel.IsNormalAttack() && SubcribedMessage(TriggerMessageType.NormalAtkHit))
            {
                DispatchMessage(TriggerMessageType.NormalAtkHit, new SkillHitMsg(skillModel, targetList));
            }
            if (skillModel.IsRealBodySkill() && SubcribedMessage(TriggerMessageType.BodyHit))
            {
                DispatchMessage(TriggerMessageType.BodyHit, new SkillHitMsg(skillModel, targetList));
            }          
        }

        public void DispatchShieldDamageMsg(FieldObject caster, long damage)
        {
            if (SubcribedMessage(TriggerMessageType.ShieldDamage))
            {
                ShieldDamageTriMsg msg = new ShieldDamageTriMsg(caster, damage);
                DispatchMessage(TriggerMessageType.ShieldDamage, msg);
            }
        }

        public void DispatchShieldBreakUpMsg()
        {
            if(SubcribedMessage(TriggerMessageType.ShieldBreakUp))
            {
                DispatchMessage(TriggerMessageType.ShieldBreakUp, null);
            }
        }

        public void DispatchRefuseDebuffMsg()
        {
            if (SubcribedMessage(TriggerMessageType.RefuseDebuff))
            {
                // 目前不需要参数
                DispatchMessage(TriggerMessageType.RefuseDebuff, null);
            }
        }

        public void DispatchOneSkilDoDamageMsg(int skillId, long damage)
        {
            if (damage <= 0)
            {
                return;
            }

            if (SubcribedMessage(TriggerMessageType.OneSkillDamage))
            {
                DispatchMessage(TriggerMessageType.OneSkillDamage, new DoDamageTriMsg(damage, skillId, null));
            }
        }

        public void DispatchAnySkilDoDamageMsg(int skillId, long damage, FieldObject field)
        {
            if (damage <= 0)
            {
                return;
            }

            if (SubcribedMessage(TriggerMessageType.AnySkillDoDamage))
            {
                DispatchMessage(TriggerMessageType.AnySkillDoDamage, new DoDamageTriMsg(damage, skillId, field));
            }
        }

        public void DispatchAnySkilDoDamageTargetListMsg(Skill skill, List<FieldObject> fieldes)
        {
            if (SubcribedMessage(TriggerMessageType.AnySkillDoDamageTargetList))
            {
                DispatchMessage(TriggerMessageType.AnySkillDoDamageTargetList, new DoDamageTargetListTriMsg(skill, fieldes));
            }
        }

        public void DispatchHeroDoDamageBeforeStartFightMsg(int skillId, FieldObject field)
        {
            if (SubcribedMessage(TriggerMessageType.AnySkillDoDamageBefore))
            {
                DispatchMessage(TriggerMessageType.AnySkillDoDamageBefore, new DoDamageTriMsg(0, skillId, field));
            }
        }

        public void DispatchHeroStartFightMsg(int heroId)
        {
            if (SubcribedMessage(TriggerMessageType.HeroFightStart))
            {
                DispatchMessage(TriggerMessageType.HeroFightStart, heroId);
            }
        }

        public void DispatchCuredMsg()
        {
            if (SubcribedMessage(TriggerMessageType.Cured))
            {
                DispatchMessage(TriggerMessageType.Cured, null);
            }
        }

        public void DisPatchSkillCastCountMsg(Skill skill)
        {
            if (SubcribedMessage(TriggerMessageType.SkillCastCount))
            {
                DispatchMessage(TriggerMessageType.SkillCastCount, skill);
            }
        }

        public void DispatchSkillAddCureBuffMsg(FieldObject target)
        {
            if (SubcribedMessage(TriggerMessageType.SkillAddCureBuff))
            {
                DispatchMessage(TriggerMessageType.SkillAddCureBuff, target);
            }
        }   

        public void DispathCastSkillMsg()
        {
            if (SubcribedMessage(TriggerMessageType.CastSkill))
            {
                DispatchMessage(TriggerMessageType.CastSkill, null);
            }
        }

        public void DispathCastLastBodyAttackMsg()
        {
            if (SubcribedMessage(TriggerMessageType.CastBodySkillLastOne))
            {
                DispatchMessage(TriggerMessageType.CastBodySkillLastOne, null);
            }
        }

        public void DispatchCastCureBuffMsg(FieldObject caster)
        {
            if (caster != null && caster.SubcribedMessage(TriggerMessageType.CastCureBuff))
            {
                caster.DispatchMessage(TriggerMessageType.CastCureBuff, this);
            }
        }

        public void DispatchCastCureSkillMsg(List<FieldObject> targetList)
        {
            if (SubcribedMessage(TriggerMessageType.CureSkill))
            {
                DispatchMessage(TriggerMessageType.CureSkill, targetList);
            }
        }

        public void DispatchFieldObjectDeadMessage(FieldObject fieldObject)
        {
            if (messageDispatcher?.Subscribed(TriggerMessageType.FieldObjectDead) == true)
            {
                messageDispatcher.Dispatch(TriggerMessageType.FieldObjectDead, fieldObject);
            }
            if (fieldObject != null && fieldObject.IsEnemy(this) && messageDispatcher?.Subscribed(TriggerMessageType.EnemyDead) == true)
            {
                messageDispatcher.Dispatch(TriggerMessageType.EnemyDead, fieldObject);
            }
            if (fieldObject != null && fieldObject.IsAlly(this) && messageDispatcher?.Subscribed(TriggerMessageType.AllyDead) == true)
            {
                messageDispatcher.Dispatch(TriggerMessageType.AllyDead, fieldObject);
            }

            //检测存货单位
            DispatchAliveCountMessage();
        }

        public void DispatchAliveCountMessage()
        {
            if (messageDispatcher == null) return;

            if (messageDispatcher.Subscribed(TriggerMessageType.CheckAllyAliveCount))
            { 
                messageDispatcher.Dispatch(TriggerMessageType.CheckAllyAliveCount, null);
            }

            if (messageDispatcher.Subscribed(TriggerMessageType.CheckEnemyAliveCount))
            {
                messageDispatcher.Dispatch(TriggerMessageType.CheckEnemyAliveCount, null);
            }
        }

        public void DispatchTargetsInSkillRange(List<FieldObject> target, int skillId)
        {
            if (SubcribedMessage(TriggerMessageType.CheckTargetInSkillRange))
            {
                DispatchMessage(TriggerMessageType.CheckTargetInSkillRange, new TargetInSkillRangeMsg(target, skillId));
            }
        }

        public void DispatchAllySkillTypeStartMessage(SkillModel model)
        {
            CurDungeon?.DispatchBridgeTriggerMessage(this, TriggerMessageType.AllySkillTypeStart, new SkillStartMsg(model, this));
        }

        //被攻击
        private void DispatchTargetBeenAttacked(List<FieldObject> target, FieldObject caster)
        {
            foreach (var field in target)
            {
                if (field.SubcribedMessage(TriggerMessageType.BeenAttacked))
                {
                    field.DispatchMessage(TriggerMessageType.BeenAttacked, caster);
                }
            }

        }
    }
}
