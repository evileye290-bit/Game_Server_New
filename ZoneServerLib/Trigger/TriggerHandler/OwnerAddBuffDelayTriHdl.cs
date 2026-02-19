using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class OwnerAddBuffDelayTriHdl : BaseTriHdl
    {
        private readonly int buffId;
        private readonly float delayTime;
        public OwnerAddBuffDelayTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] strArr = handlerParam.Split('|');
            if(strArr.Length < 2)
            {
                Log.Warn("create owner add buff delay failed: invalid param {0}", handlerParam);
                return;
            }
            if (!int.TryParse(strArr[0], out buffId) || !float.TryParse(strArr[1], out delayTime))
            {
                Log.Warn("create owner add buff delay failed: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            Owner.AddBuffDelay(Owner, buffId, skillLevelGrowth, delayTime);
        }
    }
}
