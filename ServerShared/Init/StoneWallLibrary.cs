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
    public class StoneWallLibrary
    {
        private static Dictionary<int, StoneWallConfig> stoneWallConfigs = new Dictionary<int, StoneWallConfig>();
        private static Dictionary<int, StoneWallRewardModel> stoneWallRewards = new Dictionary<int, StoneWallRewardModel>();
        private static Dictionary<int, Dictionary<int, StoneWallUnlockRewardModel>> stoneWallUnlockRewards = new Dictionary<int, Dictionary<int, StoneWallUnlockRewardModel>>();     
        public static int MaxLine { get; private set; }
        public static int MaxColumn { get; private set; }

        public static void Init()
        {
            InitStoneWallConfig();
            InitStoneWallRewards();
            InitStoneWallUnlockRewards();
            InitStoneWallLimit();
        }

        private static void InitStoneWallConfig()
        {
            Dictionary<int, StoneWallConfig> stoneWallConfigs = new Dictionary<int, StoneWallConfig>();

            DataList dataList = DataListManager.inst.GetDataList("StoneWall");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                StoneWallConfig info = new StoneWallConfig(data);
                stoneWallConfigs.Add(info.Type, info);
            }
            StoneWallLibrary.stoneWallConfigs = stoneWallConfigs;
        }

        private static void InitStoneWallRewards()
        {
            Dictionary<int, StoneWallRewardModel> stoneWallRewards = new Dictionary<int, StoneWallRewardModel>();

            DataList dataList = DataListManager.inst.GetDataList("StoneWallReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                StoneWallRewardModel info = new StoneWallRewardModel(data);
                stoneWallRewards.Add(info.Id, info);
            }
            StoneWallLibrary.stoneWallRewards = stoneWallRewards;
        }

        private static void InitStoneWallUnlockRewards()
        {
            Dictionary<int, Dictionary<int, StoneWallUnlockRewardModel>> stoneWallUnlockRewards = new Dictionary<int, Dictionary<int, StoneWallUnlockRewardModel>>();

            DataList dataList = DataListManager.inst.GetDataList("StoneWallUnlockReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                StoneWallUnlockRewardModel info = new StoneWallUnlockRewardModel(data);
                Dictionary<int, StoneWallUnlockRewardModel> dic;
                if (!stoneWallUnlockRewards.TryGetValue(info.Type, out dic))
                {
                    dic = new Dictionary<int, StoneWallUnlockRewardModel>();
                    stoneWallUnlockRewards.Add(info.Type, dic);
                }
                dic.Add(info.Id, info);
            }
            StoneWallLibrary.stoneWallUnlockRewards = stoneWallUnlockRewards;
        }

        private static void InitStoneWallLimit()
        {
            Data data = DataListManager.inst.GetData("StoneWallLimit", 1);
            MaxLine = data.GetInt("MaxLine");
            MaxColumn = data.GetInt("MaxColumn");
        }

        public static StoneWallConfig GetStoneWallConfig(int type)
        {
            StoneWallConfig info;
            stoneWallConfigs.TryGetValue(type, out info);
            return info;
        }
        
        public static StoneWallRewardModel GetStoneWallReward(int id)
        {
            StoneWallRewardModel reward;
            stoneWallRewards.TryGetValue(id, out reward);
            return reward;
        }

        public static List<StoneWallUnlockRewardModel> GetStoneWallUnlockReward(int type, int line, int lineUnlockNum, int column, int columnUnlockNum, int rewardCount)
        {
            List<StoneWallUnlockRewardModel> list = new List<StoneWallUnlockRewardModel>();
            Dictionary<int, StoneWallUnlockRewardModel> dic;
            stoneWallUnlockRewards.TryGetValue(type, out dic);
            if (dic == null)
            {
                return list;
            }
            foreach (var item in dic)
            {
                if (lineUnlockNum == MaxColumn && item.Value.Line == line + 1)
                {
                    list.Add(item.Value);
                }
                if (columnUnlockNum == MaxLine && item.Value.Column == column + 1)
                {
                    list.Add(item.Value);
                }
                if (rewardCount == MaxLine * MaxColumn && item.Value.FinalReward == 1)
                {
                    list.Add(item.Value);
                }
            }
            return list;
        }
    }
}
