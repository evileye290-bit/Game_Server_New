using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AllySkillTypeStartTriCon : BaseTriCon
    {
        private readonly int skillType;
        public AllySkillTypeStartTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out skillType))
            {
                Log.Warn($"init AllySkillTypeStartTriCon condition failed: invalid skill type {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            trigger.TryGetParam(TriggerParamKey.BuildAllySkillTypeStartKey(skillType), out param);
            if (param == null)
            {
                return false;
            }

            SkillStartMsg msg = param as SkillStartMsg;
            if (msg == null) return false;

            return msg.Caster != owner && (int)msg.Model.Type == skillType; ;
        }
    }
}
