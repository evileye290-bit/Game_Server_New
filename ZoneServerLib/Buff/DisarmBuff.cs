using ServerModels;

namespace ZoneServerLib
{
    public class DisarmBuff : BaseBuff
    {
        public DisarmBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
