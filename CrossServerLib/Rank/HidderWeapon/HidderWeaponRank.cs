using EnumerateUtility;
using RedisUtility;
using ServerModels;
using ServerShared;
using System.Linq;

namespace CrossServerLib
{
    public class HidderWeaponRank : BaseRank
    {
        public HidderWeaponRank(CrossServerApi server, RankType rankType = RankType.HidderWeapon) : base(server, rankType)
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

            server.CrossRedis.Call(new OperateUpdateCrossRankScore(RankType.HidderWeapon, groupId, paramId, uid, value, server.Now()));

            ////新
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
            //排序
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
