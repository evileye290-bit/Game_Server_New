using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AnySkillDamageCountTriCon : BaseTriCon
    {
        private readonly int count;
        public AnySkillDamageCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out count))
            {
                Log.Warn($"init AnySkillDamageCountTriCon condition failed: invalid count {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            return count <= trigger.GetCounter(TriggerCounter.AnySkillDamage);
        }
    }
}
