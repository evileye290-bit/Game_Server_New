using CommonUtility;
using System.Collections.Generic;

namespace ZoneServerLib
{
    //玩家死亡达到次数
    public class PlayerDeadCount : BaseTriCon
    {
        readonly int deadCount;
        public PlayerDeadCount(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
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
            int count = trigger.GetParam_PlayerDeadCount();
            return count >= deadCount;
        }
    }
}
