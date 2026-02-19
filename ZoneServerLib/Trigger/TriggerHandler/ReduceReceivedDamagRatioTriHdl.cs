using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ReduceReceivedDamagRatioTriHdl : BaseTriHdl
    {
        private readonly int ratio;

        public ReduceReceivedDamagRatioTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out ratio))
            {
                Log.Warn("init ReduceReceivedDamagRatioTriHdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.OnceDamage, out param))
            {
                return;
            }

            DamageTriMsg msg = param as DamageTriMsg;
            if (msg != null)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();

            }
        }
    }
}
