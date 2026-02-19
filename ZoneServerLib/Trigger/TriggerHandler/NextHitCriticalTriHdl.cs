using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class NextHitCriticalTriHdl : BaseTriHdl
    {
        int count = 0;
        public NextHitCriticalTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out count))
            {
                Log.Warn("in next hit critical tri hdl: invalid param {0}", handlerParam);
                return;
            }
            count = trigger.CalcParam(handlerType, count);
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner.NextCriticalHitCount += count;
        }
    }
}
