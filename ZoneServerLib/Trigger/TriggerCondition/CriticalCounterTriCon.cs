using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CriticalCounterTriCon : BaseTriCon
    {
        private readonly int count;
        public CriticalCounterTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init CriticalCounterTriCon condition failed: invalid count {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            if (count <= trigger.GetCounter(TriggerCounter.Critical))
            {
                trigger.SetCounter(TriggerCounter.Critical, 0);
                return true;
            }
            return false;
        }
    }
}
