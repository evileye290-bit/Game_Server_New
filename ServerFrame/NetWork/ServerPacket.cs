
using System.IO;
namespace ServerFrame
{
    public class ServerPacket
    {
        public uint MsgId;
        public int Uid;
        public MemoryStream Msg;
        public ServerPacket(uint msg_id, int uid)
        {
            MsgId = msg_id;
            Uid = uid;
        }
    }
}
