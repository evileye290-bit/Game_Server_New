using DataProperty;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class TitleLibrary
    {
        private static Dictionary<int, TitleInfo> titleInfos = new Dictionary<int, TitleInfo>();     

        public static HashSet<int> titleIds = new HashSet<int>();

        public static List<int> sortedPopScoreLevels = new List<int>();

        private static Dictionary<int, List<TitleInfo>> typeTitles = new Dictionary<int, List<TitleInfo>>();

        private static Dictionary<int, int> titleCardList = new Dictionary<int, int>();
        public static int EmailId;
        public static int SwornTitleId;

        public static void Init()
        {
            //titleInfos.Clear();         
            //sortedPopScoreLevels.Clear();
            //typeTitles.Clear();
           
            InitTitleInfos();
            InitTitleCard();
            InitTitleConfig();
        }

        private static void InitTitleInfos()
        {
            Dictionary<int, TitleInfo> titleInfos = new Dictionary<int, TitleInfo>();
            //titleIds.Clear();

            DataList titleDataList = DataListManager.inst.GetDataList("Title");
            foreach (var item in titleDataList)
            {
                Data data = item.Value;
                TitleInfo titleInfo = new TitleInfo(data);
                titleInfos.Add(titleInfo.Id, titleInfo);
                titleIds.Add(titleInfo.Id);
            }

            TitleLibrary.titleInfos = titleInfos;

            List<int> sortedPopScoreLevels = new List<int>();
            foreach (var title in titleInfos)
            {
                if (title.Value.ObtainCondition == (int)TitleObtainType.PopScore)
                {
                    sortedPopScoreLevels.Add(title.Value.ConditionNumber.ToInt());
                }
            }
            sortedPopScoreLevels.Sort();

            TitleLibrary.sortedPopScoreLevels = sortedPopScoreLevels;

            InitTypeTitles();
        }

        private static void InitTypeTitles()
        {
            Dictionary<int, List<TitleInfo>> typeTitles = new Dictionary<int, List<TitleInfo>>();

            foreach (var item in titleInfos)
            {
                if (!typeTitles.ContainsKey((int)item.Value.ObtainCondition))
                {
                    List<TitleInfo> titles = new List<TitleInfo>();
                    typeTitles.Add(item.Value.ObtainCondition, titles);
                }

                List<TitleInfo> tempTitles = null;
                typeTitles.TryGetValue(item.Value.ObtainCondition, out tempTitles);
                if (tempTitles != null)
                {
                    tempTitles.Add(item.Value);
                }
            }
            TitleLibrary.typeTitles = typeTitles;
        }

        private static void InitTitleCard()
        {
            //titleCardList.Clear();
            Dictionary<int, int> titleCardList = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("TitleCard");
            foreach (var item in dataList)
            {             
                Data data = item.Value;   
                int titleId = data.GetInt("TitleId");              
                titleCardList.Add(data.ID, titleId);              
            }
            TitleLibrary.titleCardList = titleCardList;
        }

        private static void InitTitleConfig()
        {
            Data data = DataListManager.inst.GetData("TitleConfig", 1);
            EmailId = data.GetInt("EmailId");
            SwornTitleId = data.GetInt("SwornTitleId");
        }
        #region api

        public static List<TitleInfo> GetLeftTitles(HashSet<int> ownTitles)
        {
            List<TitleInfo> leftTitles = new List<TitleInfo>();
            foreach (var item in titleInfos)
            {
                if (!ownTitles.Contains(item.Key))
                {
                    leftTitles.Add(item.Value);
                }
            }
            return leftTitles;
        }

        public static List<TitleInfo> GetTitleInType(TitleObtainType type)
        {
            List<TitleInfo> leftTitles = null;
            //foreach (var item in titleInfos)
            //{
            //    if (item.Value.ObtainCondition == (int)type)
            //    {
            //        leftTitles.Add(item.Value);
            //    }
            //}
            typeTitles.TryGetValue((int)type, out leftTitles);
            if (leftTitles == null)
            {
                return new List<TitleInfo>();
            }
            return leftTitles;
        }

        public static int CountTitleInfos()
        {
            return titleIds.Count;
        }

        public static bool NeedBroadCast(double highestScore, double historyScore)
        {
            bool need = false;
            for (int i = 0; i < sortedPopScoreLevels.Count - 1; i++)
            {
                if (highestScore >= sortedPopScoreLevels[i] && historyScore < sortedPopScoreLevels[i])
                {
                    need = true;
                }
            }

            return need;
        }

        public static int GetTitleIdByItemId(int itemId)
        {
            int titleId;
            titleCardList.TryGetValue(itemId, out titleId);
            return titleId;
        }

        public static TitleInfo GetTitleById(int id)
        {
            TitleInfo title;
            titleInfos.TryGetValue(id, out title);
            return title;
        }

        public static List<TitleInfo> GetTitleListByCondition(TitleObtainCondition condition)
        {
            List<TitleInfo> titleList;
            typeTitles.TryGetValue((int)condition, out titleList);
            return titleList;
        }
        #endregion
    }
}
