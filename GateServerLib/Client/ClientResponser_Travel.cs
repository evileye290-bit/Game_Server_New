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
        public void OnResponse_ActivateHeroTravel(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ACTIVATE_HERO_TRAVEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ACTIVATE_HERO_TRAVEL>(stream);
            MSG_GateZ_ACTIVATE_HERO_TRAVEL request = new MSG_GateZ_ACTIVATE_HERO_TRAVEL();
            request.HeroId = msg.HeroId;

            WriteToZone(request);
        }

        public void OnResponse_AddHeroTravelAffinity(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ADD_HERO_TRAVEL_AFFINITY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ADD_HERO_TRAVEL_AFFINITY>(stream);
            MSG_GateZ_ADD_HERO_TRAVEL_AFFINITY request = new MSG_GateZ_ADD_HERO_TRAVEL_AFFINITY();
            request.HeroId = msg.HeroId;
            request.ItemId = msg.ItemId;
            request.ItemNum = msg.ItemNum;
            WriteToZone(request);
        }

        public void OnResponse_StartHeroTravelEvevt(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_START_HERO_TRAVEL_EVENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_START_HERO_TRAVEL_EVENT>(stream);
            MSG_GateZ_START_HERO_TRAVEL_EVENT request = new MSG_GateZ_START_HERO_TRAVEL_EVENT();
            request.HeroId = msg.HeroId;
            request.Slot = msg.Slot;
            WriteToZone(request);
        }

        public void OnResponse_GetHeroTravelEvevt(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_HERO_TRAVEL_EVENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_HERO_TRAVEL_EVENT>(stream);
            MSG_GateZ_GET_HERO_TRAVEL_EVENT request = new MSG_GateZ_GET_HERO_TRAVEL_EVENT();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_ButTravelShopItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_HERO_TRAVEL_SHOP_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_HERO_TRAVEL_SHOP_ITEM>(stream);
            MSG_GateZ_BUY_HERO_TRAVEL_SHOP_ITEM request = new MSG_GateZ_BUY_HERO_TRAVEL_SHOP_ITEM();
            request.HeroId = msg.HeroId;
            request.ItemIndex = msg.ItemIndex;
            WriteToZone(request);
        }
    }
}
