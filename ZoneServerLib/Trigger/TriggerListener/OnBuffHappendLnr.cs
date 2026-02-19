using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class OnBuffHappendLnr : BaseTriLnr
    {
        public OnBuffHappendLnr(BaseTrigger trigger, TriggerMessageType messageType) 
            : base(trigger, messageType)
        {
        }

    }
}
