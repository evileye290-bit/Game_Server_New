using CommonUtility;

namespace ZoneServerLib
{
    public class CastTypeBuffOwnerAddBuffTriHdl : BaseTriHdl
    {
        readonly int buffType, extraBuffId;
        public CastTypeBuffOwnerAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 2 || !int.TryParse(param[0], out buffType) || !int.TryParse(param[1], out extraBuffId))
            {
                Logger.Log.Error($"init CastTypeBuffOwnerAddBuffTriHdl erro :invalid param {handlerParam}");
                return;
            }
        }

        public override void Handle()
        {
            object obj;
            if (!trigger.TryGetParam(TriggerParamKey.BuildCastBuffTypeKey(buffType), out obj))
            {
                return;
            }
            BaseBuff buff = obj as BaseBuff;
            if (buff != null && (int)buff.BuffType == buffType)
            {
                buff.Owner.AddBuff(trigger.Caster, extraBuffId, trigger.GetFixedParam_SkillLevelGrowth());
            }

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
