using EnumerateUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;


namespace ZoneServerLib
{
    public class BuyAllWeeklyGiftBagAction : PayAction
    {
        public BuyAllWeeklyGiftBagAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            CheckAndReset();

            int id;
            if (!int.TryParse(obj.ToString(), out id)) return false;

            AddProdectId(id);

            List<RechargeItemModel> itemId;
            if (!RechargeLibrary.DailyWeeklyMonthlyItemsList.TryGetValue(CommonGiftType.Weekly, out itemId) || itemId == null || itemId.Count == 0)
            {
                return false;
            }

            foreach (var check in itemId.Select(x => Owner.GiftItemCurBuyCount(x.Id) < x.BuyLimit))
            {
                //某一项没有买完
                if (check) return false;
            }

            //走sdk推荐礼包
            if (CheckActionBySdk(model.Param1))
            {
                return false;
            }

            return true;
        }
    }
}
