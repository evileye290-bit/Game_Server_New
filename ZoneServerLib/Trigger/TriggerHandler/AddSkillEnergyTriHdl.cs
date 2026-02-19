using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AddSkillEnergyTriHdl : BaseTriHdl
    {
        private readonly int skillType = 0;
        private readonly int energy = 0;
        public AddSkillEnergyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if(paramArr.Length != 2)
            {
                Log.Warn("in add skill energy tri hdl: invalid param {0}", handlerParam);
                return;
            }
            if(!int.TryParse(paramArr[0], out skillType) || !int.TryParse(paramArr[1], out energy))
            {
                Log.Warn("in add skill energy tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            Owner.SkillManager.AddEnergy((SkillType)skillType, energy, true, true, true);
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id{Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
