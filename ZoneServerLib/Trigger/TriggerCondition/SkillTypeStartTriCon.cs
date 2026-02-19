using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个类型的技能开始了
    public class SkillTypeStartTriCon : BaseTriCon
    {
        private readonly int skillType;
        public SkillTypeStartTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillType))
            {
                Log.Warn($"init cast skill type start condition failed: invalid skill type {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            trigger.TryGetParam(TriggerParamKey.BuildSkillTypeStartKey(skillType), out param);
            if (param == null)
            {
                return false;
            }
            int paramInt;
            if(!int.TryParse(param.ToString(), out paramInt))
            {
                return false;
            }
            return skillType == paramInt;
        }
    }
}
