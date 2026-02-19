using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class CureRateSelfOnHpLessBuff : BaseBuff
    {
        private bool effected = false;
        public CureRateSelfOnHpLessBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.DamageOnce, OnDamage);
            AddListener(TriggerMessageType.AddHp, OnAddHp);
            if (owner.HpLessThanRate((int)c))
            {
                effected = true;
            }
        }

        protected override void Update(float dt)
        {
            if (!effected)
            {
                return;
            }
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }

            elapsedTime = 0;

            owner.DoCure(owner, (int)(owner.GetNatureValue(NatureType.PRO_MAX_HP) * (n * 0.0001f)), Model.DispatchCureSKillMsg);
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.DamageOnce, OnDamage);
            RemoveListener(TriggerMessageType.AddHp, OnAddHp);         
            effected = false;
        }     

        private void OnDamage(object param)
        {          
            if (owner.HpLessThanRate((int)c))
            {
                effected = true;
            }
        }
     
        private void OnAddHp(object param)
        {     
            if (!owner.HpLessThanRate((int)c))
            {
                effected = false;
            }
        }
    }
}
