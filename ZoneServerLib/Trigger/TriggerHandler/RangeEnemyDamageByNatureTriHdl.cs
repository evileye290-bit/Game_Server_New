using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class RangeEnemyDamageByNatureTriHdl : BaseTriHdl
    {
        private readonly float range;
        private readonly int damage;
        
        public RangeEnemyDamageByNatureTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 4)
            {
                Log.Warn("init RangeEnemyDamageByNatureTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
            
            int nature;
            float growth, baseDamage;

            if (!float.TryParse(paramArr[0], out range)|| !int.TryParse(paramArr[1], out nature) || !float.TryParse(paramArr[2], out growth) || !float.TryParse(paramArr[3], out baseDamage))
            {
                Log.Warn("init RangeEnemyDamageByNatureTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
            
            NatureType natureType = (NatureType) nature;
            int skillLevel = trigger.GetFixedParam_SkillLevelGrowth();
            damage = (int) (trigger.CalcParam(growth, baseDamage, skillLevel) * 0.0001f * Owner.GetNatureValue(natureType));
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
