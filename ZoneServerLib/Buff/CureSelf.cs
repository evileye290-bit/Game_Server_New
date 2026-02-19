using ServerModels;

namespace ZoneServerLib
{
    public class CureSelf : BaseBuff
    {
        public CureSelf(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
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
            owner.DoCure(caster, n, Model.DispatchCureSKillMsg);
        }
    }
}
