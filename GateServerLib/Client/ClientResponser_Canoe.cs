using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_GetCanoeInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CANOE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_CANOE_INFO>(stream);
            MSG_GateZ_GET_CANOE_INFO request = new MSG_GateZ_GET_CANOE_INFO();
            WriteToZone(request);
        }

        public void OnResponse_CanoeGameStart(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CANOE_GAME_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CANOE_GAME_START>(stream);
            MSG_GateZ_CANOE_GAME_START request = new MSG_GateZ_CANOE_GAME_START();
            request.Type = msg.Type;
            WriteToZone(request);
        }

        public void OnResponse_CanoeGameEnd(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CANOE_GAME_END msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CANOE_GAME_END>(stream);
            MSG_GateZ_CANOE_GAME_END request = new MSG_GateZ_CANOE_GAME_END();
            request.Type = msg.Type;
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_CanoeGetReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CANOE_GET_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CANOE_GET_REWARD>(stream);
            MSG_GateZ_CANOE_GET_REWARD request = new MSG_GateZ_CANOE_GET_REWARD();
            request.RewardId = msg.RewardId;          
            WriteToZone(request);
        }
    }
}
