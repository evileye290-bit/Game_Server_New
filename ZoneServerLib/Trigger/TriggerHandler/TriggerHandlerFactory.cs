using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class TriggerHandlerFactory
    {
        public static BaseTriHdl CreateTriggerHandler(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
        {
            switch (handlerType)
            {
                case TriggerHandlerType.OwnerAddBuff:
                    return new OwnerAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.OwnerRemoveBuff:
                    return new OwnerRemoveBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillReady:
                    return new SkillReadyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DungeonSuccess:
                    return new DungeonSuccessTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DungeonFailed:
                    return new DungeonFailedTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DeadMsgToMonster:
                    return new DeadMsgToMonsterTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.MonsterGen:
                    return new MonsterGenTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.OwnerAddDelayBuff:
                    return new OwnerAddBuffDelayTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CleanAllDebuff:
                    return new CleanAllDebuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillTypeHitTargetAddBuff:
                    return new SkillTypeHitTargetAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillTypeHitCasterAddBuff:
                    return new SkillTypeHitCasterAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillTypeHitTargetDamage:
                    return new SkillTypeHitTargetDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.NormalAtkHitTargetAddBuff:
                    return new NormalAtkHitTargetAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillTypeHitTargetRemoveDebuff:
                    return new SkillTypeHitTargetRemoveDebuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddRandomSkillEnergy:
                    return new AddRandomSkillEnergyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddSkillEnergy:
                    return new AddSkillEnergyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddHpRate:
                    return new AddHpRateTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddHp:
                    return new AddHpTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CasterOwnerAddBuff:
                    return new CasterOwnerAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.HeroAddHpRate:
                    return new HeroAddHpRateTrlHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.HeroAddRandomSkillEnergy:
                    return new HeroAddRandomSkillEnergyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnoughSkillTypeEnergy:
                    return new EnoughSkillTypeEnergyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RangeEnemyAddBuff:
                    return new RangeEnemyAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceCastedBuffTime:
                    return new EnhanceCastedBuffTimeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceCastedBuffDamage:
                    return new EnhanceCastedBuffDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceCastedBuffCure:
                    return new EnhanceCastedBuffCureTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddSkillEffect:
                    return new AddSkillEffectTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ChangeModelRadius:
                    return new ChangeModelRadiusTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SwitchHpRate:
                    return new SwitchHpRateTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.NormalSkillAddEnergy:
                    return new NormalSkillAddEnergyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EndKillFSM:
                    return new EndSkillFSM(trigger, handlerType, handlerParam);
                case TriggerHandlerType.BodyDamageAddMark:
                    return new BodyDamageAddMarkTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RemoveMark:
                    return new RemoveMarkTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.OwnerDamage:
                    return new OwnerDoDamage(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RemoveSkill:
                    return new RemoveSkillTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.NotifyBattleEndTime:
                    return new NotifyBattleEndTimeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.MonsterGenCountEqualPlayerCount:
                    return new MonsterGenCountEqPlayerCountTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.TriggerOwnerDie:
                    return new TriggerOwnerDieTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.MonsterGenWithOtherPosition:
                    return new MonsterGenWithOwnerPositionTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.BattleStage:
                    return new BattleStageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkilDamageCureSelf:
                    return new SkillDamageCureSelfTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.NormalAttackThorns:
                    return new NormalAttackThornsTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.KillSkillHitTargetOnHpLessRate :
                    return new KillSkillHitTargetOnHpLessRateTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RemoveRandomDebuff:
                    return new RemoveRandomDebuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RemoveRandomBuff:
                    return new RemoveRandomBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DamageCasterAddBuff:
                    return new DamageCasterAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddMark:
                    return new AddMarkTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CureBySkillDamage:
                    return new CureBySkillDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceDebuffBuffTime:
                    return new EnhanceDebuffBuffTimeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ReduceHpRateRange:
                    return new ReduceHpRateRangeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddHpWithHPRatio:
                    return new AddHPWithHPRatioTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.OwnerDoRandomDamage:
                    return new OwnerDoRandomDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ReduceCurrHpRate:
                    return new ReduceCurrHpRateTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceBuffBuffTime:
                    return new EnhanceBuffTimeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddBodyEnergy:
                    return new AddBodyEnergyTrlHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ReduceMarkCount:
                    return new ReduceMarkNumTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ReplaceMonster:
                    return new RepleaseMonsterTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CriticalTargetAddBuff:
                    return new CriticalTargetAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CastTypeBuffOwnerAddBuff:
                    return new CastTypeBuffOwnerAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkilDamageCureSelf:
                    return new AnySkilDamageCureSelfTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkilDoDamageCureSelf:
                    return new AnySkilDoDamageCureSelfTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkilDoDamageBeforeAddDamageOnec:
                    return new AnySkilDoDamageBeforeAddDamageOnecTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkilDamageTargetAddBuff:
                    return new AnySkilDamageTargetAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkilDamageTargetAddMark:
                    return new AnySkilDamageTargetAddMarkTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceTypeBuffTime:
                    return new EnhanceTypeBuffTimeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DeadFieldObjectRangeEnemyDoDamage:
                    return new DeadFieldObjectRangeEnemyDoDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.MoveKilleEnemyTypeBuffToRangeEnemy:
                    return new MoveKilledEnemyTypeBuffToRangeEnemyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DeadFieldObjectRangeEnemyAddBuff:
                    return new DeadFieldObjectRangeEnemyAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CriticalTargetReduceBodyEnergy:
                    return new CriticalTargetReduceBodyEnergyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CriticalTargetEnhanceTypeBuffTime:
                    return new CriticalTargetEnhanceTypeBuffTimeBuff(trigger, handlerType, handlerParam);
                case TriggerHandlerType.NextHitCritical:
                    return new NextHitCriticalTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RangeEnemyDamage:
                    return new RangeEnemyDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillTypeHitTargetLessHpAddBuff:
                    return new SkillTypeHitTargetLessHpAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillTypeHitTargetDamageByOwnerMarkCount:
                    return new SkillTypeHitTargetDamageByOwnerMarkCountTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CureByOwnerMarkCount:
                    return new CureByMarkCountTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.FleeSkilOwnerDoDamage:
                    return new FleeSkilOwnerDoDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CriticalTargetAddBuffOverlay:
                    return new CriticalTargetAddBuffOverlayTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.TriggerTriggeredMsg:
                    return new TriggerTriggeredMsgTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.OtherAllyRemoveRandomDebuff:
                    return new OtherAllyRemoveRandomDebuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DamageCasterDoDamage:
                    return new DamageCasterDoDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddSkillEnergy4LowerEnergy:
                    return new AddSkillEnergy4LowerEnergyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AllyInRangeAddBuff:
                    return new AllyAddBuffTriHdl(trigger,handlerType,handlerParam);
                case TriggerHandlerType.SkillTypeHitTargetAddTrigger:
                    return new SkillTypeHitTargetAddTriggerTriHdl(trigger,handlerType,handlerParam);
                case TriggerHandlerType.AddTrigger:
                    return new AddTriggerTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddTrigger4Ally:
                    return new AddTrigger4AllyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddTrigger4Enemy:
                    return new AddTrigger4EnemyTriHdl(trigger, handlerType, handlerParam);
                //case TriggerHandlerType.CastSkillThenDead:
                //    return new CastSkillThenDeadTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.FleeNextDamage:
                    return new FleeNextDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceTargetDebuffBuffTime:
                    return new EnhanceTargetDebuffBuffTimeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.HeroAddBuff:
                    return new HeroAddBuffTrlHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.MonsterGenWithKillPosition:
                    return new MonsterGenWithKillPositionTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkillDoDamageAddExtraDamage:
                    return new AnySkillDoDamageAddExtraDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CureTargetDoExtraCure:
                    return new CureTargetDoExtraCureTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AllAllyAddBuff:
                    return new AllAllyAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CriticalTargetDoExtraDamage:
                    return new CriticalTargetDoExtraDamageTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ExtraRatioCureOnce:
                    return new ExtraRatioCureOnceTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CureTargetAddBuff:
                    return new CureTargetAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ControlTargetAddBuff:
                    return new ControlTargetAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ExtendDebuffTime:
                    return new ExtendDebuffTimeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.GetDeadlyHurtAddBuff:
                    return new GetDeadlyHurtAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.KillSAnySkillDoDamageTargetOnHpLessRate:
                    return new KillSAnySkillDoDamageTargetOnHpLessRateTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.Flash2EnemyBack:
                    return new Flash2EnemyBackTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceCastedBuffParamC:
                    return new EnhanceCastedBuffParamCTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CriticalDamageCureRatio:
                    return new CriticalDamageCureRatioTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddBodyEnergyOnlyForNormalSKill1:
                    return new AddBodyEnergyOnlyForNormalSKill1(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ChangeSkillPositionFilterType:
                    return new ChangeSkillPositionFilterTypeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DomainEffect:
                    return new DomainEffectTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RandomGetBuff:
                    return new RandomGetBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.TargetInSkillRangeMoreDamageGreater:
                    return new TargetInSkillRangeMoreDamageGreaterTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.NormalAtkWithNatureTypeRatioDam:
                    return new NormalAtkWithNatureTypeRatioDamTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SelfNaureEnhance:
                    return new SelfNaureEnhanceTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.CureMaxHpRatioByDebuffCount:
                    return new CureMaxHpRatioByDebuffCountTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceSkillEffectModelT:
                    return new EnhanceSkillEffectModelTTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceIdSkillDamageRatio:
                    return new EnhanceIdSkillDamageRatioTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RangeEnemyDoExtraDamageByPoisonBuffCount:
                    return new RangeEnemyDoExtraDamageByPoisonBuffCountTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DoExtraDamageRatio2JobTypes:
                    return new DoExtraDamageRatio2JobTypesTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ClearCriticalTargetDispelBuff:
                    return new ClearCriticalTargetDispelBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ClearCureTargetDebuff:
                    return new ClearCureTargetDebuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkillDoDamageAddTypeSkillEnergyTargetCount:
                    return new AnySkillDoDamageAddTypeSkillEnergyTargetCountTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkilDamageCasterAddBuff:
                    return new AnySkilDamageCasterAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddBuffWithCasterIsKilledEmemy:
                    return new AddBuffWithCasterIsKilledEmemy(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillDoExtraDamageByNatureRatio:
                    return new SkillDoExtraDamageByNatureRatioTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddBuffPileCount:
                    return new AddBuffPileCountTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.TargetInSkillRangeAddBuff:
                    return new TargetInSkillRangeAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AtkHightestAllyAddBodyEnergy:
                    return new AtkHightestAllyAddBodyEnergyTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.DoExtraDamageByPoisonBuffCountTriHdl:
                    return  new DoExtraDamageByPoisonBuffCountTriHdl(trigger, handlerType, handlerParam);
                case  TriggerHandlerType.EnhanceCastedControlBuffTime:
                    return  new EnhanceCastedControlBuffTimeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RangeEnemyDamageByNature:
                    return new RangeEnemyDamageByNatureTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.OwnerReplaceTriggerCreateBySoulRing:
                    return new OwnerReplaceTriggerCreateBySoulRingTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.MonsterGenInRange:
                    return new MonsterGenInRangeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.RandomCountEnemyAddBuff:
                    return new RandomCountEnemyAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.EnhanceIdTypeSkillDamageRatio:
                    return new EnhanceIdTypeSkillDamageRatioTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillReadyOnDefHighestAllyFlee:
                    return  new SkillReadyOnDefHighestAllyFleeTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.UpBuffPileLimit:
                    return new UpBuffPileLimitTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.UpMarkLimit:
                    return new UpMarkLimitTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.SkillEffectMustHit:
                    return new SkillEffectMustHitTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AllyHeroAddBuff:
                    return new AllyHeroAddBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.ChangeBuffNatureRatioOnCastBuff:
                    return new ChangeBuffNatureRatioOnCastBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.OwnerRemoveTypeBuff:
                    return new OwnerRemoveTypeBuffTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AnySkillDoDamageBeforeEnhanceDamageOnce:
                    return new AnySkillDoDamageBeforeEnhanceDamageOnceTriHdl(trigger, handlerType, handlerParam);
                case TriggerHandlerType.AddBuffOnDefHighestAllyFlee:
                    return new AddBuffOnDefHighestAllyFleeTriHdl(trigger, handlerType, handlerParam);
                default:
                    Log.Warn("create trigger handler {0} failed: not supported yet", handlerType);
                    return new BaseTriHdl(trigger, handlerType, handlerParam);
            }
        }
    }
}
