using CommonUtility;

namespace ZoneServerLib
{
    public class OnCuredTriLnr : BaseTriLnr
    {
        public OnCuredTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            base.ParseMessage(message);
            trigger.AddCounter(TriggerCounter.BeenCured);
        }
    }
}

