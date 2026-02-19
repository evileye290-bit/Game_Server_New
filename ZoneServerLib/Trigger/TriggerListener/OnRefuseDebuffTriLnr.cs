using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnRefuseDebuffTriLnr : BaseTriLnr
    {
        public OnRefuseDebuffTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.KillEnemy, message);
        }
    }
}
