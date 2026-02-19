using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class TargetHaveTypeBuffTriCon : BaseTriCon
    {
        private readonly int buffType;
        public TargetHaveTypeBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffType))
            {
                Log.Warn($"OwnerHaveTypeBuffTriCon error: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param) &&
                !trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return false;
            }
            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg == null)
            {
                return false;
            }

            List<BaseBuff> buffList = msg.FieldObject?.BuffManager.GetBuffList();
            switch (buffType)
            {
                case 1:
                    return buffList.Where(x => x.Model.Debuff).Count() > 0;
                case 2:
                    return buffList.Where(x => x.Model.CleanUp).Count() > 0;
                case 3:
                    return buffList.Where(x => x.Model.Dispel).Count() > 0;
                default:
                    break;
            }
            return false;
        }
    }
}
