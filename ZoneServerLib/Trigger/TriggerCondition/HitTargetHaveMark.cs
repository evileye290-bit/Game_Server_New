using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class HitTargetHaveMark : BaseTriCon
    {
        private readonly int skillType;
        private readonly int markId;
        public HitTargetHaveMark(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] info = conditionParam.Split(':');
            if (info.Length != 2 || !int.TryParse(info[0], out skillType) || !int.TryParse(info[1], out markId))
            {
                Log.Warn($"init HitTargetHaveMark failed: invalid skill type {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildSkillTypeHitKey(skillType), out param))
            {
                return false;
            }

            SkillHitMsg msg = param as SkillHitMsg;
            if (msg == null)
            {
                return false;
            }

            foreach (var kv in msg.TargetList)
            {
                if (kv.MarkManager?.GetMark(markId) != null) return true;
            }

            return false;
        }
    }
}
