using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class PromotionShop : Shop
    {
        public PromotionShop(ShopManager manager) : base(manager, ShopType.Promotion)
        {
        }

        public override void BindShopItems(DBShopInfo shopInfo)
        {
            foreach (var kv in shopInfo.CommonItemList)
            {
                AddItem(new BaseShopItem(kv.Value.ShopItemId, kv.Value.BuyCount, kv.Value.ItemInfo));
            }
        }

        public override MSG_ZGC_SHOP_INFO GenerateShopMsg()
        {
            MSG_ZGC_SHOP_INFO msg = new MSG_ZGC_SHOP_INFO();
            msg.ShopType = (int)ShopType;
            msg.RefreshCount = RefreshCount;

            foreach (var kv in commonItemList)
            {
                msg.ShopItems.Add(kv.Value.GenerateMsg());
            }
            return msg;
        }

        public override void Refresh()
        {
            //刷新商品
            List<int> shopItemIds = manager.GetShopItemList(ShopType);
            RefreshItems(shopItemIds);
            //同步库
            SyncUpdateShopItems();
        }
    }
}
