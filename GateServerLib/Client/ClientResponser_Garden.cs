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
        public void OnResponse_GetGardenInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            //MSG_CG_GARDEN_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GARDEN_INFO>(stream);
            MSG_GateZ_GARDEN_INFO request = new MSG_GateZ_GARDEN_INFO();

            WriteToZone(request);
        }

        public void OnResponse_PlantedSeed(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GARDEN_PLANTED_SEED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GARDEN_PLANTED_SEED>(stream);
            MSG_GateZ_GARDEN_PLANTED_SEED request = new MSG_GateZ_GARDEN_PLANTED_SEED() { Pit = msg.Pit, SeedId = msg.SeedId };

            WriteToZone(request);
        }

        public void OnResponse_GetGardenReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GARDEN_REAWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GARDEN_REAWARD>(stream);
            MSG_GateZ_GARDEN_REAWARD request = new MSG_GateZ_GARDEN_REAWARD() { Type = msg.Type, Pit = msg.Pit, UseDiamond = msg.UseDiamond };

            WriteToZone(request);
        }

        public void OnResponse_BuySeed(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GARDEN_BUY_SEED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GARDEN_BUY_SEED>(stream);
            MSG_GateZ_GARDEN_BUY_SEED request = new MSG_GateZ_GARDEN_BUY_SEED() { SeedId = msg.SeedId , Num = msg.Num};

            WriteToZone(request);
        }

        private void OnResponse_GardenShopExchange(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GARDEN_SHOP_EXCHANGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GARDEN_SHOP_EXCHANGE>(stream);
            MSG_GateZ_GARDEN_SHOP_EXCHANGE request = new MSG_GateZ_GARDEN_SHOP_EXCHANGE() { Id = msg.Id };

            WriteToZone(request);
        }
    }
}
