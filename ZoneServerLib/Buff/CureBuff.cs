using CommonUtility;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class CureBuff : BaseBuff
    {
        public CureBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
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
            List<FieldObject> targetList = new List<FieldObject>();
            owner.GetAllyInSplash(owner, SplashType.Circle, owner.Position, new Vec2(), r, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                target.DoCure(caster, n, Model.DispatchCureSKillMsg);
            }
        }
    }
}
