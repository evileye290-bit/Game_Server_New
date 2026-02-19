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
    public class ShovelTreasureLibrary
    {
        private static Dictionary<int, Dictionary<int, int>> puzzleList = new Dictionary<int, Dictionary<int, int>>();
        //小游戏关卡奖励
        private static Dictionary<int, Dictionary<int, TreasureCheckPointReward>> checkPointRewards = new Dictionary<int, Dictionary<int, TreasureCheckPointReward>>();
        //小游戏通关奖励
        private static Dictionary<int, Dictionary<int, ShovelRewardModel>> checkPointPassRewards = new Dictionary<int, Dictionary<int, ShovelRewardModel>>();
        private static Dictionary<int, ShovelRewardModel> checkPointPassRewardsById = new Dictionary<int, ShovelRewardModel>();
        private static Dictionary<int, int> zoneTreasureList = new Dictionary<int, int>();
        private static Dictionary<int, string> puzzleRewards = new Dictionary<int, string>();
        private static Dictionary<int, Dictionary<int, ShovelRewardModel>> shovelRewardsModelList = new Dictionary<int, Dictionary<int, ShovelRewardModel>>();
        private static Dictionary<int, string> shovelRewards = new Dictionary<int, string>();

        public static int PuzzleItemId;
        public static int PuzzleCost;
        public static int PuzzleFinishCount;
        public static int TreasureGameTime;
        public static int GameRewardMaxCount;
        public static int HighTrerasureMap;
        public static int GameReviveDiamond;
        public static int GameReviveDiamondDiff;

        public static void Init()
        {
            InitJjgsawPuzzle();
            InitCheckPointRewards();
            InitShovelTreasureRewards();
            InitZoneTreasure();
            InitShovelTreasureConfig();
            InitTreasurePuzzleRewards();
            InitCheckPointsPassRewards();
        }

        private static void InitJjgsawPuzzle()
        {
            Dictionary<int, Dictionary<int, int>> puzzleList = new Dictionary<int, Dictionary<int, int>>();
            //puzzleList.Clear();

            DataList dataList = DataListManager.inst.GetDataList("JjgsawPuzzlePieces");
            foreach (var item in dataList)
            {
                JjgsawPuzzleModel model = new JjgsawPuzzleModel(item.Value);
                Dictionary<int, int> list;
                if (puzzleList.TryGetValue(model.Type, out list))
                {
                    list.Add(model.Index, model.Extra);//
                }
                else
                {
                    list = new Dictionary<int, int>();
                    list.Add(model.Index, model.Extra);
                    puzzleList.Add(model.Type, list);
                }
            }
            ShovelTreasureLibrary.puzzleList = puzzleList;
        }

        private static void InitCheckPointRewards()
        {
            Dictionary<int, Dictionary<int, TreasureCheckPointReward>> checkPointRewards = new Dictionary<int, Dictionary<int, TreasureCheckPointReward>>();
            //checkPointRewards.Clear();

            DataList dataList = DataListManager.inst.GetDataList("ShovelTreasureGame");
            foreach (var item in dataList)
            {
                TreasureCheckPointReward model = new TreasureCheckPointReward(item.Value);
                Dictionary<int, TreasureCheckPointReward> tempDic;
                if (checkPointRewards.TryGetValue(model.Quality, out tempDic))
                {
                    tempDic.Add(model.Id, model);
                }
                else
                {
                    tempDic = new Dictionary<int, TreasureCheckPointReward>();
                    tempDic.Add(model.Id, model);
                    checkPointRewards.Add(model.Quality, tempDic);
                }
            }
            ShovelTreasureLibrary.checkPointRewards = checkPointRewards;
        }

        private static void InitCheckPointsPassRewards()
        {
            Dictionary<int, Dictionary<int, ShovelRewardModel>> checkPointPassRewards = new Dictionary<int, Dictionary<int, ShovelRewardModel>>();
            Dictionary<int, ShovelRewardModel> checkPointPassRewardsById = new Dictionary<int, ShovelRewardModel>();
            //checkPointPassRewards.Clear();
            //checkPointPassRewardsById.Clear();

            DataList dataList = DataListManager.inst.GetDataList("PassCheckPointRewards");
            int weight = 0;
            foreach (var item in dataList)
            {
                ShovelRewardModel model = new ShovelRewardModel(item.Value);
               
                Dictionary<int, ShovelRewardModel> tempDic;
                if (checkPointPassRewards.TryGetValue(model.Quality, out tempDic))
                {
                    //weight += model.Weight;
                    weight = tempDic.Keys.Max() + model.Weight;
                    tempDic.Add(weight, model);
                }
                else
                {
                    tempDic = new Dictionary<int, ShovelRewardModel>();
                    weight = model.Weight;
                    tempDic.Add(weight, model);
                    checkPointPassRewards.Add(model.Quality, tempDic);
                }
                checkPointPassRewardsById.Add(model.Id, model);
            }
            ShovelTreasureLibrary.checkPointPassRewards = checkPointPassRewards;
            ShovelTreasureLibrary.checkPointPassRewardsById = checkPointPassRewardsById;
        }

        private static void InitZoneTreasure()
        {
            Dictionary<int, int> zoneTreasureList = new Dictionary<int, int>();
            //zoneTreasureList.Clear();

            DataList dataList = DataListManager.inst.GetDataList("ZoneShovelTreasure");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                zoneTreasureList.Add(data.ID, data.GetInt("ZoneId"));
            }
            ShovelTreasureLibrary.zoneTreasureList = zoneTreasureList;
        }

        private static void InitShovelTreasureConfig()
        {
            Data data = DataListManager.inst.GetData("ShovelTreasureConfig", 1);
            PuzzleItemId = data.GetInt("PuzzleItemId");
            PuzzleCost = data.GetInt("PuzzleCost");
            PuzzleFinishCount = data.GetInt("PuzzleFinishCount");
            TreasureGameTime = data.GetInt("TreasureGameTime");
            GameRewardMaxCount = data.GetInt("GameRewardMaxCount");
            HighTrerasureMap = data.GetInt("HighTrerasureMap");
            GameReviveDiamond = data.GetInt("GameReviveDiamond");
            GameReviveDiamondDiff = data.GetInt("GameReviveDiamondDiff");
        }

        private static void InitShovelTreasureRewards()
        {
            Dictionary<int, Dictionary<int, ShovelRewardModel>> shovelRewardsModelList = new Dictionary<int, Dictionary<int, ShovelRewardModel>>();
            Dictionary<int, string> shovelRewards = new Dictionary<int, string>();
            //shovelRewardsModelList.Clear();
            //shovelRewards.Clear();

            DataList dataList = DataListManager.inst.GetDataList("ShovelRewards");
            int weight = 0;
            foreach (var item in dataList)
            {
                ShovelRewardModel model = new ShovelRewardModel(item.Value);
                
                Dictionary<int, ShovelRewardModel> tempDic;
                if (shovelRewardsModelList.TryGetValue(model.Quality, out tempDic))
                {
                    //weight += model.Weight;
                    weight = tempDic.Keys.Max() + model.Weight;
                    tempDic.Add(weight, model);
                }
                else
                {
                    tempDic = new Dictionary<int, ShovelRewardModel>();
                    weight = model.Weight;
                    tempDic.Add(weight, model);
                    shovelRewardsModelList.Add(model.Quality, tempDic);
                }
                shovelRewards.Add(model.Id, model.Rewards);
            }
            ShovelTreasureLibrary.shovelRewardsModelList = shovelRewardsModelList;
            ShovelTreasureLibrary.shovelRewards = shovelRewards;
        }

        private static void InitTreasurePuzzleRewards()
        {
            Dictionary<int, string> puzzleRewards = new Dictionary<int, string>();
            //puzzleRewards.Clear();

            DataList dataList = DataListManager.inst.GetDataList("TreasurePuzzleReward");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                string rewards = data.GetString("Reward");
                puzzleRewards.Add(data.ID, rewards);
            }
            ShovelTreasureLibrary.puzzleRewards = puzzleRewards;
        }

        public static string GetCheckPointBasicRewardsById(int id, int quality)
        {
            TreasureCheckPointReward model;
            Dictionary<int, TreasureCheckPointReward> rewards;
            checkPointRewards.TryGetValue(quality, out rewards);
            if (rewards == null)
            {
                return string.Empty;
            }
            rewards.TryGetValue(id, out model);
            if (model != null)
            {
                return model.BasicRewards;
            }
            return string.Empty;
        }

        public static string GetCheckPointPassRewardsById(int id)
        {
            ShovelRewardModel model;
            checkPointPassRewardsById.TryGetValue(id, out model);
            if (model != null)
            {
                return model.Rewards;
            }
            return string.Empty;
        }

        public static List<int> GetRandomShovelRewardsList(int quality)
        {
            List<int> randList = new List<int>();

            Dictionary<int, ShovelRewardModel> rewards;
            shovelRewardsModelList.TryGetValue(quality, out rewards);
            if (rewards == null)
            {
                return randList;
            }
            Dictionary<int, ShovelRewardModel> descRewards = rewards.OrderByDescending(kv =>kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

            for (int i = 0; i < GameRewardMaxCount; i++)
            {
                int randNum = NewRAND.Next(1, descRewards.Keys.Max());
                int result = 0;
                foreach (var kv in descRewards)
                {
                    if (randNum >= kv.Key)
                    {
                        break;
                    }
                    result = kv.Value.Id;
                }
                randList.Add(result);
            }
            return randList;
        }

        public static string GetShovelRewardsById(int id)
        {
            string rewards = string.Empty;
            shovelRewards.TryGetValue(id, out rewards);
            return rewards;
        }

       public static int RandomPuzzleType(int curPuzzleType, Dictionary<int, List<int>> lightedList, bool needRefresh)
        {
            List<int> randPool = new List<int>();
            randPool.AddRange(puzzleList.Keys);
            Random rand = new Random();
            int result = 0;

            if (curPuzzleType == 0)
            {
                result = rand.Next(0, randPool.Count);
                return randPool[result];
            }
            List<int> list;
            lightedList.TryGetValue(curPuzzleType, out list);
            if (list == null)
            {
                return curPuzzleType;
            }
            if (list.Count == 0 && needRefresh)
            {
                result = rand.Next(0, randPool.Count);
                return randPool[result];
            }
            if (list.Count < PuzzleFinishCount)
            {
                return curPuzzleType;
            }

            result = rand.Next(0, randPool.Count);
            return randPool[result];
        }


        public static int RandomTreasureId()
        {
            List<int> randPool = new List<int>();
            randPool.AddRange(zoneTreasureList.Keys);
            Random rand = new Random();
            int result = rand.Next(0, randPool.Count);
            return randPool[result];
        }

        public static Dictionary<int, int> GetPuzzlePiecesList(int type)
        {
            Dictionary<int, int> list;
            puzzleList.TryGetValue(type, out list);
            return list;
        }

        public static string GetTreasurePuzzleReward(int id)
        {
            string reward;
            puzzleRewards.TryGetValue(id, out reward);
            return reward;
        }

        public static int GetRandomPassRewards(int quality)
        {
            Dictionary<int, ShovelRewardModel> passRewards;
            checkPointPassRewards.TryGetValue(quality, out passRewards);
            if (passRewards == null)
            {
                return 0;
            }
            Dictionary<int, ShovelRewardModel> descRewards = passRewards.OrderByDescending(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
            int randNum = NewRAND.Next(1, descRewards.Keys.Max());
            int result = 0;
            foreach (var kv in descRewards)
            {
                result = kv.Value.Id;
                if (randNum >= kv.Key)
                {
                    break;
                }
            }
            return result;
        }

        public static bool CheckPointMatchTreasureMapQuality(int checkPointId, int quality)
        {
            Dictionary<int, TreasureCheckPointReward> rewards;
            checkPointRewards.TryGetValue(quality, out rewards);
            if (rewards != null && rewards.ContainsKey(checkPointId))
            {
                return true;
            }
            return false;
        }

        //public static string GetPassRewards(int id)
        //{
        //    ShovelRewardModel model;
        //    checkPointPassRewards.TryGetValue(id, out model);
        //    return model.Rewards;
        //}
    }
}
