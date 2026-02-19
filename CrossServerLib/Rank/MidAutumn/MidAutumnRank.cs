using EnumerateUtility;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossServerLib
{
    public class MidAutumnRank : BaseRank
    {
        public MidAutumnRank(CrossServerApi server, RankType rankType = RankType.MidAutumn) : base(server, rankType)
        {
        }

        public void UpdateScore(int uid, int value)
        {
            server.CrossRedis.Call(new OperateUpdateCrossRankScore(RankType.MidAutumn, groupId, paramId, uid, value, server.Now()));

            RankBaseModel ownerInfo = ChangeScore(uid, value);

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
