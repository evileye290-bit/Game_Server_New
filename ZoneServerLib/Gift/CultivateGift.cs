using CommonUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CultivateGift : BaseGift
    {        
        public CultivateGift(int type) : base(type)
        {
            limitTimeItemList = new Dictionary<ulong, BaseGiftItem>();
            sameIdGiftList = new Dictionary<int, Dictionary<ulong, BaseGiftItem>>();
        }

        public void BindDbItems(Dictionary<ulong, BaseDbGiftItem> itemList)
        {
            foreach (var kv in itemList)
            {
                AddDbItem(kv.Value);
            }
        }

        protected override void AddDbItem(BaseDbGiftItem dbItem)
        {         
            if (!limitTimeItemList.ContainsKey(dbItem.Uid))
            {
                BaseGiftItem item = CreateGiftItem(dbItem);
                limitTimeItemList.Add(item.Uid, item);

                Dictionary<ulong, BaseGiftItem> sameGiftItems;
                if (sameIdGiftList.TryGetValue(item.Id, out sameGiftItems))
                {
                    sameGiftItems.Add(item.Uid, item);
                }
                else
                {
                    sameGiftItems = new Dictionary<ulong, BaseGiftItem>();
                    sameGiftItems.Add(item.Uid, item);
                    sameIdGiftList.Add(item.Id, sameGiftItems);
                }
            }
        }

        protected override BaseGiftItem CreateGiftItem(BaseDbGiftItem baseDbItem)
        {
            DbGift2Item dbItem = baseDbItem as DbGift2Item;
            CultivateGiftItem item = new CultivateGiftItem();
            item.Uid = dbItem.Uid;
            item.Id = dbItem.Id;
            item.Type = dbItem.Type;
            item.BuyState = dbItem.BuyState;
            item.CreateTime = dbItem.CreateTime;
            return item;
        }

        public override BaseGiftItem AddGiftItem(int id, int type, DateTime createTime, ulong uid = 0)
        {
            if (LimitTimeItemList.ContainsKey(uid))
            {
                return null;
            }
            BaseGiftItem item = CreateGiftItem(id, type, createTime, uid);
            limitTimeItemList.Add(item.Uid, item);

            Dictionary<ulong, BaseGiftItem> sameGiftItems;
            if (sameIdGiftList.TryGetValue(item.Id, out sameGiftItems))
            {
                sameGiftItems.Add(item.Uid, item);
            }
            else
            {
                sameGiftItems = new Dictionary<ulong, BaseGiftItem>();
                sameGiftItems.Add(item.Uid, item);
                sameIdGiftList.Add(item.Id, sameGiftItems);
            }
            return item;
        }

        protected override BaseGiftItem CreateGiftItem(int id, int type, DateTime createTime, ulong uid = 0)
        {
            CultivateGiftItem item = new CultivateGiftItem();
            item.Uid = uid;
            item.Id = id;
            item.Type = type;
            item.BuyState = (int)GiftBuyState.NotBuy;
            item.CreateTime = createTime;
            return item;
        }

        public void LoadItem(ZMZ_CULTIVATE_GIFT_ITEM itemMsg)
        {
            if (!LimitTimeItemList.ContainsKey(itemMsg.Uid))
            {
                CultivateGiftItem item = CreateGiftItem(itemMsg);
                limitTimeItemList.Add(item.Uid, item);

                Dictionary<ulong, BaseGiftItem> sameGiftItems;
                if (sameIdGiftList.TryGetValue(item.Id, out sameGiftItems))
                {
                    sameGiftItems.Add(item.Uid, item);
                }
                else
                {
                    sameGiftItems = new Dictionary<ulong, BaseGiftItem>();
                    sameGiftItems.Add(item.Uid, item);
                    sameIdGiftList.Add(item.Id, sameGiftItems);
                }
            }
        }

        private CultivateGiftItem CreateGiftItem(ZMZ_CULTIVATE_GIFT_ITEM itemMsg)
        {
            CultivateGiftItem item = new CultivateGiftItem();
            item.Uid = itemMsg.Uid;
            item.Id = itemMsg.Id;
            item.Type = itemMsg.Type;
            item.BuyState = itemMsg.BuyState;
            item.CreateTime = Timestamp.TimeStampToDateTime(itemMsg.CreateTime);
            return item;
        }
    }
}
