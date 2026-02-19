using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class NormalAttHitTargetHaveTypeBuffTriCon : BaseTriCon
    {
        private readonly BuffType buffType;
        public NormalAttHitTargetHaveTypeBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int buff;
            if (!int.TryParse(conditionParam, out buff))
            {
                Log.Warn($"init NormalAttHitTargetHaveTypeBuffTriCon failed: invalid buffId {conditionParam}");
                return;
            }

            buffType = (BuffType)buff;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.NormalAtkHit, out param))
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
