using CommonUtility;

namespace ZoneServerLib
{
    public class DeadFieldObjectInIdBuffTriCon : BaseTriCon
    {
        private readonly int buffId = 0;
        public DeadFieldObjectInIdBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffId))
            {
                Logger.Log.Error($"DeadFieldObjectInIdBuffTriCon error, buff id {buffId}");
                return;
            }
        }

        public override bool Check()
        {
            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.FieldObjectDead, out param))
            {
                return false;
            }
            FieldObject fieldObject = param as FieldObject;
            if (fieldObject == null) return false;

            return fieldObject?.BuffManager.GetBuff(buffId) != null;
        }
    }
}


