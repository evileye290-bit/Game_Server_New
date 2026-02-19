using CommonUtility;

namespace ZoneServerLib
{
    public class EnergyChangeTargetTypeTriCon : BaseTriCon
    {
        //0自身，1友方，2自身和友方（我方全体）
        private int param;

        public EnergyChangeTargetTypeTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            param = int.Parse(conditionParam);
        }

        public override bool Check()
        {
            object obj;
            if (!trigger.TryGetParam(TriggerParamKey.EnergyChangeTarget, out obj))
            {
                return false;
            }

            EnergyChangeMsg msg = obj as EnergyChangeMsg;
            if (msg == null)
            {
                return false;
            }

            switch (param)
            {
                case 0:
                    return msg.Target == trigger.Owner;
                case 1:
                    return msg.Target != trigger.Owner && msg.Target.IsAlly(trigger.Owner);
                case 2:
                    return !msg.Target.IsEnemy(trigger.Owner);
                default:
                    return false;
            }
        }
    }
}