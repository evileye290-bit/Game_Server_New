using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class MarkGTTriCon : BaseTriCon
    {
        private readonly int matkId, count;
        public MarkGTTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            var paramList = conditionParam.ToList(':');
            if (paramList.Count != 2)
            {
                Log.Warn($"init MarkGTTriCon trigger condition failed: invalid mark id {conditionParam}");
                return;
            }

            matkId = paramList[0];
            count = paramList[1];
        }

        public override bool Check()
        {
            return owner.MarkManager.GetMark(matkId)?.CurCount >= count;
        }
    }
}
