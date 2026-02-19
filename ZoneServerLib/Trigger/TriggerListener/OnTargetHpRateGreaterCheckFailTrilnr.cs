using CommonUtility;

namespace ZoneServerLib
{
    class OnTargetHpRateGreaterCheckFailTrilnr : BaseTriLnr
    {
        public OnTargetHpRateGreaterCheckFailTrilnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            //trigger.RecordParam(TriggerParamKey.AnySkillDoDamage, message);
        }

    }
}

