using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnDamageTotalTriLnr : BaseTriLnr
    {
        public OnDamageTotalTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            long damage;
            if(!long.TryParse(message.ToString(), out damage))
            {
                return;
            }

            object totalDamageParam;
            if(!trigger.TryGetParam(TriggerParamKey.TotalDamage, out totalDamageParam))
            {
                trigger.RecordParam(TriggerParamKey.TotalDamage, damage);
                return;
            }

            long totalDamage;
            if (long.TryParse(totalDamageParam.ToString(), out totalDamage))
            {
                totalDamage += damage;
                trigger.RecordParam(TriggerParamKey.TotalDamage, totalDamage);
            }
        }

    }
}
