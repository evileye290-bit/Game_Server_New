using ServerModels;

namespace ZoneServerLib
{
    public class IgnoreDebuffBuff : BaseBuff
    {
        public IgnoreDebuffBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
