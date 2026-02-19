using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddReduceDamageNatureOnHpLessPerRatioBuff : BaseBuff
    {
        private int lastAddV = 0;

        public AddReduceDamageNatureOnHpLessPerRatioBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.DamageOnce, OnDamage);
            AddListener(TriggerMessageType.AddHp, OnAddHp);
            Effect();
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.DamageOnce, OnDamage);
            RemoveListener(TriggerMessageType.AddHp, OnAddHp);

            // 属性还原
            if (lastAddV>0)
            {
                UnEffect();
            }
        }

        private void Effect()
        {
            float lostRatio = 1f - owner.GetHpRatio();

            int ratio = (int) (lostRatio / (c * 0.0001f));

            int addV = ratio * n;
            if (addV != lastAddV)
            {
                owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG, addV - lastAddV, Model.Notify);
                lastAddV = addV;
            }
        }

        private void UnEffect()
        {
            owner.AddNatureAddedValue(NatureType.PRO_RDC_DMG, -1 * lastAddV);
        }

        private void OnDamage(object param)
        {
            Effect();
        }

        private void OnAddHp(object param)
        {
            Effect();
        }
    }
}
