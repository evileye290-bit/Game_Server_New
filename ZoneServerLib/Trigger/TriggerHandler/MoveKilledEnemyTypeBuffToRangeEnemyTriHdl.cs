using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class MoveKilledEnemyTypeBuffToRangeEnemyTriHdl : BaseTriHdl
    {
        private readonly float range;
        private readonly BuffType buffType;

        public MoveKilledEnemyTypeBuffToRangeEnemyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int buff;
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 2 || !float.TryParse(paramArr[0], out range) || !int.TryParse(paramArr[1], out buff))
            {
                Log.Warn("init MoveTypeBuffToRangeEnemyTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            buffType = (BuffType)buff;
        }

        public override void Handle()
        {
            object obj;
            if (!trigger.TryGetParam(TriggerParamKey.FieldObjectDead, out obj))
            {
                return;
            }

            FieldObject dead = obj as FieldObject;
            if (dead == null) return;

            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            List<BaseBuff> buffs = dead.BuffManager.GetBuffsByType(buffType);

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            Vec2 center = dead.Position;
            List<FieldObject> targetList = new List<FieldObject>();
            Owner.GetEnemyInSplash(Owner, SplashType.Circle, center, new Vec2(0, 0), range, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                if (target.IsDead) continue;

                buffs.ForEach(x => target.AddBuff(x.Caster, x.Id, skillLevelGrowth));
            }
        }
    }
}
