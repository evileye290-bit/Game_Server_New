using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_CommentGame(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SAVE_GAME_COMMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SAVE_GAME_COMMENT>(stream);
            int thumbsUp = msg.ThumbsUp;

            MSG_GateZ_SAVE_GAME_COMMENT request = new MSG_GateZ_SAVE_GAME_COMMENT();
            request.PcUid = Uid;
            request.ThumbsUp = thumbsUp;

            WriteToZone(request);
        }
    }
}
