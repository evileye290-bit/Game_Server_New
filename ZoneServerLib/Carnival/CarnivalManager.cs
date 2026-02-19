using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Message.Zone.Protocol.ZM;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CarnivalManager
    {
        private PlayerChar owner { get; set; }
        private CarnivalRechargeInfo rechargeInfo = new CarnivalRechargeInfo();
        public CarnivalRechargeInfo RechargeInfo { get { return rechargeInfo; } }

        private Dictionary<int, Dictionary<int, CarnivalMallInfo>> mallInfoList = new Dictionary<int, Dictionary<int, CarnivalMallInfo>>();
        public Dictionary<int, Dictionary<int, CarnivalMallInfo>> MallInfoList { get { return mallInfoList; } }

        public CarnivalManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void InitRechargeInfo(CarnivalRechargeInfo rechargeInfo)
        {
            this.rechargeInfo = rechargeInfo;
        }

        public void AddCarnivalAccumulatePrice(float price)
        {
            rechargeInfo.AccumulatePrice += price;         
        }
        
        public void AddRechargeReward(int rewardId)
        {
            rechargeInfo.RechargeRewards.Add(rewardId);
        }

        public void ClearRechargeInfo()
        {
            rechargeInfo.AccumulatePrice = 0;
            rechargeInfo.RechargeRewards.Clear();
        }

        #region 特卖场
        public void InitMallInfo(List<CarnivalMallInfo> list)
        {
            Dictionary<int, CarnivalMallInfo> typeList;
            foreach (var item in list)
            {
                if (!mallInfoList.TryGetValue(item.Type, out typeList))
                {
                    typeList = new Dictionary<int, CarnivalMallInfo>();
                    mallInfoList.Add(item.Type, typeList);
                }
                typeList.Add(item.Id, item);
            }
        }

        public void BuyCarnivalMallGiftItem(CarnivalMallModel mallModel)
        {
            Dictionary<int, CarnivalMallInfo> typeList;
            mallInfoList.TryGetValue(mallModel.Type, out typeList);
            if (typeList == null)
            {
                AddNewTypeMallInfo(mallModel);               
            }
            else
            {
                CarnivalMallInfo info;
                if (typeList.TryGetValue(mallModel.Id, out info))
                {
                    info.BuyState = (int)GiftBuyState.Bought;
                    SyncDbUpdateCarnivalMallBuyState(info);
                }
                else
                {
                    info = new CarnivalMallInfo()
                    {
                        Id = mallModel.Id,
                        Type = mallModel.Type,
                        Stage = mallModel.Stage,
                        BuyState = (int)GiftBuyState.Bought
                    };
                    typeList.Add(mallModel.Id, info);
                    SyncDbInsertCarnivalMallInfo(info);
                }
            }          
        }

        private void AddNewTypeMallInfo(CarnivalMallModel mallModel)
        {
            Dictionary<int, CarnivalMallInfo> typeList = new Dictionary<int, CarnivalMallInfo>();
            CarnivalMallInfo info = new CarnivalMallInfo()
            {
                Id = mallModel.Id,
                Type = mallModel.Type,
                Stage = mallModel.Stage,
                BuyState = (int)GiftBuyState.Bought
            };
            typeList.Add(info.Id, info);
            mallInfoList.Add(info.Type, typeList);
            SyncDbInsertCarnivalMallInfo(info);
        }

        public void ClearCarnivalMallInfo()
        {
            foreach (var kv in mallInfoList)
            {
                foreach (var item in kv.Value)
                {
                    item.Value.BuyState = 0;
                }
            }
        }

        private void SyncDbInsertCarnivalMallInfo(CarnivalMallInfo info)
        {
            owner.server.GameDBPool.Call(new QueryInsertCarnivalMallInfo(owner.Uid, info.Id, info.Type, info.Stage, info.BuyState));
        }

        private void SyncDbUpdateCarnivalMallBuyState(CarnivalMallInfo info)
        {
            owner.server.GameDBPool.Call(new QueryUpdateCarnivalBuyState(owner.Uid, info.Id, info.BuyState));
        }
        #endregion

        public void LoadCarnivalInfo(MSG_ZMZ_CARNIVAL_INFO msg)
        {
            LoadCarnivalRechargeInfo(msg.RechargeInfo);
            LoadCarnivalMallInfo(msg.MallInfoList);
        }

        private void LoadCarnivalRechargeInfo(ZMZ_CARNIVAL_RECHARGE msg)
        {
            rechargeInfo.AccumulatePrice = msg.AccumulatePrice;
            rechargeInfo.RechargeRewards.AddRange(msg.RechargeRewards);
        }

        private void LoadCarnivalMallInfo(RepeatedField<ZMZ_CARNIVAL_MALL_INFO> infoList)
        {
            Dictionary<int, CarnivalMallInfo> typeList;
            foreach (var info in infoList)
            {
                if (!mallInfoList.TryGetValue(info.Type, out typeList))
                {
                    typeList = new Dictionary<int, CarnivalMallInfo>();
                    mallInfoList.Add(info.Type, typeList);
                }
                CarnivalMallInfo item = new CarnivalMallInfo()
                {
                    Id = info.Id,
                    Type = info.Type,
                    Stage = info.Stage,
                    BuyState = info.BuyState
                };
                typeList.Add(item.Id, item);
            }
        }
    }
}
