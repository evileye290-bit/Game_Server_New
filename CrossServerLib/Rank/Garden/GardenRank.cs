using EnumerateUtility;
using RedisUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace CrossServerLib
{
    public class GardenRank : BaseRank
    {
        public GardenRank(CrossServerApi server, RankType rankType = RankType.Garden) : base(server, rankType)
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
            server.CrossRedis.Call(new OperateUpdateCrossRankScore(RankType.Garden, groupId, paramId, uid, value, server.Now()));

            //RankBaseModel ownerInfo = GetRankBaseInfo(uid);
            //if (ownerInfo != null)
            //{
            //    ownerInfo.Score = value;
            //}
            //else
            //{
            //    //新
            //    ownerInfo = new RankBaseModel();
            //    ownerInfo.Uid = uid;
            //    ownerInfo.Rank = 0;
            //    ownerInfo.Score = value;
            //    uidRankInfoDic[ownerInfo.Uid] = ownerInfo;
            //}

            RankBaseModel ownerInfo = ChangeScore(uid, value);
            ////排序
            //uidRankInfoDic = uidRankInfoDic.OrderByDescending(o => o.Value.Score).ToDictionary(o => o.Key, p => p.Value);
            //int rank = 1;
            //foreach (var rankItem in uidRankInfoDic)
            //{
            //    rankItem.Value.Rank = rank;
            //    rank++;
            //}

            BILoggerRecordLog(uid, value, ownerInfo);
        }

        private void BILoggerRecordLog(int uid, int addValue, RankBaseModel ownerInfo)
        {
            if (GameConfig.TrackingLogSwitch)
            {
                int rank = 0;
                int tatalValue = 0;
                //RankBaseModel ownerInfo = GetRankBaseInfo(uid);
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
