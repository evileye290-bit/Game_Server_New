using CommonUtility;
using ServerFrame;

namespace ZoneServerLib
{
    public class OwnerHpGTRatioTriCon : BaseTriCon
    {
        readonly int ratio = 0;

        private float lastCheckTime = 0f;

        public OwnerHpGTRatioTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            ratio = int.Parse(conditionParam);
        }

        public override void Update(float dt)
        {
            lastCheckTime -= dt;
            if (lastCheckTime <= 0)
            {
                lastCheckTime = 1f;
                if (owner.HpEqualOrGreaterThanRate(ratio))
                {
                    trigger.TryHandle();
                }
            }
        }

        public override bool Check()
        {
            return owner.HpEqualOrGreaterThanRate(ratio);
        }
    }
    
}