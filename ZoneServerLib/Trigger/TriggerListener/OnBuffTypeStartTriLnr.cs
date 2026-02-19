using CommonUtility;

namespace ZoneServerLib
{
    public class OnBuffTypeStartTriLnr : BaseTriLnr
    {
        public OnBuffTypeStartTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            BuffStartTriMsg param = message as BuffStartTriMsg;
            if (param != null)
            {
                trigger.RecordParam(TriggerParamKey.BuildStartBuffTypeKey(param.BuffType), message);
            }

            int count = trigger.GetParam_BeenAddedTypeBuffCount(param.BuffType);
            trigger.RecordParam(TriggerParamKey.BuildBeenAddedTypeBuffCount(param.BuffType), ++count);
        }

    }
}
