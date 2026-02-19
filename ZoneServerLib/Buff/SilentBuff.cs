using ServerModels;

namespace ZoneServerLib
{
    public class SilentBuff : BaseBuff
    {
        public SilentBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
