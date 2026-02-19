using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CasterOwnerAddBuffTriHdl : BaseTriHdl
    {
        readonly int buffId;
        public CasterOwnerAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            buffId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            FieldObject target = Owner.GetOwner();
            if(target == null)
            {
                return;
            }
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            target.AddBuff(Owner, buffId, skillLevelGrowth);

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id{Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
