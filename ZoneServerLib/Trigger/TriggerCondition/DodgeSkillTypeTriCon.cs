using CommonUtility;

namespace ZoneServerLib
{
    public class DodgeSkillTypeTriCon : BaseTriCon
    {
        readonly SkillType skillType;
        public DodgeSkillTypeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int type = 0;
            int.TryParse(conditionParam, out type);
            skillType = (SkillType)type;
        }

        public override bool Check()
        {
            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.DodgeSkill, out param))
            {
                return false;
            }
            Skill msg = param as Skill;
            if (msg != null || msg.SkillModel.Type == skillType)
            {
                return true;
            }
            return false;
        }
    }
}
