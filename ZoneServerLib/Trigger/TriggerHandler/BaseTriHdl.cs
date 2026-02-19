using CommonUtility;

namespace ZoneServerLib
{
    public class BaseTriHdl
    {
        protected BaseTrigger trigger;
        public FieldObject Owner
        { get { return trigger.Owner; } }

        protected FieldMap CurMap
        { get { return trigger.CurMap; } }
        protected string handlerParam;
        public TriggerHandlerType handlerType = TriggerHandlerType.None;

        private int lastSkillId;//上次生效技能
        private FieldObject lastTarget;//上次生效目标
        private int lastHandleFpsIndex;//上次handle 的FPS num
        public DungeonMap DungeonMap { get; private set; }

        public BaseTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
        {
            this.trigger = trigger;
            this.handlerType = handlerType;
            this.handlerParam = handlerParam;

            DungeonMap = trigger.CurMap as DungeonMap;
        }

        public bool ThisFpsHadHandled(FieldObject target = null, int skillId = 0)
        {
            if (DungeonMap?.FpsNum == lastHandleFpsIndex)
            {
                if (lastTarget != null)
                {
                    if (lastTarget == target && lastSkillId == skillId)
                    {
                        Logger.Log.Debug($"map {DungeonMap?.DungeonModel.Id} same fps handled caster {trigger?.Caster?.GetHeroId()} trigger handler {handlerType} owner {Owner?.GetHeroId()} by {trigger?.Model?.Id} same target");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Logger.Log.Debug($"map {DungeonMap?.DungeonModel.Id} same fps handled caster {trigger?.Caster? .GetHeroId()} trigger handler {handlerType} owner {Owner? .GetHeroId()} by {trigger?.Model?.Id}");
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public virtual void Handle()
        {
        }

        protected void SetThisFspHandled(FieldObject target = null, int skillId = 0, bool needLog = true)
        {
            lastSkillId = skillId;
            lastTarget = target;
            lastHandleFpsIndex = DungeonMap == null ? 0 : DungeonMap.FpsNum;

#if DEBUG
            if (needLog)
            { 
                Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
            }
#endif
        }
    }
}
