using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 主线任务taskId结束，param1 taskId
    /// </summary>
    public class MainTaskAction : BaseAction
    {
        public MainTaskAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            int taskId = (int)obj;

            if (taskId >= model.Param1)
            {
                if (!CheckActionBySdk(model.Param1))
                {
                    SetNum(taskId, true);
                    return true;
                }
            }
            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            AddNum(model.Param1, true);
        }
    }
}
