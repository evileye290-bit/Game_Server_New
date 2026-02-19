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
        public void OnResponse_DragonBoatGameStart(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DRAGON_BOAT_GAME_START pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DRAGON_BOAT_GAME_START>(stream);
            Log.Write("player {0} DragonBoatGameStart", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} DragonBoatGameStart not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} DragonBoatGameStart not in map ", uid);
                return;
            }

            player.DragonBoatGameStart();
        }

        public void OnResponse_DragonBoatGameEnd(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DRAGON_BOAT_GAME_END pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DRAGON_BOAT_GAME_END>(stream);
            Log.Write("player {0} DragonBoatGameEnd", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} DragonBoatGameEnd not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} DragonBoatGameEnd not in map ", uid);
                return;
            }

            player.DragonBoatGameEnd(pks.Index);
        }

        public void OnResponse_DragonBoatBuyTicket(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DRAGON_BOAT_BUY_TICKET pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DRAGON_BOAT_BUY_TICKET>(stream);
            Log.Write("player {0} DragonBoatBuyTicket", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} DragonBoatBuyTicket not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} DragonBoatBuyTicket not in map ", uid);
                return;
            }

            player.DragonBoatBuyTicket(pks.Id, pks.Count);
        }

        public void OnResponse_DragonBoatGetFreeTicket(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DRAGON_BOAT_FREE_TICKET pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DRAGON_BOAT_FREE_TICKET>(stream);
            Log.Write("player {0} DragonBoatGetFreeTicket", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} DragonBoatGetFreeTicket not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} DragonBoatGetFreeTicket not in map ", uid);
                return;
            }

            player.DragonBoatGetFreeTicket();
        }
    }
}
