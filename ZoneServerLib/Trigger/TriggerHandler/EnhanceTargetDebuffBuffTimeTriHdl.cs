using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    class EnhanceTargetDebuffBuffTimeTriHdl : BaseTriHdl
    {
        private readonly float time;

        public EnhanceTargetDebuffBuffTimeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!float.TryParse(handlerParam, out time))
            {
                Log.Error($"EnhanceTargetDebuffBuffTimeTriHdl error, handlerParam {handlerParam}");
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.AnySkillDoDamage, out param) &&
                !trigger.TryGetParam(TriggerParamKey.AnySkillDoDamageBefore, out param))
            {
                return;
            }

            DoDamageTriMsg msg = param as DoDamageTriMsg;
            if (msg != null)
            {
                int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
                msg.FieldObject?.BuffManager.EnhanceDebuffBuffTime(time);
            }
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}

