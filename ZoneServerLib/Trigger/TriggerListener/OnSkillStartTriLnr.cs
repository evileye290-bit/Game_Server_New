using CommonUtility;

namespace ZoneServerLib
{
    public class OnSkillStartTriLnr : BaseTriLnr
    {
        private int skillId;
        public OnSkillStartTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            if (!int.TryParse(message.ToString(), out skillId))
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.BuildStartedSkillKey(skillId), message);
        }

    }
}
