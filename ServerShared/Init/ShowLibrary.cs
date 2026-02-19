using CommonUtility;
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
    public class ShowLibrary
    {
        private static Dictionary<int, CareerInfo> CareerList = new Dictionary<int, CareerInfo>();
        public static void BindData()
        {
            //CareerList.Clear();

            BindCareerData();
        }

        private static void BindCareerData()
        {
            Dictionary<int, CareerInfo> CareerList = new Dictionary<int, CareerInfo>();

            DataList gameConfig = DataListManager.inst.GetDataList("Career");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                AddCareerInfo(CareerList, data);
            }

            ShowLibrary.CareerList = CareerList;
        }

        private static void AddCareerInfo(Dictionary<int, CareerInfo> CareerList, Data data)
        {
            int careerId = data.GetInt("chapter");

            string[] contents = StringSplit.GetArray("|", data.GetString("content"));
            string[] stats = StringSplit.GetArrayContainNune("|", data.GetString("stat"));
            if (contents.Length != stats.Length)
            {
                Logger.Log.Error("ShowLibrary AddCareerInfo error : contents {0} stats {1}", contents.Length, stats.Length);
                return;
            }

            CareerInfo career;
            if (!CareerList.TryGetValue(careerId, out career))
            {
                career = new CareerInfo();
                career.CareerId = careerId;
                CareerList.Add(careerId, career);
            }

            for (int i = 0; i < contents.Length; i++)
            {
                int contentId = int.Parse(contents[i]);
                string staString = stats[i];

                ContentInfo content;
                if (!career.ContentList.TryGetValue(contentId, out content))
                {
                    content = new ContentInfo();
                    content.ContentId = contentId;
                    career.ContentList.Add(contentId, content);
                }

                string[] types = StringSplit.GetArray(":", staString);
                foreach (var type in types)
                {
                    HashField_StatData stat;
                    if (Enum.TryParse(type, out stat))
                    {
                        content.StatDatas.Add(stat);
                    }
                    else
                    {
                        Logger.Log.Error("ShowLibrary AddCareerInfo not find type {0}", type);
                    }
                }
            }
        }

        //public static CareerInfo GetCareerInfo(int carerrId)
        //{
        //    CareerInfo career;
        //    CareerList.TryGetValue(carerrId, out career);
        //    return career;
        //}
    }
}
