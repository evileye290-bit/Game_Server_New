using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class ReduceHpRateRangeTriHdl : BaseTriHdl
    {
        private readonly int rateMin, rateMax, maxDamage;

        public ReduceHpRateRangeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {

            string[] param = handlerParam.Split('|');
            if (param.Length != 3)
            {
                Log.Warn("in ReduceHpRateRangeTriHdl: invalid param {0}", handlerParam);
                return;
            }

            if (!int.TryParse(param[0], out rateMin) || !int.TryParse(param[1], out rateMax) || !int.TryParse(param[2], out maxDamage))
            {
                Log.Warn("in ReduceHpRateRangeTriHdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            int rato = RAND.Range(rateMin, rateMax - 1);
            long damage = (long)(Owner.GetNatureValue(NatureType.PRO_MAX_HP) * (rato * 0.0001f));

            if (damage > maxDamage)
            {
                damage = maxDamage;
            }

            Owner.AddNatureBaseValue(NatureType.PRO_HP, damage * -1);
            Owner.CheckDead();
            Owner.BroadCastHp();

            MSG_ZGC_DAMAGE damageMsg = Owner.GenerateDamageMsg(Owner.InstanceId, DamageType.Skill, damage);
            Owner.BroadCast(damageMsg);
        }
    }
}
