using CommonUtility;
using Logger;
using System.Threading;

namespace ZoneServerLib
{
    public class HpRateDeclineOnceTriCon : BaseTriCon
    {
        private readonly long damageValue;
        private int declineCount = 1;
        private long totalDamageValue;

        public HpRateDeclineOnceTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
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
            declineCount = 1;
            totalDamageValue = damageValue*declineCount ;
        }

        public override bool Check()
        {
            if (damageValue <= 0) return false;

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

            if (damage >= totalDamageValue)
            {
                while (totalDamageValue < damage)
                {
                    declineCount++;
                    totalDamageValue = damageValue * declineCount;
                }
                return true;
            }
            return false;
        }
    }
}
