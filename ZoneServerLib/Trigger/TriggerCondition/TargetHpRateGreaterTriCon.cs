using CommonUtility;

namespace ZoneServerLib
{
    public class TargetHpRateGreaterTriCon : BaseTriCon
    {
        readonly float rate = 0;
        private FieldObject target = null;
        bool isCheck = false;
        public TargetHpRateGreaterTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam) 
            : base(trigger, conditionType, conditionParam)
        {
            rate = int.Parse(conditionParam) * 0.0001f;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;

            if (msg.FieldObject == null)
            {
                return false;
            }
            else
            {
                target = msg.FieldObject;
                isCheck = msg.FieldObject.GetHp() >= msg.FieldObject.GetMaxHp() * rate;
                return isCheck;
            }
        }

        public override void Update(float dt)
        {
            if (target == null)
            {
                return;
            }

            if (isCheck || target.IsDead || target.GetHp() < target.GetMaxHp() * rate)
            {
                owner.DispatchMessage(TriggerMessageType.TargetHpRateGreaterCheckFail, target);
                isCheck = false;
            }
        }
    }
}
