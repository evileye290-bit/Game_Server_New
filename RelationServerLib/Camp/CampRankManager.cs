using CommonUtility;
using EnumerateUtility;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Relation.Protocol.RZ;
using RelationServerLib;
using DBUtility;
using ServerModels;
using RedisUtility;

namespace RelationServerLib
{
    public class CampRankManager
    {
        private RelationServerApi server = null;
        private int mainId;

        //public CampActivityManager CampActivityManager =null;

        public CampPrestigeManager TianDouCamp = null;
        public CampPrestigeManager XingLuoCamp = null;

        public CampElectionManager TianDouElection = null;
        public CampElectionManager XingLuoElection = null;

        //public CampBuildRankManager TianDouCampBuild = null;
        //public CampBuildRankManager XingLuoCampBuild = null;

        public CampRankManager(RelationServerApi api)
        {
            server = api;
            mainId = api.MainId;
            Init();
        }

        public int GetWeakCamp()
        {
            //Todo 等战力出现时在补
            if (TianDouCamp.GetBattlePower() > XingLuoCamp.GetBattlePower())
            {
                return 2;
            }
            else if (TianDouCamp.GetBattlePower() == XingLuoCamp.GetBattlePower())
            {
                return RAND.Range(1, 2);
            }
            else
            {
                return 1;
            }
        }


        //internal MSG_RZ_CAMPBUILD_INFO GetCampBuildPhaseInfo(int camp)
        //{
        //    MSG_RZ_CAMPBUILD_INFO msg = null;
        //    server.CampActivityMng.GetCampBuildPhaseInfo(out msg);
        //    msg.Camp = camp;

        //    switch ((CampType)camp)
        //    {
        //        case CampType.TianDou:
        //            msg.BuildingValue = TianDouCampBuild.BuildingValue;
        //            break;
        //        case CampType.XingLuo:
        //            msg.BuildingValue = XingLuoCampBuild.BuildingValue;
        //            break;
        //        default:
        //            break;
        //    }
        //    return msg;
        //}

        //internal void AddCampBuildValue(int uid, int camp, int addValue)
        //{
        //    switch ((CampType)camp)
        //    {
        //        case CampType.TianDou:
        //            TianDouCampBuild.AddCampBuildValue(uid, addValue);
        //            break;
        //        case CampType.XingLuo:
        //            XingLuoCampBuild.AddCampBuildValue(uid, addValue);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //internal MSG_RZ_CAMPBUILD_RANK_LIST GetCampBuildRankInfo(int camp, int page, int uid)
        //{
        //    MSG_RZ_CAMPBUILD_RANK_LIST msg = null;
        //    switch ((CampType)camp)
        //    {
        //        case CampType.TianDou:
        //            TianDouCampBuild.GetList(out msg, page, uid);
        //            break;
        //        case CampType.XingLuo:
        //            XingLuoCampBuild.GetList(out msg, page, uid);
        //            break;
        //        default:
        //            break;
        //    }
        //    return msg;
        //}

        public List<PlayerRankBaseInfo> GetElectionList(CampType type, int count)
        {
            switch (type)
            {
                case CampType.TianDou:
                    return TianDouElection.GetElectionList4Manager(count);
                case CampType.XingLuo:
                    return XingLuoElection.GetElectionList4Manager(count);
                default:
                    break;
            }
            return null;

        }
        internal List<PlayerRankBaseInfo> GetLeaderList(CampType type)
        {
            switch (type)
            {
                case CampType.TianDou:
                    return TianDouElection.GetLeaderList();
                case CampType.XingLuo:
                    return XingLuoElection.GetLeaderList();
                default:
                    break;
            }
            return null;
        }


        public void Init()
        {
            TianDouCamp = new CampPrestigeManager(server, mainId, RankType.CampPrestige, CampType.TianDou);
            XingLuoCamp = new CampPrestigeManager(server, mainId, RankType.CampPrestige, CampType.XingLuo);

            TianDouElection = new CampElectionManager(server, mainId, RankType.CampBattlePower, CampType.TianDou);
            XingLuoElection = new CampElectionManager(server, mainId, RankType.CampBattlePower, CampType.XingLuo);

            //TianDouCampBuild = new CampBuildRankManager(server, mainId, RankType.CampBuild, CampType.TianDou);
            //XingLuoCampBuild = new CampBuildRankManager(server, mainId, RankType.CampBuild, CampType.XingLuo);


        }




        public void Update(double dt)
        {
            TianDouCamp.Update();
            XingLuoCamp.Update();

            //TianDouElection.Update();
            //XingLuoElection.Update();

            //TianDouCampBuild.Update();
            //XingLuoCampBuild.Update();
            if (TianDouElection.NeedSync && XingLuoElection.NeedSync)
            {
                NotifyZone();
                TianDouElection.NeedSync = false;
                XingLuoElection.NeedSync = false;
                SendCampLeaderTitleCard();
            }
        }

        public void ReStartRank()
        {
            TianDouElection.ReStartRank();
            XingLuoElection.ReStartRank();
        }

        public bool CheckChangeCamp2(int camp)
        {
            if (camp != GetWeakCamp())
            {
                return false;
            }
            return true;
        }

        public MSG_RZ_CAMP_RANK_LIST GetCampRankInfo(int camp, int page)
        {
            MSG_RZ_CAMP_RANK_LIST msg = null;
            switch ((CampType)camp)
            {
                case CampType.TianDou:
                    TianDouCamp.GetList(page, out msg);
                    break;
                case CampType.XingLuo:
                    XingLuoCamp.GetList(page, out msg);
                    break;
                default:
                    break;
            }
            return msg;
        }

        public MSG_RZ_CAMP_RANK_LIST GetCampRankInfo(int camp, int page, int pcUid)
        {
            MSG_RZ_CAMP_RANK_LIST msg = null;
            switch ((CampType)camp)
            {
                case CampType.TianDou:
                    TianDouCamp.GetList(page, out msg, pcUid);
                    break;
                case CampType.XingLuo:
                    XingLuoCamp.GetList(page, out msg, pcUid);
                    break;
                default:
                    break;
            }
            return msg;
        }

        public MSG_RZ_CAMP_PANEL_LIST GetCampPanelInfo(int camp, int pcUid)
        {
            MSG_RZ_CAMP_PANEL_LIST msg = null;
            switch ((CampType)camp)
            {
                case CampType.TianDou:
                    TianDouCamp.GetPanelListFromManager(out msg, pcUid);
                    break;
                case CampType.XingLuo:
                    XingLuoCamp.GetPanelListFromManager(out msg, pcUid);
                    break;
                default:
                    break;
            }
            return msg;
        }


        public MSG_RZ_CAMP_ELECTION_LIST GetCampElectionInfo(int camp, int page)
        {
            MSG_RZ_CAMP_ELECTION_LIST msg = null;
            switch ((CampType)camp)
            {
                case CampType.TianDou:
                    //TianDouElection.GetList(page, out msg);
                    break;
                case CampType.XingLuo:
                    //XingLuoElection.GetList(page, out msg);
                    break;
                default:
                    break;
            }
            return msg;
        }

        public void TryUpdateElectionInfos(int camp)
        {
            switch ((CampType)camp)
            {
                case CampType.TianDou:
                    TianDouElection.TryUpdateRankList();
                    break;
                case CampType.XingLuo:
                    XingLuoElection.TryUpdateRankList();
                    break;
                default:
                    break;
            }
        }

        public void SyncRankInfos(ZoneServer zone)
        {
            //全部发送从而可以检查周期是否对齐
            TianDouCamp.NotifyZonePeriod();
            XingLuoCamp.NotifyZonePeriod();
            NotifyZone();
        }

        public void NotifyZone()
        {
            foreach (var item in server.ZoneManager.ServerList)
            {
                ((ZoneServer)item.Value).NotifyCampBattlePower();
            }
        }

        private void SendCampLeaderTitleCard()
        {
            OperateGetWorshipInfo tiandou = new OperateGetWorshipInfo(mainId, (int)CampType.TianDou);
            OperateGetWorshipInfo xingluo = new OperateGetWorshipInfo(mainId, (int)CampType.XingLuo);

            //天斗
            server.GameRedis.Call(tiandou, ret =>
            {
                int count = tiandou.Infos.Count;
                if (count > 0)
                {
                    foreach (var item in tiandou.Infos)
                    {
                        //阵营领袖发称号卡
                        CampLeaderSendTitleCard(item.Uid, item.Rank);
                    }
                }
            });
            server.GameRedis.Call(xingluo, ret =>
            {
                int count = xingluo.Infos.Count;
                if (count > 0)
                {
                    foreach (var item in xingluo.Infos)
                    {
                        //阵营领袖发称号卡
                        CampLeaderSendTitleCard(item.Uid, item.Rank);
                    }
                }
            });
        }

        private void CampLeaderSendTitleCard(int uid, int rank)
        {
            List<TitleInfo> titleModelList = TitleLibrary.GetTitleListByCondition(TitleObtainCondition.CampLeader);
            if (titleModelList == null)
            {
                return;
            }
            foreach (var title in titleModelList)
            {
                if (title.SubType == rank)
                {
                    server.EmailMng.SendPersonEmail(uid, TitleLibrary.EmailId, title.Reward);
                    break;
                }
            }
        }
    }
}
