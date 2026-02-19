using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class HelpRewardLibrary
    {
        private static Dictionary<int, HelpRewardModel> helpRewardList = new Dictionary<int, HelpRewardModel>();

        public static void Init()
        {
            Dictionary<int, HelpRewardModel> helpRewardList = new Dictionary<int, HelpRewardModel>();
            //helpRewardList.Clear();
            Data data;
            HelpRewardModel model = null;
            DataList dataList = DataListManager.inst.GetDataList("HelpReward");
            foreach (var item in dataList)
            {
                data = item.Value;
                model = new HelpRewardModel(data);
                helpRewardList.Add(item.Key, model);
            }
            HelpRewardLibrary.helpRewardList = helpRewardList;
        }

        public static HelpRewardModel Get(int id)
        {
            HelpRewardModel model = null;
            helpRewardList.TryGetValue(id, out model);
            return model;
        }

        public static HelpRewardModel GetHelpReward(int level)
        {
            return helpRewardList.Values.Where(x => x.MinLevel <= level && x.MaxLevel >= level).FirstOrDefault();
        }
    }
}
