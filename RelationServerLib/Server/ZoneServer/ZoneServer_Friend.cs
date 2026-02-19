using System;
using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using RedisUtility;
using Message.Relation.Protocol.RR;
using ServerFrame;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        //public void OnResponse_ChallengePlayerRequst(MemoryStream stream, int uid = 0)
        //{
        //    MSG_ZR_CHALLENGE_PLAYER_REQUEST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CHALLENGE_PLAYER_REQUEST>(stream);
        //    Log.Write("player {0} mainId {1} request challenge player {2} mainId {3}", msg.Attacker.Uid, MainId, msg.DefenderUid,msg.MainId);

        //    if (msg.MainId == MainId)
        //    {
        //        Client defenderClient = ZoneManager.GetClient(msg.DefenderUid);
        //        if (defenderClient != null)
        //        {
        //            MSG_RZ_CHALLENGE_PLAYER_REQUEST response = new MSG_RZ_CHALLENGE_PLAYER_REQUEST();
        //            response.Attacker = PlayerInfo.GetRZPlayerBaseInfo(msg.Attacker);
        //            response.DefenderUid = msg.DefenderUid;
        //            response.AttackerMainId = msg.MainId;
        //            defenderClient.CurZone.Write(response);
        //        }
        //        else
        //        {
        //            Log.Warn("player {0} requst challenge player {1} failed: not in client list", msg.Attacker.Uid, msg.DefenderUid);
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        MSG_RR_CHALLENGE_PLAYER_REQUEST r2rMsg = new MSG_RR_CHALLENGE_PLAYER_REQUEST();
        //        r2rMsg.Attacker = PlayerInfo.GetRRPlayerBaseInfo(msg.Attacker);
        //        r2rMsg.DefenderUid = msg.DefenderUid;
        //        r2rMsg.MainId = msg.MainId;
        //        BaseServer relation = Api.GetRelationServer(msg.MainId);
        //        if (relation != null)
        //        {
        //            relation.Write(r2rMsg);
        //        }
        //    }
        //}

        public void OnResponse_FriendHeartGive(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_FRIEND_HEART_GIVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FRIEND_HEART_GIVE>(stream);
            Log.Write("player {0} mainId {1} give friend heart to player {2}", uid, MainId, msg.FriendUid);

            Client friend = ZoneManager.GetClient(msg.FriendUid);
            if (friend != null)
            {
                MSG_RZ_FRIEND_HEART_GIVE response = new MSG_RZ_FRIEND_HEART_GIVE();
                response.GiveHeartUid = uid;
                friend.CurZone.Write(response,msg.FriendUid);
            }

            //friend = ZoneManager.GetOfflineClient(msg.FriendUid);
            //if (friend != null)
            //{
            //    MSG_RZ_FRIEND_HEART_GIVE response = new MSG_RZ_FRIEND_HEART_GIVE();
            //    response.GiveHeartUid = uid;
            //    friend.CurZone.Write(response, msg.FriendUid);
            //}

            //else
            //{
            //    Log.Error("player {0} mainId {1} give friend heart to player {2} fail,can not find player {2}", uid, MainId, msg.FriendUid);
            //}
        }

        public void OnResponse_FriendInvite(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_FRIEND_INVITE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FRIEND_INVITE>(stream);

            Client client = ZoneManager.GetClient(msg.FriendUid);
            if (client != null)
            {
                //client
                MSG_RZ_FRIEND_INVITE res = new MSG_RZ_FRIEND_INVITE();
                res.InviterUid = uid;
                client.CurZone.Write(res, msg.FriendUid);
            }
            //else
            //{
            //    Logger.Log.Warn($"try get client {msg.FriendUid} failed in OnResponse_FriendInvite");
            //}
        }

        public void OnResponse_FriendResponse(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_FRIEND_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FRIEND_RESPONSE>(stream);
            Client client = ZoneManager.GetClient(msg.InviterUid);
            if (client != null)
            {
                MSG_RZ_FRIEND_RESPONSE res = new MSG_RZ_FRIEND_RESPONSE();
                res.FriendUid = msg.ResponserUid;
                res.Agree = msg.Agree;
                client.CurZone.Write(res,msg.InviterUid);
            }
            //else
            //{
            //    Logger.Log.Warn($"try get client {msg.ResponserUid} failed in OnResponse_FriendResponse");
            //}
        }

        public void OnResponse_FriendRemove(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_FRIEND_REMOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_FRIEND_REMOVE>(stream);
            Client client = ZoneManager.GetClient(msg.FriendUid);
            if (client != null)
            {
                MSG_RZ_FRIEND_REMOVE res = new MSG_RZ_FRIEND_REMOVE();
                res.FriendUid = uid;
                client.CurZone.Write(res, msg.FriendUid);
            }
            else
            {
                Logger.Log.Warn($"try get client {msg.FriendUid} failed in OnResponse_FriendRemove");
            }
        }


    }
}
