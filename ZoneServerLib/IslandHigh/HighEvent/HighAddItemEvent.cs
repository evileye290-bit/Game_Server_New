using EnumerateUtility;
using ServerShared;

namespace ZoneServerLib
{
    public class HighAddItemEvent : BaseHighEvent
    {
        public HighAddItemEvent(IslandHighManager islandHighManager) : base(islandHighManager)
        {
        }

        public override void Invoke(int param)
        {
            IslandHighManager.RewardManager.AddReward(new ServerModels.ItemBasicInfo((int)RewardType.NormalItem, param, 1));
            //var items = IslandHighManager.Owner.AddItem2Bag(MainType.Consumable, RewardType.NormalItem, param, 1, ObtainWay.IslandHigh);
            //IslandHighManager.Owner.SyncClientItemsInfo(items);
        }
    }
}
