using CommonUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class PettyGift : BaseGift
    {       
        public PettyGiftItem RefreshGiftItem { get; private set; }

        public PettyGift(int type) : base(type)
        {
            itemList = new Dictionary<int, BaseGiftItem>();
        }

        public void BindDbItems(Dictionary<int, BaseDbGiftItem> itemList)
        {
            foreach (var kv in itemList)
            {
                AddDbItem(kv.Value);
            }
        }

        protected override void AddDbItem(BaseDbGiftItem dbItem)
        {
            if (!itemList.ContainsKey(dbItem.Id))
            {
                BaseGiftItem item = CreateGiftItem(dbItem);
                itemList.Add(item.Id, item);
            }
        }

        protected override BaseGiftItem CreateGiftItem(BaseDbGiftItem baseDbItem)
        {
            DbPettyGiftItem dbItem = baseDbItem as DbPettyGiftItem;
            PettyGiftItem item = new PettyGiftItem();            
            item.Id = dbItem.Id;
            item.Type = dbItem.Type;
            item.BuyState = dbItem.BuyState;          
            item.CreateTime = dbItem.CreateTime;
            item.RefreshTime = dbItem.RefreshTime;
            item.CurFlag = dbItem.CurFlag;
            return item;
        }

        public override BaseGiftItem AddGiftItem(int id, int type, DateTime createTime, ulong uid = 0)
        {
            if (ItemList.ContainsKey(id))
            {
                return null;
            }
            BaseGiftItem item = CreateGiftItem(id, type, createTime, uid);
            itemList.Add(item.Id, item);
          
            return item;
        }

        protected override BaseGiftItem CreateGiftItem(int id, int type, DateTime createTime, ulong uid = 0)
        {
            PettyGiftItem item = new PettyGiftItem();
            item.Uid = uid;
            item.Id = id;
            item.Type = type;
            item.BuyState = (int)GiftBuyState.NotBuy;
            item.CreateTime = createTime;
            item.RefreshTime = createTime;
            item.CurFlag = 1;        
            return item;
        }
     
        public void ResetPettyGiftBuyState(PettyGiftItem giftItem)
        {
            giftItem.BuyState = (int)GiftBuyState.NotBuy;
        }

        public void UpdateRefreshTime(PettyGiftItem giftItem, DateTime refreshTime)
        {
            giftItem.RefreshTime = refreshTime;
        }

        public BaseGiftItem UpdatePettyMoneyGift(int giftId)
        {
            BaseGiftItem giftItem;
            ItemList.TryGetValue(giftId, out giftItem);
            if (giftItem.Type == (int)PettyGiftType.SixRmb)
            {
                giftItem.BuyState = (int)GiftBuyState.Received;
            }
            else
            {
                giftItem.BuyState = (int)GiftBuyState.Bought;
            }
            return giftItem;
        }

        public bool UpdateNextPettyMoneyGift(int giftId, int giftType, out PettyGiftItem giftItem)
        {
            bool isAdd = false;
            BaseGiftItem baseItem;           
            ItemList.TryGetValue(giftId, out baseItem);
            if (baseItem == null)
            {
                giftItem = new PettyGiftItem();
                giftItem.Id = giftId;
                giftItem.BuyState = (int)GiftBuyState.NotBuy;
                giftItem.Type = giftType;
                giftItem.CreateTime = ZoneServerApi.now;
                giftItem.RefreshTime = ZoneServerApi.now;
                itemList.Add(giftItem.Id, giftItem);
                isAdd = true;
            }
            else
            {
                giftItem = baseItem as PettyGiftItem;
                giftItem.BuyState = (int)GiftBuyState.NotBuy;
                giftItem.CreateTime = ZoneServerApi.now;
                giftItem.CurFlag = 0;
                isAdd = false;
            }
            return isAdd;
        }

        public void LoadItem(ZMZ_PETTY_GIFT_ITEM itemMsg)
        {
            if (!ItemList.ContainsKey(itemMsg.Id))
            {
                PettyGiftItem giftItem = CreateGiftItem(itemMsg);
                itemList.Add(giftItem.Id, giftItem);
            }
        }

        private PettyGiftItem CreateGiftItem(ZMZ_PETTY_GIFT_ITEM itemMsg)
        {
            PettyGiftItem giftItem = new PettyGiftItem();        
            giftItem.Id = itemMsg.Id;
            giftItem.Type = itemMsg.Type;
            giftItem.BuyState = itemMsg.BuyState;
            giftItem.CreateTime = Timestamp.TimeStampToDateTime(itemMsg.CreateTime);
            giftItem.RefreshTime = Timestamp.TimeStampToDateTime(itemMsg.RefreshTime);
            giftItem.CurFlag = itemMsg.CurFlag;
            return giftItem;
        }

        public void UpdateCurFlag(PettyGiftItem giftItem, int curFlag)
        {
            giftItem.CurFlag = curFlag;
        }

        public void SetRefreshPettyGift(PettyGiftItem giftItem)
        {
            RefreshGiftItem = giftItem;
        }
    }
}
