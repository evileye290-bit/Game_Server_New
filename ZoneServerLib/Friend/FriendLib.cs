using CommonUtility;
using DataProperty;
using EnumerateUtility.Timing;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public static class FriendLib
    {
        /// <summary>
        /// 好友列表个数上限
        /// </summary>
        public static int FRIEND_LIST_MAX_COUNT;
        /// <summary>
        /// 好友申请列表个数上限
        /// </summary>
        public static int FRIEND_REQUEST_LIST_MAX_COUNT;
        /// <summary>
        /// 黑名单列表
        /// </summary>
        public static int BLACK_LIST_MAX_COUNT;
        /// <summary>
        /// 最近联系人列表
        /// </summary>
        public static int RECENT_LIST_MAX_COUNT;

        /// <summary>
        /// 推荐数
        /// </summary>
        public static int RECOMMEND_COUNT;

        public static int RECOMMEND_LEVEL_MIN;
        public static int RECOMMEND_LEVEL_MAX;


        /// <summary>
        /// 挑战请求等待时间
        /// </summary>
        public static int ChallengeRequsetWaitTime;

        /// <summary>
        /// 好友邀请列表长度
        /// </summary>
        public static int FRIEND_INVITER_LIST_MAXCNT;

        /// <summary>
        /// 好友送心，系统消息Id
        /// </summary>
        public static int TakeHeartPersonSystemMsgId;
        /// <summary>
        /// 送心数量上限
        /// </summary>
        public static int GiveHeartMaxCnt;
        /// <summary>
        /// 收心数量上限
        /// </summary>
        public static int TakeHeartMaxCnt;



        /// <summary>
        /// 送心间隔增长 时间 单位：分钟
        /// </summary>
        public static TimeSpan GiveHeartIntervalGrowthTime;
        /// <summary>
        /// 收心间隔增长 时间 单位：分钟
        /// </summary>
        public static TimeSpan TakeHeartIntervalGrowthTime;
        /// <summary>
        /// 送心间隔增长 数目
        /// </summary>
        public static int GiveHeartIntervalGrowthCnt;
        /// <summary>
        /// 收心间隔增长 数目
        /// </summary>
        public static int TakeHeartIntervalGrowthCnt;
        /// <summary>
        /// 送心 数定时更新 时间
        /// </summary>
        public static List<TimeSpan> GiveHeartTimingGrowthTimes = new List<TimeSpan>();
        /// <summary>
        /// 收心数 定时更新 时间
        /// </summary>
        public static List<TimeSpan> TakeHeartTimingGrowthTimes = new List<TimeSpan>();

        /// <summary>
        /// 送心定时增长 数
        /// </summary>
        public static int GiveHeartTimingGrowthCnt;
        /// <summary>
        /// 收心定时增长 数
        /// </summary>
        public static int TakeHeartTimingGrowthCnt;

        public static int TakeHeartCntBuyCost;

        public static int GiveHeartCntBuyCost;

        public static int TakeHeartCntBuyCnt;
        public static int GiveHeartCntBuyCnt;

        public static Dictionary<int, int> FriendGift = new Dictionary<int, int>();

        public static int GetGiftScore(int id)
        {
            int score;
            FriendGift.TryGetValue(id, out score);
            return score;
        }

        public static void LoadDatas()
        {

            //FriendGift.Clear();
            //GiveHeartTimingGrowthTimes.Clear();
            //TakeHeartTimingGrowthTimes.Clear();

            Dictionary<int, int> FriendGift = new Dictionary<int, int>();
            List<TimeSpan> GiveHeartTimingGrowthTimes = new List<TimeSpan>();
            List<TimeSpan> TakeHeartTimingGrowthTimes = new List<TimeSpan>();

            // Init FriendConfig
            Data friendConfig = DataListManager.inst.GetData("FriendConfig", 1);
            FRIEND_LIST_MAX_COUNT = friendConfig.GetInt("FriendListMaxCnt");
            FRIEND_REQUEST_LIST_MAX_COUNT = friendConfig.GetInt("FriendRequestListMaxCnt");
            BLACK_LIST_MAX_COUNT = friendConfig.GetInt("BlackListMaxCnt");
            RECENT_LIST_MAX_COUNT = friendConfig.GetInt("RecentListMaxCnt");
            RECOMMEND_COUNT = friendConfig.GetInt("RecommendCount");
            RECOMMEND_LEVEL_MIN = friendConfig.GetInt("RecommendLevelMin");
            RECOMMEND_LEVEL_MAX = friendConfig.GetInt("RecommendLevelMax");

            FRIEND_INVITER_LIST_MAXCNT = friendConfig.GetInt("FriendInviterListMaxCnt");

            ChallengeRequsetWaitTime = friendConfig.GetInt("BattleWaitTime");


            //Init FriendlyHeart
            Data friendlyHeart = DataListManager.inst.GetData("FriendlyHeart", 1);

            TakeHeartPersonSystemMsgId = friendlyHeart.GetInt("TakeHeartPersonSystemMsgId");

            GiveHeartMaxCnt = friendlyHeart.GetInt("GiveHeartMaxCnt");
            TakeHeartMaxCnt = friendlyHeart.GetInt("TakeHeartMaxCnt");

            GiveHeartTimingGrowthCnt = friendlyHeart.GetInt("GiveHeartTimingGrowthCnt");
            TakeHeartTimingGrowthCnt = friendlyHeart.GetInt("TakeHeartTimingGrowthCnt");

            GiveHeartIntervalGrowthTime = TimeSpan.FromMinutes(friendlyHeart.GetInt("GiveHeartIntervalGrowthTime"));
            TakeHeartIntervalGrowthTime = TimeSpan.FromMinutes(friendlyHeart.GetInt("TakeHeartIntervalGrowthTime"));

            GiveHeartIntervalGrowthCnt = friendlyHeart.GetInt("GiveHeartIntervalGrowthCnt");
            TakeHeartIntervalGrowthCnt = friendlyHeart.GetInt("TakeHeartIntervalGrowthCnt");


            string strCntBuy = friendlyHeart.GetString("TakeHeartCntBuy");

            string[] list = StringSplit.GetArray("|", strCntBuy);
            if (list.Length > 1)
            {
                TakeHeartCntBuyCost = int.Parse(list[0]);
                TakeHeartCntBuyCnt = int.Parse(list[1]);
            }

            strCntBuy = friendlyHeart.GetString("GiveHeartCntBuy");

            list = StringSplit.GetArray("|", strCntBuy);
            if (list.Length > 1)
            {
                GiveHeartCntBuyCost = int.Parse(list[0]);
                GiveHeartCntBuyCnt = int.Parse(list[1]);
            }


            /////////////////DailyTiming

            Data dailyTiming = DataListManager.inst.GetData("DailyTiming", (int)(TimingType.FriendlyHeartTiming));
            string times = dailyTiming.GetString("Timing");
            string[] timeList = StringSplit.GetArray("|", times);
            foreach (var time in timeList)
            {
                if (!string.IsNullOrEmpty(time))
                {
                    TimeSpan timeSpan = TimeSpan.Parse(time);
                    GiveHeartTimingGrowthTimes.Add(timeSpan);
                    TakeHeartTimingGrowthTimes.Add(timeSpan);
                }
            }


            DataList sendGift = DataListManager.inst.GetDataList("FriendGift");

            foreach (var item in sendGift)
            {
                int score = item.Value.GetInt("Score");
                FriendGift.Add(item.Value.ID, score);
            }

            FriendLib.FriendGift = FriendGift;
            FriendLib.GiveHeartTimingGrowthTimes = GiveHeartTimingGrowthTimes;
            FriendLib.TakeHeartTimingGrowthTimes = TakeHeartTimingGrowthTimes;
        }
    }
}
