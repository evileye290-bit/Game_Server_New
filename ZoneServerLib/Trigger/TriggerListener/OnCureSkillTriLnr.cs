using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class OnCureSkillTriLnr : BaseTriLnr
    {
        public OnCureSkillTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            List<FieldObject> param = message as List<FieldObject>;
            if (param == null)
            {
                return;
            }

            trigger.RecordParam(TriggerParamKey.CureSkill, param);
        }
    }
}
