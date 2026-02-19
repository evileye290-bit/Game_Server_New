using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DoDamageTargetNotHaveMarkTriCon : BaseTriCon
    {
        private readonly int markId;
        public DoDamageTargetNotHaveMarkTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out markId))
            {
                Log.Warn($"init FieldObjectDeadCheckHateEnemyTriCon trigger condition failed: invalid mark id {conditionParam}");
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

            if (msg != null)
            {
                Mark mark = msg.FieldObject.MarkManager.GetMark(markId);
                if (mark == null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
