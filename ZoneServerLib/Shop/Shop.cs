using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    /// <summary>
    /// 商店基础类，各个商店继承该类
    /// </summary>
    public class Shop
    {
        protected ShopManager manager;

        protected Dictionary<int, ShopItem> itemList = new Dictionary<int, ShopItem>();
        public Dictionary<int, ShopItem> ItemList => itemList;      

        public ShopType ShopType { get; private set; }
        public int RefreshCount { get; set; }

        public Shop(ShopManager manager, ShopType shopType)
        {
            this.ShopType = shopType;
            this.manager = manager;
        }

        public virtual void BindShopItems(DBShopInfo shopInfo)
        {
            this.RefreshCount = shopInfo.RefreshCount;
            foreach (var kv in shopInfo.ItemList)
            {
                AddItem(new ShopItem(kv.Value.Id, kv.Value.BuyCount, kv.Value.ItemInfo));
            }
        }

        public virtual void Refresh()
        {
        }

        public virtual void Clear()
        {
            itemList.Clear();
        }

        public ShopItem GetShopItem(int id)
        {
            ShopItem item;
            itemList.TryGetValue(id, out item);
            return item;
        }

        //public BaseShopItem GetCommonShopItem(int shopItemId)
        //{
        //    BaseShopItem item;
        //    commonItemList.TryGetValue(shopItemId, out item);
        //    return item;
        //}

        protected void AddItem(ShopItem item)
        {
            itemList[item.Id] = item;
        }              

        public void AddShopItem(ShopItem item)
        {
            AddItem(item);
        }

        #region seialize

        public virtual string BuildIdString()
        {
            return string.Join("|", itemList.Keys);
        }     

        public virtual string BuildBuyCountString()
        {
            return string.Join("|", itemList.Values.Select(x => x.BuyCount));
        }  

        /// <summary>
        /// 商品序列化
        /// </summary>
        /// <returns></returns>
        public virtual string GetShopItemsString()
        {
            return string.Join("|", itemList.Values.Select(x => x.GetItemInfo()));
        }

        public virtual MSG_ZGC_SHOP_INFO GenerateShopMsg()
        {
            MSG_ZGC_SHOP_INFO msg = new MSG_ZGC_SHOP_INFO();
            msg.ShopType = (int)ShopType;
            msg.RefreshCount = RefreshCount;

            foreach (var kv in itemList)
            {
                msg.ShopItems.Add(kv.Value.GenerateMsg());
            }
            return msg;
        }

        #endregion

        #region DB Sync

        /// <summary>
        /// 新创建商店同步库
        /// </summary>
        public void SyncDBShopInfo()
        {
            QueryInsertShop query = new QueryInsertShop(manager.Owner.Uid, (int)ShopType, RefreshCount, BuildIdString(), BuildBuyCountString(), GetShopItemsString());
            manager.Owner.server.GameDBPool.Call(query);
        }    

        /// <summary>
        /// 刷新商店同步库
        /// </summary>
        public void SyncDbUpdateShopItem()
        {
            QueryUpdateShopList query = new QueryUpdateShopList(manager.Owner.Uid, (int)ShopType, RefreshCount, BuildIdString(), BuildBuyCountString(), GetShopItemsString());
            manager.Owner.server.GameDBPool.Call(query);
        }

        /// <summary>
        /// 购买商品同步库
        /// </summary>
        public void SyncDbUpdateShopBuy()
        {
            QueryUpdateShopBuyCount query = new QueryUpdateShopBuyCount(manager.Owner.Uid, (int)ShopType, BuildBuyCountString());
            manager.Owner.server.GameDBPool.Call(query);
        }    
        #endregion
    }
}
