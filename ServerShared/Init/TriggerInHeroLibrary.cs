using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class TriggerInHeroLibrary
    {
        private static Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();
        public static int RealBodyEnergyTriggerId = 101;
        public static void Init()
        {
            Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();
            //triggerList.Clear();
            DataList dataList = DataListManager.inst.GetDataList("TriggerInHero");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!triggerList.ContainsKey(item.Key))
                {
                    triggerList.Add(item.Key, new TriggerModel(data));
                }
            }
            TriggerInHeroLibrary.triggerList = triggerList;
        }

        public static TriggerModel GetModel(int id)
        {
            TriggerModel model = null;
            triggerList.TryGetValue(id, out model);
            return model;
        }
    }
}
