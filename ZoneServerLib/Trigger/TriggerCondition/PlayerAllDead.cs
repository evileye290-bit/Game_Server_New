using CommonUtility;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class PlayerAllDead : BaseTriCon
    {
        public PlayerAllDead(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
        }

        public override bool Check()
        {
            if (trigger.CurMap == null)
            {
                return false;
            }

            var playersInMap = trigger.CurMap.PcList;
            foreach (var item in playersInMap)
            {
                if (!item.Value.IsDead)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
