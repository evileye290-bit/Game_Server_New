using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class ShopItem
    {
        public int Id { get; private set; }
        public int BuyCount { get; set; }
        public string ItemInfo { get; protected set; }

        public ShopItem(int id, int buyCount, string info)
        {
            Id = id;
            BuyCount = buyCount;
            ItemInfo = info;
        }

        /// <summary>
        /// 获取价格
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual int GetPrice(int id)
        {
            return 0;
        }

        public virtual string GetItemInfo()
        {
            return string.Empty;
        }

        public virtual MSG_ZGC_SHOP_ITEM GenerateMsg()
        {
            MSG_ZGC_SHOP_ITEM msg = new MSG_ZGC_SHOP_ITEM()
            {
                Id = Id,
                BuyNum = BuyCount,
                ItemInfo = ItemInfo,
            };
            return msg;
        }
    }
}
