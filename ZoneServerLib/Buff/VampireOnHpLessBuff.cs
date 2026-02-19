using ServerModels;

namespace ZoneServerLib
{
    public class VampireOnHpLessBuff : BaseBuff
    {
        private  readonly float hpRatio;
        public VampireOnHpLessBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            hpRatio = n * 0.0001f;
        }

        public override void SpecLogic(object param)
        {
            if (owner.GetHp() * 1f / owner.GetMaxHp() <= hpRatio)
            {
                int damage = 0;
                if (!int.TryParse(param.ToString(), out damage))
                {
                    return;
                }
                int addHp = (int)(damage * c * 0.0001f);
                if (addHp > 0)
                {
                    owner.AddHp(owner, addHp);
                }
            }
        }
    }
}
