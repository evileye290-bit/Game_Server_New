using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个id的技能造成伤害
    public class OneSkillDamageTriCon : BaseTriCon
    {
        private readonly int skillId;
        public OneSkillDamageTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillId))
            {
                Log.Warn($"init one skill damage condition failed: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if(! trigger.TryGetParam(TriggerParamKey.BuildOneSkillDamageKey(skillId), out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            return msg!=null && skillId == msg.SkillId;
        }
    }
}
