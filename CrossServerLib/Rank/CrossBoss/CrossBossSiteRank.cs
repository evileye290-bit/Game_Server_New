using EnumerateUtility;
using Logger;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossServerLib
{
    public class CrossBossSiteRank : BaseRank
    {
        public CrossBossSiteRank(CrossServerApi server, RankType rankType = RankType.CrossBossSite) : base(server, rankType)
        {
        }

        public void UpdateScore(int uid, int addValue)
        {
            RankBaseModel ownerInfo = GetRankBaseInfo(uid);
            if (ownerInfo != null)
            {
                ownerInfo.Score += addValue;
            }
            else
            {
                //新
                ownerInfo = new RankBaseModel();
                ownerInfo.Uid = uid;
                ownerInfo.Rank = 0;
                ownerInfo.Score = addValue;
                uidRankInfoDic[ownerInfo.Uid] = ownerInfo;
            }

            OperateAddCrossRankScore qperate = new OperateAddCrossRankScore(rankType, groupId, paramId, uid, addValue, server.Now());
            server.CrossRedis.Call(qperate, re =>
            {
                int total = qperate.TotalScore;
               
                ownerInfo = ChangeScore(uid, total);

                BILoggerRecordLog(uid, addValue, ownerInfo);
            });

            ////排序
            //sort();
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

        //public void sort()
        //{
        //    uidRankInfoDic = uidRankInfoDic.OrderByDescending(o => o.Value.Score).ToDictionary(o => o.Key, p => p.Value);
        //    int rank = 1;
        //    foreach (var rankItem in uidRankInfoDic)
        //    {
        //        rankItem.Value.Rank = rank;
        //        rank++;
        //    }
        //}
    }
}
