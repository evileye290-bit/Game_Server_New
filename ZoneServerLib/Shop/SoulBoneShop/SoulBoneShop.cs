using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System.Collections.Generic; 
using System.Linq;

namespace ZoneServerLib
{
    class SoulBoneShop : Shop
    {
        public SoulBoneShop(ShopManager manager) : base(manager, ShopType.SoulBone)
        {
        }

        public override void BindShopItems(DBShopInfo shopInfo)
        {
            this.RefreshCount = shopInfo.RefreshCount;
            foreach (var kv in shopInfo.ItemList)
            {
                SoulBoneShopItem item = new SoulBoneShopItem(kv.Value);
                SoulBone soulBone = SoulBoneManager.GenerateSoulBoneInfo(kv.Value.ItemInfo, true);
                item.SetRewardInfo(kv.Value.ItemInfo, soulBone);

                AddItem(item);
            }
        }

        public void AddItems(Dictionary<int, BingoDropModel> items)
        {
            Clear();
            SoulBoneShopItem item = null;
            foreach (var kv in items)
            {
                DBShopItemInfo dbShopItemInfo = new DBShopItemInfo() {Id = kv.Value.Id, ShopItemId = kv.Value.Id, BuyCount = 0, ItemInfo = ""};
                item = new SoulBoneShopItem(dbShopItemInfo);

                string rewardStr = kv.Value.Data.GetString("SoulBone");
                if (string.IsNullOrEmpty(rewardStr))
                {
                    continue;
                }

                List<SoulBone> soulBone = new List<SoulBone>();
                List<ItemBasicInfo> rewardList = SoulBoneManager.GenerateSoulboneReward(rewardStr, soulBone, (int) manager.Owner.HeroMng.GetFirstHeroJob());
                ItemBasicInfo soulBoneItem = rewardList?.FirstOrDefault();
                SoulBone soulBoneInfo = soulBone.FirstOrDefault();
                if (soulBoneItem != null && soulBoneInfo != null)
                {
                    item.SetRewardInfo(soulBoneItem.ToString(), soulBoneInfo);
                }
                AddItem(item);
            }
        }

        public override void Refresh()
        {
            int tire = manager.Owner.SecretAreaManager.GetTire();
            Dictionary<int, BingoDropModel> soulBoneItems = SecretAreaLibrary.GetTireSoulBoneItems(tire);
            if (soulBoneItems != null)
            {
                AddItems(soulBoneItems);
            }
            SyncDbUpdateShopItem();
        }

        public override MSG_ZGC_SHOP_INFO GenerateShopMsg()
        {
            MSG_ZGC_SHOP_INFO msg = new MSG_ZGC_SHOP_INFO();
            msg.ShopType = (int)ShopType;
            msg.RefreshCount = RefreshCount;

            foreach (var kv in itemList)
            {
                SoulBoneShopItem item = kv.Value as SoulBoneShopItem;
                msg.SoulBones.Add(item.GenerateSoulBoneMsg());
            }
            return msg;
        }
    }
}
