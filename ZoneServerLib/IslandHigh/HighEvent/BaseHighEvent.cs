using EnumerateUtility;
using ServerShared;

namespace ZoneServerLib
{
    public abstract class BaseHighEvent
    {
        public IslandHighManager IslandHighManager { get; }

        public BaseHighEvent(IslandHighManager islandHighManager)
        {
            IslandHighManager = islandHighManager;
        }

        public abstract void Invoke(int param);
    }

    //public class HeightGoBackEvent : BaseHighEvent
    //{
    //    public HeightGoBackEvent(IslandHighManager islandHighManager) : base(islandHighManager)
    //    {
    //    }

    //    public override void Invoke(int param)
    //    {
    //        IslandHighManager.AddGrid(-param);
    //    }
    //}

    //public class HeightDelItemEvent : BaseHighEvent
    //{
    //    public HeightDelItemEvent(IslandHighManager islandHighManager) : base(islandHighManager)
    //    {
    //    }

    //    public override void Invoke(int param)
    //    {
    //        NormalItem item = IslandHighManager.Owner.BagManager.NormalBag.GetItemBySubType(param);

    //        if (item == null) return;

    //        IslandHighManager.Owner.DelItem2Bag(item, RewardType.NormalItem, 1, ConsumeWay.GodPathHeight);
    //        IslandHighManager.Owner.SyncClientItemInfo(item);
    //    }
    //}

    //public class HeightIgnoreNegativeEvent : BaseHighEvent
    //{
    //    public HeightIgnoreNegativeEvent(IslandHighManager islandHighManager) : base(islandHighManager)
    //    {
    //    }

    //    public override void Invoke(int param)
    //    {
    //        IslandHighManager.AddAction((int)HeightActionType.IgnoreNegative);
    //    }
    //}
}
