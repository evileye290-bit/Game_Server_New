using ServerModels;

namespace ZoneServerLib
{
    public class IgnoreControlBuff : BaseBuff
    {
        public IgnoreControlBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
