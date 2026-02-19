using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public partial class ZoneServer
    {

        public void OnResponse_GetWeakCamp(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_WEAK_CAMP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_WEAK_CAMP>(stream);

            Client client = ZoneManager.GetClient(msg.PcUid);
            if (client != null)
            {
                //client
                int camp = Api.ArenaMng.GetWeakCamp();
                MSG_RZ_WEAK_CAMP res = new MSG_RZ_WEAK_CAMP();
                res.Camp = camp;
                res.PcUid = msg.PcUid;
                client.CurZone.Write(res);
            }
            else
            {
                Logger.Log.Warn($"try get client {msg.PcUid} failed in get weak camp");
            }
        }

        public void OnResponse_ChangeCamp(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CHANGE_CAMP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHANGE_CAMP>(stream);
            Client client = ZoneManager.GetClient(msg.PcUid);
            if (client != null)
            {
                //client
                bool allowed = true; //更换阵营，不判断弱势阵营 所以暂时去掉判断  Api.CampRankMng.CheckChangeCamp2(msg.CampId);
                MSG_RZ_CHANGE_CAMP res = new MSG_RZ_CHANGE_CAMP();
                res.Allowed = allowed;
                res.PcUid = msg.PcUid;
                res.CampId = msg.CampId;
                client.CurZone.Write(res);

                client.SetCamp(msg.CampId);
                if (allowed)
                {
                    Api.CampActivityMng.GiveUpFort(uid);
                }
            }
            else
            {
                Logger.Log.Warn($"try get client {msg.PcUid} failed in change camp");
            }
        }

        public void OnResponse_GetCampRank(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CAMP_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CAMP_RANK>(stream);
            Client client = ZoneManager.GetClient(msg.PcUid);
            int Camp = msg.Camp;
            if (client != null)
            {
                if (msg.Camp > 0)
                {
                    MSG_RZ_CAMP_RANK_LIST info = Api.CampRankMng.GetCampRankInfo(msg.Camp, msg.Page,msg.PcUid);
                    //todo 把个人信息放进去
                    info.PcUid = msg.PcUid;
                    info.CampId = msg.Camp;
                    client.CurZone.Write(info);
                }
                else
                {
                    Logger.Log.Warn($"player {msg.PcUid} try get camp rank  failed with camp {msg.Camp}");
                }
            }
            else
            {
                Logger.Log.Warn($"try get client {msg.PcUid} failed in Get Camp Rank");
            }
        }

        public void OnResponse_GetCampPanel(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CAMP_PANEL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CAMP_PANEL_INFO>(stream);

            Client client = ZoneManager.GetClient(msg.PcUid);
            int Camp = msg.Camp;
            if (client != null)
            {
                if (msg.Camp > 0)
                {
                    MSG_RZ_CAMP_PANEL_LIST info = Api.CampRankMng.GetCampPanelInfo(msg.Camp,msg.PcUid);
                    info.BoxCount = Api.CampActivityMng.GetBoxCount((CampType)msg.Camp);
                    info.PcUid = msg.PcUid;
                    info.CampId = msg.Camp;
                    client.CurZone.Write(info);
                }
                else
                {
                    Log.Warn($"player {msg.PcUid} try get camp panel info  failed with camp {msg.Camp}");
                }
            }
            else
            {
                Log.Warn($"try get client {msg.PcUid} failed in Get Camp panel info");
            }
        }

        public void OnResponse_TryUpdateElectionRank(MemoryStream stream,int uid = 0)
        {
            MSG_ZR_UPDATE_ELECTION_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_ELECTION_RANK>(stream);
            Api.CampRankMng.TryUpdateElectionInfos(msg.Camp);

        }

        public void OnResponse_GetElectionRank(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CAMP_ELECTION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CAMP_ELECTION>(stream);
            Client client = ZoneManager.GetClient(msg.PcUid);
            int Camp = msg.Camp;
            if (client != null)
            {
                if (msg.Camp > 0)
                {
                    Logger.Log.Warn($"player {msg.PcUid} try get camp election  failed with camp {msg.Camp}(function close)");
                    //MSG_RZ_CAMP_ELECTION_LIST info = Api.CampRankMng.GetCampElectionInfo(msg.Camp, msg.Page);
                    //info.CampId = msg.Camp;
                    //info.PcUid = msg.PcUid;
                    //client.CurZone.Write(info);
                }
                else
                {
                    Logger.Log.Warn($"player {msg.PcUid} try get camp election  failed with camp {msg.Camp}");
                }
            }
            else
            {
                Logger.Log.Warn($"try get client {msg.PcUid} failed in Get Election Rank");
            }
        }

        public void NotifyCampPeriod(int period, RankType type, DateTime begin, DateTime end)
        {
            MSG_RZ_UPDATE_CAMP_RANK_PERIOD notify = new MSG_RZ_UPDATE_CAMP_RANK_PERIOD();
            notify.CurPeriod = period;
            notify.RankType = (int)type;
            notify.Begin = begin.ToString();
            notify.End = end.ToString();
            Logger.Log.Write($"notify camp rank {type} period to zone {MainId} with CurPeriod {period} Begin {begin} End {end}");
            Write(notify);
        }


        //public void NotifyElectionPeriod(int period, RankType type, DateTime begin, DateTime end)
        //{
        //    MSG_RZ_UPDATE_CAMP_ELECTION_PERIOD notify = new MSG_RZ_UPDATE_CAMP_ELECTION_PERIOD();
        //    notify.CurPeriod = period;
        //    notify.RankType = (int)type;
        //    notify.Begin = begin.ToString();
        //    notify.End = end.ToString();
        //    Write(notify);
        //}


        internal void NotifyCampBattlePower()
        {
            MSG_RZ_UPDATE_CAMP_ELECTION_PERIOD notify = new MSG_RZ_UPDATE_CAMP_ELECTION_PERIOD();
            notify.CurPeriod = 0;
            notify.RankType = (int)RankType.CampBattlePower;
            notify.Begin = "";
            notify.End = "";
            Write(notify);
        }

        //public void NotifyRankPeriodStart(int period, RankType type, DateTime begin, DateTime end)
        //{
        //    MSG_RZ_UPDATE_RANK_PERIOD notify = new MSG_RZ_UPDATE_RANK_PERIOD();
        //    notify.CurPeriod = period;
        //    notify.RankType = (int)type;
        //    notify.Begin = begin.ToString();
        //    notify.End = end.ToString();
        //    Write(notify);
        //}

        public void NotifyCampCoin(int camp, int grain)
        {
            MSG_RZ_CAMP_GRAIN response = Api.CampActivityMng.GetGrain((CampType)camp);
            Write(response);
        }

        public void OnResponse_AskForRankPeriod(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_ASK_RANK_PERIOD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ASK_RANK_PERIOD>(stream);
            if (Api.CampRankMng != null)
            {
                Api.CampRankMng.SyncRankInfos(this);
            }

        }

        public void OnResponse_AddCampGrain(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_CAMP_GRAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_CAMP_GRAIN>(stream);
            Api.CampActivityMng.AddGrain((CampType)msg.Camp, msg.GrainAdd);
        }

        public void OnResponse_GetCampCoin(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CAMP_GRAIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CAMP_GRAIN>(stream);
            MSG_RZ_CAMP_GRAIN response = Api.CampActivityMng.GetGrain((CampType)msg.Camp);
            Write(response, uid);
        }

        internal void SyncCampActivityPhaseInfo(int nowShowPhaseNum, CampActivityType type, DateTime nowShowBegin, DateTime nowShowEnd)
        {
            switch (type)
            {
                case CampActivityType.Default:
                    break;
                case CampActivityType.BattleAssart:
                    NotifyCampBattlePhaseInfo(nowShowPhaseNum, nowShowBegin, nowShowEnd, (int)Api.CampActivityMng.CurCampBattleStep);
                    //NotifyCampBattlePhaseInfo(nowShowPhaseNum, nowShowBegin, nowShowEnd, (int)Api.CampActivityMng.CurCampBattleStep);
                    break;
               //case CampActivityType.Build:
               //     NotifyCampBuildPhaseInfo(nowShowPhaseNum, nowShowBegin, nowShowEnd);
                //    NotifyCampBuildPhaseInfo(nowShowPhaseNum, CampType.XingLuo, nowShowBegin, nowShowEnd);
                    break;
                default:
                    break;
            }
        }

        internal void SyncCampBoxCount(CampType camp, int count)
        {
            MSG_RZ_CAMP_BOX_COUNT notify = new MSG_RZ_CAMP_BOX_COUNT();
            notify.Camp = (int)camp;
            notify.Count = count;
            Write(notify);
        }


        public void NotifyCampBuildPhaseInfo(int phaseNum,DateTime begin, DateTime end)
        {
            MSG_RZ_CAMPBUILD_RESET notify = new MSG_RZ_CAMPBUILD_RESET();
            notify.PhaseNum = phaseNum;
            notify.Begin = begin.ToString();
            notify.End = end.ToString();
            notify.NeedSync = true;
            Write(notify);
        }

        public void NotifyCampBattlePhaseInfo(int phaseNum, DateTime begin, DateTime end, int step)
        {
            MSG_RZ_SYNC_CAMPBATTLE_DATA notify = Api.CampActivityMng.GetCampBattleInfo();
            notify.PhaseNum = phaseNum;
            notify.Step = step;
            notify.BeginTime = begin.ToString();
            notify.EndTime = end.ToString();
            Write(notify);
        }

    }
}
