using DataProperty;
using ServerModels.Monster;
using System.Collections.Generic;

namespace ServerShared
{
    public class MonsterGenLibrary
    {
        // key regenId
        private static Dictionary<int, MonsterGenModel> modelList = new Dictionary<int, MonsterGenModel>();
        // key mapId
        private static Dictionary<int, List<MonsterGenModel>> mapModelList = new Dictionary<int, List<MonsterGenModel>>();

        public static void Init()
        {
            //modelList.Clear();
            //mapModelList.Clear();
            Dictionary<int, MonsterGenModel> modelList = new Dictionary<int, MonsterGenModel>();
            Dictionary<int, List<MonsterGenModel>> mapModelList = new Dictionary<int, List<MonsterGenModel>>();

            DataList dataList = DataListManager.inst.GetDataList("ZoneMonster");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                MonsterGenModel model = new MonsterGenModel(data);
                modelList.Add(item.Key, model);

                List<MonsterGenModel> list = null;
                if (!mapModelList.TryGetValue(model.MapId, out list))
                {
                    list = new List<MonsterGenModel>();
                    mapModelList.Add(model.MapId, list);
                }
                list.Add(model);
            }
            MonsterGenLibrary.modelList = modelList;
            MonsterGenLibrary.mapModelList = mapModelList;
        }

        public static List<MonsterGenModel> GetModelsByMap(int mapId)
        {
            List<MonsterGenModel> list = null;
            mapModelList.TryGetValue(mapId, out list);
            return list;
        }

        public static MonsterGenModel GetModelById(int id)
        {
            MonsterGenModel model = null;
            modelList.TryGetValue(id, out model);
            return model;
        }
    }
}
