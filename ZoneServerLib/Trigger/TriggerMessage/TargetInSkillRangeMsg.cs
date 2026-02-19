using System.Collections.Generic;

namespace ZoneServerLib
{
    public class TargetInSkillRangeMsg
    {
        public int SkillId { get; private set; }
        public readonly List<FieldObject> Target;

        public TargetInSkillRangeMsg(List<FieldObject> target, int skillId)
        {
            this.Target = target;
            this.SkillId = skillId;
        }
    }
}
