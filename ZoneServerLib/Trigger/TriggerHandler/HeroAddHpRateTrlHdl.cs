using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class HeroAddHpRateTrlHdl : BaseTriHdl
    {
        private readonly int rate = 0;
        public HeroAddHpRateTrlHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out rate))
            {
                Log.Warn("in hero add hp rate tri hdl: invalid param {0}", handlerParam);
                return;
            }
            rate = trigger.CalcParam(handlerType, rate);
        }

        public override void Handle()
        {
            PlayerChar player = Owner as PlayerChar;
            if(player == null)
            {
                return;
            }
            int heroId = trigger.GetFixedParam_HeroId();
            Hero hero = player.HeroMng.GetHero(heroId);
            if(hero == null)
            {
                return;
            }
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            long hp = (long)(hero.GetNatureValue(NatureType.PRO_MAX_HP) * (rate * 0.0001f));
            hero.AddHp(trigger.Owner, hp);
        }
    }
}
