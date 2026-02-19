using EnumerateUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_BrotherInvite(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BROTHERS_INVITE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BROTHERS_INVITE>(stream);

            MSG_GateZ_BROTHERS_INVITE requset = new MSG_GateZ_BROTHERS_INVITE();
            requset.FriendUid = msg.FriendUid;
            WriteToZone(requset);
        }

        public void OnResponse_BrotherResponse(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BROTHERS_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BROTHERS_RESPONSE>(stream);
            MSG_GateZ_BROTHERS_RESPONSE requset = new MSG_GateZ_BROTHERS_RESPONSE();
            requset.InviterUid = msg.InviterUid;
            requset.Agree = msg.Agree;
            WriteToZone(requset);
        }


        public void OnResponse_BrotherRemove(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BROTHERS_REMOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BROTHERS_REMOVE>(stream);
            MSG_GateZ_BROTHERS_REMOVE requset = new MSG_GateZ_BROTHERS_REMOVE();
            requset.BrotherUid = msg.BrotherUid;
            WriteToZone(requset);
        }

        


    }
}
