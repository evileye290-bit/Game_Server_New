using System;
using CommonUtility;

namespace ZoneServerLib
{
    public class SkillEffectMustHitTriHdl : BaseTriHdl
    {
        private readonly int skillEffectId;

        public SkillEffectMustHitTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            skillEffectId = Convert.ToInt32(handlerParam);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled())
            {
                return;
            }

            Owner?.SkillManager.SetMustHitSkillEffect(skillEffectId);

            SetThisFspHandled();
        }
    }
}