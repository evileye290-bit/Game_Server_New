using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class WorldLevelLibrary
    {
        private static Dictionary<int, WorldLevelModel> worldLevelList = new Dictionary<int, WorldLevelModel>();
        private static Dictionary<int, int> days2Level = new Dictionary<int, int>();

        public static int ServerMaxLevel { get; private set; }
        public static int MaxChapter { get; private set; }
        public static int MaxDays { get; private set; }

        public static void Init()
        {
            InitWorldLevel();
        }

        public static void GetServerLevel(int serverOpenDays, ref int level, ref int day)
        {
            if (serverOpenDays >= MaxDays)
            {
                level = ServerMaxLevel;
            }
            foreach (var kv in days2Level)
            {
                if (serverOpenDays >= kv.Value)
                {
                    day = kv.Value;
                }
                else
                {
                    level = kv.Key;
                    break;
                }
            }
        }

        public static int GetChapterId(int serverLevel)
        {
            WorldLevelModel model;
            worldLevelList.TryGetValue(serverLevel, out model);
            return model == null ? 1 : model.Chapter;
        }


        private static void InitWorldLevel()
        {
            Dictionary<int, int> days2Level = new Dictionary<int, int>();
            Dictionary<int, WorldLevelModel> worldLevelList = new Dictionary<int, WorldLevelModel>();
            DataList dataList = DataListManager.inst.GetDataList("WorldLevel");
            foreach (var kv in dataList)
            {
                WorldLevelModel model = new WorldLevelModel();
                model.BindData(kv.Value);
                worldLevelList.Add(model.Id, model);
                days2Level.Add(model.Id, model.Day);

                MaxDays = model.Day;
                MaxChapter = model.Chapter > MaxChapter ? model.Chapter : MaxChapter;
                ServerMaxLevel = model.Id > ServerMaxLevel ? model.Id : ServerMaxLevel;
            }

            WorldLevelLibrary.days2Level = days2Level.OrderBy(kv => kv.Key).ToDictionary(kv=>kv.Key, kv=>kv.Value);
            WorldLevelLibrary.worldLevelList = worldLevelList;
        }

    }
}
