using CommonUtility;

namespace ZoneServerLib
{
    public class OnCasterNatureLEOwnerTriCon : BaseTriCon
    {
        readonly NatureType natureType;
        public OnCasterNatureLEOwnerTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            natureType = (NatureType) int.Parse(conditionParam);
        }

        public override bool Check()
        {
            return trigger.Caster.GetNatureValue(natureType) < trigger.Owner.GetNatureValue(natureType);
        }
    }
}
