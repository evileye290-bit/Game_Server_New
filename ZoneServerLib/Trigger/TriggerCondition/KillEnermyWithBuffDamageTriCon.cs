using CommonUtility;

namespace ZoneServerLib
{
    public class KillEnermyWithBuffDamageTriCon : BaseTriCon
    {
        private readonly int buffId = 0;
        public KillEnermyWithBuffDamageTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffId))
            {
                Logger.Log.Error($"KillEnermyWithBuffDamageTriCon error, buff id {buffId}");
                return;
            }
        }

        public override bool Check()
        {
            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.KillEnemy, out param))
            {
                return false;
            }
            KillEnemyTriMsg msg = param as KillEnemyTriMsg;
            if (msg == null || msg.Param == null || buffId != (int)msg.Param)
            {
                return false;
            }
            switch (msg.DamageType)
            {
                case DamageType.Bleed:
                case DamageType.Burn:
                case DamageType.Poison:
                    return true;
            }
            return false;
        }
    }
}

