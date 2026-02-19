using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {

        private void OnResponse_GetCampBuildInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CAMPBUILD_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CAMPBUILD_INFO>(stream);
            Log.Write("player {0} request get camp build info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetCampBuildInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("camp build info fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("camp build info fail, can not find player {0} .", uid);
                }
            }
        }


        private void OnResponse_CampBuildGo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CAMPBUILD_GO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CAMPBUILD_GO>(stream);
            Log.Write("player {0} request camp build GO", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CampBuildGo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("camp build GO fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("camp build GO fail, can not find player {0} .", uid);
                }
            }
        }

        //private void OnResponse_BuyCampBuildGoCount(MemoryStream stream,int uid = 0)
        //{
        //    MSG_GateZ_BUY_CAMPBUILD_GO_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_CAMPBUILD_GO_COUNT>(stream);
        //    PlayerChar player = Api.PCManager.FindPc(uid);
        //    if (player != null)
        //    {
        //        player.BuyCampBuildGoCount(msg.Count);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(uid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("buy build go count fail, player {0} is offline.", uid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("buy build go count fail, can not find player {0} .", uid);
        //        }
        //    }
        //}

        private void OnResponse_CampBuildRankList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CAMPBUILD_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CAMPBUILD_RANK_LIST>(stream);
            Log.Write("player {0} request camp build rank list page {1}", uid, msg.Page);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ShowCampBuildRankList(msg.Page);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("camp build rank list fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("camp build rank list fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_OpenCampBuildBox(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_OPEN_CAMPBUILD_BOX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_OPEN_CAMPBUILD_BOX>(stream);
            Log.Write("player {0} request open camp build box", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.OpenCampBuildBox(msg.BoxType);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("open build box {1} fail, player {0} is offline.", uid,msg.BoxType);
                }
                else
                {
                    Log.WarnLine("open build box {1} fail, can not find player {0} .", uid,msg.BoxType);
                }
            }
        }

        private void OnResponse_CampCreateDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CAMP_CREATE_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CAMP_CREATE_DUNGEON>(stream);
            Log.Write("player {0} request camp create dungeon: fort {1} dungeon {2}", uid, msg.FortId, msg.DungeonId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CreateCampDungeon(msg.FortId,msg.DungeonId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("CampCreateDungeon {1} fail, player {0} is offline.", uid, msg.DungeonId);
                }
                else
                {
                    Log.WarnLine("CampCreateDungeon {1} fail, can not find player {0} .", uid, msg.DungeonId);
                }
            }
        }
    }
}
