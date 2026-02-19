using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个技能全都Miss了
    public class SkillMissPlayerTriCon : BaseTriCon
    {
        private readonly int skillId;
        public SkillMissPlayerTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init skill miss trigger condition failed: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            return trigger.TryGetParam(TriggerParamKey.BuildMissedSkillKey(skillId), out param);
        }
    }
}
