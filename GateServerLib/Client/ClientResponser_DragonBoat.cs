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
        public void OnResponse_DragonBoatGameStart(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DRAGON_BOAT_GAME_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DRAGON_BOAT_GAME_START>(stream);
            MSG_GateZ_DRAGON_BOAT_GAME_START request = new MSG_GateZ_DRAGON_BOAT_GAME_START();
            WriteToZone(request);
        }

        public void OnResponse_DragonBoatGameEnd(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DRAGON_BOAT_GAME_END msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DRAGON_BOAT_GAME_END>(stream);
            MSG_GateZ_DRAGON_BOAT_GAME_END request = new MSG_GateZ_DRAGON_BOAT_GAME_END();
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_DragonBoatBuyTicket(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DRAGON_BOAT_BUY_TICKET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DRAGON_BOAT_BUY_TICKET>(stream);
            MSG_GateZ_DRAGON_BOAT_BUY_TICKET request = new MSG_GateZ_DRAGON_BOAT_BUY_TICKET();
            request.Id = msg.Id;
            request.Count = msg.Count;
            WriteToZone(request);
        }

        public void OnResponse_DragonBoatGetFreeTicket(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DRAGON_BOAT_FREE_TICKET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DRAGON_BOAT_FREE_TICKET>(stream);
            MSG_GateZ_DRAGON_BOAT_FREE_TICKET request = new MSG_GateZ_DRAGON_BOAT_FREE_TICKET();        
            WriteToZone(request);
        }
    }
}
