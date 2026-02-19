using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DoDamageTargetInTypeBuffStateTriCon : BaseTriCon
    {
        private readonly BuffType buffType;
        public DoDamageTargetInTypeBuffStateTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int buff;
            if (!int.TryParse(conditionParam, out buff))
            {
                Log.Warn($"init DoDamageTargetInTypeBuffStateTriCon failed: invalid buffType {conditionParam}");
                return;
            }

            buffType = (BuffType)buff;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param) &&
                !trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;

            return msg != null && msg.FieldObject.InBuffState(buffType); ;
        }
    }
}

