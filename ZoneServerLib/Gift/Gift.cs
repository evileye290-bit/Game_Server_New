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
    public class Gift
    {
        //key:itemId
        private Dictionary<int, GiftItem> itemList = new Dictionary<int, GiftItem>();
        public Dictionary<int, GiftItem> ItemList => itemList;

        //key:subtype, itemid
        private Dictionary<int, Dictionary<int, GiftItem>> subTypeList = new Dictionary<int, Dictionary<int, GiftItem>>();
        public Dictionary<int, Dictionary<int, GiftItem>> SubTypeList => subTypeList;

        //key：itemUid
        private Dictionary<ulong, GiftItem> limitTimeItemList = new Dictionary<ulong, GiftItem>();
        public Dictionary<ulong, GiftItem> LimitTimeItemList => limitTimeItemList;

        //key:ItemId, uid
        private Dictionary<int, Dictionary<ulong, GiftItem>> sameIdTimingGifts = new Dictionary<int, Dictionary<ulong, GiftItem>>();
        public Dictionary<int, Dictionary<ulong, GiftItem>> SameIdTimingGifts => sameIdTimingGifts;

        //key:ItemId, uid
        //private Dictionary<int, Dictionary<ulong, GiftItem>> sameIdTimingGifts = new Dictionary<int, Dictionary<ulong, GiftItem>>();
        //public Dictionary<int, Dictionary<ulong, GiftItem>> SameIdTimingGifts => sameIdTimingGifts;

        public void BindDbItems(Dictionary<int, DbGiftItem> itemList)
        {
            foreach (var kv in itemList)
            {
                AddDbItem(kv.Value);
            }
        }

        public void BindLimitTimeDbItems(Dictionary<ulong, DbGiftItem> itemList)
        {
            foreach (var kv in itemList)
            {
                AddLimitTimeDbItem(kv.Value);
            }
        }

        private void AddDbItem(DbGiftItem dbItem)
        {
            RechargeItemModel model = RechargeLibrary.GetRechargeItemOrSdkItem(dbItem.Id);
            if (model != null && !itemList.ContainsKey(dbItem.Id))
            {
                GiftItem item = new GiftItem(dbItem.Id, dbItem.BuyCount, dbItem.CurBuyCount, dbItem.DoubleFlag, dbItem.Discount, dbItem.DiamondRatio);
                itemList.Add(dbItem.Id, item);

                if (model.GiftType != RechargeGiftType.Common)
                {
                    return;
                }
                Dictionary<int, GiftItem> giftItemList;
                if (!subTypeList.TryGetValue(model.SubType, out giftItemList))
                {
                    giftItemList = new Dictionary<int, GiftItem>();
                    giftItemList.Add(item.Id, item);
                    subTypeList.Add(model.SubType, giftItemList);//
                }
                else
                {
                    giftItemList.Add(item.Id, item);
                }
            }
        }    

        private void AddLimitTimeDbItem(DbGiftItem dbItem)
        {
            if (!limitTimeItemList.ContainsKey(dbItem.Uid))
            {
                GiftItem item = new GiftItem(dbItem.Uid, dbItem.Id, dbItem.BuyCount, dbItem.StartTime, dbItem.IsSdkGift, dbItem.DataBox);
                limitTimeItemList.Add(item.Uid, item);

                Dictionary<ulong, GiftItem> sameGiftItems;
                if (sameIdTimingGifts.TryGetValue(dbItem.Id, out sameGiftItems))
                {
                    sameGiftItems.Add(item.Uid, item);
                }
                else
                {
                    sameGiftItems = new Dictionary<ulong, GiftItem>();
                    sameGiftItems.Add(item.Uid, item);
                    sameIdTimingGifts.Add(item.Id, sameGiftItems);
                }
            }
           
        }

        public string BuildGiftItemIdString()
        {         
            if (itemList == null)
            {
                return "";
            }
            return string.Join("|", itemList.Keys);
        }

        public string BuildGiftBuyCountString()
        {                  
            if (itemList == null)
            {
                return "";
            }
            return string.Join("|", itemList.Values.Select(x => x.BuyCount));
        }    

        public string BuildGiftCurBuyCountString()
        {
            if (itemList == null)
            {
                return "";
            }
            return string.Join("|", itemList.Values.Select(x => x.CurBuyCount));
        }

        public string BuildGiftDoubleFlagString()
        {
            if (itemList == null)
            {
                return "";
            }
            return string.Join("|", itemList.Values.Select(x => x.DoubleFlag));
        }

        public string BuildGiftDiscountString()
        {
            if (itemList == null)
            {
                return "";
            }
            return string.Join("|", itemList.Values.Select(x => x.Discount));
        }

        public string BuildGifItemStartTimeString()
        {
            if (itemList == null)
            {
                return "";
            }
            return string.Join("|", itemList.Values.Select(x => x.StartTime));
        }

        public string BuildDiamondRatioString()
        {
            if (itemList == null)
            {
                return "";
            }
            return string.Join("|", itemList.Values.Select(x => x.DiamondRatio));
        }
        
        public GiftItem CreateItem(RechargeItemModel model, PlayerChar owner)
        {
            GiftItem item = new GiftItem(model.Id, 1, 1, 0, 0, 1);
            if (!itemList.ContainsKey(item.Id))
            {
                itemList.Add(item.Id, item);
            }
            if (model.GiftType == RechargeGiftType.Common)
            {
                Dictionary<int, GiftItem> giftItemList;
                if (subTypeList.TryGetValue(model.SubType, out giftItemList))
                {
                    giftItemList.Add(item.Id, item);
                }
                else
                {
                    giftItemList = new Dictionary<int, GiftItem>();
                    giftItemList.Add(item.Id, item);
                    subTypeList.Add(model.SubType, giftItemList);
                }
            }
            return item;
        }

        public GiftItem CreateLimitTimeGiftItem(ulong uid, RechargeItemModel model, string startTime, bool isSdkGift, string dataBox)
        {
            GiftItem item = new GiftItem(uid, model.Id, 0, startTime, isSdkGift, dataBox);
            if (!limitTimeItemList.ContainsKey(item.Uid))
            {
                limitTimeItemList.Add(item.Uid, item);
            }
            Dictionary<ulong, GiftItem> sameItemList;
            if (sameIdTimingGifts.TryGetValue(item.Id, out sameItemList))
            {
                sameItemList.Add(item.Uid, item);
            }
            else
            {
                sameItemList = new Dictionary<ulong, GiftItem>();
                sameItemList.Add(item.Uid, item);
                sameIdTimingGifts.Add(item.Id, sameItemList);
            }
            return item;
        }

        public GiftItem UpdateGiftItem(RechargeItemModel model, PlayerChar owner)
        {
            GiftItem item;
            if (model.GiftType == RechargeGiftType.LimitTime)
            {
                item = UpdateLimitTimeGiftItemInfo(model);
            }
            else
            {
                item = UpdateRegularGiftItemInfo(model, owner);
                CheckRecordBuyAllFirstPayDiamond(owner, item.Id);
            }
            return item;
        }

        private GiftItem UpdateLimitTimeGiftItemInfo(RechargeItemModel model)
        {
            GiftItem item = RandomNotBoughtLimitTimeGift(model.Id);
            if (item != null)
            {
                item.UpdateBuyCount();
            }
            return item;
        }

        private GiftItem UpdateRegularGiftItemInfo(RechargeItemModel model, PlayerChar owner)
        {
            GiftItem item;
            if (itemList.TryGetValue(model.Id, out item))
            {
                item.UpdateBuyCount();
                item.CheckChangeDoubleFlag();
                item.CheckChangeDiscount();
                item.CheckChangeDiamondRatio();
            }
            else
            {
                item = CreateItem(model, owner);
            }
            return item;
        }

        public int GetRechargeItemRatio(int rechargeItemId)
        {
            //Dictionary<int, GiftItem> giftItemList;
            //SubTypeList.TryGetValue((int)CommonGiftType.Diamond, out giftItemList);
            //if (giftItemList == null)
            //{
            //    return true;
            //}
            GiftItem item;
            ItemList.TryGetValue(rechargeItemId, out item);
            if (item == null)
            {
                return 2;
            }
            return item.DiamondRatio;
        }

        public List<GiftItem> RefreshFirstRechargeItemDoubleFlag()
        {
            List<GiftItem> giftList  = new List<GiftItem>();
            foreach (var item in ItemList)
            {
                if (item.Value.DoubleFlag == 0)
                {
                    item.Value.ResetDoubleFlag();
                    giftList.Add(item.Value);
                }
            }
            return giftList;
        }

        public void ReSetItemCurBuyCount(int itemId)
        {
            GiftItem item;
            ItemList.TryGetValue(itemId, out item);
            if (item != null)
            {
                item.ReSetItemCurBuyCount();
            }
        }

        public List<GiftItem> RefreshRechargeDiscount()
        {
            List<GiftItem> giftList = new List<GiftItem>();
            foreach (var item in ItemList)
            {
                if (item.Value.Discount == 0)
                {
                    item.Value.ResetDiscount();
                    giftList.Add(item.Value);
                }
            }
            return giftList;
        }

        public GiftItem LoadItem(RechargeItemModel model, PlayerChar owner, ZMZ_GIFT_ITEM itemInfo)
        {
            GiftItem item;
            if (model.GiftType != RechargeGiftType.LimitTime)
            {
                item = new GiftItem(itemInfo.ItemId, itemInfo.BuyCount, itemInfo.CurBuyCount, itemInfo.DoubleFlag, itemInfo.Discount, itemInfo.RewardRatio, itemInfo.IsSdkGift, itemInfo.DataBox);
                if (!itemList.ContainsKey(item.Id))
                {
                    itemList.Add(item.Id, item);
                }
                if (model.GiftType == RechargeGiftType.Common)
                {
                    Dictionary<int, GiftItem> giftItemList;
                    if (subTypeList.TryGetValue(model.SubType, out giftItemList))
                    {
                        giftItemList.Add(item.Id, item);
                    }
                    else
                    {
                        giftItemList = new Dictionary<int, GiftItem>();
                        giftItemList.Add(item.Id, item);
                        subTypeList.Add(model.SubType, giftItemList);
                    }
                }
            }
            else
            {
                item = new GiftItem(itemInfo.Uid, itemInfo.ItemId, itemInfo.BuyCount, itemInfo.StartTime, itemInfo.IsSdkGift, itemInfo.DataBox);
                if (!limitTimeItemList.ContainsKey(item.Uid))
                {
                    limitTimeItemList.Add(item.Uid, item);
                }
                Dictionary<ulong, GiftItem> sameItems;
                if (SameIdTimingGifts.TryGetValue(item.Id, out sameItems))
                {
                    sameItems.Add(item.Uid, item);
                }
                else
                {
                    sameItems = new Dictionary<ulong, GiftItem>();
                    sameItems.Add(item.Uid, item);
                    SameIdTimingGifts.Add(item.Id, sameItems);
                }
            }
            return item;
        }

        public GiftItem GetGiftItem(int itemId)
        {
            GiftItem item;
            ItemList.TryGetValue(itemId, out item);
            return item;
        }

        public bool CheckHasDiscountPrice(int giftItemId)
        {
            GiftItem item;
            ItemList.TryGetValue(giftItemId, out item);
            if (item == null || item.Discount == 1)
            {
                return true;
            }
            return false;
        }

        public void CheckRecordBuyAllFirstPayDiamond(PlayerChar owner, int itemId)
        {
            Dictionary<int, GiftItem> diamondItems;
            if (SubTypeList.TryGetValue((int)CommonGiftType.Diamond, out diamondItems))
            {
                bool allBought = true;
                foreach (var item in diamondItems)
                {
                    if (item.Value.DoubleFlag == 1)
                    {
                        allBought = false;
                    }
                }
                if (diamondItems.Count == RechargeLibrary.GetDiamondRechargeItemTotalCount() && allBought)
                {
                    owner.ActionManager.RecordActionAndCheck(ActionType.BuyAllFirstPayDiamond, itemId);
                }
            }
        }

        public bool CheckHasBuyCount(RechargeItemModel model)
        {
            GiftItem item;
            if (model.GiftType == RechargeGiftType.LimitTime)
            {              
                if (CheckHaveNotBoughtLimitTimeGift(model.Id))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (ItemList.TryGetValue(model.Id, out item))
                {
                    return item.CurBuyCount < model.BuyLimit;
                }
                return true;
            }
        }

        public bool CheckHaveNotBoughtLimitTimeGift(int itemId)
        {
            Dictionary<ulong, GiftItem> itemList;
            if (SameIdTimingGifts.TryGetValue(itemId, out itemList))
            {
                List<GiftItem> notBoughtGifts = itemList.Values.Where(x => x.BuyCount == 0 && (ZoneServerApi.now - DateTime.Parse(x.StartTime)).TotalSeconds < RechargeLibrary.GiftLimitTime * 3600).ToList();
                GiftItem item = notBoughtGifts.FirstOrDefault();
                if (item != null)
                {
                    return true;
                }
            }
            return false;
        }

        public GiftItem RandomNotBoughtLimitTimeGift(int itemId)
        {
            GiftItem item = null;
            Dictionary<ulong, GiftItem> itemList;
            if (SameIdTimingGifts.TryGetValue(itemId, out itemList))
            {
                List<GiftItem> notBoughtGifts = itemList.Values.Where(x => x.BuyCount == 0 && (ZoneServerApi.now - DateTime.Parse(x.StartTime)).TotalSeconds < RechargeLibrary.GiftLimitTime * 3600).ToList();
                item = notBoughtGifts.OrderBy(x => DateTime.Parse(x.StartTime)).FirstOrDefault();
            }
            return item;
        }
    }
}
