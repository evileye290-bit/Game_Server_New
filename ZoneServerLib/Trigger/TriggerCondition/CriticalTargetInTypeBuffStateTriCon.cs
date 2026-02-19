using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class CriticalTargetInTypeBuffStateTriCon : BaseTriCon
    {
        private readonly BuffType buffType;
        private readonly bool IsInBuffState;
        public CriticalTargetInTypeBuffStateTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            int buff;
            int isIn;
            string[] info = conditionParam.Split(':');
            if (info.Length != 2 || !int.TryParse(info[0], out buff) || !int.TryParse(info[1], out isIn))
            {
                Log.Warn($"init CriticalTargetInTypeBuffStateTriCon failed: invalid buffType {conditionParam}");
                return;
            }

            buffType = (BuffType)buff;
            IsInBuffState = isIn == 1;
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.Critical, out param))
            {
                return false;
            }

            CriticalTriMsg msg = param as CriticalTriMsg;

            if(IsInBuffState)
            {
                return msg != null && msg.Target.InBuffState(buffType);
            }
            else
            {
                return msg != null && !msg.Target.InBuffState(buffType);
            }
        }
    }
}

