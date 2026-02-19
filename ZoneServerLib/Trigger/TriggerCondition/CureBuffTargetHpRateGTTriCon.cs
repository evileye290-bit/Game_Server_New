using CommonUtility;

namespace ZoneServerLib
{
    public class CureBuffTargetHpRateGTTriCon : BaseTriCon
    {
        readonly float rate = 0;
        public CureBuffTargetHpRateGTTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int temp = int.Parse(conditionParam);
            temp = trigger.CalcParam(conditionType, temp);
            rate = temp * 0.0001f;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.CastCureBuff, out param))
            {
                return false;
            }

            FieldObject target = param as FieldObject;

            if (target == null)
            {
                return false;
            }
            else
            {
                return target.GetHp() > target.GetMaxHp() * rate;
            }
        }
    }
}