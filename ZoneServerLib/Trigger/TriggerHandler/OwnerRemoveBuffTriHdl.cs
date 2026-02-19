using CommonUtility;

namespace ZoneServerLib
{
    public class OwnerRemoveBuffTriHdl : BaseTriHdl
    {
        readonly int buffId;
        public OwnerRemoveBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            buffId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner.BuffManager.RemoveBuffsById(buffId);
        }
    }
}
