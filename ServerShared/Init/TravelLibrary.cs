using CommonUtility;
using DataProperty;
using EnumerateUtility;
using ServerModels;
using ServerModels.Travel;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public static class TravelLibrary
    {
        private static Dictionary<int, int> slotCountList = new Dictionary<int, int>();
        private static Dictionary<int, int> levelAffinity = new Dictionary<int, int>();
        private static Dictionary<int, int> itemAffinity = new Dictionary<int, int>();
        private static Dictionary<int, TravelHeroInfo> heroList = new Dictionary<int, TravelHeroInfo>();
        private static Dictionary<int, TravelEventModel> eventList = new Dictionary<int, TravelEventModel>();
        private static Dictionary<int, TravelCardInfo> cardList = new Dictionary<int, TravelCardInfo>();
        private static Dictionary<int, int> cardLevelList = new Dictionary<int, int>();

        private static Dictionary<TowerShopItemType, int> TowerShopItemTypeLimit = new Dictionary<TowerShopItemType, int>();
        private static Dictionary<TowerShopItemType, int> TowerShopItemTypeWeight = new Dictionary<TowerShopItemType, int>();
        private static Dictionary<int, TowerShopItemModel> TowerShopItemList = new Dictionary<int, TowerShopItemModel>();
        private static Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>> TowerTypeShopItemList = new Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>>();



        public static int FriendAddAffinity { get; set; }
        public static int ShopItemCount { get; private set; }
        public static int OldCardLevel { get; private set; }
        public static void Init()
        {
            InitTravelEvent();
            InitTravelLevel();
            InitTravelItem();
            InitTravelHero();
            InitConfig();
            InitTravelCard();
            InitTravelCardLevel();

            InitShop();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("TravelConfig", 1);

            Dictionary<int, int> slotCountList = new Dictionary<int, int>();
            List<string> list = data.GetStringList("slotCount", "|");
            foreach (var itemString in list)
            {
                string[] item = StringSplit.GetArray(":", itemString);
                slotCountList[int.Parse(item[0])] = int.Parse(item[1]);
            }
            TravelLibrary.slotCountList = slotCountList;


            FriendAddAffinity = data.GetInt("friendAddAffinity");
            ShopItemCount = data.GetInt("ShopItemCount");
            ShopItemCount = data.GetInt("ShopItemCount");
            OldCardLevel = data.GetInt("OldCardLevel");
        }

        private static void InitTravelLevel()
        {
            Dictionary<int, int> levelAffinity = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("TravelLevel");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                levelAffinity.Add(data.ID, data.GetInt("affinity"));
            }
            TravelLibrary.levelAffinity = levelAffinity;
        }

        private static void InitTravelItem()
        {
            Dictionary<int, int> itemAffinity = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("TravelItem");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                itemAffinity.Add(data.ID, data.GetInt("addAffinity"));
            }
            TravelLibrary.itemAffinity = itemAffinity;
        }

        private static void InitTravelHero()
        {
            Dictionary<int, TravelHeroInfo> heroList = new Dictionary<int, TravelHeroInfo>();

            TravelHeroInfo heroInfo;
            DataList dataList = DataListManager.inst.GetDataList("TravelHero");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                heroInfo = new TravelHeroInfo(data);
                heroList.Add(data.ID, heroInfo);
            }

            TravelHeroEvent heroEvent;
            dataList = DataListManager.inst.GetDataList("TravelHeroEvent");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                heroEvent = new TravelHeroEvent(data);

                if (heroList.TryGetValue(heroEvent.HeroId, out heroInfo))
                {
                    heroInfo.AddHeroEvent(heroEvent);
                }
            }

            TravelLibrary.heroList = heroList;
        }
        private static void InitTravelEvent()
        {
            Dictionary<int, TravelEventModel> eventList = new Dictionary<int, TravelEventModel>();

            DataList dataList = DataListManager.inst.GetDataList("TravelEvent");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                eventList.Add(data.ID, new TravelEventModel(data));
            }
            TravelLibrary.eventList = eventList;
        }

        private static void InitTravelCard()
        {
            Dictionary<int, TravelCardInfo> cardList = new Dictionary<int, TravelCardInfo>();

            DataList dataList = DataListManager.inst.GetDataList("TravelCard");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                cardList.Add(data.ID, new TravelCardInfo(data));
            }
            TravelLibrary.cardList = cardList;
        }

        private static void InitTravelCardLevel()
        {
            Dictionary<int, int> cardLevelList = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("TravelCardLevel");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                cardLevelList.Add(data.ID, data.GetInt("Exp"));
            }
            TravelLibrary.cardLevelList = cardLevelList;
        }

        public static int GetCardLevelExp(int level)
        {
            int info;
            cardLevelList.TryGetValue(level, out info);
            return info;
        }

        public static TravelCardInfo GetCardInfo(int cardId)
        {
            TravelCardInfo info;
            cardList.TryGetValue(cardId, out info);
            return info;
        }

        public static Dictionary<int, TravelEventModel> GetTravelEventLsit()
        {
            return eventList;
        }

        //public static TravelEventModel GetTravelEvent(int num)
        //{
        //    TravelEventModel model;
        //    eventList.TryGetValue(num, out model);
        //    return model;
        //}

        public static TravelHeroInfo GetHeroInfo(int heroId)
        {
            TravelHeroInfo info;
            heroList.TryGetValue(heroId, out info);
            return info;
        }

        public static int GetItemAffinity(int itemId)
        {
            int affinity;
            itemAffinity.TryGetValue(itemId, out affinity);
            return affinity;
        }

        public static int GetLevelAffinity(int level)
        {
            int affinity;
            levelAffinity.TryGetValue(level, out affinity);
            return affinity;
        }

        public static int GetSlotCount(int affinity)
        {
            int count = 0;
            lock (slotCountList)
            {
                foreach (var item in slotCountList)
                {
                    if (affinity >= item.Key)
                    {
                        count = item.Value;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return count;
        }


        #region shop
        private static void InitShop()
        {
            Dictionary<TowerShopItemType, int> TowerShopItemTypeLimit = new Dictionary<TowerShopItemType, int>();
            Dictionary<TowerShopItemType, int> TowerShopItemTypeWeight = new Dictionary<TowerShopItemType, int>();
            Dictionary<int, TowerShopItemModel> TowerShopItemList = new Dictionary<int, TowerShopItemModel>();
            Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>> TowerTypeShopItemList = new Dictionary<TowerShopItemType, Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>>();

            DataList dataList = DataListManager.inst.GetDataList("IslandChallengeShopItemTypeRandom");
            foreach (var kv in dataList)
            {
                TowerShopItemType type = (TowerShopItemType)kv.Value.GetInt("Type");
                TowerShopItemTypeWeight[type] = kv.Value.GetInt("Weight");
                TowerShopItemTypeLimit[type] = kv.Value.GetInt("TaskLimit");
            }

            dataList = DataListManager.inst.GetDataList("IslandChallengeShopItem");
            foreach (var kv in dataList)
            {
                TowerShopItemModel model = new TowerShopItemModel(kv.Value);
                TowerShopItemList.Add(model.Id, model);

                Dictionary<TowerShopItemQuality, TowerShopItemQualityItems> modelist;
                if (!TowerTypeShopItemList.TryGetValue(model.Type, out modelist))
                {
                    modelist = new Dictionary<TowerShopItemQuality, TowerShopItemQualityItems>();
                    TowerTypeShopItemList[model.Type] = modelist;
                }

                bool added = false;
                foreach (var item in modelist)
                {
                    if (model.QualityMin == item.Key.Min && model.QualityMax == item.Key.Max)
                    {
                        added = true;
                        item.Value.AddItem(model);
                    }
                }
                if (!added)
                {
                    modelist.Add(new TowerShopItemQuality() { Min = model.QualityMin, Max = model.QualityMax }, new TowerShopItemQualityItems(model));
                }
            }
            TravelLibrary.TowerShopItemTypeLimit = TowerShopItemTypeLimit;
            TravelLibrary.TowerShopItemTypeWeight = TowerShopItemTypeWeight;
            TravelLibrary.TowerShopItemList = TowerShopItemList;
            TravelLibrary.TowerTypeShopItemList = TowerTypeShopItemList;
        }

        public static TowerShopItemModel GetIslandChallengeShopItemModel(int id)
        {
            TowerShopItemModel model;
            TowerShopItemList.TryGetValue(id, out model);
            return model;
        }

        public static TowerShopItemType RandomShopItemType(int mainTaskId)
        {
            int weight = 0;
            Dictionary<TowerShopItemType, int> itemTypeWeight = new Dictionary<TowerShopItemType, int>();
            TowerShopItemTypeLimit.ForEach(x =>
            {
                if (mainTaskId >= x.Value)
                {
                    weight += TowerShopItemTypeWeight[x.Key];
                    itemTypeWeight[x.Key] = weight;
                }
            });

            int ratio = RAND.Range(0, weight - 1);
            TowerShopItemType rewardType = itemTypeWeight.Where(x => ratio < x.Value).First().Key;
            return rewardType;
        }

        public static TowerShopItemModel RandomShopItem(TowerShopItemType rewardType, int quality)
        {
            Dictionary<TowerShopItemQuality, TowerShopItemQualityItems> modelist;
            if (!TowerTypeShopItemList.TryGetValue(rewardType, out modelist)) return null;

            foreach (var kv in modelist)
            {
                if (quality >= kv.Key.Min && quality <= kv.Key.Max) return kv.Value.RandomItem();
            }

            return null;
        }

        #endregion
    }
}
