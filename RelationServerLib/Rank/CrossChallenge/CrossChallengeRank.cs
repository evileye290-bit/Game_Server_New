using System.Collections.Generic;
using DBUtility;
using EnumerateUtility;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;

namespace RelationServerLib
{
    public class CrossChallengeRank : BaseRank
    {
        public CrossChallengeRank(RelationServerApi server, RankType rankType = RankType.CrossChallenge) : base(server, rankType)
        {
        }

        protected override void UpdateCrossRank(RankBaseModel tempRank, int rank)
        {
            tempRank.Rank = rank;
            server.GameDBPool.Call(new QueryUpdateCrossChallengeRank(tempRank.Uid, rank));

            //找到玩家说明玩家在线，通知玩家发送信息回来
            MSG_RZ_UPDATE_CROSS_CHALLENGE_RANK msg = new MSG_RZ_UPDATE_CROSS_CHALLENGE_RANK();
            msg.PcUid = tempRank.Uid;
            msg.Rank = rank;
            server.ZoneManager.Broadcast(msg);
        }

        public void ResetRankList()
        {
            Dictionary<int, int> oldDic = new Dictionary<int, int>(); 
            foreach (var kv in uidRankInfoDic)
            {
                oldDic.Add(kv.Value.Uid, kv.Value.Score);
            }

            base.Clear();
            uidRankInfoDic.Clear();

            foreach (var kv in oldDic)
            {
                int score = 0;
                CrossLevelInfo info = CrossChallengeLibrary.CheckCrossLevel(kv.Value);
                if (info != null)
                {
                    score = info.ResetStar;
                }
                ChangeScore(kv.Key, 0);
                server.GameRedis.Call(new OperateUpdateRankScore(rankType, server.MainId, kv.Key, score, server.Now()));
            }
        }
    }
}
