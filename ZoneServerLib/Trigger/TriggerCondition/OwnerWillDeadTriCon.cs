using CommonUtility;

namespace ZoneServerLib
{
    public class OwnerWillDeadTriCon : BaseTriCon
    {
        private readonly int IsAlly = 1;
        public OwnerWillDeadTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out IsAlly))
            {
                Logger.Log.Error($"KillEnermyWithBuffDamageTriCon error, buff id {IsAlly}");
                return;
            }
        }

        public override bool Check()
        {
            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.WillDead, out param))
            {
                return false;
            }
            FieldObject fieldObject = param as FieldObject;
            switch (IsAlly)
            {
                case 2:
                   return  owner.IsAlly(fieldObject);
                case 1:
                   return  owner.IsEnemy(fieldObject);
                case 0:
                   return owner.InstanceId == fieldObject.InstanceId;
                default:
                    break;
            }
            return false;
        }
    }
}

