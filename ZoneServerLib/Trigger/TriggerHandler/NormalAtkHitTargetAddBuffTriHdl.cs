using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class NormalAtkHitTargetAddBuffTriHdl : BaseTriHdl
    {
        private readonly int buffId = 0;
        public NormalAtkHitTargetAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            

            if ( !int.TryParse(handlerParam, out buffId))
            {
                Log.Warn("init normal atk hit target add buff tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.NormalAtkHit, out param))
            {
                return;
            }
            SkillHitMsg msg = param as SkillHitMsg;
            if (msg == null || msg.TargetList == null || msg.TargetList.Count == 0)
            {
                return;
            }

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();
            if (skillLevelGrowth <= 1)
            {
                Skill skill = Owner.SkillManager.GetSkill(msg.Model.Id);
                if (skill == null)
                {
                    return;
                }
                skillLevelGrowth = skill.Level;
            }
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();
            foreach (var target in msg.TargetList)
            {
                target.AddBuff(Owner, buffId, skillLevelGrowth);
            }
        }
    }
}
