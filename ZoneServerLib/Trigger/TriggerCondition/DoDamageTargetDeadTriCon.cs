using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DoDamageTargetDeadTriCon : BaseTriCon
    {
        private readonly int skillId;
        public DoDamageTargetDeadTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init DoDamageTargetBySkillIdTriCon failed: invalid buffType {conditionParam}");
                return;
            }

        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            return msg != null && msg.FieldObject.IsDead;
        }
    }
}

