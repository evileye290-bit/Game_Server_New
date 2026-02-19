using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class CommonShop
    {
        private ShopManager manager;
        //key:shopId, shopItemId
        private Dictionary<int, Dictionary<int, CommonShopItem>> itemDic = new Dictionary<int, Dictionary<int, CommonShopItem>>();
        public Dictionary<int, Dictionary<int, CommonShopItem>> ItemDic => itemDic;

        private Dictionary<int, int> refreshCounts = new Dictionary<int, int>();
        public Dictionary<int, int> RefrehCounts => refreshCounts;

        public bool Changed { get; private set; }

        private Dictionary<int, bool> activityRefreshDic = new Dictionary<int, bool>();
        public Dictionary<int, bool> ActivityRefreshDic => activityRefreshDic;

        public CommonShop(ShopManager manager)
        {
            this.manager = manager;
        }

        public void BindShopItems(ShopType shopType, DBShopInfo shopInfo)
        {
            foreach (var kv in shopInfo.CommonItemList)
            {
                CommonShopItem item = BuildShopItem(kv.Value);          
                if(item == null) continue;

                AddItem((int)shopType, item);
            }

            if (Changed)
            {
                manager.SyncUpdateShopItems((int)shopType, this);
                Changed = false;
            }
        }

        public CommonShopItem BuildShopItem(int id)
        {
            CommonShopItemModel shopItem = CommonShopLibrary.GetShopItemModel(id);

            DBShopItemInfo dbShopItemInfo = new DBShopItemInfo() { Id = id, ShopItemId = id, BuyCount = 0, ItemInfo = shopItem.Reward };

            return BuildShopItem(dbShopItemInfo);
        }

        public CommonShopItem BuildShopItem(DBShopItemInfo itemInfo)
        {
            CommonShopItem item = null;
            CommonShopItemModel shopItem = CommonShopLibrary.GetShopItemModel(itemInfo.ShopItemId);
            if (shopItem == null)
            {
                Log.Warn($"shop item {itemInfo.ShopItemId} is null check it !");
                return null;
            }

            switch ((RewardType)shopItem.RewardType)
            {
                case RewardType.SoulBone:
                    SoulBoneShopItem soulBoneItem = new SoulBoneShopItem(itemInfo);

                    string rewardStr = string.IsNullOrEmpty(itemInfo.ItemInfo) ? shopItem.Reward : itemInfo.ItemInfo;
                    ItemBasicInfo basicInfo = ItemBasicInfo.Parse(rewardStr);
                    if (basicInfo.IsNeedFixSoulBone())
                    {
                        Changed = true;
                    }

                    SoulBone soulBoneInfo = SoulBoneManager.GenerateSoulBoneInfo(basicInfo, true);
                    if (soulBoneInfo != null)
                    {
                        soulBoneItem.SetRewardInfo(basicInfo.ToString(), soulBoneInfo);
                        item = soulBoneItem;
                    }
                    break;
                default:
                    item = new CommonShopItem(itemInfo);
                    break;
            }
            return item;
        }

        private void AddItem(int shopId, CommonShopItem item)
        {
            Dictionary<int, CommonShopItem> itemList;
            itemDic.TryGetValue(shopId, out itemList);
            if (itemList != null)
            {
                itemList.Add(item.ShopItemId, item);
            }
            else
            {
                itemList = new Dictionary<int, CommonShopItem>();
                itemList.Add(item.ShopItemId, item);
                itemDic.Add(shopId, itemList);
            }
        }

        public void BindRefreshCount(ShopType shopType, DBShopInfo shopInfo)
        {
            refreshCounts.Add((int)shopType, shopInfo.RefreshCount);
        }

        public void InitRefreshCount(int shopId)
        {
            refreshCounts.Add(shopId, 0);
        }

        public bool ContainsShopType(int shopId)
        {
            Dictionary<int, CommonShopItem> itemList;         
            if (itemDic.TryGetValue(shopId, out itemList))
            {
                return true;
            }
            return false;
        }

        public CommonShopItem GetShopItem(int shopId, int shopItemId)
        {
            CommonShopItem item;
            Dictionary<int, CommonShopItem> itemList;
            itemDic.TryGetValue(shopId, out itemList);
            if (itemList == null)
            {
                return null;
            }
            itemList.TryGetValue(shopItemId, out item);
            return item;
        }

        public List<int> GetShopItems(int shopId)
        {
            Dictionary<int, CommonShopItem> itemList;
            itemDic.TryGetValue(shopId, out itemList);
            if (itemList == null)
            {
                return null;
            }
            List<int> items = new List<int>();
            foreach (var item in itemList)
            {
                items.Add(item.Key);
            }
            return items;
        }     

        public List<int> GetDailyRefreshShopIds(int uid)
        {
            List<int> list = new List<int>();
            foreach (var kv in itemDic)
            {
                CommonShopModel model = CommonShopLibrary.GetShopModel(kv.Key);
                if (model == null)
                {
                    Log.Warn($"player {uid} refresh daily shop failed, not have shop model {kv.Key}");
                    continue;
                }
                if (model.RefreshFreq == RefreshFreqType.Daily)
                {
                    list.Add(kv.Key);
                }
            }
            return list;
        }

        public List<int> GetWeeklyRefreshShopIds(int uid)
        {
            List<int> list = new List<int>();
            foreach (var kv in itemDic)
            {
                CommonShopModel model = CommonShopLibrary.GetShopModel(kv.Key);
                if (model == null)
                {
                    Log.Warn($"player {uid} refresh weekly shop failed, not have shop model {kv.Key}");
                    continue;
                }
                if (model.RefreshFreq == RefreshFreqType.Weekly)
                {
                    list.Add(kv.Key);
                }
            }
            return list;
        }

        public List<int> GetMonthlyRefreshShopIds(int uid)
        {
            List<int> list = new List<int>();
            foreach (var kv in itemDic)
            {
                CommonShopModel model = CommonShopLibrary.GetShopModel(kv.Key);
                if (model == null)
                {
                    Log.Warn($"player {uid} refresh monthly shop failed, not have shop model {kv.Key}");
                    continue;
                }
                if (model.RefreshFreq == RefreshFreqType.Monthly)
                {
                    list.Add(kv.Key);
                }
            }
            return list;
        }

        public void RefreshItems(int shopId, List<int> shopItemIds)
        {
            if (itemDic.ContainsKey(shopId))
            {
                itemDic.Remove(shopId);
            }
            foreach (var shopItemId in shopItemIds)
            {
                //根据商品的奖励类型生成商品信息              
                AddShopItem(shopId, shopItemId);
            }
        }

        private void AddShopItem(int shopId, int shopItemId)
        {         
            CommonShopItemModel shopItem = manager.GetShopItem(shopItemId);
            if (string.IsNullOrEmpty(shopItem.Reward))
            {
                return;
            }               
            
            AddCommonShopItem(shopId, shopItem);                   
        }         

        //public void AddShopItem(int shopId, int shopItemId, int buyCount, string itemInfo)
        //{
        //    Dictionary<int, CommonShopItem> itemList;
        //    itemDic.TryGetValue(shopId, out itemList);

        //    DBShopItemInfo dbShopItemInfo = new DBShopItemInfo(){Id = shopItemId, ShopItemId =  shopItemId, BuyCount = buyCount, ItemInfo = itemInfo};

        //    if (itemList != null)
        //    {
        //        CommonShopItem item = new CommonShopItem(dbShopItemInfo);
        //        if (!itemList.ContainsKey(shopItemId))
        //        {
        //            itemList.Add(shopItemId, item);
        //        }
        //    }
        //    else
        //    {
        //        itemList = new Dictionary<int, CommonShopItem>();
        //        CommonShopItem item = new CommonShopItem(dbShopItemInfo);
        //        itemList.Add(shopItemId, item);
        //        itemDic.Add(shopId, itemList);
        //    }
        //}

        public void LoadCommonShopItem(int shopId, int shopItemId, int buyCount, string itemInfo)
        {
            DBShopItemInfo dbShopItemInfo = new DBShopItemInfo()
            {
                Id = shopItemId,
                ShopItemId = shopItemId,
                BuyCount = buyCount,
                ItemInfo = itemInfo
            };

            CommonShopItem item = BuildShopItem(dbShopItemInfo);
            AddItem(shopId, item);
        }

        private void AddCommonShopItem(int shopId, CommonShopItemModel shopItem)
        {
            DBShopItemInfo dbShopItemInfo = new DBShopItemInfo()
            {
                Id = shopItem.Id,
                ShopItemId = shopItem.Id, 
                BuyCount = 0,
                ItemInfo = shopItem.Reward
            };

            CommonShopItem item = BuildShopItem(dbShopItemInfo);   
            AddItem(shopId, item);
        }

        public string BuildShopItemIdString(int shopId)
        {
            Dictionary<int, CommonShopItem> itemList;
            itemDic.TryGetValue(shopId, out itemList);
            if (itemList == null)
            {
                return "";
            }
            return string.Join("|", itemList.Keys);
        }

        public string BuildCommonBuyCountString(int shopId)
        {
            Dictionary<int, CommonShopItem> itemList;
            itemDic.TryGetValue(shopId, out itemList);
            if (itemList == null)
            {
                return "";
            }          
            return string.Join("|", itemList.Values.Select(x => x.BuyCount));
        }

        public string BuildItemInfosString(int shopId)
        {
            Dictionary<int, CommonShopItem> itemList;
            if (!itemDic.TryGetValue(shopId, out itemList)) return string.Empty;

            return string.Join("|", itemList.Values.Select(x => x.ItemInfo));
        }

        public int GetRefreshCount(int shopId)
        {
            int refreshCount;
            refreshCounts.TryGetValue(shopId, out refreshCount);
            return refreshCount;
        }

        public void AddRefreshCount(int shopId, int count = 1)
        {

            if (!refreshCounts.ContainsKey(shopId))
            {
                refreshCounts.Add(shopId, count);
            }
            else
            {
                refreshCounts[shopId] += count;
            }
        }

        public void Refresh(int shopId)
        {
            manager.Cache5MaxBattlePowerHeroInfo();
            //刷新商品
            List<int> shopItemIds = manager.GetShopItemList(shopId);
            RefreshItems(shopId, shopItemIds);
        }

        public void ResetRefreshCount(int shopId)
        {
            refreshCounts[shopId] = 0;
        }

        public void UpdateItemBuyCount(CommonShopItem item, int buyCount)
        {
            item.BuyCount += buyCount;       
        }

        public MSG_ZGC_SHOP_INFO GenerateShopMsg(int shopId)
        {
            MSG_ZGC_SHOP_INFO msg = new MSG_ZGC_SHOP_INFO();

            msg.ShopType = shopId;

            msg.RefreshCount = GetRefreshCount(shopId);

            Dictionary<int, CommonShopItem> itemList;
            itemDic.TryGetValue(shopId, out itemList);
            foreach (var kv in itemList)
            {
                msg.ShopItems.Add(kv.Value.GenerateMsg());
            }
            return msg;
        }

        public void AddRegularShopItem(int shopId, int shopItemId)
        {
            Dictionary<int, CommonShopItem> itemList;
            itemDic.TryGetValue(shopId, out itemList);
            CommonShopItem item = BuildShopItem(shopItemId);
            itemList.Add(shopItemId, item);
        }

        public void SubRegularShopItem(int shopId, int shopItemId)
        {
            Dictionary<int, CommonShopItem> itemList;
            itemDic.TryGetValue(shopId, out itemList);
            itemList.Remove(shopItemId);
        }

        public void BindActivityRefreshed(ShopType shopType, DBShopInfo shopInfo)
        {
            activityRefreshDic.Add((int)shopType, shopInfo.ActivityRefreshed);
        }

        public void InitActivityRefreshed(int shopId, DateTime showStart)
        {
            if (showStart != DateTime.MinValue)
            {
                activityRefreshDic.Add(shopId, true);
            }
            else
            {
                activityRefreshDic.Add(shopId, false);
            }
        }
      
        public void UpdateActivityRefreshed(int shopId, bool refreshed)
        {
            if (!activityRefreshDic.ContainsKey(shopId))
            {
                activityRefreshDic.Add(shopId, refreshed);
            }
            else
            {
                activityRefreshDic[shopId] = refreshed;
            }
        }

        public bool GetShopActivityRefreshed(int shopId)
        {
            bool flag;
            activityRefreshDic.TryGetValue(shopId, out flag);
            return flag;
        }
    }
}
