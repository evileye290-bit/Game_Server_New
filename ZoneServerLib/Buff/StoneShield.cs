using ServerModels;

namespace ZoneServerLib
{
    class StoneShield : BaseBuff
    {
        public StoneShield(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
