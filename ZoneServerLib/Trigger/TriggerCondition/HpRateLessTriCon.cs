using CommonUtility;

namespace ZoneServerLib
{
    public class HpRateLessTriCon : BaseTriCon
    {
        readonly float rate = 0;
        public HpRateLessTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int temp = int.Parse(conditionParam);
            temp = trigger.CalcParam(conditionType, temp);
            rate = temp * 0.0001f;
        }

        public override bool Check()
        {
            return owner.GetHp() <= owner.GetMaxHp() * rate;
        }
    }
}
