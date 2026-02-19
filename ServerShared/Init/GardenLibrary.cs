using DataProperty;
using ServerModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Configuration;

namespace ServerShared
{
    public class GardenLibrary
    {
        //<period,<id, info>>
        private static DoubleDepthMap<int, int, GardenSeedModel> seedList = new DoubleDepthMap<int, int, GardenSeedModel>();
        private static Dictionary<int, GardenScoreReward> scoreReward = new Dictionary<int, GardenScoreReward>();
        private static DoubleDepthMap<int, int, int> nextRewardId = new DoubleDepthMap<int, int, int>();

        private static DoubleDepthMap<int, int, GardenExchangeShopModel> exchangeShopModels = new DoubleDepthMap<int, int, GardenExchangeShopModel>();

        public static int PitCount { get; private set; }
        public static float HarverstFactor { get; private set; }

        public static void Init()
        {
            InitConfg();
            InitSeed();
            InitScoreReward();
            InitExchangeShop();
        }

        private static void InitConfg()
        {
            Data data = DataListManager.inst.GetData("GardenConfig", 1);
            PitCount = data.GetInt("PitCount");
            HarverstFactor = data.GetFloat("HarverstFactor");
        }

        private static void InitSeed()
        {
            DoubleDepthMap<int, int, GardenSeedModel> seedList = new DoubleDepthMap<int, int, GardenSeedModel>();
            DataList dataList = DataListManager.inst.GetDataList("GardenSeed");
            foreach (var kv in dataList)
            {
                GardenSeedModel model = new GardenSeedModel(kv.Value);
                seedList.Add(model.Period, model.SeedId, model);
            }
            GardenLibrary.seedList = seedList;
        }

        private static void InitScoreReward()
        {
            DoubleDepthMap<int, int, int> nextRewardId = new DoubleDepthMap<int, int, int>();
            Dictionary<int, GardenScoreReward> scoreReward = new Dictionary<int, GardenScoreReward>();

            DataList dataList = DataListManager.inst.GetDataList("GardenScoreReward");
            int lastId = 0, lastPeriod = 0;
            foreach (var kv in dataList)
            {
                GardenScoreReward model = new GardenScoreReward(kv.Value);
                if (lastPeriod != model.Period)
                {
                    lastId = 0;
                    lastPeriod = model.Period;
                }

                nextRewardId.Add(model.Period, lastId, kv.Key);
                scoreReward.Add(model.Id, model);

                lastId = kv.Key;
            }

            GardenLibrary.scoreReward = scoreReward;
            GardenLibrary.nextRewardId = nextRewardId;
        }

        private static void InitExchangeShop()
        {
            DoubleDepthMap<int, int, GardenExchangeShopModel> exchangeShopModels = new DoubleDepthMap<int, int, GardenExchangeShopModel>();
            DataList dataList = DataListManager.inst.GetDataList("GardenShop");
            foreach (var kv in dataList)
            {
                GardenExchangeShopModel model = new GardenExchangeShopModel(kv.Value);
                exchangeShopModels.Add(model.Period, model.Id, model);
            }
            GardenLibrary.exchangeShopModels = exchangeShopModels;
        }

        public static GardenSeedModel GetGardenSeedModel(int period, int seedId)
        {
            GardenSeedModel model;
            if(seedList.TryGetValue(period, seedId, out model))
            {
                return model;
            }
            return null;
        }

        public static GardenScoreReward GetNextReward(int period, int Id)
        {
            int nextId;
            if (nextRewardId.TryGetValue(period, Id,out nextId))
            {
                GardenScoreReward model;
                if (scoreReward.TryGetValue(nextId, out model))
                {
                    return model;
                }
            }
            return null;
        }

        public static GardenExchangeShopModel GetGardenExchangeShopModel(int period, int id)
        {
            GardenExchangeShopModel model;
            if (exchangeShopModels.TryGetValue(period, id, out model))
            {
                return model;
            }

            return null;
        }
    }
}
