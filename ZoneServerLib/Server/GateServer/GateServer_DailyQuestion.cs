using DataProperty;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_DailyQuestionCounter(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DAILY_QUESTION_COUNTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DAILY_QUESTION_COUNTER>(stream);           
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null) return;
            MSG_ZGC_DAILY_QUESTION_COUNTER response = new MSG_ZGC_DAILY_QUESTION_COUNTER();
            //response.CountLeft = player.GetCounterLeft(CounterType.DailyQuestion);
            player.Write(response);
        }

        private void OnResponse_DailyQuestionReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DAILY_QUESTION_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DAILY_QUESTION_REWARD>(stream);
            Log.Write("player {0} request daily question reward correct {1} diamond {2}", msg.Uid, msg.CorrectAnswers, msg.CostDiamond);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null) return;
            MSG_ZGC_DAILY_QUESTION_REWARD response = new MSG_ZGC_DAILY_QUESTION_REWARD();
            //if (player.GetCounterLeft(CounterType.DailyQuestion) <= 0)
            //{
            //    Log.Warn("player {0} request daily question reward correct {1} diamond {2} failed: max count", msg.Uid, msg.CorrectAnswers, msg.CostDiamond);
            //    response.Result = (int)ErrorCode.MaxCount;
            //    player.Write(response);
            //    return;
            //}
            if (msg.CostDiamond < 0)
            {
                Log.Warn("player {0} request daily question reward correct {1} diamond {2} failed: invalid diamond!", msg.Uid, msg.CorrectAnswers, msg.CostDiamond);
                response.Result = (int)ErrorCode.DiamondNotEnough;
                player.Write(response);
                return;
            }
            if (player.GetCoins(CurrenciesType.diamond) < msg.CostDiamond)
            {
                Log.Warn("player {0} request daily question reward correct {1} diamond {2} failed: diamond not enough", msg.Uid, msg.CorrectAnswers, msg.CostDiamond);
                response.Result = (int)ErrorCode.DiamondNotEnough;
                player.Write(response);
                return;
            }

            // 校验通过 发奖
            // 获取奖励信息
            DataList dataList = DataListManager.inst.GetDataList("TreasureMapReward");
            string rewardStr = string.Empty;
            foreach (var data in dataList.AllData)
            {
                if (data.Value.GetInt("question_count") == msg.CorrectAnswers)
                {
                    rewardStr = data.Value.GetString("rewards");
                }
            }
            if (msg.CostDiamond > 0)
            {
                player.DelCoins(CurrenciesType.diamond, msg.CostDiamond, ConsumeWay.DailyQuestion, msg.CorrectAnswers.ToString());
            }
            //player.UpdateCounter(CounterType.DailyQuestion, 1);
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(rewardStr);
            player.AddRewards(rewards, ObtainWay.DailyQuestion);
            response.Result = (int)ErrorCode.Success;
            player.Write(response);
        }
    }
}
