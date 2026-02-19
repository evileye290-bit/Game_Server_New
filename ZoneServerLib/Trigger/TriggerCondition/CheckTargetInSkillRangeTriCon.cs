using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CheckTargetInSkillRangeTriCon : BaseTriCon
    {
        private readonly int skillId;
        public CheckTargetInSkillRangeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init CheckTargetInSkillRangeTriCon failed: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.TargetInSkillRange, out param)) return false;

            TargetInSkillRangeMsg msg = param as TargetInSkillRangeMsg;
            if (msg == null) return false;

            return msg.SkillId == skillId;
        }
    }
}
