using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RC;
using Message.Corss.Protocol.CorssR;

namespace RelationServerLib
{
    public partial class CrossServer
    {
        public void OnResponse_GetShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_SHOW_PLAYER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_SHOW_PLAYER>(stream);
            Log.Write("player {0} get show player info find player {1}.", uid, pks.ShowPcUid);
            //到缓存中获取缓存信息
            ShowInfoMessage showInfo = Api.ZoneManager.ShowMng.GetShowInfo(pks.ShowPcUid);
            if (showInfo != null)
            {
                //在缓存中找到信息，将信息发回ZONE
                MSG_ZRZ_RETURN_PLAYER_SHOW info = showInfo.Message;
                info.SeeMainId = pks.SeeMainId;
                Write(info, uid);
            }
            else
            {
                //没有缓存信息，查看玩家是否在线
                Client client = Api.ZoneManager.GetClient(pks.ShowPcUid);
                if (client != null)
                {
                    //找到玩家说明玩家在线，通知玩家发送信息回来
                    MSG_RZ_GET_SHOW_PLAYER msg = new MSG_RZ_GET_SHOW_PLAYER();
                    msg.PcUid = pks.PcUid;
                    msg.ShowPcUid = pks.ShowPcUid;
                    msg.SeeMainId = pks.SeeMainId;
                    client.CurZone.Write(msg, uid);
                }
                else
                {
                    //没有找到玩家，通知ZONE自己去DB读取玩家信息
                    MSG_RZ_ONE_SERVER_FIND_SHOW_PLAYER msg = new MSG_RZ_ONE_SERVER_FIND_SHOW_PLAYER();
                    msg.PcUid = pks.PcUid;
                    msg.ShowPcUid = pks.ShowPcUid;
                    msg.SeeMainId = pks.SeeMainId;
                    FrontendServer server = Api.ZoneManager.GetOneServer();
                    server.Write(msg, uid);
                }
            }
        }

        public void OnResponse_ReturnShowPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_PLAYER_SHOW pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_PLAYER_SHOW>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                //没有缓存信息，查看玩家是否在线
                Log.Warn("player {0} return show player find show player {1} failed: not find ", uid, pks.ShowPcUid);
                return;
            }
            //if (pks.Result == (int)ErrorCode.Success)
            //{
            //    //将信息添加到缓存中
            //    Api.ZoneManager.ShowMng.AddShowInfo(pks);
            //}
            client.Write(pks);
        }

        public void OnResponse_GetChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_GET_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_GET_CHALLENGER>(stream);
            Log.Write("player {0} get arena challenger info find player {1}.", uid, pks.ChallengerUid);

            //到缓存中获取缓存信息
            ArenaChallengerInfoMessage challengerInfo = Api.ZoneManager.GetArenaChallengerInfo(pks.ChallengerUid);
            if (challengerInfo != null)
            {
                //在缓存中找到信息，将信息发回ZONE
                Write(challengerInfo.Message, uid);
            }
            else
            {
                //没有缓存信息，查看玩家是否在线
                Client client = Api.ZoneManager.GetClient(pks.ChallengerUid);
                if (client != null)
                {
                    //找到玩家说明玩家在线，通知玩家发送信息回来
                    MSG_RZ_GET_ARENA_CHALLENGER msg = new MSG_RZ_GET_ARENA_CHALLENGER();
                    msg.PcUid = pks.PcUid;
                    msg.ChallengerUid = pks.ChallengerUid;
                    msg.ChallengerDefensive.AddRange(pks.ChallengerDefensive);
                    msg.PcDefensive.AddRange(pks.PcDefensive);
                    msg.CDefPoses.AddRange(pks.CDefPoses);
                    msg.PDefPoses.AddRange(pks.PDefPoses);
                    msg.GetType = pks.GetType;
                    client.Write(msg);
                }
                else
                {
                    //没有找到玩家，通知ZONE自己去DB读取玩家信息
                    MSG_RZ_NOT_FIND_ARENA_CHALLENGER msg = new MSG_RZ_NOT_FIND_ARENA_CHALLENGER();
                    msg.PcUid = pks.PcUid;
                    msg.ChallengerUid = pks.ChallengerUid;
                    msg.PcDefensive.AddRange(pks.PcDefensive);
                    msg.ChallengerDefensive.AddRange(pks.ChallengerDefensive);
                    msg.CDefPoses.AddRange(pks.CDefPoses);
                    msg.PDefPoses.AddRange(pks.PDefPoses);
                    msg.GetType = pks.GetType;

                    FrontendServer server = Api.ZoneManager.GetOneServer();
                    server.Write(msg, uid);
                }
            }
        }

        public void OnResponse_ReturnChallengerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZRZ_RETURN_ARENA_CHALLENGER pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZRZ_RETURN_ARENA_CHALLENGER>(stream);

            Client client = Api.ZoneManager.GetClient(uid);
            if (client == null)
            {
                //没有缓存信息，查看玩家是否在线
                Log.Warn("player {0} return challenger find show player {1} failed: not find ", uid, pks.ChallengerUid);
                return;
            }
            //if (pks.Result == (int)ErrorCode.Success)
            //{
            //    //将信息添加到缓存中
            //    Api.ZoneManager.AddArenaChallengerInfo(pks.Info, pks.ChallengerUid);
            //}
            client.Write(pks);
        }
    }
}
