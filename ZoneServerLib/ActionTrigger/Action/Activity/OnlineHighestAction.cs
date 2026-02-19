using Logger;
using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// OnlineHighestAction
    /// model param1 起始章节
    /// </summary>
    public class OnlineHighestAction : UserActivityAction
    {
        public OnlineHighestAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            CheckOutOfTime();

            int count;
            int.TryParse(obj.ToString(), out count);

            if (count > CurNum)
            {
                //state = 0 标识周期内没有触发过
                bool invoke = State == 0;

                SetNum(count, true);

                return invoke;
            }

            return false;
        }
    }
}
