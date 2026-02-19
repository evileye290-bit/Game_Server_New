using CommonUtility;
using Logger;
using ServerShared;

namespace ZoneServerLib
{
    public class SkillTypeHitTargetDamageByOwnerMarkCountTriHdl : BaseTriHdl
    {
        private readonly int skillType, markId;
        private float growth, baseValue;

        public SkillTypeHitTargetDamageByOwnerMarkCountTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] param = handlerParam.Split(':');
            if (param.Length != 4 || !int.TryParse(param[0], out skillType) || !int.TryParse(param[1], out markId) || !float.TryParse(param[2], out growth) || !float.TryParse(param[3], out baseValue))
            {
                Log.Warn("init SkillTypeHitTargetDamageByOwnerMarkCountTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
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

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();

            Mark mark = Owner.MarkManager.GetMark(markId);
            if (mark == null || mark.CurCount < 1) return;
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            int value = (int)(trigger.CalcParam(growth, baseValue, skillLevelGrowth) * mark.CurCount);

            foreach (var target in msg.TargetList)
            {
                target.DoSpecDamage(trigger.Caster, DamageType.Skill, value);
            }
        }
    }
}