using System.IO;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;

namespace GateServerLib
{
    partial class Client
    {
        private void OnResponse_GetXuanBoxInfo(MemoryStream stream)
        {
            MSG_GateZ_XUANBOX_GET_INFO request = new MSG_GateZ_XUANBOX_GET_INFO();
            WriteToZone(request);
        }

        private void OnResponse_WishLanternLight(MemoryStream stream)
        {
            MSG_CG_WISH_LANTERN_LIGHT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_WISH_LANTERN_LIGHT>(stream);
            WriteToZone(new MSG_GateZ_WISH_LANTERN_LIGHT() { Index = msg.Index });
        }

        private void OnResponse_WishLanternReset(MemoryStream stream)
        {
            WriteToZone(new MSG_GateZ_WISH_LANTERN_RESET());
        }
    }
}
