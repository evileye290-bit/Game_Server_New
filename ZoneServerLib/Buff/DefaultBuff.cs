using ServerModels;

namespace ZoneServerLib
{
    public class DefaultBuff : BaseBuff
    {
        public DefaultBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
