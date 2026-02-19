using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class OnTriggerTriggeredTriLnr : BaseTriLnr
    {
        public OnTriggerTriggeredTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            var msg = message as Tuple<int, FieldObject>;
            if (msg != null)
            {
                trigger.RecordParam(TriggerParamKey.BuildTriggerTriggerdKey(msg.Item1), msg.Item2);
            }
        }
    }
}
