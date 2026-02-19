using CommonUtility;

namespace ZoneServerLib
{
    class OnAnySkillDoDamageTargetListTrilnr : BaseTriLnr
    {
        public OnAnySkillDoDamageTargetListTrilnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.AnySkillDoDamageTargetList, message);
        }

    }
}

