using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class EnhanceAtkRatioOnHpRatioGTBuff : BaseBuff
    {
        private bool effected;

        public EnhanceAtkRatioOnHpRatioGTBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Update(float dt)
        {
            if (owner.HpEqualOrGreaterThanRate(n))
            {
                if (!effected)
                {
                    Effect();
                }
            }
            else
            {
                if (effected)
                {
                    UnEffect();
                }
            }
        }

        protected override void End()
        {
            // 属性还原
            if (effected)
            {
                UnEffect();
            }
        }

        private void Effect()
        {
            effected = true;
            owner.AddNatureRatio(NatureType.PRO_ATK, (int)c, Model.Notify);
        }

        private void UnEffect()
        {
            effected = false;
            owner.AddNatureRatio(NatureType.PRO_ATK, (int)c * -1);
        }
    }
}
