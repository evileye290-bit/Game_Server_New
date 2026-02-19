using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DoDamageTargetInControlledBuffTriCon : BaseTriCon
    {
        private readonly bool isInControlledBuff;
        public DoDamageTargetInControlledBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int value;
            if (!int.TryParse(conditionParam, out value))
            {
                Log.Warn($"init DoDamageTargetInControlledBuffTriCon failed: invalid value {conditionParam}");
                return;
            }

            isInControlledBuff = value == 0;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;

            if(isInControlledBuff)
            {
                return msg != null && msg.FieldObject.BeControlled();
            }
            else
            {
                return msg != null && !msg.FieldObject.BeControlled();
            }
        }
    }
}

