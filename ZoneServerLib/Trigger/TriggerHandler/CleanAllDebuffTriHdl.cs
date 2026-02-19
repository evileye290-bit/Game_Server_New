using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CleanAllDebuffTriHdl : BaseTriHdl
    {
        public CleanAllDebuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            if (Owner == null) return;
            Owner.BuffManager.CleanAllDebuff();

            SetThisFspHandled();
        }
    }
}
