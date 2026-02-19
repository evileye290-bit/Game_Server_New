using Logger;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_UploadPhoto(MemoryStream stream)
        {
            MSG_CG_UPLOAD_PHOTO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPLOAD_PHOTO>(stream);
            Log.Write("player {0} upload photo {1}", Uid, msg.PhotoName);
            MSG_GateZ_UPLOAD_PHOTO request = new MSG_GateZ_UPLOAD_PHOTO();
            request.Uid = Uid;
            request.PhotoName = msg.PhotoName;
            WriteToZone(request);
        }

        private void OnResponse_RemovePhoto(MemoryStream stream)
        {
            MSG_CG_REMOVE_PHOTO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_REMOVE_PHOTO>(stream);
            Log.Write("player {0} remove photo {1}", Uid, msg.PhotoName);
            MSG_GateZ_REMOVE_PHOTO request = new MSG_GateZ_REMOVE_PHOTO();
            request.Uid = Uid;
            request.PhotoName = msg.PhotoName;
            WriteToZone(request);
        }

        private void OnResponse_PhotoList(MemoryStream stream)
        {
            MSG_CG_PHOTO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PHOTO_LIST>(stream);
            Log.Write("player {0} request player {1} photo list", Uid, msg.OwnerUid);
            MSG_GateZ_PHOTO_LIST request = new MSG_GateZ_PHOTO_LIST();
            request.OwnerUid = msg.OwnerUid;
            request.RequestUid = Uid;
            WriteToZone(request);
        }

        private void OnResponse_PopRank(MemoryStream stream)
        {
            MSG_CG_POP_RANK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_POP_RANK>(stream);
            MSG_GateZ_POP_RANK request = new MSG_GateZ_POP_RANK();
            request.Uid = Uid;
            request.Page = msg.Page;
            WriteToZone(request);
        }
    }
}
