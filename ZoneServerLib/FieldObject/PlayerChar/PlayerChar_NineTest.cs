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
        public NineTestManager NineTestMng { get; private set; }
        public void InitNineTestManager()
        {
            NineTestMng = new NineTestManager(this);
        }

        public void GetNineTestInfoByLoading()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NineTest, ZoneServerApi.now, out model))
            {
                NineTestMng.CheckInit(model.SubType);
                GetNineTestInfo();
            }
        }

        public void GetNineTestInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_RANK_FIRST_INFO msg = new MSG_ZR_GET_RANK_FIRST_INFO() { RankType = (int)RankType.NineTest};
            server.SendToRelation(msg, Uid);
        }

        public MSG_ZGC_GET_NINETEST_INFO GenerateNineTestInfo()
        {
            MSG_ZGC_GET_NINETEST_INFO msg = new MSG_ZGC_GET_NINETEST_INFO();
            msg.Score = NineTestMng.Info.Score;
            msg.CurRewards.AddRange(NineTestMng.Info.CurRewards);
            NineTestMng.Info.IndexRewards.ForEach(x => msg.IndexRewards.Add(x.Key, x.Value));//           
            msg.ScoreRewards.AddRange(NineTestMng.Info.ScoreRewards);
            return msg;
        }

        /// <summary>
        /// 开启格子
        /// </summary>
        /// <param name="index"></param>
        public void ClickNineTestGrid(int index)
        {
            MSG_ZGC_NINETEST_CLICK_GRID response = new MSG_ZGC_NINETEST_CLICK_GRID();
            response.Index = index;

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.NineTest, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} click nine test grid failed: activity not open");
                Write(response);
                return;
            }

            int period = activityModel.SubType;

            NineTestConfig config = NineTestLibrary.GetConfig(period);
            if (config == null)
            {
                Log.Warn($"player {Uid} click nine test grid failed: not find period {period} config");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (index > config.GridCount || NineTestMng.Info.IndexRewards.ContainsKey(index))
            {
                Log.Warn($"player {Uid} click nine test grid failed: index {index} error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int costDiamond = config.GetCostDiamond(NineTestMng.Info.IndexRewards.Count + 1);
            if (GetCoins(CurrenciesType.diamond) < costDiamond)
            {
                Log.Warn($"player {Uid} click nine test grid failed: diamond {GetCoins(CurrenciesType.diamond)} not enough, need num {costDiamond}");
                response.Result = (int)ErrorCode.DiamondNotEnough;
                Write(response);
                return;
            }

            RandomRewardModel rewardModel = NineTestMng.GetRandomReward(config.RewardTypeWeight);
            int score = costDiamond / config.OneScoreMappingDiamond;
            NineTestMng.UpdateIndexRewardInfo(index, rewardModel.Id, score);

            DelCoins(CurrenciesType.diamond, costDiamond, ConsumeWay.NineTest, index.ToString());

            if (!string.IsNullOrEmpty(rewardModel.Reward))
            {
                //按有装备和魂骨生成奖励              
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Reward);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

                RewardManager manager = new RewardManager();
                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.NineTest);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;         
            response.Score = NineTestMng.Info.Score;
            response.RewardId = rewardModel.Id;
            Write(response);

            if (config.ShowRank)
            {
                SerndUpdateRankValue(RankType.NineTest, NineTestMng.Info.Score);
            }
            
            BIRecordPointGameLog(score,  NineTestMng.Info.Score, "nine_test", period);
        }

        /// <summary>
        /// 领积分奖
        /// </summary>
        public void GetNineTestScoreReward(int rewardId)
        {
            MSG_ZGC_NINETEST_SCORE_REWARD response = new MSG_ZGC_NINETEST_SCORE_REWARD();
            response.RewardId = rewardId;

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.NineTest, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get nine test score reward failed: not open");
                Write(response);
                return;
            }
            int period = activityModel.SubType;

            ScoreRewardModel rewardModel = NineTestLibrary.GetScoreReward(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetNineTestScoreReward failed: not find rewardId {rewardId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (rewardModel.Period != period)
            {
                Log.Warn($"player {Uid} GetNineTestScoreReward failed: rewardId {rewardId} not cur period {period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (NineTestMng.Info.ScoreRewards.Contains(rewardModel.Id))
            {
                Log.Warn($"player {Uid} GetNineTestScoreReward failed: rewardId {rewardId} alrady got");
                response.Result = (int)ErrorCode.AlreadyGot;
                Write(response);
                return;
            }

            if (NineTestMng.Info.Score < rewardModel.Score)
            {
                Log.Warn($"player {Uid} GetNineTestScoreReward {rewardId} failed: score {NineTestMng.Info.Score} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            NineTestMng.UpdateScoreRewards(rewardModel.Id);

            if (!string.IsNullOrEmpty(rewardModel.Reward))
            {
                //按有装备和魂骨生成奖励
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Reward);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.NineTest);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void NineTestReset(bool free)
        {
            MSG_ZGC_NINETEST_RESET response = new MSG_ZGC_NINETEST_RESET();

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.NineTest, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} nine test reset failed: activity not open");
                Write(response);
                return;
            }

            int period = activityModel.SubType;

            NineTestConfig config = NineTestLibrary.GetConfig(period);
            if (config == null)
            {
                Log.Warn($"player {Uid} nine test reset failed: not find period {period} config");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if ((free && NineTestMng.Info.IndexRewards.Count == 0) || (!free && NineTestMng.Info.IndexRewards.Count > 0))
            {
                Log.Warn($"player {Uid} nine test reset failed: free param error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!free)
            {
                if (GetCoins(CurrenciesType.diamond) < config.RestDiamond)
                {
                    Log.Warn($"player {Uid} nine test reset failed: diamond {GetCoins(CurrenciesType.diamond)} not enough, need num {config.RestDiamond}");
                    response.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(response);
                    return;
                }

                DelCoins(CurrenciesType.diamond, config.RestDiamond, ConsumeWay.NineTest, "");
            }

            NineTestMng.Reset(config);

            response.Result = (int)ErrorCode.Success;
            response.CurRewards.AddRange(NineTestMng.Info.CurRewards);
            //NineTestMng.Info.IndexRewards.ForEach(x => response.IndexRewards.Add(x.Key, x.Value));重置完是空的
            Write(response);
        }

        public void ClearNineTestInfo()
        {
            NineTestMng.Clear();
            GetNineTestInfo();
        }
    }
}
