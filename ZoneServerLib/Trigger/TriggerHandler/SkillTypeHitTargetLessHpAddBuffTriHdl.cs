using CommonUtility;
using Logger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class SkillTypeHitTargetLessHpAddBuffTriHdl : BaseTriHdl
    {
        private readonly int ratio = 0;
        private readonly int skillType;
        private readonly int buffId = 0;
        public SkillTypeHitTargetLessHpAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 3)
            {
                Log.Warn("init skill type hit target less hp add buff tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if (!int.TryParse(paramArr[0], out ratio) || !int.TryParse(paramArr[1], out skillType) || !int.TryParse(paramArr[2], out buffId))
            {
                Log.Warn("init skill type hit target less hp add buff tri hdl failed, invalid handler param {0}", handlerParam);
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
            if (msg == null || (int)(msg.Model.Type) != skillType || msg.TargetList == null || msg.TargetList.Count == 0)
            {
                return;
            }
            int skillLevel = 0;
            if (trigger is TriggerCreatedBySkill)
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

            int skillLevelGrowth = SkillLibrary.GetSkillGrowth(skillLevel);
            if (Owner as Pet != null)
            {
                skillLevelGrowth = PetLibrary.GetPetInbornSkillGrowth(skillLevel);
            }
            foreach (var target in msg.TargetList)
            {
                if (target.HpLessThanRate(ratio))
                {
                    target.AddBuff(Owner, buffId, skillLevelGrowth);
                }
            }
#if DEBUG
            Logger.Log.DebugLine($"owner {Owner.Uid} id {Owner.GetHeroId()} instance id {Owner.InstanceId} trigger handler id {trigger.Model.Id} type {this.GetType().Name}");
#endif
        }
    }
}
