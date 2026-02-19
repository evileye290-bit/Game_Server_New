using ServerModels;

namespace ZoneServerLib
{
    public class EnhanceShieldHpBuff : BaseBuff
    {
        public EnhanceShieldHpBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}