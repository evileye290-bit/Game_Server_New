using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerFrame;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public GardenManager GardenManager { get; private set; }
        public int GardenPeriod => GardenManager.Period;

        private void InitGardenManager()
        {
            GardenManager = new GardenManager(this);
        }

        public void GetGardenByLoading()
        {
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.Garden, ZoneServerApi.now))
            {
                GetGardenInfo();
            }
        }

        public void GetGardenInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_RANK_FIRST_INFO msg = new MSG_ZR_GET_RANK_FIRST_INFO() {RankType = (int) RankType.Garden};
            server.SendToRelation(msg, Uid);
        }

        public void PlantedSeed(int pit, int seedId)
        {
            MSG_ZGC_GARDEN_PLANTED_SEED msg = new MSG_ZGC_GARDEN_PLANTED_SEED();

            if (!GardenManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} PlantedSeed failed: time is error");
                msg.Result = (int) ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            GardenSeedModel model = GardenLibrary.GetGardenSeedModel(GardenPeriod, seedId);
            if (model == null)
            {
                Log.Warn($"player {Uid} PlantedSeed failed: have no this seed  info {seedId}");
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (pit <= 0 || pit > GardenLibrary.PitCount)
            {
                Log.Warn($"player {Uid} PlantedSeed failed: have no this pit  info {pit}");
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            BaseItem item = bagManager.NormalBag.GetItem(seedId);
            if (item == null || item.PileNum <= 0)
            {
                Log.Warn($"player {Uid} PlantedSeed failed: item {seedId} not enough ");
                msg.Result = (int) ErrorCode.NoSuchItem;
                Write(msg);
                return;
            }

            if (GardenManager.GardenInfo.SeedEndTime.ContainsKey(pit))
            {
                Log.Warn($"player {Uid} PlantedSeed failed: pit {pit} had planted");
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            SeedInfo seed = GardenManager.PlantSeed(pit, seedId, model.HarvestTime);
            if (seed == null)
            {
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            DelItem2Bag(item, RewardType.NormalItem, 1, ConsumeWay.Garden);
            SyncClientItemInfo(item);

            msg.Result = (int) ErrorCode.Success;
            msg.Pit = seed.Pit;
            msg.SeedId = seed.SeedId;
            msg.FinishTime = seed.EndTime;
            Write(msg);
        }

        public void GetGardenReward(int type, int pit, bool useDiamond)
        {

            if (type == 1)
            {
                GetGardenScoreReward();
            }
            else
            {
                GetSeedReward(pit, useDiamond);
            }
        }

        private void GetSeedReward(int pit, bool useDiamond)
        {
            MSG_ZGC_GARDEN_REAWARD msg = new MSG_ZGC_GARDEN_REAWARD();

            SeedInfo seedInfo = GardenManager.GetSeedInfo(pit);
            if (seedInfo == null)
            {
                Log.Warn($"player {Uid} GetSeedReward failed: have no pit {pit} info");
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            GardenSeedModel model = GardenLibrary.GetGardenSeedModel(GardenPeriod, seedInfo.SeedId);
            if (model == null)
            {
                Log.Warn($"player {Uid} GetSeedReward failed: have no this seed  info {seedInfo.SeedId}");
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            int diamondNum = 0;
            DateTime endTime = Timestamp.TimeStampToDateTime(seedInfo.EndTime);
            if (useDiamond)
            {
                int diamond = (int) Math.Ceiling((endTime - server.Now()).TotalSeconds * GardenLibrary.HarverstFactor);
                if (diamond > 0)
                {
                    if (!CheckCoins(CurrenciesType.diamond, diamond))
                    {
                        msg.Result = (int) ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }

                    DelCoins(CurrenciesType.diamond, diamond, ConsumeWay.Garden, pit.ToString());
                    diamondNum = diamond;
                }
            }
            else
            {
                if (server.Now() < endTime)
                {
                    msg.Result = (int) ErrorCode.NotReachTime;
                    Write(msg);
                    return;
                }
            }

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Data.GetString("Reward"));
            AddRewards(manager, ObtainWay.Garden);
            manager.GenerateRewardItemInfo(msg.Rewards);

            GardenManager.HarvestPitSeed(pit, model.Score);

            msg.Result = (int) ErrorCode.Success;
            msg.Score = GardenManager.GardenInfo.Score;
            Write(msg);

            //日志
            BIRecordGardenLog(GardenPeriod, diamondNum, useDiamond, model.Score, GardenManager.GardenInfo.Score);
        }

        private void GetGardenScoreReward()
        {
            MSG_ZGC_GARDEN_REAWARD msg = new MSG_ZGC_GARDEN_REAWARD();

            int lastId = GardenManager.MaxScordRewardId();
            GardenScoreReward model = GardenLibrary.GetNextReward(GardenPeriod, lastId);
            if (model == null)
            {
                Log.Warn($"player {Uid} GetGardenScoreReward failed: have no next reward {lastId}");
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.Score > GardenManager.GardenInfo.Score)
            {
                Log.Warn($"player {Uid} GetGardenScoreReward failed: socre not enough {GardenManager.GardenInfo.Score}");
                msg.Result = (int) ErrorCode.GardenScoreNotEnough;
                Write(msg);
                return;
            }

            if (!GardenManager.AddRewardId(model.Id))
            {
                Log.Warn($"player {Uid} GetGardenScoreReward failed: socre rewarded {model.Id}");
                msg.Result = (int) ErrorCode.GardenScoreRewarded;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Data.GetString("Reward"));
            AddRewards(manager, ObtainWay.Garden);
            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.Result = (int) ErrorCode.Success;
            msg.Type = 1;
            msg.Score = GardenManager.GardenInfo.Score;
            msg.RewardId = GardenManager.MaxScordRewardId();
            Write(msg);
        }

        public void BuySeed(int seedId, int num)
        {
            MSG_ZGC_GARDEN_BUY_SEED msg = new MSG_ZGC_GARDEN_BUY_SEED();

            if (!GardenManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} BuySeed failed: have no seedInfo period {GardenPeriod} seedId {seedId}");
                msg.Result = (int)ErrorCode.NotOnTime;
                Write(msg);
                return;
            }

            GardenSeedModel model = GardenLibrary.GetGardenSeedModel(GardenPeriod, seedId);
            if (model == null || num <= 0)
            {
                Log.Warn($"player {Uid} BuySeed failed: have no seedInfo period {GardenPeriod} seedId {seedId}");
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            int diamond = model.Diamond * num;
            if (!CheckCoins(CurrenciesType.diamond, diamond))
            {
                msg.Result = (int) ErrorCode.DiamondNotEnough;
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.diamond, diamond, ConsumeWay.Garden, seedId.ToString());

            RewardManager manager = new RewardManager();
            manager.AddReward(new ItemBasicInfo((int) RewardType.NormalItem, model.SeedId, num));
            manager.BreakupRewards();

            AddRewards(manager, ObtainWay.Garden);
            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.Result = (int) ErrorCode.Success;
            msg.SeedId = seedId;
            Write(msg);
        }

        public void GardenShopExchange(int id)
        {
            MSG_ZGC_GARDEN_SHOP_EXCHANGE msg = new MSG_ZGC_GARDEN_SHOP_EXCHANGE();

            RechargeGiftModel shopModel;
            if (!RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.Garden, BaseApi.now, out shopModel))
            {
                Log.Warn($"player {Uid} GardenShopExchange failed: have no active info");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            var model = GardenLibrary.GetGardenExchangeShopModel(shopModel.SubType, id);
            if (model == null || model.CostItems.Count <= 0)
            {
                Log.Warn($"player {Uid} GardenShopExchange failed: have no shop model info {id}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (shopModel?.ShowShop != true)
            {
                Log.Warn($"player {Uid} GardenShopExchange failed: shop not open");
                msg.Result = (int)ErrorCode.GardenShopNotOpen;
                Write(msg);
                return;
            }

            Dictionary<BaseItem, int> costItems = new Dictionary<BaseItem, int>(model.CostItems.Count);

            foreach (var item in model.CostItems)
            {
                BaseItem baseItem = bagManager.NormalBag.GetItem(item.Id);
                if (baseItem == null || baseItem.PileNum < item.Num)
                {
                    Log.Warn($"player {Uid} GardenShopExchange failed: have no enough item {item.Id}");
                    msg.Result = (int)ErrorCode.ItemNotEnough;
                    Write(msg);
                    return;
                }

                costItems.Add(baseItem, item.Num);
            }

            List<BaseItem> reItems = DelItem2Bag(costItems, RewardType.NormalItem, ConsumeWay.Garden);
            if (reItems != null)
            {
                SyncClientItemsInfo(reItems);
            }

            RewardManager manager = new RewardManager();
            manager.AddReward(model.RewardItems);
            manager.BreakupRewards();

            AddRewards(manager, ObtainWay.Garden);
            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.Result = (int)ErrorCode.Success;
            msg.Id = id;
            Write(msg);
        }
    }

}
