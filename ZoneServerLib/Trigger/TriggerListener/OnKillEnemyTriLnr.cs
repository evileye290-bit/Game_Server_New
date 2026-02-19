using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnKillEnemyTriLnr : BaseTriLnr
    {
        public OnKillEnemyTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.KillEnemy, message);
        }
    }
}
