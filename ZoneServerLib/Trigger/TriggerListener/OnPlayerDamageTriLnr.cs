using CommonUtility;

namespace ZoneServerLib
{
    public class OnPlayerDamageTriLnr : BaseTriLnr
    {
        public OnPlayerDamageTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
        }
    }
}
