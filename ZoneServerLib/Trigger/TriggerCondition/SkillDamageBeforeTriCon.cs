using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个技能开始了
    public class SkillDamageBeforeTriCon : BaseTriCon
    {
        private readonly int skillId;
        public SkillDamageBeforeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init SkillDamageBeforeTriCon trigger condition failed: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            return msg != null && msg.SkillId == skillId;
        }
    }
}
