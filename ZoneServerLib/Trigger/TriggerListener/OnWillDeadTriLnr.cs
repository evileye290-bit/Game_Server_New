using CommonUtility;
using EnumerateUtility;

namespace ZoneServerLib
{
    public class OnWillDeadTriLnr : BaseTriLnr
    {
        public OnWillDeadTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
            if (trigger.Owner.CastSkillThenDeadSkillId < 0)
            {
                trigger.Owner.CastSkillThenDeadSkillId = 0;
            }
        }

        protected override void ParseMessage(object message)
        {
            FieldObject fieldObject = message as FieldObject;
            if (fieldObject == null)
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.WillDead, fieldObject);
        }
    }
}
