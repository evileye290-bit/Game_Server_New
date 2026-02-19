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
        public void OnResponse_ShovelGameRewards(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOVEL_GAME_REWARDS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOVEL_GAME_REWARDS>(stream);
            MSG_GateZ_SHOVEL_GAME_REWARDS response = new MSG_GateZ_SHOVEL_GAME_REWARDS();
            response.CheckPointId = msg.CheckPointId;
            response.CollideCount = msg.CollideCount;
            response.Blood = msg.Blood;
            response.Pass = msg.Pass;
            WriteToZone(response);
        }

        public void OnResponse_ShovelGameStart(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOVEL_GAME_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOVEL_GAME_START>(stream);
            MSG_GateZ_SHOVEL_GAME_START response = new MSG_GateZ_SHOVEL_GAME_START();
            WriteToZone(response);
        }

        public void OnResponse_LightTreasurePuzzle(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_LIGHT_TREASURE_PUZZLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_LIGHT_TREASURE_PUZZLE>(stream);
            MSG_GateZ_LIGHT_TREASURE_PUZZLE response = new MSG_GateZ_LIGHT_TREASURE_PUZZLE();
            response.Index = msg.Index;
            WriteToZone(response);
        }

        public void OnResponse_TreasureFlyStart(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TREASURE_FLY_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TREASURE_FLY_START>(stream);
            MSG_GateZ_TREASURE_FLY_START response = new MSG_GateZ_TREASURE_FLY_START();           
            WriteToZone(response);
        }

        public void OnResponse_TreasureFlyDone(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TREASURE_FLY_DONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TREASURE_FLY_DONE>(stream);
            MSG_GateZ_TREASURE_FLY_DONE response = new MSG_GateZ_TREASURE_FLY_DONE();
            WriteToZone(response);
        }

        public void OnResponse_LookPuzzleInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_LOOK_PUZZLE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_LOOK_PUZZLE_INFO>(stream);
            MSG_GateZ_LOOK_PUZZLE_INFO response = new MSG_GateZ_LOOK_PUZZLE_INFO();
            WriteToZone(response);
        }

        public void OnResponse_ShovelGameRevive(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOVEL_GAME_REVIVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOVEL_GAME_REVIVE>(stream);
            MSG_GateZ_SHOVEL_GAME_REVIVE response = new MSG_GateZ_SHOVEL_GAME_REVIVE();
            WriteToZone(response);
        }
    }
}
