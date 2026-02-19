using ServerModels;

namespace ZoneServerLib
{
    public class AddNatureRatioWhileHaveNotDebuffBuff : BaseBuff
    {
        private bool effected = false;
        public AddNatureRatioWhileHaveNotDebuffBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (!owner.BuffManager.HaveDeBuff())
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
            if (!owner.BuffManager.HaveDeBuff())
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
                owner.AddNatureRatio(buffModel.NatureType, (int)buffModel.NatureRatio, Model.Notify);
            }
        }

        private void UnEffect()
        {
            if (effected)
            {
                effected = false;
                owner.AddNatureRatio(buffModel.NatureType, (int)buffModel.NatureRatio * -1);
            }
        }
    }
}
