using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class EnhanceSkillEffectModelTTriHdl : BaseTriHdl
    {
        private readonly int skillId, skillEffectId;
        private readonly float addV = 0;
        public EnhanceSkillEffectModelTTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] strParam = handlerParam.Split(':');
            if (strParam.Length != 3 || 
                !int.TryParse(strParam[0], out skillId) || 
                !int.TryParse(strParam[1], out skillEffectId) || 
                !float.TryParse(strParam[2], out addV))
            {
                Log.WarnLine($"init EnhanceSkillEffectModelTTriHdl error ,param {handlerParam}");
            }
        }

        public override void Handle()
        {
            Owner?.SkillManager?.GetSkill(skillId)?.AddSkillEffectT(skillEffectId, addV);
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
