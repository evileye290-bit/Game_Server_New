using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class BuffLibrary
    {
        private static Dictionary<int, BuffModel> buffList = new Dictionary<int, BuffModel>();

        public static void Init()
        {
            InitBuff();
        }

        private static void InitBuff()
        {
            Dictionary<int, BuffModel> buffList = new Dictionary<int, BuffModel>();

            DataList dataList = DataListManager.inst.GetDataList("Buff");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!buffList.ContainsKey(item.Key))
                {
                    buffList.Add(item.Key, new BuffModel(data));
                }
            }
            BuffLibrary.buffList = buffList;
        }

        public static BuffModel GetBuffModel(int id)
        {
            BuffModel model = null;
            buffList.TryGetValue(id, out model);
            return model;
        }

    }
}
