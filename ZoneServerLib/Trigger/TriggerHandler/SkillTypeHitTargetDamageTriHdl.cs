using System;
using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class SkillTypeHitTargetDamageTriHdl : BaseTriHdl
    {
        private readonly int skillType;
        private readonly float growth, baseDamage;
        private readonly int damage = 0;
        public SkillTypeHitTargetDamageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length != 3)
            {
                Log.Warn("init skill type hit target damage tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            if (!int.TryParse(paramArr[0], out skillType) || !float.TryParse(paramArr[1], out growth) || !float.TryParse(paramArr[2], out baseDamage))
            {
                Log.Warn("init skill type hit target damage tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            damage = (int)trigger.CalcParam(growth, baseDamage, trigger.GetFixedParam_SkillLevelGrowth());
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
            Skill skill = Owner.SkillManager.GetSkill(msg.Model.Id);
            if (skill == null)
            {
                return;
            }
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            foreach (var target in msg.TargetList)
            {
                target.DoSpecDamage(Owner, DamageType.Skill, damage);
            }
        }
    }
}
