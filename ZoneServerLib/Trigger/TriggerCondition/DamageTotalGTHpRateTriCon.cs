using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DamageGreaterThanHpRateTriCon : BaseTriCon
    {
        private readonly long damageValue;
        public DamageGreaterThanHpRateTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if(owner == null)
            {
                Log.Warn("create damage total greated than hp rate tri con failed: owner is null");
                return;
            }
            int rate = 0;
            if(!int.TryParse(conditionParam, out rate))
            {
                Log.Warn($"create damage total greated than hp rate tri con failed: invalid param {conditionParam}");
                return;
            }
            damageValue = (long)(owner.GetMaxHp() * (rate * 0.0001f));
        }

        public override bool Check()
        {
            object param;
            if(!trigger.TryGetParam(TriggerParamKey.TotalDamage, out param))
            {
                return false;
            }
            int damage;
            if (!int.TryParse(param.ToString(), out damage))
            {
                return false;
            }
            return damage >= damageValue;
        }
    }
}
