using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class SkillTypeHitCasterAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId = 0;
        public SkillTypeHitCasterAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out buffId))
            {
                Log.Warn("init skill type hit caster add buff tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            Owner.AddBuff(Owner, buffId, skillLevelGrowth);
        }
    }
}
