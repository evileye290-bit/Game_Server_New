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
        private void OnResponse_GetWishPoolInfo(MemoryStream stream)
        {
            MSG_CG_GET_WISHPOOL_UPDATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_WISHPOOL_UPDATE>(stream);
            MSG_GateZ_GET_WISHPOOL_UPDATE request = new MSG_GateZ_GET_WISHPOOL_UPDATE();
            WriteToZone(request);
        }

        private void OnResponse_UseWishPool(MemoryStream stream)
        {
            MSG_CG_USINIG_WISHPOOL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_USINIG_WISHPOOL>(stream);
            MSG_GateZ_USINIG_WISHPOOL request = new MSG_GateZ_USINIG_WISHPOOL();
            WriteToZone(request);
        }
    }
}
