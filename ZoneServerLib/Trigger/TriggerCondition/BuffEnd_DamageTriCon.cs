using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    // 某个技能起效了
    public class BuffEnd_DamageTriCon : BaseTriCon
    {
        private readonly int buffId;
        public BuffEnd_DamageTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffId))
            {
                Log.Warn($"init cast buff end damage trigger condition failed: invalid buff id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildEndedBuffKey(buffId), out param))
            {
                return false;
            }
            BuffEndTriMsg msg = param as BuffEndTriMsg;
            if (msg == null)
            {
                return false;
            }
            return msg.BuffId == buffId && msg.Reason == BuffEndReason.Damage;
        }
    }
}
