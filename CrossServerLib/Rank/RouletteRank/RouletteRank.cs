using System.Collections.Generic;
using System.Linq;
using EnumerateUtility;
using RedisUtility;
using ServerModels;
using ServerShared;

namespace CrossServerLib
{
    public class RouletteRank : BaseRank
    {
        public RouletteRank(CrossServerApi server, RankType rankType = RankType.Roulette) : base(server, rankType)
        {
        }

        public RankBaseModel GetFirst()
        {
            if (uidRankInfoDic.Count > 0)
            {
                return uidRankInfoDic.First().Value;
            }
            else
            {
                return null;
            }
        }

        public void UpdateScore(int uid, int value)
        {
            server.CrossRedis.Call(new OperateUpdateCrossRankScore(RankType.Roulette, groupId, paramId, uid, value, server.Now()));

            RankBaseModel ownerInfo = ChangeScore(uid, value);

            BILoggerRecordLog(uid, value, ownerInfo);
        }

        private void BILoggerRecordLog(int uid, int addValue, RankBaseModel ownerInfo)
        {
            if (GameConfig.TrackingLogSwitch)
            {
                int rank = 0;
                int tatalValue = 0;
                if (ownerInfo != null)
                {
                    rank = ownerInfo.Rank;
                    tatalValue = ownerInfo.Score;
                }
                server.TrackingLoggerMng.RecordCrossRankLog(uid, addValue, tatalValue, rank, rankType.ToString(), groupId, paramId, server.Now());
            }
        }

        public Dictionary<int, RankBaseModel> GetRankInfoList()
        {
            return uidRankInfoDic;
        }
    }
}