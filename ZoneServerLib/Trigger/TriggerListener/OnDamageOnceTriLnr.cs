using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnDamageOnceTriLnr : BaseTriLnr
    {
        public OnDamageOnceTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.OnceDamage, message);
        }

    }
}
