using CommonUtility;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class CureByOwnerNature : BaseBuff
    {
        NatureType natureType;
        public CureByOwnerNature(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            natureType = (NatureType)x;
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
                long value = (long)(target.GetNatureValue(natureType) * (n * 0.0001f));
                target.DoCure(caster, value, Model.DispatchCureSKillMsg);
            }
        }
    }
}

