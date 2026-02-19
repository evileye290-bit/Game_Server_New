using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 每日消耗数量
    /// model param1 消耗数量
    /// </summary>
    public class DailyDiamondCostCountAction : BaseAction
    {
        public DailyDiamondCostCountAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            CheckAndReset();

            //当日已经触发过了
            if (State == 1) return false;

            int num;
            int.TryParse(obj.ToString(), out num);

            if (CurNum + num >= model.Param1)
            {
                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }
                AddNum(num, true);
                return true;
            }
            else
            {
                AddNum(num);
            }

            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            SetNum(model.Param1, true);
        }
    }
}
