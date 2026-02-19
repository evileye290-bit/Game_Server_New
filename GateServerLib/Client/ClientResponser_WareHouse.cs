using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
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
        public void OnResponse_GetWareHouseCurrencies(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_WAREHOUSE_CURRENCIES msg = ProtobufHelper.Deserialize<MSG_CG_GET_WAREHOUSE_CURRENCIES>(stream);
            MSG_GateZ_GET_WAREHOUSE_CURRENCIES request = new MSG_GateZ_GET_WAREHOUSE_CURRENCIES();
            request.CurrencyType = msg.CurrencyType;
            WriteToZone(request);
        }

        public void OnResponse_ShowWareHouseItems(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOW_WAREHOUSE_ITEMS msg = ProtobufHelper.Deserialize<MSG_CG_SHOW_WAREHOUSE_ITEMS>(stream);
            MSG_GateZ_SHOW_WAREHOUSE_ITEMS request = new MSG_GateZ_SHOW_WAREHOUSE_ITEMS();
            request.Type = msg.Type;
            request.Page = msg.Page;
            WriteToZone(request);
        }

        public void OnResponse_BatchGetWareHouseItems(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BATCH_GET_WAREHOUSE_ITEMS msg = ProtobufHelper.Deserialize<MSG_CG_BATCH_GET_WAREHOUSE_ITEMS>(stream);
            MSG_GateZ_BATCH_GET_WAREHOUSE_ITEMS request = new MSG_GateZ_BATCH_GET_WAREHOUSE_ITEMS();
            request.Type = msg.Type;
            WriteToZone(request);
        }
    }
}
