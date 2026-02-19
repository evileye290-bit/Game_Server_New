using EnumerateUtility;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        private DaysRechargeManager daysRechargeManager;
        public DaysRechargeManager DaysRechargeManager => daysRechargeManager;

        private void InitDaysRechargeManager()
        {
            daysRechargeManager = new DaysRechargeManager(this);
        }

        public void SendDaysRechargeInfo()
        {
            var msg = daysRechargeManager.GenerateInfo();
            Write(msg);
        }
    }
}
