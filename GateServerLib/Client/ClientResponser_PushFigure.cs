using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_PushFigureFinishTask(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PUSHFIGURE_FINISHTASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PUSHFIGURE_FINISHTASK>(stream);
            MSG_GateZ_PUSHFIGURE_FINISHTASK request = new MSG_GateZ_PUSHFIGURE_FINISHTASK()
            {
                Id = msg.Id,
            };

            WriteToZone(request);
        }
    }
}
