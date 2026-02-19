using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CriticalTargetAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId = 0;
        public CriticalTargetAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out buffId))
            {
                Log.Warn("init CriticalTargetAddBuffTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.Critical, out param))
            {
                return;
            }
            CriticalTriMsg msg = param as CriticalTriMsg;
            if (msg == null || msg.Target == null || msg.Model == null)
            {
                return;
            }
            Skill skill = Owner.SkillManager.GetSkill(msg.Model.Id);
            if (skill == null)
            {
                return;
            }

            //魂环技能等级
            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();

            msg.Target.AddBuff(Owner, buffId, Math.Max(skill.Level, skillLevelGrowth));
            SetThisFspHandled();
        }
    }
}

