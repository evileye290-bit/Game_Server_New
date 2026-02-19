using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DoDamageTargetInDebuffTriCon : BaseTriCon
    {
        private readonly int sillId;
        public DoDamageTargetInDebuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out sillId))
            {
                Log.Warn($"init DoDamageTargetInControlledBuffTriCon failed: invalid value {conditionParam}");
                return;
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;

            return msg != null && msg.SkillId == sillId && msg.FieldObject.HasDebuff();
        }
    }
}

