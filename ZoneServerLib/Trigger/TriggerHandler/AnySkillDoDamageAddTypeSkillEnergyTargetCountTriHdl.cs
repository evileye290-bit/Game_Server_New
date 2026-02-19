using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AnySkillDoDamageAddTypeSkillEnergyTargetCountTriHdl : BaseTriHdl
    {
        private readonly SkillType skillType;
        private readonly int energy;
        public AnySkillDoDamageAddTypeSkillEnergyTargetCountTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            var parm = handlerParam.ToList(':');

            if (parm.Count != 2)
            {
                Log.Warn("init AnySkillDoDamageAddTypeSkillEnergyTargetCountTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            skillType = (SkillType)parm[0];
            energy = parm[1];
        }

        public override void Handle()
        {
            object param;
            trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageTargetList, out param);

            DoDamageTargetListTriMsg msg = param as DoDamageTargetListTriMsg;
            if (msg == null) return;

            trigger.Owner?.SkillManager.AddEnergy(skillType, energy * msg.TargetList.Count, true, true, true);

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id{Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }    
}
