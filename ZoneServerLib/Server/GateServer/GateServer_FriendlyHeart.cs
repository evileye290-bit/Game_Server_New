using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerShared;
using StackExchange.Redis;
using System.Collections.Generic;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_FriendHeartGive(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FRIEND_HEART_GIVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_HEART_GIVE>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} give heart to friend {1}", pcUid, msg.FriendUid);
            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player != null)
            {
                player.GiveHeart(msg.FriendUid);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline. .OnResponse_FriendHeartGive ", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0} OnResponse_FriendHeartGive fail：can not find player.", pcUid);
                }
            }
        }




        public void OnResponse_FriendHeartCountBuy(MemoryStream stream, int uid = 0)
        {
            Log.Warn("discard msg MSG_CG_FRIEND_HEART_COUNT_BUY");
            //MSG_GateZ_FRIEND_HEART_COUNT_BUY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FRIEND_HEART_COUNT_BUY>(stream);
            //int pcUid = msg.PcUid;
            //PlayerChar player = Api.PCManager.FindPc(pcUid);
            //if (player != null)
            //{
            //    player.FriendHeartCountBuy(msg.IsGiveCount);
            //}
            //else
            //{
            //    player = Api.PCManager.FindOfflinePc(pcUid);
            //    if (player != null)
            //    {
            //        Log.WarnLine("player {0} is offline. .can not buy heart {1} count ", pcUid, msg.IsGiveCount);
            //    }
            //    else
            //    {
            //        Log.WarnLine("player {0} buy heart {1} count fail：can not find player.", pcUid, msg.IsGiveCount);
            //    }
            //}
        }

        public void OnResponse_RepayFriendsHeart(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_REPAY_FRIENDS_HEART msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_REPAY_FRIENDS_HEART>(stream);
            int pcUid = msg.PcUid;
            Log.Write("player {0} repay friends heart", pcUid);
            List<int> friendList = new List<int>();
            friendList.AddRange(msg.FriendUids);


            PlayerChar player = Api.PCManager.FindPc(pcUid);
            if (player == null)
            {
                player = Api.PCManager.FindOfflinePc(pcUid);
                if (player != null)
                {
                    Log.WarnLine("player {0} is offline .can not repay friends heart", pcUid);
                }
                else
                {
                    Log.WarnLine("player {0}  repay friends heart can not find player.", pcUid);
                }
                return;
            }

            player.OneKeyGiveHeart();

            
        }
    }
}
