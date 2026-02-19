using CommonUtility;

namespace ZoneServerLib
{
    public class OwnerRemoveTypeBuffTriHdl : BaseTriHdl
    {
        readonly BuffType buffType;

        public OwnerRemoveTypeBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            buffType = (BuffType)int.Parse(handlerParam);
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner.BuffManager.RemoveBuffsByType(buffType);
        }
    }
}
