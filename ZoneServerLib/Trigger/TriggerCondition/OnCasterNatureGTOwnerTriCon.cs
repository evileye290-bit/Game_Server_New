using CommonUtility;

namespace ZoneServerLib
{
    public class OnCasterNatureGTOwnerTriCon : BaseTriCon
    {
        readonly NatureType natureType;
        public OnCasterNatureGTOwnerTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            natureType = (NatureType) int.Parse(conditionParam);
        }

        public override bool Check()
        {
            return trigger.Caster.GetNatureValue(natureType)>trigger.Owner.GetNatureValue(natureType);
        }
    }
}
