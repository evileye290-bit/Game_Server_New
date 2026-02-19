using ServerModels;

namespace ZoneServerLib
{
    public class BleedMoreBuff:BaseBuff
    {
        public BleedMoreBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) : 
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
