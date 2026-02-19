using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class EnhanceBeenCureOnHpLessBuff : BaseBuff
    {
        private bool effected = false;
        public EnhanceBeenCureOnHpLessBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (owner.HpLessThanRate((int)c))
            {
                Effect();
            }
        }

        protected override void Update(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }
            elapsedTime = 0;
            if (owner.HpLessThanRate((int)c))
            {
                Effect();
            }
            else
            {

                UnEffect();
            }
        }

        protected override void End()
        {
            UnEffect();
        }

        private void Effect()
        {
            if (!effected)
            {
                effected = true;
                owner.AddNatureAddedValue(NatureType.PRO_BECURED_ENHANCE, n, Model.Notify);
            }
        }

        private void UnEffect()
        {
            if(effected)
            {
                effected = false;
                owner.AddNatureAddedValue(NatureType.PRO_BECURED_ENHANCE, n * -1);
            }
        }
    }
}
