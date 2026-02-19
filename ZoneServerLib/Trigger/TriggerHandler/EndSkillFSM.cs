using CommonUtility;
using EnumerateUtility;
using Logger;

namespace ZoneServerLib
{
    public class EndSkillFSM : BaseTriHdl
    {
        private int skillId = 0;
        public EndSkillFSM(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out skillId))
            {
                Log.Warn("in EndSkillFSM tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (Owner.FsmManager.CurFsmState.FsmStateType == FsmStateType.SKILL)
            {
                FsmSkillState skillFSM = Owner.FsmManager.CurFsmState as FsmSkillState;
                if (skillFSM != null && skillFSM.Skill.Id == skillId)
                {
                    skillFSM.LeftTime = 0;
                }
            }
        }
    }
}
