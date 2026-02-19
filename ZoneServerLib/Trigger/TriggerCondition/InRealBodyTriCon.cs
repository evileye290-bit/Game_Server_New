using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    // 释放某个buff
    public class InRealBodyTriCon : BaseTriCon
    {
        private readonly int buffId;
        public InRealBodyTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            
        }

        public override bool Check()
        {
            return owner.InRealBody;
        }
    }
}
