using CommonUtility;
using EnumerateUtility;
using ServerModels.Recharge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib.Recharge
{
    public class RechargeManager
    {
        public float AccumulateMoney { get; set; }
        public float AccumulatePrice { get; set; }
        public int AccumulateTotal { get; set; }
        public int AccumulateCurrent { get; set; }
        public int AccumulateDaily { get; set; }
        public int First { get; set; }
        /// <summary>
        /// 最近购买的礼包的最大额度
        /// </summary>
        public int AccumulateOnceMaxMoney { get; set; }
        /// <summary>
        /// 上次购买常规礼包的时间
        /// </summary>
        public int LastCommonRechargeTime { get; set; }

        public int MonthCardTime { get; set; }
        public int MonthCardState { get; set; }
        public int SuperMonthCardTime { get; set; }
        public int SuperMonthCardState { get; set; }
        public int SeasonCardTime { get; set; }
        public int SeasonCardState { get; set; }
        public int WeekCardStart { get; set; }
        public int WeekCardEnd { get; set; }
        public int GrowthFund { get; set; }
        public string AccumulateRechargeRewards { get; set; }
        public int PayCount { get; set; }


        public int NewRechargeGiftScore { get; set; }
        public string NewRechargeGiftRewards { get; set; }

        private RechargeHistoryItem firstOrderInfo = new RechargeHistoryItem();
        public RechargeHistoryItem FirstOrderInfo { get { return firstOrderInfo; } }
        public bool GrowthFund1()
        {
            return (GrowthFund & 1 )>0;
        }

        public bool GrowthFund2()
        {
            return (GrowthFund & 2) > 0;
        }

        public bool ClientDailyRecharge()
        {
            bool change = false;
            if (AccumulateDaily > 0)
            {
                AccumulateDaily = 0;
                change = true;
            }
            return change;
        }

        public void UpdateMonthCardState(int subType)
        {
            switch ((MonthCardType)subType)
            {
                case MonthCardType.Normal:
                    MonthCardState = 1;
                    break;
                case MonthCardType.Super:
                    SuperMonthCardState = 1;
                    break;
                default:
                    break;
            }
        }

        public void RefreshMonthCardReceiveState()
        {
            if (ZoneServerApi.now < Timestamp.TimeStampToDateTime(MonthCardTime) && MonthCardState != 0)
            {
                MonthCardState = 0;
            }
            if (ZoneServerApi.now < Timestamp.TimeStampToDateTime(SuperMonthCardTime) && SuperMonthCardState != 0)
            {
                SuperMonthCardState = 0;
            }          
        }

        public List<int> GetAccumulateRechargeRewards()
        {
            string[] gotRewards = StringSplit.GetArray("|", AccumulateRechargeRewards);
            List<int> gotRewardList = new List<int>();
            gotRewards.ForEach(x=> gotRewardList.Add(x.ToInt()));
            return gotRewardList;
        }

        public List<int> GetNewRechargeGiftRewards()
        {
            string[] gotRewards = StringSplit.GetArray("|", NewRechargeGiftRewards);
            List<int> gotRewardList = new List<int>();
            gotRewards.ForEach(x => gotRewardList.Add(x.ToInt()));
            return gotRewardList;
        }

        public void BindFirstOrderInfo(RechargeHistoryItem firstOrderInfo)
        {
            if (firstOrderInfo != null)
            {
                this.firstOrderInfo = firstOrderInfo;
            }
        }
    }
}
