using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class NormalAttackHitCountTriCon : BaseTriCon
    {
        private readonly int count;
        public NormalAttackHitCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init NormalAttackHitCountTriCon condition failed: invalid count {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            return count <= trigger.GetCounter(TriggerCounter.NormalAttackHit);
        }
    }
}
