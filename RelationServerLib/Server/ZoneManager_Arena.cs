using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace RelationServerLib
{
    public class ArenaChallengerInfoMessage
    {
        public DateTime DeleteTime { get; set; }
        public MSG_ZGC_ARENA_CHALLENGER_HERO_INFO Message { get; set; }
    }

    public partial class ZoneServerManager
    {

        private Dictionary<int, ArenaChallengerInfoMessage> arenaChallengerInfoDic = new Dictionary<int, ArenaChallengerInfoMessage>();

        double challengerDeleteTime = 0.0;
        //double challengerDeleteTickTime = 60000.0;
        List<int> challengerRemoveList = new List<int>();

        public void DeleteArenaChallengerInfo(double dt)
        {
            if (!CheckArenaChallengerDeleteTickTime(dt))
            {
                return;
            }
            foreach (var item in arenaChallengerInfoDic)
            {
                if (RelationServerApi.now > item.Value.DeleteTime)
                {
                    challengerRemoveList.Add(item.Key);
                }
            }

            if (challengerRemoveList.Count > 0)
            {
                foreach (var uid in challengerRemoveList)
                {
                    RemoveArenaChallengerInfo(uid);
                }
                challengerRemoveList.Clear();
            }
        }

        private bool CheckArenaChallengerDeleteTickTime(double deltaTime)
        {
            challengerDeleteTime += (float)deltaTime;
            if (challengerDeleteTime < ArenaLibrary.RankRefreshTime)
            {
                return false;
            }
            else
            {
                challengerDeleteTime = 0;
                return true;
            }
        }


        public ArenaChallengerInfoMessage GetArenaChallengerInfo(int uid)
        {
            ArenaChallengerInfoMessage info = null;
            arenaChallengerInfoDic.TryGetValue(uid, out info);    
            return info;
        }

        public void AddArenaChallengerInfo(MSG_ZGC_ARENA_CHALLENGER_HERO_INFO info, int uid)
        {
            if (info == null)
            {
                Log.Warn("add arena challenger info failed: show info is null");
                return;
            }
            ArenaChallengerInfoMessage msg = new ArenaChallengerInfoMessage();
            msg.DeleteTime = RelationServerApi.now.AddMinutes(10);
            msg.Message = info;
            arenaChallengerInfoDic[uid] = msg;
        }


        public bool RemoveArenaChallengerInfo(int Uid)
        {
            return arenaChallengerInfoDic.Remove(Uid);
        }

        //发放每日竞技场奖励
        public void RefreshArenaDailyReward()
        {
            ////发送奖励邮件
            //Dictionary<int, RankRewardInfo> dic = ArenaLibrary.GetDailyRankRewards();
            //foreach (var item in dic)
            //{
            //    //记录已经发送奖励
            //    Api.GameDBPool.Call(new QueryUpdateArenaDailyRankReward(item.Value.Id, item.Value.RankMin, item.Value.RankMax));
            //}

            //通知所有zone来获取奖励
            Api.ArenaMng.GetDailyRewardList();
        }
    }
}
