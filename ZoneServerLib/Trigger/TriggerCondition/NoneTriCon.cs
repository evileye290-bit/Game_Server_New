using System;
using CommonUtility;

namespace ZoneServerLib
{
    public class NoneTriCon : BaseTriCon
    {
        public NoneTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
        }
    }
}
