using System;
using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class AddSkillEffectTriHdl : BaseTriHdl
    {
        private readonly int skillId = 0;
        private readonly int skillEffId = 0;
        public AddSkillEffectTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 2)
            {
                Log.Warn("in add skill eff tri hdl: invalid param {0}", handlerParam);
                return;
            }
            if (!int.TryParse(paramArr[0], out skillId) || !int.TryParse(paramArr[1], out skillEffId))
            {
                Log.Warn("in add skill eff tri hdl: invalid param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            SkillEffectModel effModel = SkillEffectLibrary.GetSkillEffectModel(skillEffId);
            if(effModel == null)
            {
                return;
            }
            Skill skill = Owner.SkillManager.GetSkill(skillId);
            if(skill == null)
            {
                return;
            }

            //添加的技能效果按照原技能等级生效
            TriggerCreatedBySkill triggerCreatedBySkill = trigger as TriggerCreatedBySkill;
            if (triggerCreatedBySkill != null)
            {
                skill.AddSkillEffect(effModel, triggerCreatedBySkill.SkillLevel);
            }
            else
            {
                skill.AddSkillEffect(effModel);
            }

#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
