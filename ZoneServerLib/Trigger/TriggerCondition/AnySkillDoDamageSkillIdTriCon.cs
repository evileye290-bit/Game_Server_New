using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AnySkillDoDamageSkillIdTriCon : BaseTriCon
    {
        private readonly int skillId;
        public AnySkillDoDamageSkillIdTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init AnySkillDoDamageSkillIdTriCon condition failed: invalid count {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param) && !trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return false;
            }
            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg == null || msg.SkillId == 0 || msg.FieldObject == null)
            {
                return false;
            }
            return msg.SkillId == skillId;
        }
    }
}
