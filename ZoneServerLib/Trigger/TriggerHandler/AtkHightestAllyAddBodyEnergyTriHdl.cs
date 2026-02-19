using CommonUtility;
using Logger;
using System.Linq;

namespace ZoneServerLib
{
    public class AtkHightestAllyAddBodyEnergyTriHdl : BaseTriHdl
    {
        private readonly int energy;
        public AtkHightestAllyAddBodyEnergyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if(!int.TryParse(handlerParam, out energy))
            { 
                Log.Warn("init AttkHightestAllAddEnergyTriHdl failed, invalid handler param {0}", handlerParam);
            }
        }

        public override void Handle()
        {
            Hero hero = Owner.CurrentMap?.HeroList.Values.Where(x => x.InstanceId != Owner.InstanceId && x.IsAlly(Owner)).OrderByDescending(x => x.GetNatureValue(NatureType.PRO_ATK)).FirstOrDefault();
            if (hero == null) return;

            if (ThisFpsHadHandled(hero)) return;
            SetThisFspHandled(hero);

            hero.SkillManager.AddBodyEnergy(energy, true, true);
        }
    }
    
}
