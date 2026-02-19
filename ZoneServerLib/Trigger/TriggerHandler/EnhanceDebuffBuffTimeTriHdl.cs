using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnhanceDebuffBuffTimeTriHdl : BaseTriHdl
    {
        private readonly float time;
        public EnhanceDebuffBuffTimeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!float.TryParse(handlerParam, out time))
            {
                Log.Error($"EnhanceDebuffBuffTimeTriHdl error, handlerParam {handlerParam}");
            }
        }

        public override void Handle()
        {
            trigger.Owner?.BuffManager.EnhanceDebuffBuffTime(time);
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
