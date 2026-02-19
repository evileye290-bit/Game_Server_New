using ServerModels;

namespace ZoneServerLib
{
    public class IgnoreLessDamageBuff : BaseBuff
    {
        public IgnoreLessDamageBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
