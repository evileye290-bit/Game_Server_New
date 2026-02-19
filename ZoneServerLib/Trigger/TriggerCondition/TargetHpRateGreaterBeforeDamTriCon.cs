using CommonUtility;

namespace ZoneServerLib
{
    public class TargetHpRateGreaterBeforeDamTriCon : BaseTriCon
    {
        readonly float rate = 0;
        private FieldObject target = null;
        bool isCheck = false;
        
        public TargetHpRateGreaterBeforeDamTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int temp = int.Parse(conditionParam);
            temp = trigger.CalcParam(conditionType, temp);
            rate = temp * 0.0001f;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
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
                isCheck = target.GetHp() > target.GetMaxHp() * rate;
                return isCheck;
            }
        }
    }
}
