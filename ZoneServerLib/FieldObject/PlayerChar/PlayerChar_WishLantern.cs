using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using ServerModels;
using ServerModels.WishLantern;
using ServerShared;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public WishLanternManager WishLanternManager { get; private set; }

        private void InitWishLanternManager()
        {
            WishLanternManager = new WishLanternManager(this);
        }

        public void GetWishLanternInfo()
        {
            if (WishLanternManager.CheckPeriodInfo())
            {
                SendWishLanternInfo();
            }
        }

        public void SendWishLanternInfo()
        {
            Write(WishLanternManager.GenerateInfo());
        }

        public void WishLanternSelectReward(int index)
        {
            MSG_ZGC_WISH_LANTERN_SELECT_REWARD response = new MSG_ZGC_WISH_LANTERN_SELECT_REWARD();

            if (!WishLanternManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} WishLanternSelectReward failed: time is error");
                response.Result = (int)ErrorCode.WishLanternNotOpen;
                Write(response);
                return;
            }

            WishLanternInfo info = WishLanternManager.WishLanternInfo;

            if (WishLanternManager.CheckGotAllReward())
            {
                Log.Warn($"player {Uid} WishLanternSelectReward failed: got all reward");
                response.Result = (int)ErrorCode.WishLanternGotAllReward;
                Write(response);
                return;
            }

            if (info.BoxIndex == index || info.RewardBoxIndex.Contains(index))
            {
                Log.Warn($"player {Uid} WishLanternSelectReward failed: had select {index} reward");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            WishLanternBoxItemModel boxItemModel = WishLanternLibrary.GetLanternBoxItemModel(WishLanternManager.Period, index);
            if(boxItemModel == null)
            {
                Log.Warn($"player {Uid} WishLanternSelectReward failed: had not god {index} reward");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            WishLanternManager.SetSelectReward(index);
            SendWishLanternInfo();

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }


        public void WishLanternLight(int index)
        {
            MSG_ZGC_WISH_LANTERN_LIGHT msg = new MSG_ZGC_WISH_LANTERN_LIGHT();
            msg.LightIndex = index;

            if (!WishLanternManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} WishLanternLight failed: time is error");
                msg.Result = (int)ErrorCode.WishLanternNotOpen;
                Write(msg);
                return;
            }

            int period = WishLanternManager.Period;
            WishLanternInfo info = WishLanternManager.WishLanternInfo;

            WishLanternCostModel costModel = WishLanternLibrary.GetLanternCostModel(info.LanternIndex.Count + 1);
            if (costModel == null)
            {
                Log.Warn($"player {Uid} WishLanternLight failed: had not got cost count {info.LanternIndex.Count + 1}");
                msg.Result = (int)ErrorCode.WishLanternNotOpen;
                Write(msg);
                return;
            }

            if (!CheckCoins(CurrenciesType.diamond, costModel.Diamond))
            {
                Log.Warn($"player {Uid} WishLanternLight failed: diamond not enough");
                msg.Result = (int)ErrorCode.DiamondNotEnough;
                Write(msg);
                return;
            }

            WishLanternBoxItemModel boxItemModel = WishLanternLibrary.GetLanternBoxItemModel(period, info.BoxIndex);
            if (boxItemModel == null)
            {
                Log.Warn($"player {Uid} WishLanternLight failed: had not select reward");
                msg.Result = (int)ErrorCode.WishLanternHadNotSelectReward;
                Write(msg);
                return;
            }

            WishLanternItemModel itemModel = WishLanternLibrary.RandomItemModel(period);
            if (itemModel == null)
            {
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }

            bool success = RAND.Range(0, 10000) <= costModel.Ratio;

            DelCoins(CurrenciesType.diamond, costModel.Diamond, ConsumeWay.WishLantern, "");

            RewardManager manager = new RewardManager();
            manager.AddSimpleReward(itemModel.Reward);


            if (success)
            {
                WishLanternManager.AddLanternLightIndex(index, false);

                manager.AddSimpleReward(boxItemModel.Reward);

                msg.Result = (int)ErrorCode.Success;
                msg.HadReset = WishLanternManager.AddBoxRewardIndex(boxItemModel.Index);
                WishLanternManager.Reset(0,true, false);
            }
            else
            {
                WishLanternManager.AddLanternLightIndex(index, true);
                msg.Result = (int)ErrorCode.WishLanternLightFail;
            }

            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.WishLantern);
            manager.GenerateRewardMsg(msg.Rewards);

            SendWishLanternInfo();

            Write(msg);
        }

        public void WishLanternReset()
        {
            MSG_ZGC_WISH_LANTERN_RESET msg = new MSG_ZGC_WISH_LANTERN_RESET();

            if (!WishLanternManager.CheckPeriodInfo())
            {
                Log.Warn($"player {Uid} WishLanternReset failed: time is error");
                msg.Result = (int)ErrorCode.WishLanternNotOpen;
                Write(msg);
                return;
            }

            int resetCount = WishLanternLibrary.GetBoxItemCount(WishLanternManager.Period) - WishLanternManager.WishLanternInfo.RewardBoxIndex.Count; 
            int costDiamond = WishLanternLibrary.GetResetCost(resetCount);
            if (!CheckCoins(CurrenciesType.diamond, costDiamond))
            {
                Log.Warn($"player {Uid} WishLanternReset failed: diamond not enough");
                msg.Result = (int)ErrorCode.DiamondNotEnough;
                Write(msg);
                return;
            }

            DelCoins(CurrenciesType.diamond, costDiamond, ConsumeWay.WishLantern, "");

            WishLanternManager.Reset(1,true);
            SendWishLanternInfo();

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }
    }
}
