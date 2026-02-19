using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class DamageOnceBuff : BaseBuff
    {
        public DamageOnceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            owner.DoSpecDamage(caster, DamageType.Skill, m);
        }

    }
}

