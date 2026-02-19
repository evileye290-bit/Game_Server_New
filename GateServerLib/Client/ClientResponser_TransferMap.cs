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
        public void OnResponse_TransferEnterMap(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TRANSFER_ENTER_MAP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TRANSFER_ENTER_MAP>(stream);
            MSG_GateZ_TRANSFER_ENTER_MAP request = new MSG_GateZ_TRANSFER_ENTER_MAP();
            request.MapId = msg.MapId;
            WriteToZone(request);
        }
    }
}
