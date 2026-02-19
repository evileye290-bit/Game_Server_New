using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class ReduceMarkNumTriHdl : BaseTriHdl
    {
        private readonly int markId;
        public ReduceMarkNumTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out markId))
            {
                Log.Warn("in reduce mark count tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner.MarkManager.ReduceMarkCount(markId, 1);
        }
    }
}
