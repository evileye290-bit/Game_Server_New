using CommonUtility;
using DataProperty;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class MarkLibrary
    {
        private static Dictionary<int, MarkModel> markList = new Dictionary<int, MarkModel>();
        public static void Init()
        {
            Dictionary<int, MarkModel> markList = new Dictionary<int, MarkModel>();
            //markList.Clear();
            DataList dataList = DataListManager.inst.GetDataList("Mark");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!markList.ContainsKey(item.Key))
                {
                    markList.Add(item.Key, new MarkModel(data));
                }
            }
            MarkLibrary.markList = markList;
        }

        public static MarkModel GetMarkModel(int id)
        {
            MarkModel model = null;
            markList.TryGetValue(id, out model);
            return model;
        }

    }
}
