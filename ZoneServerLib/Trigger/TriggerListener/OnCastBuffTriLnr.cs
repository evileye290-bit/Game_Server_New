using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnCastBuffTriLnr : BaseTriLnr
    {
        public OnCastBuffTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            BaseBuff param = message as BaseBuff;
            if (param != null)
            {
                trigger.RecordParam(TriggerParamKey.BuildCastBuffKey(param.Id), message);
            }
        }

    }
}
