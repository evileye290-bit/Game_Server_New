using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerModels.Contribution;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public int ContributionValue { get; set; }

        public void GetContributionInfo()
        {
            MSG_ZGC_CONTRIBUTION_INFO response = new MSG_ZGC_CONTRIBUTION_INFO();
            response.PhaseNum = server.ContributionMng.PhaseNum;
            response.CurrentValue = server.ContributionMng.CurrentValue;
            Write(response);

            GetContributionValue();
        }

        private void GetContributionValue()
        {
            OperateGetRankScoreByUid op = new OperateGetRankScoreByUid(RankType.Contribution, server.MainId, uid);
            server.GameRedis.Call(op, ret =>
            {
                ContributionValue = op.Score;
            });
        }

        public float GetContributionOnhookRatio()
        {
            int phaseNum = server.ContributionMng.PhaseNum;
            ContributionModel model = ContributionLibrary.GetContributionModel(phaseNum - 1);
            if (model == null)
            {
                return 0;
            }
            return model.OnhookReward;
        }

        public void GetContributionReward()
        {
            int phaseNum = server.ContributionMng.PhaseNum;
            int value = server.ContributionMng.CurrentValue;

            MSG_ZGC_GET_CONTRIBUTION_REWARD msg = new MSG_ZGC_GET_CONTRIBUTION_REWARD();
            Counter counter = GetCounter(CounterType.GetContributionCount);
            if (counter == null)
            {
                Log.Warn($"player {Uid} get contribution reward failed: not find contribution count");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
            if (counter.Count + 1 >= phaseNum)
            {
                //已经领取
                Log.Warn($"player {Uid} get contribution reward failed: contribution count {counter.Count+1} over phaseNum {phaseNum}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
            ContributionModel model = ContributionLibrary.GetContributionModel(counter.Count + 1);
            if (model == null)
            {
                Log.Warn($"player {Uid} get contribution reward failed: not find contribution by count {counter.Count+1}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
            string reward = ContributionLibrary.GetContributionReward(model.RewardId);
            if (!string.IsNullOrEmpty(reward))
            {
                RewardManager manager = new RewardManager();
                manager.InitSimpleReward(reward);
                AddRewards(manager, ObtainWay.Contribution);

                manager.GenerateRewardMsg(msg.Rewards);
            }
            UpdateCounter(CounterType.GetContributionCount, 1);
            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

    }
}
