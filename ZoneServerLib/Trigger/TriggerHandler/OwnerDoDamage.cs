using CommonUtility;
using System.Collections.Generic;

namespace ZoneServerLib
{
    class OwnerDoDamage : BaseTriHdl
    {
        readonly int damage;
        public OwnerDoDamage(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam) 
            : base(trigger, handlerType, handlerParam)
        {
            Logger.Log.Debug("================== handlerParam:" + handlerParam + "ID:" + trigger.Model.Id);

            List<int> param = handlerParam.ToList(':');
            if (param.Count != 2)
            {
                Logger.Log.Error($"OwnerDoDamage param error {handlerParam}");
                return;
            }

            NatureType nature = (NatureType)param[0];
            damage = (int)(trigger.Caster.GetNatureValue(nature) * (param[1] * 0.0001f));
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            Logger.Log.Debug($"Owner {Owner.InstanceId} DoDamage {damage} trigger id: {trigger.Model.Id}");
            Owner.DoSpecDamage(Owner, DamageType.Extra, damage);
        }
    }
}
