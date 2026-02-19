using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个类型的技能成功命中target
    public class SkillTypeHitCountTriCon : BaseTriCon
    {
        private readonly int skillType, count;
        public SkillTypeHitCountTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] strParam = conditionParam.Split(':');

            if (strParam.Length != 2 || !int.TryParse(strParam[0], out skillType) || !int.TryParse(strParam[1], out count))
            {
                Log.Warn($"init SkillTypeHitCountTriCon failed: invalid info {conditionParam}");
            }
        }

        public override bool Check()
        {
            string key = TriggerParamKey.BuildSkillTypeHitCountKey(skillType);
            object param;
            int count = 0;
            if (!trigger.TryGetParam(key, out param) || !int.TryParse(param.ToString(), out count))
            {
                return false;
            }

            return count>= this.count;
        }
    }
}
