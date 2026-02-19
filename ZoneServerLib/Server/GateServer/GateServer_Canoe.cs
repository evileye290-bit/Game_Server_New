using Logger;
using Message.Gate.Protocol.GateZ;
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
        private void OnResponse_GetCanoeInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_CANOE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_CANOE_INFO>(stream);
            Log.Write("player {0} GetCanoeInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.GetCanoeInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("GetCanoeInfo fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("GetCanoeInfo, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_CanoeGameStart(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CANOE_GAME_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CANOE_GAME_START>(stream);
            Log.Write("player {0} CanoeGameStart", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CanoeGameStart(msg.Type);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("CanoeGameStart fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("CanoeGameStart, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_CanoeGameEnd(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CANOE_GAME_END msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CANOE_GAME_END>(stream);
            Log.Write("player {0} CanoeGameEnd", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CanoeGameEnd(msg.Type, msg.Index);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("CanoeGameEnd fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("CanoeGameEnd, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_CanoeGetReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CANOE_GET_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CANOE_GET_REWARD>(stream);
            Log.Write("player {0} CanoeGetReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.CanoeGetReward(msg.RewardId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("CanoeGetReward fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("CanoeGetReward, can not find player {0} .", uid);
                }
            }
        }
    }
}
