using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class ReduceCurrHpRateTriHdl : BaseTriHdl
    {
        private readonly int ratio;

        public ReduceCurrHpRateTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out ratio))
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
            long damage = (long)(Owner.GetNatureValue(NatureType.PRO_HP) * (ratio * 0.0001f));

            Owner.AddNatureBaseValue(NatureType.PRO_HP, damage * -1);
            Owner.CheckDead();
            Owner.BroadCastHp();

            Owner.CurrentMap.RecordBattleDataCure(trigger.Caster, Owner, BattleDataType.Cure, damage);

            MSG_ZGC_DAMAGE damageMsg = Owner.GenerateDamageMsg(Owner.InstanceId, DamageType.Skill, damage);
            Owner.BroadCast(damageMsg);
        }
    }
}
