using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
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
        public void OnResponse_CampBulidInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CAMPBUILD_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CAMPBUILD_INFO>(stream);
            //获取阵营期数信息

            //if (uid == 0)
            //{
            //    MSG_RZ_CAMPBUILD_INFO msg1 = Api.CampRankMng.GetCampBuildPhaseInfo((int)CampType.XingLuo);
            //    Write(msg1);
            //    MSG_RZ_CAMPBUILD_INFO msg2 = Api.CampRankMng.GetCampBuildPhaseInfo((int)CampType.TianDou);
            //    Write(msg2);
            //}
            //else
            {

                MSG_RZ_CAMPBUILD_INFO msg = Api.CampActivityMng.GetCampBuildPhaseInfo(pks.Camp);
                msg.NeedSync = true;
                Write(msg,uid);
            }
        }
        public void OnResponse_CampBuildRankList(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CAMPBUILD_RANK_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CAMPBUILD_RANK_LIST>(stream);

            Client client = ZoneManager.GetClient(uid);

            if (client != null)
            {
                //获取排行榜信息

                CampRank campRank = Api.CampActivityMng.GetCampRank((CampType)pks.Camp, RankType.CampBuild);
                if (campRank == null)
                {
                    Logger.Log.Warn($"player {uid} get camp battle rank list failed ");
                    return;
                }
                //campRank.RefreshAndPush(uid, pks.Page);
                //client.Write(msg);
            }
            else
            {
                Logger.Log.Error("player {0} get battle rank list fail! cannot find client");
            }
                     
        }

        public void OnResponse_CampBuildAddValue(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CAMPBUILD_ADD_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CAMPBUILD_ADD_VALUE>(stream);
            //添加阵营建设贡献值
            Api.CampActivityMng.AddCampBuildValue((CampType)pks.Camp,pks.AddValue);
        }

        public void NotifyCampBuildPhaseInfo(int phaseNum,CampType camp, DateTime begin, DateTime end,int buildingValue)
        {
            MSG_RZ_CAMPBUILD_INFO notify = new MSG_RZ_CAMPBUILD_INFO();
            notify.Camp = (int)camp;
            notify.PhaseNum = phaseNum;
            notify.Begin = begin.ToString();
            notify.End = end.ToString();
            notify.BuildingValue = buildingValue;
            Logger.Log.Write($"notify camp build phase to zone {MainId} with cur phase {phaseNum} Begin {begin} End {end}");
            Write(notify);
        }

        public void OnResponse_CampCreateDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CAMP_CREATE_DUNGEON pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CAMP_CREATE_DUNGEON>(stream);
            MSG_RZ_CAMP_CREATE_DUNGEON msg =  Api.CampActivityMng.CreateDungeon(uid, pks.Camp, pks.FortId, pks.DungeonId);
            Write(msg, uid);
        }

        public void OnResponse_CampBattleResult(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CAMP_DUNGEON_END msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CAMP_DUNGEON_END>(stream);

            //结算当前战斗
            Api.CampActivityMng.DungeonWindUp(msg);
        }
    }
}
