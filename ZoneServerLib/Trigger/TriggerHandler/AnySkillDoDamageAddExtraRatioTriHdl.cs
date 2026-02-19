using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ControlTargetAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId;
        public ControlTargetAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out buffId))
            {
                Log.Warn("init ControlTargetAddBuffTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.CastControlledBuff, out param))
            {
                return;
            }

            BaseBuff buff = param as BaseBuff;
            if (buff == null || buff.Owner == null)
            {
                return;
            }
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            buff.Owner.AddBuff(Owner, buffId, skillLevelGrowth);

            SetThisFspHandled();
        }
    }
}
