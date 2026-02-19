using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FullHpTriCon : BaseTriCon
    {      
        public FullHpTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam) 
            : base(trigger, conditionType, conditionParam)
        {         
        }

        public override bool Check()
        {
            return owner.FullHp();
        }
    }
}
