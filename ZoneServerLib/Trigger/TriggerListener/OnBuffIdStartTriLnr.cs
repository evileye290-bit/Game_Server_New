using CommonUtility;

namespace ZoneServerLib
{
    public class OnBuffIdStartTriLnr : BaseTriLnr
    {
        public OnBuffIdStartTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            BuffStartTriMsg param = message as BuffStartTriMsg;
            if (param != null)
            {
                trigger.RecordParam(TriggerParamKey.BuildStartBuffIdKey(param.BuffId), message);
            }
        }

    }
}
