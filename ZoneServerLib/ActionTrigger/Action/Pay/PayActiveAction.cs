using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 活动付费，购买某个id的充值项目
    /// </summary>
    public class PayActiveAction : PayAction
    {
        public PayActiveAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            CheckAndReset();

            int id;
            if (!int.TryParse(obj.ToString(), out id)) return false;

            AddProdectId(id);

            if (id == model.Param1)
            {

                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }

                SetNum(1, true);
                return true;
            }

            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            //所有魂环都满足年限
            SetNum(1, true);
        }
    }
}
