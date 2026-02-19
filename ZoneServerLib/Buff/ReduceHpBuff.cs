using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    class ReduceHpBuff : BaseBuff
    {
        public ReduceHpBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) : 
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Update(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }
            elapsedTime = 0;
            owner.DoSpecDamage(caster, DamageType.Bleed, m, Id);
        }
    }
}
