using EnumerateUtility;
using RedisUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace CrossServerLib
{
    public class IslandHighLastStageRank : BaseRank
    {
        public IslandHighLastStageRank(CrossServerApi server, RankType rankType = RankType.IslandHighLastStage) : base(server, rankType)
        {
        }

        private void BILoggerRecordLog(int uid, int addValue, RankBaseModel ownerInfo)
        {
            if (GameConfig.TrackingLogSwitch)
            {
                int rank = 0;
                int tatalValue = ownerInfo.Score;
                ownerInfo = GetRankBaseInfo(uid);
                if (ownerInfo != null)
                {
                    rank = ownerInfo.Rank;
                }
                server.TrackingLoggerMng.RecordCrossRankLog(uid, addValue, tatalValue, rank, rankType.ToString(), groupId, paramId, server.Now());
            }
        }

        public Dictionary<int, RankBaseModel> GetRankInfoList()
        {
            return uidRankInfoDic;
        }

        public void UpdateRankInfoList(Dictionary<int, RankBaseModel> rankBaseModels)
        {
            uidRankInfoDic.Clear();
            foreach (KeyValuePair<int, RankBaseModel> rankBaseModel in rankBaseModels)
            {
                server.CrossRedis.Call(new OperateUpdateCrossRankScore(RankType.IslandHighLastStage, groupId, paramId, rankBaseModel.Value.Uid, rankBaseModel.Value.Score, server.Now()));

                //新
                RankBaseModel ownerInfo = new RankBaseModel();
                ownerInfo.Uid = rankBaseModel.Value.Uid;
                ownerInfo.Rank = 0;
                ownerInfo.Score = rankBaseModel.Value.Score;
                uidRankInfoDic[ownerInfo.Uid] = ownerInfo;
            }

            //排序
            uidRankInfoDic = uidRankInfoDic.OrderByDescending(o => o.Value.Score).ToDictionary(o => o.Key, p => p.Value);
            int rank = 1;
            foreach (var rankItem in uidRankInfoDic)
            {
                rankItem.Value.Rank = rank;
                rank++;
            }
        }
    }
}
