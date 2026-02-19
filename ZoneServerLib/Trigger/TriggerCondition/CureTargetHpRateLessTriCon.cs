using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CureTargetHpRateLessTriCon : BaseTriCon
    {
        readonly float rate = 0;
        private FieldObject target = null;
        public CureTargetHpRateLessTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int temp = int.Parse(conditionParam);
            temp = trigger.CalcParam(conditionType, temp);
            rate = temp * 0.0001f;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.SkillAddCureBuff, out param))
            {
                return false;
            }

            FieldObject msg = param as FieldObject;

            if (msg == null)
            {
                return false;
            }
            else
            {
                target = msg;
                return target.GetHp() <= target.GetMaxHp() * rate;
            }
        }
    }
}
