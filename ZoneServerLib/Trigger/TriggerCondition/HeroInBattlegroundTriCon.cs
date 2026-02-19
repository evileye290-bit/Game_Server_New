using CommonUtility;

namespace ZoneServerLib
{
    public class HeroInBattlegroundTriCon : BaseTriCon
    {
        private readonly int heroId;
        public HeroInBattlegroundTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out heroId))
            {
                Logger.Log.Error($"HeroInBattlegroundTriCon error, heroid {heroId}");
                return;
            }
        }

        public override bool Check()
        {
            if (trigger.Owner == null) return false;

            foreach (var kv in trigger.CurMap.HeroList)
            {
                if (kv.Value.HeroId == heroId)
                { 
                    return trigger.Owner.IsAlly(kv.Value);
                }
            }
            return false;
        }
    }
}
