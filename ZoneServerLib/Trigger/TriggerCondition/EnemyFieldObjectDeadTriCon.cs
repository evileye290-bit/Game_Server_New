using CommonUtility;

namespace ZoneServerLib
{
    public class EnemyFieldObjectDeadTriCon : BaseTriCon
    {
        public EnemyFieldObjectDeadTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.FieldObjectDead, out param))
            {
                return false;
            }

            FieldObject fieldObject = param as FieldObject;

            return fieldObject != null && fieldObject.IsEnemy(trigger.Owner);
        }
    }
}
