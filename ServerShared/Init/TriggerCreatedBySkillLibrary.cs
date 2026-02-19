using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class TriggerCreatedBySkillLibrary
    {
        private static Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();

        public static void Init()
        {
            //triggerList.Clear();
            Dictionary<int, TriggerModel> triggerList = new Dictionary<int, TriggerModel>();

            DataList dataList = DataListManager.inst.GetDataList("TriggerCreatedBySkill");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!triggerList.ContainsKey(item.Key))
                {
                    triggerList.Add(item.Key, new TriggerModel(data));
                }
            }
            TriggerCreatedBySkillLibrary.triggerList = triggerList;
        }

        public static TriggerModel GetModel(int id)
        {
            TriggerModel model = null;
            triggerList.TryGetValue(id, out model);
            return model;
        }
    }
}
