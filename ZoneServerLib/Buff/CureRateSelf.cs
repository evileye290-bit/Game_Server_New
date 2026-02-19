using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class CureRateSelf : BaseBuff
    {
        public CureRateSelf(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Update(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }

            elapsedTime = 0;

            owner.DoCure(owner, (long)(owner.GetNatureValue(NatureType.PRO_MAX_HP) * (c * 0.0001f)), Model.DispatchCureSKillMsg);
        }
    }
}
