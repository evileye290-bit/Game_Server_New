using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class BaseGift
    {
        public int Type { get; private set; }

        //key：itemUid
        protected Dictionary<ulong, BaseGiftItem> limitTimeItemList;
        public Dictionary<ulong, BaseGiftItem> LimitTimeItemList => limitTimeItemList;

        //key:ItemId, uid
        protected Dictionary<int, Dictionary<ulong, BaseGiftItem>> sameIdGiftList;
        public Dictionary<int, Dictionary<ulong, BaseGiftItem>> SameIdGiftList => sameIdGiftList;

        //key:giftId
        protected Dictionary<int, BaseGiftItem> itemList;
        public Dictionary<int, BaseGiftItem> ItemList => itemList;

        public BaseGift(int type)
        {
            Type = type;
        }

        protected virtual void AddDbItem(BaseDbGiftItem dbItem)
        {
        }

        public virtual BaseGiftItem AddGiftItem(int id, int type, DateTime createTime, ulong uid = 0)
        {
            if (ItemList.ContainsKey(id))
            {
                return null;
            }
            BaseGiftItem item = CreateGiftItem(id, type, createTime, uid);
            itemList.Add(item.Id, item);
            return item;
        }

        protected virtual BaseGiftItem CreateGiftItem(BaseDbGiftItem dbItem)
        {
            BaseGiftItem item = new BaseGiftItem();
            item.Uid = dbItem.Uid;
            item.Id = dbItem.Id;
            item.Type = dbItem.Type;
            item.BuyState = dbItem.BuyState;           
            return item;
        }

        protected virtual BaseGiftItem CreateGiftItem(int id, int type, DateTime createTime, ulong uid = 0)
        {
            BaseGiftItem item = new BaseGiftItem();
            item.Uid = uid;
            item.Id = id;
            item.Type = type;
            item.BuyState = (int)GiftBuyState.NotBuy;
            return item;
        }
    }
}
