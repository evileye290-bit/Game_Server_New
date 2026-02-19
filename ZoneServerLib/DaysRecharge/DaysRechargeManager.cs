using System.Collections.Generic;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class DaysRechargeManager : BasePeriodActivity
    {
        private PlayerChar owner;
        private DaysRechargeInfo dbInfo;

        public DaysRechargeManager(PlayerChar player) : base(RechargeGiftType.DaysRecharge)
        {
            owner = player;
        }

        protected override void InitPeriodInfo()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.CheckInSpecialRechargeGiftTime(RechargeGiftType.DaysRecharge, BaseApi.now, out model))
            {
                Period = model.SubType;
            }
        }

        public override bool CheckPeriodInfo()
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInSpecialRechargeGiftTime(RechargeGiftType.DaysRecharge, BaseApi.now, out model))
            {
                return false;
            }

            Period = model.SubType;
            return true;
        }

        public void Init(DaysRechargeInfo info)
        {
            dbInfo = info;

            CheckPeriodInfo();
        }

        public bool CheckDaysRecharge(RechargeItemModel itemModel)
        {
            if (!CheckPeriodInfo())
            {
                return false;
            }

            DaysRechargeModel model = DaysRechargeLibrary.GetDaysRechargeModel(Period, itemModel.Id);
            if (model == null)
            {
                return false;
            }

            return model.LastGiftId == 0 || dbInfo.RechargeId.Contains(model.LastGiftId);
        }

        public void UpdateRechargeInfo(RechargeItemModel rechargeItemModel)
        {
            var model = DaysRechargeLibrary.GetDaysRechargeModel(Period, rechargeItemModel.Id);
            if (model == null)
            {
                Log.Error($"DaysRecharge have not this recharge item model Id {rechargeItemModel.Id}");
                return;
            }
            
            dbInfo.RechargeId.Add(rechargeItemModel.Id);

            SyncInfo2Db();
            owner.SendDaysRechargeInfo();
        }


        public override void Clear()
        {
            dbInfo.Reset();
            SyncInfo2Db();
            InitPeriodInfo();
            owner.SendDaysRechargeInfo();
        }

        public MSG_ZGC_DAYS_RECHARGE_INFO GenerateInfo()
        {
            MSG_ZGC_DAYS_RECHARGE_INFO msg = new MSG_ZGC_DAYS_RECHARGE_INFO();
            msg.RechargeList.Add(dbInfo.RechargeId);
            return msg;
        }

        public MSG_ZMZ_DAYS_RECHARGE_INFO GenerateTransformInfo()
        {
            MSG_ZMZ_DAYS_RECHARGE_INFO msg = new MSG_ZMZ_DAYS_RECHARGE_INFO();
            msg.RechargeList.Add(dbInfo.RechargeId);
            return msg;
        }

        public void LoadFromTransform(MSG_ZMZ_DAYS_RECHARGE_INFO msg)
        {
            DaysRechargeInfo dbInfo = new DaysRechargeInfo();
            dbInfo.RechargeId = new List<int>(msg.RechargeList);
            Init(dbInfo);
        }

        private void SyncInfo2Db()
        {
            QueryUpdateDaysRecharge query = new QueryUpdateDaysRecharge(owner.Uid, dbInfo);
            owner.server.GameDBPool.Call(query);
        }
    }
}
