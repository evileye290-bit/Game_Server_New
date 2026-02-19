using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class NormalAttackThornsTriHdl : BaseTriHdl
    {
        readonly int thornsRate;//反伤百分比

        public NormalAttackThornsTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam) 
            : base(trigger, handlerType, handlerParam)
        {
            thornsRate = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.NormalAttackDamage, out param))
            {
                return;
            }

            SkillDamageTriMsg msg = param as SkillDamageTriMsg;
            if (msg != null)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();
                int damage = (int)(thornsRate * 0.0001f * msg.Damage);
                bool immune = false;
                msg.Caster.OnHit(Owner, DamageType.Thorns, damage * -1, ref immune);
            }
        }
    }
}
