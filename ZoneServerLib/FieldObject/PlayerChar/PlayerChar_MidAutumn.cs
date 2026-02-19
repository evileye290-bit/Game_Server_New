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
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public MidAutumnManager MidAutumnMng { get; private set; }
        public void InitMidAutumnManager()
        {
            MidAutumnMng = new MidAutumnManager(this);
        }

        public void GetMidAutumnInfoByLoading()
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.MidAutumn, ZoneServerApi.now, out model))
            {
                return;
            }
            //if (MidAutumnMng.Period == 0)
            //{
            //    return;
            //}
            GetMidAutumnInfo();
        }

        public void GetMidAutumnInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_RANK_FIRST_INFO msg = new MSG_ZR_GET_RANK_FIRST_INFO() { RankType = (int)RankType.MidAutumn };
            server.SendToRelation(msg, Uid);
        }

        public MSG_ZGC_MIDAUTUMN_INFO GenerateMidAutumnInfo()
        {
            MSG_ZGC_MIDAUTUMN_INFO msg = new MSG_ZGC_MIDAUTUMN_INFO();
            msg.Score = MidAutumnMng.Info.Score;
            msg.CurDiscount = MidAutumnMng.Info.CurDiscount;
            msg.FreeUsed = MidAutumnMng.Info.FreeUsed;
            msg.ScoreRewards.AddRange(MidAutumnMng.Info.ScoreRewards);
            foreach (var item in MidAutumnMng.Info.ItemExchangeCounts)
            {
                msg.ExchangeCount.Add(item.Key, item.Value);
            }
            msg.CurSpecialDiscountRatio = MidAutumnMng.Info.CurSpecialDiscountRatio;
            return msg;
        }

        /// <summary>
        /// 抽奖
        /// </summary>
        /// <param name="free">是否免费</param>
        /// <param name="consecutive">是否连抽</param>
        public void DrawMidAutumnReward(bool free, bool consecutive)
        {
            MSG_ZGC_DRAW_MIDAUTUMN_REWARD response = new MSG_ZGC_DRAW_MIDAUTUMN_REWARD();

            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.MidAutumn, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} DrawMidAutumnReward failed: time is error");
                response.Result = (int)ErrorCode.RouletteNotOpen;
                Write(response);
                return;
            }
            int period = model.SubType;

            //if (MidAutumnMng.Period == 0)
            //{
            //    Log.Warn($"player {Uid} DrawMidAutumnReward failed: time is error");
            //    response.Result = (int)ErrorCode.NotOnTime;
            //    Log.Write(response);
            //    return;
            //}
            
            MidAutumnConfig config = MidAutumnLibrary.GetConfig(period);
            if (config == null)
            {
                Log.Warn($"player {Uid} DrawMidAutumnReward failed: not find period {period} config");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int costDiamond = config.DrawOnceDiamond;
            if (consecutive)
            {
                costDiamond = config.DrawFiveTimesDiamond;
            }
            costDiamond = (int)Math.Ceiling(costDiamond * MidAutumnMng.Info.CurDiscount * 0.1);

            //连抽
            if (consecutive)
            {
                if (GetCoins(CurrenciesType.diamond) < costDiamond)
                {
                    Log.Warn($"player {Uid} DrawMidAutumnReward failed: diamond {GetCoins(CurrenciesType.diamond)} not enough, need num {costDiamond}");
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(response);
                    return;
                }

                if (free)
                {
                    Log.Warn($"player {Uid} DrawMidAutumnReward failed: consecutive draw can not free");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            }
            else
            {
               //单抽免费
                if (free)
                {
                    //免费次数用尽
                    if (MidAutumnMng.Info.FreeUsed)
                    {
                        Log.Warn($"player {Uid} DrawMidAutumnReward failed: can not draw for free");
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                }
                else
                {
                    //单抽耗钻
                    if (!MidAutumnMng.Info.FreeUsed)
                    {
                        Log.Warn($"player {Uid} DrawMidAutumnReward failed: can draw for free");
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }

                    if (GetCoins(CurrenciesType.diamond) < costDiamond)
                    {
                        Log.Warn($"player {Uid} DrawMidAutumnReward failed: diamond {GetCoins(CurrenciesType.diamond)} not enough, need num {costDiamond}");
                        response.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(response);
                        return;
                    }
                }
            }
           
            int score = config.DrawOnceScore;
            int drawCount = 1;
            if (consecutive)
            {
                score = score * config.ConsecutiveCount;
                drawCount = config.ConsecutiveCount;
            }

            if (!free)
            {
                DelCoins(CurrenciesType.diamond, costDiamond, ConsumeWay.MidAutumn, score.ToString());
            }

            MidAutumnMng.UpdateDrawInfo(free, score, config);

            List<string> rewards = GenerateMidAutumnReward(drawCount, config, period);
            if (rewards.Count > 0)
            {
                RewardManager manager = new RewardManager();
                List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();
                //按有装备和魂骨生成奖励
                foreach (var reward in rewards)
                {
                    if (!string.IsNullOrEmpty(reward))
                    {
                        RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        rewardItems.AddRange(items);
                    }
                }
                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.MidAutumn);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            response.CurDiscount = MidAutumnMng.Info.CurDiscount;
            response.Score = MidAutumnMng.Info.Score;
            response.FreeUsed = MidAutumnMng.Info.FreeUsed;
            response.CurSpecialDiscountRatio = MidAutumnMng.Info.CurSpecialDiscountRatio;
            Write(response);

            SerndUpdateRankValue(RankType.MidAutumn, MidAutumnMng.Info.Score);
            
            BIRecordPointGameLog(score, MidAutumnMng.Info.Score, "midautumn", model.SubType);
        }

        private List<string> GenerateMidAutumnReward(int drawCount, MidAutumnConfig config, int period)
        {
            List<string> rewards = new List<string>();

            for (int i = 0; i < drawCount; i++)
            {
                //随机福字
                List<MidAutumnRandomReward> words = MidAutumnLibrary.GetRandomRewardList(period, config.WordsPoolType);
                string word = GetRandomRewardByWeight(words);
                if (!string.IsNullOrEmpty(word))
                {
                    rewards.Add(word);
                }

                //随机物品
                int rewardPool = config.RandRewardPool();
                List<MidAutumnRandomReward> rewardList = MidAutumnLibrary.GetRandomRewardList(period, rewardPool);
                string reward = GetRandomRewardByWeight(rewardList);
                if (!string.IsNullOrEmpty(reward))
                {
                    rewards.Add(reward);
                }
            }

            return rewards;
        }

        private string GetRandomRewardByWeight(List<MidAutumnRandomReward> rewardList)
        {
            string reward = string.Empty;
            if (rewardList == null)
            {
                return reward;
            }

            int totalWeight = 0;
            Dictionary<int, MidAutumnRandomReward> weightDic = new Dictionary<int, MidAutumnRandomReward>();
            foreach (var rewardModel in rewardList)
            {
                totalWeight += rewardModel.Weight;
                weightDic.Add(totalWeight, rewardModel);
            }
            
            int rand = NewRAND.Next(1, totalWeight);            
            int cur = 0;       
            foreach (var kv in weightDic)
            {
                if (rand > cur && rand <= kv.Key)
                {
                    reward = kv.Value.Reward;
                    break;
                }
                cur = kv.Key;
            }

            return reward;
        }

        /// <summary>
        /// 领取积分奖励
        /// </summary>
        public void GetMidAutumnScoreReward(int rewardId)
        {
            MSG_ZGC_GET_MIDAUTUMN_SCORE_REWARD response = new MSG_ZGC_GET_MIDAUTUMN_SCORE_REWARD();
            response.RewardId = rewardId;

            RechargeGiftModel model;
            if (!RechargeLibrary.InitRechargeGiftTime(RechargeGiftType.MidAutumn, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} GetMidAutumnScoreReward failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Write(response);
                return;
            }
            int period = model.SubType;         

            MidAutumnScoreReward rewardModel = MidAutumnLibrary.GetScoreReward(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetMidAutumnScoreReward failed: not find rewardId {rewardId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (rewardModel.Period != period)
            {
                Log.Warn($"player {Uid} GetMidAutumnScoreReward failed: rewardId {rewardId} not cur period {period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (MidAutumnMng.Info.ScoreRewards.Contains(rewardModel.Id))
            {
                Log.Warn($"player {Uid} GetMidAutumnScoreReward failed: rewardId {rewardId} alrady got");
                response.Result = (int)ErrorCode.AlreadyGot;
                Write(response);
                return;
            }

            if (MidAutumnMng.Info.Score < rewardModel.Score)
            {
                Log.Warn($"player {Uid} GetMidAutumnScoreReward {rewardId} failed: score {MidAutumnMng.Info.Score} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            MidAutumnMng.UpdateScoreRewards(rewardModel.Id);

            if (!string.IsNullOrEmpty(rewardModel.Reward))
            {
                //按有装备和魂骨生成奖励
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Reward);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.MidAutumn);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void ClearMidAutumnInfo()
        {
            MidAutumnMng.Clear();
            GetMidAutumnInfo();
        }

        public void RefreshMidAutumnFreeFlag()
        {           
            if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.MidAutumn, ZoneServerApi.now) && MidAutumnMng.Info.FreeUsed)
            {
                MidAutumnMng.UpdateFreeFlag(false);
                GetMidAutumnInfo();
            }
        }
    }
}
