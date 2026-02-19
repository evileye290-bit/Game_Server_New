using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class OnCastDebuffTriLnr : BaseTriLnr
    {
        public OnCastDebuffTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            BaseBuff buff = message as BaseBuff;
            if (buff == null)
            {
                return;
            }

            trigger.RecordParam(TriggerParamKey.CastDeBuff, buff);
        }
    }
}
