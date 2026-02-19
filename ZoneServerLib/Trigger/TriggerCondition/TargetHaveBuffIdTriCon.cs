using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class TargetHaveBuffIdTriCon : BaseTriCon
    {
        private readonly int buffId;
        public TargetHaveBuffIdTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffId))
            {
                Log.Warn($"TargetHaveBuffIdTriCon error: invalid buffId id {conditionParam}");
            }
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
            return msg != null && msg.FieldObject.BuffManager.HaveBuff(buffId);
        }
    }
}
