using CommonUtility;

namespace ZoneServerLib
{
    public class HpRateGreaterTriCon : BaseTriCon
    {
        readonly float rate = 0;
        public HpRateGreaterTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam) 
            : base(trigger, conditionType, conditionParam)
        {
            rate = int.Parse(conditionParam) * 0.0001f;
        }

        public override bool Check()
        {
            return owner.GetHp() > owner.GetMaxHp() * rate;
        }
    }
}
