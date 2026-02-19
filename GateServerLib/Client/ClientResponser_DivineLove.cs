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
        public void OnResponse_GetDivineLoveInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_DIVINE_LOVE_VALUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_DIVINE_LOVE_VALUE>(stream);
            MSG_GateZ_GET_DIVINE_LOVE_VALUE request = new MSG_GateZ_GET_DIVINE_LOVE_VALUE();

            WriteToZone(request);
        }

        public void OnResponse_GetDivineLoveReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_DIVINE_LOVE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_DIVINE_LOVE_REWARD>(stream);
            MSG_GateZ_GET_DIVINE_LOVE_REWARD request = new MSG_GateZ_GET_DIVINE_LOVE_REWARD();
            request.Id = msg.Id;
            request.UseDiamond = msg.UseDiamond;
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_GetDivineLoveCumulateReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_DIVINE_LOVE_CUMULATE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_DIVINE_LOVE_CUMULATE_REWARD>(stream);
            MSG_GateZ_GET_DIVINE_LOVE_CUMULATE_REWARD request = new MSG_GateZ_GET_DIVINE_LOVE_CUMULATE_REWARD();
            request.Id = msg.Id;
            request.HeartNum = msg.HeartNum;
            WriteToZone(request);
        }

        public void OnResponse_BuyDivineLoveItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_DIVINE_LOVE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_DIVINE_LOVE_ITEM>(stream);
            MSG_GateZ_BUY_DIVINE_LOVE_ITEM request = new MSG_GateZ_BUY_DIVINE_LOVE_ITEM();
            request.Id = msg.Id;
            request.Num = msg.Num;
            WriteToZone(request);
        }

        public void OnResponse_OpenDivineLoveRound(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_OPEN_DIVINE_LOVE_ROUND msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_OPEN_DIVINE_LOVE_ROUND>(stream);
            MSG_GateZ_OPEN_DIVINE_LOVE_ROUND request = new MSG_GateZ_OPEN_DIVINE_LOVE_ROUND();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_CloseDivineLoveRound(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CLOSE_DIVINE_LOVE_ROUND msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CLOSE_DIVINE_LOVE_ROUND>(stream);
            MSG_GateZ_CLOSE_DIVINE_LOVE_ROUND request = new MSG_GateZ_CLOSE_DIVINE_LOVE_ROUND();
            request.Id = msg.Id;
            WriteToZone(request);
        }
    }
}
