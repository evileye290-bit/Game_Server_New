using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
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
    public class CrossBattleRank : BaseRank
    {
        public CrossBattleRank(RelationServerApi server, RankType rankType = RankType.CrossServer) : base(server, rankType)
        {

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
                CrossLevelInfo info = CrossBattleLibrary.CheckCrossLevel(kv.Value);
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
