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
        public void OnResponse_GetWareHouseCurrencies(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_WAREHOUSE_CURRENCIES msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_WAREHOUSE_CURRENCIES>(stream);
            Log.Write("player {0} request get warehouse currencies", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get warehouse currencies not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetWareHouseCurrencies(msg.CurrencyType);
        }

        public void OnResponse_ShowWareHouseItems(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_WAREHOUSE_ITEMS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_WAREHOUSE_ITEMS>(stream);
            Log.Write("player {0} request show warehouse items", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} show warehouse items not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.ShowWareHouseItems(msg.Type, msg.Page);
        }

        public void OnResponse_BatchGetWareHouseItems(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BATCH_GET_WAREHOUSE_ITEMS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BATCH_GET_WAREHOUSE_ITEMS>(stream);
            Log.Write("player {0} request batch get warehouse items", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} batch get warehouse items not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.BatchGetWareHouseItems(msg.Type);
        }
    }
}
