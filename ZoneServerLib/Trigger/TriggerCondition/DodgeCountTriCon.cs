using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DodgeCountTriCon : BaseTriCon
    {
        private readonly int count;
        public DodgeCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init DodgeCountTriCon condition failed: invalid count {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            return count <= trigger.GetCounter(TriggerCounter.Dodge);
        }
    }
}
