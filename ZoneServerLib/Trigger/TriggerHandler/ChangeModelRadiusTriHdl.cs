using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class ChangeModelRadiusTriHdl : BaseTriHdl
    {
        private readonly int radius = 0;
        public ChangeModelRadiusTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out radius))
            {
                Log.Warn("in change model radius tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            Owner.SetRadius(radius);

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
