using Logger;
using Message.Relation.Protocol.RZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        public void OnResponse_AddWarehouseItemInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_ADD_WAREHOUSE_ITEMINFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ADD_WAREHOUSE_ITEMINFO>(stream);
            Log.Write("add warehouse itemInfo {0}", msg.Uid);

            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.AddNewWarehouseItem(msg.Uid, msg.GetTime, msg.Type, msg.Rewards, msg.Param);

                //player.SyncNewWarehouseItem(msg.Type);

                player.SendWarehouseItemsMsg();
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    player.AddNewWarehouseItem(msg.Uid, msg.GetTime, msg.Type, msg.Rewards, msg.Param);
                }
            }
        }
        
        public void OnResponse_SpacetimeMonsterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SPACETIME_MONSTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SPACETIME_MONSTER_INFO>(stream);
            Log.Write("notify space time monster info period {0} passed {1}", msg.Period, msg.Passed);
            
            Api.SpaceTimeTowerManager.RecordMonsterInfo(msg.Period, msg.Passed, msg.NotifyPc);
        }
    }
}
