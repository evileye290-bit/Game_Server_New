using EnumerateUtility;
using System;
using System.Collections.Generic;

namespace AnalysisServerLib
{
    public class RecommendGiftManager
    {
        private DateTime lastUpdateDate = new DateTime(2020, 1, 1);
        private Dictionary<int, Dictionary<TimingGiftType, int>> playerTygftCount = new Dictionary<int, Dictionary<TimingGiftType, int>>();


        public void Update()
        {
            DateTime curDate = AnalysisServerApi.now.Date;
            if (lastUpdateDate != curDate)
            {
                Clear();
                lastUpdateDate = curDate;
            }
        }

        private void Clear()
        {
            playerTygftCount.Clear();
        }

        public void AddPlayerTimingGiftBuyedCount(int uid, TimingGiftType type)
        {
            Dictionary<TimingGiftType, int> giftCount;
            if (!playerTygftCount.TryGetValue(uid, out giftCount))
            {
                giftCount = new Dictionary<TimingGiftType, int>();
                playerTygftCount.Add(uid, giftCount);
            }

            if (giftCount.ContainsKey(type))
            {
                giftCount[type] += 1;
            }
            else
            { 
                giftCount[type] = 1;
            }
        }

        public Dictionary<TimingGiftType, int> GetPlayerTimingGiftBuyedCount(int uid)
        {
            Dictionary<TimingGiftType, int> giftCount;
            playerTygftCount.TryGetValue(uid, out giftCount);
            return giftCount;
        }
    }
}
