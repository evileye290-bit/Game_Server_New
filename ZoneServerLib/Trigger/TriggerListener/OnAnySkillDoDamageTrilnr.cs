using CommonUtility;

namespace ZoneServerLib
{
    class OnAnySkillDoDamageTrilnr : BaseTriLnr
    {
        public OnAnySkillDoDamageTrilnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.AnySkillDoDamage, message);
        }

    }
}

