using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class TriggerCreatedBySoulBoneLibrary
    {
        private static Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();

        public static void Init()
        {
            //triggerList.Clear();
            Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();

            DataList dataList = DataListManager.inst.GetDataList("TriggerCreatedBySoulBone");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!triggerList.ContainsKey(item.Key))
                {
                    triggerList.Add(item.Key, new TriggerModel(data));
                }
            }
            TriggerCreatedBySoulBoneLibrary.triggerList = triggerList;
        }

        public static TriggerModel GetModel(int id)
        {
            TriggerModel model = null;
            triggerList.TryGetValue(id, out model);
            return model;
        }
    }
}
