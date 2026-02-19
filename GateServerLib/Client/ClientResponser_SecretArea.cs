using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_SecretAreaInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SECRET_AREA_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SECRET_AREA_INFO>(stream);
            MSG_GateZ_SECRET_AREA_INFO request = new MSG_GateZ_SECRET_AREA_INFO();
            WriteToZone(request);
        }

        public void OnResponse_SecretAreaSweep(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SECRET_AREA_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SECRET_AREA_SWEEP>(stream);
            MSG_GateZ_SECRET_AREA_SWEEP request = new MSG_GateZ_SECRET_AREA_SWEEP();
            request.Id = msg.Id;
            WriteToZone(request);
        }

        public void OnResponse_SecretAreaRankInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SECRET_AREA_RANK_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SECRET_AREA_RANK_INFO>(stream);
            MSG_GateZ_SECRET_AREA_RANK_INFO request = new MSG_GateZ_SECRET_AREA_RANK_INFO();
            request.RankType = msg.RankType;
            request.Page = msg.Page;
            WriteToZone(request);
        }

        public void OnResponse_SecretAreaContinueFight(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SECRET_AREA_CONT_FIGHT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SECRET_AREA_CONT_FIGHT>(stream);
            MSG_GateZ_SECRET_AREA_CONT_FIGHT request = new MSG_GateZ_SECRET_AREA_CONT_FIGHT();
            request.ContinueFight = msg.ContinueFight;
            WriteToZone(request);
        }
    }
}
