using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_FriendSearch(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_SEARCH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_SEARCH>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} Friend Search keyword {1}", pcUid, msg.KeyWord);
            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.SearchFriend(msg.KeyWord);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline. .can not search friend", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} friend search fail：can not find player.", pcUid);
                }
            }
        }

        public void OnResponse_RecommendFriend(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_RECOMMEND msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_RECOMMEND>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} Recommend Friend", pcUid);
            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.RecommendPlayers();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .can not get recommend players", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} get recommend players can not find player.", pcUid);
                }
            }
        }


        public void OnResponse_FriendAdd(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_ADD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_ADD>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} Add Friend Uid {1} Flag {2}", pcUid, msg.FriendUid, msg.Flag);
            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.FriendInvite(msg.FriendUid,msg.Flag);
                player.KomoeEventLogFriendFlow(1,"好友申请",msg.FriendUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline. can not add friend .", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} AddFriendRequest not find player.", pcUid);
                }
            }
        }

        public void OnResponse_FriendDelete(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_DELETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_DELETE>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} Delete Friend Uid {1} Flag {2}", pcUid, msg.FriendUid, msg.Flag);
            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.DelFriend(msg.FriendUid,msg.Flag);
                player.KomoeEventLogFriendFlow(4, "好友删除", msg.FriendUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline.can not delete friend.", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} deletet friend fail not find player.", pcUid);
                }
            }
        }

        public void OnResponse_FriendList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_LIST>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} get friend list", pcUid);

            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.GetFriendList();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .can not get friend list", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} get friend list can not find player.", pcUid);
                }
            }
        }

        public void OnResponse_BlackAdd(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_BLACK_ADD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_BLACK_ADD>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} add friend {1} to black", pcUid, msg.FriendUid);
            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.AddBlack(msg.FriendUid);
                player.KomoeEventLogFriendFlow(5, "拉入黑名单", msg.FriendUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} add black is offline.", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} addblack not find player.", pcUid);
                }
            }
        }

        public void OnResponse_BlackDel(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_BLACK_DEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_BLACK_DEL>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} del friend {1} from black", pcUid, msg.FriendUid);
            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.DelBlack(msg.FriendUid);
                player.KomoeEventLogFriendFlow(6, "移出黑名单", msg.FriendUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} del black is offline.", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} del black not find player.", pcUid);
                }
            }
        }

        public void OnResponse_FriendBlackList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_BLACK_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_BLACK_LIST>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} get friend black list", pcUid);

            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.GetBlackList();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .can not get black list", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} get black list can not find player.", pcUid);
                }
            }
        }


        public void OnResponse_FriendRecentList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_RECENT_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_RECENT_LIST>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} get friend recent list", pcUid);

            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.GetRecentList(msg.Ids);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .can not get recent list", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} get recent list can not find player.", pcUid);
                }
            }
        }


        public void OnResponse_FriendResponse(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_RESPONSE>(stream);
            Log.Write("player {0} request FriendResponse: inviterUid {1} agree {2}", uid, msg.InviterUid, msg.Agree.ToString());

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.FriendInviteResponse(msg.InviterUid, msg.Agree);
                player.KomoeEventLogFriendFlow(2, "同意申请", msg.InviterUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .can not OnResponse_FriendResponse ", uid);
                }
                else
                {
                    Log.WarnLine("player {0} OnResponse_FriendResponse fail ,can not find player.", uid);
                }
            }
        }

        public void OnResponse_OnekeyIgnoreInviter(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ONEKEY_IGNORE_INVITER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ONEKEY_IGNORE_INVITER>(stream);
            Log.Write("player {0} request OnekeyIgnoreInviter", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.OneKeyIgnoreFriendInvite();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .OnResponse_OnekeyIgnoreInviter ", uid);
                }
                else
                {
                    Log.WarnLine("player {0} OnResponse_OnekeyIgnoreInviter fail ,can not find player.", uid);
                }
            }
        }

        

    }
}
