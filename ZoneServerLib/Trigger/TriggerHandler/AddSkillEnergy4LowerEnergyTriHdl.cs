using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class AddSkillEnergy4LowerEnergyTriHdl : BaseTriHdl
    {
        private readonly int skillType = 0;
        private readonly int energy = 0;
        public AddSkillEnergy4LowerEnergyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 2)
            {
                Log.Warn("in add skill energy tri hdl: invalid param {0}", handlerParam);
                return;
            }
            if (!int.TryParse(paramArr[0], out skillType) || !int.TryParse(paramArr[1], out energy))
            {
                Log.Warn("in add skill energy tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            Hero rightOne = null;
            int tempEnergy = int.MaxValue;
            foreach (var hero in Owner.CurrentMap.HeroList.Where(x=>x.Value.IsAlly(Owner)))
            {
                int temp= hero.Value.SkillManager.GetEnergy((SkillType)skillType);
                if (temp < tempEnergy)
                {
                    rightOne = hero.Value;
                    tempEnergy = temp;
                }
            }

            rightOne?.SkillManager.AddEnergy((SkillType)skillType, energy, true, true, true);


#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id{Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
