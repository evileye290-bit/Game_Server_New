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
        private void OnResponse_SyncWarehouseCurrencies(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SYNC_WAREHOUSE_CURRENCIES>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} SyncWarehouseCurrencies not find client", pcUid);
            }
        }

        private void OnResponse_GetWarehouseCurrencies(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_WAREHOUSE_CURRENCIES>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetWarehouseCurrencies not find client", pcUid);
            }
        }

        private void OnResponse_NewWarehouseSoulRing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEW_WAREHOUSE_SOULRING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} NewWarehouseSoulRing not find client", pcUid);
            }
        }

        private void OnResponse_SyncWarehouseItems(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SYNC_WAREHOUSE_ITEMS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} SyncWarehouseItems not find client", pcUid);
            }
        }

        private void OnResponse_ShowWarehouseItems(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_WAREHOUSE_ITEMS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ShowWarehouseItems not find client", pcUid);
            }
        }

        private void OnResponse_BathchGetWarehouseItems(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BATCH_GET_WAREHOUSE_ITEMS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BathchGetWarehouseItems not find client", pcUid);
            }
        }
    }
}
