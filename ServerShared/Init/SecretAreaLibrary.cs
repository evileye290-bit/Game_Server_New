using System.Collections.Generic;
using ServerModels;
using DataProperty;

namespace ServerShared
{
    public class SecretAreaLibrary
    {
        private static Dictionary<int, SecretAreaModel> sercetAreaList = new Dictionary<int, SecretAreaModel>();
        private static Dictionary<int, SecretAreaModel> sercetAreaListIndexByDungeonId = new Dictionary<int, SecretAreaModel>();

        private static Dictionary<int, Dictionary<int, BingoDropModel>> tireSoulBoneList = new Dictionary<int, Dictionary<int, BingoDropModel>>();
        private static Dictionary<int, BonusGroup> tireSoulBoneBonus = new Dictionary<int, BonusGroup>();

        public static int HuntingPrice { get; private set; }
        public static int BonusPrice { get; private set; }
        public static int FirstSecretArea { get; private set; }
        public static int SpeedUpLimitId { get; private set; }


        public static void Init()
        {
            //sercetAreaList.Clear();
            //sercetAreaListIndexByDungeonId.Clear();
            //tireSoulBoneList.Clear();
            //tireSoulBoneBonus.Clear();

            InitConfig();
            InitSecretArea();
            InitSoulBoneShopItems();
            InitSoulBoneBonus();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("SecretAreaConfig", 1);
            HuntingPrice = data.GetInt("HuntingPrice");
            BonusPrice = data.GetInt("BonusPrice");
            SpeedUpLimitId = data.GetInt("SpeedUpLimitId");
        }

        private static void InitSecretArea()
        {
            Dictionary<int, SecretAreaModel> sercetAreaList = new Dictionary<int, SecretAreaModel>();
            Dictionary<int, SecretAreaModel> sercetAreaListIndexByDungeonId = new Dictionary<int, SecretAreaModel>();
            SecretAreaModel model;
            DataList dataList = DataListManager.inst.GetDataList("SecretArea");
            foreach (var kv in dataList)
            {
                model = new SecretAreaModel(kv.Value);
                sercetAreaList.Add(model.Id, model);
                sercetAreaListIndexByDungeonId.Add(model.DungeonId, model);

                if (FirstSecretArea != 0)
                {
                    FirstSecretArea = kv.Value.ID;
                }
            }
            SecretAreaLibrary.sercetAreaList = sercetAreaList;
            SecretAreaLibrary.sercetAreaListIndexByDungeonId = sercetAreaListIndexByDungeonId;
        }

        private static void InitSoulBoneShopItems()
        {
            Dictionary<int, Dictionary<int, BingoDropModel>> tireSoulBoneList = new Dictionary<int, Dictionary<int, BingoDropModel>>();
            DataList dataList = DataListManager.inst.GetDataList("BingoDrop");
            foreach (var kv in dataList)
            {
                BingoDropModel model = new BingoDropModel(kv.Value);

                Dictionary<int, BingoDropModel> dic;
                if (!tireSoulBoneList.TryGetValue(model.Tire, out dic))
                {
                    dic = new Dictionary<int, BingoDropModel>();
                    dic.Add(model.Id, model);
                    tireSoulBoneList.Add(model.Tire, dic);
                }
                else
                {
                    dic.Add(model.Id, model);
                }
            }
            SecretAreaLibrary.tireSoulBoneList = tireSoulBoneList;
        }

        private static void InitSoulBoneBonus()
        {
            Dictionary<int, BonusGroup> tireSoulBoneBonus = new Dictionary<int, BonusGroup>();
            DataList dataList = DataListManager.inst.GetDataList("BingoBonus");
            foreach (var kv in dataList)
            {
                BingoBonusModel model = new BingoBonusModel(kv.Value);

                BonusGroup group;
                if (!tireSoulBoneBonus.TryGetValue(model.Tire, out group))
                {
                    group = new BonusGroup(model.Tire);
                    group.AddBonus(model);
                    tireSoulBoneBonus.Add(model.Tire, group);
                }
                else
                {
                    group.AddBonus(model);
                }
            }
            SecretAreaLibrary.tireSoulBoneBonus = tireSoulBoneBonus;
        }

        public static SecretAreaModel Get(int id)
        {
            SecretAreaModel model;
            sercetAreaList.TryGetValue(id, out model);
            return model;
        }

        public static SecretAreaModel GetModelByDungeonId(int dungeonId)
        {
            SecretAreaModel model;
            sercetAreaListIndexByDungeonId.TryGetValue(dungeonId, out model);
            return model;
        }


        public static Dictionary<int, BingoDropModel> GetTireSoulBoneItems(int tire)
        {
            Dictionary<int, BingoDropModel> items;
            tireSoulBoneList.TryGetValue(tire, out items);
            return items;
        }

        public static BonusGroup GetBonusGroup(int tire)
        {
            BonusGroup group;
            tireSoulBoneBonus.TryGetValue(tire, out group);
            return group;
        }
    }
}
