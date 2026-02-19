using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个类型的技能开始了
    public class SkillTypeEndTriCon : BaseTriCon
    {
        private readonly int skillType;
        public SkillTypeEndTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillType))
            {
                Log.Warn($"init cast skill type end condition failed: invalid skill type {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            trigger.TryGetParam(TriggerParamKey.BuildSkillTypeEndKey(skillType), out param);
            int paramInt;
            if (param==null || !int.TryParse(param.ToString(), out paramInt))
            {
                return false;
            }
            return skillType == paramInt;
        }
    }
}
