using CommonUtility;

namespace ZoneServerLib
{
    public class UpMarkLimitTriHdl : BaseTriHdl
    {
        private readonly int markId, addNum;

        public UpMarkLimitTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            var paramList = handlerParam.ToList(':');
            if (paramList.Count != 2)
            {
                Logger.Log.Error($"UpMarkLimitTriHdl param error {handlerParam}");
                return;
            }
            markId = paramList[0];
            addNum = paramList[1];
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled())
            {
                return;
            }

            Mark mark = Owner.MarkManager.GetMark(markId);
            if (mark == null)
            {
                return;
            }

            mark.AddMaxMarkCount(addNum);
            SetThisFspHandled();
        }
    }
}