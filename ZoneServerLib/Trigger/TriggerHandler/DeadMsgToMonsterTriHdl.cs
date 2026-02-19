using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DeadMsgToMonsterTriHdl : BaseTriHdl
    {
        int monsterId;
        public DeadMsgToMonsterTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            int.TryParse(handlerParam, out monsterId);
        }

        public override void Handle()
        {
            if(Owner == null || !Owner.IsDead || trigger.CurMap == null)
            {
                return;
            }
            foreach(var kv in trigger.CurMap.MonsterList)
            {
                if(kv.Value.MonsterModel.Id == monsterId)
                {
                    kv.Value.DispatchMessage(TriggerMessageType.Dead, Owner);
                }
            }
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
