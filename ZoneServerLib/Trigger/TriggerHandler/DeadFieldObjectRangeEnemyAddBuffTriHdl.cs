using System;
using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class DeadFieldObjectRangeEnemyAddBuffTriHdl : BaseTriHdl
    {
        private readonly float range;
        private readonly int buffId = 0;
        public DeadFieldObjectRangeEnemyAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 2)
            {
                Log.Warn("init KillEnemyRangeEnemyAddBuffTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if (!float.TryParse(paramArr[0], out range) || !int.TryParse(paramArr[1], out buffId))
            {
                Log.Warn("init KillEnemyRangeEnemyAddBuffTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
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
            Vec2 center = dead.Position;
            List<FieldObject> targetList = new List<FieldObject>();
            Owner.GetEnemyInSplash(Owner, SplashType.Circle, center, new Vec2(0, 0), range, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                target.AddBuff(Owner, buffId, skillLevelGrowth);
            }
            SetThisFspHandled();
        }
    }
}
