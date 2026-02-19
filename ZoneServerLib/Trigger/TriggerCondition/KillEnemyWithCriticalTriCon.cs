using CommonUtility;

namespace ZoneServerLib
{
    public class KillEnemyWithCriticalTriCon : BaseTriCon
    {
        public KillEnemyWithCriticalTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
        }

        public override bool Check()
        {
            object param = null;
            if(!trigger.TryGetParam(TriggerParamKey.KillEnemy, out param))
            {
                return false;
            }
            KillEnemyTriMsg msg = param as KillEnemyTriMsg;
            if(msg == null)
            {
                return false;
            }
            return msg.Critical;
        }
    }
}
