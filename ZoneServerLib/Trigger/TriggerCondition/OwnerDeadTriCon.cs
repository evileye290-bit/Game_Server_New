using CommonUtility;

namespace ZoneServerLib
{
    public class OwnerDeadTriCon : BaseTriCon
    {
        public OwnerDeadTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
        }

        public override bool Check()
        {
            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.Dead, out param))
            {
                return false;
            }
            FieldObject fieldObject = param as FieldObject;
            return owner.InstanceId == fieldObject.InstanceId;
        }
    }
}

