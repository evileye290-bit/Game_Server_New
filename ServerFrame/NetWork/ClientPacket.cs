
using System.IO;
namespace ServerFrame
{
    public class ClientPacket
    {
        public uint MsgId;
        public MemoryStream Msg;
        public ClientPacket(uint msg_id)
        {
            MsgId = msg_id;
        }
    }
}
