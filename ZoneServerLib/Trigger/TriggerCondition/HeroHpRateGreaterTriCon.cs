using CommonUtility;

namespace ZoneServerLib
{
    class HeroHpRateGreaterTriCon : BaseTriCon
    {
        readonly float rate = 0;
        readonly int heroId = 0;
        public HeroHpRateGreaterTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam) 
            : base(trigger, conditionType, conditionParam)
        {
            string[] param= conditionParam.Split(':');
            int temp= int.Parse(param[0]);
            temp = trigger.CalcParam(conditionType, temp);
            rate = temp * 0.0001f;
            heroId = int.Parse(param[1]);
        }

        public override bool Check()
        {
            PlayerChar player = owner as PlayerChar;
            if (player == null)
            {
                player = owner.GetOwner() as PlayerChar;
                if (player == null)
                {
                    return false;
                }
            }

            Hero hero = GetHero(player);
            if (hero != null)
            {
                return hero.GetHp() > owner.GetMaxHp() * rate;
            }
            return false;
        }

        private Hero GetHero(PlayerChar player)
        {
            return player.HeroMng.GetHero(heroId);
        }
    }
}
