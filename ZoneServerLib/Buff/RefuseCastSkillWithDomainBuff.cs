using ServerModels;

namespace ZoneServerLib
{
    public class RefuseCastSkillWithDomainBuff : BaseBuff
    {
        public RefuseCastSkillWithDomainBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
