using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_CounterBuyCount(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_COUNTER_BUY_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_COUNTER_BUY_COUNT>(stream);

            MSG_GateZ_COUNTER_BUY_COUNT request = new MSG_GateZ_COUNTER_BUY_COUNT();
            request.Uid = Uid;
            request.CounterType = msg.CounterType;
            request.Count = msg.Count;

            WriteToZone(request);
        }

        public void OnResponse_GetSpecialCount(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_SPECIAL_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_SPECIAL_COUNT>(stream);
            MSG_GateZ_GET_SPECIAL_COUNT request = new MSG_GateZ_GET_SPECIAL_COUNT();
            request.Uid = Uid;
            WriteToZone(request);
        }   
    }
}
