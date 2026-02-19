using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class BleedAndReduceAttackBuff : BaseBuff
    {
        private static int reduceAttackMax = 5000;

        private int addRatio = 0;

        private long reduceAttack = 0;

        public BleedAndReduceAttackBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            addRatio = (int)Model.C.Value;
            ChangeRatio(addRatio);
        }

        protected override void Update(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }
            elapsedTime = 0;
            owner.DoSpecDamage(caster, DamageType.Bleed, m, Id);
        }

        protected override void Pile(int addNum)
        {
            int ratio = (int)(Model.C.Value * pileNum);

            if (ratio != addRatio && ratio <= reduceAttackMax)
            {
                ChangeRatio(ratio - addRatio);
                addRatio = ratio;
            }
        }

        protected override void End()
        {
            //结束后恢复减少的值
            if (reduceAttack > 0)
            {
                owner.AddNatureAddedValue(NatureType.PRO_ATK, reduceAttack);

                addRatio = 0;
                reduceAttack = 0;
            }
        }

        private void ChangeRatio(int ratio)
        {
            long changeValue =  (long)(ratio * 0.0001f * owner.GetNatureValue(NatureType.PRO_ATK));

            reduceAttack += changeValue;

            owner.AddNatureAddedValue(NatureType.PRO_ATK, changeValue * -1, true);
        }
    }
}