using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个技能起效了
    public class SkillEfflTriCon : BaseTriCon
    {
        private readonly int skillId;
        public SkillEfflTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam) 
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init cast skill eff condition failed: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            return trigger.TryGetParam(TriggerParamKey.BuildEffSkillKey(skillId), out param);
        }
    }
}
