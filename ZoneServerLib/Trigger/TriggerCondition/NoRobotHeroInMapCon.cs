using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;

namespace ZoneServerLib
{
    public class NoRobotHeroInMapCon : BaseTriCon
    {
        public NoRobotHeroInMapCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam) : base(trigger, conditionType, conditionParam)
        {
        }
        public override bool Check()
        {
            if (trigger.CurMap == null)
            {
                return false;
            }

            foreach (var item in trigger.CurMap.HeroList.Where(kv=> !kv.Value.IsAttacker))
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
