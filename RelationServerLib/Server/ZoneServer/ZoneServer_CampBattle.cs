using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerModels;
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

        public void OnResponse_GetCampBattleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GET_CAMPBATTLE_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_CAMPBATTLE_INFO>(stream);

            MSG_RZ_SYNC_CAMPBATTLE_DATA response = Api.CampActivityMng.GetCampBattleInfo(uid);
            if (uid==0)
            {
                Write(response);
            }
            else
            {
                Client client = ZoneManager.GetClient(uid);
                if (client != null)
                {
                    client.CurZone.Write(response,uid);
                }
                else
                {
                    Logger.Log.Warn($"try get client {uid} failed in get camp battle sync info");
                }
            }

        }


        public void OnResponse_GetFortInfo(MemoryStream stream,int uid = 0)
        {
            MSG_ZR_GET_FORT_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_FORT_INFO>(stream);

            MSG_RZ_GET_FORT_DATA res = Api.CampActivityMng.GetCampBattleFortData(msg.FortId);

            Client client = ZoneManager.GetClient(uid);
            if (client != null)
            {
                client.CurZone.Write(res,uid);
            }
            else
            {
                Logger.Log.Warn($"try get client {uid} failed in get fort info");
            }
        }

        //public void OnResponse_UseNatureItem(MemoryStream stream, int uid = 0)
        //{
        //    //MSG_ZR_USE_NATURE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_USE_NATURE_ITEM>(stream);
        //    //MSG_RZ_USE_NATURE_ITEM res = Api.CampActivityMng.UseNatureItem(msg.FortId, msg.ItemId);

        //    //Client client = ZoneManager.GetClient(uid);
        //    //if (client != null)
        //    //{
        //    //    client.CurZone.Write(res, uid);

        //    //    MSG_RZ_GET_FORT_DATA sync = Api.CampActivityMng.GetCampBattleFortData(msg.FortId);
        //    //    client.CurZone.Write(sync, uid);
        //    //}
        //    //else
        //    //{
        //    //    Logger.Log.Warn($"try get client {uid} failed ,OnResponse_UseNatureItem");
        //    //}
        //}

        //public void OnResponse_CheckUseNatureItem(MemoryStream stream, int uid = 0)
        //{
        //    //MSG_ZR_CHECK_USE_NATURE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHECK_USE_NATURE_ITEM>(stream);
        //    //MSG_RZ_CHECK_USE_NATURE_ITEM res = Api.CampActivityMng.CheckUseNatureItem(msg.FortId, msg.ItemId,msg.Camp);
        //    //Client client = ZoneManager.GetClient(uid);
        //    //if (client != null)
        //    //{
        //    //    client.CurZone.Write(res, uid);
        //    //}
        //    //else
        //    //{
        //    //    Logger.Log.Warn($"try get client {uid} failed, OnResponse_CheckUseNatureItem");
        //    //}
        //}

        public void OnResponse_UpdateNatureCount(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_NATURE_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_NATURE_COUNT>(stream);
            Api.CampActivityMng.UpdateAddNature(uid, msg.NewCount);
        }

        public void NotifyCampBattleEnd(int winCamp)
        {
            MSG_RZ_CAMPBATTLE_END msg = new MSG_RZ_CAMPBATTLE_END();
            msg.WinCamp = winCamp;
            Write(msg);
        }

        public void OnResponse_GiveUpFort(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_GIVEUP_FORT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GIVEUP_FORT>(stream);
            ErrorCode result = Api.CampActivityMng.GiveUpFort(uid, msg.FortId);

            Client client = ZoneManager.GetClient(uid);
            if (client != null)
            {
                MSG_RZ_GIVEUP_FORT response = new MSG_RZ_GIVEUP_FORT();
                response.Result = (int)result;
                response.FortId = msg.FortId;
                client.CurZone.Write(response, uid);

                if(result == ErrorCode.Success)
                {
                    MSG_RZ_SYNC_CAMPBATTLE_DATA response1 = Api.CampActivityMng.GetCampBattleInfo();
                    client.CurZone.Write(response1, uid);
                }
            }
            else
            {
                Logger.Log.Warn($"try get client {uid} failed, OnResponse_GiveUpFort");
            }
        }

        public void OnResponse_HoldFort(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_HOLD_FORT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_HOLD_FORT>(stream);
            ErrorCode result = Api.CampActivityMng.HoldFort(msg);

            Client client = ZoneManager.GetClient(uid);
            if (client != null)
            {
                MSG_RZ_HOLD_FORT response = new MSG_RZ_HOLD_FORT();
                response.Result = (int)result;
                response.FortId = msg.FortId;
                client.CurZone.Write(response, uid);

                if (result == ErrorCode.Success)
                {
                    MSG_RZ_SYNC_CAMPBATTLE_DATA response1 = Api.CampActivityMng.GetCampBattleInfo();
                    client.CurZone.Write(response1, uid);
                }
            }
            else
            {
                Logger.Log.Warn($"try get client {uid} failed, OnResponse_GiveUpFort");
            }
        }

        public void OnResponse_SyncHistoricalMaxCampScore(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SYNC_HISTORICALMAXCAMPSCORE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SYNC_HISTORICALMAXCAMPSCORE>(stream);
            Api.CampActivityMng.SyncHistoricalMaxCampScore(msg.Uid,msg.Score);
        }
        public void OnResponse_UpdateDefensiveQueue(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_DEFENSIVEQUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_DEFENSIVEQUEUE>(stream);
            Api.CampActivityMng.UpdateDefensiveQueue(uid, msg.HeroList,msg.SetForts);
        }

        
    }
}
