using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class SkillTypeHitTargetAddTriggerTriHdl : BaseTriHdl
    {
        private readonly int skillType;
        private readonly int triggerId = 0;
        public SkillTypeHitTargetAddTriggerTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if(paramArr.Length != 2)
            {
                Log.Warn("init skill type hit target add trigger tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if(!int.TryParse(paramArr[0], out skillType) || !int.TryParse(paramArr[1], out triggerId))
            {
                Log.Warn("init skill type hit target add trigger tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildSkillTypeHitKey(skillType), out param))
            {
                return;
            }
            SkillHitMsg msg = param as SkillHitMsg;
            if(msg == null || (int)(msg.Model.Type) != skillType || msg.TargetList == null || msg.TargetList.Count == 0)
            {
                return;
            }
            int skillLevel = 0;
            if(trigger is TriggerCreatedBySkill)
            {
                skillLevel = trigger.GetFixedParam_SkillLevel();
            }
            else
            {
                Skill skill = Owner.SkillManager.GetSkill(msg.Model.Id);
                if (skill == null)
                {
                    return;
                }
                skillLevel = skill.Level;
            }
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            foreach (var target in msg.TargetList)
            {
                target.AddTriggerCreatedBySkill(triggerId, skillLevel, Owner);
            }
        }
    }
}
