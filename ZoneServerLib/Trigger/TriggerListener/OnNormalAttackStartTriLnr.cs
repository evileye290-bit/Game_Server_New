using CommonUtility;

namespace ZoneServerLib
{

    public class OnNormalAttackStartTriLnr : BaseTriLnr
    {
        private int targetId;
        public OnNormalAttackStartTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            if (!int.TryParse(message.ToString(), out targetId))
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.NormalAttackStart, message);
        }
    }
}
