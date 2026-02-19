using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class OnCheckTargetInSkillRangeTriLnr : BaseTriLnr
    {
        public OnCheckTargetInSkillRangeTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.TargetInSkillRange, message);
        }
    }
}
