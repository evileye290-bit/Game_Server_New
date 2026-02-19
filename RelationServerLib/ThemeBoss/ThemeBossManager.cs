using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class ThemeBossManager
    {
        private RelationServerApi server { get; set; }
        //rank, uid
        private Dictionary<int, int> rewardRankUidList = new Dictionary<int, int>();

        public ThemeBossManager(RelationServerApi server)
        {
            this.server = server;
            //LoadRankInfoFromRedis();
        }

        public void Update()
        {
            SendThemeBossRankReward();
        }

        private void LoadRankInfoFromRedis()
        {
            OperateGetRankScore op = new OperateGetRankScore(RankType.ThemeBoss, server.MainId, 0, ThemeBossLibrary.RankRewardCount);
            server.GameRedis.Call(op, ret =>
            {
                Dictionary<int, RankBaseModel> uidRankInfoDic = op.uidRank;
                if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
                {
                    Log.Warn($"load themeBoss rank list fail ,can not find data in redis");
                }
                foreach (var item in uidRankInfoDic)
                {
                    rewardRankUidList.Add(item.Value.Rank, item.Key);

                    server.TrackingLoggerMng.TrackRankLog(server.MainId, RankType.ThemeBoss.ToString(), item.Value.Rank, item.Value.Score, item.Value.Uid, server.Now()); //FIXME :加这里不太严谨。
                                                                                                                                                                          //BI
                    server.KomoeEventLogRankFlow(item.Value.Uid, RankType.ThemeBoss, item.Value.Rank, item.Value.Rank, item.Value.Score, RewardManager.GetRewardDic(""));
                }
            });
        }

        public void Clear()
        {
            server.RankMng.ThemeBossRank.Clear();
        }

        public void ActivityEnd()
        {
            LoadRankInfoFromRedis();
        }

        public void SendThemeBossRankReward()
        {
            if (rewardRankUidList.Count > 0)
            {
                KeyValuePair<int, int> item = rewardRankUidList.First();
                RankRewardInfo info = ThemeBossLibrary.GetThemeBossRankRewardInfo(item.Key);
                if (info != null)
                {
                    //发送邮件
                    server.EmailMng.SendPersonEmail(item.Value, info.EmailId, info.Rewards);     
                }
                else
                {
                    Log.Warn("player {0} rank {1} send theme boss rank reward failed: not find reward info", item.Value, item.Key);
                }
                rewardRankUidList.Remove(item.Key);
            }
        }

        public void MergeServerReward()
        {
            rewardRankUidList.Clear();
            OperateGetRankScore op = new OperateGetRankScore(RankType.ThemeBoss, server.MainId, 0, ThemeBossLibrary.RankRewardCount);
            server.GameRedis.Call(op, ret =>
            {
                Dictionary<int, RankBaseModel> uidRankInfoDic = op.uidRank;
                if (uidRankInfoDic == null || uidRankInfoDic.Count < 1)
                {
                    Log.Warn($"load themeBoss rank list fail ,can not find data in redis");
                }
                foreach (var item in uidRankInfoDic)
                {
                    rewardRankUidList.Add(item.Value.Rank, item.Key);

                    server.TrackingLoggerMng.TrackRankLog(server.MainId, RankType.ThemeBoss.ToString(), item.Value.Rank, item.Value.Score, item.Value.Uid, server.Now()); //FIXME :加这里不太严谨。
                }

                DoReward();
            });
        }

        private void DoReward()
        {
            foreach (var kv in rewardRankUidList)
            {
                RankRewardInfo info = ThemeBossLibrary.GetThemeBossRankRewardInfo(kv.Key);
                if (info != null)
                {
                    //发送邮件
                    server.EmailMng.SendPersonEmail(kv.Value, info.EmailId, info.Rewards);
                }
                else
                {
                    Log.Warn("player {0} rank {1} send theme boss rank reward failed: not find reward info", kv.Value, kv.Key);
                }
            }
            rewardRankUidList.Clear();
            Log.Warn("MergeServerReward Theme Boss reward success !");
        }

        private void NotifyOpenNewThemeBoss()
        {
            MSG_RZ_NEW_THEMEBOSS msg = new MSG_RZ_NEW_THEMEBOSS();
            server.ZoneManager.Broadcast(msg);
        }
    }
}
