using System;
using System.Collections.Generic;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class HeroAddBuffTrlHdl : BaseTriHdl
    {
        private readonly int heroId = 0;
        private readonly int buffId = 0;

        public HeroAddBuffTrlHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length < 2)
            {
                Log.Warn("in hero add buff tri hdl: invalid param failed, invalid handler param {0}", handlerParam);
                return;
            }

            if (!int.TryParse(paramArr[0], out heroId))
            {
                Log.Warn("in hero add buff tri hdl: invalid param failed, invalid hero Id {0}", heroId);
                return;
            }

            if (!int.TryParse(paramArr[1], out buffId))
            {
                Log.Warn("in hero add buff tri hdl: invalid param failed, invalid buff Id {0}", buffId);
                return;
            }

        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            Vec2 center = Owner.Position;
            List<FieldObject> targetList = new List<FieldObject>();
            Owner.GetAllyInSplash(Owner, SplashType.Circle, center, new Vec2(0, 0), 999, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                if (target.GetHeroId() == heroId)
                {
                    target.AddBuff(Owner, buffId, skillLevelGrowth);
                    return;
                }
            }
        }
    }
}
