using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class MonsterDeadTriCon : BaseTriCon
    {
        readonly int zoneMonsterId;
        public MonsterDeadTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out zoneMonsterId))
            {
                Log.Warn("in monster dead tri con: invalid param {0}", conditionParam);
                return;
            }
        }

        public override bool Check()
        {
            object deadObject;
            if (!trigger.TryGetParam(TriggerParamKey.Dead, out deadObject))
            {
                return false;
            }

            Monster monster = deadObject as Monster;
            return monster != null && monster.Generator.Id == zoneMonsterId;
        }
    }
}
