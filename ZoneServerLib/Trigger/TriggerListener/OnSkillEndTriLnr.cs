using CommonUtility;

namespace ZoneServerLib
{
    public class OnSkillEndTriLnr : BaseTriLnr
    {
        private int skillId;
        public OnSkillEndTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            if (!int.TryParse(message.ToString(), out skillId))
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.BuildEndedSkillKey(skillId), message);
        }

    }
}
