using System;
using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class RemoveMarkTriHdl : BaseTriHdl
    {
        private readonly int markId;
        public RemoveMarkTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out markId))
            {
                Log.Warn("in remove mark tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner.RemoveMark(markId);
        }
    }
}
