using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个技能结束了
    public class SkillEndTriCon : BaseTriCon
    {
        private readonly int skillId;
        public SkillEndTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init skill end trigger condition failed: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            return trigger.TryGetParam(TriggerParamKey.BuildEndedSkillKey(skillId), out param);
        }
    }
}
