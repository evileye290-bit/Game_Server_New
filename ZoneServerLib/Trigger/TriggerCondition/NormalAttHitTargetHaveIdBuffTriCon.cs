using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class NormalAttHitTargetHaveIdBuffTriCon : BaseTriCon
    {
        private readonly int buffId;
        public NormalAttHitTargetHaveIdBuffTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffId))
            {
                Log.Warn($"init NormalAttHitTargetHaveBuffId failed: invalid buffId {conditionParam}");
            }
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
                if (kv?.BuffManager.GetBuff(buffId) != null) return true;
            }

            return false;
        }
    }
}
