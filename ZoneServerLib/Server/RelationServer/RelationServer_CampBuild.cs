using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using System.IO;
using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using System;
using Message.Gate.Protocol.GateZ;
using System.Collections.Generic;
using CommonUtility;
using ServerModels;
using StackExchange.Redis;
using Google.Protobuf.Collections;
using ServerShared;
using Message.Zone.Protocol.ZR;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        public CampBuildPhaseInfo TianDouCampBuild = new CampBuildPhaseInfo(CampType.TianDou);
        public CampBuildPhaseInfo XinLuoCampBuild = new CampBuildPhaseInfo(CampType.XingLuo);

        public void AskForCampBuildInfo()
        {
            MSG_ZR_CAMPBUILD_INFO requset = new MSG_ZR_CAMPBUILD_INFO();
            Write(requset);
        }

        private void OnResponse_CampBuildInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMPBUILD_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMPBUILD_INFO>(stream);
            Log.Write($"camp {msg.Camp} build with cur phase {msg.PhaseNum} Begin {msg.Begin} End {msg.End}");

            switch (msg.Camp)
            {
                case 1:// CampType.TianDou:
                    UpdataCampBuildPhaseInfo(msg, TianDouCampBuild);
                    break;
                case 2://CampType.XingLuo:
                    UpdataCampBuildPhaseInfo(msg, XinLuoCampBuild);
                    break;
                default:
                    break;
            }
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                if (player.CampBuild.PhaseNum < msg.PhaseNum)
                {
                    player.CampBuild.RefreshCampBuildAllInfo();
                }
                else
                {
                    player.SendCampBuildInfoMsg();
                    if (msg.NeedSync)
                    {
                        player.SendCampBuildSyncInfoMsg();
                    }
                }
            }
        }

        private void OnResponse_RestCampBuildCounter(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_RESET_CAMP_BUILD_COUNTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_RESET_CAMP_BUILD_COUNTER>(stream);
            Log.Write($"camp build reset counter {msg.Counter}");

            foreach (var player in Api.PCManager.PcList)
            {
                player.Value.SetCounter(CounterType.CampBuildRefreshDiceCount, msg.Counter);
            }

            foreach (var player in Api.PCManager.PcOfflineList)
            {
                player.Value.SetCounter(CounterType.CampBuildRefreshDiceCount, msg.Counter, false);
            }
        }

        private void OnResponse_CampBuildRest(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_CAMPBUILD_RESET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMPBUILD_RESET>(stream);
            //Log.Write($"camp build with cur phase {msg.PhaseNum} Begin {msg.Begin} End {msg.End}");

            //ResetCampBuildPhaseInfo(msg, TianDouCampBuild);
            //ResetCampBuildPhaseInfo(msg, XinLuoCampBuild);

            //foreach (var player in Api.PCManager.PcList)
            //{
            //    player.Value.CampBuild.RefreshCampBuildAllInfo();
            //}

            //foreach (var player in Api.PCManager.PcOfflineList)
            //{
            //    player.Value.CampBuild.RefreshCampBuildAllInfo();
            //}
        }

        //private void ResetCampBuildPhaseInfo(MSG_RZ_CAMPBUILD_RESET msg, CampBuildPhaseInfo campBuildModel)
        //{
        //    campBuildModel.Init(msg.PhaseNum, msg.Begin, msg.End,msg.NextBegin);
        //    campBuildModel.BuildingValue = 0;
        //}

        private void UpdataCampBuildPhaseInfo(MSG_RZ_CAMPBUILD_INFO msg, CampBuildPhaseInfo campBuildModel)
        {
            campBuildModel.Init(msg.PhaseNum, msg.Begin,msg.End,msg.NextBegin);
            campBuildModel.BuildingValue = msg.BuildingValue;
        }

        private void OnResponse_CampBuildRankList(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CAMPBUILD_RANK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CAMPBUILD_RANK_LIST>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                MSG_ZGC_CAMPBUILD_RANK_LIST response = new MSG_ZGC_CAMPBUILD_RANK_LIST();
                response.Page = msg.Page;
                response.TotalCount = msg.TotalCount;

                var info = GetCampBuildRankPlayerInfo(msg.PcInfo);
                if (info == null)
                {
                    info = GetCampBuildRankPlayerInfo(player);
                }
                else
                {
                    response.OwnerInfo = info;
                }
                foreach (var item in msg.RankList)
                {
                    response.RankList.Add(GetCampBuildRankPlayerInfo(item));
                }
                player.Write(response);
            }
            else
            {
                Log.Error("player {0} mainId {1} get camp build rank list fail,can not find player {0}", uid, MainId, uid);
            }

        }
        private CAMP_BUILD_RANK_INFO GetCampBuildRankPlayerInfo(CAMPBUILD_RANK_INFO playerInfo)
        {
            if (playerInfo == null)
            {
                return null;
            }

            CAMP_BUILD_RANK_INFO info = new CAMP_BUILD_RANK_INFO();
            info.Uid = playerInfo.Uid;
            info.Rank = playerInfo.Rank;
            info.Name = playerInfo.Name;
            info.Sex = playerInfo.Sex;
            info.Icon = playerInfo.Icon;
            info.ShowDIYIcon = playerInfo.ShowDIYIcon;
            info.IconFrame = playerInfo.IconFrame;
            info.Level = playerInfo.Level;
            info.BuildValue = playerInfo.BuildValue;
            info.TitleLevel = playerInfo.TitleLevel;

            return info;
        }

        private CAMP_BUILD_RANK_INFO GetCampBuildRankPlayerInfo(PlayerChar player)
        {
            if (player == null)
            {
                return null;
            }

            CAMP_BUILD_RANK_INFO info = new CAMP_BUILD_RANK_INFO();
            info.Uid = player.Uid;
            info.Rank = 0;
            info.Name = player.Name;
            info.Sex = player.Sex;
            info.Icon = player.Icon;
            info.ShowDIYIcon = player.ShowDIYIcon;
            info.IconFrame = player.GetFaceFrame();
            info.Level = player.Level;
            info.BuildValue = 0;
            info.TitleLevel = 0;

            return info;
        }


    }
}
