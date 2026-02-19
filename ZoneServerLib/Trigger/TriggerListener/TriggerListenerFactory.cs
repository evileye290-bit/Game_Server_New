using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class TriggerListenerFactory
    {
        public static BaseTriLnr CreateTriggerListener(BaseTrigger trigger, TriggerMessageType messageType)
        {
            switch (messageType)
            {
                case TriggerMessageType.None:
                    return new BaseTriLnr(trigger, messageType);
                case TriggerMessageType.AddHp:
                    return new OnAddHpTriLnr(trigger, messageType);
                case TriggerMessageType.SkillStart:
                    return new OnSkillStartTriLnr(trigger, messageType);
                case TriggerMessageType.SkillEffect:
                    return new OnSkillEffTriLnr(trigger, messageType);
                case TriggerMessageType.SkillTypeHit:
                    return new OnSkillTypeHitTriLnr(trigger, messageType);
                case TriggerMessageType.NormalSkillHit:
                    return new OnNormalSkillHitTriLnr(trigger, messageType);
                case TriggerMessageType.NormalAtkHit:
                    return new OnNormalAtkHitTriLnr(trigger, messageType);
                case TriggerMessageType.SkillEnd:
                    return new OnSkillEndTriLnr(trigger, messageType);
                case TriggerMessageType.SkillMissed:
                    return new OnSkillMissTriLnr(trigger, messageType);
                case TriggerMessageType.SkillTypeStart:
                    return new OnSkillTypeStartTriLnr(trigger, messageType);
                case TriggerMessageType.SkillTypeEnd:
                    return new OnSkillTypeEndTriLnr(trigger, messageType);
                case TriggerMessageType.NormalSkillStart:
                    return new OnNormalSkillStartTriLnr(trigger, messageType);
                case TriggerMessageType.KillEnemy:
                    return new OnKillEnemyTriLnr(trigger, messageType);
                case TriggerMessageType.Dead:
                    return new OnDeadTriLnr(trigger, messageType);
                case TriggerMessageType.BuffEnd:
                    return new OnBuffEndTriLnr(trigger, messageType);
                case TriggerMessageType.CastBuff:
                    return new OnCastBuffTriLnr(trigger, messageType);
                case TriggerMessageType.CastTypebuff:
                    return new OnCastBuffTypeTriLnr(trigger, messageType);
                case TriggerMessageType.CastControlledBuff:
                    return new OnCastControlledBuffTriLnr(trigger, messageType);
                case TriggerMessageType.DamageTotal:
                    return new OnDamageTotalTriLnr(trigger, messageType);
                case TriggerMessageType.DamageOnce:
                    return new OnDamageOnceTriLnr(trigger, messageType);
                case TriggerMessageType.RefuseDebuff:
                    return new OnRefuseDebuffTriLnr(trigger, messageType);
                case TriggerMessageType.OneSkillDamage:
                    return new OnOneSkillDamageTriLnr(trigger, messageType);
                case TriggerMessageType.MarkEnough:
                    return new OnMarkEnoughTriLnr(trigger, messageType);
                case TriggerMessageType.BodyDamage:
                    return new OnBodyDamageTriLnr(trigger, messageType);
                case TriggerMessageType.ControlledBuffStart:
                    return new OnContralBuffStartTriLnr(trigger, messageType);
                case TriggerMessageType.SkillDamage:
                    return new OnSkillDamageTriLnr(trigger, messageType);
                case TriggerMessageType.NormalAttackDamage:
                    return new OnNormalAttackDamageTriLnr(trigger, messageType);
                case TriggerMessageType.AnySkillHit:
                    return new OnAnySkillHitTriLnr(trigger, messageType);
                case TriggerMessageType.PlayerDamage:
                    return new OnPlayerDamageTriLnr(trigger, messageType);
                case TriggerMessageType.DodgeSkill:
                    return new DodgeSkillTriLnr(trigger, messageType);
                case TriggerMessageType.HeroFightStart:
                    return new StartFightTriLnr(trigger, messageType);
                case TriggerMessageType.Critical:
                    return new OnCriticalTrilnr(trigger, messageType);
                case TriggerMessageType.Cured:
                    return new OnCuredTriLnr(trigger, messageType);
                case TriggerMessageType.MonsterDamage:
                    return new OnMonsterDamageTotalTriLnr(trigger, messageType);
                case TriggerMessageType.BuffStartId:
                    return new OnBuffIdStartTriLnr(trigger, messageType);
                case TriggerMessageType.BuffStartType:
                    return new OnBuffTypeStartTriLnr(trigger, messageType);
                case TriggerMessageType.ImmuneDead:
                    return new OnImmuneDead(trigger, messageType);
                case TriggerMessageType.AnySkillDamage:
                    return new OnAnySkillDamageTrilnr(trigger, messageType);
                case TriggerMessageType.AnySkillDoDamage:
                    return new OnAnySkillDoDamageTrilnr(trigger, messageType);
                case TriggerMessageType.AnySkillDoDamageBefore:
                    return new OnAnySkillDoDamageBeforeTrilnr(trigger, messageType);
                case TriggerMessageType.AddMark:
                    return new OnAddMarkTriLnr(trigger, messageType);
                case TriggerMessageType.FieldObjectDead:
                    return new OnFieldDeadTriLnr(trigger, messageType);
                case TriggerMessageType.TargetHpRateLessCheckFail:
                    return new OnTargetHpRateLessCheckFailTrilnr(trigger, messageType);
                case TriggerMessageType.TargetHpRateGreaterCheckFail:
                    return new OnTargetHpRateGreaterCheckFailTrilnr(trigger, messageType);
                case TriggerMessageType.TriggerTriggerd:
                    return new OnTriggerTriggeredTriLnr(trigger, messageType);
                case TriggerMessageType.WillDead:
                    return new OnWillDeadTriLnr(trigger, messageType);
                case TriggerMessageType.BuffHappend:
                    return new OnBuffHappendLnr(trigger, messageType);
                case TriggerMessageType.SkillCastCount:
                    return new OnSkillCastCountTriLnr(trigger, messageType);
                case TriggerMessageType.SkillAddCureBuff:
                    return new OnSkillAddCureBuffTriLnr(trigger, messageType);
                case TriggerMessageType.CastDebuff:
                    return new OnCastDebuffTriLnr(trigger, messageType);
                case TriggerMessageType.EnemyDead:
                    return new OnEnemyDeadTriLnr(trigger, messageType);
                case TriggerMessageType.AllyDead:
                    return new OnAllyDeadTriLnr(trigger, messageType);
                case TriggerMessageType.CastCureBuff:
                    return new OnCastCureBuffTriLnr(trigger, messageType);
                case TriggerMessageType.CastSkill:
                    return new OnCastSkillTriLnr(trigger, messageType);
                case TriggerMessageType.GetDeadluHurt:
                    return new OnGetDeadlyHurtTriLnr(trigger, messageType);
                case TriggerMessageType.CastBodySkillLastOne:
                    return new OnCastBodyAttackLastOneTriLnr(trigger, messageType);
                case TriggerMessageType.CureSkill:
                    return new OnCureSkillTriLnr(trigger, messageType);
                case TriggerMessageType.CheckAllyAliveCount:
                    return  new OnCheckAllyAliveCountTrilnr(trigger, messageType);
                case TriggerMessageType.CheckEnemyAliveCount:
                    return  new OnCheckEnemyAliveCountTrilnr(trigger, messageType);
                case TriggerMessageType.CheckTargetInSkillRange:
                    return new OnCheckTargetInSkillRangeTriLnr(trigger, messageType);
                case TriggerMessageType.GetCriticalStrike:
                    return new OnGetCriticalStrikeTriLnr(trigger, messageType);
                case TriggerMessageType.NormalAtkStart:
                    return new OnNormalAttackStartTriLnr(trigger, messageType);
                case TriggerMessageType.BodyHit:
                    return new OnBodyHitTriLnr(trigger, messageType);
                case TriggerMessageType.AnySkillDoDamageTargetList:
                    return new OnAnySkillDoDamageTargetListTrilnr(trigger, messageType);
                case TriggerMessageType.AllySkillTypeStart:
                    return new OnAllySkillTypeStartTriLnr(trigger, messageType);
                case TriggerMessageType.AllyCastTypeBuff:
                    return new OnAllyCastTypeBuffTriLnr(trigger, messageType);
                case TriggerMessageType.BeenAttacked:
                    return  new OnBeenAttackedTriLnr(trigger, messageType);
                case TriggerMessageType.EnergyChange:
                    return new EnergyChangeTriLnr(trigger, messageType);
                default:
                    Log.Warn("create trigger listener {0} failed: not supported yet", messageType);
                    return new BaseTriLnr(trigger, messageType);
            }
        }
    }
}
