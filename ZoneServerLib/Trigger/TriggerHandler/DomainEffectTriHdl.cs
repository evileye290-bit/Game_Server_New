using CommonUtility;

namespace ZoneServerLib
{

    public class DomainEffectTriHdl : BaseTriHdl
    {
        readonly int domainId;
        public DomainEffectTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            domainId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;

            trigger.CurMap?.DomainEffect(Owner, domainId, trigger.GetFixedParam_SkillLevel(), trigger.GetFixedParam_SkillLevelGrowth());
            SetThisFspHandled();
        }
    }
}
