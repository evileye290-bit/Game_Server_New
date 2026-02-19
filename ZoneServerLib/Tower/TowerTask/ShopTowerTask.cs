using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class ShopTowerTask : TowerTask
    {
        public ShopTowerTask(TowerManager manager, int id) : base(manager, id, TowerTaskType.Shop)
        {
        }

        public override ErrorCode Execute(int param, MSG_ZGC_TOWER_EXECUTE_TASK msg)
        {
            if (param < 0)
            {
                Manager.GotoNextNode();
            }
            else
            {
                if (TaskInfo == null || param >= TaskInfo.param.Count) return ErrorCode.Fail;

                if (Manager.ShopList.Contains(param))  return ErrorCode.TowerShopBuyed;

                int shopItemId = TaskInfo.param[param];

                TowerShopItemModel model = TowerLibrary.GetShopItemModel(shopItemId);
                if (model == null) return ErrorCode.Fail;

                CommonShopItemModel itemModel = CommonShopLibrary.GetShopItemModel(model.Id);
                if (itemModel == null) return ErrorCode.Fail;

                int currenciesTypeId = int.Parse(itemModel.CurrentPrice[0]);
                CurrenciesType itemId = (CurrenciesType)currenciesTypeId;
                int rewardType = itemModel.CurrentPrice[1].ToInt();
                int num = itemModel.CurrentPrice[2].ToInt();

                if (!Manager.Owner.CheckCoins(itemId, num)) return ErrorCode.NoCoin;

                Manager.Owner.DelCoins(itemId, num, ConsumeWay.Tower, model.Id.ToString());

                ItemBasicInfo info =ItemBasicInfo.Parse(itemModel.Reward);
                RewardManager rewardManager = new RewardManager();
                if (info!= null && Manager.SoulBoneList.ContainsKey(info.Id))
                {
                    rewardManager.AddReward(Manager.SoulBoneList[info.Id]);
                }
                else
                {
                    rewardManager.AddSimpleRewardWithSoulBoneCheck(itemModel.Reward);
                }

                rewardManager.BreakupRewards();

                msg.Index = param;
                rewardManager.GenerateRewardItemInfo(msg.Rewards);

                Manager.Owner.AddRewards(rewardManager, ObtainWay.Tower);

                Manager.AddBuyedShop(param);

                //埋点BI
                foreach (var item in rewardManager.AllRewards)
                {
                    Manager.Owner.BIRecordShopByItemLog(ShopType.TowerNodeShop, currenciesTypeId.ToString(), num, ObtainWay.Tower, (RewardType)item.RewardType, item.Id, item.Num, TimingGiftType.None, 1);
                    Manager.Owner.RecordShopByItemLog(ShopType.TowerNodeShop, currenciesTypeId.ToString(), num, ObtainWay.Tower, (RewardType)item.RewardType, item.Id, item.Num);
                }
                Manager.Owner.KomoeEventLogShopPurchase(shopItemId, 1, currenciesTypeId, itemId.ToString(), num, (int)ShopType.TowerNodeShop, ShopType.TowerNodeShop.ToString());
                //卖完了自动进入到下一节点
                if (Manager.ShopList.Count == TowerLibrary.ShopItemCount)
                {
                    Manager.GotoNextNode();
                }
            }

            return ErrorCode.Success;
        }

        public override bool CheckFinished()
        {
            return Manager.ShopList.Count == TowerLibrary.ShopItemCount;
        }
    }
}
