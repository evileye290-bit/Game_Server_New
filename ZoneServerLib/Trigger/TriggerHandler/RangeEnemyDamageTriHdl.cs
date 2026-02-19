using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class RangeEnemyDamageTriHdl : BaseTriHdl
    {
        private readonly float range, growth, baseDamage;
        private readonly int damage;
        public RangeEnemyDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 3)
            {
                Log.Warn("init range enemy add damage tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if (!float.TryParse(paramArr[0], out range) || !float.TryParse(paramArr[1], out growth) || !float.TryParse(paramArr[2], out baseDamage))
            {
                Log.Warn("init range enemy add damage tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            damage = (int)trigger.CalcParam(growth, baseDamage, trigger.GetFixedParam_SkillLevelGrowth());
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Vec2 center = Owner.Position;
            List<FieldObject> targetList = new List<FieldObject>();
            Owner.GetEnemyInSplash(Owner, SplashType.Circle, center, new Vec2(0, 0), range, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                target.DoSpecDamage(Owner, DamageType.Skill, damage);
            }
        }
    }
}
