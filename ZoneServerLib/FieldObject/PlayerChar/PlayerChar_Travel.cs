using CommonUtility;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.Tarvel;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerModels.Travel;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using ZoneServerLib.Travel;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public TravelManager TravelMng { get; private set; }

        public void InitTravelManager()
        {
            TravelMng = new TravelManager(this);
        }

        public void InitTravelManager(Dictionary<int, TravelHeroItem> infoList)
        {
            TravelMng.Init(infoList);
        }

        public void SendTravelManager()
        {
            MSG_ZGC_TRAVEL_MANAGER msg = new MSG_ZGC_TRAVEL_MANAGER();
            Dictionary<int, TravelHeroItem> heroList = TravelMng.GetHeroList();
            foreach (var heroItem in heroList)
            {
                MSG_ZGC_HERO_TRAVEL_ITEM msgItem = GetHeroTravelItemMsg(heroItem.Value);
                msg.HeroLsit.Add(msgItem);
            }
            Write(msg);
        }

        private MSG_ZGC_HERO_TRAVEL_ITEM GetHeroTravelItemMsg(TravelHeroItem heroItem)
        {
            MSG_ZGC_HERO_TRAVEL_ITEM msgItem = new MSG_ZGC_HERO_TRAVEL_ITEM();
            msgItem.HeroId = heroItem.Id;
            msgItem.Level = heroItem.Level;
            msgItem.Affinity = heroItem.Affinity;
            msgItem.StartTime = heroItem.StartTime;
            //msgItem.Slot = heroItem.Slot;

            foreach (var travelEvent in heroItem.TravelEvents)
            {
                MSG_ZGC_TRAVEL_EVENT_ITEM eventItem = new MSG_ZGC_TRAVEL_EVENT_ITEM();
                eventItem.Type = (int)travelEvent.Type;
                eventItem.EndTime = (int)travelEvent.EndTime;
                msgItem.Travel.Add(eventItem);
            }
            msgItem.BuyList.AddRange(heroItem.BuyList);

            foreach (var item in heroItem.CardList)
            {
                MSG_ZGC_TRAVEL_CARD_ITEM cardItem = new MSG_ZGC_TRAVEL_CARD_ITEM();
                cardItem.Id = item.Value.Id;
                cardItem.Level = item.Value.Level;
                cardItem.Exp = item.Value.Exp;
                msgItem.CardList.Add(cardItem);
            }
            return msgItem;
        }


        //激活
        public void ActivateHeroTravel(int heroId)
        {
            MSG_ZGC_ACTIVATE_HERO_TRAVEL msg = new MSG_ZGC_ACTIVATE_HERO_TRAVEL();
            msg.HeroId = heroId;
            //检查是否已经激活
            TravelHeroItem heroTravelItem = TravelMng.GetHeroTravelInfo(heroId);
            if (heroTravelItem != null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} ActivateHeroTravel has got hero item in {heroId}");
                msg.Result = (int)ErrorCode.HeroHasActivate;
                Write(msg);
                return;
            }

            //判断激活物品
            TravelHeroInfo heroTravelInfo = TravelLibrary.GetHeroInfo(heroId);
            if (heroTravelInfo == null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} ActivateHeroTravel not got hero info in {heroId}");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }

            if (heroTravelInfo.ActivateItem > 0)
            {
                BaseItem item = BagManager.GetItem(MainType.Consumable, heroTravelInfo.ActivateItem);
                if (item != null)
                {
                    if (item.PileNum < heroTravelInfo.ActivateNum)
                    {
                        Log.Warn($"player {Uid} ActivateHeroTravel failed: item {heroTravelInfo.ActivateItem} count {item.PileNum} not enough");
                        msg.Result = (int)ErrorCode.ItemNotEnough;
                        Write(msg);
                        return;
                    }
                }
                else
                {
                    Log.Warn($"player {Uid} ActivateHeroTravel failed: no item {heroTravelInfo.ActivateItem}");
                    msg.Result = (int)ErrorCode.ItemNotEnough;
                    Write(msg);
                    return;
                }

                BaseItem it = DelItem2Bag(item, RewardType.NormalItem, heroTravelInfo.ActivateNum, ConsumeWay.TravelHeroActivate);
                if (it != null)
                {
                    SyncClientItemInfo(it);
                }
            }


            //激活
            TravelHeroItem newItem = CreateNewTravelHeroItem(heroId);
            TravelMng.AddHeroTravelInfo(newItem);

            //DB同步
            server.GameDBPool.Call(new QueryInsertTravelHero(uid, newItem));

            msg.Result = (int)ErrorCode.Success;
            msg.HeroTravel = GetHeroTravelItemMsg(newItem);
            Write(msg);
        }

        private static TravelHeroItem CreateNewTravelHeroItem(int heroId)
        {
            TravelHeroItem newItem = new TravelHeroItem();
            newItem.Id = heroId;
            newItem.Level = 1;
            //newItem.Affinity = 0;
            //newItem.Travel = string.Empty;
            //newItem.CardList = new List<int>();
            //newItem.Slot = 0;
            //newItem.StartTime = 0;
            return newItem;
        }

        //提升亲和度
        public void AddHeroTravelAffinity(int heroId, int itemId, int itemNum)
        {

            MSG_ZGC_ADD_HERO_TRAVEL_AFFINITY msg = new MSG_ZGC_ADD_HERO_TRAVEL_AFFINITY();
            msg.HeroId = heroId;
            msg.ItemId = itemId;
            msg.ItemNum = itemNum;
            //检查是否已经激活
            TravelHeroItem heroTravelItem = TravelMng.GetHeroTravelInfo(heroId);
            if (heroTravelItem == null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} AddHeroTravelAffinity has not got hero info in {heroId}");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }

            //判断激活物品
            TravelHeroInfo heroTravelInfo = TravelLibrary.GetHeroInfo(heroId);
            if (heroTravelInfo == null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} ActivateHeroTravel not got hero info in {heroId}");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }

            int itemAffinity = TravelLibrary.GetItemAffinity(itemId);
            if (itemAffinity == 0)
            {
                Log.ErrorLine($"player {Uid} AddHeroTravelAffinity error item is {itemId}");
                msg.Result = (int)ErrorCode.NoItemInfo;
                Write(msg);
                return;
            }
            if (itemNum <= 0)
            {
                Log.ErrorLine($"player {Uid} AddHeroTravelAffinity error num is {itemNum}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }


            //判断激活物品
            BaseItem item = BagManager.GetItem(MainType.Consumable, itemId);
            if (item != null)
            {
                if (item.PileNum < itemNum)
                {
                    Log.Warn($"player {Uid} AddHeroTravelAffinity failed: item {itemId} count {item.PileNum} not enough {itemNum}");
                    msg.Result = (int)ErrorCode.ItemNotEnough;
                    Write(msg);
                    return;
                }
            }
            else
            {
                Log.Warn($"player {Uid} AddHeroTravelAffinity failed: no item {itemId}");
                msg.Result = (int)ErrorCode.ItemNotEnough;
                Write(msg);
                return;
            }
            int addAffinity = TravelHeroItemAddAffinity(itemAffinity * itemNum, heroTravelInfo.MaxLevel, heroTravelItem);
            if (addAffinity == 0)
            {
                Log.Warn($"player {Uid} AddHeroTravelAffinity failed: currentLevel {heroTravelItem.Level} currentAffinity {heroTravelItem.Affinity} is max");
                msg.Result = (int)ErrorCode.ItemNotEnough;
                Write(msg);
                return;
            }


            BaseItem it = DelItem2Bag(item, RewardType.NormalItem, itemNum, ConsumeWay.TravelHeroActivate);
            if (it != null)
            {
                SyncClientItemInfo(it);
            }

            AddCoins(CurrenciesType.travelAffinity, addAffinity, ObtainWay.TravelItemAdd, itemId.ToString());

            //DB同步
            server.GameDBPool.Call(new QueryUpdateTravelHero(uid, heroTravelItem));


            msg.Result = (int)ErrorCode.Success;
            msg.HeroTravel = GetHeroTravelItemMsg(heroTravelItem);
            Write(msg);
        }

        private int TravelHeroItemAddAffinity(int addAffinity, int maxLevel, TravelHeroItem heroTravelItem)
        {

            int currentLevel = heroTravelItem.Level;
            int currentAffinity = heroTravelItem.Affinity;

            int currentMaxValue = TravelLibrary.GetLevelAffinity(currentLevel);
            if (currentLevel == maxLevel && currentAffinity == currentMaxValue)
            {
                return 0;
            }

            if (currentAffinity + addAffinity > currentMaxValue)
            {
                int level = currentLevel;
                int current = currentAffinity;
                int add = addAffinity;
                int max = TravelLibrary.GetLevelAffinity(level);
                //进阶
                while (current + add > max)
                {
                    int next = TravelLibrary.GetLevelAffinity(level + 1);
                    if (next == 0)
                    {
                        //说明到最高级了
                        addAffinity = addAffinity - (add + current - max);
                        current = max;
                        add = 0;
                        break;
                    }
                    else
                    {
                        add = current + add - max;
                        current = 0;
                        max = next;
                        level++;
                    }
                }
                current += add;
                heroTravelItem.Affinity = current;
                heroTravelItem.Level = level;
            }
            else
            {
                //没进阶
                heroTravelItem.Affinity += addAffinity;
            }

            return addAffinity;
        }

        //出巡
        public void StartHeroTravelEvevt(int heroId)
        {
            MSG_ZGC_START_HERO_TRAVEL_EVENT msg = new MSG_ZGC_START_HERO_TRAVEL_EVENT();
            msg.HeroId = heroId;
            //检查是否已经激活
            TravelHeroItem heroTravelItem = TravelMng.GetHeroTravelInfo(heroId);
            if (heroTravelItem == null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} StartHeroTravelEvevt has not got hero info in {heroId}");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }
            if (heroTravelItem.StartTime > 0)
            {
                //正在游历
                Log.ErrorLine($"player {Uid} StartHeroTravelEvevt hero {heroId} time is {heroTravelItem.StartTime}");
                msg.Result = (int)ErrorCode.HeroIsTravel;
                Write(msg);
                return;
            }
            //检查是否有空位
            int maxSlotCount = TravelLibrary.GetSlotCount(GetCoins(CurrenciesType.travelAffinity));
            int currentCount = TravelMng.GetCurrentSlotCount();
            if (currentCount >= maxSlotCount)
            {
                Log.ErrorLine($"player {Uid} StartHeroTravelEvevt hero {heroId} currentCount is {currentCount} not maxSlotCount{maxSlotCount}");
                msg.Result = (int)ErrorCode.MaxCount;
                Write(msg);
                return;
            }

            heroTravelItem.TravelEvents.Clear();
            DateTime time = ZoneServerApi.now;
            heroTravelItem.StartTime = Timestamp.GetUnixTimeStampSeconds(time);
            //计算事件
            Dictionary<int, TravelEventModel> eventList = TravelLibrary.GetTravelEventLsit();
            foreach (var eventInfo in eventList)
            {
                time = time.AddMinutes(eventInfo.Value.EventTime);
                TravelEventType travelEventType = (TravelEventType)eventInfo.Value.GetEventId();

                if (travelEventType != TravelEventType.None)
                {
                    TravelEventItem eventItem = new TravelEventItem();
                    eventItem.Type = travelEventType;
                    eventItem.EndTime = Timestamp.GetUnixTimeStampSeconds(time);
                    heroTravelItem.TravelEvents.Add(eventItem);

                    if (travelEventType == TravelEventType.Home)
                    {
                        break;
                    }
                }
            }

            //DB同步
            server.GameDBPool.Call(new QueryUpdateTravelHero(uid, heroTravelItem));

            msg.Result = (int)ErrorCode.Success;
            msg.HeroTravel = GetHeroTravelItemMsg(heroTravelItem);
            Write(msg);
        }

        //领取事件
        public void GetHeroTravelEvevt(int heroId)
        {
            MSG_ZGC_GET_HERO_TRAVEL_EVENT msg = new MSG_ZGC_GET_HERO_TRAVEL_EVENT();
            msg.HeroId = heroId;
            //检查是否已经激活
            TravelHeroItem heroTravelItem = TravelMng.GetHeroTravelInfo(heroId);
            if (heroTravelItem == null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} GetHeroTravelEvevt has not got hero info in {heroId}");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }

            //判断激活物品
            TravelHeroInfo heroTravelInfo = TravelLibrary.GetHeroInfo(heroId);
            if (heroTravelInfo == null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} GetHeroTravelEvevt not got hero info in {heroId}");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }
            TravelHeroEvent travelHeroEvent = heroTravelInfo.GetLevelEvent(heroTravelItem.Level);
            if (travelHeroEvent == null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} GetHeroTravelEvevt not got hero level {heroTravelItem.Level} event");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }

            if (heroTravelItem.StartTime == 0)
            {
                //正在游历
                Log.ErrorLine($"player {Uid} GetHeroTravelEvevt hero {heroId} time is {heroTravelItem.StartTime}");
                msg.Result = (int)ErrorCode.HeroNotTravel;
                Write(msg);
                return;
            }

            if (heroTravelItem.TravelEvents.Count == 0)
            {
                //正在游历
                Log.ErrorLine($"player {Uid} GetHeroTravelEvevt hero {heroId} envent count is {heroTravelItem.TravelEvents.Count}");
                msg.Result = (int)ErrorCode.HeroNotTravel;
                Write(msg);
                return;
            }

            //执行计算
            TravelEventItem eventItem = heroTravelItem.TravelEvents.First();

            DateTime endTime = Timestamp.TimeStampToDateTime(eventItem.EndTime);
            if (endTime > ZoneServerApi.now)
            {
                Log.ErrorLine($"player {Uid} GetHeroTravelEvevt hero {heroId} envent end time is {endTime}");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            int eventId = 0;
            switch (eventItem.Type)
            {
                case TravelEventType.Email:
                    {
                        //随机故事
                        int cardId = travelHeroEvent.GetCardId();
                        TravelCardInfo info = TravelLibrary.GetCardInfo(cardId);
                        if (info != null)
                        {
                            msg.EventParam = cardId.ToString();

                            bool levelUp = TravelMng.AddCardId(heroTravelItem, cardId);

                            TravelCardItem card = heroTravelItem.GetCardItem(cardId);
                            if (card != null)
                            {
                                MSG_ZGC_UPDATE_HERO_TRAVEL_CARD_INFO cardMsg = new MSG_ZGC_UPDATE_HERO_TRAVEL_CARD_INFO();
                                cardMsg.HeroId = heroTravelItem.Id;

                                MSG_ZGC_TRAVEL_CARD_ITEM cardItem = new MSG_ZGC_TRAVEL_CARD_ITEM();
                                cardItem.Id = card.Id;
                                cardItem.Level = card.Level;
                                cardItem.Exp = card.Exp;
                                cardMsg.CardInfo = cardItem;
                                cardMsg.LevelUp = levelUp;
                                Write(cardMsg);
                            }
                        }
                        heroTravelItem.TravelEvents.RemoveAt(0);
                        eventId = cardId;
                    }
                    break;
                case TravelEventType.Friend:
                    {
                        //随机朋友
                        int friend = travelHeroEvent.GetFriendId();
                        msg.EventParam = friend.ToString();
                        //检查是否已经激活
                        TravelHeroItem friendItem = TravelMng.GetHeroTravelInfo(friend);
                        if (friendItem != null)
                        {
                            TravelHeroInfo friendTravelInfo = TravelLibrary.GetHeroInfo(heroId);
                            //说明已经激活,增加亲密度
                            int addAffinity = TravelHeroItemAddAffinity(TravelLibrary.FriendAddAffinity, friendTravelInfo.MaxLevel, friendItem);
                            if (addAffinity > 0)
                            {
                                AddCoins(CurrenciesType.travelAffinity, addAffinity, ObtainWay.TravelFriendAdd, friend.ToString());
                            }
                            msg.EventHero = GetHeroTravelItemMsg(friendItem);
                            server.GameDBPool.Call(new QueryUpdateTravelHero(uid, friendItem));
                        }
                        else
                        {
                            TravelHeroInfo friendInfo = TravelLibrary.GetHeroInfo(friend);
                            if (friendInfo != null)
                            {
                                //激活
                                TravelHeroItem newItem = CreateNewTravelHeroItem(friend);
                                TravelMng.AddHeroTravelInfo(newItem);

                                //DB同步
                                server.GameDBPool.Call(new QueryInsertTravelHero(uid, newItem));
                                msg.EventHero = GetHeroTravelItemMsg(newItem);
                            }
                            else
                            {
                                //说明不能激活
                                Log.Warn($"player {Uid} GetHeroTravelEvevt hero {heroId} envent type is {eventItem.Type} not find {friend} info");
                            }
                        }
                        heroTravelItem.TravelEvents.RemoveAt(0);
                        eventId = friend;
                    }
                    break;
                case TravelEventType.Box:
                    {
                        //随机奖励
                        RewardManager manager = new RewardManager();
                        RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.EntiretyNew, travelHeroEvent.BoxReward);
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        manager.AddReward(items);
                        manager.BreakupRewards(true);
                        // 发放奖励
                        AddRewards(manager, ObtainWay.TravelBoxReward, heroId.ToString());
                        //通知前端奖励
                        manager.GenerateRewardMsg(msg.Rewards);

                        heroTravelItem.TravelEvents.RemoveAt(0);

                        if (items.Count > 0)
                        {
                            eventId = items[0].Id;
                        }
                    }
                    break;
                case TravelEventType.Shop:
                    {
                        if (heroTravelItem.ShopItemList.Count == 0)
                        {
                            heroTravelItem.ShopItemList = RandomShopItem();
                        }

                        foreach (var itemId in heroTravelItem.ShopItemList)
                        {
                            MSG_TOWER_TASK_ITEM_INFO shopItem = GetShopItemMsg(itemId, heroTravelItem);
                            msg.ShopItems.Add(shopItem);
                        }
                    }
                    break;
                case TravelEventType.Home:
                    {
                        //回家
                        heroTravelItem.StartTime = 0;
                        heroTravelItem.TravelEvents.Clear();
                    }
                    break;
                case TravelEventType.Help:
                    {
                        heroTravelItem.TravelEvents.RemoveAt(0);
                    }
                    break;
                case TravelEventType.None:
                default:
                    Log.Warn($"player {Uid} GetHeroTravelEvevt hero {heroId} envent type is {eventItem.Type}");
                    heroTravelItem.TravelEvents.RemoveAt(0);
                    break;
            }

            //DB同步
            server.GameDBPool.Call(new QueryUpdateTravelHero(uid, heroTravelItem));

            msg.EventType = (int)eventItem.Type;
            msg.Result = (int)ErrorCode.Success;
            msg.HeroTravel = GetHeroTravelItemMsg(heroTravelItem);
            Write(msg);


            BIRecordTravelLog(heroTravelItem.Slot, heroId, eventItem.Type.ToString(), eventId);
        }


        private List<int> RandomShopItem()
        {
            Cache5MaxBattlePowerHeroInfo();

            List<int> itemIds = new List<int>();

            for (int i = 0; i < TravelLibrary.ShopItemCount; i++)
            {
                TowerShopItemType itemType = TravelLibrary.RandomShopItemType(MainTaskId);
                int quality = GetShopItemQuality(itemType);

                TowerShopItemModel itemModel = TravelLibrary.RandomShopItem(itemType, quality);

                if (itemModel == null)
                {
                    Log.Warn($"随机商品出错 itemtype {itemType} quality {quality}");
                }
                itemIds.Add(itemModel?.Id ?? 1);
            }
            return itemIds;
        }

        private MSG_TOWER_TASK_ITEM_INFO GetShopItemMsg(int itemId, TravelHeroItem travelItem)
        {
            MSG_TOWER_TASK_ITEM_INFO itemInfo = new MSG_TOWER_TASK_ITEM_INFO() { ItemId = itemId };
            CommonShopItemModel shopItem = CommonShopLibrary.GetShopItemModel(itemId);
            if (shopItem == null)
            {
                Log.Warn($"配置表信息有误 not find 商品id {itemId}");
                return null;
            }

            ItemBasicInfo basicItem = ItemBasicInfo.Parse(shopItem.Reward);
            switch ((RewardType)basicItem.RewardType)
            {
                case RewardType.SoulBone:
                    ItemBasicInfo cacheBasicInfo;
                    if (travelItem.SoulBoneList.TryGetValue(basicItem.Id, out cacheBasicInfo))
                    {
                        basicItem = cacheBasicInfo;
                    }
                    else
                    {
                        //随机的魂骨需要保存
                        travelItem.SoulBoneList[basicItem.Id] = basicItem;
                    }
                    SoulBone soulBone = SoulBoneManager.GenerateSoulBoneInfo(basicItem, true);
                    if (soulBone != null)
                    {
                        itemInfo.SoulBone = SoulBoneManager.GenerateSoulBoneMsg(soulBone);
                    }
                    break;
                case RewardType.Equip:
                    MSG_ZGC_ITEM_EQUIPMENT euqipmentMsg = CommonShopItem.GenerateEquipmentMsg(basicItem);
                    itemInfo.Equip = CommonShopItem.EquipmentAndScoreMsg(euqipmentMsg);
                    break;
                default:
                    break;
            }
            return itemInfo;
        }

        public void ButTravelShopItem(int heroId, int itemIndex)
        {
            MSG_ZGC_BUY_HERO_TRAVEL_SHOP_ITEM msg = new MSG_ZGC_BUY_HERO_TRAVEL_SHOP_ITEM();
            msg.HeroId = heroId;
            //检查是否已经激活
            TravelHeroItem heroTravelItem = TravelMng.GetHeroTravelInfo(heroId);
            if (heroTravelItem == null)
            {
                //说明已经激活
                Log.ErrorLine($"player {Uid} ButTravelShopItem has not got hero info in {heroId}");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }

            TravelEventItem eventItem = heroTravelItem.TravelEvents.First();
            if (eventItem.Type != TravelEventType.Shop)
            {
                Log.ErrorLine($"player {Uid} ButTravelShopItem error type {eventItem.Type}");
                msg.Result = (int)ErrorCode.NoHeroInfo;
                Write(msg);
                return;
            }
            if (itemIndex < 0)
            {
                //结束商店
                ClearHeroTravelShop(heroTravelItem);
            }
            else
            {
                if (heroTravelItem.BuyList.Contains(itemIndex))
                {
                    Log.ErrorLine($"player {Uid} ButTravelShopItem has buy itemIndex {itemIndex}");
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
                if (heroTravelItem.ShopItemList.Count <= itemIndex)
                {
                    Log.ErrorLine($"player {Uid} ButTravelShopItem can not buy itemIndex {itemIndex} list cout is {heroTravelItem.ShopItemList.Count}");
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
                int shopItemId = heroTravelItem.ShopItemList[itemIndex];

                TowerShopItemModel model = TravelLibrary.GetIslandChallengeShopItemModel(shopItemId);
                if (model == null)
                {
                    Log.ErrorLine($"player {Uid} ButTravelShopItem not find shop item {shopItemId}");
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }
                CommonShopItemModel itemModel = CommonShopLibrary.GetShopItemModel(model.Id);
                if (itemModel == null)
                {
                    Log.ErrorLine($"player {Uid} ButTravelShopItem can not buy itemId {model.Id}  not find");
                    msg.Result = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
                }

                int currenciesTypeId = int.Parse(itemModel.CurrentPrice[0]);
                CurrenciesType costType = (CurrenciesType)currenciesTypeId;
                int rewardType = itemModel.CurrentPrice[1].ToInt();
                int num = itemModel.CurrentPrice[2].ToInt();

                if (!CheckCoins(costType, num))
                {
                    Log.ErrorLine($"player {Uid} ButTravelShopItem can not buy itemId {model.Id}  not find");
                    msg.Result = (int)ErrorCode.NoCoin;
                    Write(msg);
                    return;
                }
                //扣除花费
                DelCoins(costType, num, ConsumeWay.ButTravelShopItem, model.Id.ToString());

                ItemBasicInfo info = ItemBasicInfo.Parse(itemModel.Reward);
                RewardManager rewardManager = new RewardManager();
                if (info != null && heroTravelItem.SoulBoneList.ContainsKey(info.Id))
                {
                    rewardManager.AddReward(heroTravelItem.SoulBoneList[info.Id]);
                }
                else
                {
                    rewardManager.AddSimpleRewardWithSoulBoneCheck(itemModel.Reward);
                }
                rewardManager.BreakupRewards();
                rewardManager.GenerateRewardItemInfo(msg.Rewards);
                AddRewards(rewardManager, ObtainWay.TravelShopBuy);

                heroTravelItem.AddBuyedShop(itemIndex);

                //埋点BI
                foreach (var item in rewardManager.AllRewards)
                {
                    BIRecordShopByItemLog(ShopType.TravelNodeShop, currenciesTypeId.ToString(), num, ObtainWay.TravelShopBuy, (RewardType)item.RewardType, item.Id, item.Num, TimingGiftType.None, 1);
                    //RecordShopByItemLog(ShopType.TravelNodeShop, currenciesTypeId.ToString(), num, ObtainWay.TravelShopBuy, (RewardType)item.RewardType, item.Id, item.Num);
                }
                KomoeEventLogShopPurchase(shopItemId, 1, currenciesTypeId, costType.ToString(), num, (int)ShopType.TravelNodeShop, ShopType.TravelNodeShop.ToString());
                ////卖完了自动进入到下一节点
                //if (heroTravelItem.BuyList.Count == TravelLibrary.ShopItemCount)
                //{
                //    //结束商店
                //    ClearHeroTravelShop(heroTravelItem);
                //}

                BIRecordTravelLog(heroTravelItem.Slot, heroId, eventItem.Type.ToString(), shopItemId);

            }

            //DB同步
            server.GameDBPool.Call(new QueryUpdateTravelHero(uid, heroTravelItem));

            msg.Result = (int)ErrorCode.Success;
            msg.HeroTravel = GetHeroTravelItemMsg(heroTravelItem);
            Write(msg);
        }

        private void ClearHeroTravelShop(TravelHeroItem heroTravelItem)
        {
            heroTravelItem.TravelEvents.RemoveAt(0);
            heroTravelItem.ShopItemList.Clear();
            heroTravelItem.BuyList.Clear();
            heroTravelItem.SoulBoneList.Clear();
        }


        public MSG_ZMZ_TRAVEL_MANAGER GenerateTravelManagerTransformMsg()
        {
            MSG_ZMZ_TRAVEL_MANAGER msg = new MSG_ZMZ_TRAVEL_MANAGER();
            Dictionary<int, TravelHeroItem> heroList = TravelMng.GetHeroList();
            foreach (var heroItem in heroList)
            {
                MSG_ZMZ_HERO_TRAVEL_ITEM msgItem = GetHeroTravelItemTransformMsg(heroItem.Value);
                msg.HeroLsit.Add(msgItem);
            }
            return msg;
        }

        private MSG_ZMZ_HERO_TRAVEL_ITEM GetHeroTravelItemTransformMsg(TravelHeroItem heroItem)
        {
            MSG_ZMZ_HERO_TRAVEL_ITEM msgItem = new MSG_ZMZ_HERO_TRAVEL_ITEM();
            msgItem.HeroId = heroItem.Id;
            msgItem.Level = heroItem.Level;
            msgItem.Affinity = heroItem.Affinity;
            msgItem.StartTime = heroItem.StartTime;
            msgItem.Slot = heroItem.Slot;
            msgItem.TravelStr = heroItem.GetTravelEventString();

            msgItem.ShopList.AddRange(heroItem.ShopItemList);
            msgItem.BuyList.AddRange(heroItem.BuyList);

            foreach (var item in heroItem.CardList)
            {
                ZMZ_TRAVEL_CARD_ITEM card = new ZMZ_TRAVEL_CARD_ITEM();
                card.Id = item.Value.Id;
                card.Level = item.Value.Level;
                card.Exp = item.Value.Exp;
                msgItem.CardList.Add(card);
            }
           
            msgItem.SoulBoneStr = heroItem.GetSoulBoneStr();

            return msgItem;
        }

        public void LoadTravelHeroInfoTransform(MSG_ZMZ_TRAVEL_MANAGER info)
        {
            foreach (var item in info.HeroLsit)
            {
                TravelHeroItem msgItem = InitHeroTravelItemTransformMsg(item);
                TravelMng.AddHeroTravelInfo(msgItem);
            }
            TravelMng.InitNaturList();

        }

        private TravelHeroItem InitHeroTravelItemTransformMsg(MSG_ZMZ_HERO_TRAVEL_ITEM heroItem)
        {
            TravelHeroItem msgItem = new TravelHeroItem();
            msgItem.Id = heroItem.HeroId;
            msgItem.Level = heroItem.Level;
            msgItem.Affinity = heroItem.Affinity;
            msgItem.StartTime = heroItem.StartTime;
            msgItem.Slot = heroItem.Slot;
            msgItem.InitTravelEvent(heroItem.TravelStr);

            msgItem.ShopItemList.AddRange(heroItem.ShopList);
            msgItem.BuyList.AddRange(heroItem.BuyList);

            foreach (var item in heroItem.CardList)
            {
                TravelCardItem card = new TravelCardItem();
                card.Id = item.Id;
                card.Level = item.Level;
                card.Exp = item.Exp;
                msgItem.AddCardItem(card);
            }

            msgItem.InitSoulBoneList(heroItem.SoulBoneStr);

            return msgItem;
        }
    }
}
