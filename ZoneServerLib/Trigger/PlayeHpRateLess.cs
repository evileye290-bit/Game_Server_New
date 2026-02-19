using CommonUtility;

namespace ZoneServerLib
{
    class PlayerHpRateLessTriCon : BaseTriCon
    {
        readonly float rate = 0;
        public PlayerHpRateLessTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            rate = int.Parse(conditionParam) * 0.0001f;
        }

        public override bool Check()
        {
            PlayerChar player = owner as PlayerChar;
            if (player == null)
            { 
                return false;
            }

            return player.GetHp() < player.GetMaxHp() * rate;
        }
    }
}
