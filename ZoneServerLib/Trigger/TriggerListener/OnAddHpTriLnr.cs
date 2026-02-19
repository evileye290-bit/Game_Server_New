using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;

namespace ZoneServerLib{

    public class OnAddHpTriLnr :BaseTriLnr
    {
        public OnAddHpTriLnr(BaseTrigger trigger, TriggerMessageType messageType) 
            : base(trigger, messageType)
        {
        }

    }
}
