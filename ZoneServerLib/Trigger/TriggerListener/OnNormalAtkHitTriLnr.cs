using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnNormalAtkHitTriLnr : BaseTriLnr
    {
        public OnNormalAtkHitTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.NormalAtkHit, message);
            trigger.AddCounter(TriggerCounter.NormalAttackHit);
        }

    }
}
