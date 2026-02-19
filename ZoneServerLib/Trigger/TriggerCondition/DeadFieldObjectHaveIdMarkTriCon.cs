using CommonUtility;

namespace ZoneServerLib
{
    public class DeadFieldObjectHaveIdMarkTriCon : BaseTriCon
    {
        private readonly int markId = 0;
        public DeadFieldObjectHaveIdMarkTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            if (!int.TryParse(conditionParam, out markId))
            {
                Logger.Log.Error($"DeadFieldObjectHaveIdMarkTriCon error, mark id {markId}");
                return;
            }
        }

        public override bool Check()
        {
            object param = null;
            if (!trigger.TryGetParam(TriggerParamKey.FieldObjectDead, out param))
            {
                return false;
            }

            return (param as FieldObject)?.MarkManager.GetMark(markId) != null;
        }
    }
}


