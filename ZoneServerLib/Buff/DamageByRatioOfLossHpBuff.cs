using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class DamageByRatioOfLossHpBuff : BaseBuff
    {
        public DamageByRatioOfLossHpBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (happened) return;

            long lossHp = caster.GetNatureValue(NatureType.PRO_MAX_HP) - caster.GetNatureValue(NatureType.PRO_HP);
            long damage = (long)(lossHp * 0.0001f * m);

            owner.DoSpecDamage(caster, DamageType.Skill, damage);

            happened = true;
            isEnd = true;
        }
    }
}
