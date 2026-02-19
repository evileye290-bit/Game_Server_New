using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerShared;
using ServerModels;
using CommonUtility;
using DataProperty;
using System.Linq;
using System.Collections.Generic;
using Logger;
using ServerFrame;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        private TridentManager tridentManager;
        public TridentManager TridentManager => tridentManager;

        private void InitTridentManager()
        {
            tridentManager = new TridentManager(this);
        }

        private void UpdateTridentInfo(RechargeItemModel recharge, string reward)
        {
            GetTridentShovel();
            tridentManager.UpdateRechargeInfo(recharge);
            SendBuyRechargeGiftInfo(recharge.Id, reward);
        }

        private void GetTridentShovel()
        {
            tridentManager.GetTridentShovel();
        }

        public bool CheckTridentRechargeLegal(RechargeItemModel itemModel)
        {
            RechargeGiftModel rechargeGiftModel;
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.Trident, BaseApi.now, out rechargeGiftModel))
            {
                return false;
            }
            tridentManager.SetPeriod(rechargeGiftModel.SubType);

            return itemModel.Id == TridentLibrary.GetNextRechargeId(tridentManager.Period, tridentManager.TridentDbInfo.PullTotalNum);
        }

        public void SendTridentInfo()
        {
            MSG_ZGC_TRIDENT_INFO msg = tridentManager.GenerateTridentInfo();
            Write(msg);
        }

        public void TridentReward(int type, int rewardId)
        {
            MSG_ZGC_TRIDENT_REWARD msg = new MSG_ZGC_TRIDENT_REWARD();

            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.Trident, ZoneServerApi.now))
            {
                msg.ErrorCode = (int)ErrorCode.TridentNotOpen;
                Write(msg);
                return;
            }

            TridentDbInfo info = tridentManager.TridentDbInfo;
            string rewardStr = "";
             switch (type)
            {
                case 1: //解锁奖励
                    {
                        if (tridentManager.TridentDbInfo.IsTierReward)
                        {
                            Log.Warn($"TridentReward had got unlock reward {info.Tier}");
                            msg.ErrorCode = (int) ErrorCode.TridentRewarded;
                            Write(msg);
                            return;
                        }

                        TridentTierRewardModel model = TridentLibrary.GetTridentTierRewardModel(TridentManager.Period, info.Tier);
                        if (model == null)
                        {
                            Log.Warn($"TridentReward had got unlock reward error tire {info.Tier}");
                            msg.ErrorCode = (int) ErrorCode.TridentHaveNoThisReward;
                            Write(msg);
                            return;
                        }

                        info.IsTierReward = true;
                        rewardStr = model.UnlockReward;
                        TridentManager.GotUnlockReward();
                    }
                    break;
                case 2:
                    {
                        if (info.TotalGotRewardList.Contains(rewardId))
                        {
                            Log.Warn($"TridentReward had got reward {info.Tier} rewardId {rewardId}");
                            msg.ErrorCode = (int) ErrorCode.TridentRewarded;
                            Write(msg);
                            return;
                        }

                        TridentTotalRewardModel model = TridentLibrary.GetTridentTotalRewardModel(TridentManager.Period, rewardId);
                        if (model == null)
                        {
                            Log.Warn($"TridentReward had got unlock reward error rewardId {rewardId}");
                            msg.ErrorCode = (int) ErrorCode.Fail;
                            Write(msg);
                            return;
                        }

                        rewardStr = model.Reward;
                        TridentManager.GotTotalReward(rewardId);
                    }
                    break;
                default:
                    msg.ErrorCode = (int)ErrorCode.Fail;
                    Write(msg);
                    return;
            }

             RewardManager rewardManager = new RewardManager();
             rewardManager.AddSimpleReward(rewardStr);
             rewardManager.BreakupRewards();

             AddRewards(rewardManager, ObtainWay.Trident);

            rewardManager.GenerateRewardItemInfo(msg.RewardList);

            SendTridentInfo();

            msg.Type = type;
            msg.ErrorCode = (int) ErrorCode.Success;
            Write(msg);
        }

        public void TridentUseShovel(int giftItemId)
        {
            MSG_ZGC_TRIDENT_USE_SHOVEL msg = new MSG_ZGC_TRIDENT_USE_SHOVEL();

            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.Trident, ZoneServerApi.now))
            {
                msg.ErrorCode = (int)ErrorCode.TridentNotOpen;
                Write(msg);
                return;
            }
            
            int nextGiftId = TridentLibrary.GetNextRechargeId(tridentManager.Period, tridentManager.TridentDbInfo.PullTotalNum);
            if (giftItemId != nextGiftId)
            {
                msg.ErrorCode = (int)ErrorCode.TridentShovelRechargeIdError;
                Write(msg);
                return;
            }

            TridentDbInfo info = tridentManager.TridentDbInfo;
            if (info.ShovelNum <= 0)
            {
                msg.ErrorCode = (int) ErrorCode.TridentShovelNotEnough;
                Write(msg);
                return;
            }

            
            //获取礼包奖励
            RechargeItemModel recharge = RechargeLibrary.GetRechargeItem(giftItemId);
            if (recharge == null)
            {
                //没有找到产品ID
                msg.ErrorCode = (int) ErrorCode.TridentShovelNotFindGift;
                Log.ErrorLine($"player {Uid} TridentUseShovel productId {giftItemId} error: not find {giftItemId} item model");
                return;
            }
            if (RechargeLibrary.CheckIsRechargeReward(recharge.GiftType, recharge.Id))
            {
                msg.ErrorCode = (int) ErrorCode.TridentShovelRechargeReward;
                Log.ErrorLine($"player {Uid} TridentUseShovel productId {giftItemId} error: item is recharge reward can not buy");
                return;
            }
            string rewardStr = recharge.Reward;
            
            //扣除铲子数量
            info.ShovelNum--;
            
            GetTridentShovel();

            RewardManager rewardManager = new RewardManager();
            rewardManager.AddSimpleReward(rewardStr);
            rewardManager.BreakupRewards();

            AddRewards(rewardManager, ObtainWay.Trident);

            //处理后续逻辑

            rewardManager.GenerateRewardItemInfo(msg.RewardList);
            tridentManager.UpdateRechargeInfo(recharge);

            msg.ErrorCode = (int) ErrorCode.Success;
            Write(msg);
        }
    }
}
