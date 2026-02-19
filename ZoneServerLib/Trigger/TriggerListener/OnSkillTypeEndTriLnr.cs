using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnSkillTypeEndTriLnr : BaseTriLnr
    {
        public OnSkillTypeEndTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            int skillType;
            if (!int.TryParse(message.ToString(), out skillType))
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.BuildSkillTypeEndKey(skillType), message);
        }

    }
}
