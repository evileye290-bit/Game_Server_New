using Message.Zone.Protocol.ZR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Relation.Protocol.RZ;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_AddWarehouseItemInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ADD_WAREHOUSE_ITEMINFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ADD_WAREHOUSE_ITEMINFO>(stream);

            if (pks.Uid > 0)
            {
                Api.WarehouseMng.AddItemInfoToWarehouse(pks.Uid, pks.Type, pks.Reward, pks.Param);
            }
        }
        
        public void NotifySpaceTimeMonsterInfo(int period, bool passed, bool notifyPc)
        {
            MSG_RZ_SPACETIME_MONSTER_INFO msg = new MSG_RZ_SPACETIME_MONSTER_INFO();
            msg.Period = period;
            msg.Passed = passed;
            msg.NotifyPc = notifyPc;
            Write(msg);
        }
        
        public void OnResponse_SpaceTimeMonsterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SPACETIME_MONSTER_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SPACETIME_MONSTER_INFO>(stream);

            Api.SpaceTimeTowerManager.NotifyUpdateMonsterInfo(pks.Period, pks.Passed);
        }
    }
}
