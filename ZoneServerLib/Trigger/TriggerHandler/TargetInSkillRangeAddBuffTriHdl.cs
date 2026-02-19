using CommonUtility;

namespace ZoneServerLib
{
    public class TargetInSkillRangeAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId;

        public TargetInSkillRangeAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            buffId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.TargetInSkillRange, out param))
            {
                return;
            }
            TargetInSkillRangeMsg msg = param as TargetInSkillRangeMsg;
            if (msg == null || msg.Target == null)
            {
                return;
            }
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            foreach (var target in msg.Target)
            {
                target.AddBuff(trigger.Caster, buffId, skillLevelGrowth);
            }

            SetThisFspHandled();
        }
    }
}
