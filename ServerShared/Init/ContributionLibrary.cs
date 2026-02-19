using DataProperty;
using ServerModels.Contribution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared
{
    public class ContributionLibrary
    {

        private static Dictionary<int, ContributionModel> contributionList = new Dictionary<int, ContributionModel>();
        private static Dictionary<int, ContributionRewardModel> rewardList = new Dictionary<int, ContributionRewardModel>();

        public static void Init()
        {
            InitContribution();
            InitReward();
        }

        private static void InitContribution()
        {
            Dictionary<int, ContributionModel> contributionList = new Dictionary<int, ContributionModel>();
            //contributionList.Clear();

            ContributionModel model;
            DataList dataList = DataListManager.inst.GetDataList("ServerContribution");
            foreach (var kv in dataList)
            {
                model = new ContributionModel(kv.Value);
                contributionList.Add(model.Id, model);
            }
            ContributionLibrary.contributionList = contributionList;
        }

        private static void InitReward()
        {
            Dictionary<int, ContributionRewardModel> rewardList = new Dictionary<int, ContributionRewardModel>();
            //rewardList.Clear();
            ContributionRewardModel model;
            DataList dataList = DataListManager.inst.GetDataList("ServerContributionReward");
            foreach (var kv in dataList)
            {
                model = new ContributionRewardModel(kv.Value);
                rewardList.Add(model.Id, model);
            }
            ContributionLibrary.rewardList = rewardList;
        }

      
        public static ContributionModel GetContributionModel(int id)
        {
            ContributionModel model;
            contributionList.TryGetValue(id, out model);
            return model;
        }

        public static string GetContributionReward(int id)
        {
            ContributionRewardModel model;
            if (rewardList.TryGetValue(id, out model))
            {
                return model.Reward;
            }
            return null;
        }
    }
}
