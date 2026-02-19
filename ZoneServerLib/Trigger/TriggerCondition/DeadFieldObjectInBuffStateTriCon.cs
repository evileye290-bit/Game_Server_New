using CommonUtility;

namespace ZoneServerLib
{
    public class DeadFieldObjectInBuffStateTriCon : BaseTriCon
    {
        private readonly int buffType = 0;
        public DeadFieldObjectInBuffStateTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffType))
            {
                Logger.Log.Error($"KillEnemyInBuffStateTriCon error, buff id {buffType}");
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

            return fieldObject.InBuffState((BuffType)buffType);
        }
    }
}


