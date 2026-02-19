using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class DrawLibrary
    {
        //各个抽奖的品质概率
        public static Dictionary<int, DrawQualityModel> QualityList = new Dictionary<int, DrawQualityModel>();
        //不同品质各个英雄的权重
        private static Dictionary<int, Dictionary<int, DrawHeroQualityModel>> heroRatioList = new Dictionary<int, Dictionary<int, DrawHeroQualityModel>>();
        //英雄的羁绊
        private static Dictionary<int, HeroComboModel> heroComboList = new Dictionary<int, HeroComboModel>();
        public static Dictionary<int, List<int>> ComboGroupList = new Dictionary<int, List<int>>();
        //开启时间列表
        private static Dictionary<int, List<DrawTimeModel>> drawTimeList = new Dictionary<int, List<DrawTimeModel>>();

        private static Dictionary<int, ConstellationModel> constellationList = new Dictionary<int, ConstellationModel>();
        public static Dictionary<int, string> drawRewardList = new Dictionary<int, string>();
        private static HashSet<int> heroTypeIds = new HashSet<int>();
        private static Dictionary<int, List<DrawRatioChangeModel>> ratioChangeList = new Dictionary<int, List<DrawRatioChangeModel>>();
        public static void Init(DateTime openServer)
        {
            InitQualityList();

            InitDrawRatioChange();

            InitHeroRatioList(openServer, DateTime.Now);

            InitHeroComboList();

            InitDrawTime(openServer);

            //InitConstellationList();

            CheckNoSameQualityMaxCount();

            InitDrawHeroReward();

        }

        private static void CheckNoSameQualityMaxCount()
        {
            foreach (var drawType in QualityList)
            {
                Dictionary<int, DrawHeroQualityModel> heroDic = GetHeroQualityList(drawType.Key, DateTime.Now);
                if (heroDic != null)
                {
                    foreach (var heroQuality in heroDic)
                    {
                        if (drawType.Value.NoSameQuality > 0 && heroQuality.Key <= drawType.Value.NoSameQuality)
                        {
                            drawType.Value.MaxQualityCount = Math.Min(heroQuality.Value.HeroRatioLsit.Count, drawType.Value.MaxQualityCount);
                        }
                    }
                }
            }
        }

        //private static void InitConstellationList()
        //{
        //    Dictionary<int, ConstellationModel> constellationList = new Dictionary<int, ConstellationModel>();
        //    ConstellationModel info;
        //    DrawStarModel star;
        //    DataList dataList = DataListManager.inst.GetDataList("Constellation");
        //    foreach (var item in dataList)
        //    {
        //        Data data = item.Value;
        //        int type = data.GetInt("Type");

        //        if (constellationList.TryGetValue(type, out info))
        //        {
        //            star = new DrawStarModel(data);
        //            info.Add(star);
        //        }
        //        else
        //        {
        //            info = new ConstellationModel();
        //            star = new DrawStarModel(data);
        //            info.Add(star);
        //            constellationList.Add(type, info);
        //        }
        //    }

        //    DrawLibrary.constellationList = constellationList;
        //}

        private static void InitDrawRatioChange()
        {
            Dictionary<int, List<DrawRatioChangeModel>> ratioChangeList = new Dictionary<int, List<DrawRatioChangeModel>>();
            DrawRatioChangeModel info;
            List<DrawRatioChangeModel> list;
            DataList dataList = DataListManager.inst.GetDataList("DrawRatioChange");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                info = new DrawRatioChangeModel(item.Value);

                if (ratioChangeList.TryGetValue(info.Type, out list))
                {
                    list.Add(info);
                }
                else
                {
                    list = new List<DrawRatioChangeModel>();
                    list.Add(info);
                    ratioChangeList.Add(info.Type, list);
                }

                if (!heroTypeIds.Contains(info.ChangeId))
                {
                    heroTypeIds.Add(info.ChangeId);
                }
            }

            DrawLibrary.ratioChangeList = ratioChangeList;
        }

        private static void InitDrawTime(DateTime openServer)
        {
            Dictionary<int, List<DrawTimeModel>> drawTimeList = new Dictionary<int, List<DrawTimeModel>>();
            DrawTimeModel info;
            List<DrawTimeModel> list;
            DataList dataList = DataListManager.inst.GetDataList("DrawTime");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                info = new DrawTimeModel(item.Value, openServer);

                if (drawTimeList.TryGetValue(info.Type, out list))
                {
                    list.Add(info);
                }
                else
                {
                    list = new List<DrawTimeModel>();
                    list.Add(info);
                    drawTimeList.Add(info.Type, list);
                }
            }

            DrawLibrary.drawTimeList = drawTimeList;
        }

        private static void InitHeroComboList()
        {
            Dictionary<int, HeroComboModel> heroComboList = new Dictionary<int, HeroComboModel>();
            Dictionary<int, List<int>> ComboGroupList = new Dictionary<int, List<int>>();
            List<int> list;
            HeroComboModel model;
            DataList dataList = DataListManager.inst.GetDataList("HeroCombo");
            foreach (var item in dataList)
            {
                if (!heroComboList.ContainsKey(item.Value.ID))
                {
                    model = new HeroComboModel(item.Value);
                    heroComboList.Add(item.Value.ID, model);

                    if (ComboGroupList.TryGetValue(model.Group, out list))
                    {
                        list.Add(model.Id);
                    }
                    else
                    {
                        list = new List<int>();
                        list.Add(model.Id);
                        ComboGroupList.Add(model.Group, list);
                    }
                }
            }

            DrawLibrary.heroComboList = heroComboList;
            DrawLibrary.ComboGroupList = ComboGroupList;
        }

        public static void InitHeroRatioList(DateTime openServer, DateTime now)
        {
            int addDay = (int)openServer.DayOfWeek;
            if (openServer.DayOfWeek == DayOfWeek.Sunday)
            {
                addDay = 7;
            }

            Dictionary<int, Dictionary<int, DrawHeroQualityModel>> heroRatioList = new Dictionary<int, Dictionary<int, DrawHeroQualityModel>>();
            DataList dataList = DataListManager.inst.GetDataList("DrawHeroRatio");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                int heroId = data.ID;
                int openWeek = data.GetInt("OpenWeek");
                int openDay = data.GetInt("OpenDay");
                if (openWeek > 0 || openDay > 0)
                {
                    DateTime openTime = openServer.Date.AddDays(openWeek * 7 + openDay - addDay);
                    if (now >= openTime)
                    {
                        foreach (var id in heroTypeIds)
                        {
                            AddHeroRatio(heroRatioList, data, heroId, id);
                        }
                    }
                }
                else
                {
                    foreach (var id in heroTypeIds)
                    {
                        AddHeroRatio(heroRatioList, data, heroId, id);
                    }
                }
            }

            DrawLibrary.heroRatioList = heroRatioList;
        }

        private static void AddHeroRatio(Dictionary<int, Dictionary<int, DrawHeroQualityModel>> heroRatioList, Data data, int heroId, int i)
        {
            Dictionary<int, DrawHeroQualityModel> list;
            DrawHeroQualityModel info;
            int quality = data.GetInt("Quality" + i);
            int ratio = data.GetInt("Ratio" + i);
            if (ratio > 0)
            {
                if (heroRatioList.TryGetValue(i, out list))
                {
                    if (list.TryGetValue(quality, out info))
                    {
                        info.Add(heroId, ratio);
                    }
                    else
                    {
                        info = new DrawHeroQualityModel();
                        info.Quality = quality;
                        info.Add(heroId, ratio);
                        list.Add(info.Quality, info);
                    }
                }
                else
                {
                    list = new Dictionary<int, DrawHeroQualityModel>();
                    info = new DrawHeroQualityModel();
                    info.Quality = quality;
                    info.Add(heroId, ratio);
                    list.Add(info.Quality, info);
                    heroRatioList.Add(i, list);
                }
            }
        }

        private static void InitQualityList()
        {
            Dictionary<int, DrawQualityModel> QualityList = new Dictionary<int, DrawQualityModel>();
            DataList dataList = DataListManager.inst.GetDataList("DrawQualityRatio");
            foreach (var item in dataList)
            {
                QualityList.Add(item.Value.ID, new DrawQualityModel(item.Value));

                if (!heroTypeIds.Contains(item.Value.ID))
                {
                    heroTypeIds.Add(item.Value.ID);
                }
            }

            DrawLibrary.QualityList = QualityList;
        }

        private static void InitDrawHeroReward()
        {
            Dictionary<int, string> drawRewardList = new Dictionary<int, string>();
            DataList dataList = DataListManager.inst.GetDataList("DrawHeroReward");
            foreach (var item in dataList)
            {
                string reward = item.Value.GetString("Reward");
                if (!string.IsNullOrEmpty(reward))
                {
                    if (!drawRewardList.ContainsKey(item.Value.ID))
                    {
                        drawRewardList.Add(item.Value.ID, reward);
                    }
                }
            }
            DrawLibrary.drawRewardList = drawRewardList;
        }


        public static string GetDrawHeroReward(int type)
        {
            string reward = null;
            drawRewardList.TryGetValue(type, out reward);
            return reward;
        }
        public static HeroComboModel GetHeroComboModel(int id)
        {
            HeroComboModel model = null;
            heroComboList.TryGetValue(id, out model);
            return model;
        }

        public static List<int> GetComboGroupList(int group)
        {
            List<int> list = null;
            ComboGroupList.TryGetValue(group, out list);
            return list;
        }

        public static DrawQualityModel GetQualityModel(int id)
        {
            DrawQualityModel model = null;
            QualityList.TryGetValue(id, out model);
            return model;
        }

        public static Dictionary<int, DrawHeroQualityModel> GetHeroQualityList(int id, DateTime now)
        {
            Dictionary<int, DrawHeroQualityModel> list = null;
            List<DrawRatioChangeModel> changelist;
            if (ratioChangeList.TryGetValue(id, out changelist))
            {
                foreach (var changeItem in changelist)
                {
                    if (changeItem.CheckOpen(now))
                    {
                        //说明在
                        heroRatioList.TryGetValue(changeItem.ChangeId, out list);
                        return list;
                    }
                }
                heroRatioList.TryGetValue(id, out list);
            }
            else
            {
                heroRatioList.TryGetValue(id, out list);
            }
            return list;
        }

        private static List<DrawTimeModel> GetDrawTimeList(int type)
        {
            List<DrawTimeModel> list = null;
            drawTimeList.TryGetValue(type, out list);
            return list;
        }

        private static DrawTimeModel GetDrawTime(List<DrawTimeModel> list, DateTime now, bool ignoreNewServerActivity)
        {
            DrawTimeModel drawTime = null;
            foreach (var time in list)
            {
                if (time.CheckOpen(now, ignoreNewServerActivity))
                {
                    drawTime = time;
                    break;
                }
            }
            return drawTime;
        }

        public static DrawTimeModel GetDrawTime(int type, DateTime now, bool ignoreNewServerActivity)
        {
            List<DrawTimeModel> list = GetDrawTimeList(type);
            if (list != null)
            {
                return GetDrawTime(list, now, ignoreNewServerActivity);
            }
            return null;
        }


        public static ConstellationModel GetStarList(int type)
        {
            ConstellationModel list = null;
            constellationList.TryGetValue(type, out list);
            return list;
        }

        //public static DrawStarModel GetDrawStar(ConstellationModel list, int id)
        //{
        //    DrawStarModel info = null;
        //    list.StarList.TryGetValue(id, out info);
        //    return info;
        //}
    }
}
