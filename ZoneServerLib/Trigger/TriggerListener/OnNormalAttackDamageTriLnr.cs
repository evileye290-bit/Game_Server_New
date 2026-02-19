using CommonUtility;

namespace ZoneServerLib
{
    class OnNormalAttackDamageTriLnr : BaseTriLnr
    {
        public OnNormalAttackDamageTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.NormalAttackDamage, message);
        }

    }
}
