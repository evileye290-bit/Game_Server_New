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
        public void OnResponse_GetNineTestInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_NINETEST_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_NINETEST_INFO>(stream);
            MSG_GateZ_GET_NINETEST_INFO request = new MSG_GateZ_GET_NINETEST_INFO();
            WriteToZone(request);
        }

        public void OnResponse_NineTestClickGrid(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_NINETEST_CLICK_GRID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_NINETEST_CLICK_GRID>(stream);
            MSG_GateZ_NINETEST_CLICK_GRID request = new MSG_GateZ_NINETEST_CLICK_GRID();
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_NineTestScoreReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_NINETEST_SCORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_NINETEST_SCORE_REWARD>(stream);
            MSG_GateZ_NINETEST_SCORE_REWARD request = new MSG_GateZ_NINETEST_SCORE_REWARD();
            request.RewardId = msg.RewardId;
            WriteToZone(request);
        }

        public void OnResponse_NineTestReset(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_NINETEST_RESET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_NINETEST_RESET>(stream);
            MSG_GateZ_NINETEST_RESET request = new MSG_GateZ_NINETEST_RESET();
            request.Free = msg.Free;
            WriteToZone(request);
        }
    }
}
