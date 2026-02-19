using CommonUtility;

namespace ZoneServerLib
{
    public class OnAnySkillHitTriLnr : BaseTriLnr
    {
        public OnAnySkillHitTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            SkillHitMsg param = message as SkillHitMsg;
            if (param == null)
            {
                return;
            }

            trigger.RecordParam(TriggerParamKey.AnySkillHit, param);
        }

    }
}
