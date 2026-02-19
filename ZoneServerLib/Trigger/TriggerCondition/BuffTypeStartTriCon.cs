using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class BuffTypeStartTriCon : BaseTriCon
    {
        private readonly int buffType;
        public BuffTypeStartTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out buffType))
            {
                Log.Warn($"init BuffTypeStartTriCon trigger condition failed: invalid buff id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.BuildStartBuffTypeKey(buffType), out param))
            {
                return false;
            }
            BuffStartTriMsg msg = param as BuffStartTriMsg;
            if (msg == null)
            {
                return false;
            }
            return msg.BuffType == buffType;
        }
    }
}