using CommonUtility;

namespace ZoneServerLib
{
    public class OnCastBuffTypeTriLnr : BaseTriLnr
    {
        public OnCastBuffTypeTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            BaseBuff param = message as BaseBuff;
            if (param != null)
            {
                trigger.RecordParam(TriggerParamKey.BuildCastBuffTypeKey((int)param.BuffType), message);
            }
        }

    }
}
