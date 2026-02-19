using CommonUtility;
using DataProperty;
using EnumerateUtility.Timing;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public static class BrothersLib
    {
        /// <summary>
        /// 好友列表个数上限
        /// </summary>
        public static int BROTHER_LIST_MAX_COUNT;

        public static int BROTHER_FRIEND_SCORE { get; internal set; }
        public static int BROTHER_INVITER_LIST_MAX_COUNT { get; internal set; }

        public static void LoadDatas()
        {
            // Init FriendConfig
            Data brotherConfig = DataListManager.inst.GetData("BrothersConfig", 1);
            BROTHER_LIST_MAX_COUNT = brotherConfig.GetInt("BrotherListMaxCnt");
            BROTHER_FRIEND_SCORE = brotherConfig.GetInt("BrotherFriendScore");
            BROTHER_INVITER_LIST_MAX_COUNT = brotherConfig.GetInt("BrotherInviterListMaxCnt");
        }


    }
}
