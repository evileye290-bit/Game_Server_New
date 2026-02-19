using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{ 
    class OnSkillDamageTriLnr : BaseTriLnr
    {
        public OnSkillDamageTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            trigger.RecordParam(TriggerParamKey.SkillDamage, message);
        }
    }
}
