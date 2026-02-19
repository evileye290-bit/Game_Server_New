using CommonUtility;

namespace ZoneServerLib
{
    public class OwnerHpLessRatioTriCon : BaseTriCon
    {
        readonly int ratio = 0;

        public OwnerHpLessRatioTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            ratio = int.Parse(conditionParam);
        }

        public override void Update(float dt)
        {
            if (ready)
            {
                return;
            }
            
            if (owner.HpLessThanRate(ratio))
            {
                ready = true;
                trigger.TryHandle();
            }
        }

        public override bool Check()
        {
            return owner.HpLessThanRate(ratio);
        }
    }    
}