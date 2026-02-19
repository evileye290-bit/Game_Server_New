using CommonUtility;

namespace ZoneServerLib
{
    public class StartFightTriLnr : BaseTriLnr
    {
        public StartFightTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.StartFight, message);
        }

    }
}
