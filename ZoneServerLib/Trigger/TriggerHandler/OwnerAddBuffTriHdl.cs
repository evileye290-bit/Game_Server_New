using CommonUtility;

namespace ZoneServerLib
{
    public class OwnerAddBuffTriHdl : BaseTriHdl
    {
        readonly int buffId;
        public OwnerAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam) 
            : base(trigger, handlerType, handlerParam)
        {
            buffId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            //同一帧不连续触发
            //if (ThisFpsHadHandled()) return;
            //SetThisFspHandleed();
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            Owner.AddBuff(trigger.Caster, buffId, skillLevelGrowth);
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
