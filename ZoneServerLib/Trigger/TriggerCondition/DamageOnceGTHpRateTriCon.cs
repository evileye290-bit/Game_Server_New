using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DamageOnceGTHpRateTriCon : BaseTriCon
    {
        private readonly long damageValue;
        public DamageOnceGTHpRateTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (owner == null)
            {
                Log.Warn("create damage once greated than hp rate tri con failed: owner is null");
                return;
            }
            int rate = 0;
            if (!int.TryParse(conditionParam, out rate))
            {
                Log.Warn($"create damage once greated than hp rate tri con failed: invalid param {conditionParam}");
                return;
            }
            damageValue = (long)(owner.GetMaxHp() * (rate * 0.0001f));

        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.OnceDamage, out param))
            {
                return false;
            }

            DamageTriMsg msg = param as DamageTriMsg;
            if (msg == null) return false;

            return msg.Damage >= damageValue;
        }
    }
}
