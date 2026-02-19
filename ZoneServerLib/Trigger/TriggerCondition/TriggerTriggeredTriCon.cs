using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class TriggerTriggeredTriCon : BaseTriCon
    {
        public TriggerTriggeredTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam) 
            : base(trigger, conditionType, conditionParam)
        {
            //if (!int.TryParse(conditionParam, out triggerId))
            //{
            //    Log.Warn($"init TriggerTriggeredTriCon trigger condition failed: invalid mark id {conditionParam}");
            //}
        }

        public override bool Check()
        {
            object param;

            if (!trigger.TryGetParam(TriggerParamKey.BuildTriggerTriggerdKey(trigger.Model.Id), out param)
                || param == null
                || !(param is FieldObject))
            {
                return false;
            }

            return true;
        }

    }
}
