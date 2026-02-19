using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class HaveNotTypeOfBuffTriCon : BaseTriCon
    {
        private readonly BuffType buffType;
        public HaveNotTypeOfBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int buff;
            if (!int.TryParse(conditionParam, out buff))
            {
                Log.Warn($"init HaveNotTypeOfBuffTriCon failed: invalid buffType {conditionParam}");
                return;
            }

            buffType = (BuffType)buff;
        }

        public override bool Check()
        {
            return !owner.InBuffState(buffType);
        }
    }
}

