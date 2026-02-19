using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public RouletteManager RouletteManager { get; private set; }

        private void InitRouletteManager()
        {
            RouletteManager = new RouletteManager(this);
        }

        private void GetRouletteByLoading()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.Roulette, BaseApi.now, out model))
            {
                GetRouletteInfo();
            }
        }

        public void GetRouletteInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_RANK_FIRST_INFO msg = new MSG_ZR_GET_RANK_FIRST_INFO() {RankType = (int) RankType.Roulette};
            server.SendToRelation(msg, Uid);
        }

        public void RouletteRandom(int num)
        {
            MSG_ZGC_ROULETTE_RANDOM response = new MSG_ZGC_ROULETTE_RANDOM();

            if (!RouletteManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} PlantedSeed failed: time is error");
                response.Result = (int) ErrorCode.RouletteNotOpen;
                Write(response);
                return;
            }

            BaseItem costItem = bagManager.NormalBag.GetItem(RouletteLibrary.CostItemId.Id);
            if (costItem == null || costItem.PileNum < num)
            {
                response.Result = (int)ErrorCode.ItemNotEnough;
                Write(response);
                return;
            }

            int weight = 0;
            List<RouletteItemModel> models = new List<RouletteItemModel>();
            if (!RouletteManager.CheckItems(models, out weight) || models.Count == 0)
            {
                Log.Error($"roulette list error cur period {RouletteManager.Period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            BaseItem synItem = DelItem2Bag(costItem, RewardType.NormalItem, num, ConsumeWay.Roulette);
            if (synItem != null)
            {
                SyncClientItemInfo(synItem);
            }

            int extraNum = 0, addScore = 0;
            List<int> randomIdList = new List<int>();
            RewardManager manager = new RewardManager();

            for (int i = 0; i < num; i++)
            {
                RouletteItemModel model = RandomRouletteItemModel(models, weight);
                if (model == null)
                {
                    Log.Warn($"had not random a roulette module weight {weight} model count {models.Count}");
                    i--;
                    continue;
                }

                addScore += model.Score;
                randomIdList.Add(model.Id);
                if (model.ActionType == RouletteActionType.ExtraStep)
                {
                    extraNum += model.ActionParam;
                }
                manager.AddSimpleReward(model.Reward);
            }

            if (extraNum > 0)
            {
                models = models.Where(x => x.ActionType != RouletteActionType.ExtraStep).ToList();
                weight = models.Sum(x => x.RandomWeight);
                for (int i = 0; i < extraNum; i++)
                {
                    RouletteItemModel model = RandomRouletteItemModel(models, weight);
                    addScore += model.Score;
                    randomIdList.Add(model.Id);
                    manager.AddSimpleReward(model.Reward);
                }
            }

            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.Roulette);
            manager.GenerateRewardMsg(response.Rewards);

            RouletteManager.AddScore(addScore);

            response.Score = RouletteManager.Score;
            response.RandomList.Add(randomIdList);
            response.Result = (int) ErrorCode.Success;
            Write(response);

            RouletteRefresh(false);

            BIRecordPointGameLog(addScore, RouletteManager.RouletteInfo.Score, "roulette", RouletteManager.Period);
        }

        private RouletteItemModel RandomRouletteItemModel(List<RouletteItemModel> models, int weight)
        {
            int random = RAND.Range(0, weight - 1);
            foreach (var item in models)
            {
                if (random < item.RandomWeight) return item;
                random -= item.RandomWeight;
            }

            return null;
        }

        public void RouletteReward()
        {
            MSG_ZGC_ROULETTE_REWARD msg = new MSG_ZGC_ROULETTE_REWARD();

            //if (!RouletteManager.CheckPeriodInfo())
            //{
            //    Log.Warn($"player {Uid} RouletteReward failed: time is error");
            //    msg.Result = (int) ErrorCode.RouletteNotOpen;
            //    Write(msg);
            //    return;
            //}

            int lastId = RouletteManager.MaxScoreRewardId();
            RouletteScoreReward model = RouletteLibrary.GetNextReward(RouletteManager.Period, lastId);
            if (model == null)
            {
                Log.Warn($"player {Uid} RouletteReward failed: have no next reward {lastId}");
                msg.Result = (int) ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.Score > RouletteManager.Score)
            {
                Log.Warn($"player {Uid} RouletteReward failed: score not enough {RouletteManager.Score}");
                msg.Result = (int) ErrorCode.RouletteScoreNotEnough;
                Write(msg);
                return;
            }

            if (!RouletteManager.AddRewardId(model.Id))
            {
                Log.Warn($"player {Uid} RouletteReward failed: score rewarded {model.Id}");
                msg.Result = (int) ErrorCode.RouletteScoreRewarded;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Data.GetString("Reward"));
            AddRewards(manager, ObtainWay.Garden);
            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.Result = (int) ErrorCode.Success;
            msg.Score = RouletteManager.Score;
            msg.RewardId = RouletteManager.MaxScoreRewardId();
            Write(msg);
        }

        public void RouletteRefresh(bool costCoin = true)
        {
            MSG_ZGC_ROULETTE_REFRESH msg = new MSG_ZGC_ROULETTE_REFRESH();

            if (!RouletteManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} RouletteReward failed: time is error");
                msg.Result = (int) ErrorCode.RouletteNotOpen;
                Write(msg);
                return;
            }

            if (costCoin)
            {
                if (!CheckCoins(CurrenciesType.diamond, RouletteLibrary.RefreshCost.Num))
                {
                    msg.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(msg);
                    return;
                }

                DelCoins(CurrenciesType.diamond, RouletteLibrary.RefreshCost.Num, ConsumeWay.Roulette, "");
            }

            RouletteManager.Refresh();

            msg.IdList.Add(RouletteManager.RouletteInfo.IdList);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void RouletteBuyItem(int num)
        {
            MSG_ZGC_ROULETTE_BUY_ITEM msg = new MSG_ZGC_ROULETTE_BUY_ITEM();

            Data buyData = DataListManager.inst.GetData("Counter", (int)CounterType.RouletteBuyCount);
            if (buyData == null)
            {
                Log.Warn($"player {Uid} RouletteBuyItem failed: not find in counter xml");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (!RouletteManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} RouletteReward failed: time is error");
                msg.Result = (int)ErrorCode.RouletteNotOpen;
                Write(msg);
                return;
            }

            if (CheckCounter(CounterType.RouletteBuyCount))
            {
                Log.Warn($"player {Uid} RouletteReward failed: already max count");
                msg.Result = (int)ErrorCode.MaxCount;
                Write(msg);
                return;
            }

            int cost = 0;
            int buyCount = GetCounterValue(CounterType.RouletteBuyCount);
            for (int i = 0; i < num; i++)
            {
                cost += CounterLibrary.GetBuyCountCost(buyData.GetString("Price"), buyCount + i + 1);
            }

            if (!CheckCoins(CurrenciesType.diamond, cost))
            {
                msg.Result = (int)ErrorCode.DiamondNotEnough;
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.diamond, cost, ConsumeWay.Roulette, "");

            UpdateCounter(CounterType.RouletteBuyCount, num);

            List<BaseItem> synList = AddItem2Bag(MainType.Consumable, RewardType.NormalItem, RouletteLibrary.CostItemId.Id, num, ObtainWay.Roulette);
            if (synList != null)
            {
                SyncClientItemsInfo(synList);
            }

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }
    }
}
