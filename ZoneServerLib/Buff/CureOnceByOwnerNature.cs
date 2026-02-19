using CommonUtility;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class CureOnceByOwnerNature : BaseBuff
    {
        NatureType natureType;
        public CureOnceByOwnerNature(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            natureType = (NatureType)x;
        }

        protected override void Start()
        {
            if (happened) return;
            List<FieldObject> targetList = new List<FieldObject>();
            owner.GetAllyInSplash(owner, SplashType.Circle, owner.Position, new Vec2(), r, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                long value = (long)(target.GetNatureValue(natureType) * (n * 0.0001f));
                target.DoCure(caster, value, Model.DispatchCureSKillMsg);
            }
            happened = true;
            isEnd = true;
        }
    }
}

