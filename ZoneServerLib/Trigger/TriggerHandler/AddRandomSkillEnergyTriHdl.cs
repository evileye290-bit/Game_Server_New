using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AddRandomSkillEnergyTriHdl : BaseTriHdl
    {
        private readonly int energy = 0;
        public AddRandomSkillEnergyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            energy = int.Parse(handlerParam);
            energy = trigger.CalcParam(handlerType, energy);
        }

        public override void Handle()
        {
            int index = RAND.Range((int)SkillType.Normal_Skill_1, (int)SkillType.Normal_Skill_3);
            SkillType skillType = (SkillType)index;
            Owner.SkillManager.AddEnergy(skillType, energy, true, true, true);

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id{Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
