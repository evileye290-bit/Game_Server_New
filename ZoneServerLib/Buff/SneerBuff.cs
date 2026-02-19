using ServerModels;

namespace ZoneServerLib
{
    public class SneerBuff : BaseBuff
    {
        public SneerBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}