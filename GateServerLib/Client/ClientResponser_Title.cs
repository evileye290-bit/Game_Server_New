using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_ChangeTitle(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHANGE_TITLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHANGE_TITLE>(stream);
            MSG_GateZ_CHANGE_TITLE request = new MSG_GateZ_CHANGE_TITLE();
            request.PCUid = Uid;
            request.Title = msg.Title;

            WriteToZone(request);
        }

        public void OnResponse_GetTitleConditionCount(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TITLE_CONDITION_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TITLE_CONDITION_COUNT>(stream);
            MSG_GateZ_TITLE_CONDITION_COUNT request = new MSG_GateZ_TITLE_CONDITION_COUNT();
            request.TitleId = msg.TitleId;
            WriteToZone(request);
        }

        public void OnResponse_LookTitle(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_LOOK_TITLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_LOOK_TITLE>(stream);
            MSG_GateZ_LOOK_TITLE request = new MSG_GateZ_LOOK_TITLE();
            request.TitleId = msg.TitleId;
            WriteToZone(request);
        }
    }
}
