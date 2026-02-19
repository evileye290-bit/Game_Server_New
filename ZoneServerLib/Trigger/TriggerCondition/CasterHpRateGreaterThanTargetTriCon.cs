using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CasterHpRateGreaterThanTargetTriCon : BaseTriCon
    {
        public CasterHpRateGreaterThanTargetTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam) 
            : base(trigger, conditionType, conditionParam)
        {
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param) &&
                !trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg == null)
            {
                return false;
            }

            return owner.GetHpRate() >= msg.FieldObject.GetHpRate();
        }
    }
}
