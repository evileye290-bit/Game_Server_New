using CommonUtility;

namespace ZoneServerLib
{
    public class OnAllyDeadTriLnr : BaseTriLnr
    {
        public OnAllyDeadTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.AddCounter(TriggerCounter.AllyDead);
        }
    }
}
