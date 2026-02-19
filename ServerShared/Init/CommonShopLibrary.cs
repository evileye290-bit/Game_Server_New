using DataProperty;
using EnumerateUtility;
using Logger;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ServerShared
{
    public class CommonShopLibrary
    {
        //key:shopId
        private static Dictionary<int, CommonShopModel> shopList = new Dictionary<int, CommonShopModel>();
        //key:ShopItemId
        private static Dictionary<int, CommonShopItemModel> shopItemList = new Dictionary<int, CommonShopItemModel>();
        //key:shopId  ShopItemId
        private static Dictionary<int, Dictionary<int, CommonShopItemModel>> itemList = new Dictionary<int, Dictionary<int, CommonShopItemModel>>();
        //key:shopId  groupId
        private static Dictionary<int, Dictionary<int, List<CommonShopItemModel>>> shopGroupList = new Dictionary<int, Dictionary<int, List<CommonShopItemModel>>>();

        private static ListMap<int, CommonShopItemModel> quality2ItemList = new ListMap<int, CommonShopItemModel>();

        private static Dictionary<int, float> couponList = new Dictionary<int, float>();      

        public static int EquipmentMaxQuality { get; private set; }
        public static int SoulBoneMaxQuality { get; private set; }

        public static int StartActivityShop { get; private set; }
        public static int EndActivityShop { get; private set; }

        public static void Init()
        {
            //shopList.Clear();
            //shopItemList.Clear();
            //itemList.Clear();
            //shopGroupList.Clear();
            //quality2ItemList.Clear();

            InitShopList();
            InitShopItemList();
            InitShopGroupList();
            InitShopConfig();
            InitCouponItem();
        }

        public static void InitShopList()
        {
            Dictionary<int, CommonShopModel> shopList = new Dictionary<int, CommonShopModel>();

            DataList dataList = DataListManager.inst.GetDataList("CommonShop");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CommonShopModel model = new CommonShopModel(data);
                shopList[model.ShopId] = model;
            }

            CommonShopLibrary.shopList = shopList;
        }

        private static void InitShopItemList()
        {
            Dictionary<int, CommonShopItemModel> shopItemList = new Dictionary<int, CommonShopItemModel>();
            Dictionary<int, Dictionary<int, CommonShopItemModel>> itemList = new Dictionary<int, Dictionary<int, CommonShopItemModel>>();
            ListMap<int, CommonShopItemModel> quality2ItemList = new ListMap<int, CommonShopItemModel>();

            DataList dataList = DataListManager.inst.GetDataList("ShopItemInfo");
            Dictionary<int, CommonShopItemModel> itemsDic;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CommonShopItemModel model = new CommonShopItemModel(data);          
                if (itemList.ContainsKey(model.ShopId))//
                {
                    if (!itemList[model.ShopId].ContainsKey(model.Id))
                    {
                        itemList[model.ShopId].Add(model.Id, model);                    
                    }
                    else
                    {
                        Log.Warn("shop items init fail, item repeated");
                    }                
                }
                else
                {
                    itemsDic = new Dictionary<int, CommonShopItemModel>();
                    itemsDic.Add(model.Id, model);
                    itemList.Add(model.ShopId, itemsDic);
                }

                if (!shopItemList.ContainsKey(model.Id))
                {
                    shopItemList.Add(model.Id, model);
                }

                if (model.MinQuality > 0)
                {
                    quality2ItemList.Add(model.MinQuality, model);
                }
            }
            CommonShopLibrary.shopItemList = shopItemList;
            CommonShopLibrary.itemList = itemList;
            CommonShopLibrary.quality2ItemList = quality2ItemList;
        }

        private static void InitShopGroupList()
        {
            Dictionary<int, Dictionary<int, List<CommonShopItemModel>>> shopGroupList = new Dictionary<int, Dictionary<int, List<CommonShopItemModel>>>();

            DataList dataList = DataListManager.inst.GetDataList("ShopItemInfo");
            List<CommonShopItemModel> itemList;
            Dictionary<int, List<CommonShopItemModel>> groupItems;
            foreach (var item in dataList)
            {
                Data data = item.Value;
                CommonShopItemModel model = new CommonShopItemModel(data);
                if (shopGroupList.TryGetValue(model.ShopId, out groupItems))
                {
                    if (groupItems.TryGetValue(model.GroupType, out itemList))
                    {
                        itemList.Add(model);
                    }
                    else
                    {
                        itemList = new List<CommonShopItemModel>();
                        itemList.Add(model);
                        groupItems.Add(model.GroupType, itemList);
                    }
                }
                else
                {
                    itemList = new List<CommonShopItemModel>();
                    itemList.Add(model);
                    groupItems = new Dictionary<int, List<CommonShopItemModel>>();
                    groupItems.Add(model.GroupType, itemList);
                    shopGroupList.Add(model.ShopId, groupItems);
                }
            }
            CommonShopLibrary.shopGroupList = shopGroupList;
        }

        private static void InitShopConfig()
        {
            Data data = DataListManager.inst.GetData("ShopConfig", 1);
            EquipmentMaxQuality = data.GetInt("EquipmentMaxQuality");
            SoulBoneMaxQuality = data.GetInt("SoulBoneMaxQuality");
        }

        private static void InitCouponItem()
        {
            Dictionary<int, float> couponList = new Dictionary<int, float>();

            DataList dataList = DataListManager.inst.GetDataList("CouponItem");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                couponList.Add(data.ID, data.GetFloat("Discount"));
            }

            CommonShopLibrary.couponList = couponList;
        }

        public static CommonShopModel GetShopModel(int shopId)
        {
            CommonShopModel shop;
            shopList.TryGetValue(shopId, out shop);
            return shop;
        }

        public static CommonShopItemModel GetShopItemModel(int shopItemId)
        {
            CommonShopItemModel item;
            shopItemList.TryGetValue(shopItemId, out item);
            return item;
        }    

        public static bool ShopIsRegular(int shopId)
        {
            CommonShopModel shop = GetShopModel(shopId);
            if (shop != null && shop.IsRegular)
            {
                return true;
            }
            return false;
        }

        public static Dictionary<int, CommonShopItemModel> GetShopItems(int shopId)
        {
            Dictionary<int, CommonShopItemModel> shopItems;
            itemList.TryGetValue(shopId, out shopItems);
            return shopItems;
        }

        public static Dictionary<int, List<CommonShopItemModel>> GetShopGroupList(int shopId)
        {
            Dictionary<int, List<CommonShopItemModel>> groupList;
            shopGroupList.TryGetValue(shopId, out groupList);
            return groupList;
        }

        public static List<CommonShopItemModel> GetQualityItems(int minId, int maxId, int quality)
        {
            List<CommonShopItemModel> itemList;
            if (!quality2ItemList.TryGetValue(quality, out itemList)) return null;

            return itemList.Where(x => x.Id >= minId && x.Id <= maxId).ToList();
        }

        public static float GetCouponDiscount(int id)
        {
            float discount;
            couponList.TryGetValue(id, out discount);
            return discount;
        }

        private static List<CommonShopModel> GetStartActivityShopList()
        {
            return shopList.Values.Where(x=>x.ShowStart != DateTime.MinValue).ToList();
        }

        private static List<CommonShopModel> GetEndActivityShopList()
        {
            return shopList.Values.Where(x => x.ShowEnd != DateTime.MinValue).ToList();
        }

        public static void CheckStartDateTiming(DateTime now, Dictionary<DateTime, List<RechargeGiftTimeType>> tasks, RechargeGiftTimeType type)
        {
            List<CommonShopModel> shopList = GetStartActivityShopList();
            if (shopList != null)
            {
                List<RechargeGiftTimeType> list;
                foreach (var shop in shopList)
                {
                    if (shop.ShowStart.Date == now.Date && now.TimeOfDay <= shop.ShowStart.TimeOfDay)
                    {
                        if (tasks.TryGetValue(shop.ShowStart, out list))
                        {
                            list.Add(type);
                        }
                        else
                        {
                            list = new List<RechargeGiftTimeType>();
                            list.Add(type);
                            tasks.Add(shop.ShowStart, list);
                        }
                        StartActivityShop = shop.ShopId;
                        break;
                    }
                }
            }
        }

        public static void CheckEndDateTiming(DateTime now, Dictionary<DateTime, List<RechargeGiftTimeType>> tasks, RechargeGiftTimeType type)
        {
            List<CommonShopModel> shopList = GetEndActivityShopList();
            if (shopList != null)
            {
                List<RechargeGiftTimeType> list;
                foreach (var shop in shopList)
                {
                    if (shop.ShowEnd.Date == now.Date && now.TimeOfDay <= shop.ShowEnd.TimeOfDay)
                    {
                        if (tasks.TryGetValue(shop.ShowEnd, out list))
                        {
                            list.Add(type);
                        }
                        else
                        {
                            list = new List<RechargeGiftTimeType>();
                            list.Add(type);
                            tasks.Add(shop.ShowEnd, list);
                        }
                        EndActivityShop = shop.ShopId;
                        break;
                    }
                }
            }
        }

        public static List<int> GetActivityStartNeedRefreshShop(DateTime now, Dictionary<int, bool> refreshFlags)
        {
            List<int> shopIds = new List<int>();

            List<CommonShopModel> shopList = GetStartActivityShopList();
            if (shopList != null)
            {
                foreach (var shop in shopList)
                {
                    bool refreshed;
                    refreshFlags.TryGetValue(shop.ShopId, out refreshed);
                    if (shop.ShowStart <= now && now < shop.ShowEnd && !refreshed)
                    {
                        shopIds.Add(shop.ShopId);
                    }
                }
            }

            return shopIds;
        }   
    }
}
