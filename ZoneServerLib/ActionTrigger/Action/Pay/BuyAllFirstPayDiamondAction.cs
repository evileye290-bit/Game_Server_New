using Logger;
using ServerModels;

namespace ZoneServerLib
{
    public class BuyAllFirstPayDiamondAction : PayAction
    {
        public BuyAllFirstPayDiamondAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            CheckAndReset();

            int id;
            if (!int.TryParse(obj.ToString(), out id)) return false;

            AddProdectId(id);

            //走sdk推荐礼包
            if (CheckActionBySdk(model.Param1))
            {
                return false;
            }
            SetNum(1, true);
            return true;
        }

        public override void SetFinishedBySdk(int current)
        {
            //所有魂环都满足年限
            SetNum(1, true);
        }
    }
}
