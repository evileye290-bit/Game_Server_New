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
        public void OnResponse_GetStoneWallInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_STONE_WALL_VALUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_STONE_WALL_VALUE>(stream);
            MSG_GateZ_GET_STONE_WALL_VALUE request = new MSG_GateZ_GET_STONE_WALL_VALUE();

            WriteToZone(request);
        }

        public void OnResponse_GetStoneWallReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_STONE_WALL_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_STONE_WALL_REWARD>(stream);
            MSG_GateZ_GET_STONE_WALL_REWARD request = new MSG_GateZ_GET_STONE_WALL_REWARD();
            request.Id = msg.Id;
            request.Line = msg.Line;
            request.Column = msg.Column;
            request.UseDiamond = msg.UseDiamond;
            WriteToZone(request);
        }

        public void OnResponse_BuyStoneWallItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_STONE_WALL_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_STONE_WALL_ITEM>(stream);
            MSG_GateZ_BUY_STONE_WALL_ITEM request = new MSG_GateZ_BUY_STONE_WALL_ITEM();
            request.Id = msg.Id;
            request.Num = msg.Num;
            WriteToZone(request);
        }

        public void OnResponse_RestStoneWall(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RESET_STONE_WALL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RESET_STONE_WALL>(stream);
            MSG_GateZ_RESET_STONE_WALL request = new MSG_GateZ_RESET_STONE_WALL();
            request.Id = msg.Id;         
            WriteToZone(request);
        }
    }
}
