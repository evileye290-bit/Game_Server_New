using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnBuffEndTriLnr : BaseTriLnr
    {
        public OnBuffEndTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            BuffEndTriMsg param = message as BuffEndTriMsg;
            if(param != null)
            {
                trigger.RecordParam(TriggerParamKey.BuildEndedBuffKey(param.BuffId), message);
            }
        }

    }
}
