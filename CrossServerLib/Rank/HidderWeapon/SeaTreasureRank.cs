using EnumerateUtility;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossServerLib
{
    public class SeaTreasureRank : BaseRank
    {
        public SeaTreasureRank(CrossServerApi server, RankType rankType = RankType.SeaTreasure) : base(server, rankType)
        {
        }
        //public RankBaseModel GetFirst()
        //{
        //    if (uidRankInfoDic.Count > 0)
        //    {
        //        return uidRankInfoDic.First().Value;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public void ClearCrossRankPlayerInfo()
        //{
        //    //OperateClearCrossPlayerInfo operate = new OperateClearCrossPlayerInfo(uidRankInfoDic.Keys.ToList());
        //    //server.CrossRedis.Call(operate);

        //    Clear();
        //}

        public void UpdateScore(int uid, int value, int addValue)
        {

            server.CrossRedis.Call(new OperateUpdateCrossRankScore(RankType.SeaTreasure, groupId, paramId, uid, value, server.Now()));

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

            //if (FirstRank != null)
            //{
            //    if (value > FirstRank.Score)
            //    {
            //        FirstRank = ownerInfo;
            //    }
            //}
            //else
            //{
            //    FirstRank = ownerInfo;
            //}

            ////新
            //RankBaseModel rankItem = new RankBaseModel();
            //rankItem.Uid = uid;
            //rankItem.Rank = 0;
            //rankItem.Score = value;
            //uidRankInfoDic[rankItem.Uid] = rankItem;
            ////排序
            //uidRankInfoDic = uidRankInfoDic.OrderByDescending(o => o.Value.Score).ToDictionary(o => o.Key, p => p.Value);

            BILoggerRecordLog(uid, addValue, ownerInfo);
        }


        private void BILoggerRecordLog(int uid, int addValue, RankBaseModel ownerInfo)
        {
            if (GameConfig.TrackingLogSwitch)
            {
                int rank = 0;
                int tatalValue = ownerInfo.Score;
                RankBaseModel firstRank = GetFirst();
                if (firstRank != null && firstRank.Uid == uid)
                {
                    rank = 1;
                }
                server.TrackingLoggerMng.RecordCrossRankLog(uid, addValue, tatalValue, rank, rankType.ToString(), groupId, paramId, server.Now());
            }
        }
    }
}
