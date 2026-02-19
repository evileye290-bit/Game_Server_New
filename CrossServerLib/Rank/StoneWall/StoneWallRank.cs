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
    public class StoneWallRank : BaseRank
    {
        public StoneWallRank(CrossServerApi server, RankType rankType = RankType.StoneWall) : base(server, rankType)
        {
        }

        public void UpdateScore(int uid, int value, int addValue)
        {

            server.CrossRedis.Call(new OperateUpdateCrossRankScore(RankType.StoneWall, groupId, paramId, uid, value, server.Now()));          

            RankBaseModel ownerInfo = ChangeScore(uid, value);           

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
