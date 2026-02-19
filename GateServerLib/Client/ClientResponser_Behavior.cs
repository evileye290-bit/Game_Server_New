using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_CharacterMove(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHARACTER_MOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHARACTER_MOVE>(stream);
            MSG_GateZ_CHARACTER_MOVE response = new MSG_GateZ_CHARACTER_MOVE();
            response.Uid = Uid;
            response.X = msg.X;
            response.Y = msg.Y;
            WriteToZone(response);
        }

        public void OnResponse_MoveZone(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_MOVE_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_MOVE_ZONE>(stream);
            MSG_GateZ_MOVE_ZONE request = new MSG_GateZ_MOVE_ZONE();
            request.Uid = Uid;
            request.MapId = msg.MapId;
            WriteToZone(request);
        }
    }
}
