using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个技能开始了
    public class SkillStartTriCon : BaseTriCon
    {
        private readonly int skillId;
        public SkillStartTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init skill start trigger condition failed: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            return trigger.TryGetParam(TriggerParamKey.BuildStartedSkillKey(skillId), out param);
        }
    }
}
