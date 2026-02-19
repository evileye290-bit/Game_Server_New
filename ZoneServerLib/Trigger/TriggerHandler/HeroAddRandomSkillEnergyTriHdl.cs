using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class HeroAddRandomSkillEnergyTriHdl : BaseTriHdl
    {
        private readonly int energy = 0;
        public HeroAddRandomSkillEnergyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out energy))
            {
                Log.Warn("in hero add random skill energy tri hdl: invalid param {0}", handlerParam);
                return;
            }
            energy = trigger.CalcParam(handlerType, energy);
        }

        public override void Handle()
        {
            PlayerChar player = Owner as PlayerChar;
            if (player == null)
            {
                return;
            }
            int heroId = trigger.GetFixedParam_HeroId();
            Hero hero = player.HeroMng.GetHero(heroId);
            if (hero == null)
            {
                return;
            }
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            int index = RAND.Range((int)SkillType.Normal_Skill_1, (int)SkillType.Normal_Skill_4);
            SkillType skillType = (SkillType)index;
            hero.SkillManager.AddEnergy(skillType, energy, true, true, true);
        }
    }
}
