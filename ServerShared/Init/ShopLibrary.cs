using System.Collections.Generic;
using ServerModels;
using EnumerateUtility;
using DataProperty;
using System;
using CommonUtility;
using System.Linq;

namespace ServerShared
{
    static public class ShopLibrary
    {
        private static Dictionary<ShopType, ShopModel> shopList = new Dictionary<ShopType, ShopModel>();
        //private static Dictionary<ShopType, Dictionary<int, ShopItemModel>> shopItemList = new Dictionary<ShopType, Dictionary<int, ShopItemModel>>();

        private static Dictionary<TimeSpan, List<ShopType>> dailRefreshShopList = new Dictionary<TimeSpan, List<ShopType>>();


        public static void Init()
        {
            //shopList.Clear();
            //shopItemList.Clear();

            InitShop();
        }

        public static ShopModel GetShopModel(ShopType shopType)
        {
            ShopModel model;
            shopList.TryGetValue(shopType, out model);
            return model;
        }

        //public static ShopItemModel GetShopItemModel(ShopType shopType, int id)
        //{
        //    Dictionary<int, ShopItemModel> modelList;
        //    if (shopItemList.TryGetValue(shopType, out modelList))
        //    {
        //        ShopItemModel model;
        //        if (modelList.TryGetValue(id, out model)) return model;
        //    }
        //    return null;
        //}

        private static void InitShop()
        {
            Dictionary<ShopType, ShopModel> shopList = new Dictionary<ShopType, ShopModel>();
            Dictionary<TimeSpan, List<ShopType>> dailRefreshShopList = new Dictionary<TimeSpan, List<ShopType>>();

            DataList dataList = DataListManager.inst.GetDataList("Shop");
            foreach (var kv in dataList)
            {
                ShopModel model = new ShopModel(kv.Value);
                shopList[model.ShopType] = model;

                AddToDailRefresh(dailRefreshShopList, kv.Value, model.ShopType);
            }

            ShopLibrary.shopList = shopList;
            ShopLibrary.dailRefreshShopList = dailRefreshShopList;
        }


        private static void AddToDailRefresh(Dictionary<TimeSpan, List<ShopType>> dailRefreshShopList, Data data, ShopType shopType)
        {
            string times = data.GetString("RefreshTime");

            string[] list = StringSplit.GetArray("|", times);
            foreach (var time in list)
            {
                if (!string.IsNullOrEmpty(time))
                {
                    TimeSpan timeSpan = TimeSpan.Parse(time);

                    List<ShopType> shopList;
                    if (!dailRefreshShopList.TryGetValue(timeSpan, out shopList))
                    {
                        shopList = new List<ShopType>() { shopType };
                        dailRefreshShopList.Add(timeSpan, shopList);
                    }
                    else
                    {
                        shopList.Add(shopType);
                    }
                }
            }
        }

        public static List<ShopType> CheckRefreshShopList(DateTime LastRefresh, DateTime now)
        {
            List<ShopType> shopList = new List<ShopType>(); 

            //检查是否是隔天了
            if (LastRefresh.Date < now.Date)
            {
                TimeSpan span = now - LastRefresh;
                //查看时间间隔
                if (span.TotalDays >= 1)
                {
                    dailRefreshShopList.ForEach(x => shopList.AddRange(x.Value));
                }
                else
                {
                    //如果没大于1，说明0点前登录或者刷新过，
                    foreach (var kv in dailRefreshShopList)
                    {
                        //刷新时间只要大于上次刷新时间或者小于当前时间就可以刷新
                        if (LastRefresh.TimeOfDay < kv.Key || kv.Key <= now.TimeOfDay)
                        {
                            shopList.AddRange(kv.Value);
                        }
                    }
                }
            }
            else if (LastRefresh.Date == now.Date)
            {
                //日期相同，上次是同一天刷新时间
                foreach (var kv in dailRefreshShopList)
                {
                    //刷新时间只要大于上次刷新时间并且小于当前时间就可以刷新
                    if (LastRefresh.TimeOfDay < kv.Key && kv.Key <= now.TimeOfDay)
                    {
                        shopList.AddRange(kv.Value);
                    }
                }
            }

            return shopList.Distinct().ToList();
        }

    }
}
