using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AddTriggerTriHdl : BaseTriHdl
    {
        private readonly int triggerId = 0;
        public AddTriggerTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out triggerId))
            {
                Log.Warn("init add trigger tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            Owner.AddTriggerCreatedBySkill(triggerId, 1, Owner);
        }
    }
}
