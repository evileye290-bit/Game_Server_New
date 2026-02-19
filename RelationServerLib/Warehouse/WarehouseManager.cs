using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Relation.Protocol.RZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class WarehouseManager
    {
        private RelationServerApi server { get; set; }
        public WarehouseManager(RelationServerApi server)
        {
            this.server = server;
        }

        public void AddItemInfoToWarehouse(int pcUid, int type, string rewards, string param)
        {
            //一条仓库信息的uid
            ulong warehouseUid = server.UID.NewWuid(server.MainId, server.MainId);

            int getTime = Timestamp.GetUnixTimeStampSeconds(RelationServerApi.now);

            switch ((ItemWarehouseType)type)
            {
                case ItemWarehouseType.SoulRing:
                    //增加仓库信息
                    server.GameDBPool.Call(new QueryInsertWarehouseSoulRing(pcUid, warehouseUid, getTime, rewards, param));
                    break;
                default:
                    break;
            }

            if (pcUid > 0)
            {
                MSG_RZ_ADD_WAREHOUSE_ITEMINFO msg = GetWarehouseItemInfoMsg(warehouseUid, pcUid, getTime, type, rewards, param);

                Client client = server.ZoneManager.GetClient(pcUid);
                if (client != null)
                {
                    client.CurZone.Write(msg);
                }
                else
                {                  
                    server.ZoneManager.Broadcast(msg);
                }
            }

            server.TrackingLoggerMng.RecordWarehouseItemLog(pcUid, warehouseUid, rewards, param, server.MainId, server.Now());
        }

        private MSG_RZ_ADD_WAREHOUSE_ITEMINFO GetWarehouseItemInfoMsg(ulong wUid, int pcUid, int getTime, int type, string rewards, string param)
        {
            MSG_RZ_ADD_WAREHOUSE_ITEMINFO msg = new MSG_RZ_ADD_WAREHOUSE_ITEMINFO();

            msg.Uid = wUid;
            msg.PcUid = pcUid;
            msg.GetTime = getTime;
            msg.Type = type;
            msg.Rewards = rewards;
            msg.Param = param;

            return msg;
        }
    }
}
