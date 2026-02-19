using ServerModels;

namespace ZoneServerLib
{
    public class NextAttackCriticalPerNSecBuff :BaseBuff
    {
        private float effectTime = 0f;
        public NextAttackCriticalPerNSecBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) : base(caster, owner, skillLevel, buffModel)
        {
            effectTime = s;
        }

        protected override void Update(float dt)
        {
            effectTime -= dt;
            if (effectTime <= 0)
            {
                effectTime = s;
                owner.NextCriticalHitCount++;
            }
        }
    }
}