using CommonUtility;
using ServerModels;
using ZoneServerLib.Buff;

namespace ZoneServerLib
{
    public class BuffFactory
    {
        public static BaseBuff CreateBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel)
        {
            switch (buffModel.BuffType)
            {
                case BuffType.None:
                    return new DefaultBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Poison:
                    return new PoisonBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Burn:
                    return new BurnBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Bleed:
                    return new BleedBuff(caster, owner, skillLevel, buffModel);
                case BuffType.BleedMore:
                    return new BleedMoreBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Cure:
                    return new CureBuff(caster, owner, skillLevel, buffModel);
                case BuffType.CureOnce:
                    return new CureOnceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.CureSelf:
                    return new CureSelf(caster, owner, skillLevel, buffModel);
                case BuffType.CureSelfOnce:
                    return new CureSelfOnceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Vampire:
                    return new VampireBuff(caster, owner, skillLevel, buffModel);
                case BuffType.RefuseDebuff:
                    return new RefuseDebuffBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Dizzy:
                    return new DizzyBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Silent:
                    return new SilentBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Fixed:
                    return new FixedBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Disarm:
                    return new DisarmBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Shield:
                case BuffType.Shield_Spider:
                case BuffType.Shield_WhiteTiger_Self:
                case BuffType.Shield_WhiteTiger_Ally:
                    return new ShieldBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceDamageRatio:
                    return new ReduceDamageRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Splash:
                    return new SplashBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Invincible:
                    return new InvincibleBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddAttack:
                    return new AddAttackBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddAttackRatio:
                    return new AddAttackRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceAttack:
                    return new ReduceAttackBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceAttackRatio:
                    return new ReduceAttackRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddDefence:
                    return new AddDefenceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddDefenceRatio:
                    return new AddDefenceRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceDefenceRatio:
                    return new ReduceDefenceRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddDefSDam:
                    return new AddDefSDamBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddDefADam:
                    return new AddDefADamBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceDefence:
                    return new ReduceDefenceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddImp:
                    return new AddImpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddImpRatio:
                    return new AddImpRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceImp:
                    return new ReduceImpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddArmour:
                    return new AddArmourBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceArmour:
                    return new ReduceArmourBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceArmourRatio:
                    return new ReduceArmourRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddHit:
                    return new AddHitBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddHitRatio:
                    return new AddHitRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceHit:
                    return new ReduceHitBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceHitRatio:
                    return new ReduceHitRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddFlee:
                    return new AddFleeBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddFleeRatio:
                    return new AddFleeRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceFlee:
                    return new ReduceFleeBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddCritical:
                    return new AddCriticalBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddCriticalRatio:
                    return new AddCriticalRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceCritical:
                    return new ReduceCriticalBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddResistance:
                    return new AddResistanceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceResistance:
                    return new ReduceResistanceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.OnSkillDamageAngry:
                    return new OnSkillDamageAngry(caster, owner, skillLevel, buffModel);
                case BuffType.DeControlledBuff:
                    return new DeControlledBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DeControlledReduceDmg:
                    return new DeControlledReduceDmgBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DeControlledTime:
                    return new DeControlledTimeBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddFleeSkillOnHpGreat:
                    return new AddFleeSkillOnHpGreatBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddFinalFleeRatio:
                    return new AddFinalFleeRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EscapeFromDeath:
                    return new EscapeFromDeath(caster, owner, skillLevel, buffModel);
                case BuffType.ShieldDamageRebound:
                    return new ShieldDamageReboundBuff(caster, owner, skillLevel, buffModel);
                case BuffType.HitNearby:
                    return new HitNearby(caster, owner, skillLevel, buffModel);
                case BuffType.AddBurnMoreDamage:
                    return new AddBurnMoreDamageBuff(caster, owner, skillLevel, buffModel);
                //case BuffType.EnhanceBurnBuff:
                //    return new EnhanceBurnBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DamageMoreToControlled:
                    return new DamageMoreToControlledBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DamageRebound:
                    return new DamageReboundBuff(caster, owner, skillLevel, buffModel);
                case BuffType.IgnoreDebuff:
                    return new IgnoreDebuffBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DisableRealBody:
                    return new DisableRealBodyBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceCure:
                    return new ReduceCureBuff(caster, owner, skillLevel, buffModel);
                case BuffType.FrozenSkill:
                    return new FrozenSkillBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceHP:
                    return new ReduceHpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Freeze:
                    return new FreezeBuff(caster, owner, skillLevel, buffModel);
                case BuffType.StoneShield:
                    return new StoneShield(caster, owner, skillLevel, buffModel);
                case BuffType.NormalAttackDamage:
                    return new NormalAttackDamageBuff(caster, owner, skillLevel, buffModel);
                case BuffType.RefuseControlBuffNature:
                    return new RefuseControlBuffNatureBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddMaxHPRatio:
                    return new AddMaxHPRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceHpOnce:
                    return new ReduceHpOnceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.SkillAttackDamage:
                    return new SkillAttackDamageBuff(caster, owner, skillLevel, buffModel);
                case BuffType.IgnoreAttackLockHP:
                    return new IgnoreAttackLockHPBuff(caster, owner, skillLevel, buffModel);
                //case BuffType.ShieldByOwnerDefence:
                //    return new ShieldByOwnerDefenceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddDamageRatio:
                    return new AddDamageRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.IgnoreSkillDamageAndReboundOnce:
                    return new IgnoreSkillDamageAndReboundOnceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.IgnoreLessDamage:
                    return new IgnoreLessDamageBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DrowHate:
                    return new DrowHateBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddPoisonMoreDamage:
                    return new AddPoisonMoreDamage(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceNormalAttack:
                    return new ReduceNormalAttackBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddResRatio:
                    return new AddResRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddADam:
                    return new AddADamBuf(caster, owner, skillLevel, buffModel);
                case BuffType.PoisonDGB:
                    return new PosionDGBBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddDam:
                    return new AddDamBuf(caster, owner, skillLevel, buffModel);
                case BuffType.RefuseRealBodyEnemy:
                    return new RefuseRealBodyEnemyBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddMulCritical:
                    return new AddMulCriticalRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddAttackWithBuff:
                    return new AddAttackWithBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddBodyEnergyPerTime:
                    return new AddBodyEnergyPerTimeBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DamageByRatioOfLossHp:
                    return new DamageByRatioOfLossHpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddHpRateWithBuff:
                    return new AddHpRateWithBuff(caster, owner, skillLevel, buffModel);
                case BuffType.CureRateSelf:
                    return new CureRateSelf(caster, owner, skillLevel, buffModel);
                case BuffType.BeCuredEnhance:
                    return new AddBeCuredEnhanceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.IgnoreControl:
                    return new IgnoreControlBuff(caster, owner, skillLevel, buffModel);
                case BuffType.IgnoreDamage:
                    return new IgnoreDamageBuff(caster, owner, skillLevel, buffModel);
                case BuffType.RedeuceCureRatio:
                    return new ReduceCureRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddMaxHp:
                    return new AddMaxHpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceDamage:
                    return new ReduceDamageBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddDamageFixed:
                    return new AddDamageFixedBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DamageMoreInControlBuff:
                    return new DamageMoreInControlBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceDamageOnHpLess:
                    return new ReduceDamageOnHpLessBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceDamageOnAllyAllAlive:
                    return new ReduceDamageOnAllyAllAliveBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceDamageOnEnemyAllAlive:
                    return new EnhanceDamageOnEnemyAllAliveBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceCriticalRatioOnHpGreater:
                    return new EnhanceCriticalRatioOnFullHpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.CureRateSelfOnHpLess:
                    return new CureRateSelfOnHpLessBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceCureEffectOnTargetHpLess:
                    return new EnhanceCureEffectOnTargetHpLessBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceDamageRatioOnFullHp:
                    return new EnhanceDamageRatioOnFullHpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddCriticalRate:
                    return new AddCriticalRateBuff(caster, owner, skillLevel, buffModel);
                case BuffType.CureByDamageRatio:
                    return new CureByDamageRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddBleedMoreDamage:
                    return new AddBleedMoreDamageBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnchangeCure:
                    return new EnchanceCureBuff(caster, owner, skillLevel, buffModel);
                case BuffType.Sneer:
                    return new SneerBuff(caster, owner, skillLevel, buffModel);
                case BuffType.NormalSkill1RefuseAddEnergy:
                    return new NormalSkill1RefuseAddEnergyBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddNormalSkill1Energy:
                    return new AddNormalSkill1EnergyBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceBeenCureOnHpLess:
                    return new EnhanceBeenCureOnHpLessBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceFixedCure:
                    return new EnhanceFixedCureBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceAtkOnHpRatioGT:
                    return new EnhanceAtkOnHpRatioGTBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceAtkByEnemyCount:
                    return new EnhanceAtkByEnemyCountBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddNatureRatioWhileHaveNotDebuff:
                    return new AddNatureRatioWhileHaveNotDebuffBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddCasterNatureRatioExtraDamage:
                    return new AddCasterNatureRatioExtraDamageBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddDamageValue:
                    return new AddADamageValueBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddNatureRatioByNatureOnFullHp:
                    return new AddNatureRatioByNatureOnFullHpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddHpRateWithCasterMaxHpRatioOnInBuff:
                    return new AddHpRateWithCasterMaxHpRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.SelfBurnMore:
                    return new SelfBurnMoreBuff(caster, owner, skillLevel, buffModel);
                case BuffType.SelfBleedMore:
                    return new SelfBleedMoreBuff(caster, owner, skillLevel, buffModel);
                case BuffType.SelfPositionMore:
                    return new SelfPoisionMoreBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceDefenceRatioByAllyCount:
                    return new EnhanceDefenceRatioByAllyCountBuff(caster, owner, skillLevel, buffModel);
                case BuffType.RedeuceControlBuffTime:
                    return new RedeuceControlBuffTimeBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceAtkByAllyCount:
                    return new EnhanceAtkByAllyCountBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddHpByRatioOfTotalDamage:
                    return new AddHpByRatioOfTotalDamageBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddNatureRatioByTypeBuffCount:
                    return new AddNatureRatioByTypeBuffCountBuff(caster, owner, skillLevel, buffModel);
                case BuffType.DamageOnce:
                    return new DamageOnceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.SelfPositionReduce:
                    return new SelfPositionReduceBuff(caster, owner, skillLevel, buffModel);
                case BuffType.RefuseCastSkillWithDomain:
                    return new RefuseCastSkillWithDomainBuff(caster, owner, skillLevel, buffModel);
                case BuffType.NextAttackCriticalPerNSec:
                    return  new  NextAttackCriticalPerNSecBuff(caster, owner, skillLevel, buffModel);
                case  BuffType.VampireOnHpLess:
                    return new VampireOnHpLessBuff(caster, owner, skillLevel, buffModel);
                case BuffType.BeCuredEnhanceOnTargetHpLess:
                    return new BeCuredEnhanceOnHpLessBuff(caster, owner, skillLevel, buffModel);
                case BuffType.IgnoreAttackLockHPSomeTime:
                    return  new IgnoreAttackLockHPSomeTimeBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddReduceDamageNatureOnHpLessPerRatio:
                    return  new AddReduceDamageNatureOnHpLessPerRatioBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceAtkRatioOnHpRatioGT:
                    return new EnhanceAtkRatioOnHpRatioGTBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceCureEffect:
                    return new EnhanceCureEffectBuff(caster, owner, skillLevel, buffModel);
                case BuffType.EnhanceShieldHp:
                    return new EnhanceShieldHpBuff(caster, owner, skillLevel, buffModel);
                case BuffType.AddHitOnSkillMiss:
                    return new AddHitOnSkillMissBuff(caster, owner, skillLevel, buffModel);
                case BuffType.ReduceBodyEnergyPerTime:
                    return new ReduceBodyEnergyPerTimeBuff(caster, owner, skillLevel, buffModel);
                case BuffType.BleedAndReduceAttack:
                    return new BleedAndReduceAttackBuff(caster, owner, skillLevel, buffModel);
                default:
                    Logger.Log.Warn("create buff type {0} failed: not supported yet", buffModel.BuffType);
                    return new BaseBuff(caster, owner, skillLevel, buffModel);
            }
        }
    }
}
