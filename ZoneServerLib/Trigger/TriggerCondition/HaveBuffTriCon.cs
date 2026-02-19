using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class HaveBuffTriCon : BaseTriCon
    {
        private readonly int buffId;
        public HaveBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffId))
            {
                Log.Warn($"HaveBuffTriCon error: invalid skill id {conditionParam}");
            }
        }

        public override bool Check()
        {
            return trigger.Owner?.BuffManager.GetBuff(buffId) != null;
        }
    }
}
