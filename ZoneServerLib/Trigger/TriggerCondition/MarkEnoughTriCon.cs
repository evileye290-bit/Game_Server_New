using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    // 某个mark数量足够
    public class MarkEnoughTriCon : BaseTriCon
    {
        private readonly int markId;
        public MarkEnoughTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out markId))
            {
                Log.Warn($"init mark enough trigger condition failed: invalid mark id {conditionParam}");
            }
        }

        public override bool Check()
        {
            object param;
            return trigger.TryGetParam(TriggerParamKey.BuildMarkEnoughKey(markId), out param);
        }
    }
}
