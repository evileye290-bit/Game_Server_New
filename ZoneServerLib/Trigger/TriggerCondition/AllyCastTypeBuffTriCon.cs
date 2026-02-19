using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AllyCastTypeBuffTriCon : BaseTriCon
    {
        private readonly int buffType;
        public AllyCastTypeBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffType))
            {
                Log.Warn($"init AllyCastTypeBuffTriCon condition failed: invalid buff type {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            trigger.TryGetParam(TriggerParamKey.BuildAllyCastTypeBuffKey(buffType), out param);
            if (param == null)
            {
                return false;
            }

            BaseBuff msg = param as BaseBuff;
            if (msg == null) return false;

            return msg.Caster != owner && (int)msg.Model.BuffType == buffType && msg.Caster.IsAlly(owner);
        }
    }
}
