using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class OwnerReplaceTriggerCreateBySoulRingTriHdl : BaseTriHdl
    {
        readonly int replaceId,newId;
        public OwnerReplaceTriggerCreateBySoulRingTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            var paramList = handlerParam.ToList(':');
            if (paramList.Count != 2)
            {
                Log.Warn($"OwnerReplaceTriggerCreateBySoulRingTriHdl param error {handlerParam}");
                return;
            }

            replaceId = paramList[0];
            newId = paramList[1];
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            object level;
            if (base.trigger.TryGetFixedParam(TriggerParamKey.CreatedBySkillLevel, out level))
            {
                if (Owner.RemoveTrigger(replaceId))
                {
                    TriggerCreatedBySoulRing trigger = new TriggerCreatedBySoulRing(Owner, newId, (int)level);
                    Owner.AddTrigger(trigger);
                }
            }
        }
    }
}
