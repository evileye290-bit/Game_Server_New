using CommonUtility;
using DataProperty;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public OnhookManager OnhookManager { get; private set; }

        public void InitOnhook()
        {
            OnhookManager = new OnhookManager(this);
        }

        public int GetOnhookTime()
        {
            return (int)(DateTime.Now - OnhookManager.LastRewardTime).TotalSeconds;
        }

        public int GetFastRewardCount()
        {
            int count = GetCounterRestCount(CounterType.OnhookCount, CounterType.OnhookBuyCount);

            return count;
        }

        public bool IsOnHookOpening()
        {
            return CheckLimitOpen(LimitType.Onhook);
        }

        public void ChecnkAndOnhookOpen()
        {
            //首次开启
            if (OnhookManager.TierId > 0) return;

            if(IsOnHookOpening())
            {
                OnhookManager.FirstOpen();
            }

            SendOnhookInfo();
        }

        public void GetOnhookInfo()
        {
            SendOnhookInfo(true);
        }

        public void SendOnhookInfo(bool caculate = false)
        {
            RewardManager manager;
            if (caculate)
            {
                manager = OnhookManager.LookReward();
            }
            else
            {
                manager = new RewardManager();
                if (!string.IsNullOrEmpty(OnhookManager.Reward))
                {
                    manager.InitSimpleReward(OnhookManager.Reward);
                }
            }

            MSG_ZGC_ONHOOK_INFO msg = new MSG_ZGC_ONHOOK_INFO
            {
                Result = (int)ErrorCode.Success,
                TierId = OnhookManager.TierId,
                OnhookTime = OnhookManager.GetOnhookTime(),
            };

            List<MSG_ZGC_ONHOOK_INFO> msgList = new List<MSG_ZGC_ONHOOK_INFO>();

            foreach (var item in manager.AllRewards)
            {
                REWARD_ITEM_INFO reward = new REWARD_ITEM_INFO();
                reward.MainType = item.RewardType;
                reward.TypeId = item.Id;
                reward.Num = item.Num;
                if (item.Attrs != null)
                {
                    foreach (var attr in item.Attrs)
                    {
                        reward.Param.Add(attr);
                    }
                }
                msg.Rewards.Add(reward);

                //拆包逻辑
                if (msg.Rewards.Count >= 50)
                {
                    msgList.Add(msg);

                    msg = new MSG_ZGC_ONHOOK_INFO()
                    {
                        Result = (int)ErrorCode.Success,
                        TierId = OnhookManager.TierId,
                        OnhookTime = OnhookManager.GetOnhookTime(),
                    };
                }
            }
            msg.Result = (int)ErrorCode.Success;

            //设置随后一个包的标识
            msg.IsTheLastOne = true;
            msgList.Add(msg);
            msgList.ForEach(x => Write(x));
        }

        public void GetOnhookReward(int type)
        {
            MSG_ZGC_ONHOOK_GET_REWARD msg = new MSG_ZGC_ONHOOK_GET_REWARD();
            msg.RewardType = type;
            int costCoin = 0;

            RewardManager manager = null;
            if (type == 1)
            {
                manager = OnhookManager.ResetReward(true);
            }
            else
            {
                if (!CheckLimitOpen(LimitType.OnhookFastReward))
                {
                    Log.Warn($"player {Uid} get onhook reward failed: not open");
                    msg.RewardType = type;
                    msg.Result = (int)ErrorCode.OnhookRewardLimit;
                    Write(msg);
                    return;
                }

                if (!CheckCounter(CounterType.OnhookCount))
                {
                    //快速战斗领取奖励
                    manager = OnhookManager.GetFastReward();

                    UpdateCounter(CounterType.OnhookCount, 1);
                }
                else
                {
                    Data buyData = DataListManager.inst.GetData("Counter", (int)CounterType.OnhookBuyCount);
                    if (buyData == null)
                    {
                        Log.Warn($"player {Uid} get onhook reward failed: not find onhook buy count");
                        msg.Result = (int)ErrorCode.Fail;
                        Write(msg);
                        return;
                    }

                    int buyedCount = GetCounterValue(CounterType.OnhookBuyCount);

                    if (buyedCount + 1 > buyData.GetInt("MaxCount"))
                    {
                        Log.Warn($"player {Uid} get onhook reward failed: onhook buy count already max");
                        msg.Result = (int)ErrorCode.MaxBuyCount;
                        Write(msg);
                        return;
                    }

                    string costStr = buyData.GetString("Price");
                    if (string.IsNullOrEmpty(costStr))
                    {
                        Log.Warn($"player {Uid} get onhook reward failed: not find price");
                        msg.Result = (int)ErrorCode.Fail;
                        Write(msg);
                        return;
                    }

                    int cost = CounterLibrary.GetBuyCountCost(costStr, buyedCount + 1);
                    if (!CheckCoins(CurrenciesType.diamond, cost))
                    {
                        Log.Warn($"player {Uid} get onhook reward failed: coin not enough, curCoin {GetCoins(CurrenciesType.diamond)} cost {cost}");
                        msg.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(msg);
                        return;
                    }
                    costCoin = cost;

                    DelCoins(CurrenciesType.diamond, cost, ConsumeWay.OnHook, buyedCount.ToString());

                    UpdateCounter(CounterType.OnhookBuyCount, 1);

                    //快速战斗领取奖励
                    manager = OnhookManager.GetFastReward();
                }
                AddPassCardTaskNum(TaskType.OnhookQuick);
                AddRunawayActivityNumForType(RunawayAction.Onhook);
                AddSchoolTaskNum(TaskType.OnhookQuick);
                AddDriftExploreTaskNum(TaskType.OnhookQuick);
            }
            manager.MergeRewards();

            manager.BreakupRewards();

            AddRewards(manager, ObtainWay.Onhook);

            List<MSG_ZGC_ONHOOK_GET_REWARD> msgList = new List<MSG_ZGC_ONHOOK_GET_REWARD>();
            foreach (var item in manager.AllRewards)
            {
                REWARD_ITEM_INFO reward = new REWARD_ITEM_INFO();
                reward.MainType = item.RewardType;
                reward.TypeId = item.Id;
                reward.Num = item.Num;
                if (item.Attrs != null)
                {
                    foreach (var attr in item.Attrs)
                    {
                        reward.Param.Add(attr);
                    }
                }
                msg.Rewards.Add(reward);

                //拆包逻辑
                if (msg.Rewards.Count >= 50)
                {
                    msg.Result = (int)ErrorCode.Success;
                    msgList.Add(msg);

                    msg = new MSG_ZGC_ONHOOK_GET_REWARD();
                    msg.RewardType = type;
                }
            }

            //komoelog
            Dictionary<CurrenciesType, int> costCoinDic = null;
            if (costCoin > 0)
            {
                costCoinDic = new Dictionary<CurrenciesType, int>();
                costCoinDic.Add(CurrenciesType.diamond, costCoin);
            }
            KomoeLogRecordPveFight(7, 4, type.ToString(), manager.RewardList, 1, 0, costCoinDic);

            msg.Result = (int)ErrorCode.Success;

            //设置随后一个包的标识
            msg.IsTheLastOne = true;
            msgList.Add(msg);
            msgList.ForEach(x => Write(x));
        }

        public void UseItemGetOnhookReward(NormalItem item, int num, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            RewardManager manager = OnhookManager.GetOnhookReward(item);
            if (manager.AllRewards.Count == 0)
            {
                return;
            }
            //月卡，全服贡献增加奖励百分比
            MonthCardUpOnhookRewards(manager);
            manager *= num;
         
            manager.BreakupRewards(true);
            AddRewards(manager, ObtainWay.ItemUse);
            manager.GenerateRewardItemInfo(rewards);
        }
    }
}
