using System;

namespace ZoneServerLib
{
    public class SkillReadyInfo
    {
        public Skill Skill { get; private set; }
        public int Priority { get; private set; }

        public BaseTrigger Trigger { get; private set; }
        public DateTime ReadyTime { get; private set; }
        public int SkillId { get; private set; }

        public SkillReadyInfo(Skill skill, BaseTrigger trigger, DateTime readyTime)
        {
            Skill = skill;
            SkillId = skill.Id;
            Priority = skill.Priority;
            Trigger = trigger;
            ReadyTime = readyTime;
        }
    }
}
