using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class DeadFieldObjectRangeEnemyDoDamageTriHdl : BaseTriHdl
    {
        private readonly float range, growth;
        private readonly int damage = 0;

        public DeadFieldObjectRangeEnemyDoDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 3)
            {
                Log.Warn("init RangeEnemyDoDamageTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if (!float.TryParse(paramArr[0], out range) || !float.TryParse(paramArr[1], out growth) || !int.TryParse(paramArr[2], out damage))
            {
                Log.Warn("init RangeEnemyDoDamageTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object obj;
            if (!trigger.TryGetParam(TriggerParamKey.FieldObjectDead, out obj))
            {
                return;
            }

            FieldObject dead = obj as FieldObject;
            if (dead == null) return;

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            Vec2 center = Owner.Position;
            List<FieldObject> targetList = new List<FieldObject>();
            Owner.GetEnemyInSplash(trigger.Owner, SplashType.Circle, center, new Vec2(0, 0), range, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                target.DoSpecDamage(Owner, DamageType.Skill, (int)trigger.CalcParam(growth, damage, skillLevelGrowth));
            }
            SetThisFspHandled();
        }
    }
}
