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
        private void OnResponse_ShopBuyItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOP_BUY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shop buy item result not find client", pcUid);
            }
        }

        private void OnResponse_ShopInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOP_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync all shop list not find client", pcUid);
            }
        }

        private void OnResponse_ShopRefresh(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOP_REFRESH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shop refresh not find client", pcUid);
            }
        }

        private void OnResponse_ShopSoulBoneBonus(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOP_SOULBONE_BONUS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shop ShopSoulBoneBonus not find client", pcUid);
            }
        }

        private void OnResponse_ShopSoulBoneReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOP_SOULBONE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shop ShopSoulBoneReward not find client", pcUid);
            }
        }

        private void OnResponse_ShopDailyRefresh(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOP_DAILY_REFRESH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shop daily refresh not find client", pcUid);
            }
        }

        private void OnResponse_BuyShopItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_SHOP_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy shop item not find client", pcUid);
            }
        }
    }
}
