using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_IslandHighInfo(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request IslandHighInfo", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetHighInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("IslandHighInfo fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("IslandHighInfo fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_IslandHighRock(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_HIGH_ROCK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_HIGH_ROCK>(stream);
            Log.Write("player {0} request IslandHighRock type {1}", uid, msg.Type);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HighRock(msg.Type, msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("IslandHighRock fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("IslandHighRock fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_IslandHighReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_HIGH_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_HIGH_REWARD>(stream);
            Log.Write("player {0} request IslandHighReward type {1}", uid, msg.Type);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.IslandHighReward(msg.Type, msg.RewardId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("IslandHighReward fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("IslandHighReward fail, can not find player {0} .", uid);
                }
            }
        }


        //private void OnResponse_IslandHighBuyItem(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GateZ_ISLAND_HIGH_BUY_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_HIGH_BUY_ITEM>(stream);
        //    Log.Write("player {0} request IslandHighBuyItem", uid);

        //    PlayerChar player = Api.PCManager.FindPc(uid);
        //    if (player != null)
        //    {
        //        player.HighBuyItem(msg.ItemId);
        //    }
        //    else
        //    {
        //        player = Api.PCManager.FindOfflinePc(uid);
        //        if (player != null)
        //        {
        //            Log.WarnLine("IslandHighBuyItem fail, player {0} is offline.", uid);
        //        }
        //        else
        //        {
        //            Log.WarnLine("IslandHighBuyItem fail, can not find player {0} .", uid);
        //        }
        //    }
        //}
    }
}
