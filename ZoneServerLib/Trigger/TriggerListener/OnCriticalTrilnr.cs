using CommonUtility;

namespace ZoneServerLib
{
    public class OnCriticalTrilnr : BaseTriLnr
    {
        public OnCriticalTrilnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            CriticalTriMsg param = message as CriticalTriMsg;
            if (param == null)
            {
                return;
            }

            trigger.AddCounter(TriggerCounter.Critical);
            trigger.RecordParam(TriggerParamKey.Critical, param);
        }
    }
}
