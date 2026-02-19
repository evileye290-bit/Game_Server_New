using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_SearchFriend(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_SEARCH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} search friend not find client", pcUid);
            }
        }

        private void OnResponse_RecommendFriend(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_RECOMMEND>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get recommend playerList not find client", pcUid);
            }
        }


        private void OnResponse_FriendAdd(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_ADD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} friend add not find client", pcUid);
            }
        }

        private void OnResponse_FriendDelete(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_DELETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} delete friend not find client", pcUid);
            }
        }

        private void OnResponse_FriendList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get friend list not find client", pcUid);
            }
        }

        private void OnResponse_BlackAdd(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_BLACK_ADD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} blacklist add not find client", pcUid);
            }
        }

        private void OnResponse_BlackDel(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_BLACK_DEL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} blacklist del requset not find client", pcUid);
            }
        }

        private void OnResponse_FriendBlackList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_BLACK_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get black list not find client", pcUid);
            }
        }

        private void OnResponse_FriendHeartGive(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_HEART_GIVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} give friend heart not find client", pcUid);
            }
        }


        public void OnResponse_FriendHeartGiveCount(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_HEART_GIVE_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update give heart count not find client", pcUid);
            }
        }

        public void OnResponse_FriendHeartTakeCount(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_HEART_TAKE_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update take heart count not find client", pcUid);
            }
        }


        private void OnResponse_FriendHeartCountBuy(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_HEART_COUNT_BUY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy heart count not find client", pcUid);
            }
        }


        private void OnResponse_FriendRecentList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_RECENT_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get recent list not find client", pcUid);
            }
        }

        private void OnResponse_RepayFriendsHeart(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REPAY_FRIENDS_HEART>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} repay friends heart not find client", pcUid);
            }
        }

        public void OnResponse_SyncFriendInviterList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SYNC_FRIEND_INVITER_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_SyncFriendInviterList can not find client", pcUid);
            }
        }

        public void OnResponse_FriendResponse(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FRIEND_RESPONSE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_FriendResponse can not find client", pcUid);
            }
        }


        public void OnResponse_OnekeyIgnoreInviter(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ONEKEY_IGNORE_INVITER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} OnResponse_OnekeyIgnoreInviter can not find client", pcUid);
            }
        }
    }
}
