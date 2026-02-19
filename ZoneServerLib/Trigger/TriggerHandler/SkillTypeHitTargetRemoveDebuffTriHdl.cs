using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class SkillTypeHitTargetRemoveDebuffTriHdl : BaseTriHdl
    {
        private readonly int skillType;
        public SkillTypeHitTargetRemoveDebuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if ( !int.TryParse(handlerParam, out skillType))
            {
                Log.Warn("init skill type hit target remove debuff tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildSkillTypeHitKey(skillType), out param))
            {
                return;
            }
            SkillHitMsg msg = param as SkillHitMsg;
            if (msg == null || (int)(msg.Model.Type) != skillType || msg.TargetList == null || msg.TargetList.Count == 0)
            {
                return;
            }
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            foreach (var target in msg.TargetList)
            {
                target.RemoveRandomDebuff();
            }
        }
    }
}
