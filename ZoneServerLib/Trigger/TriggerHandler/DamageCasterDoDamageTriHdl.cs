using CommonUtility;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class DamageCasterDoDamageTriHdl : BaseTriHdl
    {
        //private readonly int damage;
        private readonly KeyValuePair<float, float> growth;

        public DamageCasterDoDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            growth = StringSplit.ParseToFloatPair(handlerParam);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.OnceDamage, out param)) return;

            DamageTriMsg msg = param as DamageTriMsg;
            if (msg == null || msg.Caster == null) return;

            int damage = (int)trigger.CalcParam(TriggerHandlerType.DamageCasterDoDamage, growth, trigger.GetFixedParam_SkillLevelGrowth());

            msg.Caster.DoSpecDamage(trigger.Caster, DamageType.Extra, damage);
            SetThisFspHandled();
        }
    }
}
