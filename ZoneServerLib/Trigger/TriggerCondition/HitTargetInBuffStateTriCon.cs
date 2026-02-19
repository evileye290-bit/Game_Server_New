using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class HitTargetInBuffStateTriCon : BaseTriCon
    {
        private readonly int skillType;
        private readonly BuffType buffType;
        public HitTargetInBuffStateTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int buff = 0;
            string[] info = conditionParam.Split(':');
            if (info.Length != 2 || !int.TryParse(info[0], out skillType) || !int.TryParse(info[1], out buff))
            {
                Log.Warn($"init HitTargetInBuffStateTriCon failed: invalid skill type {conditionParam}");
            }

            buffType = (BuffType)buff;
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
                if (kv.InBuffState(buffType)) return true;
            }

            return false;
        }
    }
}
