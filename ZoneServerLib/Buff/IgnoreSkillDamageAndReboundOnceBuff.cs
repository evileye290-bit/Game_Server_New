using ServerModels;

namespace ZoneServerLib
{
    public class IgnoreSkillDamageAndReboundOnceBuff : BaseBuff
    {
        public IgnoreSkillDamageAndReboundOnceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

    }
}
