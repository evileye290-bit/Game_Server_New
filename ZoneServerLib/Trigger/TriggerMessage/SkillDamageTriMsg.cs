using CommonUtility;

namespace ZoneServerLib
{
    public class SkillDamageTriMsg
    {
        public FieldObject Caster { get; private set; }
        public long Damage { get; private set; }
        public DamageType DamageType { get; private set; }
        public SkillDamageTriMsg(FieldObject caster, long damage, DamageType type)
        {
            Caster = caster;
            Damage = damage;
            DamageType = type;
        }
    }
}
