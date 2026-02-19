using CommonUtility;

namespace ZoneServerLib
{
    public class UpBuffPileLimitTriHdl : BaseTriHdl
    {
        private readonly int buffId, addNum;

        public UpBuffPileLimitTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            var paramList = handlerParam.ToList(':');
            if (paramList.Count != 2)
            {
                Logger.Log.Error($"UpBuffPileLimitTriHdl param error {handlerParam}");
                return;
            }
            buffId = paramList[0];
            addNum = paramList[1];
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled())
            {
                return;
            }

            var buff = Owner.BuffManager.GetBuff(buffId);
            if (buff == null || buff.Model.OverlayType != BuffOverlayType.PileById)
            {
                return;
            }

            buff.AddMaxPileNum(addNum);
            SetThisFspHandled();
        }
    }
}