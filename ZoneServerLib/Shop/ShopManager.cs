using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class ShopManager
    {
        private Dictionary<ShopType, Shop> shopList = new Dictionary<ShopType, Shop>();
        private CommonShop commonShop;
        private List<int> cachedEquipQuelityList = new List<int>();
        private List<int> cachedSoulBoneQuelityList = new List<int>();
        public int BattlePower = 0;

        public PlayerChar Owner { get; private set; }

        public ShopManager(PlayerChar player)
        {
            this.Owner = player;
            commonShop = new CommonShop(this);
        }

        public void BindShopList(Dictionary<ShopType, DBShopInfo> shopList)
        {
            foreach (var kv in shopList)
            {
                BindShop(kv.Value);
            }
        }

        public Shop BindShop(DBShopInfo shopInfo)
        {
            Shop shop = null;
            switch (shopInfo.ShopType)
            {
                case ShopType.SoulBone:
                    shop = new SoulBoneShop(this);
                    break;
                default:
                    shop = new Shop(this, ShopType.None);
                    break;
            }

            shop.BindShopItems(shopInfo);
            shopList.Add(shopInfo.ShopType, shop);
            return shop;
        }    

        public Shop GetShop(ShopType shopType)
        {
            Shop shop;
            shopList.TryGetValue(shopType, out shop);

            if (shop == null)
            {
                shop = AddShop(shopType);
            }

            return shop;
        }

        public Shop LoadTransFormGetShop(ShopType shopType)
        {
            Shop shop;
            shopList.TryGetValue(shopType, out shop);          
            return shop;
        }

        public Dictionary<ShopType, Shop> GetShopList()
        {
            return shopList;
        }

        private Shop AddShop(ShopType shopType)
        {
            Shop shop = null;
            switch(shopType)
            {
                case ShopType.SoulBone:
                    shop = AddSoulBoneShop();               
                    break;             
            }
            shop.SyncDBShopInfo();
            shopList.Add(shop.ShopType, shop);
            return shop;
        }

        private Shop AddSoulBoneShop()
        {
            SoulBoneShop shop = new SoulBoneShop(this);

            //魂骨商店首次获取不会自动添加，需要先刷新
            //shop.Refresh();
            return shop;
        }           

        public void UpdateShop()
        {
        }

        //通用商城相关方法

        public void BindCommonShop(Dictionary<ShopType, DBShopInfo> shopList)
        {
            foreach (var kv in shopList)
            {
                commonShop.BindShopItems(kv.Key, kv.Value);
                commonShop.BindRefreshCount(kv.Key, kv.Value);
                commonShop.BindActivityRefreshed(kv.Key, kv.Value);
            }
            Cache5MaxBattlePowerHeroInfo();
            LoadBattlePower();
        }

        public CommonShop GetCommonShop(int shopId, CommonShopModel shopModel)
        {
            if (CheckNeedUpdateRegularShop(shopId))
            {
                UpdateNewShopItems(commonShop, shopId);
                SyncUpdateShopItems(shopId, commonShop);
            }

            if (!commonShop.ContainsShopType(shopId))
            {
                InitShop(shopId, shopModel);
            }
            return commonShop;
        }

        public CommonShop GetCommonShop()
        {      
            return commonShop;
        }

        public List<int> GetShopItemList(int shopId)
        {
            CommonShopModel shop = CommonShopLibrary.GetShopModel(shopId);
            if (shop == null)
            {
                return null;
            }
            List<int> returnList = new List<int>();

            //取出所有符合条件的商品
            if (shop.IsRegular)
            {
                List<CommonShopItemModel> regularItems = GetSuitableItemsByShopItemId(shop, Owner);
                AddRegularItems(returnList, regularItems);
                return returnList;
            }

            Dictionary<int, List<CommonShopItemModel>> items = GetSuitableItemsByGroup(shop, Owner);
            if (items == null)
            {
                Log.Warn($"player {Owner.Uid} get shop item list failed, not find suitable items");
                return null;
            }
            for (int i = shop.GroupCounts.Count - 1; i >= 0; i--)
            {
                int group = shop.GroupCounts[i][0];
                int num = shop.GroupCounts[i][1];

                List<CommonShopItemModel> shopItems;
                if (items.TryGetValue(group, out shopItems))
                {
                    if (shopItems.Count > num)
                    {
                        List<int> selectList = GetRandomItems(num, shopItems);
                        AddRandomItems(selectList, returnList);
                    }
                    else
                    {
                        foreach (var item in shopItems)
                        {
                            if (!returnList.Contains(item.Id))
                            {
                                returnList.Add(item.Id);
                                num--;
                            }
                        }
                    }
                }
            }
            return returnList;
        }

        private Dictionary<int, List<CommonShopItemModel>> GetSuitableItemsByGroup(CommonShopModel shop, PlayerChar owner)
        {
            Dictionary<int, HeroInfo> heroList = owner.HeroMng.GetHeroInfoList();
            List<int> heroIds = new List<int>();
            foreach (var kv in heroList)
            {
                heroIds.Add(kv.Key);
            }
            Dictionary<int, List<CommonShopItemModel>> suitableItems = GetSuitableItemsByGroup(shop.ShopId, BattlePower, owner.MainTaskId, heroIds, owner.server.Now());//暂时没特殊事件
            return suitableItems;
        }

        private List<CommonShopItemModel> GetSuitableItemsByShopItemId(CommonShopModel shop, PlayerChar owner)
        {
            Dictionary<int, HeroInfo> heroList = owner.HeroMng.GetHeroInfoList();
            List<int> heroIds = new List<int>();
            foreach (var kv in heroList)
            {
                heroIds.Add(kv.Key);
            }
            List<CommonShopItemModel> suitableItems = GetSuitableItemsByShopItemId(shop.ShopId, BattlePower, owner.MainTaskId, heroIds, owner.server.Now());//暂时没特殊事件
            return suitableItems;
        }

        public List<int> GetDailyRefreshShopIds(int uid)
        {
            return commonShop.GetDailyRefreshShopIds(uid);
        }

        public List<int> GetWeeklyRefreshShopIds(int uid)
        {
            return commonShop.GetWeeklyRefreshShopIds(uid);
        }

        public List<int> GetMonthlyRefreshShopIds(int uid)
        {
            return commonShop.GetMonthlyRefreshShopIds(uid);
        }

        public CommonShopItemModel GetShopItem(int shopItemId)
        {
            CommonShopItemModel shopItem = CommonShopLibrary.GetShopItemModel(shopItemId);
            return shopItem;
        }

        public MSG_ZGC_SHOP_INFO GetShopInfo(CommonShop shop, int shopId)
        {
            return shop.GenerateShopMsg(shopId);
        }

        private void AddShopItems(CommonShop shop, int shopId)
        {
            //从表里取
            List<int> shopItemIds = GetShopItemList(shopId);         
            //往商城页添加商品
            shop.RefreshItems(shopId, shopItemIds);
        }

        private void UpdateNewShopItems(CommonShop shop, int shopId)
        {
            List<int> itemIds = shop.GetShopItems(shopId);
            List<int> allItemIds = GetShopItemList(shopId);
            if (itemIds.Count == allItemIds.Count)
            {
                return;
            }
            else if (itemIds.Count > allItemIds.Count)
            {          
                foreach (var itemId in itemIds)
                {
                    if (!allItemIds.Contains(itemId))
                    {
                        shop.SubRegularShopItem(shopId, itemId);
                    }
                }
            }
            else
            {
                foreach (var itemId in allItemIds)
                {
                    if (!itemIds.Contains(itemId))
                    {
                        shop.AddRegularShopItem(shopId, itemId);
                    }
                }
            }
        }

        private void AddRegularItems(List<int> returnList, List<CommonShopItemModel> shopItems)
        {

            foreach (var item in shopItems)
            {             
                if (!returnList.Contains(item.Id))
                {
                    returnList.Add(item.Id);
                }            
            }
        }

        private void AddRandomItems(List<int> selectList, List<int> returnList)
        {
            foreach (var id in selectList)
            {
                returnList.Add(id);
            }
        }

        //商品随机算法
        private List<int> GetRandomItems(int needCount, List<CommonShopItemModel> shopItems)
        {
            //key:totalWeight, value:shopItemId
            Dictionary<int, int> weightDic = new Dictionary<int, int>();
            int weights = 0;
            int totalWeight = 0;
            Dictionary<int, int> weightSortDic = new Dictionary<int, int>();
            foreach (var item in shopItems)
            {
                weights += item.Weight;
                weightSortDic.Add(item.Id, item.Weight);
            }
            Dictionary<int,int> decWeightSortDic = weightSortDic.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
            List<int> selectList = new List<int>();
            //无权重商品直接按相同权重
            if (weights == 0)
            {
                foreach (var item in shopItems)
                {
                    selectList.Add(item.Id);
                    needCount--;
                    if (needCount == 0)
                    {
                        break;
                    }
                }
                return selectList;
            }

            for (int i = 0; i < shopItems.Count; i++)
            {             
                int weight = 0;
                if (i == shopItems.Count - 1)
                {
                    weight = 10000 - totalWeight;
                }
                else
                {
                    weight = shopItems[i].Weight * 10000 / weights;
                }
                totalWeight += weight;
                weightDic.Add(totalWeight, shopItems[i].Id);
            }

            Dictionary<int, int> decWeightDic = weightDic.OrderByDescending(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
            for (int i = 0; i < needCount; i++)
            {
                int random = RAND.Range(0, 9999);
                int tempWeight = 0;
                foreach (var kv in decWeightDic)
                {
                    if (random < kv.Key)
                    {
                        tempWeight = kv.Key;
                    }
                }
                int itemId;
                decWeightDic.TryGetValue(tempWeight, out itemId);
                if (!selectList.Contains(itemId))
                {
                    selectList.Add(itemId);
                    decWeightSortDic.Remove(itemId);
                }
            }
            int rest = needCount - selectList.Count;
            if (rest > 0)
            {
                foreach (var kv in decWeightSortDic)
                {
                    selectList.Add(kv.Key);
                    rest--;
                    if (rest == 0)
                    {
                        break;
                    }
                }
            }
            return selectList;
        }   
         
        public void UpdateShopItemBuyCount(CommonShop shop, int shopId, CommonShopItem item, int buyCount)
        {
            shop.UpdateItemBuyCount(item, buyCount);
            SyncUpdateShopItemBuyCount(shopId, shop);
        }

        private bool CheckNeedUpdateRegularShop(int shopId)
        {
            return ShopIsRegular(shopId) && commonShop.ContainsShopType(shopId);
        }

        private void InitShop(int shopId, CommonShopModel shopModel)
        {
            AddShopItems(commonShop, shopId);
            commonShop.InitActivityRefreshed(shopId, shopModel.ShowStart);
            SyncInsertShopInfo(shopId, commonShop);
            commonShop.InitRefreshCount(shopId);
        }

        public MSG_ZGC_SHOP_ITEM GenerateShopItemInfo(CommonShopItem shopItem)
        {
            MSG_ZGC_SHOP_ITEM item = new MSG_ZGC_SHOP_ITEM();
            item.Id = shopItem.ShopItemId;
            item.BuyNum = shopItem.BuyCount;
            return item;
        }

        /// <summary>
        /// 新建通用商店同步库
        /// </summary>
        private void SyncInsertShopInfo(int shopId, CommonShop shop)
        {
            Owner.server.GameDBPool.Call(new QueryInsertShopItems(Owner.Uid, shopId, 
                shop.BuildShopItemIdString(shopId), 
                shop.BuildCommonBuyCountString(shopId),
                shop.BuildItemInfosString(shopId),
                shop.GetShopActivityRefreshed(shopId)));
        }

        /// <summary>
        /// 刷新通用商店同步库
        /// </summary>
        public void SyncUpdateShopInfo(int shopId, CommonShop shop)
        {
            QueryUpdateShopInfo query = new QueryUpdateShopInfo(Owner.Uid, shopId, shop.RefrehCounts[shopId], shop.BuildShopItemIdString(shopId), shop.BuildCommonBuyCountString(shopId), shop.BuildItemInfosString(shopId), shop.ActivityRefreshDic[shopId]);
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncUpdateShopItems(int shopId, CommonShop shop)
        {
            QueryUpdateShopItems query = new QueryUpdateShopItems(Owner.Uid, shopId, shop.BuildShopItemIdString(shopId), shop.BuildCommonBuyCountString(shopId), shop.BuildItemInfosString(shopId));
            Owner.server.GameDBPool.Call(query);
        }

        /// <summary>
        /// 通用商店购买商品同步库
        /// </summary>
        private void SyncUpdateShopItemBuyCount(int shopId, CommonShop shop)
        {
            QueryUpdateShopItemBuyCount query = new QueryUpdateShopItemBuyCount(Owner.Uid, shopId, shop.BuildCommonBuyCountString(shopId));
            Owner.server.GameDBPool.Call(query);

        }

        private bool ShopIsRegular(int shopId)
        {
            return CommonShopLibrary.ShopIsRegular(shopId);
        }

        public void Cache5MaxBattlePowerHeroInfo()
        {
            cachedEquipQuelityList.Clear();
            cachedSoulBoneQuelityList.Clear();

            List<HeroInfo> heroInfos = new List<HeroInfo>();

            var enumable = Owner.HeroMng.GetHeroInfoList().Values.ToList().OrderByDescending(x => x.GetBattlePower()).GetEnumerator();
            while (enumable.MoveNext())
            {
                heroInfos.Add(enumable.Current);

                if (heroInfos.Count >= 5) break;
            }

            GetMaxBattlePowerHeroEquipQuality(heroInfos);
            GetMaxBattlePowerHeroSoulBoneQuality(heroInfos);
        }

        public int GetHighestEquipQuality(bool withShopLimit = true)
        {
            Cache5MaxBattlePowerHeroInfo();
            return GetShopItemQuality(RewardType.Equip, withShopLimit);
        }

        public int GetHighestSoulBoneQuality(bool withShopLimit = true)
        {
            Cache5MaxBattlePowerHeroInfo();
            return GetShopItemQuality(RewardType.SoulBone, withShopLimit);
        }

        private void GetMaxBattlePowerHeroEquipQuality(List<HeroInfo> heroInfos)
        {
            Dictionary<int, int> qualityCount = new Dictionary<int, int>();

            foreach (var kv in heroInfos)
            {
                for (int i = 1; i <= 4; i++)
                {
                    EquipmentItem equipment = Owner.EquipmentManager.GetEquipedItem(kv.Id, i);
                    //没穿装备默认0
                    cachedEquipQuelityList.Add(equipment == null ? 0 : equipment.Model.Grade);
                }
            }

            //没穿魂骨品质补0
            int needCount = 5 * 4;
            for (int i = 0; i < needCount - cachedEquipQuelityList.Count; i++)
            {
                cachedEquipQuelityList.Add(0);
            }
        }

        private void GetMaxBattlePowerHeroSoulBoneQuality(List<HeroInfo> heroInfos)
        {
            int addCount = 0;
            int needCount = 5 * 6;

            foreach (var kv in heroInfos)
            {
                List<SoulBone> soulBones = Owner.SoulboneMng.GetEnhancedHeroBones(kv.Id);
                if (soulBones == null) continue;
                addCount += soulBones.Count;

                soulBones.ForEach(x => cachedSoulBoneQuelityList.Add(x.Prefix));
            }

            //没穿魂骨品质补0
            for (int i = 0; i < needCount - addCount; i++)
            {
                cachedSoulBoneQuelityList.Add(0);
            }
        }

        public void LoadBattlePower()
        {
            OperateLoadMaxBattlePower operateLoad = new OperateLoadMaxBattlePower(Owner.Uid);
            Owner.server.GameRedis.Call(operateLoad, (object msg) =>
            {
                if ((int)msg == 1)
                {
                    BattlePower = operateLoad.BattlePower;
                    if (BattlePower == 0)
                    {
                        BattlePower = Owner.HeroMng.CalcBattlePower();
                    }
                }
            });
        }

        private int GetShopItemQuality(RewardType rewardType, bool withShopLimit = true)
        {
            switch (rewardType)
            {
                case RewardType.Equip:
                    {
                        int quality = ScriptManager.Shop.GetEquipQuality(cachedEquipQuelityList);
                        return withShopLimit ? Math.Min(CommonShopLibrary.EquipmentMaxQuality, quality) : quality;
                    }
                case RewardType.SoulBone:
                {
                    int quality = ScriptManager.Shop.GetSoulBoneQuality(cachedSoulBoneQuelityList);
                    return withShopLimit ? Math.Min(CommonShopLibrary.SoulBoneMaxQuality, quality) : quality;
                }
                default:
                    return 0;
            }
        }

        private List<CommonShopItemModel> GetSuitableItemsByShopItemId(int shopId, int battlePower, int finishedTaskId, List<int> heroIds, DateTime now)
        {
            Dictionary<int, CommonShopItemModel> shopItems = CommonShopLibrary.GetShopItems(shopId);
            if (shopItems == null)
            {
                return null;
            }
            List<CommonShopItemModel> suitableItems = new List<CommonShopItemModel>();
            foreach (var item in shopItems)
            {
                if (item.Value.StartDate != DateTime.MinValue && DateTime.Compare(now, item.Value.StartDate) < 0)
                {
                    continue;
                }
                if (item.Value.EndDate != DateTime.MaxValue && DateTime.Compare(now, item.Value.EndDate) >= 0)
                {
                    continue;
                }
                if (item.Value.RewardType == 0)
                {
                    continue;
                }
                if (item.Value.MinBattlePower != 0 && battlePower < item.Value.MinBattlePower)
                {
                    continue;
                }
                if (item.Value.MaxBattlePower != 0 && battlePower > item.Value.MaxBattlePower)
                {
                    continue;
                }
                if (item.Value.TaskId != 0 && finishedTaskId < item.Value.TaskId)
                {
                    continue;
                }
                if (item.Value.HeroIds.Count != 0)
                {
                    bool pass = false;
                    foreach (var heroId in item.Value.HeroIds)
                    {
                        if (heroIds.Contains(heroId))
                        {
                            pass = true;
                        }
                    }
                    if (!pass)
                    {
                        continue;
                    }
                }
                int quality = GetShopItemQuality((RewardType)item.Value.RewardType);
                if (quality != 0 && item.Value.MinQuality != 0 && quality < item.Value.MinQuality)
                {
                    continue;
                }
                if (quality != 0 && item.Value.MaxQuality != 0 && quality > item.Value.MaxQuality)
                {
                    continue;
                }
                if (item.Value.AccumulateDiamond != 0 && Owner.RechargeManager.AccumulateTotal < item.Value.AccumulateDiamond)
                {
                    continue;
                }
                suitableItems.Add(item.Value);
            }
            return suitableItems;
        }

        private Dictionary<int, List<CommonShopItemModel>> GetSuitableItemsByGroup(int shopId, int battlePower, int finishedTaskId, List<int> heroIds, DateTime now)
        {
            Dictionary<int, List<CommonShopItemModel>> groupList = CommonShopLibrary.GetShopGroupList(shopId);
            if (groupList == null)
            {
                return null;
            }

            Dictionary<int, List<CommonShopItemModel>> suitableItems = new Dictionary<int, List<CommonShopItemModel>>();
            foreach (var group in groupList)
            {
                List<CommonShopItemModel> list = new List<CommonShopItemModel>();
                foreach (var item in group.Value)
                {
                    if (item.StartDate != DateTime.MinValue && DateTime.Compare(now, item.StartDate) < 0)
                    {
                        continue;
                    }
                    if (item.EndDate != DateTime.MaxValue && DateTime.Compare(now, item.EndDate) >= 0)
                    {
                        continue;
                    }
                    if (item.RewardType == 0)
                    {
                        continue;
                    }
                    if (item.MinBattlePower != 0 && battlePower < item.MinBattlePower)
                    {
                        continue;
                    }
                    if (item.MaxBattlePower != 0 && battlePower > item.MaxBattlePower)
                    {
                        continue;
                    }
                    if (item.TaskId != 0 && finishedTaskId < item.TaskId)
                    {
                        continue;
                    }
                    if (item.HeroIds.Count != 0)
                    {
                        bool pass = false;
                        foreach (var heroId in item.HeroIds)
                        {
                            if (heroIds.Contains(heroId))
                            {
                                pass = true;
                            }
                        }
                        if (!pass)
                        {
                            continue;
                        }
                    }
                    int quality = GetShopItemQuality((RewardType)item.RewardType);
                    if (quality != 0 && item.MinQuality != 0 && quality < item.MinQuality)
                    {
                        continue;
                    }
                    if (quality != 0 && item.MaxQuality != 0 && quality > item.MaxQuality)
                    {
                        continue;
                    }
                    if (item.AccumulateDiamond != 0 && Owner.RechargeManager.AccumulateTotal < item.AccumulateDiamond)
                    {
                        continue;
                    }
                    list.Add(item);
                }
                suitableItems.Add(group.Key, list);
            }
            return suitableItems;
        }

        public void UpdateMaxBattlePower(int maxBattlePower)
        {
            if (maxBattlePower > BattlePower)
            {
                BattlePower = maxBattlePower;
            }
        }
    }
}
