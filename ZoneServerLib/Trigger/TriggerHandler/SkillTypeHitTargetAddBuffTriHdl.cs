using System;
using CommonUtility;
using Logger;
using ServerShared;

namespace ZoneServerLib
{
    public class SkillTypeHitTargetAddBuffTriHdl : BaseTriHdl
    {
        private readonly int skillType;
        private readonly int buffId = 0;
        public SkillTypeHitTargetAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if(paramArr.Length != 2)
            {
                Log.Warn("init skill type hit target add buff tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if(!int.TryParse(paramArr[0], out skillType) || !int.TryParse(paramArr[1], out buffId))
            {
                Log.Warn("init skill type hit target add buff tri hdl failed, invalid handler param {0}", handlerParam);
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

            int skillLevelGrowth = SkillLibrary.GetSkillGrowth(skillLevel);
            if (Owner as Pet != null)
            {
                skillLevelGrowth = PetLibrary.GetPetInbornSkillGrowth(skillLevel);
            }

            foreach(var target in msg.TargetList)
            {
                target.AddBuff(Owner, buffId, skillLevelGrowth);
            }
        }
    }
}
