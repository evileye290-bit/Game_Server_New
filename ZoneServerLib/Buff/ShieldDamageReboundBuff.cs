using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;

namespace ZoneServerLib
{
    public class ShieldDamageReboundBuff : BaseBuff
    {
        private bool effected = false;
        public ShieldDamageReboundBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.ShieldDamage, OnShieldDamage);
        }

        protected override void Update(float dt)
        {

        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.ShieldDamage, OnShieldDamage);
        }

        private void OnShieldDamage(object param)
        {
            if(!owner.InShield())
            {
                return;
            }
            ShieldDamageTriMsg msg = param as ShieldDamageTriMsg;
            if(msg == null)
            {
                return;
            }
            int reboundDamage = (int)(msg.Damage * c * 0.0001f);
            msg.Caster.DoSpecDamage(owner, DamageType.Thorns, reboundDamage);
        }
    }
}
