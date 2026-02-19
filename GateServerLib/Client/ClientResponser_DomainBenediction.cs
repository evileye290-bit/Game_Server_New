using System.IO;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_HandleGetStageAward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DOMAIN_BENEDICTION_GET_STAGE_AWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DOMAIN_BENEDICTION_GET_STAGE_AWARD>(stream);

            MSG_GateZ_DOMAIN_BENEDICTION_GET_STAGE_AWARD requset = new MSG_GateZ_DOMAIN_BENEDICTION_GET_STAGE_AWARD();
            requset.Id = msg.Id;
            WriteToZone(requset);
        }
        
        public void OnResponse_HandleGetBaseCurrencyAward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD>(stream);
            MSG_GateZ_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD requset = new MSG_GateZ_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD();
            WriteToZone(requset);
        }
        
        public void OnResponse_HandlePrayOperation(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DOMAIN_BENEDICTION_PRAY_OPERATION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DOMAIN_BENEDICTION_PRAY_OPERATION>(stream);

            MSG_GateZ_DOMAIN_BENEDICTION_PRAY_OPERATION requset = new MSG_GateZ_DOMAIN_BENEDICTION_PRAY_OPERATION();
            WriteToZone(requset);
        }
        
        public void OnResponse_HandleDrawOperation(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DOMAIN_BENEDICTION_DRAW_OPERATION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DOMAIN_BENEDICTION_DRAW_OPERATION>(stream);

            MSG_GateZ_DOMAIN_BENEDICTION_DRAW_OPERATION requset = new MSG_GateZ_DOMAIN_BENEDICTION_DRAW_OPERATION();
            requset.Id = msg.Id;
            WriteToZone(requset);
        }
    }
}