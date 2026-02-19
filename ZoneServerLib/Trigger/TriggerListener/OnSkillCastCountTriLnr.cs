using CommonUtility;

namespace ZoneServerLib
{
    public class OnSkillCastCountTriLnr : BaseTriLnr
    {
        public OnSkillCastCountTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            Skill skill = (Skill)message;
            if (skill == null) return;

            int count = trigger.GetParam_SkillCastCount(skill.Id);
            trigger.RecordParam(TriggerParamKey.BuildSkillCastCount(skill.Id), ++count);

            count = trigger.GetParam_SkillTypeCastCount((int)skill.SkillModel.Type);
            trigger.RecordParam(TriggerParamKey.BuildSkillTypeCastCount((int)skill.SkillModel.Type), ++count);
        }
    }
}
