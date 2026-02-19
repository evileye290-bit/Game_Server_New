using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CastTypeBuffTriCon : BaseTriCon
    {
        private readonly int buffType;
        public CastTypeBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffType))
            {
                Log.Warn($"init CastTypeBuffTriCon condition failed: invalid buff id {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildCastBuffTypeKey(buffType), out param))
            {
                return false;
            }
            BaseBuff buff = param as BaseBuff;
            if (buff == null)
            {
                return false;
            }
            return (int)buff.BuffType == buffType;
        }
    }
}
