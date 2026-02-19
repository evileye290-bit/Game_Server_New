using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AllyDeadCountTriCon : BaseTriCon
    {
        private readonly int count;
        public AllyDeadCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init AllyDeadCountTriCon condition failed: invalid count {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            return count <= trigger.GetCounter(TriggerCounter.AllyDead);
        }
    }
}
