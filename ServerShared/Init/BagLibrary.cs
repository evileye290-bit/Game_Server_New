using DataProperty;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerModels.Bag;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;

namespace ServerShared
{
    public class BagLibrary
    {
        private static Dictionary<int, HeroFragmentModel> heroFragmentItems = new Dictionary<int, HeroFragmentModel>();
        private static Dictionary<int, FaceFrameModel> faceFrameItems = new Dictionary<int, FaceFrameModel>();
        private static Dictionary<int, ItemModel> normalItems = new Dictionary<int, ItemModel>();
        private static Dictionary<int, ItemUsingModel> itemUsing = new Dictionary<int, ItemUsingModel>();
        private static Dictionary<int, ItemChooseBoxModel> itemChooseBox = new Dictionary<int, ItemChooseBoxModel>();
        private static Dictionary<int, ItemChooseBoxRewardModel> itemChooseBoxReward = new Dictionary<int, ItemChooseBoxRewardModel>();
        private static Dictionary<int, ItemResolveModel> itemResolve = new Dictionary<int, ItemResolveModel>();
        private static Dictionary<int, ItemForgeModel> itemForge = new Dictionary<int, ItemForgeModel>();
        private static Dictionary<int, BagSpaceModel> bagSpaceIncrease = new Dictionary<int, BagSpaceModel>();//背包扩容表
        private static List<SoulRingResolveModel> soulRingResolveList = new List<SoulRingResolveModel>();
        private static Dictionary<int, ItemReceive> itemReceive = new Dictionary<int, ItemReceive>();
        private static Dictionary<int, List<int>> itemChangeList = new Dictionary<int, List<int>>();
        private static Dictionary<int, ItemChangeModel> itemChangeItemList = new Dictionary<int, ItemChangeModel>();
        //Fashion
        private static Dictionary<int, FashionModel> fashionItems = new Dictionary<int, FashionModel>();

        private static List<XuanyuWeightBySecretModel> xuanyuDropModel = new List<XuanyuWeightBySecretModel>();

        private static Dictionary<int, ItemExchangeRewardModel> itemExchangeReward = new Dictionary<int, ItemExchangeRewardModel>();

        /// <summary>
        /// 改名卡相关
        /// </summary>
        public static int ChangeNameTicketId { get; private set; }
        public static int ChangeNameTicketNum { get; private set; }
        public static int ChangeNameTicketCost { get; private set; }

        /// <summary>
        /// 背包扩容券id
        /// </summary>
        public static int BagTicketId { get; private set; }
        public static int BagTicketPrice { get; private set; }
        public static int BagInitSpace { get; private set; }
        public static int BagMaxSpace { get; private set; }
        public static int BagFullEmailId { get; private set; }//背包满了发邮件
        public static int BatchCountLimit { get; private set; }
        public static int RandomEquipBoxMinQuality { get; private set; }
        public static int RandomSoulBoneBoxMinQuality { get; private set; }
        public static int MaxPrefix { get; private set; }
        /// <summary>
        /// 物品数据展示每页条数
        /// </summary>
        public static int CountPerPage { get; private set; }

        public static int IncreaseSpacePerTicket { get; private set; }

        public static void Init()
        {
            DataList itemDataList = DataListManager.inst.GetDataList("Item");
            DataList itemUsingDataList = DataListManager.inst.GetDataList("ItemUsing");
            DataList ItemResolveDataList = DataListManager.inst.GetDataList("ItemResolve");
            DataList ItemForgeDataList = DataListManager.inst.GetDataList("ItemForge");
            DataList fashionDataList = DataListManager.inst.GetDataList("Fashion");
            DataList faceFrameDataList = DataListManager.inst.GetDataList("FaceFrame");
            DataList heroFragmentDataList = DataListManager.inst.GetDataList("HeroFragment");
            DataList soulRingResolveDataList = DataListManager.inst.GetDataList("SoulRingResolve");
            DataList ItemReceiveDataList = DataListManager.inst.GetDataList("ItemReceive");


            InitItem(itemDataList);
            InitItemUsing(itemUsingDataList);
            InitItemResolve(ItemResolveDataList);
            InitItemForge(ItemForgeDataList);
            InitFashionItem(fashionDataList);
            InitItemFaceFrame(faceFrameDataList);
            InitItemHeroFragment(heroFragmentDataList);
            InitSoulRingResolve(soulRingResolveDataList);
            InitItemReceiveDataList(ItemReceiveDataList);

            InitItemChooseBox();
            InitItemChooseBoxReward();

            InitXuanyuDrop();

            DataList bagSpaceDataList = DataListManager.inst.GetDataList("BagSpace");
            InitBagSpace(bagSpaceDataList);

            InitBagConfig();

            InitItemChangeList();

            InitItemExchangeReward();
        }


        private static void InitItemChangeList()
        {
            Dictionary<int, List<int>> itemChangeList = new Dictionary<int, List<int>>();
            Dictionary<int, ItemChangeModel> itemChangeItemList = new Dictionary<int, ItemChangeModel>();

            DataList dataList = DataListManager.inst.GetDataList("ItemChange");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                itemChangeList[data.ID] = data.GetIntList("Change", "|");
            }

            dataList = DataListManager.inst.GetDataList("ItemChangeExt");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                itemChangeItemList[data.ID] = new ItemChangeModel(data);
            }
            BagLibrary.itemChangeList = itemChangeList;
            BagLibrary.itemChangeItemList = itemChangeItemList;
        }

        public static string GetItemChange(int id, DateTime time)
        {
            string reward = string.Empty;
            List<int> list;
            if (itemChangeList.TryGetValue(id, out list))
            {
                ItemChangeModel item;
                foreach (var changeId in list)
                {
                    if (itemChangeItemList.TryGetValue(changeId, out item))
                    {
                        if (item.CheckTime(time))
                        {
                            return item.Reward;
                        }
                    }
                }
            }
            return reward;
        }

        private static void InitSoulRingResolve(DataList dataList)
        {
            List<SoulRingResolveModel> soulRingResolveList = new List<SoulRingResolveModel>();
            //soulRingResolveList.Clear();
            foreach (var kv in dataList)
            {
                soulRingResolveList.Add(new SoulRingResolveModel(kv.Value));
            }
            BagLibrary.soulRingResolveList = soulRingResolveList;
        }

        public static SoulRingResolveModel GetSoulRingResolveModel(int year)
        {
            return soulRingResolveList.Where(x => year >= x.MinYear && year <= x.MaxYear).FirstOrDefault();
        }

        public static ItemModel GetItemModel(int typeId)
        {
            ItemModel dataInfo;
            if (normalItems.TryGetValue(typeId, out dataInfo))
            {
            }
            else
            {
                Log.Warn($"have not this type of consume item id {typeId}, please check it!");
            }

            return dataInfo;
        }

        public static Data GetItemModelData(int typeId)
        {
            ItemModel temp = GetItemModel(typeId);

            return temp == null ? null : temp.Data;
        }

        public static ItemUsingModel GetItemUsingModel(int typeId)
        {
            ItemUsingModel temp;
            if (itemUsing.TryGetValue(typeId, out temp))
            {
            }
            else
            {
                Log.Warn($"Have not this type of ItemUsingModel item id {typeId}");
            }

            return temp;
        }

        public static ItemResolveModel GetItemResolveModel(int typeId)
        {
            ItemResolveModel temp;
            if (itemResolve.TryGetValue(typeId, out temp))
            {
            }
            else
            {
                Log.Warn($"Have not this type of ItemResolveModel item id {typeId}");
            }

            return temp;
        }

        public static ItemForgeModel GetItemForgeModel(int typeId)
        {
            ItemForgeModel temp;
            if (itemForge.TryGetValue(typeId, out temp))
            {
            }
            else
            {
                Log.Warn($"Have not this type of ItemResolveModel item id {typeId}");
            }

            return temp;
        }

        public static FashionModel GetFashionModel(int typeId)
        {
            FashionModel fashion;
            if (fashionItems.TryGetValue(typeId, out fashion))
            {
            }
            else
            {
                Log.Warn($"Have not this type of fashion id {typeId}");
            }

            return fashion;
        }


        public static Data GetFashionModelData(int typeId)
        {
            FashionModel fashion = GetFashionModel(typeId);

            return fashion == null ? null : fashion.Data;
        }

        public static FaceFrameModel GetFaceFrameModel(int typeId)
        {
            FaceFrameModel fashion;
            if (faceFrameItems.TryGetValue(typeId, out fashion))
            {
            }
            else
            {
                Log.Warn($"Have not this type of FaceFrame id {typeId}");
            }

            return fashion;
        }


        public static HeroFragmentModel GetHeroFragmentModel(int typeId)
        {
            HeroFragmentModel fashion;
            if (heroFragmentItems.TryGetValue(typeId, out fashion))
            {
            }
            else
            {
                Log.Warn($"Have not this type of HeroFragment id {typeId}");
            }

            return fashion;
        }

        public static int GetBagSpaceIncreaseCostTicketNum(int currSpace, int increaseNum)
        {
            int endSpace = currSpace + increaseNum;

            return bagSpaceIncrease[endSpace].Ticket - bagSpaceIncrease[currSpace].Ticket;
        }

        private static void InitItem(DataList dataList)
        {
            Dictionary<int, ItemModel> normalItems = new Dictionary<int, ItemModel>();
            string trueStr = "1";
            foreach (var item in dataList)
            {
                ItemModel temp = new ItemModel();
                Data data = item.Value;
                temp.Id = data.ID;
                temp.MainType = (MainType)data.GetInt("MainType");
                temp.SubType = data.GetInt("SubType");
                temp.Quality = data.GetInt("Quality");
                temp.PileMax = data.GetInt("PileMax");
                temp.IsUsable = data.GetString("IsUsable") == trueStr;
                temp.IsResolve = data.GetString("IsResolve") == trueStr;
                temp.IsComposed = data.GetString("IsCompose") == trueStr;
                temp.IsVisible = data.GetBoolean("IsVisible");
                temp.IsSalable = !string.IsNullOrEmpty(data.GetString("SellingPrice"));
                temp.LevelLimit = data.GetInt("LevelLimit");
                temp.UsableNum = data.GetInt("UsableNum");
                temp.LevelUpNum = data.GetInt("LevelUpNum");

                temp.Data = data;

                normalItems.Add(temp.Id, temp);
            }
            BagLibrary.normalItems = normalItems;
        }

        private static void InitItemUsing(DataList dataList)
        {
            Dictionary<int, ItemUsingModel> itemUsing = new Dictionary<int, ItemUsingModel>();
            foreach (var item in dataList)
            {
                ItemUsingModel temp = new ItemUsingModel();
                temp.Data = item.Value;
                Data data = item.Value;
                temp.Id = data.ID;
                temp.Type = data.GetInt("Type");
                temp.Rewards = data.GetString("Gain");

                itemUsing.Add(temp.Id, temp);
            }
            BagLibrary.itemUsing = itemUsing;
        }

        private static void InitItemChooseBox()
        {
            DataList dataList = DataListManager.inst.GetDataList("ItemChooseBox");
            Dictionary<int, ItemChooseBoxModel> itemChooseBox = new Dictionary<int, ItemChooseBoxModel>();
            foreach (var item in dataList)
            {
                Data data = item.Value;
                ItemChooseBoxModel temp = new ItemChooseBoxModel();
                temp.Id = data.ID;
                temp.Grade = data.GetIntList("Grade","|");
                itemChooseBox.Add(temp.Id, temp);
            }
            BagLibrary.itemChooseBox = itemChooseBox;
        }
        private static void InitItemChooseBoxReward()
        {
            DataList dataList = DataListManager.inst.GetDataList("ItemChooseBoxExt");
            Dictionary<int, ItemChooseBoxRewardModel> itemChooseBoxReward = new Dictionary<int, ItemChooseBoxRewardModel>();
            foreach (var item in dataList)
            {
                Data data = item.Value;
                ItemChooseBoxRewardModel temp = new ItemChooseBoxRewardModel();
                temp.Id = data.ID;
                temp.Count = data.GetInt("Count");
                temp.Rewards = data.GetStringList("Reward", "|");
                itemChooseBoxReward.Add(temp.Id, temp);
            }
            BagLibrary.itemChooseBoxReward = itemChooseBoxReward;
        }

        private static void InitItemResolve(DataList dataList)
        {
            Dictionary<int, ItemResolveModel> itemResolve = new Dictionary<int, ItemResolveModel>();
            foreach (var item in dataList)
            {
                ItemResolveModel temp = new ItemResolveModel();
                Data data = item.Value;
                temp.Id = data.ID;
                //temp.Result = data.GetInt("Result");
                //temp.Num = data.GetInt("Num");
                //temp.Currency = data.GetInt("Currency");
                //temp.Price = data.GetInt("Price");
                temp.Data = data;

                itemResolve.Add(temp.Id, temp);
            }
            BagLibrary.itemResolve = itemResolve;
        }

        private static void InitItemForge(DataList dataList)
        {
            Dictionary<int, ItemForgeModel> itemForge = new Dictionary<int, ItemForgeModel>();
            foreach (var item in dataList)
            {
                ItemForgeModel temp = new ItemForgeModel();
                Data data = item.Value;
                temp.Id = data.ID;
                temp.Type = data.GetInt("Type");
                temp.Product = data.GetInt("Product");
                temp.Num = data.GetInt("Num");
                temp.CostMeterial = StringSplit.GetKVPairs(data.GetString("Material"));

                temp.Data = data;

                data.GetString("Product").ToList('|').ForEach(x => itemForge.Add(x, temp));
            }
            BagLibrary.itemForge = itemForge;
        }

        private static void InitItemFaceFrame(DataList dataList)
        {
            Dictionary<int, FaceFrameModel> faceFrameItems = new Dictionary<int, FaceFrameModel>();
            foreach (var item in dataList)
            {
                FaceFrameModel temp = new FaceFrameModel();
                Data data = item.Value;
                temp.Id = data.ID;
                temp.MainType = (MainType)data.GetInt("MainType");
                temp.Data = data;

                faceFrameItems.Add(temp.Id, temp);
            }
            BagLibrary.faceFrameItems = faceFrameItems;
        }

        private static void InitFashionItem(DataList dataList)
        {
            Dictionary<int, FashionModel> fashionItems = new Dictionary<int, FashionModel>();
            foreach (var item in dataList)
            {
                FashionModel temp = new FashionModel();
                Data data = item.Value;
                temp.Id = data.ID;
                temp.TypeId = data.ID;
                temp.MainType = (MainType)data.GetInt("MainType");
                temp.SonType = data.GetInt("SubType");
                temp.Data = data;

                fashionItems.Add(temp.Id, temp);
            }
            BagLibrary.fashionItems = fashionItems;
        }

        private static void InitBagSpace(DataList dataList)
        {
            Dictionary<int, BagSpaceModel> bagSpaceIncrease = new Dictionary<int, BagSpaceModel>();
            foreach (var item in dataList)
            {
                BagSpaceModel temp = new BagSpaceModel();
                Data data = item.Value;
                temp.Id = data.ID;
                temp.Ticket = data.GetInt("TypeId");
                temp.Ticket = data.GetInt("Ticket");
                temp.Data = data;

                //临时使用id
                bagSpaceIncrease.Add(temp.Id, temp);
            }
            BagLibrary.bagSpaceIncrease = bagSpaceIncrease;
        }

        private static void InitItemHeroFragment(DataList dataList)
        {
            Dictionary<int, HeroFragmentModel> heroFragmentItems = new Dictionary<int, HeroFragmentModel>();
            foreach (var item in dataList)
            {
                HeroFragmentModel temp = new HeroFragmentModel();
                Data data = item.Value;
                temp.Id = data.ID;
                temp.MainType = (MainType)data.GetInt("MainType");
                //temp.HeroToNum = data.GetInt("HeroToNum");
                temp.Data = data;

                heroFragmentItems.Add(temp.Id, temp);
            }
            BagLibrary.heroFragmentItems = heroFragmentItems;
        }

        public static void InitBagConfig()
        {
            try
            {
                // Init BagConfig
                Data bagConfig = DataListManager.inst.GetData("BagConfig", 1);

                //改名相关
                string strTicket = bagConfig.GetString("ChangeNameTicket");
                string[] arr = strTicket.Split('|');
                ChangeNameTicketId = int.Parse(arr[0]);
                ChangeNameTicketNum = int.Parse(arr[1]);
                ChangeNameTicketCost = bagConfig.GetInt("ChangeNameDiamond");//改名卡钻石

                BagTicketId = bagConfig.GetInt("BagTicketId");//背包扩容券
                BagTicketPrice = bagConfig.GetInt("BagTicketPrice");//扩容券钻石价格
                BagInitSpace = bagConfig.GetInt("BagInitSpace");//背包初始空间
                BagMaxSpace = bagConfig.GetInt("BagMaxSpace");
                BagFullEmailId = bagConfig.GetInt("BagFullEmailId");
                BatchCountLimit = bagConfig.GetInt("BatchCountLimit");
                RandomEquipBoxMinQuality = bagConfig.GetInt("RandomEquipBoxMinQuality");
                RandomSoulBoneBoxMinQuality = bagConfig.GetInt("RandomSoulBoneBoxMinQuality");
                MaxPrefix = bagConfig.GetInt("MaxPrefix");
                IncreaseSpacePerTicket = bagConfig.GetInt("IncreaseSpacePerTicket");
            }
            catch (Exception e)
            {
                Log.Alert("Load Bag Xml Fail: " + e.ToString());
            }

        }

        private static void InitItemReceiveDataList(DataList dataList)
        {
            Dictionary<int, ItemReceive> itemReceive = new Dictionary<int, ItemReceive>();
            foreach (var item in dataList)
            {
                Data data = item.Value;
                ItemReceive temp = new ItemReceive();
                temp.Id = data.ID;
                temp.MainType = data.GetInt("MainType");
                temp.Rewards = data.GetString("Rewards");
                itemReceive.Add(temp.Id, temp);
            }
            BagLibrary.itemReceive = itemReceive;
        }

        public static ItemReceive GetItemReceive(int id)
        {
            ItemReceive model;
            itemReceive.TryGetValue(id, out model);
            return model;
        }

        public static ItemChooseBoxModel GetChooseBox(int id)
        {
            ItemChooseBoxModel model;
            itemChooseBox.TryGetValue(id, out model);
            return model;
        }

        public static ItemChooseBoxRewardModel GetChooseBoxReward(int id)
        {
            ItemChooseBoxRewardModel model;
            itemChooseBoxReward.TryGetValue(id, out model);
            return model;
        }
        private static void InitXuanyuDrop()
        {
            List<XuanyuWeightBySecretModel> xuanyuDropModel = new List<XuanyuWeightBySecretModel>();
            var dataList = DataListManager.inst.GetDataList("XuanyuWeightBySecret");
            foreach (var item in dataList)
            {
                xuanyuDropModel.Add(new XuanyuWeightBySecretModel(item.Value));
            }
            BagLibrary.xuanyuDropModel = xuanyuDropModel;
        }

        public static XuanyuWeightBySecretModel GetXuanyuWeightBySecretModel(int secret)
        {
            return xuanyuDropModel.FirstOrDefault(x => secret >= x.Min && secret <= x.Max);
        }

        public static void InitItemExchangeReward()
        {
            Dictionary<int, ItemExchangeRewardModel> itemExchangeReward = new Dictionary<int, ItemExchangeRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("ItemExchangeReward");

            foreach (var item in dataList)
            {
                Data data = item.Value;
                ItemExchangeRewardModel model = new ItemExchangeRewardModel(data);
                itemExchangeReward.Add(model.Id, model);
            }

            BagLibrary.itemExchangeReward = itemExchangeReward;
        }

        public static ItemExchangeRewardModel GetItemExchangeReward(int id)
        {
            ItemExchangeRewardModel model;
            itemExchangeReward.TryGetValue(id, out model);
            return model;
        }
    }
}
