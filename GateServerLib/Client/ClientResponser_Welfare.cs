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
        public void OnResponse_WelfareTriggerState(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_WELFARE_TRIGGER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_WELFARE_TRIGGER_STATE>(stream);
            MSG_GateZ_WELFARE_TRIGGER_STATE request = new MSG_GateZ_WELFARE_TRIGGER_STATE();
            request.Id = msg.Id;
            request.State = msg.State;
            WriteToZone(request);
        }
    }
}
