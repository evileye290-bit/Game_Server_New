using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
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
        private DiamondRebateInfo info;       

        public void InitDiamondRebateInfo(DiamondRebateInfo info)
        {
            this.info = info;
        }

        public void CheckSendDiamondRebateInfo()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.DiamondRebate, ZoneServerApi.now, out model))
            {
                SendDiamondRebateInfo(model.SubType);
            }
        }

        public void SendDiamondRebateInfo(int period)
        {
            MSG_ZGC_DIAMOND_REBATE_INFO msg = new MSG_ZGC_DIAMOND_REBATE_INFO();
            msg.Period = period;
            msg.ConsumeDiamond = info.ConsumeDiamond;
            msg.GetList.AddRange(info.GetList);
            Write(msg);
        }

        public void GetDiamondRebateRewards(int id)
        {
            MSG_ZGC_GET_DIAMOND_REBATE_REWARDS response = new MSG_ZGC_GET_DIAMOND_REBATE_REWARDS();
            response.Id = id;

            //检查是否活动开启
            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.DiamondRebate, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} GetDiamondRebateRewards failed: activity not open");
                Write(response);
                return;
            }

            int period = activityModel.SubType;

            DiamondRebateModel rebateModel = ShrekInvitationLibrary.GetDiamondRebateModel(period, id);
            if (rebateModel == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} GetDiamondRebateRewards failed: not find id {id}");
                Write(response);
                return;
            }

            if (info.GetList.Contains(id))
            {
                response.Result = (int)ErrorCode.AlreadyGot;
                Log.Warn($"player {Uid} GetDiamondRebateRewards failed: alreadt got reward {id}");
                Write(response);
                return;
            }

            if (info.ConsumeDiamond < rebateModel.DiamondLimit)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} GetDiamondRebateRewards failed: reward {id} need cost {rebateModel.DiamondLimit} real cost {info.ConsumeDiamond}");
                Write(response);
                return;
            }

            info.GetList.Add(id);
            SyncDbUpdateDiamondRebateInfo();

            if (!string.IsNullOrEmpty(rebateModel.Rewards))
            {
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rebateModel.Rewards);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.DiamondRebate);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void CheckAddDiamondRebateConsume(CurrenciesType type, int consumeCoins, ConsumeWay way)
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.DiamondRebate, ZoneServerApi.now, out model))
            {
                return;
            }
            List<int> ignoreWays = ShrekInvitationLibrary.GetDiamondRebateIgnoreWays(model.SubType);
            if (ignoreWays == null || !ignoreWays.Contains((int)way))
            {
                info.ConsumeDiamond += consumeCoins;
                
                SyncDbUpdateDiamondRebateInfo();
                
                SendDiamondRebateInfo(model.SubType);
            }
        }

        public void ClearDiamondRebateInfo()
        {
            info.Clear();
            CheckSendDiamondRebateInfo();
        }

        private void SyncDbUpdateDiamondRebateInfo()
        {
            server.GameDBPool.Call(new QueryUpdateDiamondRebateInfo(Uid, info.ConsumeDiamond, info.GetList));
        }

        public MSG_ZMZ_DIAMOND_REBATE_INFO GenerateDiamondRebateTransformMsg()
        {
            MSG_ZMZ_DIAMOND_REBATE_INFO msg = new MSG_ZMZ_DIAMOND_REBATE_INFO();
            msg.ConsumeDiamond = info.ConsumeDiamond;
            msg.GetList.AddRange(info.GetList);
            return msg;
        }
        
        public void LoadDiamondRebateTransformMsg(MSG_ZMZ_DIAMOND_REBATE_INFO msg)
        {
            info = new DiamondRebateInfo();
            info.ConsumeDiamond = msg.ConsumeDiamond;
            info.GetList.AddRange(msg.GetList);
        }
    }
}
