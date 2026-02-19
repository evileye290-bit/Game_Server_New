using ServerModels;

namespace ZoneServerLib
{
    public class RefuseDebuffBuff : BaseBuff
    {
        public RefuseDebuffBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
