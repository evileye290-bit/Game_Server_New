using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using CommonUtility;
using System.Collections.Generic;
using DBUtility;
using System;
using Message.Relation.Protocol.RC;
using Message.Corss.Protocol.CorssR;

namespace CrossServerLib
{
    public partial class RelationServer 
    {
        public void OnResponse_GetShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_SHOW_PLAYER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_SHOW_PLAYER>(stream);
            Log.Write("player {0} get show player info find player {1}.", uid, pks.ShowPcUid);
            //到缓存中获取缓存信息
            ShowInfoMessage showInfo = RelationManager.ShowMng.GetShowInfo(pks.ShowPcUid);
            if (showInfo != null)
            {
                //在缓存中找到信息，将信息发回ZONE
                MSG_ZRZ_RETURN_PLAYER_SHOW info = showInfo.Message;
                Write(info, uid);
            }
            else
            {
                int mianId = pks.MainId;
                //没有缓存信息，查看玩家是否在线
                FrontendServer relation = RelationManager.GetSinglePointServer(mianId);
                if (relation != null)
                {
                    //找到玩家说明玩家在线，通知玩家发送信息回来
                    MSG_CorssR_GET_SHOW_PLAYER msg = new MSG_CorssR_GET_SHOW_PLAYER();
                    msg.PcUid = pks.PcUid;
                    msg.ShowPcUid = pks.ShowPcUid;
                    msg.SeeMainId = this.MainId;
                    relation.Write(msg, uid);
                }
                else
                {
                    Log.Warn("player {0} get show player info find player {1} mainId {2} relation.", uid, pks.ShowPcUid, mianId);
                    ////没有找到玩家，通知ZONE自己去DB读取玩家信息
                    //MSG_CorssR_NOT_FIND_SHOW_PLAYER msg = new MSG_CorssR_NOT_FIND_SHOW_PLAYER();
                    //msg.PcUid = pks.PcUid;
                    //msg.ShowPcUid = pks.ShowPcUid;
                    //Write(msg, uid);
                }
            }
        }

        public void OnResponse_ReturnShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_PLAYER_SHOW pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_PLAYER_SHOW>(stream);
            if (pks.SeeMainId > 0)
            {
                //int mianId = BaseApi.GetMainIdByUid(uid);
                //没有缓存信息，查看玩家是否在线
                FrontendServer relation = RelationManager.GetSinglePointServer(pks.SeeMainId);
                if (relation != null)
                {
                    //找到玩家，将信息返回给zone
                    relation.Write(pks, uid);
                }
                else
                {
                    Log.Warn("player {0} ReturnShowPlayer info find player {1} mainId {2} relation.", uid, pks.ShowPcUid, pks.SeeMainId);
                }
            }

            if (pks.Result == (int)ErrorCode.Success)
            {
                pks.SeeMainId = 0;
                //将信息添加到缓存中
                RelationManager.ShowMng.AddShowInfo(pks);
            }
        }

        public void OnResponse_AddShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_PLAYER_SHOW pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_PLAYER_SHOW>(stream);
            if (pks.Info != null)
            {
                pks.Info.SeeMainId = 0;
                //将信息添加到缓存中
                RelationManager.ShowMng.AddShowInfo(pks.Info);
            }
        }

        public void OnResponse_GetChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_GET_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_CHALLENGER>(stream);
            Log.Write("player {0} get challenger info find player {1}.", uid, pks.ChallengerUid);

            //到缓存中获取缓存信息
            ChallengerInfoMessage challengerInfo = RelationManager.ChallengerMng.GetArenaChallengerInfo(pks.ChallengerUid);
            if (challengerInfo != null)
            {
                //在缓存中找到信息，将信息发回ZONE
                Write(challengerInfo.Message, uid);
            }
            else
            {
                int mianId = BaseApi.GetMainIdByUid(pks.ChallengerUid);
                //没有缓存信息，查看玩家是否在线
                FrontendServer relation = RelationManager.GetSinglePointServer(mianId);
                if (relation != null)
                {
                    //找到玩家说明玩家在线，通知玩家发送信息回来
                    MSG_CorssR_GET_CHALLENGER msg = new MSG_CorssR_GET_CHALLENGER();
                    msg.PcUid = pks.PcUid;
                    msg.ChallengerUid = pks.ChallengerUid;
                    msg.PcDefensive.AddRange(pks.PcDefensive);
                    msg.ChallengerDefensive.AddRange(pks.ChallengerDefensive);
                    msg.CDefPoses.AddRange(pks.CDefPoses);
                    msg.PDefPoses.AddRange(pks.PDefPoses);
                    //
                    msg.GetType = pks.GetType;
                    relation.Write(msg, uid);
                }
                else
                {
                    Log.Warn("player {0} get challenger info find player {1} mainId {2} relation.", uid, pks.ChallengerUid, mianId);
                    //没有找到玩家，通知ZONE自己去DB读取玩家信息
                    MSG_CorssR_NOT_FIND_CHALLENGER msg = new MSG_CorssR_NOT_FIND_CHALLENGER();
                    msg.PcUid = pks.PcUid;
                    msg.ChallengerUid = pks.ChallengerUid;
                    msg.PcDefensive.AddRange(pks.PcDefensive);
                    msg.ChallengerDefensive.AddRange(pks.ChallengerDefensive);
                    msg.CDefPoses.AddRange(pks.CDefPoses);
                    msg.PDefPoses.AddRange(pks.PDefPoses);
                    msg.GetType = pks.GetType;
                    Write(msg, uid);
                }
            }
        }

        public void OnResponse_AddChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO>(stream);
            if (pks.Info != null)
            {
                //将信息添加到缓存中
                RelationManager.ChallengerMng.AddArenaChallengerInfo(pks.Info, uid);
            }
        }

        public void OnResponse_ReturnChallenger(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_ARENA_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_ARENA_CHALLENGER>(stream);

            if (pks.Result == (int)ErrorCode.Success)
            {
                //将信息添加到缓存中
                RelationManager.ChallengerMng.AddArenaChallengerInfo(pks.Info, pks.ChallengerUid);
            }

            int mianId = BaseApi.GetMainIdByUid(uid);
            //没有缓存信息，查看玩家是否在线
            FrontendServer relation = RelationManager.GetSinglePointServer(mianId);
            if (relation != null)
            {
                //找到玩家，将信息返回给zone
                relation.Write(pks, uid);
            }
            else
            {
                Log.Warn("player {0} ReturnChallenger info find player {1} mainId {2} relation.", uid, pks.ChallengerUid, mianId);
            }
        }
     
    }
}
