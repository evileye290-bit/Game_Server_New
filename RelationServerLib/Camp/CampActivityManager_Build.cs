using EnumerateUtility;
using Message.Relation.Protocol.RZ;

namespace RelationServerLib
{

    public partial class CampActivityManager : AbstractCampWeekActivity
    {
        CampBuild battleBuildPhase;

        internal MSG_RZ_CAMPBUILD_INFO GetCampBuildPhaseInfo(int camp)
        {
            MSG_RZ_CAMPBUILD_INFO msg = new MSG_RZ_CAMPBUILD_INFO();
            msg.Camp = camp;
            msg.PhaseNum = nowShowPhaseNum;
            msg.Begin = battleBuildPhase.NowShowBeginTime.ToString();
            msg.End = battleBuildPhase.NowShowEndTime.ToString();
            msg.NextBegin = battleBuildPhase.NextBegin.ToString();
            msg.BuildingValue = GetCampBuildValue((CampType)camp);
            msg.NeedSync = false;
            return msg;
        }


        public void MergeServerReward()
        {
            battleBuildPhase.MergeServerReward();
        }


        //private bool CheckInBattleRank(int uid, int campType)
        //{
        //    var rank = GetCampRank((CampType)campType, RankType.CampBattleFight);

        //    var info = rank.GetRankBaseInfo(uid);
        //    if (info == null || info.Rank > RankLibrary.GetConfig(RankType.CampBattleFight).ShowCount)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

    }
}
