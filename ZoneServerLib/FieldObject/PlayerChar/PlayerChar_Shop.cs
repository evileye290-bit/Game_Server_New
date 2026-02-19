using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar : FieldObject
    {
        //商店
        public ShopManager ShopManager { get; private set; }

        private void InitShopManager()
        {
            ShopManager = new ShopManager(this);
        }

        public void GetShopInfo(ShopType type)
        {
            Shop shop = ShopManager.GetShop(type);
            if (shop == null)
            {
                Log.Warn($"player {uid} get shop error, have not this shop {type}");
                return;
            }

            MSG_ZGC_SHOP_INFO msg = shop.GenerateShopMsg();
            Write(msg);
        }

        public void ShopBuyItem(ShopType type, int itemId, int buyNum)
        {
            Shop shop = ShopManager.GetShop(type);
            if (shop == null)
            {
                Log.Warn($"player {uid} buy item {itemId} shop error, have not this shop {type}");
                return;
            }

            ShopItem item = shop.GetShopItem(itemId);
            if (item == null)
            {
                return;
            }
        }

        public void ShopRefresh(ShopType type)
        {
            Shop shop = ShopManager.GetShop(type);
            if (shop == null)
            {
                Log.Warn($"player {uid} refresh shop error, have not this shop {type}");
                return;
            }
            ShopModel model = ShopLibrary.GetShopModel(type);
            if (model == null)
            {
                Log.Warn($"player {uid} refresh shop error, have not shop model {type}");
                return;
            }

            if (!model.CanFreshByUser())
            {
                MSG_ZGC_SHOP_REFRESH msg = new MSG_ZGC_SHOP_REFRESH();
                msg.ShopType = (int)type;
                msg.Result = (int)ErrorCode.ShopCannotRefresh;
                Write(msg);
                Log.Warn($"player {uid} refresh shop error, shop cannot refresh");
                return;
            }

            switch (type)
            {
                case ShopType.SoulBone:
                    FreshSoulBoneShop(shop);
                    break;
                default:
                    FreshShop(shop, model);
                    break;
            }
        }

        private void FreshSoulBoneShop(Shop shop)
        {
            MSG_ZGC_SHOP_REFRESH msg = new MSG_ZGC_SHOP_REFRESH();
            msg.ShopType = (int)shop.ShopType;

            //魂骨商店必须先吧奖励领取完成之后才能刷新
            if (shop.ItemList.Count > 0)
            {
                Log.Warn($"player {uid} refresh soulbone shop error, have rest items");
                msg.Result = (int)ErrorCode.SoulBoneShopNeedReward;
                Write(msg);
                return;
            }

            if (GetCoins(CurrenciesType.secretAreaCoin) < SecretAreaLibrary.HuntingPrice)
            {
                Log.Warn($"player {uid} refresh soulbone shop error: coins not enough, curCoin {GetCoins(CurrenciesType.secretAreaCoin)} cost {SecretAreaLibrary.HuntingPrice}");
                msg.Result = (int)ErrorCode.NoCoin;//货币不足
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.secretAreaCoin, SecretAreaLibrary.HuntingPrice, ConsumeWay.Shop, shop.ShopType.ToString());
            shop.Refresh();

            msg.ShopInfo = shop.GenerateShopMsg();
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        private void FreshShop(Shop shop, ShopModel model)
        {
            MSG_ZGC_SHOP_REFRESH msg = new MSG_ZGC_SHOP_REFRESH();
            msg.ShopType = (int)shop.ShopType;

            int count = model.GetRefreshCost(shop.RefreshCount);
            if (GetCoins(model.CurrenciesType) < count)
            {
                msg.Result = (int)ErrorCode.NoCoin;//货币不足
                Write(msg);
                return;
            }

            DelCoins(model.CurrenciesType, count, ConsumeWay.Shop, shop.ShopType.ToString());

            shop.Refresh();
            msg.ShopInfo = shop.GenerateShopMsg();
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        /// <summary>
        /// 魂骨商店增益
        /// </summary>
        public void ShopSoulBoneBonus()
        {
            Shop shop = ShopManager.GetShop(ShopType.SoulBone);
            if (shop == null)
            {
                Log.Warn($"player {uid} ShopSoulBoneBonus error, have not this shop {ShopType.SoulBone}");
                return;
            }

            MSG_ZGC_SHOP_SOULBONE_BONUS msg = new MSG_ZGC_SHOP_SOULBONE_BONUS();

            //魂骨商店必须先有奖励才能增益
            if (shop.ItemList.Count <= 0)
            {
                Log.Warn($"player {uid} ShopSoulBoneBonus error, not have shop items");
                msg.Result = (int)ErrorCode.SoulBoneShopNeedRefresh;
                Write(msg);
                return;
            }

            if (GetCoins(CurrenciesType.secretAreaCoin) < SecretAreaLibrary.BonusPrice)
            {
                Log.Warn($"player {uid} ShopSoulBoneBonus error: coin not enough, curCoin {GetCoins(CurrenciesType.secretAreaCoin)} cost {SecretAreaLibrary.BonusPrice}");
                msg.Result = (int)ErrorCode.NoCoin;//货币不足
                Write(msg);
                return;
            }

            BonusGroup group = SecretAreaLibrary.GetBonusGroup(SecretAreaManager.GetTire());
            if (group == null)
            {
                Log.Warn($"player {uid} ShopSoulBoneBonus error, not find bonus group");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            BingoBonusModel model = group.RandomBonus();
            if (model == null)
            {
                Log.Warn($"player {uid} ShopSoulBoneBonus error, not find bonus model");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (bagManager.BagFull())
            {
                Log.Warn($"player {uid} ShopSoulBoneBonus error, bag full");
                msg.Result = (int)ErrorCode.MaxBagSpace;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            AddShopAllReward(manager, shop);
            manager.BreakupRewards();

            //增益奖励
            switch (model.BonusType)
            {
                case BonusType.Item:
                    manager.AddSimpleReward(model.Bonusfigure);
                    break;
                case BonusType.More://主属性最高的额翻Bonusfigure倍
                    int addNum = int.Parse(model.Bonusfigure);//倍数
                    ItemBasicInfo itemInfo = GetMaxMainAttrSoulBoneItem(manager);
                    if (itemInfo != null)
                    {
                        manager.AddReward(itemInfo, addNum);
                    }
                    break;
            }

            DelCoins(CurrenciesType.secretAreaCoin, SecretAreaLibrary.BonusPrice, ConsumeWay.Shop, shop.ShopType.ToString());

            manager.BreakupRewards(true);
            AddRewards(manager, ObtainWay.ShopSoulBone);

            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.BonusId = model.Id;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            //领取完奖励，清空魂骨商店
            ClearShopAndSyncDB(shop);

            //猎取魂骨
            AddTaskNumForType(TaskType.SoulBoneShopReward);
            AddPassCardTaskNum(TaskType.SoulBoneShopReward);
        }

        /// <summary>
        /// 魂骨商店领取奖励
        /// </summary>
        public void ShopSoulBoneReward()
        {
            Shop shop = ShopManager.GetShop(ShopType.SoulBone);
            if (shop == null)
            {
                Log.Warn($"player {uid} ShopSoulBoneReward error, have not this shop {ShopType.SoulBone}");
                return;
            }

            MSG_ZGC_SHOP_SOULBONE_REWARD msg = new MSG_ZGC_SHOP_SOULBONE_REWARD();

            //魂骨商店必须先有奖励才能领奖
            if (shop.ItemList.Count <= 0)
            {
                Log.Warn($"player {uid} ShopSoulBoneReward error, not have shop items");
                msg.Result = (int)ErrorCode.SoulBoneShopNeedRefresh;
                Write(msg);
                return;
            }

            if (bagManager.BagFull())
            {
                Log.Warn($"player {uid} ShopSoulBoneReward error, bag full");
                msg.Result = (int)ErrorCode.MaxBagSpace;
                Write(msg);
                return;
            }

            BonusGroup group = SecretAreaLibrary.GetBonusGroup(SecretAreaManager.GetTire());
            if (group == null)
            {
                Log.Warn($"player {uid} ShopSoulBoneReward error, not find bonus group");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            BingoBonusModel model = group.RandomBonus();
            if (model == null)
            {
                Log.Warn($"player {uid} ShopSoulBoneReward error, not find bonus model");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();

            //增益奖励
            switch (model.BonusType)
            {
                case BonusType.Item:
                    manager.AddSimpleReward(model.Bonusfigure);
                    break;
                case BonusType.More://主属性最高的额翻Bonusfigure倍
                    int addNum = int.Parse(model.Bonusfigure);//倍数
                    ItemBasicInfo itemInfo = GetMaxMainAttrSoulBoneItem(manager);
                    if (itemInfo != null)
                    {
                        manager.AddReward(itemInfo, addNum);
                    }
                    break;
            }

            AddShopAllReward(manager, shop);
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.ShopSoulBone);

            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.BonusId = model.Id;
            msg.Result = (int)ErrorCode.Success;
            Write(msg);

            ClearShopAndSyncDB(shop);

            //猎取魂骨
            AddTaskNumForType(TaskType.SoulBoneShopReward);
            AddPassCardTaskNum(TaskType.SoulBoneShopReward);
        }

        //此方法适用于魂骨商店
        private void AddShopAllReward(RewardManager manager, Shop shop)
        {
            foreach (var kv in shop.ItemList)
            {
                //      
                if (kv.Value is SoulBoneShopItem)
                {
                    SoulBoneShopItem it = kv.Value as SoulBoneShopItem;
                    manager.AddSimpleRewardWithSoulBoneCheck(it.ItemInfo);
                }
            }
        }

        /// <summary>
        /// 获取主属性最高的item
        /// </summary>
        private ItemBasicInfo GetMaxMainAttrSoulBoneItem(RewardManager manager)
        {
            var soulBoneList = manager.GetRewardItemList(RewardType.SoulBone);

            int maxMainValue = 0;
            ItemBasicInfo maxMainValuItem = null;
            foreach (var curr in soulBoneList)
            {
                int currMainValue = curr.GetSoulBoneMainValue();
                if (currMainValue > maxMainValue)
                {
                    maxMainValuItem = curr;
                    maxMainValue = currMainValue;
                }
            }

            return maxMainValuItem;
        }

        public void ClearShopAndSyncDB(Shop shop)
        {
            shop.Clear();
            shop.SyncDbUpdateShopItem();
        }

        public void RefreshShop(List<ShopType> shopList)
        {
        }

        public void SyncDbUpdateAllShopItem()
        {
            //server.GameDBPool.Call(new QueryUpdateAllShopList(Uid, shop));
        }

        public MSG_ZMZ_SHOP_INFO GetShopTransform()
        {
            MSG_ZMZ_SHOP_INFO info = new MSG_ZMZ_SHOP_INFO();

            Dictionary<ShopType, Shop> shopList = ShopManager.GetShopList();
            shopList.Values.ForEach(x => info.ShopList.Add(GenerateShopInfo(x)));
            return info;
        }

        private ZMZ_SHOP_INFO GenerateShopInfo(Shop shop)
        {
            ZMZ_SHOP_INFO info = new ZMZ_SHOP_INFO();
            info.ShopType = (int)shop.ShopType;
            info.RefreshCount = shop.RefreshCount;
            shop.ItemList.Values.ForEach(x => info.Items.Add(GenerateItemInfo(x)));
            return info;
        }

        private ZMZ_SHOP_ITEM GenerateItemInfo(ShopItem item)
        {
            ZMZ_SHOP_ITEM info = new ZMZ_SHOP_ITEM();
            info.Id = item.Id;
            info.BuyCount = item.BuyCount;
            info.ItemInfo = item.ItemInfo;
            return info;
        }

        public void LoadShopTransform(MSG_ZMZ_SHOP_INFO info)
        {
            foreach (var temp in info.ShopList)
            {
                Shop shop = null;
                ShopType shopType = (ShopType)temp.ShopType;
                if (shopType == ShopType.SoulBone)
                {
                    DBShopInfo shopInfo = new DBShopInfo(shopType) { RefreshCount = temp.RefreshCount};
                    foreach (var item in temp.Items)
                    {
                        shopInfo.AddItem(new DBShopItemInfo() { Id = item.Id, BuyCount = item.BuyCount, ItemInfo = item.ItemInfo});
                    }
                    shop = ShopManager.BindShop(shopInfo);
                }
                else 
                {
                    shop = ShopManager.LoadTransFormGetShop(shopType);
                    foreach (var item in temp.Items)
                    {
                        shop.AddShopItem(new ShopItem(item.Id, item.BuyCount, item.ItemInfo));
                    }
                }
                if (shop == null)
                {
                    continue;
                }
                shop.RefreshCount = temp.RefreshCount;
            }
        }

        //通用商城相关方法

        //获取商城信息
        public void GetCommonShopInfo(int shopId)
        {
            CommonShopModel model = CommonShopLibrary.GetShopModel(shopId);
            if (model == null)
            {
                Log.Warn($"player {uid} refresh shop failed, not have shop model {shopId}");
                return;
            }

            CommonShop shop = ShopManager.GetCommonShop(shopId, model);
          
            //商城开启检查
            ShopOpenCheck(model);

            MSG_ZGC_SHOP_INFO msg = shop.GenerateShopMsg(shopId);
            Write(msg);
        }

        private void ShopOpenCheck(CommonShopModel model)
        {
            MSG_ZGC_SHOP_INFO response = new MSG_ZGC_SHOP_INFO();
            switch (model.LimitType)
            {
                case LimitType.ArenaShopOpen:
                    if (!CheckLimitOpen(LimitType.ArenaShopOpen))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.NormalShopOpen:
                    if (!CheckLimitOpen(LimitType.NormalShopOpen))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.PromotionShopOpen:
                    if (!CheckLimitOpen(LimitType.PromotionShopOpen))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.CampNormal:               
                    if (!CheckLimitOpen(LimitType.CampNormal))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.CampShopOpen:
                    if (!CheckLimitOpen(LimitType.CampShopOpen))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.FreshShopOpen:
                    if (!CheckLimitOpen(LimitType.FreshShopOpen))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.NewShop1Open:
                    if (!CheckLimitOpen(LimitType.NewShop1Open))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.NewShop2Open:
                    if (!CheckLimitOpen(LimitType.NewShop2Open))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.NewShop3Open:
                    if (!CheckLimitOpen(LimitType.NewShop3Open))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.NewShop4Open:
                    if (!CheckLimitOpen(LimitType.NewShop4Open))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                case LimitType.NewShop5Open:
                    if (!CheckLimitOpen(LimitType.NewShop5Open))
                    {
                        Log.Warn("player {0} get shop info fail, player level is {1}, mainTaskId is {2}", Uid, Level, MainTaskId);
                        response.Result = (int)ErrorCode.NotOpen;
                        Write(response);
                        return;
                    }
                    break;
                default:
                    break;
            }
        }

        //商城刷新
        public void CommonShopRefresh(int shopId)
        {
            CommonShopModel model = CommonShopLibrary.GetShopModel(shopId);
            if (model == null)
            {
                Log.Warn($"player {uid} refresh shop failed, not have shop model {shopId}");
                return;
            }

            CommonShop shop = ShopManager.GetCommonShop(shopId, model);

            if (!model.CanFreshByUser())
            {
                MSG_ZGC_SHOP_REFRESH msg = new MSG_ZGC_SHOP_REFRESH();
                msg.ShopType = shopId;
                msg.Result = (int)ErrorCode.ShopCannotRefresh;
                Write(msg);
                Log.Warn($"player {uid} refresh shop failed, shop {shopId} can not refresh by user");
                return;
            }

            RefreshCommonShop(shopId, shop, model);
        }

        private void RefreshCommonShop(int shopId, CommonShop shop, CommonShopModel model)
        {
            MSG_ZGC_SHOP_REFRESH msg = new MSG_ZGC_SHOP_REFRESH();
            msg.ShopType = shopId;
            int refreshCount = shop.GetRefreshCount(shopId);
            int price = model.GetRefreshPrice(refreshCount + 1);
            if (GetCoins(model.RefreshCurrency) < price)
            {
                Log.Warn($"player {uid} refresh shop failed, coins not enough, curCoin {GetCoins(model.RefreshCurrency)} cost {price}");
                msg.Result = (int)ErrorCode.NoCoin;//货币不足
                Write(msg);
                return;
            }

            DelCoins(model.RefreshCurrency, price, ConsumeWay.Shop, shopId.ToString());

            //刷新次数加一
            shop.AddRefreshCount(shopId);
            shop.Refresh(shopId);
            //同步库
            ShopManager.SyncUpdateShopInfo(shopId, shop);

            msg.ShopInfo = shop.GenerateShopMsg(shopId);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        //每日刷新商城自动刷新
        public void DailyRefrshShopAutoRefresh()
        {
            List<int> shopIds = ShopManager.GetDailyRefreshShopIds(Uid);
            CommonShop shop = ShopManager.GetCommonShop();
            List<int> refreshShopIds = new List<int>();
            foreach (var shopId in shopIds)
            {    
                CommonShopModel model = CommonShopLibrary.GetShopModel(shopId);
                if (model == null)
                {
                    Log.Warn($"player {uid} refresh shop failed, not have shop model {shopId}");
                    return;
                }
                //检查商店是否符合开启条件
                if (!CheckShopIsOpen(model))
                {
                    continue;
                }
                //检查商店是否需要自动刷新
                if (!model.CanAutoFresh())
                {
                    continue;
                }
                shop.Refresh(shopId);
                //自动刷新重置刷新次数
                shop.ResetRefreshCount(shopId);
                //同步库
                ShopManager.SyncUpdateShopInfo(shopId, shop);

                refreshShopIds.Add(shopId);
            }

            //同步客户端
            SyncShopRefreshMsg(shop, refreshShopIds);
        }

        //每周刷新商城自动刷新
        public void WeeklyRefreshShopAutoRefresh()
        {
            List<int> shopIds = ShopManager.GetWeeklyRefreshShopIds(Uid);
            CommonShop shop = ShopManager.GetCommonShop();
            List<int> refreshShopIds = new List<int>();
            foreach (var shopId in shopIds)
            {
                CommonShopModel model = CommonShopLibrary.GetShopModel(shopId);
                if (model == null)
                {
                    Log.Warn($"player {uid} refresh shop failed, not have shop model {shopId}");
                    return;
                }
                //检查商店是否符合开启条件
                if (!CheckShopIsOpen(model))
                {
                    continue;
                }
                //检查商店是否需要自动刷新
                if (!model.CanAutoFresh())
                {
                    continue;
                }
                shop.Refresh(shopId);
                //自动刷新重置刷新次数
                shop.ResetRefreshCount(shopId);
                //同步库
                ShopManager.SyncUpdateShopInfo(shopId, shop);

                refreshShopIds.Add(shopId);
            }

            //同步客户端
            SyncShopRefreshMsg(shop, refreshShopIds);
        }

        //每月刷新商城自动刷新
        public void MonthlyRefreshShopAutoRefresh()
        {
            List<int> shopIds = ShopManager.GetMonthlyRefreshShopIds(Uid);
            CommonShop shop = ShopManager.GetCommonShop();
            List<int> refreshShopIds = new List<int>();
            foreach (var shopId in shopIds)
            {           
                CommonShopModel model = CommonShopLibrary.GetShopModel(shopId);
                if (model == null)
                {
                    Log.Warn($"player {uid} refresh shop failed, not have shop model {shopId}");
                    return;
                }
                //检查商店是否符合开启条件
                if (!CheckShopIsOpen(model))
                {
                    continue;
                }
                //检查商店是否需要自动刷新
                if (!model.CanAutoFresh())
                {
                    continue;
                }
                shop.Refresh(shopId);
                //自动刷新重置刷新次数
                shop.ResetRefreshCount(shopId);
                //同步库
                ShopManager.SyncUpdateShopInfo(shopId, shop);

                refreshShopIds.Add(shopId);
            }

            //同步客户端
            SyncShopRefreshMsg(shop, refreshShopIds);
        }

        public bool CheckShopIsOpen(CommonShopModel model)
        {
            switch (model.LimitType)
            {
                case LimitType.ArenaShopOpen:
                    if (!CheckLimitOpen(LimitType.ArenaShopOpen))
                    {
                        return false;
                    }
                    break;
                case LimitType.NormalShopOpen:
                    if (!CheckLimitOpen(LimitType.NormalShopOpen))
                    {
                        return false;
                    }
                    break;
                case LimitType.PromotionShopOpen:
                    if (!CheckLimitOpen(LimitType.PromotionShopOpen))
                    {
                        return false;
                    }
                    break;
                case LimitType.CampNormal:
                    if (!CheckLimitOpen(LimitType.CampNormal))
                    {
                        return false;
                    }
                    break;
                case LimitType.CampShopOpen:
                    if (!CheckLimitOpen(LimitType.CampShopOpen))
                    {
                        return false;
                    }
                    break;
                case LimitType.FreshShopOpen:
                    if (!CheckLimitOpen(LimitType.FreshShopOpen))
                    {
                        return false;
                    }
                    break;
                case LimitType.NewShop1Open:
                    if (!CheckLimitOpen(LimitType.NewShop1Open))
                    {
                        return false;
                    }
                    break;
                case LimitType.NewShop2Open:
                    if (!CheckLimitOpen(LimitType.NewShop2Open))
                    {
                        return false;
                    }
                    break;
                case LimitType.NewShop3Open:
                    if (!CheckLimitOpen(LimitType.NewShop3Open))
                    {
                        return false;
                    }
                    break;
                case LimitType.NewShop4Open:
                    if (!CheckLimitOpen(LimitType.NewShop4Open))
                    {
                        return false;
                    }
                    break;
                case LimitType.NewShop5Open:
                    if (!CheckLimitOpen(LimitType.NewShop5Open))
                    {
                        return false;
                    }
                    break;
                case LimitType.Tower:
                    if (!CheckLimitOpen(LimitType.Tower))
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public void SyncShopRefreshMsg(CommonShop shop, List<int> shopIds)
        {
            MSG_ZGC_SHOP_DAILY_REFRESH msg = new MSG_ZGC_SHOP_DAILY_REFRESH();
            foreach (var shopId in shopIds)
            {
                MSG_ZGC_SHOP_INFO info = ShopManager.GetShopInfo(shop, shopId);
                msg.ShopInfos.Add(info);
            }
            Write(msg);
        }

        //购买商城商品
        public void BuyShopItem(int shopItemId, int buyCount, int couponId)
        {
            if (buyCount <= 0)
            {
                Log.Warn($"player {uid} buy shop item {shopItemId} failed, buyCount param error");
                return;
            }

            MSG_ZGC_BUY_SHOP_ITEM response = new MSG_ZGC_BUY_SHOP_ITEM();

            CommonShopItemModel shopItem = ShopManager.GetShopItem(shopItemId);
            if (shopItem == null)
            {
                Log.Warn($"player {uid} buy shop item {shopItem.Id} failed, not find shopItem in xml");
                return;
            }

            CommonShopModel shopModel = CommonShopLibrary.GetShopModel(shopItem.ShopId);
            if (shopModel == null)
            {
                Log.Warn($"player {uid} buy shop item {shopItem.Id} failed, not find shop in xml");
                return;
            }

            CommonShop shop = ShopManager.GetCommonShop(shopItem.ShopId, shopModel);

            CommonShopItem item = shop.GetShopItem(shopItem.ShopId, shopItem.Id);
            if (item == null)
            {
                Log.Warn($"player {uid} buy shop item {shopItem.Id} failed, this shop not have this item yet");
                return;
            }
          
            if ((shopModel.ShowStart != DateTime.MinValue && ZoneServerApi.now < shopModel.ShowStart) || (shopModel.ShowEnd != DateTime.MinValue && ZoneServerApi.now > shopModel.ShowEnd))
            {
                Log.Warn($"player {Uid} buy shop item failed , shoStart {shopModel.ShowStart}, showEnd {shopModel.ShowEnd}, now {ZoneServerApi.now}");
                response.Result = (int)ErrorCode.NotOnSale;
                Write(response);
                return;
            }
            //已售空
            if (shopItem.BuyLimit != 0 && item.BuyCount + buyCount > shopItem.BuyLimit)
            {
                Log.Warn($"player {uid} buy shop item {shopItem.Id} failed, item is already sold");
                response.Result = (int)ErrorCode.NotOnSale;
                Write(response);
                return;
            }
            //检查是否在售卖期
            if (shopItem.StartDate != DateTime.MinValue && shopItem.StartDate > ZoneServerApi.now)
            {
                Log.Warn($"player {Uid} buy shop item failed , startDate is {shopItem.StartDate}, now is {ZoneServerApi.now}");
                response.Result = (int)ErrorCode.NotOnSale;
                Write(response);
                return;
            }
            if (shopItem.EndDate != DateTime.MaxValue && shopItem.EndDate < ZoneServerApi.now)
            {
                Log.Warn($"player {Uid} buy shop item failed , endDate is {shopItem.EndDate}, now is {ZoneServerApi.now}");
                response.Result = (int)ErrorCode.NotOnSale;
                Write(response);
                return;
            }
            //检查是否超限购数
            if (shopItem.BuyLimit != 0 && buyCount > shopItem.BuyLimit)
            {
                Log.Warn($"player {uid} buy shop item {shopItem.Id} failed, buyCount {buyCount} exceed limit {shopItem.BuyLimit}");
                response.Result = (int)ErrorCode.ShopBuyItemLimit;
                Write(response);
                return;
            }
            //检查是否超最大购买数      
            if (shopItem.MaxCount < buyCount)
            {
                Log.Warn($"player {uid} buy shop item {shopItem.Id} failed, buyCount {buyCount} exceed maxCount {shopItem.MaxCount}");
                response.Result = (int)ErrorCode.ShopBuyItemLimit;
                Write(response);
                return;
            }
            int itemId = shopItem.CurrentPrice[0].ToInt();
            int rewardType = shopItem.CurrentPrice[1].ToInt();
            int num = shopItem.CurrentPrice[2].ToInt();

            NormalItem couponItem;
            if (CheckCanUseCoupon(couponId, shopItem.ShopId, out couponItem))
            {
                float discount = CommonShopLibrary.GetCouponDiscount(couponId);
                num = (int)Math.Ceiling(num * 0.1 * discount);
            }

            int totalNum = 0;
            if (!CheckPriceIsOutOfRange(num, buyCount))
            {
                totalNum = num * buyCount;
            }
            else
            {
                totalNum = int.MaxValue;
                buyCount = totalNum / num;
            }

            string itemType = string.Empty;
            if (Enum.IsDefined(typeof(CurrenciesType), itemId))
            {
                //货币购买
                CurrenciesBuyItem(shopItem, response, itemId, totalNum);
                itemType = ((CurrenciesType)itemId).ToString();
            }
            else
            {
                //物品兑换
                ItemExchangeItem(shopItem, response, itemId, rewardType, totalNum);
                itemType = "item";
            }
            //发放奖励
            if (response.Result == (int)ErrorCode.Success)
            {
                if (couponItem != null)
                {
                    BaseItem baseItem = DelItem2Bag(couponItem, RewardType.NormalItem, 1, ConsumeWay.ItemUse);

                    if (baseItem != null)
                    {
                        SyncClientItemInfo(couponItem);
                        //使用消耗品
                        AddTaskNumForType(TaskType.UseConsumable, 1, true, couponItem.SubType);
                    }
                }

                RewardManager manager = new RewardManager();
                SoulBoneShopItem soulBoneItem = item as SoulBoneShopItem;
                if (soulBoneItem != null)
                {
                    manager = GetSoulBoneShopItemReward(shopItem.Reward, soulBoneItem.SoulBone, ObtainWay.ShopBuy, buyCount);
                }
                else
                {
                    manager = GetSimpleReward(shopItem.Reward, ObtainWay.ShopBuy, buyCount);
                }
                manager.GenerateRewardItemInfo(response.Rewards);
                //商品信息更新
                ShopManager.UpdateShopItemBuyCount(shop, shopItem.ShopId, item, buyCount);

                //埋点BI
                foreach (var reward in manager.AllRewards)
                {
                    BIRecordShopByItemLog((ShopType)shopItem.ShopId, itemId.ToString(), totalNum, ObtainWay.ShopBuy, (RewardType)reward.RewardType, reward.Id, reward.Num, shopItem.TimingGiftType, buyCount);
                    //RecordShopByItemLog((ShopType)shopItem.ShopId, itemId.ToString(), totalNum, ObtainWay.ShopBuy, (RewardType)reward.RewardType, reward.Id, reward.Num);
                }
                KomoeEventLogShopPurchase(shopItemId, buyCount, itemId, itemType, totalNum, shopItem.ShopId, ((ShopType)shopItem.ShopId).ToString());

            }

            response.Item = ShopManager.GenerateShopItemInfo(item);
            Write(response);

            //商店购买
            AddTaskNumForType(TaskType.ShopBuyItem, 1, true, shopItem.ShopId);
            AddPassCardTaskNum(TaskType.ShopBuyItem);
            AddSchoolTaskNum(TaskType.ShopBuyItem);
        }

        private void CurrenciesBuyItem(CommonShopItemModel shopItem, MSG_ZGC_BUY_SHOP_ITEM response, int consumeType, int currentPrice)
        {         
            //检查货币是否足够
            CurrenciesType currencyType = (CurrenciesType)consumeType;
            int coins = GetCoins(currencyType);
            if (coins < currentPrice)
            {
                Log.Warn($"player {uid} buy shop item {shopItem.Id} failed: coins not enough, curCoin {coins} cost {currentPrice}");
                response.Result = (int)ErrorCode.NoCoin;
                return;
            }
            //扣货币
            DelCoins(currencyType, currentPrice, ConsumeWay.Shop, shopItem.Id.ToString());
            response.Result = (int)ErrorCode.Success;
        }

        private void ItemExchangeItem(CommonShopItemModel shopItem, MSG_ZGC_BUY_SHOP_ITEM response, int itemId, int consumeType, int needCount)
        {
            BaseItem item = BagManager.GetItem((MainType)consumeType, itemId);
            if (item != null)
            {
                //碎片/节日货币个数
                if (item.PileNum < needCount)
                {
                    Log.Warn("player {0} buy shop item failed: no item {1} num {2}", uid, itemId, item.PileNum);               
                    response.Result = (int)ErrorCode.ItemNotEnough;
                    return;
                }
                else
                {
                    BaseItem it = DelItem2Bag(item, (RewardType)consumeType, needCount, ConsumeWay.Shop);
                    if (it != null)
                    {
                        SyncClientItemInfo(it);
                        response.Result = (int)ErrorCode.Success;
                    }
                }
            }
        }

        private bool CheckPriceIsOutOfRange(int num, int count)
        {
            if (num > int.MaxValue / count)
            {
                return true;
            }
            return false;
        }

        private bool CheckCanUseCoupon(int couponId, int shopId, out NormalItem item)
        {
            item = null;
            if (couponId == 0)
            {
                return false;
            }
            CommonShopModel shopModel = CommonShopLibrary.GetShopModel(shopId);
            if (shopModel == null || !shopModel.CouponUse)
            {
                return false;
            }
            item = BagManager.GetItem(MainType.Consumable, couponId) as NormalItem;
            if (item == null)
            {
                return false;
            }
            return true;
        }
      
        //活动商店刷新
        public void ActivityShopRefresh(bool isLogin, List<int> shopList)
        {
            CommonShop shop = ShopManager.GetCommonShop();
            List<int> refreshShopIds = new List<int>();
            foreach (var shopId in shopList)
            {
                //未开启的商店不刷新
                if (!shop.ItemDic.ContainsKey(shopId))
                {
                    continue;
                }
                shop.Refresh(shopId);
                //自动刷新重置刷新次数
                shop.ResetRefreshCount(shopId);
                //重置刷新标记
                shop.UpdateActivityRefreshed(shopId, true);
                //同步库
                ShopManager.SyncUpdateShopInfo(shopId, shop);

                refreshShopIds.Add(shopId);

                if (isLogin)
                {
                    BIRecordRefreshLog(LastRefreshTime.ToString(), "activityShop", shopId, "login");
                }
            }
            //同步客户端
            SyncShopRefreshMsg(shop, refreshShopIds);
        }

        public void ActivityShopResetRefreshFlag(int shopId)
        {
            CommonShop shop = ShopManager.GetCommonShop();
            //未开启的商店不刷新
            if (!shop.ItemDic.ContainsKey(shopId))
            {
                return;
            }
            //重置刷新标记
            shop.UpdateActivityRefreshed(shopId, false);
        }

        public MSG_ZMZ_COMMON_SHOP_INFO GetCommonShopTransform()
        {
            MSG_ZMZ_COMMON_SHOP_INFO info = new MSG_ZMZ_COMMON_SHOP_INFO();
            CommonShop shop = ShopManager.GetCommonShop();
            foreach (var kv in shop.ItemDic)
            {              
                info.ShopList.Add(GenerateCommonShopInfo(kv.Key, kv.Value, shop));
            }
            return info;
        }

        private ZMZ_COMMON_SHOP_INFO GenerateCommonShopInfo(int shopId, Dictionary<int, CommonShopItem> shopItemDic, CommonShop shop)
        {
            ZMZ_COMMON_SHOP_INFO info = new ZMZ_COMMON_SHOP_INFO();
            info.ShopType = shopId;
            info.RefreshCount = shop.RefrehCounts[shopId];
            info.ActivityRefreshed = shop.ActivityRefreshDic[shopId];
            foreach (var item in shopItemDic)
            {         
                info.Items.Add(GenerateCommonShopItem(item));
            }
            return info;
        }

        private ZMZ_COMMON_SHOP_ITEM GenerateCommonShopItem(KeyValuePair<int, CommonShopItem> item)
        {
            ZMZ_COMMON_SHOP_ITEM shopItem = new ZMZ_COMMON_SHOP_ITEM();
            shopItem.Id = item.Key;
            shopItem.BuyCount = item.Value.BuyCount;
            shopItem.ItemInfo = item.Value.ItemInfo;
            return shopItem;
        }

        public void LoadCommonShopTransform(MSG_ZMZ_COMMON_SHOP_INFO infos)
        {
            ShopManager.Cache5MaxBattlePowerHeroInfo();
            ShopManager.LoadBattlePower();
            foreach (var info in infos.ShopList)
            {
                CommonShop shop = ShopManager.GetCommonShop();
                foreach (var item in info.Items)
                {
                    shop.LoadCommonShopItem(info.ShopType, item.Id, item.BuyCount, item.ItemInfo);
                }
                shop.AddRefreshCount(info.ShopType, info.RefreshCount);
                shop.UpdateActivityRefreshed(info.ShopType, info.ActivityRefreshed);
            }
        }
    }
}
