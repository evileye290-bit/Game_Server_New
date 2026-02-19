using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class HitTargetHaveBuffTriCon : BaseTriCon
    {
        private readonly int skillType;
        private readonly int  buffId;
        public HitTargetHaveBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] info = conditionParam.Split(':');
            if (info.Length != 2 || !int.TryParse(info[0], out skillType) || !int.TryParse(info[1], out buffId))
            {
                Log.Warn($"init HitTargetHaveBuffTriCon failed: invalid skill type {conditionParam}");
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
                if (kv.BuffManager.GetBuff(buffId) != null) return true;
            }

            return false;
        }
    }
}
