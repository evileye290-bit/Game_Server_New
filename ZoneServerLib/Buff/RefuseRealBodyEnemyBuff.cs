using ServerModels;

namespace ZoneServerLib
{
    public class RefuseRealBodyEnemyBuff : BaseBuff
    {
        public RefuseRealBodyEnemyBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }
    }
}
