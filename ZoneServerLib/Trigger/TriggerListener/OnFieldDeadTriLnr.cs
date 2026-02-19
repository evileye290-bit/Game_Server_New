using CommonUtility;

namespace ZoneServerLib
{
    public class OnFieldDeadTriLnr : BaseTriLnr
    {
        public OnFieldDeadTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            FieldObject fieldObject = message as FieldObject;
            if (fieldObject == null)
            {
                return;
            }

            trigger.RecordParam(TriggerParamKey.FieldObjectDead, fieldObject);
        }
    }
}
