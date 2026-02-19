using DataProperty;
using Logger;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public static class CampStarsLibrary
    {
        private static List<CampStarModel> campDragons = new List<CampStarModel>();
        private static List<CampStarModel> campTigers = new List<CampStarModel>();
        private static List<CampStarModel> campPhoenixs = new List<CampStarModel>();
        private static List<CampStarModel> campTortoises = new List<CampStarModel>();
        private static SortedDictionary<int, int> campTitleLevels = new SortedDictionary<int, int>();
        public static CampBlessingModel campBlessingModel { get; private set; }

        public static void Init()
        {
            //campDragons.Clear();
            //campTigers.Clear();
            //campPhoenixs.Clear();
            //campTortoises.Clear();
            //campTitleLevels.Clear();

            DataList campStarsDataList = DataListManager.inst.GetDataList("CampStarsUpdate");
            DataList campTitleDataList = DataListManager.inst.GetDataList("CampTitle");
            Data campBlessingData = DataListManager.inst.GetData("CampBlessing", 1);

            InitCampStars(campStarsDataList);
            InitCampTitle(campTitleDataList);
            InitCampBlessing(campBlessingData);
        }

        private static void InitCampStars(DataList dataList)
        {
            List<CampStarModel> campDragons = new List<CampStarModel>();
            List<CampStarModel> campTigers = new List<CampStarModel>();
            List<CampStarModel> campPhoenixs = new List<CampStarModel>();
            List<CampStarModel> campTortoises = new List<CampStarModel>();

            List<Data> sortedDataList = new List<Data>();

            foreach (var item in dataList)
            {
                sortedDataList.Add(item.Value);
            }
            sortedDataList.Sort((left, right) =>
            {
                if (left.ID < right.ID)
                {
                    return -1;
                }
                return 1;
            });

            for (int i = 0; i < sortedDataList.Count; i++)
            {
                //CampStarModel aheadDragon = i == 0 ? null : campDragons[i - 1];
                //CampStarModel aheadTiger = i == 0 ? null : campTigers[i - 1];
                //CampStarModel aheadPhoenix = i == 0 ? null : campPhoenixs[i - 1];
                //CampStarModel aheadTortoise = i == 0 ? null : campTortoises[i - 1];
                Data curData = sortedDataList[i];
                //InitCampDragon(curData, aheadDragon);
                //InitCampTiger(curData, aheadTiger);
                //InitCampPhoenix(curData, aheadPhoenix);
                //InitCampTortoise(curData, aheadTortoise);
                InitCampDragon(campDragons, curData);
                InitCampTiger(campTigers, curData);
                InitCampPhoenix(campPhoenixs, curData);
                InitCampTortoise(campTortoises, curData);
            }

            CampStarsLibrary.campDragons = campDragons;
            CampStarsLibrary.campTigers = campTigers;
            CampStarsLibrary.campPhoenixs = campPhoenixs;
            CampStarsLibrary.campTortoises = campTortoises;
        }

        //private static void InitCampDragon(Data data, CampStarModel aheadModel)
        //{
        //    CampStarModel dragon = new CampStarModel();
        //    dragon.Level = data.ID;

        //    dragon.AttrList = new Dictionary<int, int>();

        //    string[] dragonAttrs = data.GetString("GreenDragonAttr").Split('|');
        //    foreach (var dragonAttr in dragonAttrs)
        //    {
        //        string[] dAttr = dragonAttr.Split(':');
        //        //长度目前不确定
        //        if (dAttr.Length == 2)
        //        {
        //            dragon.AttrList.Add(int.Parse(dAttr[0]), int.Parse(dAttr[1]));//
        //        }
        //        else
        //        {
        //            Log.WarnLine("InitCampStars With dragonAttr error, level {0}, dragonAttr {1}", dragon.Level, data.GetString("GreenDragonAttr"));
        //        }
        //    }

        //    if (aheadModel != null)
        //    {
        //        foreach (var aheadAttr in aheadModel.AttrList)
        //        {

        //            if (dragon.AttrList.ContainsKey(aheadAttr.Key))
        //            {
        //                dragon.AttrList[aheadAttr.Key] += aheadAttr.Value;
        //            }
        //            else
        //            {
        //                dragon.AttrList[aheadAttr.Key] = aheadAttr.Value;
        //            }
        //        }
        //    }

        //    string[] dragonCost = data.GetString("GreenDragonCost").Split(':');
        //    if (dragonCost.Length == 3)
        //    {
        //        dragon.CostItemId = int.Parse(dragonCost[0]);
        //        dragon.CostType = int.Parse(dragonCost[1]);
        //        dragon.CostNum = int.Parse(dragonCost[2]);
        //    }
        //    else
        //    {
        //        Log.WarnLine("InitCampStars With dragonCost error, level {0}, dragonCost {1}", dragon.Level, data.GetString("GreenDragonCost"));
        //    }

        //    dragon.Fee = data.GetInt("GreenDragonFee");
        //    dragon.Rate = data.GetInt("Rate");
        //    dragon.TitleLevel = data.GetInt("TitleId");
        //    campDragons.Add(dragon);
        //}

        //private static void InitCampTiger(Data data, CampStarModel aheadModel)
        //{
        //    CampStarModel tiger = new CampStarModel();
        //    tiger.Level = data.ID;

        //    tiger.AttrList = new Dictionary<int, int>();

        //    string[] tigerAttrs = data.GetString("WhiteTigerAttr").Split('|');
        //    foreach (var tigerAttr in tigerAttrs)
        //    {
        //        string[] tiAttr = tigerAttr.Split(':');
        //        if (tiAttr.Length == 2)
        //        {
        //            tiger.AttrList.Add(int.Parse(tiAttr[0]), int.Parse(tiAttr[1]));
        //        }
        //        else
        //        {
        //            Log.WarnLine("InitCampStars With tigerAttr error, level {0}, tigerAttr {1}", tiger.Level, data.GetString("WhiteTigerAttr"));
        //        }
        //    }
        //    if (aheadModel != null)
        //    {
        //        foreach (var aheadAttr in aheadModel.AttrList)
        //        {

        //            if (tiger.AttrList.ContainsKey(aheadAttr.Key))
        //            {
        //                tiger.AttrList[aheadAttr.Key] += aheadAttr.Value;
        //            }
        //            else
        //            {
        //                tiger.AttrList[aheadAttr.Key] = aheadAttr.Value;
        //            }
        //        }
        //    }
        //    string[] tigerCost = data.GetString("WhiteTigerCost").Split(':');
        //    if (tigerCost.Length == 3)
        //    {
        //        tiger.CostItemId = int.Parse(tigerCost[0]);
        //        tiger.CostType = int.Parse(tigerCost[1]);
        //        tiger.CostNum = int.Parse(tigerCost[2]);
        //    }
        //    else
        //    {
        //        Log.WarnLine("InitCampStars With tigerCost error, level {0}, tigerCost {1}", tiger.Level, data.GetString("WhiteTigerCost"));
        //    }
        //    tiger.Fee = data.GetInt("WhiteTigerFee");
        //    tiger.Rate = data.GetInt("Rate");
        //    tiger.TitleLevel = data.GetInt("TitleId");
        //    campTigers.Add(tiger);
        //}

        //private static void InitCampPhoenix(Data data, CampStarModel aheadModel)
        //{
        //    CampStarModel phoenix = new CampStarModel();
        //    phoenix.Level = data.ID;

        //    phoenix.AttrList = new Dictionary<int, int>();

        //    string[] phoenixAttrs = data.GetString("RedPhoenixAttr").Split('|');
        //    foreach (var phoenixAttr in phoenixAttrs)
        //    {
        //        string[] pAttr = phoenixAttr.Split(':');
        //        if (pAttr.Length == 2)
        //        {
        //            phoenix.AttrList.Add(int.Parse(pAttr[0]), int.Parse(pAttr[1]));
        //        }
        //        else
        //        {
        //            Log.WarnLine("InitCampStars With phoenixAttr error, level {0}, phoenixAttr {1}", phoenix.Level, data.GetString("RedPhoenixAttr"));
        //        }
        //    }
        //    if (aheadModel != null)
        //    {
        //        foreach (var aheadAttr in aheadModel.AttrList)
        //        {

        //            if (phoenix.AttrList.ContainsKey(aheadAttr.Key))
        //            {
        //                phoenix.AttrList[aheadAttr.Key] += aheadAttr.Value;
        //            }
        //            else
        //            {
        //                phoenix.AttrList[aheadAttr.Key] = aheadAttr.Value;
        //            }
        //        }
        //    }
        //    string[] phoenixCost = data.GetString("RedPhoenixCost").Split(':');
        //    if (phoenixCost.Length == 3)
        //    {
        //        phoenix.CostItemId = int.Parse(phoenixCost[0]);
        //        phoenix.CostType = int.Parse(phoenixCost[1]);
        //        phoenix.CostNum = int.Parse(phoenixCost[2]);
        //    }
        //    else
        //    {
        //        Log.WarnLine("InitCampStars With phoenixCost error, level {0}, phoenixCost {1}", phoenix.Level, data.GetString("RedPhoenixCost"));
        //    }
        //    phoenix.Fee = data.GetInt("RedPhoenixFee");
        //    phoenix.Rate = data.GetInt("Rate");
        //    phoenix.TitleLevel = data.GetInt("TitleId");
        //    campPhoenixs.Add(phoenix);
        //}

        //private static void InitCampTortoise(Data data, CampStarModel aheadModel)
        //{
        //    CampStarModel tortoise = new CampStarModel();
        //    tortoise.Level = data.ID;

        //    tortoise.AttrList = new Dictionary<int, int>();

        //    string[] tortoiseAttrs = data.GetString("BlackTortoiseAttr").Split('|');
        //    foreach (var tortoiseAttr in tortoiseAttrs)
        //    {
        //        string[] toAttr = tortoiseAttr.Split(':');
        //        if (toAttr.Length == 2)
        //        {
        //            tortoise.AttrList.Add(int.Parse(toAttr[0]), int.Parse(toAttr[1]));
        //        }
        //        else
        //        {
        //            Log.WarnLine("InitCampStars With tortoiseAttr error, level {0}, tortoiseAttr {1}", tortoise.Level, data.GetString("BlackTortoiseAttr"));
        //        }
        //    }
        //    if (aheadModel != null)
        //    {
        //        foreach (var aheadAttr in aheadModel.AttrList)
        //        {

        //            if (tortoise.AttrList.ContainsKey(aheadAttr.Key))
        //            {
        //                tortoise.AttrList[aheadAttr.Key] += aheadAttr.Value;
        //            }
        //            else
        //            {
        //                tortoise.AttrList[aheadAttr.Key] = aheadAttr.Value;
        //            }
        //        }
        //    }
        //    string[] tortoiseCost = data.GetString("BlackTortoiseCost").Split(':');
        //    if (tortoiseCost.Length == 3)
        //    {
        //        tortoise.CostItemId = int.Parse(tortoiseCost[0]);
        //        tortoise.CostType = int.Parse(tortoiseCost[1]);
        //        tortoise.CostNum = int.Parse(tortoiseCost[2]);
        //    }
        //    else
        //    {
        //        Log.WarnLine("InitCampStars With tortoiseCost error, level {0}, tortoiseCost {1}", tortoise.Level, data.GetString("BlackTortoiseCost"));
        //    }
        //    tortoise.Fee = data.GetInt("BlackTortoiseFee");
        //    tortoise.Rate = data.GetInt("Rate");
        //    tortoise.TitleLevel = data.GetInt("TitleId");
        //    campTortoises.Add(tortoise);
        //}

        public static CampStarModel GetDragonModel(int level)
        {
            return campDragons[level];
        }

        public static CampStarModel GetTigerModel(int level)
        {
            return campTigers[level];
        }

        public static CampStarModel GetPhoenixModel(int level)
        {
            return campPhoenixs[level];
        }

        public static CampStarModel GetTortoiseModel(int level)
        {
            return campTortoises[level];
        }

        private static void InitCampTitle(DataList dataList)
        {
            SortedDictionary<int, int> campTitleLevels = new SortedDictionary<int, int>();
            foreach (var item in dataList)
            {
                int prestige = item.Value.GetInt("PrestigeNum");
                if (!campTitleLevels.ContainsKey(prestige))
                {
                    campTitleLevels.Add(prestige, item.Value.ID);
                }
                else
                {
                    Log.Warn("PrestigeNum {0} already add to campTitleLevels", prestige);
                }
            }
            CampStarsLibrary.campTitleLevels = campTitleLevels;
        }

        public static int GetCampTitleLevel(int prestige)
        {
            int campTitleLevel;
            int curPrestige = 0;
            foreach (var item in campTitleLevels.Keys)
            {
                if (prestige >= item)
                {
                    curPrestige = item;
                }
                else
                {
                    campTitleLevels.TryGetValue(curPrestige, out campTitleLevel);
                    return campTitleLevel;
                }
            }
            campTitleLevels.TryGetValue(curPrestige, out campTitleLevel);
            return campTitleLevel;
        }

        private static void InitCampBlessing(Data data)
        {
            campBlessingModel = new CampBlessingModel(data);
        }

        public static CampBlessingModel GetCampBlessModel()
        {
            return campBlessingModel;
        }

        private static void InitCampDragon(List<CampStarModel> campDragons, Data data)
        {
            CampStarModel dragon = new CampStarModel();
            dragon.Level = data.ID;

            dragon.AttrList = new Dictionary<int, int>();

            string[] dragonAttrs = data.GetString("GreenDragonAttr").Split('|');
            foreach (var dragonAttr in dragonAttrs)
            {
                string[] dAttr = dragonAttr.Split(':');

                if (dAttr.Length == 2)
                {
                    dragon.AttrList.Add(int.Parse(dAttr[0]), int.Parse(dAttr[1]));
                }
                else
                {
                    Log.WarnLine("InitCampStars With dragonAttr error, level {0}, dragonAttr {1}", dragon.Level, data.GetString("GreenDragonAttr"));
                }
            }

            string[] dragonCost = data.GetString("GreenDragonCost").Split(':');
            if (dragonCost.Length == 3)
            {
                dragon.CostItemId = int.Parse(dragonCost[0]);
                dragon.CostType = int.Parse(dragonCost[1]);
                dragon.CostNum = int.Parse(dragonCost[2]);
            }
            else
            {
                Log.WarnLine("InitCampStars With dragonCost error, level {0}, dragonCost {1}", dragon.Level, data.GetString("GreenDragonCost"));
            }

            dragon.Fee = data.GetInt("GreenDragonFee");
            dragon.Rate = data.GetInt("Rate");
            dragon.TitleLevel = data.GetInt("TitleId");
            campDragons.Add(dragon);
        }

        private static void InitCampTiger(List<CampStarModel> campTigers, Data data)
        {
            CampStarModel tiger = new CampStarModel();
            tiger.Level = data.ID;

            tiger.AttrList = new Dictionary<int, int>();

            string[] tigerAttrs = data.GetString("WhiteTigerAttr").Split('|');
            foreach (var tigerAttr in tigerAttrs)
            {
                string[] tiAttr = tigerAttr.Split(':');
                if (tiAttr.Length == 2)
                {
                    tiger.AttrList.Add(int.Parse(tiAttr[0]), int.Parse(tiAttr[1]));
                }
                else
                {
                    Log.WarnLine("InitCampStars With tigerAttr error, level {0}, tigerAttr {1}", tiger.Level, data.GetString("WhiteTigerAttr"));
                }
            }

            string[] tigerCost = data.GetString("WhiteTigerCost").Split(':');
            if (tigerCost.Length == 3)
            {
                tiger.CostItemId = int.Parse(tigerCost[0]);
                tiger.CostType = int.Parse(tigerCost[1]);
                tiger.CostNum = int.Parse(tigerCost[2]);
            }
            else
            {
                Log.WarnLine("InitCampStars With tigerCost error, level {0}, tigerCost {1}", tiger.Level, data.GetString("WhiteTigerCost"));
            }
            tiger.Fee = data.GetInt("WhiteTigerFee");
            tiger.Rate = data.GetInt("Rate");
            tiger.TitleLevel = data.GetInt("TitleId");
            campTigers.Add(tiger);
        }

        private static void InitCampPhoenix(List<CampStarModel> campPhoenixs, Data data)
        {
            CampStarModel phoenix = new CampStarModel();
            phoenix.Level = data.ID;

            phoenix.AttrList = new Dictionary<int, int>();

            string[] phoenixAttrs = data.GetString("RedPhoenixAttr").Split('|');
            foreach (var phoenixAttr in phoenixAttrs)
            {
                string[] pAttr = phoenixAttr.Split(':');
                if (pAttr.Length == 2)
                {
                    phoenix.AttrList.Add(int.Parse(pAttr[0]), int.Parse(pAttr[1]));
                }
                else
                {
                    Log.WarnLine("InitCampStars With phoenixAttr error, level {0}, phoenixAttr {1}", phoenix.Level, data.GetString("RedPhoenixAttr"));
                }
            }

            string[] phoenixCost = data.GetString("RedPhoenixCost").Split(':');
            if (phoenixCost.Length == 3)
            {
                phoenix.CostItemId = int.Parse(phoenixCost[0]);
                phoenix.CostType = int.Parse(phoenixCost[1]);
                phoenix.CostNum = int.Parse(phoenixCost[2]);
            }
            else
            {
                Log.WarnLine("InitCampStars With phoenixCost error, level {0}, phoenixCost {1}", phoenix.Level, data.GetString("RedPhoenixCost"));
            }
            phoenix.Fee = data.GetInt("RedPhoenixFee");
            phoenix.Rate = data.GetInt("Rate");
            phoenix.TitleLevel = data.GetInt("TitleId");
            campPhoenixs.Add(phoenix);
        }

        private static void InitCampTortoise(List<CampStarModel> campTortoises, Data data)
        {
            CampStarModel tortoise = new CampStarModel();
            tortoise.Level = data.ID;

            tortoise.AttrList = new Dictionary<int, int>();

            string[] tortoiseAttrs = data.GetString("BlackTortoiseAttr").Split('|');
            foreach (var tortoiseAttr in tortoiseAttrs)
            {
                string[] toAttr = tortoiseAttr.Split(':');
                if (toAttr.Length == 2)
                {
                    tortoise.AttrList.Add(int.Parse(toAttr[0]), int.Parse(toAttr[1]));
                }
                else
                {
                    Log.WarnLine("InitCampStars With tortoiseAttr error, level {0}, tortoiseAttr {1}", tortoise.Level, data.GetString("BlackTortoiseAttr"));
                }
            }

            string[] tortoiseCost = data.GetString("BlackTortoiseCost").Split(':');
            if (tortoiseCost.Length == 3)
            {
                tortoise.CostItemId = int.Parse(tortoiseCost[0]);
                tortoise.CostType = int.Parse(tortoiseCost[1]);
                tortoise.CostNum = int.Parse(tortoiseCost[2]);
            }
            else
            {
                Log.WarnLine("InitCampStars With tortoiseCost error, level {0}, tortoiseCost {1}", tortoise.Level, data.GetString("BlackTortoiseCost"));
            }
            tortoise.Fee = data.GetInt("BlackTortoiseFee");
            tortoise.Rate = data.GetInt("Rate");
            tortoise.TitleLevel = data.GetInt("TitleId");
            campTortoises.Add(tortoise);
        }
    }
}
