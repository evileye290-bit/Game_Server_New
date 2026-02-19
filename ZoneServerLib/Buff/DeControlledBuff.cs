using ServerModels;

namespace ZoneServerLib
{
    public class DeControlledBuff : BaseBuff
    {
        public DeControlledBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
