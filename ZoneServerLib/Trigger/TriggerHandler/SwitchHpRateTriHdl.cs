using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class SwitchHpRateTriHdl : BaseTriHdl
    {
        private readonly int heroId = 0;
        public SwitchHpRateTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out heroId))
            {
                Log.Warn("in SwitchHpRate tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            PlayerChar player = Owner as PlayerChar;
            if (player == null)
            {
                player = Owner.GetOwner() as PlayerChar;
                if (player == null||player.IsDead)
                {
                    return;
                }
            }

            Hero hero = GetHero(player);
            if (hero != null)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();

                float heroRate = hero.GetHp() *1f/ hero.GetMaxHp();
                float myRate = Owner.GetHp() *1f / Owner.GetMaxHp();

                long addhp = (long)(Owner.GetNatureValue(NatureType.PRO_MAX_HP) * (heroRate - myRate));

                Owner.AddNatureBaseValue(NatureType.PRO_HP, addhp);
                hero.AddNatureBaseValue(NatureType.PRO_HP, (long)(hero.GetNatureValue(NatureType.PRO_MAX_HP) * (myRate - heroRate)));
                Owner.CheckDead();
                hero.CheckDead();
                Owner.BroadCastHp();
                hero.BroadCastHp();

                Owner.CurrentMap.RecordBattleDataHurt(hero, Owner, BattleDataType.Cure, addhp);
            }
        }

        private Hero GetHero(PlayerChar player)
        {
            return player.HeroMng.GetHero(heroId);
        }
    }
}
