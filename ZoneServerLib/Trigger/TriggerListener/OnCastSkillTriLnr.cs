using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class OnCastSkillTriLnr : BaseTriLnr
    {
        public OnCastSkillTriLnr(BaseTrigger trigger, TriggerMessageType messageType) 
            : base(trigger, messageType)
        {
        }
    }
}
