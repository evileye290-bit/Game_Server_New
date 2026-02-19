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
        public void OnResponse_GiftCodeExchangeReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GIFT_CODE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GIFT_CODE_REWARD>(stream);
            MSG_GateZ_GIFT_CODE_REWARD request = new MSG_GateZ_GIFT_CODE_REWARD();
            request.GiftCode = msg.GiftCode;
            WriteToZone(request);
        }

        public void OnResponse_CheckCodeUnique(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHECK_CODE_UNIQUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHECK_CODE_UNIQUE>(stream);
            MSG_GateZ_CHECK_CODE_UNIQUE request = new MSG_GateZ_CHECK_CODE_UNIQUE();
            request.GiftCode = msg.GiftCode;
            WriteToZone(request);
        }
    }
}
