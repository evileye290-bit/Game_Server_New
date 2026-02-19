using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class BaseShopItem //通用商品基类
    {
        //商城商品表id，非道具id
        public int ShopItemId { get; private set; }
        public int BuyCount { get; set; }
        public string ItemInfo { get; set; }

        public BaseShopItem(int id, int buyCount, string info)
        {
            ShopItemId = id;
            BuyCount = buyCount;
            ItemInfo = info;
        }

        public virtual MSG_ZGC_SHOP_ITEM GenerateMsg()
        {
            MSG_ZGC_SHOP_ITEM msg = new MSG_ZGC_SHOP_ITEM()
            {
                Id = ShopItemId,
                BuyNum = BuyCount,
                ItemInfo = ItemInfo,
            };
            return msg;
        }
    }
}
