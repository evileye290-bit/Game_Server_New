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
        public void OnResponse_ActivateHeroTravel(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ACTIVATE_HERO_TRAVEL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ACTIVATE_HERO_TRAVEL>(stream);
            Log.Write("player {0} ActivateHeroTravel {1}", uid, pks.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ActivateHeroTravel not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} ActivateHeroTravel not in map ", uid);
                return;
            }

            player.ActivateHeroTravel(pks.HeroId);           
        }

        public void OnResponse_AddHeroTravelAffinity(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ADD_HERO_TRAVEL_AFFINITY pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ADD_HERO_TRAVEL_AFFINITY>(stream);
            Log.Write("player {0} AddHeroTravelAffinity {1} item id {2}", uid, pks.HeroId, pks.ItemId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0}AddHeroTravelAffinity count not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} AddHeroTravelAffinity count not in map ", uid);
                return;
            }

            player.AddHeroTravelAffinity(pks.HeroId, pks.ItemId, pks.ItemNum);
        }

        public void OnResponse_StartHeroTravelEvevt(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_START_HERO_TRAVEL_EVENT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_START_HERO_TRAVEL_EVENT>(stream);
            Log.Write("player {0} StartHeroTravelEvevt {1} slot id {2}", uid, pks.HeroId, pks.Slot);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} StartHeroTravelEvevt not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} StartHeroTravelEvevt not in map ", uid);
                return;
            }

            player.StartHeroTravelEvevt(pks.HeroId);
        }

        public void OnResponse_GetHeroTravelEvevt(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_HERO_TRAVEL_EVENT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_HERO_TRAVEL_EVENT>(stream);
            Log.Write("player {0} ActivateHeroTravel {1}", uid, pks.HeroId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ActivateHeroTravel not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0}  ActivateHeroTravel not in map ", uid);
                return;
            }

            player.GetHeroTravelEvevt(pks.HeroId);
        }

        public void OnResponse_ButTravelShopItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_HERO_TRAVEL_SHOP_ITEM pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_HERO_TRAVEL_SHOP_ITEM>(stream);
            Log.Write("player {0} ButTravelShopItem {1} slot id {2}", uid, pks.HeroId, pks.ItemIndex);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ButTravelShopItem not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} ButTravelShopItem not in map ", uid);
                return;
            }

            player.ButTravelShopItem(pks.HeroId, pks.ItemIndex);
        }
    }
}
