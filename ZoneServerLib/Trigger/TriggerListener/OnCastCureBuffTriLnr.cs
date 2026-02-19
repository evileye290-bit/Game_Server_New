using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class OnCastCureBuffTriLnr : BaseTriLnr
    {
        public OnCastCureBuffTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            FieldObject param = message as FieldObject;
            if (param == null)
            {
                return;
            }

            trigger.RecordParam(TriggerParamKey.CastCureBuff, param);
        }
    }
}
