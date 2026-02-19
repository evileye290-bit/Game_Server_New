using CommonUtility;

namespace ZoneServerLib
{

    public class OnBodyDamageTriLnr : BaseTriLnr
    {
        public OnBodyDamageTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            BodyDamageMsg msg = message as BodyDamageMsg;
            if (msg != null)
            {
                trigger.RecordParam(TriggerParamKey.BodyDamage, msg);
            }
        }
    }
}
