using CommonUtility;

namespace ZoneServerLib
{
    class OnAnySkillDamageTrilnr : BaseTriLnr
    {
        public OnAnySkillDamageTrilnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.AnySkillDamage, message);
            trigger.AddCounter(TriggerCounter.AnySkillDamage);
        }

    }
}

