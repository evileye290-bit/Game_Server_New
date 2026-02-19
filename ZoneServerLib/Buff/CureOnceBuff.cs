using CommonUtility;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class CureOnceBuff : BaseBuff
    {
        public CureOnceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (happened) return;
            List<FieldObject> targetList = new List<FieldObject>();
            owner.GetAllyInSplash(owner, SplashType.Circle, owner.Position, new Vec2(), r, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                target.DoCure(caster, n, Model.DispatchCureSKillMsg);
            }
            happened = true;
            isEnd = true;
        }
    }
}
