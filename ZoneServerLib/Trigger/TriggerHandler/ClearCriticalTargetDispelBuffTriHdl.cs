using CommonUtility;

namespace ZoneServerLib
{
    public class ClearCriticalTargetDispelBuffTriHdl : BaseTriHdl
    {
        public ClearCriticalTargetDispelBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.Critical, out param))
            {
                return;
            }
            CriticalTriMsg msg = param as CriticalTriMsg;
            if (msg == null || msg.Target == null || msg.Model == null)
            {
                return;
            }
            msg.Target.BuffManager.CleanAllDispelBuff();
            SetThisFspHandled();
        }
    }
}
