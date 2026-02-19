using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    class ReduceHpOnceBuff : BaseBuff
    {
        public ReduceHpOnceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (happened) return;
            owner.DoSpecDamage(caster, DamageType.Skill, m, Id);
            happened = true;
            isEnd = true;
        }
    }
}
