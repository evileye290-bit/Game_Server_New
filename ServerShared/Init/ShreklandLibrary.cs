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
    public class ShreklandLibrary
    {
        private static Dictionary<int, ScoreRewardModel> scoreRewardDic = new Dictionary<int, ScoreRewardModel>();
        private static Dictionary<int, List<ShreklandRandReward>> randomRewardList = new Dictionary<int, List<ShreklandRandReward>>();
        private static Dictionary<int, ShreklandRandReward> randomRewardDic = new Dictionary<int, ShreklandRandReward>();

        private static Dictionary<int, int> rewardLevelScore = new Dictionary<int, int>();
        private static Dictionary<int, int> rewardLevelRewardType = new Dictionary<int, int>();

        public static int NormalItemCost;
        public static int RefreshCost;
        public static int ControlItemId;
        public static int DoubleItemId;
        public static int TrebleItemId;
        public static int StepNumMin;
        public static int StepNumMax;
        public static int MaxRewardLevel;
        public static int GridNum;
        public static int MaxRewardType;

        public static void Init()
        {
            InitShreklandConfig();
            InitShreklandScoreReward();
            InitShreklandRandReward();
            InitShreklandRewardLevel();
        }

        private static void InitShreklandConfig()
        {
            Data data = DataListManager.inst.GetData("ShreklandConfig", 1);
            NormalItemCost = data.GetInt("NormalItemCost");
            RefreshCost = data.GetInt("RefreshCost");
            ControlItemId = data.GetInt("ControlItem");
            DoubleItemId = data.GetInt("DoubleItem");
            TrebleItemId = data.GetInt("TrebleItem");
            string[] stepNum = StringSplit.GetArray(":", data.GetString("StepNum"));
            if (stepNum.Length != 2)
            {
                Logger.Log.Warn($"init ShreklandConfig xml stepNum param error");
            }
            StepNumMin = int.Parse(stepNum[0]);
            StepNumMax = int.Parse(stepNum[1]);
            MaxRewardLevel = data.GetInt("MaxRewardLevel");
            GridNum = data.GetInt("GridNum");
            MaxRewardType = data.GetInt("MaxRewardType");
        }

        private static void InitShreklandScoreReward()
        {
            Dictionary<int, ScoreRewardModel> scoreRewardDic = new Dictionary<int, ScoreRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("ShreklandScoreReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                ScoreRewardModel rewardModel = new ScoreRewardModel(data);
                scoreRewardDic.Add(rewardModel.Id, rewardModel);
            }

            ShreklandLibrary.scoreRewardDic = scoreRewardDic;
        }

        private static void InitShreklandRandReward()
        {
            Dictionary<int, List<ShreklandRandReward>> randomRewardList = new Dictionary<int, List<ShreklandRandReward>>();
            Dictionary<int, ShreklandRandReward> randomRewardDic = new Dictionary<int, ShreklandRandReward>();

            DataList dataList = DataListManager.inst.GetDataList("ShreklandRandReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                ShreklandRandReward rewardModel = new ShreklandRandReward(data);
                List<ShreklandRandReward> list;
                if (!randomRewardList.TryGetValue(rewardModel.RewardType, out list))
                {
                    list = new List<ShreklandRandReward>() { rewardModel };
                    randomRewardList.Add(rewardModel.RewardType, list);
                }
                list.Add(rewardModel);
                randomRewardDic.Add(rewardModel.Id, rewardModel);
            }

            ShreklandLibrary.randomRewardList = randomRewardList;
            ShreklandLibrary.randomRewardDic = randomRewardDic;
        }

        private static void InitShreklandRewardLevel()
        {
            Dictionary<int, int> rewardLevelScore = new Dictionary<int, int>();
            Dictionary<int, int> rewardLevelRewardType = new Dictionary<int, int>();

            DataList dataList = DataListManager.inst.GetDataList("ShreklandRewardLevel");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                rewardLevelScore.Add(data.ID, data.GetInt("OnceScore"));
                rewardLevelRewardType.Add(data.ID, data.GetInt("RewardType"));
            }

            ShreklandLibrary.rewardLevelScore = rewardLevelScore;
            ShreklandLibrary.rewardLevelRewardType = rewardLevelRewardType;
        }

        public static ScoreRewardModel GetScoreRewardModel(int rewardId)
        {
            ScoreRewardModel model;
            scoreRewardDic.TryGetValue(rewardId, out model);
            return model;
        }

        public static List<ShreklandRandReward> GetRandomRewardList(int rewardType)
        {
            List<ShreklandRandReward> list;
            randomRewardList.TryGetValue(rewardType, out list);
            return list;
        }

        public static ShreklandRandReward GetRandomReward(int rewardId)
        {
            ShreklandRandReward reward;
            randomRewardDic.TryGetValue(rewardId, out reward);
            return reward;
        }

        public static int GetOnceAddScoreByRewardLevel(int rewardLevel)
        {
            int score;
            rewardLevelScore.TryGetValue(rewardLevel, out score);
            return score;
        }

        public static int GetRewardTypeByRewardLevel(int rewardLevel)
        {
            int rewardType;
            rewardLevelRewardType.TryGetValue(rewardLevel, out rewardType);
            return rewardType;
        }
    }
}
