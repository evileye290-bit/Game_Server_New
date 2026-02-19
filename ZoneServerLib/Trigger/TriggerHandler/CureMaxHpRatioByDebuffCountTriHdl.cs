using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class CureMaxHpRatioByDebuffCountTriHdl : BaseTriHdl
    {
        readonly int ratio;      

        public CureMaxHpRatioByDebuffCountTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out ratio))
            {
                Log.WarnLine($"CureMaxHpRatioByDebuffCountTriHdl param {handlerParam}");
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param) && !trigger.TryGetParam(TriggerParamKey.OnceDamage, out param))
            {
                return;
            }

            if (trigger.Owner == null || trigger.Owner.BuffManager == null) return;

            int count = trigger.Owner.BuffManager.GetCanCleanUpDebuffCount();
            long hp = (long)(trigger.Owner.GetMaxHp() * (count * ratio * 0.0001f));

            Owner.AddHp(Owner, hp);
            SetThisFspHandled();
        }
    }
}

