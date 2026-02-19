using CommonUtility;
using System.Collections.Generic;

namespace ZoneServerLib
{
    //伙伴达到次数
    public class HeroDeadCount : BaseTriCon
    {
        readonly int deadCount;
        public HeroDeadCount(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            deadCount = int.Parse(conditionParam);
        }

        public override bool Check()
        {
            if (trigger.CurMap == null)
            {
                return false;
            }
            int count = trigger.GetParam_HeroDeadCount();
            return deadCount >= count;
        }
    }
}
