using CommonUtility;

namespace ZoneServerLib
{
    public class OnNormalSkillStartTriLnr : BaseTriLnr
    {
        private int skillId;
        public OnNormalSkillStartTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            if (!int.TryParse(message.ToString(), out skillId))
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.BuildNormalSkillStartKey(skillId), message);
        }

    }
}
