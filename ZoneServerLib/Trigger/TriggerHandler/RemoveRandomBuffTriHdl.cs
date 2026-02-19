using CommonUtility;

namespace ZoneServerLib
{
    class RemoveRandomBuffTriHdl : BaseTriHdl
    {
        public RemoveRandomBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner?.RemoveRandomBuff();
        }
    }
}
