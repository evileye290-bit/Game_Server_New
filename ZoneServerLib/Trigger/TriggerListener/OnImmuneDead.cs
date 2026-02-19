using CommonUtility;

namespace ZoneServerLib
{
    public class OnImmuneDead : BaseTriLnr
    {
        public OnImmuneDead(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.ImmuneDead, message);
        }
    }
}
