using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public class BleedBuff : BaseBuff
    {
        public BleedBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) : 
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
