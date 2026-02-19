using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnhanceCastedControlBuffTimeTriHdl : BaseTriHdl
    {
        private readonly float time;
        public EnhanceCastedControlBuffTimeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!float.TryParse(handlerParam, out time))
            {
                Log.Error($"EnhanceBuffTimeTriHdl error, handlerParam {handlerParam}");
            }
        }

        public override void Handle()
        {
            if(ThisFpsHadHandled()) return;
            
            object param;
            trigger.TryGetParam(TriggerParamKey.CastControlledBuff, out param);
            if(param == null) return;
            
            BaseBuff buff= param as BaseBuff;
            buff?.AddTime(time);
            
            SetThisFspHandled();

#if DEBUG
            Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
