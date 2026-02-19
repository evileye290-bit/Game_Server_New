using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class PoisonBuff : BaseBuff
    {
        public PoisonBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
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
            owner.DoSpecDamage(caster, DamageType.Poison, m, Id);
        }
    }
}
