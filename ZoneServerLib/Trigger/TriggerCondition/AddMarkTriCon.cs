using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AddMarkTriCon : BaseTriCon
    {
        private readonly int markId;
        public AddMarkTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out markId))
            {
                Log.Warn($"init AddMarkTriCon trigger condition failed: invalid mark id {conditionParam}");
            }
        }

        public override bool Check()
        {
            int id;
            object param;

            if (!trigger.TryGetParam(TriggerParamKey.BuildAddMarkKey(markId), out param)
                || param == null 
                || !int.TryParse(param.ToString(), out id))
            {
                return false;
            }

            return id == markId;
        }
    }
}
