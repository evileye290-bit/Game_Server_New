using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DoDamageTargetHaveIdBuffTriCon : BaseTriCon
    {
        private readonly int buffId;
        public DoDamageTargetHaveIdBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffId))
            {
                Log.Warn($"init DoDamageTargetHaveIdBuffTriCon failed: invalid buffId {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            return msg != null && msg.FieldObject != null && (msg.FieldObject.BuffManager.GetBuff(buffId) != null); ;
        }
    }
}

