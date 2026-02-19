using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_ShovelGameRewards(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOVEL_GAME_REWARDS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOVEL_GAME_REWARDS>(stream);
            Log.Write("player {0} request shovel game rewards: id {1} count {2} pass {3}", uid, msg.CheckPointId, msg.CollideCount, msg.Pass.ToString());

            PlayerChar player = Api.PCManager.FindPc(uid);
            Log.WriteLine("player {0}  get shovel game rewards", uid);
            if (player != null)
            {              
                player.GetShovelGameRewards(msg.CheckPointId, msg.CollideCount, msg.Blood, msg.Pass);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("get shovel game rewards fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("get shovel game rewards fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_ShovelGameStart(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOVEL_GAME_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOVEL_GAME_START>(stream);
            Log.Write("player {0} record shovel game start time", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.RecordShovelGameStartTime();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("record shovel game start time fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("record shovel game start time fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_LightTreasurePuzzle(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_LIGHT_TREASURE_PUZZLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_LIGHT_TREASURE_PUZZLE>(stream);
            Log.Write("player {0} request light treasure puzzle index {1}", uid, msg.Index);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.LightTreasurePuzzle(msg.Index);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("light treasure puzzle fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("light treasure puzzle fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_TreasureFlyStart(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TREASURE_FLY_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TREASURE_FLY_START>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.TreaureFlyPathFinding();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("treasure fly fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("treasure fly fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_TreasureFlyDone(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TREASURE_FLY_DONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TREASURE_FLY_DONE>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SetTreasureFlyPositionOrChangeMap();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("treasure fly fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("treasure fly fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_LookPuzzleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_LOOK_PUZZLE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_LOOK_PUZZLE_INFO>(stream);
            Log.Write("player {0} look puzzle info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.LookCurPuzzleInfo();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("look puzzle info fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("look puzzle info fail, can not find player {0} .", uid);
                }
            }
        }

        public void OnResponse_ShovelGameRevive(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOVEL_GAME_REVIVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOVEL_GAME_REVIVE>(stream);
            Log.Write("player {0} shovel game revive", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ShovelGameRevive();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("shovel game revive fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("shovel game revive fail, can not find player {0} .", uid);
                }
            }
        }
    }
}
