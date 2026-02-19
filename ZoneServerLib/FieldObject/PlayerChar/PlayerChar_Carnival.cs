using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
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
        public CarnivalManager CarnivalManager { get; private set; }

        public void InitCarnivalManager()
        {
            CarnivalManager = new CarnivalManager(this);
        }

        public void SendCarnivalManagerInfo()
        {
            SendCarnivaRechargeInfo();
            SendCarnivalMallInfo();
        }

        public void SendCarnivaRechargeInfo()
        {
            MSG_ZGC_CARNIVAL_RECHARGE_INFO msg = new MSG_ZGC_CARNIVAL_RECHARGE_INFO();
            msg.TotalPrice = CarnivalManager.RechargeInfo.AccumulatePrice;
            msg.RechargeRewards.AddRange(CarnivalManager.RechargeInfo.RechargeRewards);
            Write(msg);
        }
              
        public void AddCarnivalAccumulatePrice(float price)
        {
            CarnivalManager.AddCarnivalAccumulatePrice(price);
            SyncDbUpdateCarnivalAccumulatePrice();
            SendCarnivaRechargeInfo();
        }

        public void GetCarnivalRechargeReward(int rewardId)
        {
            MSG_ZGC_GET_CARNIVAL_RECHARGE_REWARD response = new MSG_ZGC_GET_CARNIVAL_RECHARGE_REWARD();
            response.Id = rewardId;

            RechargeGiftModel activityModel;
            if (!RechargeLibrary.CheckInSpecialRechargeActivityTime(RechargeGiftType.CarnivalRecharge, ZoneServerApi.now, out activityModel))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} get carnival accumulate recharge reward {rewardId} failed: activity not open yet");
                Write(response);
                return;
            }

            AccumulateRechargeReward rechargeReward = CarnivalLibrary.GetAccumulateRechargeReward(rewardId);
            if (rechargeReward == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} get carnival accumulate recharge reward {rewardId} failed: not find int xml");
                Write(response);
                return;
            }
            if (CarnivalManager.RechargeInfo.AccumulatePrice < rechargeReward.AccumulatePrice)
            {
                response.Result = (int)ErrorCode.NotReach;
                Log.Warn($"player {Uid} get carnival accumulate recharge reward {rewardId} failed: accumulate price {CarnivalManager.RechargeInfo.AccumulatePrice} not enough");
                Write(response);
                return;
            }
                    
            if (CarnivalManager.RechargeInfo.RechargeRewards.Contains(rewardId))
            {
                response.Result = (int)ErrorCode.AlreadyReceived;
                Log.Warn($"player {Uid} get carnival accumulate recharge reward {rewardId} failed: already got");
                Write(response);
                return;
            }
            CarnivalManager.AddRechargeReward(rewardId);
            SyncDbUpdateCarnivalRechargeRewards();
            //发奖 
            RewardManager manager = GetSimpleReward(rechargeReward.Rewards, ObtainWay.CarnivalRecharge);
            manager.GenerateRewardItemInfo(response.Rewards);          
                       
            response.Result = (int)ErrorCode.Success;          
            Write(response);

            //BI
            List<Dictionary<string, object>> award = ParseRewardInfoToList(manager.RewardList);
            KomoeEventLogOperationalActivity((int)activityModel.GiftType, "累充活动", "累充活动", 1, 3, GetActivityStartDays(activityModel, ZoneServerApi.now), rechargeReward.AccumulatePrice, rechargeReward.Id.ToString(), "累计充值"+ rechargeReward.AccumulatePrice + "美元", award, Name);
        }

        private void ClearCarnivalRechargeInfo()
        {
            CarnivalManager.ClearRechargeInfo();
            SendCarnivaRechargeInfo();
        }

        private void SyncDbUpdateCarnivalAccumulatePrice()
        {
            server.GameDBPool.Call(new QueryUpdateCarnivalAccumulatePrice(Uid, CarnivalManager.RechargeInfo.AccumulatePrice));
        }

        private void SyncDbUpdateCarnivalRechargeRewards()
        {
            server.GameDBPool.Call(new QueryUpdateCarnivalRechargeRewards(Uid, CarnivalManager.RechargeInfo.RechargeRewards));
        }

        #region 特卖场
        public void SendCarnivalMallInfo()
        {
            List<int> subTypeList;
            if (!RechargeLibrary.CheckInSpecialRechargeActivityTime(RechargeGiftType.CarnivalMall, ZoneServerApi.now, out subTypeList))
            {
                return;
            }
            MSG_ZGC_CARNIVAL_MALL_INFO msg = new MSG_ZGC_CARNIVAL_MALL_INFO();
            foreach (var item in CarnivalManager.MallInfoList)
            {
                if (subTypeList.Contains(item.Key))
                {
                    msg.List.Add(GenerateCarnivalMallInfo(item.Key, item.Value));
                }
            }
            Write(msg);
        }

        private CARNIVAL_MALL_INFO GenerateCarnivalMallInfo(int type, Dictionary<int, CarnivalMallInfo> list)
        {
            CARNIVAL_MALL_INFO msg = new CARNIVAL_MALL_INFO();
            msg.Type = type;
            foreach (var item in list.Values)
            {
                msg.ItemList.Add(item.Id, item.BuyState);
            }
            return msg;
        }

        public void BuyCarnivalMallGiftItem(int id)
        {
            MSG_ZGC_BUY_CARNIVAL_MALL_GIFT_ITEM response = new MSG_ZGC_BUY_CARNIVAL_MALL_GIFT_ITEM();
            response.Id = id;

            if (!CheckLimitOpen(LimitType.CarnivalMall))
            {
                response.Result = (int)ErrorCode.NotOpen;
                Log.Warn($"player {Uid} buy carnival mall giftItem {id} failed: stage one open limit");
                Write(response);
                return;
            }

            CarnivalMallModel mallModel = CarnivalLibrary.GetCarnivalMallModel(id);
            if (mallModel == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} buy carnival mall giftItem {id} failed: not find item in xml");
                Write(response);
                return;
            }

            List<int> list;
            if (!RechargeLibrary.CheckInSpecialRechargeActivityTime(RechargeGiftType.CarnivalMall, ZoneServerApi.now, out list) || !list.Contains(mallModel.Type))
            {
                response.Result = (int)ErrorCode.NotOnTime;
                Log.Warn($"player {Uid} buy carnival mall giftItem {id} failed: activity not open yet");
                Write(response);
                return;
            }

            if (mallModel.Price.Length < 3)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} buy carnival mall giftItem {id} failed: price param error");
                Write(response);
                return;
            }
            int coniType = mallModel.Price[0].ToInt();
            int costCoin = mallModel.Price[2].ToInt();
            int coins = GetCoins((CurrenciesType)coniType);
            if (coins < costCoin)
            {
                response.Result = (int)ErrorCode.NoCoin;
                Log.Warn($"player {Uid} buy carnival mall giftItem {id} failed: not have enough coins");
                Write(response);
                return;
            }

            if (!CheckCanBuyCarnivalMallGiftItem(mallModel))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} buy carnival mall giftItem {id} failed: can not buy");
                Write(response);
                return;
            }

            //更新礼包信息
            CarnivalManager.BuyCarnivalMallGiftItem(mallModel);

            //扣钱
            DelCoins((CurrenciesType)coniType, costCoin, ConsumeWay.BuyCarnivalMallGift, id.ToString());
            
            //发奖
            RewardManager manager = GetSimpleReward(mallModel.Rewards, ObtainWay.BuyCarnivalMallGift, 1, id.ToString());
            manager.GenerateRewardItemInfo(response.Rewards);
            response.Result = (int)ErrorCode.Success;
            response.BuyState = (int)GiftBuyState.Bought;
            Write(response);
        }

        private bool CheckCanBuyCarnivalMallGiftItem(CarnivalMallModel mallModel)
        {
            Dictionary<int, CarnivalMallInfo> dic;
            CarnivalManager.MallInfoList.TryGetValue(mallModel.Type, out dic);
            if (dic == null)
            {
                if (mallModel.Stage != 1)
                {
                    return false;
                }
                return true;
            }
            CarnivalMallInfo info;
            dic.TryGetValue(mallModel.Id, out info);
            if (info != null)
            {
                if (info.BuyState == (int)GiftBuyState.Bought)
                {
                    return false;
                }
                if (mallModel.Stage == 2)
                {
                    CarnivalMallInfo stageOneInfo = dic.Values.Where(x => x.Stage == 1).ToList().First();
                    if (stageOneInfo == null || stageOneInfo.BuyState != (int)GiftBuyState.Bought)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void ClearCarnivalMallInfo()
        {
            CarnivalManager.ClearCarnivalMallInfo();
            SendCarnivalMallInfo();
        }
        #endregion

        public MSG_ZMZ_CARNIVAL_INFO GenerateCarnivalTransformMsg()
        {
            MSG_ZMZ_CARNIVAL_INFO msg = new MSG_ZMZ_CARNIVAL_INFO();
            msg.RechargeInfo = GenerateCarnivalRechargeInfo();
            GenerateCarnivalMallInfo(msg.MallInfoList);
            return msg;
        }

        private ZMZ_CARNIVAL_RECHARGE GenerateCarnivalRechargeInfo()
        {
            ZMZ_CARNIVAL_RECHARGE msg = new ZMZ_CARNIVAL_RECHARGE();
            msg.AccumulatePrice = CarnivalManager.RechargeInfo.AccumulatePrice;
            msg.RechargeRewards.AddRange(CarnivalManager.RechargeInfo.RechargeRewards);
            return msg;
        }

        private void GenerateCarnivalMallInfo(RepeatedField<ZMZ_CARNIVAL_MALL_INFO> infoList)
        {
            foreach (var item in CarnivalManager.MallInfoList)
            {
                foreach (var kv in item.Value)
                {
                    ZMZ_CARNIVAL_MALL_INFO info = new ZMZ_CARNIVAL_MALL_INFO()
                    {
                        Id = kv.Value.Id,
                        Type = kv.Value.Type,
                        Stage = kv.Value.Stage,
                        BuyState = kv.Value.BuyState
                    };
                    infoList.Add(info);
                }
            }
        }

        public void LoadCarnivalTransform(MSG_ZMZ_CARNIVAL_INFO info)
        {
            CarnivalManager.LoadCarnivalInfo(info);
        }
    }
}
