using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class EnhanceIdTypeSkillDamageRatioTriHdl : BaseTriHdl
    {
        private readonly int ratio;
        private readonly SkillType skillType;

        public EnhanceIdTypeSkillDamageRatioTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            List<int> paramArr = handlerParam.ToList(':');
            if (paramArr.Count != 3)
            {
                Log.Warn("init EnhanceIdTypeSkillDamageRatioTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            int skillLevel = trigger.GetFixedParam_SkillLevelGrowth();
            skillType = (SkillType) paramArr[0];
            ratio = paramArr[1] * skillLevel + paramArr[2];
        }

        public override void Handle()
        {
            Owner?.SkillManager?.AddSkillEnhanceDamageRatio(skillType, ratio);
        }
    }
}
