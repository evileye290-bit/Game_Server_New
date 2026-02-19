using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public IslandHighManager IslandHighManager { get; private set; }

        private void InitIslandHighManager()
        {
            IslandHighManager = new IslandHighManager(this);
        }

        private void GetIslandHighByLoading()
        {
            GetHighInfo();
        }

        public void GetHighInfo()
        {
            RechargeGiftModel model;
            RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.IslandHigh, ZoneServerApi.now, out model);
            if (model != null)
            {
                MSG_ZR_GET_ISLAND_HIGH_INFO msg = new MSG_ZR_GET_ISLAND_HIGH_INFO();
                server.SendToRelation(msg, Uid);
            }
        }

        public void SendIslandHighInfo(int rankValue, int lastRankValue)
        {
            MSG_ZGC_ISLAND_HIGH_INFO msg = IslandHighManager.GenerateMsg();
            msg.RankValue = rankValue;
            msg.LastRankValue = lastRankValue;
            Write(msg);
        }

        public void HighRock(int type, int num)
        {
            MSG_ZGC_ISLAND_HIGH_ROCK msg = new MSG_ZGC_ISLAND_HIGH_ROCK();

            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.IslandHigh, ZoneServerApi.now, out model))
            {
                Log.Warn("player {0} HighRock error: finished", Uid);
                msg.ErrorCode = (int)ErrorCode.IslandHighFinished;
                Write(msg);
                return;
            }

            IslandHighManager.SetPeriod(model.SubType);

            int before = IslandHighManager.GridIndex;
            IslandHighManager.ResetRewardCache();
            IslandHighManager.ResetEventParamList();

            //1 普通骰子 2固定骰子 3双倍骰子
            int random = 0;
            int itemId = 0;
            switch (type)
            {
                case 1:
                    random = RAND.Range(1, 6);
                    msg.RockNum = random;
                    itemId = IslandHighLibrary.HighItemNormal;
                    break;
                case 2:
                    if (!IslandHighLibrary.IsLegalControlNum(num))
                    {
                        msg.ErrorCode = (int)ErrorCode.IslandHighControlNumError;
                        Write(msg);
                        return;
                    }
                    random = num;
                    msg.RockNum = num;
                    itemId = IslandHighLibrary.HighItemControl;
                    break;
                case 3:
                    random = RAND.Range(1, 6);
                    msg.RockNum = random;
                    random = random * 2;
                    itemId = IslandHighLibrary.HighItemDouble;
                    break;
                default:
                    msg.ErrorCode = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
            }

            BaseItem item = bagManager.NormalBag.GetItem(itemId);
            if (item == null || item.PileNum <= 0)
            {
                msg.ErrorCode = (int)ErrorCode.ItemNotEnough;
                Write(msg);
                return;
            }

            item = DelItem2Bag(item, RewardType.NormalItem, 1, ConsumeWay.IslandHigh);
            if (item != null)
            {
                SyncClientItemInfo(item);
            }
            IslandHighManager.AddEvent(IslandHighManager.GridIndex, 0, 1);

            //默认事件
            IslandHighManager.AddGrid(random);

            msg.GridIndex = IslandHighManager.GridIndex;
            msg.EventList.AddRange(IslandHighManager.GetEventParamList());

            IslandHighManager.SyncDbHighInfo();

            IslandHighManager.RewardManager.BreakupRewards();
            AddRewards(IslandHighManager.RewardManager, ObtainWay.IslandHigh);

            msg.ErrorCode = (int)ErrorCode.Success;
            IslandHighManager.RewardManager.GenerateRewardMsg(msg.RewardList);
            Write(msg);

            IslandHighManager.ResetRewardCache();
            IslandHighManager.ResetEventParamList();

            SerndUpdateRankValue(RankType.IslandHigh, IslandHighManager.GridIndex);

#if DEBUG
            Log.Info("-----------------------------------------------------------");
            Log.Info((object)$"islandhigh + uid {uid} before {before} after {IslandHighManager.GridIndex} ------- {msg.EventList}");
            Log.Info("-----------------------------------------------------------");
#endif
            BIRecordIslandHighLog(itemId, random, before, IslandHighManager.GridIndex, model);
        }

        public void IslandHighReward(int type, int rewardId)
        {

            switch (type)
            {
                case 1:
                    IslandHighStageReward(rewardId);
                    break;
                case 2:
                    IslandHighTotalReward(rewardId);
                    break;
            }
        }

        private void IslandHighStageReward(int rewardId)
        {
            MSG_ZGC_ISLAND_HIGH_REWARD msg = new MSG_ZGC_ISLAND_HIGH_REWARD();
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.IslandHigh, ZoneServerApi.now))
            {
                Log.Warn("player {0} IslandHighStageReward {1} HighRock error: finished", Uid);
                msg.ErrorCode = (int)ErrorCode.IslandHighFinished;
                Write(msg);
                return;
            }

            if (IslandHighManager.IslandHighDbInfo.StageRewardList.Contains(rewardId))
            {
                Log.Warn($"IslandHighStageReward have no this Id {rewardId} reward");
                msg.ErrorCode = (int)ErrorCode.IslandHighHadRewarded;
                Write(msg);
                return;
            }

            IslandHighRewardModel model = IslandHighLibrary.GetPathHighStageRewardModel(IslandHighManager.Period,rewardId);
            if (model == null)
            {
                Log.Warn($"IslandHighStageReward have no this Id {rewardId} reward");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.Grid > IslandHighManager.GridIndex)
            {
                Log.Warn($"IslandHighStageReward have no enough grid need {model.Grid} curr {IslandHighManager.GridIndex}");
                msg.ErrorCode = (int)ErrorCode.IslandHighGridNotEnough;
                Write(msg);
                return;
            }

            IslandHighManager.IslandHighDbInfo.StageRewardList.Add(rewardId);
            IslandHighManager.SyncDbHighInfo();

            msg.StageRewardList.Add(IslandHighManager.IslandHighDbInfo.StageRewardList);
            msg.TotalRewardList.Add(IslandHighManager.IslandHighDbInfo.TotalRewardList);

            IslandDoReward(msg, model.Reward);
        }

        private void IslandHighTotalReward(int rewardId)
        {
            MSG_ZGC_ISLAND_HIGH_REWARD msg = new MSG_ZGC_ISLAND_HIGH_REWARD();
            if (IslandHighManager.IslandHighDbInfo.TotalRewardList.Contains(rewardId))
            {
                Log.Warn($"IslandHighTotalReward have no this Id {rewardId} reward");
                msg.ErrorCode = (int)ErrorCode.IslandHighHadRewarded;
                Write(msg);
                return;
            }

            IslandHighRewardModel model = IslandHighLibrary.GetPathHighTotalRewardModel(IslandHighManager.Period, rewardId);
            if (model == null)
            {
                Log.Warn($"IslandHighTotalReward have no this Id {rewardId} reward");
                msg.ErrorCode = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }


            if (model.Grid > IslandHighManager.GridIndex)
            {
                Log.Warn($"IslandHighStageReward have no enough grid need {model.Grid} curr {IslandHighManager.GridIndex}");
                msg.ErrorCode = (int)ErrorCode.IslandHighGridNotEnough;
                Write(msg);
                return;
            }

            IslandHighManager.IslandHighDbInfo.TotalRewardList.Add(rewardId);
            IslandHighManager.SyncDbHighInfo();

            msg.StageRewardList.Add(IslandHighManager.IslandHighDbInfo.StageRewardList);
            msg.TotalRewardList.Add(IslandHighManager.IslandHighDbInfo.TotalRewardList);

            IslandDoReward(msg, model.Reward);
        }

        private void IslandDoReward(MSG_ZGC_ISLAND_HIGH_REWARD msg , string reward)
        {
            RewardManager manager = new RewardManager();
            manager.AddSimpleReward(reward);
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.IslandHigh);
            manager.GenerateRewardItemInfo(msg.RewardList);
            msg.ErrorCode = (int)ErrorCode.Success;
            Write(msg);
        }
    }
}
