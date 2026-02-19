using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    partial class Client
    {
        private void OnResponse_WishLanternSelect(MemoryStream stream)
        {
            MSG_CG_WISH_LANTERN_SELECT_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_WISH_LANTERN_SELECT_REWARD>(stream);
            WriteToZone(new MSG_GateZ_WISH_LANTERN_SELECT_REWARD() { Index = msg.Index });
        }

        private void OnResponse_XuanBoxRandom(MemoryStream stream)
        {
            MSG_CG_XUANBOX_RANDOM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_XUANBOX_RANDOM>(stream);
            MSG_GateZ_XUANBOX_RANDOM request = new MSG_GateZ_XUANBOX_RANDOM() {Num = msg.Num};
            WriteToZone(request);
        }

        private void OnResponse_XuanBoxReward(MemoryStream stream)
        {
            MSG_CG_XUANBOX_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_XUANBOX_REWARD>(stream);
            WriteToZone(new MSG_GateZ_XUANBOX_REWARD() { Id = msg.Id});
        }
    }
}
