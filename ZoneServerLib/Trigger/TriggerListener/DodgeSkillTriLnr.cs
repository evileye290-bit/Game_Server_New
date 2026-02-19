using CommonUtility;

namespace ZoneServerLib
{
    public class DodgeSkillTriLnr : BaseTriLnr
    {
        public DodgeSkillTriLnr(BaseTrigger trigger, TriggerMessageType messageType)
            : base(trigger, messageType)
        {
        }

        protected override void ParseMessage(object message)
        {
            Skill param = message as Skill;
            if (param == null)
            {
                return;
            }

            trigger.RecordParam(TriggerParamKey.DodgeSkill, param);
            trigger.AddCounter(TriggerCounter.Dodge);
        }

    }
}
