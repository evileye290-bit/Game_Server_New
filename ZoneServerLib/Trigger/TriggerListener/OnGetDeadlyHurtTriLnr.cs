using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class OnGetDeadlyHurtTriLnr : BaseTriLnr
    {
        public OnGetDeadlyHurtTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
            trigger.Owner.GetDeadlyHurt = true;
        }

        protected override void ParseMessage(object message)
        {
            FieldObject fieldObject = message as FieldObject;
            if (fieldObject == null)
            {
                return;
            }
            trigger.RecordParam(TriggerParamKey.GetDeadlyHurt, fieldObject);
        }
    }
}
