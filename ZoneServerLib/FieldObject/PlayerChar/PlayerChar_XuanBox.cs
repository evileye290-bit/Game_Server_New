using System;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public XuanBoxManager XuanBoxManager { get; private set; }

        private void InitXuanBoxManager()
        {
            XuanBoxManager = new XuanBoxManager(this);
        }

        public void GetXuanBoxInfoByLoading()
        {
            if (XuanBoxManager.CheckPeriodInfo())
            {
                Write(XuanBoxManager.GenerateInfo());
            }
        }


        public void XuanBoxRandom(int num)
        {
            MSG_ZGC_XUANBOX_RANDOM response = new MSG_ZGC_XUANBOX_RANDOM();

            if (!XuanBoxManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} XuanBoxRandom failed: time is error");
                response.Result = (int)ErrorCode.XuanBoxNotOpen;
                Write(response);
                return;
            }

            int addScore = 0;
            int lucky = XuanBoxManager.Lucky;
            int oldLucky = lucky;
            RewardManager manager = new RewardManager();

            num = num == 1 ? 1 : 10;
            ItemBasicInfo costItem = num == 1 ? XuanBoxLibrary.CostOneItem : XuanBoxLibrary.CostTenItem;

            if (!CheckCoins((CurrenciesType)costItem.Id, costItem.Num))
            {
                Log.Warn($"player {Uid} XuanBoxRandom failed: currency not enough");
                response.Result = (int)ErrorCode.DiamondNotEnough;
                Write(response);
                return;
            }

            bool hadFull = false;
            for (int i = 0; i < num; i++)
            {
                bool isLucky = lucky >= XuanBoxLibrary.LuckyLimit;
                XuanBoxItemModel model = XuanBoxLibrary.RandomReward(XuanBoxManager.Period, isLucky);
                if (model == null)
                {
                    Log.Warn($"had not random a xuan box module");
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }

                if (isLucky)
                {
                    lucky = 0;
                    hadFull = true;
                }

                int score = model.RandomScore();
                addScore += score;
                lucky += score;

                lucky = Math.Min(lucky, XuanBoxLibrary.LuckyLimit);
                manager.AddSimpleReward(model.Reward);
            }

            DelCoins((CurrenciesType)costItem.Id, costItem.Num, ConsumeWay.XuanBox, "");

            manager.GenerateRewardItemInfo(response.Rewards);

            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.XuanBox);
            XuanBoxManager.AddLucky(lucky, addScore);

            response.Score = addScore;
            response.HadFull = hadFull;
            response.LuckyAdd = XuanBoxManager.Lucky > oldLucky ? XuanBoxManager.Lucky - oldLucky :
                XuanBoxManager.Lucky + XuanBoxLibrary.LuckyLimit - oldLucky;
            response.LuckyCurr = XuanBoxManager.Lucky;
            response.Result = (int)ErrorCode.Success;
            Write(response);
            
            BIRecordPointGameLog(addScore, XuanBoxManager.XuanBoxInfo.LuckySum, "xuan_box", XuanBoxManager.Period);
        }


        public void XuanBoxReward(int rewardId)
        {
            MSG_ZGC_XUANBOX_REWARD msg = new MSG_ZGC_XUANBOX_REWARD();

            if (!XuanBoxManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} XuanBoxReward failed: time is error");
                msg.Result = (int)ErrorCode.XuanBoxNotOpen;
                Write(msg);
                return;
            }

            XuanBoxScoreReward model = XuanBoxLibrary.GetXuanBoxScoreReward(rewardId);
            if (model == null || model.Period != XuanBoxManager.Period)
            {
                Log.Warn($"player {Uid} XuanBoxReward failed: reward id {rewardId}");
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            if (model.Score > XuanBoxManager.LuckySum)
            {
                Log.Warn($"player {Uid} XuanBoxReward failed: score not enough {RouletteManager.Score}");
                msg.Result = (int)ErrorCode.XuanBoxScoreNotEnough;
                Write(msg);
                return;
            }

            if (!XuanBoxManager.AddRewardId(model.Id))
            {
                Log.Warn($"player {Uid} XuanBoxReward failed: score rewarded {model.Id}");
                msg.Result = (int)ErrorCode.XuanBoxScoreRewarded;
                Write(msg);
                return;
            }

            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(model.Data.GetString("Reward"));
            AddRewards(manager, ObtainWay.XuanBox);
            manager.GenerateRewardItemInfo(msg.Rewards);

            msg.Result = (int)ErrorCode.Success;
            msg.RewardId.Add(XuanBoxManager.XuanBoxInfo.RewardList);
            Write(msg);
        }
    }
}
