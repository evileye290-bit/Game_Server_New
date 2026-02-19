using CommonUtility;
using System;

namespace ZoneServerLib
{
    public class OnSkillEffTriLnr : BaseTriLnr
    {
        private int skillId;
        public OnSkillEffTriLnr(BaseTrigger trigger, TriggerMessageType messageType) 
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            if(!int.TryParse(message.ToString(), out skillId))
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.BuildEffSkillKey(skillId), message);
        }

    }
}
