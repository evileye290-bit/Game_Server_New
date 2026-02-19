using CommonUtility;

namespace ZoneServerLib
{
    public class EnergyChangeSkillTypeTriCon : BaseTriCon
    {
        private SkillType skillType;

        public EnergyChangeSkillTypeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            skillType = (SkillType)int.Parse(conditionParam);
        }

        public override bool Check()
        {
            object obj;
            if (!trigger.TryGetParam(TriggerParamKey.EnergyChangeTarget, out obj))
            {
                return false;
            }

            EnergyChangeMsg msg = obj as EnergyChangeMsg;
            if (msg == null)
            {
                return false;
            }

            return msg.Model.Type == skillType;
        }
    }
}