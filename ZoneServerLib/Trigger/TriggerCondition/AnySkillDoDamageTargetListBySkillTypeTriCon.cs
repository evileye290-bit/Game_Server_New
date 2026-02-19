using CommonUtility;

namespace ZoneServerLib
{
    public class AnySkillDoDamageTargetListBySkillTypeTriCon : BaseTriCon
    {
        private SkillType skillType;

        public AnySkillDoDamageTargetListBySkillTypeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            skillType = (SkillType)int.Parse(conditionParam);
        }

        public override bool Check()
        {
            object obj;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageTargetList, out obj))
            {
                return false;
            }

            DoDamageTargetListTriMsg targetList = obj as DoDamageTargetListTriMsg;
            if (targetList == null)
            {
                return false;
            }

            return skillType == targetList.Skill?.SkillModel.Type;
        }
    }
}