using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    class FleeSkilOwnerDoDamageTriHdl : BaseTriHdl
    {
        readonly int damage;
        public FleeSkilOwnerDoDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            var growth = StringSplit.ParseToFloatPair(handlerParam);
            damage = (int)trigger.CalcParam(TriggerHandlerType.FleeSkilOwnerDoDamage, growth, skillLevelGrowth);
        }

        public override void Handle()
        {
            object obj;
            trigger.TryGetParam(TriggerParamKey.DodgeSkill, out obj);

            Skill skill = obj as Skill;
            if (skill == null) return;

            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            if (skill.Owner != null && skill.Owner as Pet != null)return;
            
            skill.Owner?.DoSpecDamage(trigger.Caster, DamageType.Extra, damage);
        }
    }
}
