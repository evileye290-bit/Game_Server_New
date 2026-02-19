using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class InBuffStateTriCon : BaseTriCon
    {
        private readonly BuffType buffType;
        public InBuffStateTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int buff = 0;
            if (!int.TryParse(conditionParam, out buff))
            {
                Log.Warn($"init InBuffStateTriCon failed, invalid handler param {conditionParam}");
            }
            else
            {
                buffType = (BuffType)buff;
            }
        }

        public override bool Check()
        {
            return owner.InBuffState(buffType);
        }
    }
}
