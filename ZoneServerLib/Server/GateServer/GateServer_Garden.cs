using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Logger;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetGardenInfo(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} GetGardenInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetGardenInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetGardenInfo not in map ", uid);
                return;
            }

            player.GetGardenInfo();
        }

        public void OnResponse_PlantedSeed(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GARDEN_PLANTED_SEED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GARDEN_PLANTED_SEED>(stream);

            Log.Write("player {0} PlantedSeed", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} PlantedSeed not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.PlantedSeed(msg.Pit, msg.SeedId);
        }

        public void OnResponse_GetGardenReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GARDEN_REAWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GARDEN_REAWARD>(stream);

            Log.Write("player {0} GetGardenReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetGardenReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetGardenReward(msg.Type, msg.Pit, msg.UseDiamond);
        }

        private void OnResponse_BuySeed(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GARDEN_BUY_SEED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GARDEN_BUY_SEED>(stream);

            Log.Write("player {0} BuySeed", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} BuySeed not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.BuySeed(msg.SeedId, msg.Num);
        }

        private void OnResponse_GardenShopExchange(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GARDEN_SHOP_EXCHANGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GARDEN_SHOP_EXCHANGE>(stream);

            Log.Write("player {0} GardenShopExchange", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GardenShopExchange not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GardenShopExchange(msg.Id);
        }
    }
}
