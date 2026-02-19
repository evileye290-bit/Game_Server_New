using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddNatureRatioByNatureOnFullHpBuff : BaseBuff
    {
        private NatureType tempNatureType;
        private bool effected = false;
        public AddNatureRatioByNatureOnFullHpBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            tempNatureType = (NatureType)(int)buffModel.M.Value;
        }

        protected override void Start()
        {
            if (owner.HpEqualOrGreaterThanRate(10000))
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
            if (owner.HpEqualOrGreaterThanRate(10000))
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
                owner.AddNatureRatio(tempNatureType, (int)c, Model.Notify);
            }
        }

        private void UnEffect()
        {
            if (effected)
            {
                effected = false;
                owner.AddNatureRatio(tempNatureType, (int)c * -1);
            }
        }
    }
}

