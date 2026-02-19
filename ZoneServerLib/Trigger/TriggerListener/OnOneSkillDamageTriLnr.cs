using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnOneSkillDamageTriLnr : BaseTriLnr
    {
        public OnOneSkillDamageTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            if (message is DoDamageTriMsg)
            {
                trigger.RecordParam(TriggerParamKey.BuildOneSkillDamageKey((message as DoDamageTriMsg).SkillId), message);
            }
        }

    }
}
