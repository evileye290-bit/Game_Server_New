using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个类型的技能成功命中target
    public class SkillTypeHitTriCon : BaseTriCon
    {
        private readonly int skillType;
        public SkillTypeHitTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillType))
            {
                Log.Warn($"init cast skill type hit condition failed: invalid skill type {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if(! trigger.TryGetParam(TriggerParamKey.BuildSkillTypeHitKey(skillType), out param))
            {
                return false;
            }

            SkillHitMsg msg = param as SkillHitMsg;
            if(msg == null)
            {
                return false;
            }

            return skillType == (int)(msg.Model.Type);
        }
    }
}
