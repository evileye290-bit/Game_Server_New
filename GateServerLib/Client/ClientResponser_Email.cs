using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_OpenMailbox(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_EMAIL_OPENE_BOX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EMAIL_OPENE_BOX>(stream);
            MSG_GZ_EMAIL_OPENE_BOX request = new MSG_GZ_EMAIL_OPENE_BOX();
            request.PcUid = Uid;
            request.Language = msg.Language;
            WriteToZone(request);
        }

        public void OnResponse_ReadMail(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_EMAIL_READ msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_EMAIL_READ>(stream);
            MSG_GZ_EMAIL_READ request = new MSG_GZ_EMAIL_READ();
            request.PcUid = Uid;
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            request.Id = msg.Id;
            request.SendTime = msg.SendTime;
            request.Language = msg.Language;
            WriteToZone(request);
        }

        public void OnResponse_GetAttachment(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_PICKUP_ATTACHMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_PICKUP_ATTACHMENT>(stream);
            MSG_GZ_PICKUP_ATTACHMENT request = new MSG_GZ_PICKUP_ATTACHMENT();
            request.PcUid = Uid;
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            request.Id = msg.Id;
            request.SendTime = msg.SendTime;
            request.IsAll = msg.IsAll;
            WriteToZone(request);
        }
    }
}
