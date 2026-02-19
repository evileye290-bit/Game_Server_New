using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnNormalSkillHitTriLnr : BaseTriLnr
    {
        public OnNormalSkillHitTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.NormalSkillHit, message);
        }

    }
}
