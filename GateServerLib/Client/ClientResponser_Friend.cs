using EnumerateUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_SearchFriend(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_SEARCH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_SEARCH>(stream);
            if (IsGm == 0)
            {
                //检查字长
                int nameLen = server.NameChecker.GetWordLen(msg.KeyWord);//frTOD0:如果这里是id搜索，长度得加前缀长度
                if (nameLen > WordLengthLimit.CharNameLenLimit)
                {
                    MSG_ZGC_FRIEND_SEARCH notify = new MSG_ZGC_FRIEND_SEARCH();
                    notify.Result = (int)ErrorCode.LengthLimit;
                    Write(notify);
                    return;
                }
                //检查屏蔽字
                if (server.NameChecker.HasSpecialSymbol(msg.KeyWord) || server.NameChecker.HasBadWord(msg.KeyWord))
                {
                    MSG_ZGC_FRIEND_SEARCH notify = new MSG_ZGC_FRIEND_SEARCH();
                    notify.Result = (int)ErrorCode.BadWord;
                    Write(notify);
                    return;
                }
            }

            MSG_GateZ_FRIEND_SEARCH response = new MSG_GateZ_FRIEND_SEARCH();
            response.PcUid = Uid;
            response.KeyWord = msg.KeyWord;
            WriteToZone(response);
        }

        public void OnResponse_RecommendFriend(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_RECOMMEND msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_RECOMMEND>(stream);
            MSG_GateZ_FRIEND_RECOMMEND response = new MSG_GateZ_FRIEND_RECOMMEND();
            response.PcUid = Uid;
            WriteToZone(response);
        }


        public void OnResponse_FriendAdd(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_ADD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_ADD>(stream);
            MSG_GateZ_FRIEND_ADD response = new MSG_GateZ_FRIEND_ADD();
            response.PcUid = Uid;
            response.FriendUid = msg.FriendUid;
            response.Flag = msg.Flag;
            WriteToZone(response);
        }

        public void OnResponse_FriendDelete(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_DELETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_DELETE>(stream);
            MSG_GateZ_FRIEND_DELETE response = new MSG_GateZ_FRIEND_DELETE();
            response.PcUid = Uid;
            response.FriendUid = msg.FriendUid;
            response.Flag = msg.Flag;
            WriteToZone(response);
        }

        public void OnResponse_FriendList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_LIST>(stream);
            MSG_GateZ_FRIEND_LIST response = new MSG_GateZ_FRIEND_LIST();
            response.PcUid = Uid;
            WriteToZone(response);
        }

        public void OnResponse_BlackAdd(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_BLACK_ADD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_BLACK_ADD>(stream);
            MSG_GateZ_FRIEND_BLACK_ADD response = new MSG_GateZ_FRIEND_BLACK_ADD();
            response.PcUid = Uid;
            response.FriendUid = msg.FriendUid;
            WriteToZone(response);
        }

        public void OnResponse_BlackDel(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_BLACK_DEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_BLACK_DEL>(stream);
            MSG_GateZ_FRIEND_BLACK_DEL response = new MSG_GateZ_FRIEND_BLACK_DEL();
            response.PcUid = Uid;
            response.FriendUid = msg.FriendUid;
            WriteToZone(response);
        }

        public void OnResponse_FriendBlackList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_BLACK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_BLACK_LIST>(stream);
            MSG_GateZ_FRIEND_BLACK_LIST response = new MSG_GateZ_FRIEND_BLACK_LIST();
            response.PcUid = Uid;
            WriteToZone(response);
        }

        public void OnResponse_FriendHeartGive(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_HEART_GIVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_HEART_GIVE>(stream);
            MSG_GateZ_FRIEND_HEART_GIVE response = new MSG_GateZ_FRIEND_HEART_GIVE();
            response.PcUid = Uid;
            response.FriendUid = msg.FriendUid;
            WriteToZone(response);
        }

        public void OnResponse_FriendHeartCountBuy(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_HEART_COUNT_BUY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_HEART_COUNT_BUY>(stream);
            MSG_GateZ_FRIEND_HEART_COUNT_BUY response = new MSG_GateZ_FRIEND_HEART_COUNT_BUY();
            response.PcUid = Uid;
            response.IsGiveCount = msg.IsGiveCount;
            WriteToZone(response);
        }


        public void OnResponse_FriendRecentList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_RECENT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_RECENT_LIST>(stream);
            MSG_GateZ_FRIEND_RECENT_LIST response = new MSG_GateZ_FRIEND_RECENT_LIST();
            response.PcUid = Uid;
            response.Ids.AddRange(msg.Ids);
            WriteToZone(response);
        }

        public void OnResponse_RepayFriendsHeart(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_REPAY_FRIENDS_HEART msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_REPAY_FRIENDS_HEART>(stream);
            MSG_GateZ_REPAY_FRIENDS_HEART response = new MSG_GateZ_REPAY_FRIENDS_HEART();
            response.PcUid = Uid;
            response.FriendUids.AddRange(msg.FriendUids);
            WriteToZone(response);
        }


        public void OnResponse_FriendResponse(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_FRIEND_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_FRIEND_RESPONSE>(stream);
            MSG_GateZ_FRIEND_RESPONSE requset = new MSG_GateZ_FRIEND_RESPONSE();
            requset.InviterUid = msg.InviterUid;
            requset.Agree = msg.Agree;
            WriteToZone(requset);
        }

        public void OnResponse_OnekeyIgnoreInviter(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ONEKEY_IGNORE_INVITER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ONEKEY_IGNORE_INVITER>(stream);
            MSG_GateZ_ONEKEY_IGNORE_INVITER requset = new MSG_GateZ_ONEKEY_IGNORE_INVITER();
            WriteToZone(requset);
        }

        


    }
}
