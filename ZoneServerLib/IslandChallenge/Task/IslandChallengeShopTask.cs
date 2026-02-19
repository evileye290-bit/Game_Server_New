using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class IslandChallengeShopTask : BaseIslandChallengeTask
    {
        public IslandChallengeShopTask(IslandChallengeManager manager, int id) : base(manager, id, TowerTaskType.Shop)
        {
        }

        public override ErrorCode Execute(int param, MSG_ZGC_ISLAND_CHALLENGE_EXECUTE_TASK msg)
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
                IslandChallengeShopItemModel model = IslandChallengeLibrary.GetIslandChallengeShopItemModel(shopItemId);
                if (model == null) return ErrorCode.Fail;

                CommonShopItemModel itemModel = CommonShopLibrary.GetShopItemModel(model.Id);
                if (itemModel == null) return ErrorCode.Fail;

                int currenciesTypeId = int.Parse(itemModel.CurrentPrice[0]);
                CurrenciesType itemId = (CurrenciesType)currenciesTypeId;
                int rewardType = itemModel.CurrentPrice[1].ToInt();
                int num = itemModel.CurrentPrice[2].ToInt();

                if (!Manager.Owner.CheckCoins(itemId, num)) return ErrorCode.NoCoin;

                Manager.Owner.DelCoins(itemId, num, ConsumeWay.IslandChallenge, model.Id.ToString());

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

                Manager.Owner.AddRewards(rewardManager, ObtainWay.IslandChallenge);

                Manager.AddBuyedShop(param);

                //埋点BI
                foreach (var item in rewardManager.AllRewards)
                {
                    Manager.Owner.BIRecordShopByItemLog(ShopType.IslandChallengeNodeShop, currenciesTypeId.ToString(), num, ObtainWay.IslandChallenge, (RewardType)item.RewardType, item.Id, item.Num, TimingGiftType.None, 1);
                    Manager.Owner.RecordShopByItemLog(ShopType.IslandChallengeNodeShop, currenciesTypeId.ToString(), num, ObtainWay.IslandChallenge, (RewardType)item.RewardType, item.Id, item.Num);
                }
                Manager.Owner.KomoeEventLogShopPurchase(shopItemId, 1, currenciesTypeId, itemId.ToString(), num, (int)ShopType.IslandChallengeNodeShop, ShopType.IslandChallengeNodeShop.ToString());
                //卖完了自动进入到下一节点
                if (Manager.ShopList.Count == IslandChallengeLibrary.ShopItemCount)
                {
                    Manager.GotoNextNode();
                }
            }

            return ErrorCode.Success;
        }

        public override bool CheckFinished()
        {
            return Manager.ShopList.Count == IslandChallengeLibrary.ShopItemCount;
        }
    }
}
