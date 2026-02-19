using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class AnySkilDamageTargetAddMarkTriHdl : BaseTriHdl
    {
        readonly int markId;
        public AnySkilDamageTargetAddMarkTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam) 
            : base(trigger, handlerType, handlerParam)
        {
            markId = int.Parse(handlerParam);
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param))
            {
                return;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg != null)
            {
                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                msg.FieldObject.AddMark(Owner, markId, 1);
            }


#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id{Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}

