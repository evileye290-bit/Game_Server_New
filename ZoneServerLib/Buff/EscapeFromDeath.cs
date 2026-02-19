using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class EscapeFromDeath : BaseBuff
    {
        public EscapeFromDeath(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

    }
}
