using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 秘境从param1开始每param2章触发一次
    /// model param1 起始章节
    /// model param2 章节步进进度
    /// </summary>
    public class SecretAreaAction : BaseAction
    {
        public SecretAreaAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            int limit = model.Param1 + CurNum  *  model.Param2;

            if (Owner.SecretAreaManager.Id >= model.Limit)
            {
                //超过上限，直接完成
                AddNum(0, true);
                Owner.ActionManager.ActionFinished(this);
                return false;
            }

            if (Owner.SecretAreaManager.Id >= limit)
            {
                if (!CheckActionBySdk(limit))
                {
                    AddNum(1);
                    return true;
                }
            }
            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            AddNum(1);
        }
    }
}
