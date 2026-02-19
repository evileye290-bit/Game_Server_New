using CommonUtility;

namespace ZoneServerLib
{
    public class TargetHpRateLessTriCon : BaseTriCon
    {
        readonly float rate = 0;
        private FieldObject target = null;
        bool isCheck = false;
        public TargetHpRateLessTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int temp = int.Parse(conditionParam);
            temp = trigger.CalcParam(conditionType, temp);
            rate = temp * 0.0001f;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return false;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;

            if (msg == null || msg.FieldObject == null)
            {
                return false;
            }
            
            target = msg.FieldObject;
            isCheck = target.GetHp() <= target.GetMaxHp() * rate;
            return isCheck;
        }

        public override void Update(float dt)
        {
            if(target == null)
            {
                return;
            }

            if(isCheck || target.IsDead || target.GetHp() > target.GetMaxHp() * rate)
            {
                owner.DispatchMessage(TriggerMessageType.TargetHpRateLessCheckFail, target);
                isCheck = false;
            }
        }

    }
}
