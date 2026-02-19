using CommonUtility;

namespace ZoneServerLib
{
    public class CriticalDamageCureRatioTriHdl : BaseTriHdl
    {
        private readonly int ratio = 0;
        public CriticalDamageCureRatioTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            ratio = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.Critical, out param)) return;

            CriticalTriMsg msg = param as CriticalTriMsg;
            if (msg == null) return;

            trigger.Owner.AddHp(trigger.Caster, (int)(msg.Damage * 0.0001f * ratio));
            SetThisFspHandled();
        }
    }
}
