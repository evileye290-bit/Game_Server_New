using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class SkillTypeCastCountTriCon : BaseTriCon
    {
        private int skillType;
        private int count;
        public SkillTypeCastCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] info = conditionParam.Split(':');
            if (info.Length != 2 || !int.TryParse(info[0], out skillType) || !int.TryParse(info[1], out count))
            {
                Log.Warn($"init SkillTypeCastCountTriCon failed: invalid skill param {conditionParam}");
            }
        }

        public override bool Check()
        {
            return trigger.GetParam_SkillTypeCastCount(skillType) >= count;
        }
    }
}
