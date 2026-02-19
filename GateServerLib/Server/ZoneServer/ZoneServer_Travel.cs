using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_SyncTravelManagerMessage(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TRAVEL_MANAGER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync travel manager info not find client", pcUid);
            }
        }


        private void OnResponse_ActivateHeroTravel(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ACTIVATE_HERO_TRAVEL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync activate hero travel info not find client", pcUid);
            }
        }

        private void OnResponse_AddHeroTravelAffinity(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ADD_HERO_TRAVEL_AFFINITY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync add hero travel affinity info not find client", pcUid);
            }
        }

        private void OnResponse_StartHeroTravelEvevt(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_START_HERO_TRAVEL_EVENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync start hero travel evevt info not find client", pcUid);
            }
        }

        private void OnResponse_GetHeroTravelEvevt(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_HERO_TRAVEL_EVENT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync get hero travel evevt info not find client", pcUid);
            }
        }
        

        private void OnResponse_BuyHeroTravelHsopItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_HERO_TRAVEL_SHOP_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync but hero travel shop item not find client", pcUid);
            }
        }

        private void OnResponse_UpdateHeroTravelCardItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_HERO_TRAVEL_CARD_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync update hero travel card item not find client", pcUid);
            }
        }
        
    }
}
