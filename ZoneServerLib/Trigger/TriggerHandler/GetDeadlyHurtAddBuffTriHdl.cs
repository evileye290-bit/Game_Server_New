using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class GetDeadlyHurtAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId = 0;
        public GetDeadlyHurtAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out buffId))
            {
                Log.Warn("init GetDeadlyHurtAddBuffTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.GetDeadlyHurt, out param))
            {
                return;
            }
            FieldObject fieldObject = param as FieldObject;
            if (fieldObject == null)
            {
                return;
            }
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            fieldObject.AddBuff(Owner, buffId, skillLevelGrowth);
        }
    }
}
