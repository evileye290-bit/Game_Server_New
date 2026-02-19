using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_BenefitInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BENEFIT_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BENEFIT_INFO>(stream);
            MSG_GateZ_BENEFIT_INFO request = new MSG_GateZ_BENEFIT_INFO();
            WriteToZone(request);
        }

        public void OnResponse_BenefitSweep(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BENEFIT_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BENEFIT_SWEEP>(stream);
            MSG_GateZ_BENEFIT_SWEEP request = new MSG_GateZ_BENEFIT_SWEEP();
            request.Id = msg.Id;
            WriteToZone(request);
        }
    }
}
