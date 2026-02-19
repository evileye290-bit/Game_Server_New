using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class BeenCuredCounterTriCon : BaseTriCon
    {
        private readonly int cureCount;
        public BeenCuredCounterTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out cureCount))
            {
                Log.Warn($"init CounterTriCon condition failed: invalid count {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            return cureCount <= trigger.GetCounter(TriggerCounter.BeenCured);
        }
    }
}
