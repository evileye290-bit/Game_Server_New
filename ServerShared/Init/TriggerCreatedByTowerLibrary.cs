using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class TriggerCreatedByTowerLibrary
    {
        private static Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();

        public static void Init()
        {
            //triggerList.Clear();
            Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();

            DataList dataList = DataListManager.inst.GetDataList("TriggerCreatedByTower");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!triggerList.ContainsKey(item.Key))
                {
                    triggerList.Add(item.Key, new TriggerModel(data));
                }
            }
            TriggerCreatedByTowerLibrary.triggerList = triggerList;
        }

        public static TriggerModel GetModel(int id)
        {
            TriggerModel model = null;
            triggerList.TryGetValue(id, out model);
            return model;
        }
    }
}
