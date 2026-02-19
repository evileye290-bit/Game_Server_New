using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class RemoveSkillTriHdl : BaseTriHdl
    {
        private readonly int skillId;
        public RemoveSkillTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out skillId))
            {
                Log.Warn("in remove mark tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            Owner.SkillEngine.RemoveSkill(skillId);
        }
    }
}
