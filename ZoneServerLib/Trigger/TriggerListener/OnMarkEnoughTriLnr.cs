using CommonUtility;

namespace ZoneServerLib
{
    public class OnMarkEnoughTriLnr : BaseTriLnr
    {
        private int markId;
        public OnMarkEnoughTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            if (!int.TryParse(message.ToString(), out markId))
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.BuildMarkEnoughKey(markId), message);
        }

    }
}
