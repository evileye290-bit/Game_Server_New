using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnemyDeadCountTriCon : BaseTriCon
    {
        private readonly int count;
        public EnemyDeadCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init EnemyDeadCountTriCon condition failed: invalid count {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            return count <= trigger.GetCounter(TriggerCounter.EnemyDead);
        }
    }
}
