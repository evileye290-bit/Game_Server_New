using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnCastControlledBuffTriLnr : BaseTriLnr
    {
        public OnCastControlledBuffTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.CastControlledBuff, message);
        }

    }
}
