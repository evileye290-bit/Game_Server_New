using EnumerateUtility;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class BasePeriodActivity
    {
        private RechargeGiftType rechargeGiftType { get; set; }
        public int Period { get; protected set; }

        protected BasePeriodActivity(RechargeGiftType rechargeGiftType)
        {
            this.rechargeGiftType = rechargeGiftType;
        }

        protected virtual void InitPeriodInfo()
        {
            RechargeGiftModel model;
            if (RechargeLibrary.InitRechargeGiftTime(rechargeGiftType, BaseApi.now, out model))
            {
                Period = model.SubType;
            }
        }

        public virtual bool CheckPeriodInfo()
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInRechargeGiftTime(rechargeGiftType, BaseApi.now, out model))
            {
                return false;
            }

            Period = model.SubType;
            return true;
        }

        public void SetPeriod(int period)
        {
            Period = period;
        }

        public virtual void Clear()
        {
        }
    }
}