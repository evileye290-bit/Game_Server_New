using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public ShreklandManager ShreklandMng { get; private set; }

        public void InitShreklandManager()
        {
            ShreklandMng = new ShreklandManager(this);
        }

        public void SendShreklandInfoByLoading()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.Shrekland, ZoneServerApi.now, out model))
            {
                SendShreklandInfo();
            }
        }

        public void SendShreklandInfo()
        {
            MSG_ZGC_SHREKLAND_INFO msg = new MSG_ZGC_SHREKLAND_INFO();
            msg.RewardLevel = ShreklandMng.RewardLevel;
            msg.StepIndex = ShreklandMng.StepIndex;
            msg.Score = ShreklandMng.Score;
            msg.GridRewards.AddRange(ShreklandMng.GetGridRewards());
            msg.ScoreRewards.AddRange(ShreklandMng.GetScoreRewards());
            Write(msg);
        }

        /// <summary>
        /// 使用轮盘
        /// </summary>
        /// <param name="type"></param>
        /// <param name="num"></param>
        public void UseShreklandRoulette(int type, int num)
        {
            MSG_ZGC_SHREKLAND_USE_ROULETTE response = new MSG_ZGC_SHREKLAND_USE_ROULETTE();
            response.Type = type;

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.Shrekland, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} UseShreklandRoulette failed: activity not open");
                Write(response);
                return;
            }

            BaseItem rouletteItem = null;
            ShrekRouletteType rouletteType = (ShrekRouletteType)type;
            switch (rouletteType)
            {
                case ShrekRouletteType.Normal:
                    if (!CheckCoins(CurrenciesType.diamond, ShreklandLibrary.NormalItemCost))
                    {
                        response.Result = (int)ErrorCode.DiamondNotEnough;
                        Log.Warn($"player {Uid} UseShreklandRoulette failed: diamond not enough");
                        Write(response);
                        return;
                    }
                    break;
                case ShrekRouletteType.Double:
                    rouletteItem = BagManager.GetItem(MainType.Consumable, ShreklandLibrary.DoubleItemId);                   
                    break;
                case ShrekRouletteType.Treble:
                    rouletteItem = BagManager.GetItem(MainType.Consumable, ShreklandLibrary.TrebleItemId);          
                    break;
                case ShrekRouletteType.Control:
                    rouletteItem = BagManager.GetItem(MainType.Consumable, ShreklandLibrary.ControlItemId);
                    if (num < ShreklandLibrary.StepNumMin || num > ShreklandLibrary.StepNumMax)
                    {
                        response.Result = (int)ErrorCode.Fail;
                        Log.Warn($"player {Uid} UseShreklandRoulette failed: num {num} param error");
                        Write(response);
                        return;
                    }
                    break;
                default:
                    response.Result = (int)ErrorCode.Fail;
                    Log.Warn($"player {Uid} UseShreklandRoulette failed: type {type} param error");
                    Write(response);
                    return;
            }
            int stepNum = 0;
            int newStepIndex = -1;
            if (rouletteType == ShrekRouletteType.Normal)
            {
                DelCoins(CurrenciesType.diamond, ShreklandLibrary.NormalItemCost, ConsumeWay.Shrekland, type.ToString());
            }
            else
            {
                if (rouletteItem == null)
                {
                    response.Result = (int)ErrorCode.ItemNotEnough;
                    Log.Warn($"player {Uid} UseShreklandRoulette failed: item not enough");
                    Write(response);
                    return;
                }
                BaseItem it = DelItem2Bag(rouletteItem, RewardType.NormalItem, 1, ConsumeWay.Shrekland);
                if (it != null)
                {
                    SyncClientItemInfo(it);
                }
                if (rouletteType == ShrekRouletteType.Control)
                {
                    stepNum = num;
                    newStepIndex = ShreklandMng.GetNextStepIndexByStepNum(num);
                }
            }
            if (newStepIndex == -1)
            {
                Tuple<int, int> temp = ShreklandMng.GetNextStepIndex();
                stepNum = temp.Item1;
                newStepIndex = temp.Item2;
            }

            int factor = GetRewardFactor(rouletteType);

            //先发奖再更新
            List<int> gridRewards = ShreklandMng.GetGridRewards();
            int rewardId = gridRewards[newStepIndex];
            ShreklandRandReward rewardModel = ShreklandLibrary.GetRandomReward(rewardId);
            if (!string.IsNullOrEmpty(rewardModel.Reward))
            {
                //按有装备和魂骨生成奖励
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Reward);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

                RewardManager manager = new RewardManager();
                manager.AddReward(rewardItems);
                //根据轮盘类型奖励翻倍
                manager *= factor;
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.Shrekland);
                manager.GenerateRewardMsg(response.Rewards);
            }

            int beforeScore = ShreklandMng.Score;
            ShreklandMng.UseRoulette(newStepIndex, factor);
            SendShreklandInfo();
            
            BIRecordPointGameLog(ShreklandMng.Score-beforeScore, ShreklandMng.Score, "shrekland", activityModel.SubType);

            response.Num = stepNum;
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 刷新奖池
        /// </summary>
        public void RefreshShreklandRewards()
        {
            MSG_ZGC_SHREKLAND_REFRESH_REWARDS response = new MSG_ZGC_SHREKLAND_REFRESH_REWARDS();
            //检查是否活动开启
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.Shrekland, ZoneServerApi.now))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} RefreshShreklandRewards failed: activity not open");
                Write(response);
                return;
            }
            if (!CheckCoins(CurrenciesType.diamond, ShreklandLibrary.RefreshCost))
            {
                response.Result = (int)ErrorCode.DiamondNotEnough;
                Log.Warn($"player {Uid} RefreshShreklandRewards failed: diamond not enough");
                Write(response);
                return;
            }
            if (!ShreklandMng.RefreshRandomRewards())
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} RefreshShreklandRewards failed: diamond not enough");
                Write(response);
                return;
            }
            DelCoins(CurrenciesType.diamond, ShreklandLibrary.RefreshCost, ConsumeWay.Shrekland, "");
            response.GridRewards.AddRange(ShreklandMng.GetGridRewards());
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }
    
        /// <summary>
        /// 领取积分奖励
        /// </summary>
        public void GetShreklandScoreReward(int rewardId)
        {
            MSG_ZGC_SHREKLAND_GET_SCORE_REWARD response = new MSG_ZGC_SHREKLAND_GET_SCORE_REWARD();
            response.RewardId = rewardId;

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.Shrekland, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get shrekland score reward failed: not open");
                Write(response);
                return;
            }
            int period = activityModel.SubType;

            ScoreRewardModel rewardModel = ShreklandLibrary.GetScoreRewardModel(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} get shrekland score reward failed: not find rewardId {rewardId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (rewardModel.Period != period)
            {
                Log.Warn($"player {Uid} get shrekland score reward failed: rewardId {rewardId} not cur period {period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            List<int> scoreRewards = ShreklandMng.GetScoreRewards();
            if (scoreRewards.Contains(rewardModel.Id))
            {
                Log.Warn($"player {Uid} get shrekland score reward failed: rewardId {rewardId} alrady got");
                response.Result = (int)ErrorCode.AlreadyGot;
                Write(response);
                return;
            }

            if (ShreklandMng.Score < rewardModel.Score)
            {
                Log.Warn($"player {Uid} get shrekland score reward {rewardId} failed: score {ShreklandMng.Score} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            ShreklandMng.AddScoreReward(rewardModel.Id);

            if (!string.IsNullOrEmpty(rewardModel.Reward))
            {
                //按有装备和魂骨生成奖励
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Reward);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.Shrekland);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }
      
        public void ClearShreklandInfo()
        {
            ShreklandMng.Clear();
            ShreklandMng.InitRandomRewards();
            SendShreklandInfo();
        }

        /// <summary>
        /// 获取奖励系数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetRewardFactor(ShrekRouletteType type)
        {
            int factor = 1;
            if (type == ShrekRouletteType.Double)
            {
                factor = 2;
            }
            else if (type == ShrekRouletteType.Treble)
            {
                factor = 3;
            }
            return factor;
        }
    }
}
