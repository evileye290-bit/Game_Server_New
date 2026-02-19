using ServerModels;

namespace ZoneServerLib
{
    public class ReduceBodyEnergyPerTimeBuff : BaseBuff
    {
        public ReduceBodyEnergyPerTimeBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
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
            if (owner.SkillManager.HasBodySkill && !owner.IsDead && !owner.InRealBody)
            {
                owner.SkillManager.AddBodyEnergy((int)-c, true, true, true);
            }
        }
    }
}
