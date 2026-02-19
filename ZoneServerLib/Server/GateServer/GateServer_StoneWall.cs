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
        public void OnResponse_GetStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_STONE_WALL_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_STONE_WALL_VALUE>(stream);
            Log.Write("player {0} GetStoneWallInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetStoneWallInfo not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetStoneWallInfo not in map ", uid);
                return;
            }

            player.GetStoneWallInfo();
        }

        public void OnResponse_GetStoneWallReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_STONE_WALL_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_STONE_WALL_REWARD>(stream);
            Log.Write("player {0} GetStoneWallReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetStoneWallReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetStoneWallReward not in map ", uid);
                return;
            }

            player.GetStoneWallReward(pks.Id, pks.Line, pks.Column, pks.UseDiamond);
        }

        public void OnResponse_BuyStoneWallItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_STONE_WALL_ITEM pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_STONE_WALL_ITEM>(stream);
            Log.Write("player {0} BuyStoneWallItem", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} BuyStoneWallItem not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} BuyStoneWallItem not in map ", uid);
                return;
            }

            player.BuyStoneWallItem(pks.Id, pks.Num);
        }

        public void OnResponse_RestStoneWall(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RESET_STONE_WALL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RESET_STONE_WALL>(stream);
            Log.Write("player {0} ResetStoneWall", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ResetStoneWall not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} ResetStoneWall not in map ", uid);
                return;
            }

            player.ResetStoneWall(pks.Id);
        }
    }
}
