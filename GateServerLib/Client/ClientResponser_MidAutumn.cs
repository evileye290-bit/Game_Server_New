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
        public void OnResponse_GetMidAutumnInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_MIDAUTUMN_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_MIDAUTUMN_INFO>(stream);
            MSG_GateZ_GET_MIDAUTUMN_INFO request = new MSG_GateZ_GET_MIDAUTUMN_INFO();
            WriteToZone(request);
        }

        public void OnResponse_DrawMidAutumnReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DRAW_MIDAUTUMN_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DRAW_MIDAUTUMN_REWARD>(stream);
            MSG_GateZ_DRAW_MIDAUTUMN_REWARD request = new MSG_GateZ_DRAW_MIDAUTUMN_REWARD();
            request.Free = msg.Free;
            request.Consecutive = msg.Consecutive;
            WriteToZone(request);
        }

        public void OnResponse_GetMidAutumnScoreReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_MIDAUTUMN_SCORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_MIDAUTUMN_SCORE_REWARD>(stream);
            MSG_GateZ_GET_MIDAUTUMN_SCORE_REWARD request = new MSG_GateZ_GET_MIDAUTUMN_SCORE_REWARD();
            request.RewardId = msg.RewardId;         
            WriteToZone(request);
        }
    }
}
