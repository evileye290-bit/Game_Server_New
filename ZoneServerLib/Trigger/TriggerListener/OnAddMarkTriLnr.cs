using CommonUtility;

namespace ZoneServerLib
{
    public class OnAddMarkTriLnr : BaseTriLnr
    {
        private int markId;
        public OnAddMarkTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            if (!int.TryParse(message.ToString(), out markId))
            {
                return;
            }

            trigger.RecordParam(TriggerParamKey.BuildAddMarkKey(markId), message);
        }

    }
}
