using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnoughSkillTypeEnergyTriHdl : BaseTriHdl
    {
        private readonly int skillType = 0;
        public EnoughSkillTypeEnergyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out skillType))
            {
                Log.Warn("enough skill type energy tri hdl failed : invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            Owner.SkillManager.SetEnergyEnough((SkillType)skillType);
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
