using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class FleeNextDamageTriHdl : BaseTriHdl
    {
        int count = 0;
        public FleeNextDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out count))
            {
                Log.Warn("in FleeNextDamageTriHdl tri hdl: invalid param {0}", handlerParam);
                return;
            }
            count = trigger.CalcParam(handlerType, count);
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner.AddFleeCount(count);
        }
    }
}
