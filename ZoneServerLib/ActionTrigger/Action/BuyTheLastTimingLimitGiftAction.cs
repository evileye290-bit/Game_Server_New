using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 若当前仅有1个限时礼包，购买该礼包后，根据当前权重触发1次限时礼包，每天仅1次
    /// </summary>
    public class BuyTheLastTimingLimitGiftAction : BaseAction
    {
        public BuyTheLastTimingLimitGiftAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            CheckAndReset();

            //当日已经触发过了
            if (State == 1) return false;

            if (!actionManager.CheckHaveNeedBuyTimingGift())
            {
                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }
                SetNum(1,true);
                return true;
            }

            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            SetNum(1, true);
        }
    }
}
