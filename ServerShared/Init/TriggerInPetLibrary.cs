using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class TriggerInPetLibrary
    {
        private static Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();

        public static void Init()
        {
            Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();
            //triggerList.Clear();
            DataList dataList = DataListManager.inst.GetDataList("TriggerInPet");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!triggerList.ContainsKey(item.Key))
                {
                    triggerList.Add(item.Key, new TriggerModel(data));
                }
            }
            TriggerInPetLibrary.triggerList = triggerList;
        }

        public static TriggerModel GetModel(int id)
        {
            TriggerModel model = null;
            triggerList.TryGetValue(id, out model);
            return model;
        }
    }
}
