using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddHpByRatioOfTotalDamageBuff : BaseBuff
    {
        public AddHpByRatioOfTotalDamageBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            long totalDamage = owner.GetNatureValue(NatureType.PRO_TOTAL_DAMAGE);
            long hp = (long)(c * 0.0001f * totalDamage);
            owner.AddHp(caster, hp);
        }
    }
}
