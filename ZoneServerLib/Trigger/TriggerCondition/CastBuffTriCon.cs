using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    // 释放某个buff
    public class CastBuffTriCon : BaseTriCon
    {
        private readonly int buffId;
        public CastBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffId))
            {
                Log.Warn($"init cast buff trigger condition failed: invalid buff id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildCastBuffKey(buffId), out param))
            {
                return false;
            }
            BaseBuff buff = param as BaseBuff;
            if (buff == null)
            {
                return false;
            }
            return buff.Id == buffId;
        }
    }
}
