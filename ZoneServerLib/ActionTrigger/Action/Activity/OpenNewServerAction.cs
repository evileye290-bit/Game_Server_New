using Logger;
using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 加开新服庆贺 每加开param1个服务器触发一次
    /// model param1 起始章节
    /// </summary>
    public class OpenNewServerAction : UserActivityAction
    {
        public OpenNewServerAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            CheckOutOfTime();

            int count;
            int.TryParse(obj.ToString(), out count);

            if (count >= model.Param1 * (CurNum + 1))
            {
                //state = 0 标识周期内没有触发过
                bool invoke = State == 0;

                AddNum(1, true);

                return invoke;
            }

            return false;
        }
    }
}
