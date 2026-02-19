using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class TriggerConditionFactory
    {
        public static BaseTriCon CreateTriggerCondition(BaseTrigger trigger, TriggerCondition condition, string conditionParam)
        {
            switch (condition)
            {
                case TriggerCondition.HpRateLess:
                    return new HpRateLessTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HpRateGreater:
                    return new HpRateGreaterTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HeroHpRateGreater:
                    return new HeroHpRateGreaterTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillStart:
                    return new SkillStartTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillEff:
                    return new SkillEfflTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillEnd:
                    return new SkillEndTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillMiss:
                    return new SkillMissPlayerTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillTypeStart:
                    return new SkillTypeStartTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillTypeEnd:
                    return new SkillTypeEndTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillTypeHit:
                    return new SkillTypeHitTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillTypeKillEnemy:
                    return new SkillTypeKillEnemyTriCon(trigger, condition, conditionParam);
                case TriggerCondition.Time:
                    return new TimeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.MonsterAllDead:
                    return new MonsterAllDeadTriCon(trigger, condition, conditionParam);
                case TriggerCondition.MonsterAnyDead:
                    return new MonsterAnyDeadTriCon(trigger, condition, conditionParam);
                case TriggerCondition.PlayerAllDead:
                    return new PlayerAllDead(trigger, condition, conditionParam);
                case TriggerCondition.NoRobotHeroInMap:
                    return new NoRobotHeroInMapCon(trigger, condition, conditionParam);
                case TriggerCondition.NoPlayerHeroInMap:
                    return new NoPlayerHeroInMapCon(trigger, condition, conditionParam);
                case TriggerCondition.PlayerDeadCount:
                    return new PlayerDeadCount(trigger, condition, conditionParam);
                case TriggerCondition.HeroDeadCount:
                    return new HeroDeadCount(trigger, condition, conditionParam);
                case TriggerCondition.BuffEnd_Time:
                    return new BuffEnd_TimeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.BuffEnd_Damage:
                    return new BuffEnd_DamageTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CastBuff:
                    return new CastBuffTriCon(trigger, condition, conditionParam);
         
                case TriggerCondition.DamageTotalGreaterThanHpRate:
                    return new DamageGreaterThanHpRateTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DamageOnceGreaterThanHpRate:
                    return new DamageOnceGTHpRateTriCon(trigger, condition, conditionParam);
                case TriggerCondition.NormalAttKillEnemy:
                    return new NormalAttKillEnemyTriCon(trigger, condition, conditionParam);
                case TriggerCondition.KillEnemyWithCritical:
                    return new KillEnemyWithCriticalTriCon(trigger, condition, conditionParam);
                case TriggerCondition.InRealBody:
                    return new InRealBodyTriCon(trigger, condition, conditionParam);
                case TriggerCondition.OneSkillDamage:
                    return new OneSkillDamageTriCon(trigger, condition, conditionParam);
                case TriggerCondition.MarkEnough:
                    return new MarkEnoughTriCon(trigger, condition, conditionParam);
                case TriggerCondition.InTypeBuffState:
                    return new InBuffStateTriCon(trigger, condition, conditionParam);
                case TriggerCondition.NotifyBattleEndTime:
                    return new BattleEndTriCon(trigger, condition, conditionParam);
                case TriggerCondition.UseSkillAnyTime:
                    return new UseSkillManyTimesTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HeroHpRateLess:
                    return new HeroHpRateLessTriCon(trigger, condition, conditionParam);
                case TriggerCondition.PlayerHpRateLess:
                    return new PlayerHpRateLessTriCon(trigger, condition, conditionParam);
                case TriggerCondition.KillEnemyWithBuffDamage:
                    return new KillEnermyWithBuffDamageTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HeroInBattleGround:
                    return new HeroInBattlegroundTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DodgeSkillType:
                    return new DodgeSkillTypeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.MonsterDead:
                    return new MonsterDeadTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HaveBuff:
                    return new HaveBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.MonsterHpLTHpRatio:
                    return new MonsterHpLTHpRatioTriCon(trigger, condition, conditionParam);
                case TriggerCondition.NormalAttackCritical:
                    return new NormalAttackCriticalTriCon(trigger, condition, conditionParam);
                case TriggerCondition.NormalSkillCritical:
                    return new NormalSkillCriticalTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HitTargetInTypeBuffState:
                    return new HitTargetInBuffStateTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HitTargetHaveBuff:
                    return new HitTargetHaveBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.BuffStartId:
                    return new BuffIdStartTriCon(trigger, condition, conditionParam);
                case TriggerCondition.BuffStartType:
                    return new BuffTypeStartTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HitTargetHaveMark:
                    return new HitTargetHaveMark(trigger, condition, conditionParam);
                case TriggerCondition.NormalAttHitTargetHaveIdBuff:
                    return new NormalAttHitTargetHaveIdBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.NormalAttHitTargetHaveTypeBuff:
                    return new NormalAttHitTargetHaveTypeBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CastTypeBuff:
                    return new CastTypeBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AddMark:
                    return new AddMarkTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DeadFieldObjectInBuffState:
                    return new DeadFieldObjectInBuffStateTriCon(trigger, condition, conditionParam);
                case TriggerCondition.EnemyFieldObjectDead:
                    return new EnemyFieldObjectDeadTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DoDamageTargetNotHaveMark:
                    return new DoDamageTargetNotHaveMarkTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DoDamageTargetInTypeBuffState:
                    return new DoDamageTargetInTypeBuffStateTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DoDamageTargetHaveIdBuff:
                    return new DoDamageTargetHaveIdBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DoDamageTargetInControlledBuff:
                    return new DoDamageTargetInControlledBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DoDamageTargetInDebuff:
                    return new DoDamageTargetInDebuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DoDamageTargetBySkillId:
                    return new DoDamageTargetBySkillIdTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TargetHpRateLess:
                    return new TargetHpRateLessTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TargetHpRateGreater:
                    return new TargetHpRateGreaterTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AllyFieldObjectDead:
                    return new AllyFieldObjectDeadTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CriticalTargetInTypeBuffState:
                    return new CriticalTargetInTypeBuffStateTriCon(trigger, condition, conditionParam);
                case TriggerCondition.OwnerWillDead:
                    return new OwnerWillDeadTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TriggerTriggered:
                    return new TriggerTriggeredTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HaveNotTypeOfBuff:
                    return new HaveNotTypeOfBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillDamageBefore:
                    return new SkillDamageBeforeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TargetHaveBuffId:
                    return new TargetHaveBuffIdTriCon(trigger, condition, conditionParam);
                //case TriggerCondition.DoDamageTargetDead:
                //    return new DoDamageTargetDeadTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HpRateDeclineOnce:
                    return new HpRateDeclineOnceTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillCastCount:
                    return new SkillCastCountTriCon(trigger, condition, conditionParam);
                case TriggerCondition.FullHp:
                    return new FullHpTriCon(trigger, condition, conditionParam);
                case TriggerCondition.HaveBuffList:
                    return new HaveBuffListTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TargetHasDeBuff:
                    return new TargetHasDeBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CasterHpRateGreaterThanTarget:
                    return new CasterHpRateGreaterThanTargetTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CureBuffTargetHpRateLess:
                    return new CureBuffTargetHpRateLessTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CureTargetHpRateLess:
                    return new CureTargetHpRateLessTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TargetHpRateLessBeforeDam:
                    return new TargetHpRateLessBeforeDamTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TargetHpRateGreaterBeforeDam:
                    return new TargetHpRateGreaterBeforeDamTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AllyAliveCountGT:
                    return new AllyAliveCountGTTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AllyAliveCountEQ:
                    return new AllyAliveCountEQTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AllyAliveCountLE:
                    return new AllyAliveCountLETriCon(trigger, condition, conditionParam);
                case TriggerCondition.EnemyAliveCountGT:
                    return new EnemyAliveCountGTTriCon(trigger, condition, conditionParam);
                case TriggerCondition.EnemyAliveCountEQ:
                    return new EnemyAliveCountEQTriCon(trigger, condition, conditionParam);
                case TriggerCondition.EnemyAliveCountLE:
                    return new EnemyAliveCountLETriCon(trigger, condition, conditionParam);
                case TriggerCondition.AfterEffectTime:
                    return new AfterEffectTimeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.BeenCuredCount:
                    return new BeenCuredCounterTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CriticalCount:
                    return new CriticalCounterTriCon(trigger, condition, conditionParam);
                case TriggerCondition.NormalAttackHitCount:
                    return new NormalAttackHitCountTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillTypeHitCount:
                    return new SkillTypeHitCountTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillTypeCastCount:
                    return new SkillTypeCastCountTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AnySkillDamageCount:
                    return new AnySkillDamageCountTriCon(trigger, condition, conditionParam);
                case TriggerCondition.EnemyDeadCount:
                    return new EnemyDeadCountTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AllyDeadCount:
                    return new AllyDeadCountTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DodgeCount:
                    return new DodgeCountTriCon(trigger, condition, conditionParam);
                case TriggerCondition.BeenAddedTypeBuff:
                    return new BeenAddedTypeBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.OwnerHaveTypeBuff:
                    return new OwnerHaveTypeBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TargetHaveTypeBuff:
                    return new TargetHaveTypeBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DeadFieldObjectHaveIdMark:
                    return new DeadFieldObjectHaveIdMarkTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DeadFieldObjectInIdBuff:
                    return new DeadFieldObjectInIdBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.DeadEnemyHaveTypeBuff:
                    return new DeadEnemyHaveTypeBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.IdSkillCritical:
                    return new IdSkillCriticalTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AnySkillDoDamageSkillId:
                    return new AnySkillDoDamageSkillIdTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AllySkillTypeStart:
                    return new AllySkillTypeStartTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AllyCastTypeBuff:
                    return new AllyCastTypeBuffTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CheckTargetInSkillRange:
                    return new CheckTargetInSkillRangeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AllyTypeJobCountEqu:
                    return new AllyTypeJobCountEquTriCon(trigger, condition, conditionParam);
                case TriggerCondition.OwnerDead:
                    return new OwnerDeadTriCon(trigger, condition, conditionParam);
                case TriggerCondition.CureBuffTargetHpRateGT:
                    return new CureBuffTargetHpRateGTTriCon(trigger, condition, conditionParam);
                case TriggerCondition.OnCasterNatureGTOwner:
                    return new OnCasterNatureGTOwnerTriCon(trigger, condition, conditionParam);
                case TriggerCondition.OnCasterNatureLEOwner:
                    return new OnCasterNatureLEOwnerTriCon(trigger, condition, conditionParam);
                case TriggerCondition.TypeSkillDamageBeforeTriCon:
                    return new TypeSkillDamageBeforeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.MarkGT:
                    return new MarkGTTriCon(trigger, condition, conditionParam);
                case TriggerCondition.OwnerHpLessRatio:
                    return new OwnerHpLessRatioTriCon(trigger, condition, conditionParam);
                case TriggerCondition.EnergyChangeTargetType:
                    return new EnergyChangeTargetTypeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.AnySkillDoDamageTargetListBySkillType:
                    return new AnySkillDoDamageTargetListBySkillTypeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.SkillTypeEnergyChange:
                    return new EnergyChangeSkillTypeTriCon(trigger, condition, conditionParam);
                case TriggerCondition.OwnerHpGTRatio:
                    return new OwnerHpGTRatioTriCon(trigger, condition, conditionParam);
                default:
                    Log.Warn("create trigger condition {0} failed: not supported yet", condition);
                    return new BaseTriCon(trigger, condition, conditionParam);
            }
        }
    }
}
