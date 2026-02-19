using CommonUtility;

namespace ZoneServerLib
{
    class OnAnySkillDoDamageBeforeTrilnr : BaseTriLnr
    {
        public OnAnySkillDoDamageBeforeTrilnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.AnySkillDoDamageBefore, message);
        }

    }
}

