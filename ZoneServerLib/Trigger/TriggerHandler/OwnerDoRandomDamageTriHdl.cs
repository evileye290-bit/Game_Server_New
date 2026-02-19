using CommonUtility;

namespace ZoneServerLib
{
    class OwnerDoRandomDamageTriHdl : BaseTriHdl
    {
        readonly int minDamage, maxDamage;
        public OwnerDoRandomDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] damageV = handlerParam.Split('|');
            if (damageV.Length != 2)
            {
                Logger.Log.Error($"OwnerDoRandomDamage error, param {handlerParam}");
                return;
            }

            if (!int.TryParse(damageV[0], out minDamage) || !int.TryParse(damageV[1], out maxDamage))
            {
                Logger.Log.Error($"OwnerDoRandomDamage error, param {handlerParam}");
                return;
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            int damage = RAND.Range(minDamage, maxDamage - 1);
            Owner.DoSpecDamage(Owner, DamageType.Skill, damage);
        }
    }
}
